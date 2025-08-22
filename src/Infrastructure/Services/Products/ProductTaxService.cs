using ApplicationCore.Entities.Products;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Services.Products
{
    public class ProductTaxService : IProductTaxService
    {
        private readonly IDapperService<ProductTax> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public ProductTaxService(IDapperService<ProductTax> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<int> SaveSingleAsync(ProductTax entity)
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
    }
}
