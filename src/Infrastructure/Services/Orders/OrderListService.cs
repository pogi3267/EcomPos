using ApplicationCore.Entities.Orders;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Marketing;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Infrastructure.Services.Marketing
{
    public class OrderListService : IOrderListService
    {
        private readonly IDapperService<Orders> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public OrderListService(IDapperService<Orders> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<List<Orders>> GetList(string searchBy, int take, int skip, string sortBy, string sortDir, string status, bool isPickupOrder)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY O.OrdersId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);

                string pickUpCondition = "";
                if (isPickupOrder) pickUpCondition = " AND ISNULL(O.PickupPointId, 0) > 0 ";

                string sql = "";
                if (status == "all")
                {
                    sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    Count(1) Over() TotalRows
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId <> 0 {pickUpCondition}";
                }
                else if (status == "pending")
                {
                    sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    Count(1) Over() TotalRows
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId <> 0 AND O.DeliveryStatus = 'pending'  {pickUpCondition}";
                }
                else if (status == "confirmed")
                {
                    sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    Count(1) Over() TotalRows
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId <> 0 AND O.DeliveryStatus = 'confirmed'  {pickUpCondition}";
                }
                else if (status == "packaging")
                {
                    sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    Count(1) Over() TotalRows
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId <> 0 AND O.DeliveryStatus = 'packaging'  {pickUpCondition}";
                }
                else if (status == "outForDelivery")
                {
                    sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    Count(1) Over() TotalRows
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId <> 0 AND O.DeliveryStatus = 'outForDelivery'  {pickUpCondition}";
                }
                else if (status == "delivered")
                {
                    sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    Count(1) Over() TotalRows
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId <> 0 AND O.DeliveryStatus = 'delivered'  {pickUpCondition}";
                }
                else if (status == "returned")
                {
                    sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    Count(1) Over() TotalRows
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId <> 0 AND O.DeliveryStatus = 'returned'  {pickUpCondition}";
                }
                else if (status == "failedToDeliver")
                {
                    sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    Count(1) Over() TotalRows
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId <> 0 AND O.DeliveryStatus = 'failedToDeliver'  {pickUpCondition}";
                }
                else if (status == "canceled")
                {
                    sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    Count(1) Over() TotalRows
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId <> 0 AND O.DeliveryStatus = 'canceled'  {pickUpCondition}";
                }

                if (searchBy != "")
                    sql += " AND CONCAT(O.Code, U.FirstName, U.LastName, U.UserName) like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Orders>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Orders> GetAsync(int id)
        {
            var query = $@"
            Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
            O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) [Date], O.Viewed, O.DeliveryViewed, 
            O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
            PP.Name PickupPointName, PP.Phone PickupPointPhone, PP.Address PickupPointAddress, O.TotalAdminDiscount, O.CourierName, O.CourierTrackingNo
            from Orders O
            INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
            LEFT JOIN PickupPoints PP ON PP.PickupPointId = O.PickupPointId
            WHERE O.OrdersId = {id};

            Select OD.OrderDetailId, OD.OrderId, OD.SellerId, OD.ProductId, OD.Variation, OD.Price, OD.Tax, OD.ShippingCost, OD.Discount, 
            OD.Quantity, OD.AdminDiscount, OD.PaymentStatus, OD.DeliveryStatus, OD.ShippingType, OD.PickupPointId, OD.ProductReferralCode,
            P.Name ProductName, P.UnitPrice ProductPrice, img.FileName ThumbnailImage, PS.SKU VariantSKU, PS.Price VariantPrice
            FROM OrderDetails OD
            INNER JOIN Products P ON P.ProductId = OD.ProductId
            LEFT JOIN Uploads img on img.UploadId = P.ThumbnailImage
            INNER JOIN ProductStocks PS ON PS.ProductId = P.ProductId AND PS.Variant = OD.Variation
            WHERE OD.OrderId = {id};
            ";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Orders data = queryResult.Read<Orders>().FirstOrDefault();
                data.OrderDetailsList = queryResult.Read<OrderDetail>().ToList();
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

        public async Task SaveAsync(Orders entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                DECLARE 
                @CurrentStatus nvarchar(100) = (Select TOP 1 CurrentStatus from OrderStatusHistory WHERE OrderId = {entity.OrdersId} ORDER BY HistoryId DESC)
                IF(@CurrentStatus != '{entity.DeliveryStatus}')
                BEGIN
	                INSERT INTO OrderStatusHistory (OrderId, UserId, CurrentStatus, PreviousStatus, ChangedDate)
	                SELECT {entity.OrdersId}, '{entity.UserId}', '{entity.DeliveryStatus}', @CurrentStatus, GetUtcDate()
                END

                UPDATE Orders SET PaymentStatus='{entity.PaymentStatus}', DeliveryStatus='{entity.DeliveryStatus}', ShippingType='{entity.ShippingType}', 
                CourierName='{entity.CourierName}', CourierTrackingNo='{entity.CourierTrackingNo}',
                TotalAdminDiscount = (SELECT SUM(ISNULL(AdminDiscount, 0)) FROM OrderDetails WHERE OrderId = {entity.OrdersId})
                WHERE OrdersId={entity.OrdersId}
                ";

                await _connection.ExecuteAsync(sql, new { }, transaction);
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

        public async Task DeleteNotificationAsync()
        {

            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"UPDATE OrderNotification SET IsView=1;";

                await _connection.ExecuteAsync(sql, new { }, transaction);
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

        public async Task SaveDetailAsync(OrderDetail detail)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                var result = await _connection.QueryAsync("SP_OrderDetail",
                    new
                    {
                        OrderId = detail.OrderId,
                        OrderDetailId = detail.OrderDetailId,
                        ProductId = detail.ProductId,
                        Variation = detail.Variation,
                        Price = detail.Price,
                        Tax = detail.Tax,
                        ShippingType = detail.ShippingType,
                        ShippingCost = detail.ShippingCost,
                        Discount = detail.Discount,
                        Quantity = detail.Quantity,
                        AdminDiscount = detail.AdminDiscount
                    },
                    transaction, 30, CommandType.StoredProcedure);

                transaction.Commit();

                //return result.AsList().FirstOrDefault(); ;
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