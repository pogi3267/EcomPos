using ApplicationCore.Entities;
using ApplicationCore.Entities.SetupAndConfigurations;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Products
{
    public class ShippingStateService : IShippingStateService
    {
        private readonly IDapperService<State> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public ShippingStateService(IDapperService<State> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<State> GetAsync(int id)
        {
            var query = $@"
                    SELECT * FROM States WHERE StateId = {id};

                    SELECT CountriesId as Id, Name as Text  FROM Countries;";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                State data = queryResult.Read<State>().FirstOrDefault();
                data.CountriesList = DFS.GetCategory(queryResult.Read<CategorySelect2Option>().ToList());
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

        public async Task<int> SaveAsync(State entity)
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

        public async Task<List<State>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY StateId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT st.*,cun.Name as CountryName,st.Name , Count(*) Over() TotalRows FROM States st LEFT JOIN Countries cun ON st.CountriesId=cun.CountriesId ";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%' or Code like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<State>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<State> GetInitial()
        {
            try
            {
                State data = new State();
                var query = $@"SELECT CountriesId as Id, Name as Text  FROM Countries;";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.CountriesList = DFS.GetCategory(queryResult.Read<CategorySelect2Option>().ToList());
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}