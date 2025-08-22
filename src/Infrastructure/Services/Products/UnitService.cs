using ApplicationCore.Entities.Products;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services.Products
{
    public class UnitService : IUnitService
    {
        private readonly IDapperService<Unit> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public UnitService(IDapperService<Unit> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Unit> GetAsync(int id)
        {
            var query = $@"SELECT * FROM Units WHERE UnitId = {id};";

            try
            {
                await _connection.OpenAsync();
                Unit data = await _connection.QuerySingleAsync<Unit>(query);
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

        public async Task<int> SaveAsync(Unit entity)
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
        public async Task<List<Unit>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY UnitId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT *, Count(*) Over() TotalRows FROM Units";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%' or Abbreviation like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Unit>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
