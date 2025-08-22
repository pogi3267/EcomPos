using ApplicationCore.Entities;
using ApplicationCore.Entities.Products;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Products
{
    public class CategoryService : ICategoryService
    {
        private readonly IDapperService<Category> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public CategoryService(IDapperService<Category> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Category> GetInitial()
        {
            try
            {
                Category data = new Category();
                var query = $@"SELECT CategoryId as Id, Name as Text, ParentId as ParentId  FROM Categories;";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.ParentList = DFS.GetCategory(queryResult.Read<CategorySelect2Option>().ToList());
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Category>> GetAllCategories()
        {
            var query = $@"SELECT * FROM Categories;";

            try
            {
                var result = await _service.GetDataAsync<Category>(query);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Category> GetAsync(int id)
        {
            var query = $@"SELECT * FROM Categories WHERE CategoryId = {id};
                            -- dropdown list
                          SELECT CategoryId as Id, Name as Text, ParentId as ParentId  FROM Categories;
                            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Category data = queryResult.Read<Category>().FirstOrDefault();
                data.ParentList = DFS.GetCategory(queryResult.Read<CategorySelect2Option>().ToList());
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

        public async Task<int> SaveAsync(Category entity)
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
        public async Task<List<Category>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY CategoryId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"select pa.Name ParentName, Count(1) Over() TotalRows, ct.* 
                                from Categories ct
                                left join Categories pa on pa.CategoryId = ct.ParentId
                                ";
                if (searchBy != "")
                    sql += " WHERE ct.Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Category>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
