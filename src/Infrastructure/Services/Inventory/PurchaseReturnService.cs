using ApplicationCore.Common;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Accounting;
using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Inventory;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ApplicationCore.Entities.Inventory.PurchaseReturn;
using static Dapper.SqlMapper;

namespace Infrastructure.Services.Inventory
{
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IDapperService<PurchaseReturn> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;
        private SqlTransaction transaction = null;

        public PurchaseReturnService(IDapperService<PurchaseReturn> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<PurchaseReturn> GetInitial()
        {
            try
            {
                PurchaseReturn data = new PurchaseReturn();
                var query = $@" 
                                SELECT Id = OperationalUserId, Text = OP.Name +' | ' + OP.OrganizationName, Description = OP.Address
                                FROM OperationalUser OP
                                WHERE OP.Role = 'SUPPLIER'
                                ORDER BY OperationalUserId DESC;
                                SELECT Id = ItemId, Text = ItemName + ' | ' + ItemCode, Description = ItemName FROM Item ORDER BY ItemId DESC;
                                SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;
								SELECT AttributeId Id, Name Text FROM Attributes;
                                SELECT Code Id, Name Text, Code FROM Color ORDER BY Name ASC;
                                ";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.PurchaseDate = DateTime.Now;
                data.SupplierList = queryResult.Read<Select2OptionModel>().ToList();
                data.ProductList = queryResult.Read<Select2OptionModel>().ToList();
                data.UnitList = queryResult.Read<Select2OptionModel>().ToList();
                data.AttributeList = queryResult.Read<Select2OptionModel>().ToList();
                data.ColorList = queryResult.Read<ColorSelect2Option>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<PurchaseReturn>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY pu.PurchaseRetuenId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT pu.PurchaseRetuenId, pu.ProductImage, pu.PurchaseDate, it.ItemName, SUM(pt.Quantity) AS TotalQty,pu.PurchasePrice
                                FROM PurchaseReturns pu
                                LEFT JOIN PurchaseReturnItems pt ON pu.PurchaseRetuenId = pt.PurchaseId
                                LEFT JOIN Item it ON pu.ProductId = it.ItemId
                                GROUP BY  pu.ProductImage,pu.PurchaseDate,it.ItemName,pu.PurchasePrice,pu.PurchaseRetuenId";

                if (searchBy != "")
                    sql += " AND it.ItemName like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<PurchaseReturn>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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
        public async Task<int> SaveAsync(PurchaseReturn entity, List<PurchaseReturnItem> purchaseItem, List<ProductStock> prodStocks)
        {
            try
            {
                int ledgerId = await GetLedgerIdByOperationalUser(entity.SupplierId);
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                int id = await _service.SaveSingleAsync(entity, transaction);

                purchaseItem?.ForEach(item =>
                {
                    item.PurchaseId = id;
                });
                await _service.SaveAsync(purchaseItem, transaction);

                if (prodStocks.Count > 0)
                {
                    await _service.SaveAsync(prodStocks, transaction);
                }

                await SaveVoucherAsync(entity, ledgerId, transaction);

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

        public async Task<ProductStock> GetProductStockAsync(int productId, string variant)
        {

            string query = @"SELECT * FROM ProductStocks WHERE ProductId = @ProductId AND Variant = @Variant";

            return await _connection.QueryFirstOrDefaultAsync<ProductStock>(query, new { ProductId = productId, Variant = variant });
        }

        public async Task UpdateProductStockAsync(ProductStock productStock)
        {

            string query = @"
        UPDATE ProductStocks 
        SET Quantity = @Quantity, Updated_By = @UpdatedBy, Updated_At = @UpdatedAt 
        WHERE ProductId = @ProductId AND Variant = @Variant";

            await _connection.ExecuteAsync(query, new
            {
                Quantity = productStock.Quantity,
                UpdatedBy = productStock.Updated_By,
                UpdatedAt = DateTime.UtcNow,
                ProductId = productStock.ProductId,
                Variant = productStock.Variant
            });
        }

        public async Task<PurchaseReturn> GetAsync(int id)
        {
            var query = $@"SELECT PurchaseRetuenId,sp.Address, ProductId, ProductCode, SupplierId, UnitId, BranchId, PurchasePrice, RegularPrice, SalePrice, Expanse, 
                           Colors, TotalQty, ProductImage, UserId, Attributes, VariantProduct, ChoiceOptions, Variations,dbo.GetLocalDate(PurchaseDate)PurchaseDate
                           FROM PurchaseReturns pu LEFT JOIN OperationalUser sp ON pu.SupplierId=sp.OperationalUserId  WHERE PurchaseRetuenId = {id};

                            -- dropdown list

                            SELECT CAST(UnitId AS VARCHAR)  Id, Name as Text FROM Units;

                            SELECT CAST(AttributeId AS VARCHAR) Id, Name Text FROM Attributes;

                            SELECT CAST(Code AS VARCHAR) Id, Name Text, Code FROM Color ORDER BY Name ASC;

			                SELECT * FROM ProductStocks WHERE ProductId=( SELECT P.ProductId FROM Purchases P LEFT JOIN PurchaseReturns pt ON p.Id=pt.PurchaseId  WHERE pt.PurchaseRetuenId={id} );

                            SELECT
                               pri.Id,
                               pri.VariantName as variant, 
                               pri.VariantPrice as variantPrice, 
                               pri.Quantity,
                           	   COALESCE(pri.PurchaseQuantity, 0) AS PurchaseQuantity,
                               COALESCE((
                                   SELECT SUM(Quantity) 
                                   FROM PurchaseReturnItems 
                                   WHERE VariantName = pri.VariantName 
                                   AND PurchaseId IN (
                                       SELECT PurchaseRetuenId 
                                       FROM PurchaseReturns 
                                       WHERE PurchaseId IN (
                                           SELECT PurchaseId 
                                           FROM PurchaseReturns 
                                           WHERE PurchaseRetuenId = {id}
                                       )
                                   )
                               ), 0) AS alreadyReturnedQuantity
                           FROM PurchaseReturns pr
                           LEFT JOIN PurchaseReturnItems pri 
                           ON pr.PurchaseRetuenId = pri.PurchaseId
                           WHERE pri.PurchaseId = {id}
                           GROUP BY pri.VariantName, pri.VariantPrice, pri.Quantity,pri.PurchaseQuantity,pri.Id;

                            SELECT Id = OperationalUserId, Text = OP.Name +' | ' + OP.OrganizationName, Description = OP.Address
                            FROM OperationalUser OP
                            WHERE OP.Role = 'SUPPLIER'
                            ORDER BY OperationalUserId DESC;

                            SELECT Id = ItemId, Text = ItemName + ' | ' + ItemCode, Description = ItemName FROM Item ORDER BY ItemId DESC;
                               
                           
                            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                PurchaseReturn data = queryResult.Read<PurchaseReturn>().FirstOrDefault();
                data.UnitList = queryResult.Read<Select2OptionModel>().ToList();
                data.AttributeList = queryResult.Read<Select2OptionModel>().ToList();
                data.ColorList = queryResult.Read<ColorSelect2Option>().ToList();
                //data.PhotoSourceList = queryResult.Read<Select2OptionModel>().ToList();
                data.ProductStocks = queryResult.Read<ProductStock>().ToList();
                data.PurchaseItemDTO = queryResult.Read<PurchaseRtnItemDto>().ToList();
                data.SupplierList = queryResult.Read<Select2OptionModel>().ToList();
                data.ProductList = queryResult.Read<Select2OptionModel>().ToList();


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


        public async Task<PurchaseReturn> GetPurchaseForEditAsync(int id)
        {
            var query = $@"SELECT Id as PurchaseId,sp.Address, ProductId, ProductCode, SupplierId, UnitId, BranchId, PurchasePrice, RegularPrice, SalePrice, Expanse, 
                           Colors, TotalQty, ProductImage, UserId, Attributes, VariantProduct, ChoiceOptions, Variations,dbo.GetLocalDate(PurchaseDate)PurchaseDate
                           FROM Purchases pu LEFT JOIN OperationalUser sp ON pu.SupplierId=sp.OperationalUserId  WHERE Id = {id};

                            -- dropdown list

                            SELECT CAST(UnitId AS VARCHAR)  Id, Name as Text FROM Units;

                            SELECT CAST(AttributeId AS VARCHAR) Id, Name Text FROM Attributes;

                            SELECT CAST(Code AS VARCHAR) Id, Name Text, Code FROM Color ORDER BY Name ASC;

                           SELECT * FROM ProductStocks WHERE ProductId=(Select ProductId from Purchases pa where pa.Id={id});

                            SELECT  
                                pt.Id, 
                                pt.PurchaseId, 
                                pt.VariantName AS Variant, 
                                pt.Quantity AS PurchaseQuantity, 
                                COALESCE(rt.Quantity, 0) AS AlreadyReturnedQuantity, 
                                pt.VariantPrice AS VariantPrice
                            FROM  
                                PurchaseItems pt
                            LEFT JOIN 
                                (SELECT 
                                     pr.PurchaseId, 
                                     pir.VariantName, 
                                     SUM(pir.Quantity) AS Quantity 
                                 FROM PurchaseReturns pr  
                                 INNER JOIN PurchaseReturnItems pir ON pir.PurchaseId = pr.PurchaseRetuenId
                                 GROUP BY pr.PurchaseId, pir.VariantName
                                ) rt 
                            ON  
                                pt.PurchaseId = rt.PurchaseId 
                                AND pt.VariantName = rt.VariantName
                            WHERE 
                                pt.PurchaseId={id};
                            
                            SELECT Id = OperationalUserId, Text = OP.Name +' | ' + OP.OrganizationName, Description = OP.Address
                            FROM OperationalUser OP
                            WHERE OP.Role = 'SUPPLIER'
                            ORDER BY OperationalUserId DESC;

                            SELECT Id = ItemId, Text = ItemName + ' | ' + ItemCode, Description = ItemName FROM Item ORDER BY ItemId DESC;
                               
                            SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;
                            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                PurchaseReturn data = queryResult.Read<PurchaseReturn>().FirstOrDefault();
                data.UnitList = queryResult.Read<Select2OptionModel>().ToList();
                data.AttributeList = queryResult.Read<Select2OptionModel>().ToList();
                data.ColorList = queryResult.Read<ColorSelect2Option>().ToList();
                //data.PhotoSourceList = queryResult.Read<Select2OptionModel>().ToList();
                data.ProductStocks = queryResult.Read<ProductStock>().ToList();
                data.PurchaseItemDTO = queryResult.Read<PurchaseRtnItemDto>().ToList();
                data.SupplierList = queryResult.Read<Select2OptionModel>().ToList();
                data.ProductList = queryResult.Read<Select2OptionModel>().ToList();
                data.UnitList = queryResult.Read<Select2OptionModel>().ToList();

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


        public async Task<PurchaseReturn> GetPurchaseAsync(int id)
        {

            var query = $@"
                        SELECT P.* FROM PurchaseReturns P WHERE P.PurchaseRetuenId={id};
                      SELECT * FROM PurchaseReturnItems WHERE PurchaseId={id}";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                PurchaseReturn data = queryResult.Read<PurchaseReturn>().FirstOrDefault();
                data.Items = queryResult.Read<PurchaseReturnItem>().ToList();

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

        public async Task<List<ProductStock>> GetPurchaseStockAsync(int purchaseId)
        {
            string query = @" SELECT * FROM ProductStocks WHERE ProductId=( SELECT P.ProductId FROM Purchases P WHERE P.Id=@PurchaseId );";

            return (List<ProductStock>)await _connection.QueryAsync<ProductStock>(query, new { PurchaseId = purchaseId });
        }
        public async Task<List<ProductStock>> GetRtnEditStockAsync(int purchaseRetuenId)
        {
            string query = @" SELECT * FROM ProductStocks WHERE ProductId=( SELECT P.ProductId FROM Purchases P LEFT JOIN PurchaseReturns pt ON p.Id=pt.PurchaseId  WHERE pt.PurchaseRetuenId=@PurchaseRetuenId)";

            return (List<ProductStock>)await _connection.QueryAsync<ProductStock>(query, new { PurchaseRetuenId = purchaseRetuenId });
        }


        public async Task<int> UpdateAsync(PurchaseReturn entity)
        {
            try
            {
                int ledgerId = await GetLedgerIdByOperationalUser(entity.SupplierId);

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Items, transaction);

                await DeleteVoucher(entity.InvoiceNumber, transaction);
                await SaveVoucherAsync(entity, ledgerId, transaction);

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

        public async Task<List<PurchaseReturnItem>> GetPurchaseItemsByPurchaseIdAsync(int purchaseId)
        {
            string query = @"
                           SELECT Id, PurchaseId, VariantName, VariantPrice, Quantity
                           FROM PurchaseReturnItems
                           WHERE PurchaseId = @PurchaseId";

            return (List<PurchaseReturnItem>)await _connection.QueryAsync<PurchaseReturnItem>(query, new { PurchaseId = purchaseId });
        }

        public async Task SaveVoucherAsync(PurchaseReturn entity, int ledgerId, SqlTransaction transaction)
        {
            AccountVoucher accountVoucher = new AccountVoucher()
            {
                AccouVoucherTypeAutoID = (int)AccountVoucherType.RECEIEVED,
                VoucherNumber = entity.InvoiceNumber,
                VoucherDate = DateTime.Now,
                BranchId = entity.BranchId,
                SupplierId = entity.SupplierId,
                IsActive = true,
                AccountType = 2, //supplier
                AccountLedgerId = 34, // Supplier Payment
                Created_At = DateTime.Now,
                Created_By = entity.Created_By,
                EntityState = EntityState.Added,
                IpAddress = Common.GetIpAddress()
            };

            var accountVoucherId = await _service.SaveSingleAsync<AccountVoucher>(accountVoucher, transaction);

            accountVoucher.AccountVoucherDetails.Add(new AccountVoucherDetails
            {
                AccountVoucherId = accountVoucherId,
                ChildId = accountVoucher.AccountLedgerId,
                CreditAmount = (decimal)entity.ReturnPrice,
                TypeId = AmountType.DEBIT_AMOUNT,
                IsActive = true,
                VoucherDate = DateTime.Now,
                BranchId = entity.BranchId,
                Created_At = DateTime.Now,
                Created_By = entity.Created_By,
                EntityState = EntityState.Added
            });

            accountVoucher.AccountVoucherDetails.Add(new AccountVoucherDetails
            {
                AccountVoucherId = accountVoucherId,
                ChildId = ledgerId,
                DebitAmount = (decimal)entity.ReturnPrice,
                TypeId = AmountType.CREDIT_AMOUNT,
                IsActive = true,
                VoucherDate = DateTime.Now,
                BranchId = entity.BranchId,
                Created_At = DateTime.Now,
                Created_By = entity.Created_By,
                EntityState = EntityState.Added
            });

            await _service.SaveAsync<AccountVoucherDetails>(accountVoucher.AccountVoucherDetails, transaction);
        }

        public async Task<int> GetLedgerIdByOperationalUser(int id)
        {
            var query = $@"SELECT Id FROM AccountLedger WHERE RelatedId = {id}";
            return await _service.GetSingleIntFieldAsync(query);
        }

        public async Task DeleteVoucher(string voucherNumber, SqlTransaction transaction)
        {
            var query = $@"
                DELETE FROM AccountVoucherDetails WHERE AccountVoucherId = (SELECT AccountVoucherId FROM AccountVoucher WHERE VoucherNumber = (SELECT InvoiceNumber FROM PurchaseReturn WHERE InvoiceNumber = @voucherNumber));
                DELETE FROM AccountVoucher WHERE VoucherNumber = (SELECT InvoiceNumber FROM PurchaseReturn WHERE InvoiceNumber = @voucherNumber)
                ";
            await _service.ExecuteQueryAsync(query, new { voucherNumber }, transaction);
        }

    }
}

