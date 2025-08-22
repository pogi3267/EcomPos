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
using static Dapper.SqlMapper;

namespace Infrastructure.Services.Inventory
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IDapperService<Purchase> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;
        private SqlTransaction transaction = null;

        public PurchaseService(IDapperService<Purchase> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<Purchase> GetInitial()
        {
            try
            {
                Purchase data = new Purchase();
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
        public async Task<List<Purchase>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY pu.Id DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT pu.Id, pu.ProductImage, pu.PurchaseDate, it.ItemName, SUM(pt.Quantity) AS TotalQty,pu.PurchasePrice
                                FROM Purchases pu
                                LEFT JOIN PurchaseItems pt ON pu.Id = pt.PurchaseId
                                LEFT JOIN Item it ON pu.ProductId = it.ItemId
                                GROUP BY  pu.ProductImage,pu.PurchaseDate,it.ItemName,pu.PurchasePrice,pu.Id";

                if (searchBy != "")
                    sql += " AND it.ItemName like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Purchase>(sql);
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
        public async Task<int> SaveAsync(Purchase entity, List<PurchaseItem> purchaseItem, List<ProductStock> prodStocks)
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
                await SaveVoucherAsync(entity, ledgerId, _transaction);

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

        public async Task<Purchase> GetAsync(int id)
        {
            var query = $@"SELECT Id ,sp.Address, ProductId, ProductCode, SupplierId, UnitId, BranchId, PurchasePrice, RegularPrice, SalePrice, Expanse, 
                           Colors, TotalQty, ProductImage , UserId, Attributes, VariantProduct, ChoiceOptions, Variations,dbo.GetLocalDate(PurchaseDate)PurchaseDate
                           FROM Purchases pu LEFT JOIN OperationalUser sp ON pu.SupplierId=sp.OperationalUserId  WHERE Id = {id};

                            -- dropdown list

                            SELECT CAST(UnitId AS VARCHAR)  Id, Name as Text FROM Units;

                            SELECT CAST(AttributeId AS VARCHAR) Id, Name Text FROM Attributes;

                            SELECT CAST(Code AS VARCHAR) Id, Name Text, Code FROM Color ORDER BY Name ASC;

                            SELECT * FROM ProductStocks WHERE ProductId={id};

                            SELECT Id,PurchaseId,VariantName as Variant,Quantity ,VariantPrice as Price FROM PurchaseItems WHERE PurchaseId={id};

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
                Purchase data = queryResult.Read<Purchase>().FirstOrDefault();
                data.UnitList = queryResult.Read<Select2OptionModel>().ToList();
                data.AttributeList = queryResult.Read<Select2OptionModel>().ToList();
                data.ColorList = queryResult.Read<ColorSelect2Option>().ToList();
                //data.PhotoSourceList = queryResult.Read<Select2OptionModel>().ToList();
                data.ProductStocks = queryResult.Read<ProductStock>().ToList();
                data.PurchaseItemDTO = queryResult.Read<PurchaseItemDTO>().ToList();
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

        public async Task<Purchase> GetPurchaseAsync(int id)
        {

            var query = $@"
                        SELECT P.* FROM Purchases P WHERE P.Id={id};
                        SELECT P.* FROM PurchaseItems P WHERE P.PurchaseId={id};
                        SELECT * FROM ProductStocks WHERE ProductId=( SELECT P.ProductId FROM Purchases P WHERE P.Id={id} );";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Purchase data = queryResult.Read<Purchase>().FirstOrDefault();
                data.Items = queryResult.Read<PurchaseItem>().ToList();
                data.ProductStocks = queryResult.Read<ProductStock>().ToList();

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

        public async Task<int> UpdateAsync(Purchase entity, List<ProductStock> productStocks)
        {
            try
            {
                int ledgerId = await GetLedgerIdByOperationalUser(entity.SupplierId);
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Items, transaction);
                await _service.SaveAsync(productStocks, transaction);
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

        public async Task<List<PurchaseItem>> GetPurchaseItemsByPurchaseIdAsync(int purchaseId)
        {
            string query = @"
                           SELECT Id, PurchaseId, VariantName, VariantPrice, Quantity
                           FROM PurchaseItems
                           WHERE PurchaseId = @PurchaseId";

            return (List<PurchaseItem>)await _connection.QueryAsync<PurchaseItem>(query, new { PurchaseId = purchaseId });
        }

        public async Task SaveVoucherAsync(Purchase entity, int ledgerId, SqlTransaction transaction)
        {
            AccountVoucher accountVoucher = new AccountVoucher()
            {
                AccouVoucherTypeAutoID = (int)AccountVoucherType.PAYMENT,
                VoucherNumber = entity.InvoiceNumber,
                VoucherDate = DateTime.Now,
                BranchId = entity.Items.FirstOrDefault().BranchId,
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
                CreditAmount = (decimal)entity.GrandTotalAmount,
                TypeId = AmountType.CREDIT_AMOUNT,
                IsActive = true,
                VoucherDate = DateTime.Now,
                BranchId = entity.Items.FirstOrDefault().BranchId,
                Created_At = DateTime.Now,
                Created_By = entity.Created_By,
                EntityState = EntityState.Added
            });

            accountVoucher.AccountVoucherDetails.Add(new AccountVoucherDetails
            {
                AccountVoucherId = accountVoucherId,
                ChildId = ledgerId,
                DebitAmount = (decimal)entity.GrandTotalAmount,
                TypeId = AmountType.DEBIT_AMOUNT,
                IsActive = true,
                VoucherDate = DateTime.Now,
                BranchId = entity.Items.FirstOrDefault().BranchId,
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
                DELETE FROM AccountVoucherDetails WHERE AccountVoucherId = (SELECT AccountVoucherId FROM AccountVoucher WHERE VoucherNumber = (SELECT PayInvoiceNo FROM Payment WHERE PayInvoiceNo = @voucherNumber));
                DELETE FROM AccountVoucher WHERE VoucherNumber = (SELECT PayInvoiceNo FROM Payment WHERE PayInvoiceNo = @voucherNumber)
                ";
            await _service.ExecuteQueryAsync(query, new { voucherNumber }, transaction);
        }



    }
}

