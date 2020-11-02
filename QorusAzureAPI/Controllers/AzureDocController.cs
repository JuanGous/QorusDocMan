using Microsoft.AspNetCore.Mvc;
using QorusAzureAPI.Services;
using QorusShareLib.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QorusAzureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureDocController : ControllerBase
    {
        private IBlobService _blobService;
        private string _containerName = "juan";

        public AzureDocController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost("Upload"), DisableRequestSizeLimit]
        public async Task<ActionResult> Upload(DocumentModel document)
        {
            var result = await _blobService.UploadFileBlobAsync(_containerName, document);

            var toReturn = result.AbsoluteUri;

            return Ok(new { path = toReturn });
        }

        [HttpGet("Download/{blobName}")]
        public async Task<DocumentModel> Download(string blobName)
        {
            return await _blobService.DownloadBlobAsync(_containerName, blobName);
        }

        [HttpGet("List")]
        public async Task<IEnumerable<DocumentModel>> getList()
        {
            return await _blobService.GetBlobFileListAsync(_containerName);
        }
    }
}