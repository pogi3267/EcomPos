using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Entities.Orders;
using Dapper;
using Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public CustomerService(IConfiguration configuration)
        {
            _configuration = configuration;
            var conStr = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(conStr);
        }

        public async Task<List<User>> GetList(string searchBy, int take, int skip, string sortBy, string sortDir, string status)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY AU.UserName DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string searchSql = "";
                if (searchBy != "")
                    searchSql += " AND CONCAT(AU.FirstName, AU.LastName) like '%" + searchBy + "%'";
                string sql = "";
                if (status == "all")
                {
                    sql = $@"
                    Select 
                    AU.Id, AU.UserName, ISNULL(AU.Email, '') Email, AU.EmailConfirmed, ISNULL(AU.PhoneNumber, '') PhoneNumber, AU.PhoneNumberConfirmed, 
                    ISNULL(AU.FirstName, '') FirstName, ISNULL(AU.LastName, '') LastName, ISNULL(AU.Avatar, '') Avatar, ISNULL(AU.[Address], '') [Address],
                    ISNULL(AU.Country, '') Country, ISNULL(AU.[State], '') [State], ISNULL(AU.City, '') City, ISNULL(AU.PostalCode, '') PostalCode,
                    ISNULL(AU.Banned, 0) Banned, COUNT(O.OrdersId) TotalOrder, Count(1) Over() TotalRows
                    FROM AspNetUsers AU
                    LEFT JOIN Orders O ON O.UserId = AU.Id
                    WHERE AU.Id <> ''
                    {searchSql}
                    GROUP BY AU.Id, AU.UserName, AU.Email, AU.EmailConfirmed, AU.PhoneNumber, AU.PhoneNumberConfirmed, 
                    AU.FirstName, AU.LastName, AU.Avatar, AU.[Address], AU.Country, AU.[State], AU.City, AU.PostalCode,
                    AU.Banned
                    ";
                }
                else if (status == "active")
                {
                    sql = $@"
                    Select 
                    AU.Id, AU.UserName, ISNULL(AU.Email, '') Email, AU.EmailConfirmed, ISNULL(AU.PhoneNumber, '') PhoneNumber, AU.PhoneNumberConfirmed, 
                    ISNULL(AU.FirstName, '') FirstName, ISNULL(AU.LastName, '') LastName, ISNULL(AU.Avatar, '') Avatar, ISNULL(AU.[Address], '') [Address],
                    ISNULL(AU.Country, '') Country, ISNULL(AU.[State], '') [State], ISNULL(AU.City, '') City, ISNULL(AU.PostalCode, '') PostalCode,
                    ISNULL(AU.Banned, 0) Banned, COUNT(O.OrdersId) TotalOrder, Count(1) Over() TotalRows
                    FROM AspNetUsers AU
                    LEFT JOIN Orders O ON O.UserId = AU.Id
                    WHERE AU.Id <> '' AND ISNULL(AU.Banned, 0) = 0
                    {searchSql}
                    GROUP BY AU.Id, AU.UserName, AU.Email, AU.EmailConfirmed, AU.PhoneNumber, AU.PhoneNumberConfirmed, 
                    AU.FirstName, AU.LastName, AU.Avatar, AU.[Address], AU.Country, AU.[State], AU.City, AU.PostalCode,
                    AU.Banned
                    ";
                }
                else if (status == "inactive")
                {
                    sql = $@"
                    Select 
                    AU.Id, AU.UserName, ISNULL(AU.Email, '') Email, AU.EmailConfirmed, ISNULL(AU.PhoneNumber, '') PhoneNumber, AU.PhoneNumberConfirmed, 
                    ISNULL(AU.FirstName, '') FirstName, ISNULL(AU.LastName, '') LastName, ISNULL(AU.Avatar, '') Avatar, ISNULL(AU.[Address], '') [Address],
                    ISNULL(AU.Country, '') Country, ISNULL(AU.[State], '') [State], ISNULL(AU.City, '') City, ISNULL(AU.PostalCode, '') PostalCode,
                    ISNULL(AU.Banned, 0) Banned, COUNT(O.OrdersId) TotalOrder, Count(1) Over() TotalRows
                    FROM AspNetUsers AU
                    LEFT JOIN Orders O ON O.UserId = AU.Id
                    WHERE AU.Id <> '' AND ISNULL(AU.Banned, 0) = 1
                    {searchSql}
                    GROUP BY AU.Id, AU.UserName, AU.Email, AU.EmailConfirmed, AU.PhoneNumber, AU.PhoneNumberConfirmed, 
                    AU.FirstName, AU.LastName, AU.Avatar, AU.[Address], AU.Country, AU.[State], AU.City, AU.PostalCode,
                    AU.Banned
                    ";
                }

                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";

                try
                {
                    await _connection.OpenAsync();
                    var records = await _connection.QueryAsync<User>(sql);
                    return records.AsList();
                }
                catch (Exception ex)
                {
                    throw ex.SqlQueryException(sql);
                }
                finally
                {
                    _connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<User> GetAsync(string id)
        {
            var query = $@"
            WITH OD AS (
                SELECT * 
                FROM Orders O WHERE O.UserId = '{id}'
            )
            Select 
            AU.Id, AU.UserName, ISNULL(AU.Email, '') Email, AU.EmailConfirmed, ISNULL(AU.PhoneNumber, '') PhoneNumber, AU.PhoneNumberConfirmed, dbo.GetLocalDate(AU.CreatedDate) CreatedDate,
            ISNULL(AU.FirstName, '') FirstName, ISNULL(AU.LastName, '') LastName, ISNULL(AU.Avatar, '') Avatar, ISNULL(AU.[Address], '') [Address],
            ISNULL(AU.Country, '') Country, ISNULL(AU.[State], '') [State], ISNULL(AU.City, '') City, ISNULL(AU.PostalCode, '') PostalCode,
            ISNULL(AU.Banned, 0) Banned, --COUNT(OD.OrdersId) OVER() TotalOrder
            (SELECT COUNT(1) FROM OD)TotalOrder,
            (SELECT COUNT(1) FROM OD WHERE OD.DeliveryStatus IN ('pending', 'confirmed', 'packaging', 'outForDelivery')) OngoingOrder,
            (SELECT COUNT(1) FROM OD WHERE OD.DeliveryStatus = 'delivered') CompleteOrder,
            (SELECT COUNT(1) FROM OD WHERE OD.DeliveryStatus = 'returned') ReturnedOrder,
            (SELECT COUNT(1) FROM OD WHERE OD.DeliveryStatus = 'failedToDeliver') FailedToDeliverOrder,
            (SELECT COUNT(1) FROM OD WHERE OD.DeliveryStatus = 'canceled') CanceledOrder
            FROM AspNetUsers AU
            LEFT JOIN OD ON OD.UserId = AU.Id
            WHERE AU.Id = '{id}';

            Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
            O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) Date, O.Viewed, O.DeliveryViewed, 
            O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber
            from Orders O
            INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
            WHERE O.UserId = '{id}';
            ";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                User data = queryResult.Read<User>().FirstOrDefault();
                data.Orders = queryResult.Read<Orders>().ToList();
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