using ApplicationCore.Entities;
using ApplicationCore.Entities.SetupAndConfigurations;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.SetupAndConfigurations
{
    public class BusinessSettingService : IBusinessSettingService
    {
        private readonly IDapperService<BusinessSetting> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public BusinessSettingService(IDapperService<BusinessSetting> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<BusinessSetting> GetAsync(string type)
        {
            var query = $@"SELECT BusinessSettingsId, [Type], [Value], Lang FROM BusinessSettings where [Type] = '{type}';";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                BusinessSetting data = queryResult.Read<BusinessSetting>().FirstOrDefault();
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

        public async Task<List<BusinessSetting>> GetsAsync(string types)
        {
            var query = $@"SELECT [Type], [Value] FROM BusinessSettings 
                        WHERE [Type] IN (SELECT Value FROM string_split('{types}',','))";
            try
            {
                var result = await _service.GetDataAsync<BusinessSetting>(query);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public async Task<BusinessSetting> GetsImagesAsync()
        {
            var query = $@"
            SELECT Id = ROW_NUMBER() OVER(ORDER BY (SELECT NULL)), Text = value 
            FROM STRING_SPLIT((Select Value from BusinessSettings WHERE Type = 'site_carousel_images' ), ',')

            SELECT Id = ROW_NUMBER() OVER(ORDER BY (SELECT NULL)), Text = value 
            FROM STRING_SPLIT((Select Value from BusinessSettings WHERE Type = 'footer_online_payment_icon' ), ',');

            SELECT Id = ROW_NUMBER() OVER(ORDER BY (SELECT NULL)), Text = value 
            FROM STRING_SPLIT((Select Value from BusinessSettings WHERE Type = 'home_banner1_image' ), ',');

            SELECT Id = ROW_NUMBER() OVER(ORDER BY (SELECT NULL)), Text = value 
            FROM STRING_SPLIT((Select Value from BusinessSettings WHERE Type = 'home_banner2_image' ), ',');

            SELECT Id = ROW_NUMBER() OVER(ORDER BY (SELECT NULL)), Text = value 
            FROM STRING_SPLIT((Select Value from BusinessSettings WHERE Type = 'home_banner3_image' ), ',');
            ";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                BusinessSetting data = new BusinessSetting();
                data.CarouselImageList = queryResult.Read<Select2OptionModel>().ToList();
                data.PaymentImageList = queryResult.Read<Select2OptionModel>().ToList();
                data.Banner1ImageList = queryResult.Read<Select2OptionModel>().ToList();
                data.Banner2ImageList = queryResult.Read<Select2OptionModel>().ToList();
                data.Banner3ImageList = queryResult.Read<Select2OptionModel>().ToList();
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
        public async Task<int> SaveAsync(BusinessSetting entity)
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

        public async Task SaveMultipleAsync(List<BusinessSetting> entities)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveAsync(entities, transaction);
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

    }
}
