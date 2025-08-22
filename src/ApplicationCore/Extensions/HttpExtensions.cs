using ApplicationCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <summary>
    /// Response handler extension to convert response into user specified format.
    /// </summary>
    public static class HttpExtensions
    {
        /// <summary>
        /// Convert HttpResponseMessage into Json.
        /// </summary>
        /// <typeparam name="T">Convert type class.</typeparam>
        /// <param name="response">HttpResponse message.</param>
        /// <returns></returns>
        public static async Task<T> GetJsonDataAsync<T>(this HttpResponseMessage response) where T : class
        {
            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}

namespace Microsoft.AspNetCore.Http
{
    public static class HttpExtensions
    {
        public static PaginationInfo GetPaginationInfo(this HttpRequest request)
        {
            var paginationInfo = new PaginationInfo();
            IQueryCollection query = request.Query;

            var gridType = query.GetStringValue("gridType");
            paginationInfo.GridType = gridType.NullOrEmpty() ? "ej2" : gridType;

            if (paginationInfo.GridType.Equals("bootstrap-table"))
            {
                var filterValue = query.GetStringValue("filter");
                if (filterValue.NotNullOrEmpty())
                {
                    var filterValueList = new List<string>();
                    dynamic filterObj = JsonConvert.DeserializeObject(filterValue.ToString());
                    foreach (var item in filterObj)
                    {
                        filterValueList.Add($"{item.Name} like '%{item.Value.Value}%'");
                    }

                    if (filterValueList.Count > 0) paginationInfo.FilterBy = $"Where {string.Join(" And ", filterValueList)}";

                    var filterParts = paginationInfo.FilterBy.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (filterParts.Length! <= 1) paginationInfo.FilterBy = "";
                }

                var sort = query.GetStringValue("sort");
                var order = query.GetStringValue("order");
                if (sort.NotNullOrEmpty() && order.NotNullOrEmpty()) paginationInfo.OrderBy = $"Order By {sort} {order}";

                var skip = query.GetStringValue("offset");
                var take = query.GetStringValue("limit");
                if (skip.NotNullOrEmpty() && take.NotNullOrEmpty()) paginationInfo.PageBy = $"Offset {skip} Rows Fetch Next {take} Rows Only";
                else paginationInfo.PageBy = "Offset 0 Rows Fetch Next 10 Rows Only";
            }
            else
            {
                var filterValue = query.GetStringValue("$filter");
                if (filterValue.NotNullOrEmpty())
                {
                    var filterValueList = new List<string>();
                    var filterValues = filterValue.Split(new string[] { "and" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var fValue in filterValues)
                    {
                        var fValueParts = fValue.Split(",'");
                        if (fValue.Contains("eq") && fValueParts.Length == 1)
                        {
                            var fValueStr = fValue.Replace("tolower(", "").Replace(")", "").Replace("(", "").Replace("'", "");
                            fValueParts = fValueStr.Split("eq", StringSplitOptions.RemoveEmptyEntries);
                            var value = fValueParts[1].Trim().ReplaceBoolToSql();
                            filterValueList.Add($"{fValueParts[0].Trim()} Like '%{value}%'");
                        }
                        else
                        {
                            if (fValueParts.Length == 1) continue;

                            var field = fValueParts[0].Substring(fValueParts[0].LastIndexOf('(') + 1).Replace(")", "");
                            var value = fValueParts[1].Replace(")", "").Replace("'", "");
                            if (value.Trim().NullOrEmpty()) continue;
                            filterValueList.Add($"{field} Like '%{value.Trim()}%'");
                        }
                    }

                    if (filterValueList.Count > 0) paginationInfo.FilterBy = $"Where {string.Join(" And ", filterValueList)}";

                    var filterParts = paginationInfo.FilterBy.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (filterParts.Length <= 1) paginationInfo.FilterBy = "";
                }

                var orderBy = query.GetStringValue("$orderby");
                if (orderBy.NotNullOrEmpty()) paginationInfo.OrderBy = $"Order By {orderBy}";

                var skip = query.GetStringValue("$skip");
                var take = query.GetStringValue("$top");

                if (skip.NotNullOrEmpty() && take.NotNullOrEmpty()) paginationInfo.PageBy = $"Offset {skip} Rows Fetch Next {take} Rows Only";
                else paginationInfo.PageBy = "Offset 0 Rows Fetch Next 10 Rows Only";
            }

            return paginationInfo;
        }
    }
}


