using ApplicationCore.Common;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Accounting;
using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Marketing;
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
    public class SalseService : ISalseService
    {
        private readonly IDapperService<Salse> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;
        private SqlTransaction transaction = null;

        public SalseService(IDapperService<Salse> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<Salse> GetInitial()
        {
            try
            {
                Salse data = new Salse();
                var query = $@" 
                                SELECT Id = OperationalUserId, Text = OP.Name +' | ' + OP.OrganizationName, Description = OP.Address
                                FROM OperationalUser OP
                                WHERE OP.Role = 'CUSTOMER'
                                ORDER BY OperationalUserId DESC;

                               SELECT 
                               pr.ProductId AS Id, 
                               (it.ItemName + ' | ' + it.ItemCode) AS Text, 
                               MAX(pr.ProductImage) AS Description
                               FROM Purchases pr 
                               LEFT JOIN Item it ON pr.ProductId = it.ItemId 
                               GROUP BY pr.ProductId, it.ItemName, it.ItemCode;
                                SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;
                                ";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.SalseDate = DateTime.Now;
                data.CustomerList = queryResult.Read<Select2OptionModel>().ToList();
                data.ProductList = queryResult.Read<Select2OptionModel>().ToList();
                data.UnitList = queryResult.Read<Select2OptionModel>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<Salse>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY pu.SalseId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT op.Name as CustomerName, pu.SalseId, pu.ProductImage, pu.SalseDate,SUM(pt.Quantity) AS TotalQty,SUM(pt.SalePrice) AS SalsePrice
                                FROM Salses pu
                                LEFT JOIN SalseItems pt ON pu.SalseId = pt.SalseId
								LEFT JOIN OperationalUser op ON pu.CustomerId=op.OperationalUserId
                                GROUP BY  pu.SalseId,pu.ProductImage,pu.SalseDate,op.Name";

                if (searchBy != "")
                    sql += " AND op.Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Salse>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ProductStock> GetStockValue(string productId, string variantName)
        {
            string sql = @" SELECT Quantity  FROM ProductStocks 
                            WHERE ProductId = @ProductId AND Variant = @VariantName";

            return await _connection.QueryFirstOrDefaultAsync<ProductStock>(sql, new { ProductId = productId, VariantName = variantName });
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
        public async Task<int> SaveAsync(Salse entity, List<SalseItem> salseItem, List<ProductStock> prodStocks)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                int id = await _service.SaveSingleAsync(entity, transaction);

                salseItem?.ForEach(item =>
                {
                    item.SalseId = id;
                });
                await _service.SaveAsync(salseItem, transaction);

                if (prodStocks.Count > 0)
                {
                    await _service.SaveAsync(prodStocks, transaction);
                }


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

        public async Task<Salse> GetAsync(int id)
        {
            var query = $@"SELECT  SalseId,sp.Address, ProductId, ProductCode, CustomerId, UnitId, BranchId, RegularPrice, Expanse, 
                           TotalQty, ProductImage, UserId,dbo.GetLocalDate(SalseDate)SalseDate
                           FROM Salses pu LEFT JOIN OperationalUser sp ON pu.CustomerId=sp.OperationalUserId  WHERE SalseId = {id};
                         
                           select Id, SalseId, ProductId, VariantId, si.UnitId, Quantity, SalePrice, VariantName, it.ItemName as ProductName,u.Name as UnitName
					       from SalseItems si 
							LEFT JOIN Item it ON si.ProductId=it.ItemId
							LEFT JOIN Units u ON si.UnitId=u.UnitId
							Where SalseId = {id}

                       -- dropdown list
                            SELECT Id = OperationalUserId, Text = OP.Name +' | ' + OP.OrganizationName, Description = OP.Address
                            FROM OperationalUser OP
                            WHERE OP.Role = 'CUSTOMER'
                            ORDER BY OperationalUserId DESC;

                            SELECT 
                               pr.ProductId AS Id, 
                               (it.ItemName + ' | ' + it.ItemCode) AS Text, 
                               MAX(pr.ProductImage) AS Description
                               FROM Purchases pr 
                               LEFT JOIN Item it ON pr.ProductId = it.ItemId 
                               GROUP BY pr.ProductId, it.ItemName, it.ItemCode;
                                SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;

                            SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;
                            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Salse data = queryResult.Read<Salse>().FirstOrDefault();
                data.Items = queryResult.Read<SalseItem>().ToList();
                data.CustomerList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<Salse> GetSalseAsync(int id)
        {

            var query = $@"
                        SELECT P.* FROM Salses P WHERE P.SalseId={id};
                        SELECT si.* FROM SalseItems si WHERE si.SalseId={id};";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Salse data = queryResult.Read<Salse>().FirstOrDefault();
                data.Items = queryResult.Read<SalseItem>().ToList();

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

        public async Task<int> UpdateAsync(Salse entity,List<ProductStock> prodStocks)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Items, transaction);
                await _service.SaveAsync(prodStocks, transaction);

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

        public async Task<List<SalseItem>> GetSalseItemsBySalseIdAsync(int salseId)
        {
            string query = @"
                           SELECT Id, SalseId, VariantName, VariantPrice, Quantity
                           FROM SalseItems
                           WHERE SalseId = @SalseId";

            return (List<SalseItem>)await _connection.QueryAsync<SalseItem>(query, new { SalseId = salseId });
        }
        public async Task<Salse> GetProductVariantList(int id)
        {
            Salse data = new Salse();
            var query = $@"select Id =ProductStockId, Text = Variant,Description = Quantity from ProductStocks Where ProductId={id};";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.VariantList = queryResult.Read<Select2OptionModel>().ToList();

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

        public async Task SaveVoucherAsync(Salse entity, SqlTransaction transaction)
        {
            AccountVoucher accountVoucher = new AccountVoucher()
            {
                AccouVoucherTypeAutoID = (int)AccountVoucherType.RECEIEVED,
                VoucherNumber = entity.SalesNo,
                VoucherDate = DateTime.Now,
                BranchId = entity.BranchId,
                CustomerId = entity.CustomerId,
                IsActive = true,
                AccountType = 3, //Customer
                AccountLedgerId = 7, // Current Asset
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
                CreditAmount = (decimal)(entity.Expanse + entity.NetTotalAmount),
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
                ChildId = 2074, //Customer Receivable
                DebitAmount = (decimal)(entity.Expanse + entity.NetTotalAmount),
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
            var query = $@"SELECT ISNULL(Id, 0) Id FROM AccountLedger WHERE RelatedId = {id}";
            return await _service.GetSingleIntFieldAsync(query);
        }

        public async Task DeleteVoucher(string voucherNumber, SqlTransaction transaction)
        {
            var query = $@"
                DELETE FROM AccountVoucherDetails WHERE AccountVoucherId = (SELECT AccountVoucherId FROM AccountVoucher WHERE VoucherNumber = (SELECT SalesNo FROM Sales WHERE SalesNo = @voucherNumber));
                DELETE FROM AccountVoucher WHERE VoucherNumber = (SELECT SalesNo FROM Sales WHERE SalesNo = @voucherNumber)
                ";
            await _service.ExecuteQueryAsync(query, new { voucherNumber }, transaction);
        }



    }
}

