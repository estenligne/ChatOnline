using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.Files))]
    [Index(nameof(Name), IsUnique = true)]
    public class File
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public int Size { get; set; }

        public FilePurposeEnum Purpose { get; set; }

        public long UploaderId { get; set; }
        //public virtual UserProfile Uploader { get; set; }

        public DateTime DateUploaded { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
