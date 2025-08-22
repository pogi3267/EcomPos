using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.Products
{
    [Table("Uploads")]
    public class Upload : BaseEntity
    {
        public Upload()
        {
        }

        [Key]
        public int UploadId { get; set; }
        public string FileOriginalName { get; set; }
        public string FileName { get; set; }
        public int? UserId { get; set; }
        public long FileSize { get; set; }
        public string Extension { get; set; }
        public string Type { get; set; }
        public string ExternalLink { get; set; }
        public DateTime? Deleted_At { get; set; }
    }
}