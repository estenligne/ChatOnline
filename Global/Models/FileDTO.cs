using System;
using Global.Enums;

namespace Global.Models
{
    public class FileDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public FilePurposeEnum Purpose { get; set; }

        public DateTimeOffset DateUploaded { get; set; }

        public DateTimeOffset? DateDeleted { get; set; }
    }
}
