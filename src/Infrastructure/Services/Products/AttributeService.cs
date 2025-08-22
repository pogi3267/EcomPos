
using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services.Products
{
    public class AttributeService : IAttributeService
    {
        private readonly IDapperService<ProductAttribute> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public AttributeService(IDapperService<ProductAttribute> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<ProductAttribute> GetAsync(int id)
        {
            var query = $@"SELECT * FROM Attributes WHERE AttributeId = {id};";

            try
            {
                await _connection.OpenAsync();
                ProductAttribute data = await _connection.QuerySingleAsync<ProductAttribute>(query);
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

        public async Task<ProductAttribute> SaveAsync(ProductAttribute entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        int attributeId = await _service.SaveSingleAsync(entity, transaction);
                        entity.AttributeValueList.ForEach(item => { item.AttributeId = attributeId; });
                        await _service.SaveAsync(entity.AttributeValueList, transaction);
                        break;

                    case EntityState.Modified:
                        await _service.SaveSingleAsync(entity, transaction);
                        await _service.SaveAsync(entity.AttributeValueList, transaction);
                        break;

                    case EntityState.Deleted:
                        await _service.SaveSingleAsync(entity, transaction);
                        await _service.SaveAsync(entity.AttributeValueList, transaction);
                        break;

                    default:
                        break;
                }

                transaction.Commit();
                return entity;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<List<ProductAttribute>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY AttributeId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"
                SELECT A.*,
                (Select STRING_AGG(Value, ',') from AttributeValues WHERE AttributeId = A.AttributeId) [Values],
                Count(*) Over() TotalRows FROM Attributes A";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<ProductAttribute>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<AttributeValue>> GetAttributeValues(int attributeId)
        {
            var query = $@"Select Id, AttributeId, Value, ColorCode from AttributeValues WHERE AttributeId = {attributeId};";

            try
            {
                var result = await _service.GetDataAsync<AttributeValue>(query);
                return result;
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
