using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Dapper;
using Dapper.Contrib.Extensions;
using Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class DapperService<T> : DapperBaseService<T>, IDapperService<T> where T : class, IBaseEntity
    {
        private readonly IConfiguration _configuration;

        public DapperService(IConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;
            var conStr = _configuration.GetConnectionString("DefaultConnection"); //CommonDB
            Connection = new SqlConnection(conStr);
        }

        public async Task<int> SaveSingleAsync(T entity, SqlTransaction transaction)
        {
            int newId = 0;
            switch (entity.EntityState)
            {
                case EntityState.Added:
                    newId = await Connection.InsertAsync(entity, transaction);
                    break;
                case EntityState.Deleted:
                    await Connection.DeleteAsync(entity, transaction);
                    break;
                case EntityState.Modified:
                    await Connection.UpdateAsync(entity, transaction);
                    break;
                default:
                    break;
            }

            return newId;
        }

        public async Task<int> SaveSingleAsync<CT>(CT entity, SqlTransaction transaction) where CT : class, IBaseEntity
        {
            int newId = 0;
            switch (entity.EntityState)
            {
                case EntityState.Added:
                    newId = await Connection.InsertAsync(entity, transaction);
                    break;
                case EntityState.Deleted:
                    await Connection.DeleteAsync(entity, transaction);
                    break;
                case EntityState.Modified:
                    await Connection.UpdateAsync(entity, transaction);
                    break;
                default:
                    break;
            }
            return newId;
        }

        public async Task SaveAsync(IEnumerable<T> entities, SqlTransaction transaction)
        {
            var addList = entities.Where(x => x.EntityState == EntityState.Added);
            await Connection.InsertAsync(addList, transaction);

            var updateList = entities.Where(x => x.EntityState == EntityState.Modified);
            if (updateList.Any()) await Connection.UpdateAsync(updateList, transaction);

            var deleteList = entities.Where(x => x.EntityState == EntityState.Deleted);
            if (deleteList.Any()) await Connection.DeleteAsync(deleteList, transaction);
        }

        public async Task SaveAsync<CT>(IEnumerable<CT> entities, SqlTransaction transaction) where CT : class, IBaseEntity
        {
            var addList = entities.Where(x => x.EntityState == EntityState.Added);
            await Connection.InsertAsync(addList, transaction);

            var updateList = entities.Where(x => x.EntityState == EntityState.Modified);
            if (updateList.Any()) await Connection.UpdateAsync(updateList, transaction);

            var deleteList = entities.Where(x => x.EntityState == EntityState.Deleted);
            if (deleteList.Any()) await Connection.DeleteAsync(deleteList, transaction);
        }

        public async Task<int> ExecuteAsync(string query, object param, CommandType commandType = CommandType.Text, int commandTimeOut = 30)
        {
            SqlTransaction transaction = null;

            try
            {
                await Connection.OpenAsync();
                transaction = Connection.BeginTransaction();
                int rows = await Connection.ExecuteAsync(query, param, transaction, commandTimeOut, commandType);
                transaction.Commit();
                return rows;
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex.SqlQueryException(query);
            }
            finally
            {
                transaction.Dispose();
                Connection.Close();
            }
        }

        public int GetAccountIdByCode(string code)
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            string query = "SELECT IIF(id != 0, id, 0) AS result FROM AccountLedger WHERE Code = @Code;";
            var parameters = new { Code = code };

            int accountId = Connection.QueryFirstOrDefault<int>(query, parameters);

            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }

            return accountId;
        }
        public async Task<int> ExecuteQueryAsync(string query, object param, SqlTransaction transaction = null, CommandType commandType = CommandType.Text, int commandTimeOut = 30)
        {
            int rows = await Connection.ExecuteAsync(query, param, transaction, commandTimeOut, commandType);
            return rows;
        }

    
    }
}
