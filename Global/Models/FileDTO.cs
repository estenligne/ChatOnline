using System;
using Global.Enums;

namespace Global.Models
{
    public class FileDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int Size { get; set; }

        public FilePurposeEnum Purpose { get; set; }

        public DateTime DateUploaded { get; set; }

        public DateTime? DateDeleted { get; set; }
    }
}
