using ApplicationCore.DTOs;
using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using Dapper;
using Infrastructure.Interfaces.Public;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Public
{
    public class GeneralService : IGeneralService
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;
        public GeneralService(IConfiguration configuration)
        {
            _configuration = configuration;
            var conStr = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(conStr);
        }

        public async Task<dynamic> GetSystemSettingAsync()
        {
            string sql = $@"
                SELECT
                (Select [Value] from BusinessSettings WHERE Type = 'system_default_currency') Currency,
                (Select [Value] from BusinessSettings WHERE Type = 'currency_position') CurrencyPosition,
                (Select [Value] from BusinessSettings WHERE Type = 'amount_format') AmountFormat,
                (Select [Value] from BusinessSettings WHERE Type = 'home_page_config') HomePageConfig,
                (Select [Value] from BusinessSettings WHERE Type = 'maintenance_mode') MaintenanceMode,
                (Select CAST([Value] AS DATETIME) from BusinessSettings WHERE Type = 'maintenance_time') MaintenanceTime,
                (Select [Value] from BusinessSettings WHERE Type = 'free_shipping_note') ShippingNote,
                (Select [Value] from BusinessSettings WHERE Type = 'secure_payment_note') SecurePaymentNote,
                (Select [Value] from BusinessSettings WHERE Type = 'money_return_note') MoneyReturnNote,
                (Select [Value] from BusinessSettings WHERE Type = 'support_note') SupportNote,
                (Select [Value] from BusinessSettings WHERE Type = 'delivery_note') DeliveryNote,
                SystemName,  SystemLogoWhite, SystemLogoBlack, Titel AboutUsTitle, AboutUs AboutUsDescription,
                (Select [Value] from BusinessSettings WHERE Type = 'site_carousel_images') HeaderImages,
                (SELECT value from BusinessSettings WHERE Type = 'home_banner1_image') Banner1Image,
                (SELECT value from BusinessSettings WHERE Type = 'home_banner2_image') Banner2Image,
                (SELECT value from BusinessSettings WHERE Type = 'home_banner3_image') Banner3Image
                FROM GeneralSettings";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<dynamic>(sql);
                return records.ToList().FirstOrDefault();
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

        public async Task<dynamic> GetOrderSettingAsync()
        {
            string sql = $@"
                SELECT
                (Select [Value] from BusinessSettings WHERE Type = 'address_config') AddressConfig,
                (Select [Value] from BusinessSettings WHERE Type = 'payment_mode') PaymentMode,
                (Select [Value] from BusinessSettings WHERE Type = 'inside_shipping_location') InsideShippingLocation,
                (Select [Value] from BusinessSettings WHERE Type = 'inside_shipping_cost') InsideShippingCost,
                (Select [Value] from BusinessSettings WHERE Type = 'outside_shipping_location') OutsideShippingLocation,
                (Select [Value] from BusinessSettings WHERE Type = 'outside_shipping_cost') OutsideShippingCost";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<dynamic>(sql);
                return records.ToList().FirstOrDefault();
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

        public async Task<dynamic> GetTermsAndPrivacyAsync()
        {
            string sql = $@"
                SELECT
                (Select [Value] from BusinessSettings WHERE Type = 'terms_condition') TermsCondition,
                (Select [Value] from BusinessSettings WHERE Type = 'privacy_policy') PrivacyPolicy";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<dynamic>(sql);
                return records.ToList().FirstOrDefault();
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

        public async Task<List<Category>> GetCategoriesAsync()
        {
            string sql = $@"
                select ct.CategoryId, ISNULL(ct.ParentId, 0) ParentId, ct.Name, pa.Name ParentName, ct.Banner
                from Categories ct
                left join Categories pa on pa.CategoryId = ct.ParentId
                ORDER BY ct.CategoryId ASC";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<Category>(sql);
                return records.ToList();
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

        public async Task<List<dynamic>> GetBrandsAsync()
        {
            string sql = $@"
                select B.BrandId, B.Name BrandName
                from Brands B
                ORDER BY B.Name";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<dynamic>(sql);
                return records.ToList();
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

        public async Task<List<Category>> GetCategoryListAsync()
        {
            string sql = $@"
                WITH P AS (
                select ct.CategoryId
                from Categories ct 
                WHERE Featured=1
                )
                select ct.CategoryId, ISNULL(ct.ParentId, 0) ParentId, ct.Name, ct.Banner
                from Categories ct
                INNER JOIN P ON p.CategoryId = ct.CategoryId
                ORDER BY ct.CategoryId ASC";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<Category>(sql);
                return records.ToList();
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

        public async Task<List<dynamic>> GetProductStockListByProductIdsAsync(string productIds)
        {
            string sql = $@"
                SELECT PS.ProductStockId, PS.Variant, PS.SKU, PS.Price, PS.Image, PS.ProductId, PS.PurchaseQty, PS.Quantity
                FROM ProductStocks PS WHERE PS.ProductId IN (SELECT value FROM String_Split('{productIds}',','))";
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

        public async Task<List<dynamic>> GetAttributesAsync()
        {
            string sql = $@"
                Select AttributeId, Name from Attributes;";
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

        public async Task<List<dynamic>> GetFlashDealProductsAsync(int id, int noOfRows, string userId)
        {
            try
            {
                string sql = $@"
                WITH WISH AS (
	                SELECT ProductId, 1 IsWish
	                From WishLists
	                WHERE UserId = '{userId}'
                )
                Select Top {noOfRows} P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * FDP.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'amount' THEN (P.UnitPrice - FDP.Discount) 
                ELSE P.UnitPrice 
                END,
                FDP.FlashDealId, FDP.Discount FlashDealDiscount, FDP.DiscountType FlashDealDiscountType,
                FD.Title FlashDealTitle, dbo.GetLocalDate(FD.StartDate) FlashDealStartDate, dbo.GetLocalDate(FD.EndDate) FlashDealEndDate, FD.TextColor FlashDealTextColor, 
                FD.BackgroundColor FlashDealBackgroundColor, FD.Banner FlashDealBanner,
                C.Name CategoryName, B.Name BrandName,
                (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews,
                (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode
                ,ISNULL(W.IsWish, 0) IsWish
                from Products P
                INNER JOIN FlashDealProducts FDP ON FDP.ProductId = P.ProductId AND P.Published = 1
                INNER JOIN FlashDeals FD ON FD.FlashDealId = FDP.FlashDealId AND FD.Status = 1 AND FD.FlashDealId = {id} AND GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate)
                LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                LEFT JOIN Brands B ON B.BrandId = P.BrandId
                LEFT JOIN WISH W ON W.ProductId = P.ProductId
                ORDER BY P.ProductId DESC";

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

        public async Task<List<dynamic>> GetTodaysDealProductsAsync(int noOfRows, string userId)
        {
            try
            {
                string sql = $@"
                WITH PRODS AS (
                Select Top {noOfRows} P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU, P.IsTrend,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                ELSE P.UnitPrice 
                END,
                C.Name CategoryName, B.Name BrandName,
                (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews,
                (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode
                from Products P
                LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                LEFT JOIN Brands B ON B.BrandId = P.BrandId
                WHERE P.TodaysDeal = 1 AND P.Published = 1
                ORDER BY P.ProductId DESC
                ),
                FD_PRODS AS (
                SELECT P.ProductId,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * FDP.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'amount' THEN (P.UnitPrice - FDP.Discount) 
                ELSE P.UnitPrice 
                END
                FROM FlashDeals FD
                INNER JOIN FlashDealProducts FDP ON FDP.FlashDealId = FD.FlashDealId
                INNER JOIN PRODS P ON P.ProductId = FDP.ProductId
                WHERE FD.[Status] = 1 AND GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate)
                ),
                WISH AS (
	                SELECT W.ProductId, 1 IsWish
	                From WishLists W
	                INNER JOIN PRODS ON PRODS.ProductId = W.ProductId
	                WHERE UserId = '{userId}'
                )
                SELECT P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, P.Tags, P.[Description], 
                P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, P.TodaysDeal, P.Published, 
                P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, P.MinQuantity, P.LowStaockQuantity, 
                P.Discount, P.DiscountType, P.DiscountStartDate, P.DiscountEndDate, P.Tax, P.TaxType, P.ShippingType, P.ShippingCost, 
                P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, P.MetaImage, P.PDF, P.Slug, P.Rating, 
                P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, 
                P.ProductSKU, P.IsTrend, ISNULL(FDP.DiscountPrice, P.DiscountPrice) DiscountPrice, P.CategoryName, P.BrandName, P.ProductImage, 
                P.ProductRating, P.NoOfReviews, P.ColorNameWithCode, ISNULL(W.IsWish, 0) IsWish
                FROM PRODS P
                LEFT JOIN FD_PRODS FDP ON FDP.ProductId = P.ProductId
                LEFT JOIN WISH W ON W.ProductId = P.ProductId
                ORDER BY P.ProductId DESC";

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

        public async Task<List<dynamic>> GetTrendingProductsAsync(int noOfRows, string userId)
        {
            try
            {
                string sql = $@"
                WITH PRODS AS (
                Select Top {noOfRows} P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU, P.IsTrend,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                ELSE P.UnitPrice 
                END,
                C.Name CategoryName, B.Name BrandName,
                (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews,
                (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode
                from Products P
                LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                LEFT JOIN Brands B ON B.BrandId = P.BrandId
                WHERE P.IsTrend = 1 AND P.Published = 1
                ORDER BY P.ProductId DESC
                ),
                FD_PRODS AS (
                SELECT P.ProductId,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * FDP.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'amount' THEN (P.UnitPrice - FDP.Discount) 
                ELSE P.UnitPrice 
                END
                FROM FlashDeals FD
                INNER JOIN FlashDealProducts FDP ON FDP.FlashDealId = FD.FlashDealId
                INNER JOIN PRODS P ON P.ProductId = FDP.ProductId
                WHERE FD.[Status] = 1 AND GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate)
                ),
                WISH AS (
	                SELECT W.ProductId, 1 IsWish
	                From WishLists W
	                INNER JOIN PRODS ON PRODS.ProductId = W.ProductId
	                WHERE UserId = '{userId}'
                )
                SELECT P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, P.Tags, P.[Description], 
                P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, P.TodaysDeal, P.Published, 
                P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, P.MinQuantity, P.LowStaockQuantity, 
                P.Discount, P.DiscountType, P.DiscountStartDate, P.DiscountEndDate, P.Tax, P.TaxType, P.ShippingType, P.ShippingCost, 
                P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, P.MetaImage, P.PDF, P.Slug, P.Rating, 
                P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, 
                P.ProductSKU, P.IsTrend, ISNULL(FDP.DiscountPrice, P.DiscountPrice) DiscountPrice, P.CategoryName, P.BrandName, P.ProductImage, 
                P.ProductRating, P.NoOfReviews, P.ColorNameWithCode, ISNULL(W.IsWish, 0) IsWish
                FROM PRODS P
                LEFT JOIN FD_PRODS FDP ON FDP.ProductId = P.ProductId
                LEFT JOIN WISH W ON W.ProductId = P.ProductId
                ORDER BY P.ProductId DESC";

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

        public async Task<List<dynamic>> GetDiscountProductsAsync(int noOfRows, string userId)
        {
            try
            {
                string sql = $@"
                WITH FD_PRODS AS (
                SELECT P.ProductId,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * FDP.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'amount' THEN (P.UnitPrice - FDP.Discount) 
                ELSE P.UnitPrice 
                END
                FROM FlashDeals FD
                INNER JOIN FlashDealProducts FDP ON FDP.FlashDealId = FD.FlashDealId
                INNER JOIN Products P ON P.ProductId = FDP.ProductId AND P.Published = 1
                WHERE FD.[Status] = 1 AND GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate)
                ),
                WISH AS (
	                SELECT ProductId, 1 IsWish
	                From WishLists
	                WHERE UserId = '{userId}'
                )
                Select Top {noOfRows} P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU, P.IsTrend,
                DiscountPrice = ISNULL(FDP.DiscountPrice, CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                ELSE P.UnitPrice 
                END),
                C.Name CategoryName, B.Name BrandName,
                (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews,
                (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode
                ,ISNULL(W.IsWish, 0) IsWish
                from Products P
                LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                LEFT JOIN Brands B ON B.BrandId = P.BrandId
                LEFT JOIN FD_PRODS FDP ON FDP.ProductId = P.ProductId
                LEFT JOIN WISH W ON W.ProductId = P.ProductId
                WHERE --( ISNULL(P.DiscountType, '') != '' OR (GetUtcDate() BETWEEN dbo.GetLocalDate(P.DiscountStartDate) AND dbo.GetLocalDate(P.DiscountEndDate)) OR FDP.ProductId IS NOT NULL )
                --AND 
                ISNULL(FDP.DiscountPrice, CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                ELSE P.UnitPrice 
                END) < P.UnitPrice 
                AND P.Published = 1
                ORDER BY P.ProductId DESC";

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

        public async Task<List<dynamic>> GetFeaturedProductsAsync(int noOfRows, string userId)
        {
            try
            {
                string sql = $@"
                WITH PRODS AS (
                Select Top {noOfRows} P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU, P.IsTrend,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                ELSE P.UnitPrice 
                END,
                C.Name CategoryName, B.Name BrandName,
                (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews,
                (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode
                from Products P
                LEFT JOIN Categories C ON C.CategoryId = P.CategoryId 
                LEFT JOIN Brands B ON B.BrandId = P.BrandId
                WHERE P.Featured = 1 AND P.Published = 1
                ORDER BY P.ProductId DESC
                ),
                FD_PRODS AS (
                SELECT P.ProductId,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * FDP.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'amount' THEN (P.UnitPrice - FDP.Discount) 
                ELSE P.UnitPrice 
                END
                FROM FlashDeals FD
                INNER JOIN FlashDealProducts FDP ON FDP.FlashDealId = FD.FlashDealId
                INNER JOIN PRODS P ON P.ProductId = FDP.ProductId
                WHERE FD.[Status] = 1 AND GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate)
                ),
                WISH AS (
	                SELECT W.ProductId, 1 IsWish
	                From WishLists W
	                INNER JOIN PRODS ON PRODS.ProductId = W.ProductId
	                WHERE UserId = '{userId}'
                )
                SELECT P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, P.Tags, P.[Description], 
                P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, P.TodaysDeal, P.Published, 
                P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, P.MinQuantity, P.LowStaockQuantity, 
                P.Discount, P.DiscountType, P.DiscountStartDate, P.DiscountEndDate, P.Tax, P.TaxType, P.ShippingType, P.ShippingCost, 
                P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, P.MetaImage, P.PDF, P.Slug, P.Rating, 
                P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, 
                P.ProductSKU, P.IsTrend, ISNULL(FDP.DiscountPrice, P.DiscountPrice) DiscountPrice, P.CategoryName, P.BrandName, P.ProductImage, 
                P.ProductRating, P.NoOfReviews, P.ColorNameWithCode, ISNULL(W.IsWish, 0) IsWish
                FROM PRODS P
                LEFT JOIN FD_PRODS FDP ON FDP.ProductId = P.ProductId
                LEFT JOIN WISH W ON W.ProductId = P.ProductId
                ORDER BY P.ProductId DESC";

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

        public async Task<List<dynamic>> GetRelatedProductsAsync(int id, int noOfRows, string userId)
        {
            try
            {
                string sql = $@"
                WITH PRODS AS (
                Select Top {noOfRows} P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU, P.IsTrend,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                ELSE P.UnitPrice 
                END,
                C.Name CategoryName, B.Name BrandName,
                (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews,
                (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode
                from Products P
                LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                LEFT JOIN Brands B ON B.BrandId = P.BrandId
                WHERE P.ProductId != {id} AND P.Published = 1 AND C.CategoryId = (Select CategoryId FROM Products WHERE ProductId = {id})
                ORDER BY P.ProductId DESC
                ),
                FD_PRODS AS (
                SELECT P.ProductId,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * FDP.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'amount' THEN (P.UnitPrice - FDP.Discount) 
                ELSE P.UnitPrice 
                END
                FROM FlashDeals FD
                INNER JOIN FlashDealProducts FDP ON FDP.FlashDealId = FD.FlashDealId
                INNER JOIN PRODS P ON P.ProductId = FDP.ProductId
                WHERE FD.[Status] = 1 AND GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate)
                ),
                WISH AS (
	                SELECT W.ProductId, 1 IsWish
	                From WishLists W
	                INNER JOIN PRODS ON PRODS.ProductId = W.ProductId
	                WHERE UserId = '{userId}'
                )
                SELECT P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, P.Tags, P.[Description], 
                P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, P.TodaysDeal, P.Published, 
                P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, P.MinQuantity, P.LowStaockQuantity, 
                P.Discount, P.DiscountType, P.DiscountStartDate, P.DiscountEndDate, P.Tax, P.TaxType, P.ShippingType, P.ShippingCost, 
                P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, P.MetaImage, P.PDF, P.Slug, P.Rating, 
                P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, 
                P.ProductSKU, P.IsTrend, ISNULL(FDP.DiscountPrice, P.DiscountPrice) DiscountPrice, P.CategoryName, P.BrandName, P.ProductImage, 
                P.ProductRating, P.NoOfReviews, P.ColorNameWithCode, ISNULL(W.IsWish, 0) IsWish
                FROM PRODS P
                LEFT JOIN FD_PRODS FDP ON FDP.ProductId = P.ProductId
                LEFT JOIN WISH W ON W.ProductId = P.ProductId
                ORDER BY P.ProductId DESC";

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

        public async Task<List<dynamic>> GetCategoryWiseProductsAsync(int categoryId, int noOfRows, string userId)
        {
            try
            {
                string sql = $@"
                WITH CAT AS (
                SELECT TOP 1 * 
                FROM Categories WHERE CategoryId = {categoryId}
                ),
                CATS AS (
                SELECT * FROM Categories WHERE CategoryId = (SELECT CategoryId FROM CAT)
                UNION
                Select * FROM Categories WHERE ParentId = (SELECT CategoryId FROM CAT)
                UNION
                Select * FROM Categories WHERE ParentId IN (Select CategoryId FROM Categories WHERE ParentId = (SELECT CategoryId FROM CAT))
                ),
                PRODS AS ( 
                Select Top {noOfRows} P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                ELSE P.UnitPrice 
                END,
                C.Name CategoryName, C.Banner, B.Name BrandName,
                (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews,
                (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode
                ,CAT.CategoryId ParentCategoryId, CAT.[Name] ParentCategoryName, CAT.Banner ParentCategoryBanner
                from Products P
                INNER JOIN CATS C ON C.CategoryId = P.CategoryId AND P.Published = 1
                LEFT JOIN Brands B ON B.BrandId = P.BrandId
                CROSS JOIN CAT
                ORDER BY P.ProductId DESC
                ),
                FD_PRODS AS (
                SELECT P.ProductId,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * FDP.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'amount' THEN (P.UnitPrice - FDP.Discount) 
                ELSE P.UnitPrice 
                END
                FROM FlashDeals FD
                INNER JOIN FlashDealProducts FDP ON FDP.FlashDealId = FD.FlashDealId
                INNER JOIN PRODS P ON P.ProductId = FDP.ProductId
                WHERE FD.[Status] = 1 AND GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate)
                ),
                WISH AS (
	                SELECT W.ProductId, 1 IsWish
	                From WishLists W
	                INNER JOIN PRODS ON PRODS.ProductId = W.ProductId
	                WHERE UserId = '{userId}'
                )
                SELECT P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, P.Tags, P.[Description], 
                P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, P.TodaysDeal, P.Published, 
                P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, P.MinQuantity, P.LowStaockQuantity, 
                P.Discount, P.DiscountType, P.DiscountStartDate, P.DiscountEndDate, P.Tax, P.TaxType, P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, 
                P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, 
                P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU, 
                ISNULL(FDP.DiscountPrice, P.DiscountPrice) DiscountPrice, P.CategoryName, P.Banner, P.BrandName, P.ProductImage, P.ProductRating, 
                P.NoOfReviews, P.ColorNameWithCode, P.ParentCategoryId, P.ParentCategoryName, P.ParentCategoryBanner, ISNULL(W.IsWish, 0) IsWish
                FROM PRODS P
                LEFT JOIN FD_PRODS FDP ON FDP.ProductId = P.ProductId
                LEFT JOIN WISH W ON W.ProductId = P.ProductId
                ORDER BY P.ProductId DESC";

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

        public async Task<List<dynamic>> GetTopCategoryProductsAsync(bool isFirst)
        {
            try
            {
                string ordering = isFirst ? "ASC" : "DESC";
                string sql = $@"
                WITH CAT AS (
                SELECT TOP 1 * 
                FROM Categories WHERE Featured = 1 
                ORDER BY CategoryId {ordering}
                ),
                CATS AS (
                SELECT * FROM Categories WHERE CategoryId = (SELECT CategoryId FROM CAT)
                UNION
                Select * FROM Categories WHERE ParentId = (SELECT CategoryId FROM CAT)
                UNION
                Select * FROM Categories WHERE ParentId IN (Select CategoryId FROM Categories WHERE ParentId = (SELECT CategoryId FROM CAT))
                )
                Select Top 3 P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                ELSE P.UnitPrice 
                END,
                C.Name CategoryName, C.Banner, B.Name BrandName,
                (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews,
                (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode
                ,CAT.CategoryId ParentCategoryId, CAT.[Name] ParentCategoryName, CAT.Banner ParentCategoryBanner
                from Products P
                INNER JOIN CATS C ON C.CategoryId = P.CategoryId AND P.Published = 1
                LEFT JOIN Brands B ON B.BrandId = P.BrandId
                CROSS JOIN CAT
                ORDER BY P.ProductId DESC";

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

        public async Task<List<dynamic>> GetCustomerReviewsAsync()
        {
            try
            {
                string sql = $@"
                WITH REV AS (
	                Select UserId, MAX(ReviewId) ReviewId
	                from Reviews R
	                WHERE R.Rating > 2
	                GROUP BY UserId
                )
                Select ToP 10  R.ReviewId, R.Rating, R.Comment ReviewDescription, CONCAT(U.FirstName, ' ', U.LastName) CustomerName
                from Reviews R
                INNER JOIN REV ON REV.ReviewId = R.ReviewId
                INNER JOIN AspNetUsers U ON U.Id = R.UserId
                ORDER BY Rating DESC, ReviewId DESC";

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

        public async Task<int> SaveSpecialOfferEmailAsync(SpecialOfferEmails entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                var id = await _connection.ExecuteAsync("Insert into SpecialOfferEmails (Email) values (@Email)", new { entity.Email }, transaction);
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

        public async Task<dynamic> GetProductDetailsAsync(int productId, string userId)
        {
            try
            {
                string sql = $@"
                    WITH PROD AS (
                    Select P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                    P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                    P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                    P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                    P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                    P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                    P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU,
                    DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                    WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                    ELSE P.UnitPrice 
                    END,
                    C.Name CategoryName, B.Name BrandName,
                    (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                    ISNULL((SELECT STRING_AGG(u.FileName, ',') FROM Uploads u WHERE u.UploadId IN(Select s.value from string_split(p.Photos, ',') s)), '') ProductPhotos,
                    (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                    (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews,
                    --(REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')) ColorCode,
                    (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode,
                    S.SellerId, CONCAT(SU.FirstName, ' ', SU.LastName) SellerName, S.SellerType, S.Tags SellerTags, S.SellerImage, S.VerificationStatus IsVerified
                    from Products P
                    LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                    LEFT JOIN Brands B ON B.BrandId = P.BrandId
                    LEFT JOIN Sellers S ON S.UserId = P.UserId
                    LEFT JOIN AspNetUsers SU ON SU.Id = S.UserId
                    WHERE P.ProductId = {productId}
                    ),
                    FD_PRODS AS (
                    SELECT P.ProductId,
                    DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * FDP.Discount) / 100) 
                    WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'amount' THEN (P.UnitPrice - FDP.Discount) 
                    ELSE P.UnitPrice 
                    END
                    FROM FlashDeals FD
                    INNER JOIN FlashDealProducts FDP ON FDP.FlashDealId = FD.FlashDealId
                    INNER JOIN PROD P ON P.ProductId = FDP.ProductId
                    WHERE FD.[Status] = 1 AND GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate)
                    ORDER BY FDP.Id DESC OFFSET 0 ROWS
                    ),
                    WISH AS (
	                    SELECT W.ProductId, 1 IsWish
	                    From WishLists W
	                    INNER JOIN PROD ON PROD.ProductId = W.ProductId
	                    WHERE UserId = '{userId}'
                    )
                    SELECT TOP 1 P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, P.Tags, P.Description, 
                    P.UnitPrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, P.TodaysDeal, P.Published, P.Approved, 
                    P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, P.MinQuantity, P.LowStaockQuantity, P.Discount, 
                    P.DiscountType, P.DiscountStartDate, P.DiscountEndDate, P.Tax, P.TaxType, P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, 
                    P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, 
                    P.FileName, P.FilePath, P.ExternalLink, P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU, ISNULL(FDP.DiscountPrice, P.DiscountPrice) DiscountPrice, 
                    P.CategoryName, P.BrandName, P.ProductImage, P.ProductPhotos, P.ProductRating, P.NoOfReviews, P.ColorNameWithCode, P.SellerId, P.SellerName, P.SellerType, 
                    P.SellerTags, P.SellerImage, P.IsVerified, ISNULL(W.IsWish, 0) IsWish
                    FROM PROD P
                    LEFT JOIN FD_PRODS FDP ON FDP.ProductId = P.ProductId
                    LEFT JOIN WISH W ON W.ProductId = P.ProductId;

                    SELECT PS.ProductStockId, PS.Variant, PS.SKU, PS.Price, PS.Image, PS.ProductId, PS.PurchaseQty, PS.Quantity,
				    (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = PS.Image) VariantImage
				    FROM ProductStocks PS WHERE PS.ProductId = {productId};

                    WITH R AS (
                    SELECT * FROM Reviews WHERE ProductId = {productId}
                    )
                    SELECT S.value Rating, (SELECT COUNT(1) FROM R WHERE R.Rating = S.value) Count
                    FROM string_split('1,2,3,4,5', ',') S;

                    WITH HISTORY AS (
                    SELECT ReviewId,
                    COUNT(CASE WHEN [Action] = 1 THEN 1 END) NumOfLike,
                    COUNT(CASE WHEN [Action] = 2 THEN 1 END) NumOfDislike
                    FROM ReviewHistory
                    GROUP BY ReviewId
                    )
                    SELECT R.ReviewId, R.Rating, R.Comment, dbo.GetLocalDate(ISNULL(Updated_At, Created_At)) CommentDate, ISNULL(R.ReviewPhotos, '') ReviewPhotos,
                    CONCAT(U.FirstName, ' ', U.LastName) CustomerName, '' CustomerImage, ISNULL(H.NumOfLike, 0) NumOfLike, ISNULL(H.NumOfDislike, 0) NumOfDislike,
                    ISNULL(RH.[Action], 0) [Action]
                    FROM Reviews R
                    INNER JOIN AspNetUsers U ON U.Id = R.UserId
                    LEFT JOIN HISTORY H ON H.ReviewId = R.ReviewId
                    LEFT JOIN ReviewHistory RH ON RH.ReviewId = R.ReviewId AND RH.UserId = '{userId}'
                    WHERE R.[Status] = 1 AND ProductId = {productId};

                    Select AttributeId, Name from Attributes;
                    ";

                try
                {
                    await _connection.OpenAsync();

                    var queryResult = await _connection.QueryMultipleAsync(sql);
                    dynamic product = queryResult.Read().FirstOrDefault();
                    product.ProductStocks = queryResult.Read().ToList();
                    product.ReviewDetails = queryResult.Read().ToList();
                    product.Reviews = queryResult.Read().ToList(); //<List<Dictionary<string, string>>>
                    product.AttributeList = queryResult.Read().ToList();

                    return product;
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

        public async Task<List<dynamic>> GetFilterProductsAsync(ProductSearchDTO model, string userId)
        {
            try
            {
                string pagination_Sql = $@" OFFSET {(model.PageNumber - 1) * model.NumberOfRows} ROWS FETCH NEXT {model.NumberOfRows} ROWS ONLY ";

                string sortBy_Sql = " ORDER BY PD.ProductId DESC ";
                if (!string.IsNullOrEmpty(model.SortBy))
                {
                    if (model.SortBy == "price-asc") sortBy_Sql = " ORDER BY PD.DiscountPrice ASC ";
                    else if (model.SortBy == "price-desc") sortBy_Sql = " ORDER BY PD.DiscountPrice DESC ";
                    if (model.SortBy == "rating-desc") sortBy_Sql = " ORDER BY PD.ProductRating DESC ";
                }

                string where_Sql = "";
                if (!string.IsNullOrEmpty(model.Price))
                {
                    int fromPrice = Convert.ToInt32(model.Price.Split('|')[0]);
                    int toPrice = Convert.ToInt32(model.Price.Split('|')[1]);
                    where_Sql += $@" AND PD.DiscountPrice BETWEEN {fromPrice} AND {toPrice} ";
                }

                if (!string.IsNullOrEmpty(model.Rating))
                {
                    int fromRating = Convert.ToInt32(model.Rating.Split('|')[0]);
                    int toRating = Convert.ToInt32(model.Rating.Split('|')[1]);
                    where_Sql += $@" AND PD.ProductRating BETWEEN {fromRating} AND {toRating} ";
                }

                if (!string.IsNullOrEmpty(model.Brand))
                {
                    where_Sql += $@" AND B.BrandId IN (SELECT value FROM string_split('{model.Brand}','|')) ";
                }

                if (!string.IsNullOrEmpty(model.Category))
                {
                    where_Sql += $@" AND C.CategoryId IN (SELECT value FROM string_split('{model.Category}','|')) ";
                }

                if (!string.IsNullOrEmpty(model.KeySearch))
                {
                    where_Sql += $@" AND (PD.Name LIKE '%{model.KeySearch}%' OR C.Name LIKE '%{model.KeySearch}%' OR B.Name LIKE '%{model.KeySearch}%' 
                                    OR PD.Tags LIKE '%{model.KeySearch}%' OR PD.UnitPrice LIKE '%{model.KeySearch}%' OR PD.DiscountPrice LIKE '%{model.KeySearch}%') ";
                }

                string flashDealJoin = "LEFT";
                if (model.IsFlashDeal)
                {
                    flashDealJoin = "INNER";
                }

                if (model.IsTrending)
                {
                    where_Sql += " AND PD.IsTrend = 1 ";
                }

                if (model.IsTodaysDeal)
                {
                    where_Sql += " AND PD.TodaysDeal = 1 ";
                }

                if (model.IsDiscount)
                {
                    where_Sql += " AND ( ISNULL(PD.DiscountType, '') != '' OR (GetUtcDate() BETWEEN dbo.GetLocalDate(PD.DiscountStartDate) AND dbo.GetLocalDate(PD.DiscountEndDate)) ) ";
                }

                if (model.IsFeatured)
                {
                    where_Sql += " AND PD.Featured = 1 ";
                }

                if (model.InStock)
                {
                    where_Sql += " AND PD.CurrentStock > 0 ";
                }

                if (model.OutOfStock)
                {
                    where_Sql += " AND PD.CurrentStock = 0 ";
                }

                string sql = $@"
                WITH 
                FD_PRODS AS (
                SELECT P.ProductId,
                DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * FDP.Discount) / 100) 
                WHEN GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate) AND FDP.DiscountType = 'amount' THEN (P.UnitPrice - FDP.Discount) 
                ELSE P.UnitPrice 
                END
                FROM FlashDeals FD
                INNER JOIN FlashDealProducts FDP ON FDP.FlashDealId = FD.FlashDealId
                INNER JOIN Products P ON P.ProductId = FDP.ProductId AND P.Published = 1
                WHERE FD.[Status] = 1 AND GetUtcDate() >= dbo.GetLocalDate(FD.StartDate) AND GetUtcDate() <= dbo.GetLocalDate(FD.EndDate)
                ),
                PD AS (
                    Select P.ProductId, P.Name, P.CategoryId, P.BrandId, P.Photos, P.ThumbnailImage, P.VideoProvider, P.VideoLink, ISNULL(P.Tags, '') Tags,
                    P.Description, P.UnitPrice, P.PurchasePrice, P.VariantProduct, P.Attributes, P.ChoiceOptions, P.Colors, P.Variations, 
                    P.TodaysDeal, P.Published, P.Approved, P.StockVisibilityState, P.CashOnDelivery, P.Featured, P.SellerFeatured, P.CurrentStock, 
                    P.MinQuantity, P.LowStaockQuantity, P.Discount, P.DiscountType, dbo.GetLocalDate(P.DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(P.DiscountEndDate) DiscountEndDate, P.Tax, P.TaxType, 
                    P.ShippingType, P.ShippingCost, P.IsQuantityMultiplied, P.EstShippingDays, P.NumberOfSale, P.MetaTittle, P.MetaDescription, 
                    P.MetaImage, P.PDF, P.Slug, P.Rating, P.Barcode, P.Digital, P.AuctionProduct, P.FileName, P.FilePath, P.ExternalLink, 
                    P.ExternalLinkButton, P.WholeSaleProduct, P.UnitId, P.ProductSKU, P.IsTrend,
                    DiscountPrice = ISNULL(FDP.DiscountPrice, CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
                    WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
                    ELSE P.UnitPrice 
                    END),
                    (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage,
                    (SELECT AVG(Rating) FROM Reviews WHERE ProductId = P.ProductId) ProductRating,
                    (SELECT COUNT(ReviewId) FROM Reviews WHERE ProductId = P.ProductId) NoOfReviews
                    from Products P
	                {flashDealJoin} JOIN FD_PRODS FDP ON FDP.ProductId = P.ProductId
                    WHERE P.Published = 1
                ),
                WISH AS (
	                SELECT W.ProductId, 1 IsWish
	                From WishLists W
	                INNER JOIN PD ON PD.ProductId = W.ProductId
	                WHERE UserId = '{userId}'
                )
                Select PD.ProductId, PD.Name, PD.CategoryId, PD.BrandId, PD.Photos, PD.ThumbnailImage, PD.VideoProvider, PD.VideoLink, PD.Tags,
                PD.Description, PD.UnitPrice, PD.PurchasePrice, PD.VariantProduct, PD.Attributes, PD.ChoiceOptions, PD.Colors, PD.Variations, 
                PD.TodaysDeal, PD.Published, PD.Approved, PD.StockVisibilityState, PD.CashOnDelivery, PD.Featured, PD.SellerFeatured, PD.CurrentStock, 
                PD.MinQuantity, PD.LowStaockQuantity, PD.Discount, PD.DiscountType, PD.DiscountStartDate, PD.DiscountEndDate, PD.Tax, PD.TaxType, 
                PD.ShippingType, PD.ShippingCost, PD.IsQuantityMultiplied, PD.EstShippingDays, PD.NumberOfSale, PD.MetaTittle, PD.MetaDescription, 
                PD.MetaImage, PD.PDF, PD.Slug, PD.Rating, PD.Barcode, PD.Digital, PD.AuctionProduct, PD.FileName, PD.FilePath, PD.ExternalLink, 
                PD.ExternalLinkButton, PD.WholeSaleProduct, PD.UnitId, PD.ProductSKU,
                PD.DiscountPrice, PD.ProductImage, PD.ProductRating, PD.NoOfReviews,
                (SELECT STRING_AGG(Name + '-' + Code, ',') FROM Color WHERE Code IN (SELECT value FROM string_split((REPLACE(REPLACE(REPLACE(Colors, '[', ''), ']', ''), '""', '')),','))) ColorNameWithCode,
                C.Name CategoryName, B.Name BrandName,
                COUNT(PD.ProductId) OVER() TotalRows,
                (Select Max(UnitPrice) FROM Products) MaxPrice,
				(Select Min(UnitPrice) FROM Products) MinPrice,
                ISNULL(W.IsWish, 0) IsWish
                from PD
                LEFT JOIN Categories C ON C.CategoryId = PD.CategoryId
                LEFT JOIN Brands B ON B.BrandId = PD.BrandId
                LEFT JOIN WISH W ON W.ProductId = PD.ProductId
                WHERE PD.ProductId <> 0 
                {where_Sql}
                {sortBy_Sql}
                {pagination_Sql}";

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

        public async Task<dynamic> GetFooterInfoAsync()
        {
            string sql = $@"
                SELECT Linkedin, Twitter, Facebook, Youtube,
                PhoneNumber, SceoundPhoneNumber, TelephonNumber, Email, Address, ZipCode,
                (Select [Value] from BusinessSettings WHERE Type = 'site_copywrite_text') CopyWriteText,
                (Select [Value] from BusinessSettings WHERE Type = 'footer_online_payment_icon') PaymentIcons
                FROM GeneralSettings";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<dynamic>(sql);
                return records.ToList().FirstOrDefault();
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

        public async Task<List<dynamic>> GetPickupPointListAsync()
        {
            try
            {
                string sql = $@"
                SELECT PickupPointId, Name, Address, Phone, ISNULL(CashOnPickupStatus, 0) CashOnFacility
                FROM PickupPoints
                WHERE PickUpStatus = 1
                ORDER BY Name ASC
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

        public async Task<List<dynamic>> GetCountryListAsync()
        {
            try
            {
                string sql = $@"
                Select CountriesId CountryId, Name CountryName from Countries ORDER BY Name ASC
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

        public async Task<List<dynamic>> GetStateListAsync()
        {
            try
            {
                string sql = $@"
                Select StateId, Name StateName, CountriesId CountryId from States ORDER BY Name ASC
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

        public async Task<List<dynamic>> GetCityListAsync()
        {
            try
            {
                string sql = $@"
                Select CitiesId CityId, Name CityName, StateId, Cost ShippingCost  from Cities ORDER BY Name ASC
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

        public async Task<int> CreateUserActivities(SearchActivity entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                var id = await _connection.ExecuteAsync("INSERT INTO SearchActivitys (URL, SearchCriteria, Date, loginID, UserName) VALUES (@URL, @SearchCriteria, @Date, @loginID, @UserName)", new { entity.URL, entity.SearchCriteria, entity.Date, entity.loginID, entity.UserName }, transaction);
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

        public async Task<List<string>> GetSearchResustAsync(string keySearch)
        {
            try
            {
                string sql = $@"
                WITH NAMES AS (
                    Select [Name], 2 Ordr from Products WHERE [Name] LIKE '%{keySearch}%'
                    UNION
                    Select [Name], 1 Ordr from Categories WHERE [Name] LIKE '%{keySearch}%'
                    UNION
                    Select [Name], 3 Ordr from Brands WHERE [Name] LIKE '%{keySearch}%'
                )
                SELECT [Name]
                FROM NAMES
                ORDER BY Ordr ASC
                ";

                try
                {
                    await _connection.OpenAsync();
                    var records = await _connection.QueryAsync<string>(sql);
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

        public async Task<dynamic> GetProductDetailsForSEOAsync(int productId)
        {
            try
            {
                string sql = $@"
                    Select P.ProductId, P.Name, P.MetaTittle, P.MetaDescription, P.MetaImage, C.Name CategoryName,
                    (SELECT Top 1 u.FileName FROM Uploads u WHERE u.UploadId = P.ThumbnailImage) ProductImage
                    from Products P
                    LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                    WHERE P.ProductId = {productId};
                ";

                try
                {
                    await _connection.OpenAsync();

                    var queryResult = await _connection.QueryMultipleAsync(sql);
                    dynamic product = queryResult.Read().FirstOrDefault();
                    return product;
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

        public async Task ActionToReviewAsync(int id, int action, string userId)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                string sql = $@"
                DECLARE
                @ReviewId INT = {id},
                @Action INT = {action},
                @UserId NVARCHAR(200) = '{userId}'

                IF EXISTS (SELECT * FROM ReviewHistory WHERE UserId = @UserId AND ReviewId = @ReviewId)
                BEGIN
	                UPDATE ReviewHistory SET [Action] = @Action WHERE ReviewId = @ReviewId
                END
                ELSE
                BEGIN
	                INSERT INTO ReviewHistory (ReviewId, UserId, [Action])
	                VALUES (@ReviewId, @UserId, @Action)
                END";

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

    }
}

