using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace WebAPI.Models
{
    [Index(nameof(URL), nameof(DateFetched))]
    public class URLPreview
    {
        [Key, ForeignKey(nameof(File))]
        public long Id { get; set; }

        public long DateFetched { get; set; }

        [MaxLength(255)]
        public string URL { get; set; }

        [MaxLength(255)]
        public string Author { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }
    }
}
