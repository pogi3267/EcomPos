using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IDapperBaseService<T> where T : class
    {
        SqlConnection Connection { get; set; }
        SqlConnection GetInternalConnection();
        SqlConnection GetRiskConnection();
        SqlConnection GetFootballIndicesConnection();
        Task<List<dynamic>> GetDynamicDataAsync(string query);
        Task<List<dynamic>> GetDynamicDataAsync(string query, object param);
        Task<int> GetSingleIntFieldAsync(string query);
        Task<double> GetSingleDoubleFieldAsync(string query);
        Task<bool> GetSingleBooleanFieldAsync(string query);
        Task<string> GetSingleStringFieldAsync(string query);
        Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query);
        Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query, object param);
        Task<List<T>> GetDataAsync(string query);
        Task<List<T>> GetDataAsync(string query, object param);
        Task<List<CT>> GetDataAsync<CT>(string query) where CT : class;
        Task<List<CT>> GetDataAsync<CT>(string query, object param) where CT : class;
        Task<T> GetFirstOrDefaultAsync(string query);
        Task<CT> GetFirstOrDefaultAsync<CT>(string query) where CT : class;
        Task<T> GetFirstOrDefaultAsync(string query, object param);
        Task<CT> GetFirstOrDefaultAsync<CT>(string query, object param) where CT : class;
    }
}