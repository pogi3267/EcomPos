using ApplicationCore.Entities.Orders;
using ApplicationCore.Entities.Products;
using Dapper;
using Infrastructure.Interfaces.UI;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.UI
{
    public class CartService : ICartService
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public CartService(IConfiguration configuration)
        {
            _configuration = configuration;
            var conStr = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(conStr);
        }

        public async Task<int> AddToCartAsync(Carts cart)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                IF EXISTS (SELECT 1 FROM Carts WHERE UserId = '{cart.UserId}' AND ProductId = {cart.ProductId} AND Variation = '{cart.Variation}' AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsBuy, 0) = 0)
                BEGIN
                    UPDATE Carts SET Variation = '{cart.Variation}', Price = '{cart.Price}', Tax = '{cart.Tax}', Discount = '{cart.Discount}',
                    Quantity = Quantity+{cart.Quantity}, Updated_At = GetUtcDate(), Updated_By='{cart.UserId}',
                    ShippingType = '{cart.ShippingType}', ShippingCost = ISNULL(ShippingCost, 0) + {cart.ShippingCost}, PickupPoint = '{cart.PickupPoint}', 
                    ProductReferralCode = '{cart.ProductReferralCode}', CouponCode = '{cart.CouponCode}', CouponApplied = '{cart.CouponApplied}'
                    WHERE UserId = '{cart.UserId}' AND ProductId = '{cart.ProductId}' AND Variation = '{cart.Variation}'  AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsBuy, 0) = 0
                END
                ELSE
                BEGIN
                    INSERT INTO Carts
                    (OwnerId, UserId, ProductId, Variation, VariationId, Price, Tax, Discount, Quantity, Created_At, Created_By, ShippingType, ShippingCost, PickupPoint, ProductReferralCode, CouponCode, CouponApplied)
                    SELECT
                    '{cart.OwnerId}', '{cart.UserId}', '{cart.ProductId}', '{cart.Variation}', {cart.VariationId}, '{cart.Price}', '{cart.Tax}', '{cart.Discount}', {cart.Quantity}, GetUtcDate(), '{cart.UserId}', '{cart.ShippingType}','{cart.ShippingCost}', '{cart.PickupPoint}', '{cart.ProductReferralCode}', '{cart.CouponCode}', '{cart.CouponApplied}'
                END
                ";

                var id = await _connection.ExecuteAsync(sql, new { }, transaction);
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

        public async Task<int> RemoveFromCartAsync(Carts cart)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                UPDATE Carts SET Quantity = Quantity - {cart.Quantity} WHERE UserId = '{cart.UserId}' AND ProductId = '{cart.ProductId}' AND Variation = '{cart.Variant}'  AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsBuy, 0) = 0;
                ";

                var id = await _connection.ExecuteAsync(sql, new { }, transaction);
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

        public async Task<dynamic> GetCartCountAsync(string userId)
        {
            string sql = $@"
                SELECT COUNT(CartId) NoOfCart FROM Carts WHERE Quantity > 0 AND UserId = '{userId}' AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsBuy, 0) = 0;
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

        public async Task<List<dynamic>> GetCartListAsync(string userId)
        {
            try
            {
                string sql = $@"
                SELECT C.CartId, C.OwnerId, C.UserId, C.TempUserId, C.AddressId, C.ProductId, C.Variation Variant, C.VariationId, ISNULL(C.Price, 0) Price, ISNULL(C.Tax, 0) Tax, ISNULL(C.ShippingCost, 0) TotalShippingCost,
                C.ShippingType, C.PickupPoint, C.Discount, C.ProductReferralCode, C.CouponCode, C.CouponApplied, ISNULL(C.Quantity, 0) Quantity, (ISNULL(C.Price, 0) * ISNULL(C.Quantity, 0)) TotalPrice,
                ISNULL(S.SellerId, 0) SellerId, CONCAT(SU.FirstName, ' ', SU.LastName) SellerName,
                P.Name ProductName, P.ThumbnailImage, (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                P.MinQuantity, P.UnitPrice,
                (SELECT Quantity FROM ProductStocks WHERE ProductId = P.ProductId AND Variant = C.Variation) MaxQuantity,
                (ISNULL(C.Tax, 0) * ISNULL(C.Quantity, 0)) TotalTax, (ISNULL(C.Discount, 0) * ISNULL(C.Quantity, 0)) TotalDiscount
                FROM Carts C
                INNER JOIN Products P ON P.ProductId = C.ProductId
                LEFT JOIN Sellers S ON S.UserId = P.UserId
                LEFT JOIN AspNetUsers SU ON SU.Id = S.UserId
                WHERE C.Quantity > 0 AND C.UserId = '{userId}' AND ISNULL(C.IsDeleted, 0) = 0 AND ISNULL(C.IsBuy, 0) = 0
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

        public async Task<List<dynamic>> GetCartListByIdsAsync(string userId, string cartIds)
        {
            try
            {
                string sql = $@"
                SELECT C.CartId, C.OwnerId, C.UserId, C.TempUserId, C.AddressId, C.ProductId, C.Variation Variant, C.VariationId, ISNULL(C.Price, 0) Price, ISNULL(C.Tax, 0) Tax, ISNULL(C.ShippingCost, 0) ShippingCost,
                C.ShippingType, C.PickupPoint, C.Discount, C.ProductReferralCode, C.CouponCode, C.CouponApplied, ISNULL(C.Quantity, 0) Quantity, (ISNULL(C.Price, 0) * ISNULL(C.Quantity, 0)) TotalPrice,
                ISNULL(S.SellerId, 0) SellerId, CONCAT(SU.FirstName, ' ', SU.LastName) SellerName,
                P.Name ProductName, P.ThumbnailImage, (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                P.MinQuantity,
                (SELECT Quantity FROM ProductStocks WHERE ProductId = P.ProductId AND Variant = C.Variation) MaxQuantity
                FROM Carts C
                INNER JOIN Products P ON P.ProductId = C.ProductId
                LEFT JOIN Sellers S ON S.UserId = P.UserId
                LEFT JOIN AspNetUsers SU ON SU.Id = S.UserId
                WHERE C.Quantity > 0 AND C.UserId = '{userId}' AND ISNULL(C.IsDeleted, 0) = 0 AND ISNULL(C.IsBuy, 0) = 0 AND C.CartId IN (SELECT s.value from string_split('{cartIds}',',') s)
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

        public async Task<int> GetCartQuantityByProductAsync(string userId, int productId, string variant)
        {
            string sql = $@"
                SELECT TOP 1 Quantity FROM Carts WHERE Quantity > 0 AND ProductId = {productId} AND Variation = '{variant}' AND UserId = '{userId}' AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsBuy, 0) = 0;
                ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync(sql);
                var row = (IDictionary<string, object>)records.AsList().FirstOrDefault();
                int quantity = 0;
                if (row != null)
                {
                    if (row.ContainsKey("Quantity"))
                    {
                        quantity = Convert.ToInt32(row["Quantity"]);
                    }
                }
                return quantity;
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

        public async Task<int> AddToWishlistAsync(Product entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                IF NOT EXISTS (SELECT 1 FROM WishLists WHERE UserId = '{entity.UserId}' AND ProductId = '{entity.ProductId}')
                BEGIN
                    INSERT INTO WishLists
                    (UserId, ProductId, Created_At, Created_By)
                    SELECT
                    '
                {entity.UserId}', '{entity.ProductId}', GetUtcDate(), '{entity.UserId}'
                END

                ";

                var id = await _connection.ExecuteAsync(sql, new { }, transaction);
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

        public async Task<int> AddToWishlistAsync(int productId, string userId)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                IF NOT EXISTS (SELECT 1 FROM WishLists WHERE UserId = '{userId}' AND ProductId = {productId})
                BEGIN
                    INSERT INTO WishLists
                    (UserId, ProductId, Created_At, Created_By)
                    SELECT
                    '{userId}', '{productId}', GetUtcDate(), '{userId}'
                END
                ";

                var id = await _connection.ExecuteAsync(sql, new { }, transaction);
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

        public async Task<int> RemoveFromWishlistAsync(int productId, string userId)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                DELETE FROM WishLists WHERE UserId = '{userId}' AND ProductId = {productId};
                ";

                var id = await _connection.ExecuteAsync(sql, new { }, transaction);
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

        public async Task<dynamic> GetWishListCountAsync(string userId)
        {
            string sql = $@"
                SELECT COUNT(W.WishListId) NoOfWishList FROM WishLists W WHERE UserId = '{userId}';
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

        public async Task<List<dynamic>> GetWishListAsync(string userId)
        {
            try
            {
                string sql = $@"
                SELECT W.WishListId, W.UserId, W.ProductId,
                P.Name ProductName, P.UnitPrice Price, P.ThumbnailImage, (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage
                FROM WishLists W
                INNER JOIN Products P ON P.ProductId = W.ProductId
                WHERE W.UserId = '{userId}'
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

        public async Task<dynamic> GetCouponByCodeAsync(string userId, string promo)
        {
            string sql = $@"
                Select TOP 1 C.CouponId, C.Type, C.Code, C.Details, C.Discount, C.DiscountType
                from Coupons C
				LEFT JOIN CouponUsages CU ON CU.CouponId = C.CouponId AND CU.UserId = '{userId}'
                WHERE C.Code = '{promo}' AND CU.CouponId IS NULL
                AND GetUtcDate() >= dbo.GetLocalDate(C.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(C.EndDate) AND ISNULL(C.IsActive, 0) = 1
                ORDER BY C.CouponId ASC;
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

        public async Task<dynamic> GetCouponDetailsAsync(string userId, int couponId, decimal amount)
        {
            string sql = $@"
                Select TOP 1 C.CouponId, C.Type, C.Code, C.Details, C.Discount, C.DiscountType,
                DiscountAmount = CASE WHEN C.DiscountType = 'percent' THEN ({amount} * C.Discount) / 100 WHEN C.DiscountType = 'amount' THEN C.Discount ELSE 0 END
                from Coupons C
                LEFT JOIN CouponUsages CU ON CU.CouponId = C.CouponId AND CU.UserId = '{userId}'
                WHERE C.CouponId = {couponId} AND CU.CouponId IS NULL
                AND GetUtcDate() >= dbo.GetLocalDate(C.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(C.EndDate) AND ISNULL(C.IsActive, 0) = 1
                ORDER BY C.CouponId ASC;;
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

        public async Task<int> BuyProductAsync(Carts cart)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                UPDATE Carts SET IsDeleted = 1 WHERE UserId = '{cart.UserId}' AND ISNULL(IsBuy, 0) = 1 AND ISNULL(IsDeleted, 0) = 0;

                INSERT INTO Carts
                (OwnerId, UserId, ProductId, Variation, VariationId, Price, Tax, Discount, Quantity, Created_At, Created_By, ShippingType, 
                ShippingCost, PickupPoint, ProductReferralCode, CouponCode, CouponApplied, IsBuy)
                SELECT
                '{cart.OwnerId}', '{cart.UserId}', '{cart.ProductId}', '{cart.Variation}', {cart.VariationId}, '{cart.Price}', '{cart.Tax}', '{cart.Discount}', 
                {cart.Quantity}, GetUtcDate(), '{cart.UserId}', '{cart.ShippingType}','{cart.ShippingCost}', '{cart.PickupPoint}', '{cart.ProductReferralCode}', 
                '{cart.CouponCode}', '{cart.CouponApplied}', 1
                ";

                var id = await _connection.ExecuteAsync(sql, new { }, transaction);
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

        public async Task<List<dynamic>> GetBuyProductListAsync(string userId)
        {
            try
            {
                string sql = $@"
                SELECT TOP 1 C.CartId, C.OwnerId, C.UserId, C.TempUserId, C.AddressId, C.ProductId, C.Variation Variant, C.VariationId, ISNULL(C.Price, 0) Price, ISNULL(C.Tax, 0) Tax, ISNULL(C.ShippingCost, 0) TotalShippingCost,
                C.ShippingType, C.PickupPoint, C.Discount, C.ProductReferralCode, C.CouponCode, C.CouponApplied, ISNULL(C.Quantity, 0) Quantity, (ISNULL(C.Price, 0) * ISNULL(C.Quantity, 0)) TotalPrice,
                ISNULL(S.SellerId, 0) SellerId, CONCAT(SU.FirstName, ' ', SU.LastName) SellerName,
                P.Name ProductName, P.ThumbnailImage, (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                P.MinQuantity, P.UnitPrice,
                (SELECT Quantity FROM ProductStocks WHERE ProductId = P.ProductId AND Variant = C.Variation) MaxQuantity,
                (ISNULL(C.Tax, 0) * ISNULL(C.Quantity, 0)) TotalTax, (ISNULL(C.Discount, 0) * ISNULL(C.Quantity, 0)) TotalDiscount
                FROM Carts C
                INNER JOIN Products P ON P.ProductId = C.ProductId
                LEFT JOIN Sellers S ON S.UserId = P.UserId
                LEFT JOIN AspNetUsers SU ON SU.Id = S.UserId
                WHERE C.Quantity > 0 AND C.UserId = '{userId}' AND ISNULL(C.IsDeleted, 0) = 0 AND ISNULL(C.IsBuy, 0) = 1
                ORDER BY C.CartId DESC
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

    }
}