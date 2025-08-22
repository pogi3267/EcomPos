namespace ApplicationWeb.HelperAndConstant
{
    public static class DataTableHandler
    {
        public static Tuple<int, string, int, int, string, string> PaginationHandler(HttpRequest Request)
        {
            var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault());
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            return new Tuple<int, string, int, int, string, string>(draw, searchValue, pageSize, skip, sortColumn, sortColumnDir);
        }
    }
}
