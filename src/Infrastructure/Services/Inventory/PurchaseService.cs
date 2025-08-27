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

namespace Infrastructure.Services.Inventory;

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
                    COUNT(I.PurchaseId) TotalItems, SUM(I.Quantity) TotalQty
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
            throw ex;
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
        catch (Exception ex)
        {
            throw ex;
        }
    }
    

}

