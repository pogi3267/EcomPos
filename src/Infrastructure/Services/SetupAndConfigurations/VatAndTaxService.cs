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
    public class VatAndTaxService : IVatAndTaxService
    {
        private readonly IDapperService<Tax> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public VatAndTaxService(IDapperService<Tax> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Tax> GetAsync(int id)
        {
            var query = $@"SELECT * FROM Taxes WHERE TaxId = {id};";

            try
            {
                await _connection.OpenAsync();
                Tax data = await _connection.QuerySingleAsync<Tax>(query);
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

        public async Task<int> SaveAsync(Tax entity)
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
        public async Task<List<Tax>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY TaxId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT *, Count(*) Over() TotalRows FROM Taxes";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Tax>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
