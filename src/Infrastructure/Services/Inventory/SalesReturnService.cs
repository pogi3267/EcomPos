using ApplicationCore.Entities;
using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
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
    public class SalesReturnService : ISalesReturnService
    {
        private readonly IDapperService<SalseReturn> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction = null;
        private SqlTransaction transaction = null;

        public SalesReturnService(IDapperService<SalseReturn> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<SalseReturn> GetInitial()
        {
            try
            {
                SalseReturn data = new SalseReturn();
                var query = $@" 
                                SELECT Id = OperationalUserId, Text = OP.Name +' | ' + OP.OrganizationName, Description = OP.Address
                                FROM OperationalUser OP
                                WHERE OP.Role = 'CUSTOMER'
                                ORDER BY OperationalUserId DESC;
                                SELECT Id = ItemId, Text = ItemName + ' | ' + ItemCode, Description = ItemName FROM Item ORDER BY ItemId DESC;
                                SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;
                                ";
                var queryResult = await _connection.QueryMultipleAsync(query);
                data.SaleReturnDate = DateTime.Now;
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
        public async Task<List<SalseReturn>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY pu.SaleReturnId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT op.Name as CustomerName, pu.SaleReturnId, pu.ProductImage, pu.SaleReturnDate,SUM(pt.Quantity) AS TotalQty,SUM(pt.SalePrice) AS SalseReturnPrice
                                FROM SalesReturns pu
                                LEFT JOIN SalesReturnItems pt ON pu.SaleReturnId = pt.SaleReturnId
                              	LEFT JOIN OperationalUser op ON pu.CustomerId=op.OperationalUserId
                                GROUP BY  pu.SaleReturnId,pu.ProductImage,pu.SaleReturnDate,op.Name";

                if (searchBy != "")
                    sql += " AND op.Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<SalseReturn>(sql);
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
        public async Task<int> SaveAsync(SalseReturn entity, List<SalseReturnItem> salseItem, List<ProductStock> prodStocks)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                int id = await _service.SaveSingleAsync(entity, transaction);

                salseItem?.ForEach(item =>
                {
                    item.SaleReturnId = id;
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

        public async Task<SalseReturn> SalesReturnLoad(int id)
        {
            var query = $@"SELECT  SalseId as SaleId,sp.Address, ProductId, ProductCode, CustomerId, UnitId, BranchId, RegularPrice, Expanse, 
                           TotalQty, ProductImage, UserId,dbo.GetLocalDate(SalseDate)SalseDate
                           FROM Salses pu LEFT JOIN OperationalUser sp ON pu.CustomerId=sp.OperationalUserId  WHERE SalseId = {id};
                                                   
                           SELECT 
                           si.Id, 
                           si.SalseId, 
                           si.ProductId, 
                           si.VariantId, 
                           si.UnitId, 
                           si.Quantity, 
                           si.SalePrice, 
                           si.VariantName, 
                           it.ItemName AS ProductName, 
                           u.Name AS UnitName,
                           COALESCE(rt.Quantity, 0) AS AlreadyReturnedQuantity
                           FROM 
                           SalseItems si 
                          LEFT JOIN Item it ON si.ProductId = it.ItemId
                          LEFT JOIN Units u ON si.UnitId = u.UnitId
                          LEFT JOIN (
                              SELECT 
                                  sr.SaleId, 
                                  sri.VariantName, 
                                  SUM(sri.Quantity) AS Quantity 
                              FROM SalesReturns sr  
                              INNER JOIN SalesReturnItems sri ON sri.SaleReturnId = sr.SaleReturnId
                              GROUP BY sr.SaleId, sri.VariantName
                          ) rt ON si.SalseId = rt.SaleId AND si.VariantName = rt.VariantName
							Where SalseId = {id};

                       -- dropdown list
                            SELECT Id = OperationalUserId, Text = OP.Name +' | ' + OP.OrganizationName, Description = OP.Address
                            FROM OperationalUser OP
                            WHERE OP.Role = 'CUSTOMER'
                            ORDER BY OperationalUserId DESC;

                            SELECT Id = ItemId, Text = ItemName + ' | ' + ItemCode, Description = ItemName FROM Item ORDER BY ItemId DESC;

                            SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;
                            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                SalseReturn data = queryResult.Read<SalseReturn>().FirstOrDefault();
                data.Items = queryResult.Read<SalseReturnItem>().ToList();
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

        public async Task<SalseReturn> GetAsync(int id)
        {
            var query = $@"SELECT  SaleReturnId,sp.Address, ProductId, CustomerId, UnitId, BranchId, RegularPrice, Expanse, 
                           TotalQty, ProductImage, UserId,dbo.GetLocalDate(SaleReturnDate)SalseReturnDate
                                                     FROM SalesReturns pu LEFT JOIN OperationalUser sp ON pu.CustomerId=sp.OperationalUserId  WHERE SaleReturnId = {id};
                                                   
                              SELECT
                              sri.Id,
                              it.ItemName AS ProductName,
                          	   sri.ProductId,
                              sri.VariantName AS VariantName,
                              u.Name AS UnitName,
                              sri.UnitId,
                              sri.Quantity,
                              (
                                  SELECT SUM(si.Quantity)
                                  FROM [dbo].[SalseItems] si
                                  WHERE si.SalseId IN (
                                      SELECT sr.SaleId FROM SalesReturns sr WHERE sr.SaleReturnId = sri.SaleReturnId
                                  ) 
                                  AND si.ProductId = sri.ProductId 
                                  AND si.VariantId = (
                                      SELECT TOP 1 si2.VariantId 
                                      FROM [dbo].[SalseItems] si2 
                                      WHERE si2.ProductId = sri.ProductId AND si2.VariantName = sri.VariantName
                                      ORDER BY si2.Id ASC
                                  )
                              ) AS SaleQuantity,
                              (COALESCE((
                                  SELECT SUM(sri2.Quantity)
                                  FROM SalesReturnItems sri2
                                  WHERE sri2.VariantName = sri.VariantName
                                  AND sri2.ProductId = sri.ProductId
                                  AND sri2.SaleReturnId IN (
                                      SELECT sr2.SaleReturnId 
                                      FROM SalesReturns sr2
                                      WHERE sr2.SaleId = sr.SaleId
                                  )
                              ), 0)- sri.Quantity) AS AlreadyReturnedQuantity,
                              sri.SalePrice,
                              sri.SaleReturnId,
                              sri.Created_At  -- Include Created_At timestamp to check return time
                          FROM SalesReturnItems sri
                          LEFT JOIN Item it ON sri.ProductId = it.ItemId
                          LEFT JOIN Units u ON sri.UnitId = u.UnitId
                          LEFT JOIN SalesReturns sr ON sri.SaleReturnId = sr.SaleReturnId
                          WHERE sri.SaleReturnId = {id}
                          ORDER BY sri.Created_At DESC, sri.Id;

                       -- dropdown list
                            SELECT Id = OperationalUserId, Text = OP.Name +' | ' + OP.OrganizationName, Description = OP.Address
                            FROM OperationalUser OP
                            WHERE OP.Role = 'CUSTOMER'
                            ORDER BY OperationalUserId DESC;

                            SELECT Id = ItemId, Text = ItemName + ' | ' + ItemCode, Description = ItemName FROM Item ORDER BY ItemId DESC;

                            SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;
                            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                SalseReturn data = queryResult.Read<SalseReturn>().FirstOrDefault();
                data.Items = queryResult.Read<SalseReturnItem>().ToList();
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

        public async Task<SalseReturn> GetSalseReturnAsync(int id)
        {

            var query = $@"
                        SELECT P.* FROM SalesReturns P WHERE P.SaleReturnId={id};
                        SELECT si.* FROM SalesReturnItems si WHERE si.SaleReturnId={id};";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                SalseReturn data = queryResult.Read<SalseReturn>().FirstOrDefault();
                data.Items = queryResult.Read<SalseReturnItem>().ToList();

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

        public async Task<int> UpdateAsync(SalseReturn entity,List<ProductStock> prodStocks)
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

        public async Task<List<SalseReturnItem>> GetSalseReturnItemsBySalseReturnIdAsync(int salseId)
        {
            string query = @"
                           SELECT Id, SalseReturnId, VariantName, VariantPrice, Quantity
                           FROM SalseReturnItems
                           WHERE SalseReturnId = @SalseReturnId";

            return (List<SalseReturnItem>)await _connection.QueryAsync<SalseReturnItem>(query, new { SalseReturnId = salseId });
        }
        public async Task<SalseReturn> GetProductVariantList(int id)
        {
            SalseReturn data = new SalseReturn();
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



    }
}

