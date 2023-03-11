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

        public long UploaderId { get; set; }
        public long DateCreated { get; set; }
        public long? DateUpdated { get; set; }

        public long? DateDeleted { get; set; }

        public ReasonDeletedEnum ReasonDeleted { get; set; }


        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public long Size { get; set; }
        public FilePurposeEnum Type { get; set; }
        public int Pages { get; set; }
        public int Width { get; set; } 

        public int Height { get; set; }
        public byte[] Preview { get; set; }
    }
}
