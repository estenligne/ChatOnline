using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.NewModels
{
    [Index(nameof(Name), IsUnique = true)]
    public class FileModel
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public long Size { get; set; }

        public FilePurposeEnum Purpose { get; set; }

        public long UploaderId { get; set; }
        //public virtual UserProfile Uploader { get; set; }

        public DateTimeOffset DateUploaded { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }
    }
}
