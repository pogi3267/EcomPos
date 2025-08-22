using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationCore.Entities.Inventory;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Inventory;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Services.Inventory
{
    public class ItemService: IItemService
    {
         private readonly IDapperService<Item> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;

        public ItemService(IDapperService<Item> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        
        public async Task<List<Item>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY ItemId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT *, Count(*) Over() TotalRows FROM Item";
                if (searchBy != "")
                    sql += " WHERE ItemName like '%" + searchBy + "%' or ItemCode like '%" + searchBy + "%' ";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Item>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public async Task<Item> GetAsync(int id)
        {
            try
            {
                var query = $@"SELECT * FROM Item WHERE ItemId = {id};";
                await _connection.OpenAsync();
                Item data = await _connection.QuerySingleAsync<Item>(query);
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

        public async Task<int> SaveAsync(Item entity)
        {
            try
            {
                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();
                var id = await _service.SaveSingleAsync(entity, _transaction);
                _transaction.Commit();
                return id;
            }
            catch (Exception ex)
            {
                if (_transaction != null) _transaction.Rollback();
                throw ex;
            }
            finally
            {
                _transaction.Dispose();
                _connection.Close();
            }
        }
    }
}