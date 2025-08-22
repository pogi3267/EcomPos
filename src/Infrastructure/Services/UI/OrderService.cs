using ApplicationCore.Common;
using ApplicationCore.Entities.Accounting;
using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using Dapper;
using Infrastructure.Interfaces.UI;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Infrastructure.Services.UI
{
    public class OrderService : IOrderService
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public OrderService(IConfiguration configuration)
        {
            _configuration = configuration;
            var conStr = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(conStr);
        }

        public async Task<List<dynamic>> GetAddressListAsync(string userId)
        {
            try
            {
                string sql = $@"
                SELECT A.AddressId, A.Address1, A.Longitude, A.Latitude, A.PostalCode, A.Phone, A.Email, A.SetDefault, A.ReceiverName, A.AddressType,
                A.Country CountryId, A.State StateId, A.City CityId, C.Name Country, S.Name State, CT.Name City
                FROM Addresses A 
                LEFT JOIN Countries C ON C.CountriesId = A.Country
                LEFT JOIN States S ON S.StateId = A.State
                LEFT JOIN Cities CT ON CT.CitiesId = A.City
                WHERE UserId = '{userId}'
                ORDER BY A.SetDefault DESC, A.AddressId DESC
                ";

                try
                {
                    await _connection.OpenAsync();
                    var records = await _connection.QueryAsync(sql);
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

        public async Task<dynamic> GetAddressAsync(int addressId, string userId)
        {
            try
            {
                string sql = $@"
                SELECT TOP 1 A.AddressId, A.Address1, A.Longitude, A.Latitude, A.PostalCode, A.Phone, A.Email, A.SetDefault, A.ReceiverName, A.AddressType,
                A.Country CountryId, A.State StateId, A.City CityId, C.Name Country, S.Name State, CT.Name City
                FROM Addresses A 
                LEFT JOIN Countries C ON C.CountriesId = A.Country
                LEFT JOIN States S ON S.StateId = A.State
                LEFT JOIN Cities CT ON CT.CitiesId = A.City
                WHERE A.UserId = '{userId}' AND A.AddressId = {addressId}
                ORDER BY A.SetDefault DESC, A.AddressId DESC;
                ";

                try
                {
                    await _connection.OpenAsync();
                    var records = await _connection.QueryAsync(sql);
                    return records.AsList().FirstOrDefault();
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

        public async Task SaveAddressAsync(ApplicationCore.Entities.ApplicationUser.Address address, string userid)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = "";
                if (address.AddressId > 0)
                {
                    if (address.SetDefault)
                        sql += $@"
                        UPDATE Addresses SET SetDefault = 0 WHERE UserId = '{userid}' ;
                        ";

                    sql += $@"
                    UPDATE Addresses SET
                    UserId='{userid}', Address1='{address.Address1}', Country='{address.Country}', State='{address.State}', City='{address.City}',
                    Longitude='{address.Longitude}', Latitude='{address.Latitude}', PostalCode='{address.PostalCode}', ReceiverName='{address.ReceiverName}', AddressType='{address.AddressType}',
                    Phone='{address.Phone}', Email='{address.Email}', SetDefault='{address.SetDefault}', Updated_At=GetUtcDate()
                    WHERE AddressId = {address.AddressId};
                    ";
                }
                else
                {
                    if (address.SetDefault)
                        sql += $@"
                        UPDATE Addresses SET SetDefault = 0 WHERE UserId = '{userid}' ;
                        ";

                    sql += $@"
                    INSERT INTO Addresses
                    (UserId, Address1, Country, State, City, Longitude, Latitude, PostalCode, ReceiverName, Phone, Email, AddressType, SetDefault, Created_At)
                    SELECT
                    '{userid}', '{address.Address1}', '{address.Country}', '{address.State}', '{address.City}',  '{address.Longitude}',
                    '{address.Latitude}', '{address.PostalCode}', '{address.ReceiverName}',  '{address.Phone}', '{address.Email}', '{address.AddressType}', '{address.SetDefault}', GetUtcDate();
                    ";
                }

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

        public async Task DeleteAddressAsync(int addressId, string userid)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"DELETE FROM Addresses WHERE AddressId = {addressId} AND UserId = '{userid}';";

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

        public async Task<dynamic> CreateOrderAsync(string userId, string cartIds, int addressId, int couponId, string paymentType, int pickupId)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                var result = await _connection.QueryAsync("SP_CreateOrder", new { UserId = userId, CartIds = cartIds, AddressId = addressId, CouponId = couponId, PaymentType = paymentType, PickupPointId = pickupId }, transaction, 30, CommandType.StoredProcedure);


               
                transaction.Commit();

                return result.AsList().FirstOrDefault(); ;
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

        public async Task<dynamic> GetOrderAsync(int orderId)
        {
            try
            {
                string sql = $@"
                SELECT O.OrdersId, O.CombinedOrdersId, O.ShippingAddress, O.ShippingType, O.PickupPointId, O.DeliveryStatus, O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.Code
                ,CONCAT(U.FirstName, ' ', U.LastName) Name, U.Email UserEmail                
                FROM Orders O
                LEFT JOIN AspNetUsers U ON U.Id = O.UserId
                WHERE O.OrdersId = {orderId};
                ";

                try
                {
                    await _connection.OpenAsync();
                    var records = await _connection.QueryAsync(sql);
                    return records.AsList().FirstOrDefault();
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

        public async Task<List<dynamic>> GetMyOrdersAsync(string userId)
        {
            try
            {
                string sql = $@"
                WITH ORDR AS (
	                SELECT O.OrdersId, O.GrandTotal, O.Code
	                FROM Orders O WHERE UserId = '{userId}'
                ),
                HISTORY AS (
	                SELECT H.OrderId, MAX(HistoryId) HistoryId 
	                FROM OrderStatusHistory H
	                INNER JOIN ORDR ON ORDR.OrdersId = H.OrderId
	                GROUP BY H.OrderId
                )
                SELECT O.OrdersId OrderId, O.GrandTotal Amount, O.Code OrderCode, H.CurrentStatus OrderStatus, dbo.GetLocalDate(H.ChangedDate) StatusDate,
                (SELECT [FileName] FROM Uploads U WHERE U.UploadId = (SELECT ThumbnailImage FROM Products P WHERE P.ProductId = (SELECT TOP 1 ProductId FROM OrderDetails OD 
                WHERE OD.OrderId = O.OrdersId ORDER BY OD.OrderDetailId ASC))) Photo
                FROM ORDR O
                LEFT JOIN HISTORY ON HISTORY.OrderId = O.OrdersId
                LEFT JOIN OrderStatusHistory H ON H.HistoryId = HISTORY.HistoryId
                ORDER BY H.ChangedDate DESC
                ";

                try
                {
                    await _connection.OpenAsync();
                    var records = await _connection.QueryAsync(sql);
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

        public async Task<dynamic> GetOrderDetailsAsync(int orderId)
        {
            try
            {
                string sql = $@"
                    Select O.OrdersId, O.CombinedOrdersId, O.UserId, O.GuestId, O.SellerId, O.ShippingAddress, ISNULL(O.ShippingType, 'Not Selected') ShippingType, O.PickupPointId, O.DeliveryStatus, 
                    O.PaymentType, O.PaymentStatus, O.PaymentDetails, O.GrandTotal, O.CouponDiscount, O.Code, O.TrackingCode, dbo.GetLocalDate(O.Date) Date, O.Viewed, O.DeliveryViewed, 
                    O.PaymentStatusViewed, O.CommissionCalculated, CONCAT(U.FirstName, ' ', U.LastName) CustomerName, U.UserName CustomerUserName, U.Email, ISNULL(U.PhoneNumber, '') PhoneNumber,
                    O.TotalAdminDiscount TotalRebate, O.CourierName, O.CourierTrackingNo
                    from Orders O
                    INNER JOIN [AspNetUsers] U ON U.Id = O.UserId
                    WHERE O.OrdersId = {orderId};

                    Select OD.OrderDetailId, OD.OrderId, OD.SellerId, OD.ProductId, OD.Variation, OD.Price, OD.Tax, OD.ShippingCost, OD.Discount, 
                    OD.Quantity, OD.PaymentStatus, OD.DeliveryStatus, OD.ShippingType, OD.PickupPointId, OD.ProductReferralCode,
                    P.Name ProductName, P.UnitPrice ProductPrice, img.FileName ThumbnailImage, PS.SKU VariantSKU, PS.Price VariantPrice,
                    OD.AdminDiscount Rebate
                    FROM OrderDetails OD
                    INNER JOIN Products P ON P.ProductId = OD.ProductId
                    LEFT JOIN Uploads img on img.UploadId = P.ThumbnailImage
                    INNER JOIN ProductStocks PS ON PS.ProductId = P.ProductId AND PS.Variant = OD.Variation
                    WHERE OD.OrderId = {orderId};

                    SELECT H.CurrentStatus, dbo.GetLocalDate(H.ChangedDate) StatusDate
                    FROM OrderStatusHistory H
                    WHERE H.OrderId = {orderId}
                    ORDER BY H.HistoryId ASC;
                    ";

                try
                {
                    await _connection.OpenAsync();
                    var queryResult = await _connection.QueryMultipleAsync(sql);
                    dynamic order = queryResult.Read().FirstOrDefault();
                    order.OrderDetailsList = queryResult.Read().ToList();
                    order.StatusHistory = queryResult.Read().ToList();

                    return order;
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

        public async Task<List<dynamic>> GetReviewListAsync(string userId, int orderId)
        {
            try
            {
                string sql = $@"
                WITH REV AS (
                    SELECT * 
                    FROM Reviews R
                    WHERE R.UserId = '{userId}' AND R.OrderId = {orderId} AND R.[Status] = 1
                )
                Select ISNULL(R.ReviewId, 0) ReviewId, ISNULL(R.Rating, 0) Rating, ISNULL(R.Comment, '') Comment, ISNULL(R.ReviewPhotos, '') ReviewPhotos, 
                ISNULL(R.NoOfLike, 0) NoOfLike, ISNULL(R.NoOfDislike, 0) NoOfDislike,
                P.ProductId, P.[Name] ProductName,
                (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage
                From Products P
                LEFT JOIN REV R ON R.ProductId = P.ProductId 
                WHERE 
                P.ProductId IN (
                Select OD.ProductId
                from Orders O
                INNER JOIN OrderDetails OD ON OD.OrderId = O.OrdersId
                WHERE O.OrdersId = {orderId} AND O.UserId = '{userId}'
                GROUP BY OD.ProductId
                )
                ";
                try
                {
                    await _connection.OpenAsync();
                    var records = await _connection.QueryAsync(sql);
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

        public async Task SaveReviewAsync(Reviews review, string userid)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = "";
                if (review.ReviewId > 0)
                {
                    sql += $@"
                    UPDATE Reviews SET
                    Rating='{review.Rating}', Comment='{review.Comment}', ProductId='{review.ProductId}', Updated_At=GetUtcDate(), UserId='{userid}', OrderId='{review.OrderId}', ReviewPhotos = '{review.ReviewPhotos}'
                    WHERE ReviewId = {review.ReviewId};
                    ";
                }
                else
                {
                    sql += $@"
                    INSERT INTO Reviews
                    (Rating, Comment, ProductId, Created_At, UserId, Status, Viewed, OrderId, ReviewPhotos)
                    SELECT
                    '{review.Rating}', '{review.Comment}', '{review.ProductId}', GetUtcDate(), '{userid}', 1, 0, '{review.OrderId}', '{review.ReviewPhotos}';
                    ";
                }

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

        public async Task<dynamic> GetShippingCostAsync(string userid, string cartIds, string cityId)
        {
            try
            {
                string sql = $@"
                DECLARE 
                @UserId VARCHAR(100) = '{userid}',
                @CartIds VARCHAR(100) = '{cartIds}',
                @CityId VARCHAR(100) = '{cityId}'

                DECLARE @ShippingCost DECIMAL(18, 2)
                DECLARE @GlobalShippingType VARCHAR(100) = (Select ISNULL(Value, '') from BusinessSettings WHERE [Type] = 'shipping_type')
                IF (@GlobalShippingType = 'flat_rate')
                BEGIN
	                SET @ShippingCost = CAST((Select ISNULL(Value, 0) from BusinessSettings WHERE [Type] = 'flat_rate_shipping_cost') AS decimal)
                END
                ELSE IF (@GlobalShippingType = 'area_wise_shipping')
                BEGIN
	                IF EXISTS(SELECT TOP 1 CitiesId FROM Cities WHERE CitiesId = @CityId)
	                BEGIN
		                SET @ShippingCost = (SELECT TOP 1 ISNULL(Cost, 0) FROM Cities WHERE CitiesId = @CityId)
	                END
	                ELSE
	                BEGIN
		                SET @ShippingCost = 0
	                END
                END
                ELSE IF (@GlobalShippingType = 'product_wise_shipping')
                BEGIN
	                SET @ShippingCost = (SELECT SUM(ISNULL(C.ShippingCost, 0))
	                FROM Carts C
	                INNER JOIN Products P ON P.ProductId = C.ProductId
	                WHERE C.Quantity > 0 AND C.UserId = @UserId AND C.CartId IN (SELECT s.value from string_split(@CartIds,',') s))
                END
                ELSE
                BEGIN
	                SET @ShippingCost = 0
                END
                SELECT @ShippingCost ShippingCost;
                ";

                try
                {
                    await _connection.OpenAsync();
                    var records = await _connection.QueryAsync(sql);
                    return records.AsList().FirstOrDefault();
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

        public async Task<decimal> GetShippingCostAsync(int productId = 0, string cityId = "0")
        {
            try
            {
                string sql = $@"
                DECLARE 
                @ProductId INT = '{productId}',
                @CityId VARCHAR(100) = '{cityId}'

                DECLARE @ShippingCost DECIMAL(18, 2)
                DECLARE @GlobalShippingType VARCHAR(100) = (Select ISNULL(Value, '') from BusinessSettings WHERE [Type] = 'shipping_type')
                IF (@GlobalShippingType = 'flat_rate')
                BEGIN
                    SET @ShippingCost = CAST((Select ISNULL(Value, 0) from BusinessSettings WHERE [Type] = 'flat_rate_shipping_cost') AS decimal)
                END
                ELSE IF (@GlobalShippingType = 'area_wise_shipping')
                BEGIN
                    IF EXISTS(SELECT TOP 1 CitiesId FROM Cities WHERE CitiesId = @CityId)
                    BEGIN
                        SET @ShippingCost = (SELECT TOP 1 ISNULL(Cost, 0) FROM Cities WHERE CitiesId = @CityId)
                    END
                    ELSE
                    BEGIN
                        SET @ShippingCost = 0
                    END
                END
                ELSE IF (@GlobalShippingType = 'product_wise_shipping')
                BEGIN
                    SET @ShippingCost = (SELECT 
	                 SUM(CAST(ISNULL(P.ShippingCost, 0) AS DECIMAL))
                    FROM Products P
                    WHERE P.ProductId = @productId
	                )
                END
                ELSE
                BEGIN
                    SET @ShippingCost = 0
                END
                SELECT @ShippingCost ShippingCost;
                ";

                try
                {
                    await _connection.OpenAsync();
                    decimal shippingCost = await _connection.QuerySingleAsync<decimal>(sql);
                    return shippingCost;
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

        public async Task<bool> PaidOrderStatusAsync(int orderId)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                DECLARE 
                @PaymentType VARCHAR(100),
                @PaymentStatus VARCHAR(100),
                @DeliveryStatus VARCHAR(100)

                Select @PaymentType = O.PaymentType, @PaymentStatus = O.PaymentStatus, @DeliveryStatus=O.DeliveryStatus from Orders O WHERE O.OrdersId = {orderId}

                IF(@PaymentType = 'Stripe' OR @PaymentType = 'Paypal') 
                BEGIN
	                UPDATE Orders SET PaymentStatus = 'Paid', DeliveryStatus = 'Pending' WHERE OrdersId = {orderId}
                END";

                await _connection.ExecuteAsync(sql, new { }, transaction);
                transaction.Commit();

                return true;
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

        public async Task<bool> MakeOrderCODAsync(int orderId, string userId)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                DECLARE 
                @PaymentType VARCHAR(100),
                @PaymentStatus VARCHAR(100),
                @DeliveryStatus VARCHAR(100),
                @Code VARCHAR(100)

                Select @Code = O.Code, @PaymentType = O.PaymentType, @PaymentStatus = O.PaymentStatus, @DeliveryStatus=O.DeliveryStatus from Orders O WHERE O.OrdersId = {orderId}

                IF(@PaymentType = 'Stripe' OR @PaymentType = 'Paypal') 
                BEGIN
	                UPDATE Orders SET PaymentType = 'Cash On Delivery', DeliveryStatus = 'Pending' WHERE OrdersId = {orderId}
                END

                INSERT INTO OrderStatusHistory (OrderId, UserId, CurrentStatus, PreviousStatus, ChangedDate)
			    SELECT {orderId}, '{userId}', 'Pending', '', GETUTCDATE()

                INSERT INTO [dbo].OrderNotification (OrdersId, NotificationMessage, NotificationDate,IsView)
			    SELECT {orderId}, 'An order '+ @Code +' placed', GETUTCDATE(), 0
                ";

                await _connection.ExecuteAsync(sql, new { }, transaction);
                transaction.Commit();

                return true;
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

        public async Task<dynamic> CreateOrderV2Async(string userId, string productStocks, string address, int couponId, int paymentType, int pickupId, int shippingLocation)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                var result = await _connection.QueryAsync("SP_CreateOrderV2", new { UserId = userId, ProductStocks = productStocks, Address = address, CouponId = couponId, PaymentType = paymentType, PickupPointId = pickupId, ShippingLocation = shippingLocation }, transaction, 30, CommandType.StoredProcedure);

                transaction.Commit();

                return result.AsList().FirstOrDefault(); ;
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

        //public async Task SaveVoucherAsync(Collection entity, int ledgerId, SqlTransaction transaction)
        //{
        //    AccountVoucher accountVoucher = new AccountVoucher()
        //    {
        //        AccouVoucherTypeAutoID = (int)AccountVoucherType.RECEIEVED,
        //        VoucherNumber = entity.InvoiceNoCollection,
        //        VoucherDate = DateTime.Now,
        //        BranchId = entity.BranchId,
        //        CustomerId = entity.CustomerId,
        //        IsActive = true,
        //        AccountType = 3, //Customer
        //        AccountLedgerId = 7, // Current Asset
        //        Created_At = DateTime.Now,
        //        Created_By = entity.Created_By,
        //        EntityState = EntityState.Added,
        //        IpAddress = Common.GetIpAddress()
        //    };

        //    var accountVoucherId = await _service.SaveSingleAsync<AccountVoucher>(accountVoucher, transaction);

        //    accountVoucher.AccountVoucherDetails.Add(new AccountVoucherDetails
        //    {
        //        AccountVoucherId = accountVoucherId,
        //        ChildId = accountVoucher.AccountLedgerId,
        //        CreditAmount = (decimal)entity.CollectionAmount,
        //        TypeId = AmountType.DEBIT_AMOUNT,
        //        IsActive = true,
        //        VoucherDate = DateTime.Now,
        //        BranchId = entity.BranchId,
        //        Created_At = DateTime.Now,
        //        Created_By = entity.Created_By,
        //        EntityState = EntityState.Added
        //    });

        //    accountVoucher.AccountVoucherDetails.Add(new AccountVoucherDetails
        //    {
        //        AccountVoucherId = accountVoucherId,
        //        ChildId = ledgerId,
        //        DebitAmount = (decimal)entity.CollectionAmount,
        //        TypeId = AmountType.CREDIT_AMOUNT,
        //        IsActive = true,
        //        VoucherDate = DateTime.Now,
        //        BranchId = entity.BranchId,
        //        Created_At = DateTime.Now,
        //        Created_By = entity.Created_By,
        //        EntityState = EntityState.Added
        //    });

        //    await _service.SaveAsync<AccountVoucherDetails>(accountVoucher.AccountVoucherDetails, transaction);
        //}

        //public async Task<int> GetLedgerIdByOperationalUser(int id)
        //{
        //    var query = $@"SELECT ISNULL(Id, 0) Id FROM AccountLedger WHERE RelatedId = {id}";
        //    return await _service.GetSingleIntFieldAsync(query);
        //}

        //public async Task DeleteVoucher(string voucherNumber, SqlTransaction transaction)
        //{
        //    var query = $@"
        //        DELETE FROM AccountVoucherDetails WHERE AccountVoucherId = (SELECT AccountVoucherId FROM AccountVoucher WHERE VoucherNumber = (SELECT InvoiceNoCollection FROM Collection WHERE InvoiceNoCollection = @voucherNumber));
        //        DELETE FROM AccountVoucher WHERE VoucherNumber = (SELECT InvoiceNoCollection FROM Collection WHERE InvoiceNoCollection = @voucherNumber)
        //        ";
        //    await _service.ExecuteQueryAsync(query, new { voucherNumber }, transaction);
        //}

    }
}