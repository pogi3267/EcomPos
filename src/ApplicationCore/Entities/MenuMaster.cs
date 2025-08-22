using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities
{
    [Table("MenuMaster")]
    public class MenuMaster : BaseEntity
    {
        [Key]
        public int MenuMasterId { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int SerialNo { get; set; }
        public int IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PageName { get; set; }
        public string Icon { get; set; }
        [Write(false)]
        public int? MenuParamId { get; set; }
        [Write(false)]
        public string ParamValue { get; set; }
        [Write(false)]
        public int HasPermission { get; set; }
        public bool IsPermissionGranted
        {
            get
            {
                return HasPermission == 1;
            }
            set
            {
                HasPermission = value ? 1 : 0;
            }
        }

        [Write(false)]
        public string UserId { get; set; }
        [Write(false)]
        public List<MenuMaster> Menues { get; set; }
        [Write(false)]
        public List<MenuMaster> ParentMenues { get; set; }
        public MenuMaster()
        {
            Menues = new List<MenuMaster>();
            ParentMenues = new List<MenuMaster>();
        }
    }
}