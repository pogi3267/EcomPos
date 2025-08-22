using ApplicationCore.Entities.Products;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services.Products
{
    public class UploadService : IUploadService
    {
        private readonly IDapperService<Upload> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public UploadService(IDapperService<Upload> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public List<Upload> GetAll()
        {
            var query = $@"select UploadId, FileName from Uploads WHERE ISNULL(IsDeleted, 0) = 0 order by Created_At desc;";

            try
            {
                var data = _service.GetDataAsync<Upload>(query);
                return data.Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> SaveAllAsync(List<Upload> entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveAsync(entity, transaction);
                transaction.Commit();
                return entity.Count;
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
