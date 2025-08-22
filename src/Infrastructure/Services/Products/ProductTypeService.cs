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
    public class ProductTypeService : IProductTypeService
    {
        private readonly IDapperService<ProductType> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public ProductTypeService(IDapperService<ProductType> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<ProductType> GetAsync(int id)
        {
            var query = $@"SELECT * FROM ProductTypes WHERE ProductTypeId = {id};";

            try
            {
                await _connection.OpenAsync();
                ProductType data = await _connection.QuerySingleAsync<ProductType>(query);
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

        public async Task<int> SaveAsync(ProductType entity)
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
        public async Task<List<ProductType>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY ProductTypeId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT *, Count(*) Over() TotalRows FROM ProductTypes";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<ProductType>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
