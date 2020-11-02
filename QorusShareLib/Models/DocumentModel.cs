using System;
using System.ComponentModel.DataAnnotations;

namespace QorusShareLib.Models
{
    public class DocumentModel
    {
        public Guid Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Display(Name = "Size (bytes)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Size { get; set; }

        public string Category { get; set; }

        [Display(Name = "Last Reviewed")]
        public DateTime LastReviewed { get; set; }

        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}