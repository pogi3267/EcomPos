using ApplicationCore.Entities.SetupAndConfigurations;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services.Products
{
    public class ShippingCountryService : IShippingCountryService
    {
        private readonly IDapperService<Country> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public ShippingCountryService(IDapperService<Country> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Country> GetAsync(int id)
        {
            var query = $@"SELECT * FROM Countries WHERE CountriesId = {id};";

            try
            {
                await _connection.OpenAsync();
                Country data = await _connection.QuerySingleAsync<Country>(query);
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

        public async Task<int> SaveAsync(Country entity)
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

        public async Task<List<Country>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY CountriesId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT *, Count(*) Over() TotalRows FROM Countries";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%' or Code like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Country>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}