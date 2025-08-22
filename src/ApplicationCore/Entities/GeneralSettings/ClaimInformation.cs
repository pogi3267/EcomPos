using Dapper.Contrib.Extensions;
using System.Collections.Generic;

namespace ApplicationCore.Entities.GeneralSettings
{
    [Table("ClaimInformation")]
    public class ClaimInformation : BaseEntity
    {
        public ClaimInformation()
        {
            PageInfoList = new List<ClaimInformation>();
        }
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int ParentId { get; set; }
        [Write(false)]
        List<ClaimInformation> PageInfoList { get; set; }
    }
}
