using ApplicationCore.Interfaces;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IDapperService<T> : IDapperBaseService<T> where T : class, IBaseEntity
    {
        Task<int> SaveSingleAsync(T entity, SqlTransaction transaction);
        Task<int> SaveSingleAsync<CT>(CT entity, SqlTransaction transaction) where CT : class, IBaseEntity;
        Task SaveAsync(IEnumerable<T> entities, SqlTransaction transaction);
        Task SaveAsync<CT>(IEnumerable<CT> entities, SqlTransaction transaction) where CT : class, IBaseEntity;
        Task<int> ExecuteAsync(string query, object param, CommandType commandType = CommandType.Text, int commandTimeOut = 30);
        public int GetAccountIdByCode(string code);
        Task<int> ExecuteQueryAsync(string query, object param, SqlTransaction transaction, CommandType commandType = CommandType.Text, int commandTimeOut = 30);

    }
}