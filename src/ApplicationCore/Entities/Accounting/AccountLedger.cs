using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Accounting
{
    [Table("AccountLedger")]
    public class AccountLedger : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string ParentName { get; set; }
        public string Code { get; set; }
        public bool Posted { get; set; }
        public int LevelNo { get; set; }
        public int RelatedId { get; set; }
        public int RelatedIdFor { get; set; }
        public int IsEditable { get; set; }
        public int IsApproved { get; set; }
        public int Common { get; set; }
        public int Status { get; set; }
        public int DeleteBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string IsActive { get; set; }
        public string IsDelete { get; set; }
        public int ShowInIncomeStetement { get; set; }

        [Write(false)]
        public int RootAccount { get; set; }
        [Write(false)]
        public List<Users> RootName { get; set; }
        [Write(false)]
        public string ParentAccountName { get; set; }
        [Write(false)]
        public string RootAccountName { get; set; }
        [Write(false)]
        public int ParentAccount { get; set; }
        [Write(false)]
        public List<Select2OptionModel> ParentList { get; set; }
        [Write(false)]
        public string RootParentName { get; set; }
        [Write(false)]
        public List<int> ParantIdList { get; set; }

    }


    public class Users
    {
        public int ParentAccount { get; set; }

        public string ParentAccountName { get; set; }

        public string ChildAccountName { get; set; }
    }

}
