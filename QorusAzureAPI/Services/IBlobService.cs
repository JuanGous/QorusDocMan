using QorusShareLib.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QorusAzureAPI.Services
{
    public interface IBlobService
    {
        Task<Uri> UploadFileBlobAsync(string blobContainerName, DocumentModel document);

        Task<DocumentModel> DownloadBlobAsync(string blobContainerName, string fileName);

        Task<List<DocumentModel>> GetBlobFileListAsync(string blobContainerName);
    }
}