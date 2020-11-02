using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using QorusShareLib.Common;
using QorusShareLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace QorusAzureAPI.Services
{
    public class AzureBlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<Uri> UploadFileBlobAsync(string blobContainerName, DocumentModel document)
        {
            var containerClient = GetContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlobClient(document.Id.ToString());

            Dictionary<string, string> metaData = new Dictionary<string, string>();
            metaData.Add("Id", document.Id.ToString());
            metaData.Add("FileName", document.FileName);
            metaData.Add("Size", document.Size.ToString());
            metaData.Add("Category", document.Category);
            metaData.Add("LastReviewed", document.LastReviewed.ToString());

            if (document.Content != null)
            {
                var t = await blobClient.UploadAsync(new MemoryStream(document.Content), new BlobHttpHeaders { ContentType = document.ContentType });
            }
            // Set the blob's metadata.
            await blobClient.SetMetadataAsync(metaData);

            return blobClient.Uri;
        }

        public async Task<DocumentModel> DownloadBlobAsync(string blobContainerName, string fileName)
        {
            var containerClient = GetContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            BlobDownloadInfo download = await blobClient.DownloadAsync();
            DocumentModel doc = new DocumentModel();
            doc.Id = Guid.Parse(fileName);
            if (download.Details.Metadata != null)
            {
                doc.FileName = download.Details.Metadata["FileName"];
                doc.Size = int.Parse(download.Details.Metadata["Size"]);
                doc.Category = download.Details.Metadata["Category"];
                doc.LastReviewed = DateTime.Parse(download.Details.Metadata["LastReviewed"]);
            }

            doc.Content = IOExtension.ReadFully(download.Content);

            doc.ContentType = download.ContentType;

            return doc;
        }

        public async Task<List<DocumentModel>> GetBlobFileListAsync(string blobContainerName)
        {
            try
            {
                var containerClient = GetContainerClient(blobContainerName);
                List<DocumentModel> documentList = new List<DocumentModel>();

                await foreach (BlobItem item in containerClient.GetBlobsAsync(BlobTraits.All))
                {
                    var document = new DocumentModel();

                    document.Id = Guid.Parse(item.Name);
                    if (item.Metadata != null)
                    {
                        document.FileName = item.Metadata["FileName"];
                        document.Size = int.Parse(item.Metadata["Size"]);
                        document.Category = item.Metadata["Category"];
                        document.LastReviewed = DateTime.Parse(item.Metadata["LastReviewed"]);
                    }
                    documentList.Add(document);
                }

                return documentList;
            }
            catch
            {
                throw;
            }
        }

        private BlobContainerClient GetContainerClient(string blobContainerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            containerClient.CreateIfNotExists(PublicAccessType.BlobContainer);
            return containerClient;
        }
    }
}