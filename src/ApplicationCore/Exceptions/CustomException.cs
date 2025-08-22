namespace System
{
    public static class CustomException
    {
        public static Exception SqlQueryException(this Exception ex, string sql)
        {
            ex.Data.Add("SqlQuery", sql);
            return ex;
        }
    }

}
