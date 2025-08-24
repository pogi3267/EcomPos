using ApplicationCore.Entities.Products;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services.Products
{
    public class ProductStockService : IProductStockService
    {
        private readonly IDapperService<ProductStock> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public ProductStockService(IDapperService<ProductStock> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<int> SaveMultipleAsync(IEnumerable<ProductStock> entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveAsync(entity, transaction);
                transaction.Commit();
                return 1;
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
        public async Task<int> SaveSingleAsync(ProductStock entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                int id = await _service.SaveSingleAsync(entity, transaction);
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
        public async Task<List<ProductStock>> GetProductStock(int productId)
        {
            try
            {
                var query = $@"select Variant_id, ps.ProductId, Variant, ISNULL(SKU, '') SKU, Price,  up.FileName
                        from ProductVariants ps
                        left join Uploads up on up.UploadId = ps.Image
                        WHERE ps.ProductId = " + productId + ";";
                var result = await _service.GetDataAsync<ProductStock>(query);
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
        public async Task<ProductStock> GetSingleStock(int productStockId)
        {
            try
            {
                var query = $@"select * from ProductStocks where ProductStockId = " + productStockId + ";";
                var result = await _service.GetFirstOrDefaultAsync<ProductStock>(query);
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
