using ApplicationCore.Entities.SetupAndConfigurations;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.SetupAndConfigurations
{
    public class GeneralSettingsService : IGeneralSettings
    {
        private readonly IDapperService<GeneralSettings> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public GeneralSettingsService(IDapperService<GeneralSettings> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<GeneralSettings> GetAsync()
        {
            var query = $@"SELECT * FROM GeneralSettings";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                GeneralSettings data = queryResult.Read<GeneralSettings>().FirstOrDefault();
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

        public async Task<int> SaveAsync(GeneralSettings entity)
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

    }
}
