using ApplicationCore.DTOs;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using static Dapper.SqlMapper;
using Image = iText.Layout.Element.Image;
namespace Infrastructure.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IDapperService<Product> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public ProductService(IDapperService<Product> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Product> GetInitial()
        {
            try
            {
                Product data = new Product();
                var query = $@" SELECT CategoryId as Id, Name as Text, ParentId as ParentId  FROM Categories;
                                SELECT BrandId as Id, Name as Text FROM Brands;
                                SELECT UnitId as Id, Name as Text FROM Units;
                                SELECT AttributeId Id, Name Text FROM Attributes;
                                SELECT Code Id, Name Text, Code FROM Color ORDER BY Name ASC;

                                SELECT 'Youtube' as Text, 'Youtube' as Id
                                union all
                                SELECT 'Dailymotion' as Text, 'Dailymotion' as Id
                                union all
                                SELECT 'Vimeo' as Text, 'Vimeo' as Id;

                                -- Flash Deal
                                SELECT Id = fd.FlashDealId, Text = fd.Title
                                FROM FlashDeals fd WHERE FD.Status = 1; 

                                --Taxes
                                SELECT Id =TaxId, Text= Name FROM Taxes WHERE TaxStatus=1;
                                


                                ";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.CategoryList = DFS.GetCategory(queryResult.Read<CategorySelect2Option>().ToList());
                data.BrandList = queryResult.Read<Select2OptionModel>().ToList();
                data.UnitList = queryResult.Read<Select2OptionModel>().ToList();
                data.AttributeList = queryResult.Read<Select2OptionModel>().ToList();
                data.ColorList = queryResult.Read<ColorSelect2Option>().ToList();
                data.VideoProviderList = queryResult.Read<Select2OptionModel>().ToList();
                data.FlashDealList = queryResult.Read<Select2OptionModel>().ToList();
                data.TaxList = queryResult.Read<Select2OptionModel>().ToList();

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Product>> GetAll()
        {
            var query = $@"SELECT * FROM Categories;";

            try
            {
                var result = await _service.GetDataAsync<Product>(query);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Product> GetAsync(int id)
        {
            var query = $@"SELECT ProductId,Name,AddedBy,UserId,CategoryId,BrandId,Photos,ThumbnailImage,VideoProvider,VideoLink,Tags,Description,UnitPrice,
                            SalePrice,VariantProduct,Attributes,ChoiceOptions,Colors,Variations,TodaysDeal,Published,Approved,StockVisibilityState,CashOnDelivery,Featured,
                            SellerFeatured,CurrentStock,MinQuantity,LowStaockQuantity,Discount,DiscountType,dbo.GetLocalDate(DiscountStartDate)DiscountStartDate, dbo.GetLocalDate(DiscountEndDate)DiscountEndDate,Tax,TaxType,ShippingType,ShippingCost,
                            IsQuantityMultiplied,EstShippingDays,NumberOfSale,MetaTittle,MetaDescription,MetaImage,PDF,Slug,Rating,Barcode,Digital,AuctionProduct,FileName,FilePath,
                            ExternalLink,ExternalLinkButton,WholeSaleProduct,UnitId,ProductSKU,InHouseProduct,TaxId,
                            (Select ISNULL(Value, 'false') from BusinessSettings WHERE Type = 'shipping_configuration_enabled') IsShippingEnable,
                            (Select ISNULL(Value, 'false') from BusinessSettings WHERE Type = 'flash_deal_enabled') IsFlashDealEnable,
                            (Select ISNULL(Value, 'false') from BusinessSettings WHERE Type = 'stock_visibility_enabled') IsStockVisibilityEnable,
                            (Select ISNULL(Value, 'false') from BusinessSettings WHERE Type = 'vat_tax_enabled') IsVatTaxEnable
                            FROM Products WHERE ProductId = {id};

                            SELECT TOP 1 Id, FDP.FlashDealId, FDP.ProductId, FDP.Discount, FDP.DiscountType 
                            FROM FlashDealProducts FDP
                            INNER JOIN FlashDeals FD ON FD.FlashDealId = FDP.FlashDealId
                            WHERE FDP.ProductId = {id} AND FD.Status = 1;

                            -- dropdown list
                            SELECT CAST(CategoryId AS VARCHAR) Id, Name as Text, ParentId as ParentId  FROM Categories;

                            SELECT CAST(BrandId AS VARCHAR) Id, Name as Text FROM Brands;

                            SELECT CAST(UnitId AS VARCHAR)  Id, Name as Text FROM Units;

                            SELECT CAST(AttributeId AS VARCHAR) Id, Name Text FROM Attributes;

                            SELECT CAST(Code AS VARCHAR) Id, Name Text, Code FROM Color ORDER BY Name ASC;

                            SELECT 'Youtube' as Text, 'Youtube' as Id
                            union all
                            SELECT 'Dailymotion' as Text, 'Dailymotion' as Id
                            union all
                            SELECT 'Vimeo' as Text, 'Vimeo' as Id;

                            -- Flash Deal
                            SELECT Id = fd.FlashDealId, Text = fd.Title
                            FROM FlashDeals fd WHERE FD.Status = 1;

                            --Taxes
                            SELECT Id =TaxId, Text= Name FROM Taxes WHERE TaxStatus=1;

                            SELECT Id = u.UploadId, Text = u.FileName FROM Uploads u
                            WHERE u.UploadId IN (SELECT value
                            FROM STRING_SPLIT((SELECT Photos FROM Products WHERE ProductId = {id} ), ',')
                            WHERE RTRIM(value) <> '');

                            SELECT Id = u.UploadId, Text = u.FileName FROM Uploads u
                            WHERE u.UploadId IN (SELECT ThumbnailImage FROM Products WHERE ProductId = {id});

                            SELECT Id = u.UploadId, Text = u.FileName FROM Uploads u
                            WHERE u.UploadId IN (SELECT MetaImage FROM Products WHERE ProductId = {id});

                            select Variant_id,ps.Image, ps.ProductId, Variant, ISNULL(SKU, '') SKU, Price,  up.FileName as ImageUrl
                        from ProductVariants ps
                        left join Uploads up on up.UploadId = ps.Image
						WHERE ps.ProductId={id};
                            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Product data = queryResult.Read<Product>().FirstOrDefault();
                data.FlashDealProducts = queryResult.Read<FlashDealProducts>().ToList();
                data.CategoryList = DFS.GetCategory(queryResult.Read<CategorySelect2Option>().ToList());
                data.BrandList = queryResult.Read<Select2OptionModel>().ToList();
                data.UnitList = queryResult.Read<Select2OptionModel>().ToList();
                data.AttributeList = queryResult.Read<Select2OptionModel>().ToList();
                data.ColorList = queryResult.Read<ColorSelect2Option>().ToList();
                data.VideoProviderList = queryResult.Read<Select2OptionModel>().ToList();
                data.FlashDealList = queryResult.Read<Select2OptionModel>().ToList();
                data.TaxList = queryResult.Read<Select2OptionModel>().ToList();
                data.PhotoSourceList = queryResult.Read<Select2OptionModel>().ToList();
                data.ThumbnailImageSource = queryResult.Read<Select2OptionModel>().FirstOrDefault();
                data.MetaImageSource = queryResult.Read<Select2OptionModel>().FirstOrDefault();
                data.ProductVariant = queryResult.Read<ProductVariant>().ToList();

                if (data.FlashDealProducts.Count > 0)
                {
                    data.FlashDealId = data.FlashDealProducts.FirstOrDefault().FlashDealId;
                    data.FlashDiscount = data.FlashDealProducts.FirstOrDefault().Discount;
                    data.FlashDiscountType = data.FlashDealProducts.FirstOrDefault().DiscountType;
                }

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

        public async Task<int> SaveAsync(Product entity, List<ProductVariant> productVariant, ProductTax tax, FlashDealProducts flashDealProducts)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                int id = await _service.SaveSingleAsync(entity, transaction);
                if (flashDealProducts != null)
                {
                    flashDealProducts.ProductId = id;
                    await _service.SaveSingleAsync(flashDealProducts, transaction);
                }
                productVariant?.ForEach(item =>
                {
                    item.ProductId = id;
                });
                await _service.SaveAsync(productVariant, transaction);

                tax.ProductId = id;
                await _service.SaveSingleAsync(tax, transaction);

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

        public async Task<int> UpdateAsync(Product entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);

                await _service.SaveAsync(entity.FlashDealProducts, transaction);
                await _service.SaveAsync(entity.ProductVariant, transaction);
                await _service.SaveAsync(entity.ProductTaxes, transaction);

                transaction.Commit();
                return entity.ProductId;
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

        public async Task<int> UpdateProductAsync(Product entity)
        {
            try
            {
                entity.EntityState = ApplicationCore.Enums.EntityState.Modified;
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

        public async Task<Product> GetProductAsync(int id)
        {
            var query = $@"
                        SELECT P.*, img.FileName ThumbnailImagePath
                        FROM Products P
                        LEFT JOIN Uploads img on img.UploadId = P.ThumbnailImage
                        WHERE P.ProductId={id};

                        SELECT * FROM FlashDealProducts WHERE ProductId={id};

                        SELECT * FROM ProductVariants WHERE ProductId={id};

                        SELECT * FROM ProductTaxes WHERE ProductId={id};
                            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Product data = queryResult.Read<Product>().FirstOrDefault();
                data.FlashDealProducts = queryResult.Read<FlashDealProducts>().ToList();
                data.ProductVariant = queryResult.Read<ProductVariant>().ToList();
                data.ProductTaxes = queryResult.Read<ProductTax>().ToList();

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

        public async Task<Product> GetProductDiscountAsync(int id)
        {
            var query = $@"
            WITH 
            PROD AS (
            Select P.ProductId, P.UnitPrice,
            DiscountPrice = CASE WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'percent' THEN (P.UnitPrice - (P.UnitPrice * P.Discount) / 100) 
            WHEN GetUtcDate() >= dbo.GetLocalDate(P.DiscountStartDate) AND GetUtcDate() <= dbo.GetLocalDate(P.DiscountEndDate) AND P.DiscountType = 'amount' THEN (P.UnitPrice - P.Discount) 
            ELSE P.UnitPrice 
            END
            from Products P
            WHERE P.ProductId = {id}
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
            )
            Select TOP 1 P.ProductId, P.UnitPrice, ISNULL(FDP.DiscountPrice, P.DiscountPrice) DiscountPrice, (P.UnitPrice - ISNULL(FDP.DiscountPrice, P.DiscountPrice)) Discount
            from PROD P
            LEFT JOIN FD_PRODS FDP ON FDP.ProductId = P.ProductId;
            ";

            try
            {
                await _connection.OpenAsync();
                Product data = await _connection.QuerySingleAsync<Product>(query);

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

        public async Task<List<Product>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY ProductId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT
                                ps.ProductId
                                ,ps.Name
                                ,ImageLink = img.FileName
                                ,UnitPrice
                                ,ps.TodaysDeal
                                ,ps.Published
                                ,ps.Featured
                                ,ps.IsTrend
                                ,ps.CurrentStock
                                ,HasVariation = case when ps.Variations is null or Variations = '[]' then 0 else 1 end
                                ,C.Name CategoryName, ISNULL(B.Name, '') BrandName
                                ,ps.LowStaockQuantity
                                ,Count(1) Over() TotalRows
                                FROM Products ps
                                LEFT JOIN Uploads img on img.UploadId = ps.ThumbnailImage 
								LEFT JOIN Categories C ON C.CategoryId = Ps.CategoryId
								LEFT JOIN Brands B ON B.BrandId = Ps.BrandId
                                WHERE 1=1 ";

                if (searchBy != "")
                    sql += " AND ps.Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Product>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> DeleteAsync(Product entity)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);

                await _service.SaveAsync(entity.FlashDealProducts, transaction);
                await _service.SaveAsync(entity.ProductVariant, transaction);
                await _service.SaveAsync(entity.ProductTaxes, transaction);

                transaction.Commit();
                return entity.ProductId;
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

        #region Other Technical Stuffs

        public async Task<List<AttributeValue>> GetAttribute(string attributeList)
        {
            var query = $@"SELECT Id, Value, ColorCode, ATT.AttributeId, ATT.Name AttributeName
                        FROM Attributes ATT
                        INNER JOIN AttributeValues AV on AV.AttributeId = ATT.AttributeId
                        where ATT.AttributeId in (" + attributeList + ");";

            try
            {
                var result = await _service.GetDataAsync<AttributeValue>(query);
                return result;
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

        #endregion Other Technical Stuffs

        public async Task<Product> GetViewProductByIdAsync(int id)
        {
            var query = $@"SELECT ProductId,Name,AddedBy,UserId,CategoryId,BrandId,Photos,ThumbnailImage,VideoProvider,VideoLink,Tags,Description,UnitPrice,
                            SalePrice,VariantProduct,Attributes,ChoiceOptions,Colors,Variations,TodaysDeal,Published,Approved,StockVisibilityState,CashOnDelivery,Featured,
                            SellerFeatured,CurrentStock,MinQuantity,LowStaockQuantity,Discount,DiscountType,dbo.GetLocalDate(DiscountStartDate) DiscountStartDate, dbo.GetLocalDate(DiscountEndDate) DiscountEndDate,Tax,TaxType,ShippingType,ShippingCost,
                            IsQuantityMultiplied,EstShippingDays,NumberOfSale,MetaTittle,MetaDescription,MetaImage,PDF,Slug,Rating,Barcode,Digital,AuctionProduct,FileName,FilePath,
                            ExternalLink,ExternalLinkButton,WholeSaleProduct,UnitId,ProductSKU,InHouseProduct
                            FROM Products WHERE ProductId = {id};

                            SELECT Id = u.UploadId, Text = u.FileName FROM Uploads u
                            WHERE u.UploadId IN (SELECT value
                            FROM STRING_SPLIT((SELECT Photos FROM Products WHERE ProductId = {id} ), ',')
                            WHERE RTRIM(value) <> '');

                            SELECT Id = u.UploadId, Text = u.FileName FROM Uploads u
                            WHERE u.UploadId IN (SELECT ThumbnailImage FROM Products WHERE ProductId = {id});

                            SELECT Id = u.UploadId, Text = u.FileName FROM Uploads u
                            WHERE u.UploadId IN (SELECT MetaImage FROM Products WHERE ProductId = {id});
                            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Product data = queryResult.Read<Product>().FirstOrDefault();
                data.PhotoSourceList = queryResult.Read<Select2OptionModel>().ToList();
                data.ThumbnailImageSource = queryResult.Read<Select2OptionModel>().FirstOrDefault();
                data.MetaImageSource = queryResult.Read<Select2OptionModel>().FirstOrDefault();
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

        public async Task<List<Product>> GetFlashDealProductsAsync(int flashDealId)
        {
            try
            {
                string sql = $@"
                SELECT FDP.FlashDealId, FDP.ProductId, FDP.Discount FlashDiscount, FDP.DiscountType FlashDiscountType,
                P.Name, P.CategoryId, P.BrandId, P.CurrentStock, P.Discount, P.DiscountType,
                C.Name CategoryName, B.Name BrandName
                FROM FlashDealProducts FDP
                INNER JOIN Products P ON P.ProductId = FDP.ProductId AND P.Published = 1
                LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                LEFT JOIN Brands B ON B.BrandId = P.BrandId
                WHERE FDP.FlashDealId = {flashDealId} ";
                var result = await _service.GetDataAsync<Product>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Product>> GetFlashDealProductAsync(int flashDealId, int productId)
        {
            try
            {
                string sql = $@"
                SELECT FDP.FlashDealId, FDP.ProductId, FDP.Discount FlashDiscount, FDP.DiscountType FlashDiscountType,
                P.Name, P.CategoryId, P.BrandId, P.CurrentStock, P.Discount, P.DiscountType,
                C.Name CategoryName, B.Name BrandName
                FROM FlashDealProducts FDP
                INNER JOIN Products P ON P.ProductId = FDP.ProductId AND P.Published = 1
                INNER JOIN Categories C ON C.CategoryId = P.CategoryId
                INNER JOIN Brands B ON B.BrandId = P.BrandId
                WHERE FDP.FlashDealId = {flashDealId} AND FDP.ProductId = {productId}";
                var result = await _service.GetDataAsync<Product>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Select2OptionModel>> GetProductsAsync()
        {
            try
            {
                string sql = $@"
                SELECT Id = P.ProductId, Text = P.Name --'Name: ' + P.Name + ', Category: ' + C.Name + ', Brand: ' + B.Name
                FROM Products P WHERE P.Published = 1
                --LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                --LEFT JOIN Brands B ON B.BrandId = P.BrandId";
                return await _service.GetDataAsync<Select2OptionModel>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DeleteFlashDealProductAsync(int flashDealId, int productId)
        {
            try
            {
                await _connection.OpenAsync();
                var query = $@"DELETE FROM FlashDealProducts WHERE FlashDealId = @flashDealId AND ProductId = @productId";
                await _connection.ExecuteAsync(query, new { flashDealId, productId });
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

        public async Task<int> SaveFlashDealProductAsync(FlashDealProducts entity)
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

        public async Task DeleteUploadImg(string ids)
        {
            try
            {
                await _connection.OpenAsync();
                var query = $@"UPDATE Uploads SET IsDeleted = 1 WHERE UploadId IN (SELECT Value FROM String_SPLIT('{ids}',','))";
                await _connection.ExecuteAsync(query);
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

        public async Task<List<Product>> GetProductsToAddAsync()
        {
            try
            {
                string sql = $@"
                SELECT P.ProductId, P.Name, P.CategoryId, P.BrandId, P.CurrentStock, P.Discount, P.DiscountType, 
                F.Discount FlashDiscount, F.DiscountType FlashDiscountType
                FROM Products P 
                LEFT JOIN FlashDealProducts F ON F.ProductId = P.ProductId AND P.Published = 1
                GROUP BY P.ProductId, P.Name, P.CategoryId, P.BrandId, P.CurrentStock, P.Discount, P.DiscountType, 
                F.Discount, F.DiscountType";
                var result = await _service.GetDataAsync<Product>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ProductStock>> GetVariantByProductAsync(int productId)
        {
            try
            {
                string sql = $@"
                Select ProductStockId, Variant, SKU, Price, [Image], ProductId, Quantity, PurchaseQty
                from ProductStocks
                WHERE ProductId = {productId}";
                var result = await _service.GetDataAsync<ProductStock>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Barcode functionality implementation
        public async Task<List<object>> GetProductsWithVariantsForBarcode()
        {
            try
            {
                await _connection.OpenAsync();
                var query = @"
                    SELECT 
                        p.ProductId,
                        p.Name,
                        p.ProductSKU,
                        p.Barcode,
                        p.UnitPrice,
                        p.SalePrice,
                        c.Name as CategoryName,
                        CASE WHEN pv.Variant_id IS NOT NULL THEN 1 ELSE 0 END as HasVariants
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                    LEFT JOIN ProductVariants pv ON p.ProductId = pv.ProductId
                    WHERE p.Published = 1 AND p.Approved = 1
                    GROUP BY p.ProductId, p.Name, p.ProductSKU, p.Barcode, p.UnitPrice, p.SalePrice, c.Name, pv.Variant_id
                    ORDER BY p.Name";

                var products = await _connection.QueryAsync<dynamic>(query);
                var result = new List<object>();

                foreach (var product in products)
                {
                    try
                    {
                        var productObj = new
                        {
                            productId = (int)product.ProductId,
                            name = product.Name,
                            productSKU = product.ProductSKU,
                            barcode = product.Barcode,
                            unitPrice = product.UnitPrice,
                            salePrice = product.SalePrice,
                            categoryName = product.CategoryName,
                            hasVariants = product.HasVariants == 1,
                            variants = await GetProductVariantsForBarcode((int)product.ProductId)
                        };
                        result.Add(productObj);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing product: {ex.Message}");
                        continue;
                    }
                }

                return result;
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

        public async Task<List<object>> SearchProductsWithVariantsForBarcode(string searchTerm)
        {
            try
            {
                await _connection.OpenAsync();
                string sql = $@"
                    SELECT DISTINCT 
                        p.ProductId,
                        p.Name as name,
                        p.ProductSKU as productSKU,
                        p.Barcode as barcode,
                        p.UnitPrice as unitPrice,
                        c.Name as categoryName,
                        CASE WHEN COUNT(ps.Variant_id) > 0 THEN 1 ELSE 0 END as hasVariants
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                    LEFT JOIN ProductVariants ps ON p.ProductId = ps.ProductId
                    WHERE (p.Name LIKE '%{searchTerm}%' 
                           OR ps.SKU LIKE '%{searchTerm}%' 
                           OR ps.Barcode LIKE '%{searchTerm}%')
                    GROUP BY p.ProductId, p.Name, p.ProductSKU, p.Barcode, p.UnitPrice, c.Name
                    ORDER BY p.Name";

                var products = await _connection.QueryAsync<dynamic>(sql);
                var result = new List<object>();

                // Get variants for each product
                foreach (var product in products)
                {
                    try
                    {
                        // Use dynamic object properties directly
                        var productId = (int)product.ProductId;
                        var productObj = new
                        {
                            productId = productId,
                            name = product.name,
                            productSKU = product.productSKU,
                            barcode = product.barcode,
                            unitPrice = product.unitPrice,
                            categoryName = product.categoryName,
                            hasVariants = product.hasVariants == 1,
                            variants = await GetProductVariants(productId)
                        };
                        result.Add(productObj);
                    }
                    catch (Exception ex)
                    {
                        // Log the error and continue with next product
                        Console.WriteLine($"Error processing product: {ex.Message}");
                        continue;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }
        public async Task<object> GetProductWithVariantsForBarcode(int productId)
        {
            try
            {
                await _connection.OpenAsync();
                string sql = $@"
                    SELECT DISTINCT 
                        p.ProductId,
                        p.Name as name,
                        p.ProductSKU as productSKU,
                        p.Barcode as barcode,
                        p.UnitPrice as unitPrice,
                        c.Name as categoryName,
                        CASE WHEN COUNT(ps.Variant_id) > 0 THEN 1 ELSE 0 END as hasVariants
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                    LEFT JOIN ProductVariants ps ON p.ProductId = ps.ProductId
                    WHERE p.ProductId = {productId}
                    GROUP BY p.ProductId, p.Name, p.ProductSKU, p.Barcode, p.UnitPrice, c.Name";

                var products = await _connection.QueryAsync<dynamic>(sql);

                if (products != null && products.Any())
                {
                    var product = products.First();
                    var productObj = new
                    {
                        productId = (int)product.ProductId,
                        name = product.name,
                        productSKU = product.productSKU,
                        barcode = product.barcode,
                        unitPrice = product.unitPrice,
                        categoryName = product.categoryName,
                        hasVariants = product.hasVariants == 1,
                        variants = await GetProductVariants(productId)
                    };
                    return productObj;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        private async Task<List<object>> GetProductVariantsForBarcode(int productId)
        {
            try
            {
                var query = @"
                    SELECT 
                        Variant_id as VariantId,
                        SKU,
                        Barcode,
                        Attributes,
                        Regular_price as RegularPrice,
                        Price as SalePrice
                    FROM ProductVariants
                    WHERE ProductId = @productId AND Status = 1
                    ORDER BY SKU";

                var variants = await _connection.QueryAsync<dynamic>(query, new { productId });
                var result = new List<object>();

                foreach (var variant in variants)
                {
                    var variantObj = new
                    {
                        variantId = variant.VariantId,
                        sku = variant.SKU,
                        barcode = variant.Barcode,
                        attributes = variant.Attributes,
                        regularPrice = variant.RegularPrice,
                        salePrice = variant.SalePrice
                    };
                    result.Add(variantObj);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<List<object>> GetProductVariants(int productId)
        {
            try
            {
                var query = @"
                    SELECT 
                        Variant_id as VariantId,
                        SKU,
                        Barcode,
                        Attributes,
                        Regular_price as RegularPrice,
                        Price as SalePrice
                    FROM ProductVariants
                    WHERE ProductId = @productId 
                    ORDER BY SKU";

                var variants = await _connection.QueryAsync<dynamic>(query, new { productId });
                var result = new List<object>();

                foreach (var variant in variants)
                {
                    var variantObj = new
                    {
                        variantId = variant.VariantId,
                        sku = variant.SKU,
                        barcode = variant.Barcode,
                        attributes = variant.Attributes,
                        regularPrice = variant.RegularPrice,
                        salePrice = variant.SalePrice
                    };
                    result.Add(variantObj);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public async Task<byte[]> GenerateBarcodeImage(string barcodeText, string barcodeType, int width, int height)
        {
            try
            {
                if (string.IsNullOrEmpty(barcodeText))
                {
                    throw new ArgumentException("Barcode text cannot be null or empty.");
                }

                // Use the generic BarcodeWriter and specify the renderer type
                var writer = new BarcodeWriter<Bitmap>
                {
                    Format = BarcodeFormat.CODE_128, // Set a default format
                    Options = new EncodingOptions
                    {
                        Width = width,
                        Height = height,
                        Margin = 0,
                        PureBarcode = false
                    },
                    Renderer = new BitmapRenderer() // Explicitly set the renderer
                };

                // Configure barcode settings based on type
                switch (barcodeType.ToLower())
                {
                    case "code128":
                        writer.Format = BarcodeFormat.CODE_128;
                        break;
                    case "code39":
                        writer.Format = BarcodeFormat.CODE_39;
                        break;
                    case "ean13":
                        var ean13Text = new string(barcodeText.Where(char.IsDigit).ToArray());
                        if (ean13Text.Length != 13)
                        {
                            throw new ArgumentException("EAN-13 barcode requires exactly 13 digits.");
                        }
                        barcodeText = ean13Text;
                        writer.Format = BarcodeFormat.EAN_13;
                        break;
                    case "upc":
                        var upcText = new string(barcodeText.Where(char.IsDigit).ToArray());
                        if (upcText.Length != 12)
                        {
                            throw new ArgumentException("UPC-A barcode requires exactly 12 digits.");
                        }
                        barcodeText = upcText;
                        writer.Format = BarcodeFormat.UPC_A;
                        break;
                    case "qr":
                        writer.Format = BarcodeFormat.QR_CODE;
                        // QR Codes have different options, so we can configure them specifically
                        writer.Options = new QrCodeEncodingOptions
                        {
                            Width = width,
                            Height = height,
                            Margin = 1,
                            ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H
                        };
                        break;
                    default:
                        writer.Format = BarcodeFormat.CODE_128;
                        break;
                }

                // Generate barcode and handle potential exceptions from ZXing
                Bitmap bitmap = null;
                try
                {
                    bitmap = writer.Write(barcodeText);
                }
                catch (Exception zxingEx)
                {
                    throw new Exception($"ZXing failed to write barcode: {zxingEx.Message}");
                }

                if (bitmap == null)
                {
                    throw new Exception("Failed to generate barcode image.");
                }

                // Convert to byte array
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating barcode: {ex.Message}", ex);
            }
        }


        public async Task<byte[]> GenerateBarcodePDF(List<BarcodeItemModel> barcodeItems, BarcodeSettingsModel settings)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    // Paper Size নির্বাচন
                    PageSize pageSize = settings.PaperSize.ToLower() switch
                    {
                        "a4" => PageSize.A4,
                        "a5" => PageSize.A5,
                        "letter" => PageSize.LETTER,
                        "legal" => PageSize.LEGAL,
                        _ => PageSize.A4
                    };

                    // Document তৈরি
                    var writer = new PdfWriter(ms);
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf, pageSize);
                    document.SetMargins(20, 20, 20, 20);

                    // Title
                    PdfFont boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                    var title = new Paragraph("Product Barcodes")
                        .SetFont(boldFont)
                        .SetFontSize(16)
                        .SetTextAlignment(TextAlignment.CENTER);
                    document.Add(title);
                    document.Add(new Paragraph(" "));

                    // Items per row হিসাব
                    int itemsPerRow = (int)(pageSize.GetWidth() / (settings.BarcodeWidth + 20));
                    if (itemsPerRow <= 0) itemsPerRow = 1;

                    Table table = new Table(UnitValue.CreatePercentArray(itemsPerRow))
                        .UseAllAvailableWidth();

                    foreach (var item in barcodeItems)
                    {
                       

                        for (int i = 0; i < item.Quantity; i++)
                        {
                            try
                            {
                                // Barcode Image Generate
                                var barcodeBytes = await GenerateBarcodeImage(
                                    item.BarcodeText,
                                    settings.BarcodeType,
                                    settings.BarcodeWidth,
                                    settings.BarcodeHeight
                                );

                                if (barcodeBytes == null || barcodeBytes.Length == 0)
                                    throw new Exception($"Failed to generate barcode for {item.ProductName}");

                                // Image add
                                ImageData imageData = ImageDataFactory.Create(barcodeBytes);
                                Image barcodeImage = new Image(imageData)
                                    .SetAutoScale(true)
                                    .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                                Cell cell = new Cell()
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                    .SetPadding(5)
                                    .SetBorder(Border.NO_BORDER);

                                // Barcode বসানো
                                cell.Add(barcodeImage);

                                // Product info
                                if (settings.ShowProductName)
                                    cell.Add(new Paragraph(item.ProductName).SetFontSize(8));

                                if (settings.ShowPrice)
                                    cell.Add(new Paragraph($"৳{item.Price:N2}")
                                        .SetFontSize(10)
                                    );

                                if (settings.ShowCompanyName)
                                    cell.Add(new Paragraph(settings.CompanyName).SetFontSize(6));

                                table.AddCell(cell);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {item.ProductName} - {ex.Message}");
                                Cell errorCell = new Cell()
                                    .Add(new Paragraph($"Error: {item.ProductName}\n{ex.Message}")
                                    .SetFontSize(8)
                                    .SetFontColor(iText.Kernel.Colors.ColorConstants.RED))
                                    .SetTextAlignment(TextAlignment.CENTER)
                                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                    .SetPadding(10)
                                    .SetBorder(Border.NO_BORDER);
                                table.AddCell(errorCell);
                            }
                        }
                    }

                    document.Add(table);
                    document.Close();

                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating PDF: {ex.Message}", ex);
            }
        }

        public async Task<BarcodeResult> GenerateBarcode(BarcodeRequestModel request)
        {
            try
            {
                var barcodeItems = new List<BarcodeItemModel>();

                foreach (var product in request.Products)
                {
                    var barcodeText = !string.IsNullOrEmpty(product.Barcode) ?
                        product.Barcode : $"PROD{product.Id:D8}";

                    barcodeItems.Add(new BarcodeItemModel
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        BarcodeText = barcodeText,
                        Price = product.UnitPrice,
                        Variant = product.Variant,
                        Quantity = product.Quantity > 0 ? product.Quantity : 1
                    });
                }

                var settings = new BarcodeSettingsModel
                {
                    BarcodeType = request.BarcodeType,
                    BarcodeWidth = request.BarcodeWidth,
                    BarcodeHeight = request.BarcodeHeight,
                    PaperSize = request.PaperSize,
                    ShowProductName = request.ShowProductName,
                    ShowPrice = request.ShowPrice,
                    ShowCompanyName = request.ShowCompanyName,
                    CompanyName = request.CompanyName
                };

                // Generate PDF
                var pdfBytes = await GenerateBarcodePDF(barcodeItems, settings);

                var result = new BarcodeResult
                {
                    Success = true,
                    Message = "Barcode generated successfully",
                    BarcodeData = pdfBytes,
                    PrintUrl = $"/Admin/Products/PrintBarcodePreview?data={HttpUtility.UrlEncode(JsonSerializer.Serialize(request))}",
                    DownloadUrl = $"/Admin/Products/DownloadBarcode?data={HttpUtility.UrlEncode(JsonSerializer.Serialize(request))}"
                };

                return result;
            }
            catch (Exception ex)
            {
                return new BarcodeResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

    }
}