using ApplicationCore.DTOs;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Products;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.Data.SqlClient;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Infrastructure.Services.Products
{
    public class BulkActionService : IBulkActionService
    {
        private readonly IDapperService<ApplicationCore.Entities.Products.Product> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public BulkActionService(IDapperService<ApplicationCore.Entities.Products.Product> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task SaveProductsToDatabase(List<BulkImport> products)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                var query = @"INSERT INTO products 
                      (Name, Description, CategoryId, BrandId, VideoProvider, VideoLink, Tags, UnitPrice, UnitId, Slug, CurrentStock, EstShippingDays, ProductSKU, MetaDescription, ThumbnailImage, Photos)
                      VALUES 
                      (@Name, @Description, @CategoryId, @BrandId, @VideoProvider, @VideoLink, @Tags, @UnitPrice, @UnitId, @Slug, @CurrentStock, @EstShippingDays, @ProductSKU, @MetaDescription, @ThumbnailImage, @Photos)";

                // Insert each product into the database
                foreach (var product in products)
                {
                    await _connection.ExecuteAsync(query, product, transaction);
                }
                transaction.Commit();
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

        public async Task<List<ApplicationCore.Entities.Products.Product>> GetProductsAsync()
        {
            try
            {
                var query = @"SELECT 
                        Name, 
                        Description, 
                        CategoryId, 
                        BrandId, 
                        VideoProvider, 
                        VideoLink, 
                        Tags, 
                        UnitPrice, 
                        UnitId, 
                        Slug, 
                        CurrentStock, 
                        EstShippingDays, 
                        ProductSKU, 
                        MetaDescription, 
                        ThumbnailImage, 
                        Photos
                      FROM Products";

                var result = await _service.GetDataAsync<ApplicationCore.Entities.Products.Product>(query);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                var query = @"SELECT 
                            CategoryId, ParentId, Level, Name, OrderLevel, Banner, Icon, Featured, Digital, Slug, MetaTitle, MetaDescription
                            FROM Categories";

                var result = await _service.GetDataAsync<Category>(query);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Brand>> GetBrandsAsync()
        {
            try
            {
                var query = @"SELECT 
                            BrandId, [Name], Logo, [Top], Slug, MetaDescription, MetaTittle
                            FROM Brands";

                var result = await _service.GetDataAsync<Brand>(query);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
