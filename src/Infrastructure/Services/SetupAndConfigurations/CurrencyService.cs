using ApplicationCore.Entities.SetupAndConfigurations;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services.SetupAndConfigurations
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IDapperService<Currency> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public CurrencyService(IDapperService<Currency> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Currency> GetAsync(int id)
        {
            var query = $@"SELECT * FROM Currencies WHERE CurrencyId = {id};";

            try
            {
                await _connection.OpenAsync();
                Currency data = await _connection.QuerySingleAsync<Currency>(query);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<int> SaveAsync(Currency entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                var id = await _service.SaveSingleAsync(entity, transaction);
                transaction.Commit();
                return id;
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                transaction.Dispose();
                _connection.Close();
            }
        }

        public async Task<List<Currency>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY CurrencyId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT *, Count(*) Over() TotalRows FROM Currencies";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Currency>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<List<Currency>> GetAllCategories()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateCurrencyStatus(int currencyId, bool isChecked)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = "";
                if (isChecked)
                    sql += $@"
                    UPDATE Currencies SET Status = 0;
    
                    UPDATE BusinessSettings SET [Value] = (SELECT Symbol FROM Currencies WHERE CurrencyId = {currencyId}) 
                    WHERE Type = 'system_default_currency';
                    ";

                int status = isChecked ? 1 : 0;
                sql += $@"
                UPDATE Currencies SET Status = {status} WHERE CurrencyId = {currencyId};
                ";

                var id = await _connection.ExecuteAsync(sql, new { }, transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                transaction.Dispose();
                _connection.Close();
            }
        }
    }
}