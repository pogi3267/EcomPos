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
    public class ShippingCityService : IShippingCityService
    {
        private readonly IDapperService<City> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public ShippingCityService(IDapperService<City> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<City> GetAsync(int id)
        {
            var query = $@"
                SELECT ct.*,st.Name as StateName, cun.Name as CountryName, cun.CountriesId CountryId
				FROM Cities ct
				LEFT JOIN States st ON ct.StateId=st.StateId
                LEFT JOIN Countries cun ON st.CountriesId=cun.CountriesId
                WHERE ct.CitiesId = {id};

                SELECT StateId as Id, Name as Text  FROM States ORDER BY Name ASC;
                ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                City data = queryResult.Read<City>().FirstOrDefault();
                data.StateList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<int> SaveAsync(City entity)
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

        public async Task<List<City>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY CitiesId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT ct.*,st.Name as StateName, cun.Name as CountryName,
                            Count(*) Over() TotalRows
                            FROM Cities ct
                            LEFT JOIN States st ON ct.StateId=st.StateId
                            LEFT JOIN Countries cun ON st.CountriesId=cun.CountriesId";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%' or Code like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<City>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<City> GetNew()
        {
            var query = $@"
                SELECT StateId as Id, Name as Text  FROM States ORDER BY Name ASC;
            ";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                City data = new City();
                data.StateList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<List<Select2OptionModel>> GetState(int countryId)
        {
            try
            {
                string sql = $@"SELECT StateId as Id, Name as Text  FROM States WHERE CountriesId = {countryId}  ORDER BY Name ASC;";

                var result = await _service.GetDataAsync<Select2OptionModel>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<City> GetCityEntityAsync(int id)
        {
            var query = $@"
                SELECT * FROM Cities WHERE CitiesId = {id};
                ";

            try
            {
                await _connection.OpenAsync();
                City data = await _connection.QuerySingleAsync<City>(query);
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

    }
}