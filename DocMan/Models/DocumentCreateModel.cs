using Microsoft.AspNetCore.Http;
using QorusShareLib.Models;
using System.ComponentModel.DataAnnotations;

namespace DocMan.Models
{
    public class DocumentCreateModel : DocumentModel
    {
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }
    }
}