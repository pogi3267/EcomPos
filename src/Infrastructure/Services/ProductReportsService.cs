using ApplicationCore.DTOs;
using ApplicationCore.Entities;
using Dapper;
using Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Infrastructure.Services
{
    public class ProductReportsService : IProductReportsService
    {

        private readonly IDapperService<ReportsDTO> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public ProductReportsService(IDapperService<ReportsDTO> service) : base()
        {
            _connection = service.Connection;
        }


        public async Task<ReportsDTO> GetAsync()
        {
            var query = $@"
                SELECT CategoryId as Value, Name as Text  FROM Categories;
                ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                ReportsDTO data = new ReportsDTO();
                data.CategoryList = queryResult.Read<Select2OptionModel>().ToList();
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
        public async Task<List<ReportsDTO>> GetInhouseProduct(ReportsDTO reportsDTO)
        {
            try
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@categoryId", reportsDTO.CategoryId);
                var data = await _connection.QueryAsync<ReportsDTO>("Rep_InHouseproduct_sales",
                             queryParameters, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ReportsDTO>> GetProductSales(ReportsDTO reportsDTO)
        {
            try
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@categoryId", reportsDTO.CategoryId);
                var data = await _connection.QueryAsync<ReportsDTO>("Rep_product_sales",
                             queryParameters, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ReportsDTO>> GetProductStock(ReportsDTO reportsDTO)
        {
            try
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@categoryId", reportsDTO.CategoryId);
                var data = await _connection.QueryAsync<ReportsDTO>("Rep_Product_Stock",
                             queryParameters, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ReportsDTO>> GetProductWishList(ReportsDTO reportsDTO)
        {
            try
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@categoryId", reportsDTO.CategoryId);
                var data = await _connection.QueryAsync<ReportsDTO>("Rep_product_wishList",
                             queryParameters, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<ReportsDTO>> GetUserSearchList(ReportsDTO reportsDTO)
        {
            try
            {
                var queryParameters = new DynamicParameters();
                queryParameters.Add("@FromDate", reportsDTO.FromDate);
                queryParameters.Add("@ToDate", reportsDTO.ToDate);
                var data = await _connection.QueryAsync<ReportsDTO>("GetUserSearchReport",
                             queryParameters, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<InvoiceOrderDTO> GetInvoiceAsync(int id)
        {
            var query = $@"
           SELECT 
              O.Code, 
              O.Date, 
	          O.PickupPointId,
	          JSON_VALUE(O.ShippingAddress, '$.ReceiverName') ReceiverName,
	          CONCAT(
              JSON_VALUE(O.ShippingAddress, '$.Address1'), ', ',
              JSON_VALUE(O.ShippingAddress, '$.City'), ', ',
              JSON_VALUE(O.ShippingAddress, '$.State'), ', ',
              JSON_VALUE(O.ShippingAddress, '$.Country')
              ) AS FullAddress,
	          JSON_VALUE(O.ShippingAddress, '$.Phone') Phone,
	          JSON_VALUE(O.ShippingAddress, '$.AddressType') AddressType,
              O.PaymentType, 
              O.PaymentStatus, 
              O.PaymentDetails, 
              CONCAT(U.FirstName, ' ', U.LastName) AS CustomerName, 
              U.UserName AS CustomerUserName, 
              ISNULL(U.PhoneNumber, '') AS PhoneNumber
            FROM Orders O
            INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
            WHERE  O.OrdersId= {id};

           Select P.Name ProductName, OD.Quantity, OD.Price, OD.Tax, OD.ShippingCost, OD.Discount,COALESCE(OD.Quantity, 0) * COALESCE(OD.Price, 0) AS TotalAmount,
           OD.ShippingType
           FROM OrderDetails OD
           INNER JOIN Products P ON P.ProductId = OD.ProductId
           WHERE OD.OrderId = {id};
           
           select SystemName,SystemLogoWhite, PhoneNumber, TelephonNumber, Email, Address from GeneralSettings;

            ";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                InvoiceOrderDTO data = queryResult.Read<InvoiceOrderDTO>().FirstOrDefault();
                data.OrderDetails = queryResult.Read<InvoiceOrderDetail>().ToList();
                data.CompanyInfo = queryResult.Read<GeneralSettings>().FirstOrDefault();
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

    }
}

