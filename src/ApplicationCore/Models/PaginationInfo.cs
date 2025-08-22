namespace ApplicationCore.Models
{
    public class PaginationInfo
    {
        public string GridType { get; set; }
        public string PageBy { get; set; }
        public string FilterBy { get; set; }
        public string OrderBy { get; set; }

        public PaginationInfo()
        {
            PageBy = "";
            FilterBy = "";
            OrderBy = "";
        }
    }
}
