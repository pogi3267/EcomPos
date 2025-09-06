using ApplicationCore.Common;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Accounting;
using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Orders;
using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Inventory;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static iText.IO.Util.IntHashtable;

namespace Infrastructure.Services.Inventory;

public class PurchaseService : IPurchaseService
{
    private readonly IDapperService<Purchase> _service;
    private readonly SqlConnection _connection;
    private SqlTransaction transaction = null;

    public PurchaseService(IDapperService<Purchase> service) : base()
    {
        _service = service;
        _connection = service.Connection;
    }
    public async Task<List<Purchase>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status)
    {
        try
        {
            string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY P.PurchaseId DESC" : "ORDER BY " + sortBy + " " + sortDir;
            string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);

            // Updated query for new purchase structure
            string sql = $@"
                    WITH P AS (
                        SELECT pu.PurchaseId, pu.PurchaseDate, pu.PurchaseNo, pu.SupplierId
                        FROM Purchases pu
                    ),
                    Items AS (
	                    SELECT P.PurchaseId, I.TotalPrice, I.Quantity
	                    FROM PurchaseItems I
	                    INNER JOIN P ON P.PurchaseId = I.PurchaseId
                    )
                    SELECT P.PurchaseId, P.PurchaseDate, P.PurchaseNo, P.SupplierId, op.Name + ' | ' + op.OrganizationName SupplierName,
                    COUNT(I.PurchaseId) TotalItems, SUM(I.Quantity) GrandTotal
                    FROM P
                    INNER JOIN PurchaseItems I ON I.PurchaseId = P.PurchaseId
                    LEFT JOIN OperationalUser op ON P.SupplierId = op.OperationalUserId
                    GROUP BY P.PurchaseId, P.PurchaseDate, P.PurchaseNo, P.SupplierId, op.Name, op.OrganizationName";

            if (!string.IsNullOrEmpty(searchBy))
                sql += " AND (op.Name LIKE '%" + searchBy + "%' OR op.OrganizationName LIKE '%" + searchBy + "%' OR P.PurchaseNo LIKE '%" + searchBy + "%')";

            sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
            var result = await _service.GetDataAsync<Purchase>(sql);
            return result;
        }
        catch (Exception ex)
        {
            throw;
        }
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

                                SELECT Id = ProductId, Text = Name, Description = Name FROM Products ORDER BY Name ASC;

                                SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;

                                SELECT Id = BranchId, Text = BranchName FROM  Branch ORDER BY BranchName ASC;

                                SELECT Id = CostId, Text = [Description] FROM  Cost ORDER BY CostId DESC;
                                ";
            var queryResult = await _connection.QueryMultipleAsync(query);
            data.PurchaseNo = $"PUR-{DateTime.Now:yyMMdd-HHmmss}";
            data.PurchaseDate = DateTime.Now;
            data.SupplierList = queryResult.Read<Select2OptionModel>().ToList();
            data.ProductList = queryResult.Read<Select2OptionModel>().ToList();
            data.UnitList = queryResult.Read<Select2OptionModel>().ToList();
            data.BranchList = queryResult.Read<Select2OptionModel>().ToList();
            data.CostList = queryResult.Read<Select2OptionModel>().ToList();
            return data;
        }
        catch (Exception)
        {
            throw;
        }
    }
    
    public async Task<List<Select2OptionModel>> GetProductVariantsAsync(int productId)
    {
        var sql = @"SELECT Id = Variant_id, Text = Variant FROM ProductVariants WHERE ProductId = @ProductId ORDER BY Variant";
        var result = await _connection.QueryAsync<Select2OptionModel>(sql, new { ProductId = productId });
        return result.ToList();
    }

    public async Task<Purchase> GetPurchaseDetail(int id)
    {
        try
        {
            Purchase data = new Purchase();
            var query = $@" 
                        SELECT * FROM Purchases WHERE PurchaseID = {id};
                        SELECT * FROM PurchaseItems WHERE PurchaseID = {id};
                        SELECT * FROM PurchaseExpense WHERE PurchaseID = {id};
                        ";
            var queryResult = await _connection.QueryMultipleAsync(query);
            data = queryResult.Read<Purchase>().FirstOrDefault();
            data.PurchaseItems = queryResult.Read<PurchaseItem>().ToList();
            data.Expenses = queryResult.Read<PurchaseExpense>().ToList();
            return data;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> SavePurchaseAsync(Purchase purchase, List<PurchaseItem> items, List<PurchaseExpense> expenses, List<PurchaseItem> oldItems)
    {
        try
        {
            int ledgerId = await GetLedgerIdByOperationalUser(purchase.SupplierId);

            await _connection.OpenAsync();
            transaction = _connection.BeginTransaction();

            // Insert/Update master
            int purchaseId;
            if (purchase.PurchaseId > 0)
            {
                await _service.SaveSingleAsync(purchase, transaction);
                purchaseId = purchase.PurchaseId;
            }
            else
            {
                purchaseId = await _service.SaveSingleAsync(purchase, transaction);
            }

            if (items != null && items.Count > 0)
            {
                items.ForEach(i => { i.PurchaseId = purchaseId; });
                await _service.SaveAsync(items, transaction);
            }

            if (expenses != null && expenses.Count > 0)
            {
                expenses.ForEach(e => { e.PurchaseId = purchaseId; });
                await _service.SaveAsync(expenses, transaction);
            }

            var oldDict = oldItems.ToDictionary(i => (i.ProductId, i.VariantId, i.BranchId));
            var newDict = items.ToDictionary(i => (i.ProductId, i.VariantId, i.BranchId));
            foreach (var old in oldDict.Values)
            {
                if (!newDict.ContainsKey((old.ProductId, old.VariantId, old.BranchId)))
                {
                    // Item removed, decrease stock
                    await AdjustStockAsync(old.ProductId, old.VariantId, old.BranchId, -old.Quantity, old.Price, transaction);
                }
            }

            //Handle new or updated items
            foreach (var newItem in newDict.Values)
            {
                if (!oldDict.TryGetValue((newItem.ProductId, newItem.VariantId, newItem.BranchId), out var oldItem))
                {
                    // New item, increase stock
                    await AdjustStockAsync(newItem.ProductId, newItem.VariantId, newItem.BranchId, newItem.Quantity, newItem.Price, transaction);
                }
                else
                {
                    //Existing item, adjust by difference
                    int diff = newItem.Quantity - oldItem.Quantity;
                    if (diff != 0)
                    {
                        await AdjustStockAsync(newItem.ProductId, newItem.VariantId, newItem.BranchId, diff, newItem.Price, transaction);
                    }
                }
            }

            if (purchase.PurchaseId > 0)
            {
                await DeleteVoucher(purchase.PurchaseNo, transaction);
            }
            await SaveVoucherAsync(purchase, ledgerId, transaction);

            transaction.Commit();
            return purchaseId;
        }
        catch (Exception)
        {
            if (transaction != null) transaction.Rollback();
            throw;
        }
        finally
        {
            transaction?.Dispose();
            _connection.Close();
        }
    }

    public async Task AdjustStockAsync(int productId, int? variantId, int branchId, int qtyDiff, decimal price, IDbTransaction transaction)
    {
        string sql = @"
        EXEC AdjustStock_Purchase @ProductId, @VariantId, @BranchId, @QtyDiff, @Price;";

        await _connection.ExecuteAsync(sql, new
        {
            ProductId = productId,
            VariantId = variantId,
            BranchId = branchId,
            QtyDiff = qtyDiff,
            Price = price
        }, transaction);
    }

    public async Task<int> GetLedgerIdByOperationalUser(int id)
    {
        var query = $@"SELECT Id FROM AccountLedger WHERE RelatedId = {id}";
        return await _service.GetSingleIntFieldAsync(query);
    }

    public async Task SaveVoucherAsync(Purchase entity, int ledgerId, SqlTransaction transaction)
    {
        AccountVoucher accountVoucher = new AccountVoucher()
        {
            AccouVoucherTypeAutoID = (int)AccountVoucherType.PAYMENT,
            VoucherNumber = entity.PurchaseNo,
            VoucherDate = DateTime.Now,
            BranchId = entity.PurchaseItems.FirstOrDefault().BranchId,
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
            BranchId = entity.PurchaseItems.FirstOrDefault().BranchId,
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
            BranchId = entity.PurchaseItems.FirstOrDefault().BranchId,
            Created_At = DateTime.Now,
            Created_By = entity.Created_By,
            EntityState = EntityState.Added
        });

        await _service.SaveAsync<AccountVoucherDetails>(accountVoucher.AccountVoucherDetails, transaction);
    }

    public async Task DeleteVoucher(string voucherNumber, SqlTransaction transaction)
    {
        var query = $@"
                DELETE FROM AccountVoucherDetails WHERE AccountVoucherId = (SELECT AccountVoucherId FROM AccountVoucher WHERE VoucherNumber = (SELECT PayInvoiceNo FROM Payment WHERE PayInvoiceNo = @voucherNumber));
                DELETE FROM AccountVoucher WHERE VoucherNumber = (SELECT PayInvoiceNo FROM Payment WHERE PayInvoiceNo = @voucherNumber)
                ";
        await _service.ExecuteQueryAsync(query, new { voucherNumber }, transaction);
    }

    public async Task<Purchase> GetPurchaseAsync(int id)
    {
        try
        {
            var query = $@" 
                        SELECT P.*, op.[Address] SupplierAddress FROM Purchases P
						LEFT JOIN OperationalUser op ON P.SupplierId = op.OperationalUserId
                        WHERE PurchaseID = {id};

                        SELECT PD.*, P.[Name] ProductName, V.Variant VariantName, U.[Name] Unit, B.BranchName
                        FROM PurchaseItems PD
                        INNER JOIN Products P ON P.ProductId = PD.ProductId
                        LEFT JOIN ProductVariants V ON V.Variant_id = PD.VariantId
                        INNER JOIN Units U ON U.UnitId = PD.UnitId
                        INNER JOIN Branch B ON B.BranchId = PD.BranchId
                        WHERE PurchaseID = {id};

                        SELECT PE.*, C.[Description] CostName
                        FROM PurchaseExpense PE
                        LEFT JOIN Cost C ON C.CostId = PE.CostId
                        WHERE PurchaseID = {id};

                        SELECT Id = OperationalUserId, Text = OP.Name +' | ' + OP.OrganizationName, Description = OP.Address
                        FROM OperationalUser OP
                        WHERE OP.Role = 'SUPPLIER'
                        ORDER BY OperationalUserId DESC;

                        SELECT Id = ProductId, Text = Name, Description = Name FROM Products ORDER BY Name ASC;

                        SELECT Id = UnitId, Text = Name FROM  Units ORDER BY UnitId DESC;

                        SELECT Id = BranchId, Text = BranchName FROM  Branch ORDER BY BranchName ASC;

                        SELECT Id = CostId, Text = [Description] FROM  Cost ORDER BY CostId DESC;
                        ";
            var queryResult = await _connection.QueryMultipleAsync(query);
            Purchase data = queryResult.Read<Purchase>().FirstOrDefault();
            data.PurchaseItems = queryResult.Read<PurchaseItem>().ToList();
            data.Expenses = queryResult.Read<PurchaseExpense>().ToList();
            data.SupplierList = queryResult.Read<Select2OptionModel>().ToList();
            data.ProductList = queryResult.Read<Select2OptionModel>().ToList();
            data.UnitList = queryResult.Read<Select2OptionModel>().ToList();
            data.BranchList = queryResult.Read<Select2OptionModel>().ToList();
            data.CostList = queryResult.Read<Select2OptionModel>().ToList();
            return data;
        }
        catch (Exception)
        {
            throw;
        }
    }

}

