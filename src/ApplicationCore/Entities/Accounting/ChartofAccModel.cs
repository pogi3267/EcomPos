using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Accounting
{
    public class ChartofAccModel
    {
        public int RootAccount { get; set; }
        public string RootAccountName { get; set; }
        public int SupplierId { get; set; }
        public int SupplierType { get; set; }
        public int ParentAccount { get; set; }
        public string ParentAccountName { get; set; }
        public string AccountCode { get; set; }
        public string AccountHead { get; set; }
        public int Posted { get; set; }
        public int ChildAccount { get; set; }
        public int? Timesaday { get; set; }
        public string ChildAccountName { get; set; }
        public string Code { get; set; }
        public List<User> RootName { get; set; }

    }
    public class User
    {
        public int ParentAccount { get; set; }

        public string ParentAccountName { get; set; }

        public string ChildAccountName { get; set; }
    }
}
