using ApplicationCore.DTOs;
using ApplicationCore.Entities.Orders;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class HomeService
    {

        string connectionString;

        public HomeService(string connectionString)
        {
            this.connectionString = connectionString;
        }


        public async Task<HomeBusinessAnalyticsDTO> GetAllBusinessAnalyticsDataAsync(string status)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string sql = $@"EXEC Calculate_Business_Analytics_Stats @Status";
                    var parameters = new { Status = status };
                    var result = await connection.QueryAsync<HomeBusinessAnalyticsDTO>(sql, parameters);
                    connection.Close();
                    return result.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<AdminWalletDTO> GetAllAdminWalletAsync(string status)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string sql = $@"EXEC Calculate_Admin_Wallet @Status";
                    var parameters = new { Status = status };
                    var result = await connection.QueryAsync<AdminWalletDTO>(sql, parameters);
                    connection.Close();
                    return result.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

      
        public async Task<List<SalesDataDTO>> SalesForGraphAsync()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string storedProcedureName = "GetSalesForGraph";
                    var result = await connection.QueryAsync<SalesDataDTO>(
                        storedProcedureName,
                        commandType: CommandType.StoredProcedure
                    );
                    connection.Close();
                    return result.AsList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        public async Task<List<PaymentDataDTO>> PaymentForGraphAsync()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string storedProcedureName = "GetCurrentYearPaymentSummary";
                    var result = await connection.QueryAsync<PaymentDataDTO>(
                        storedProcedureName,
                        commandType: CommandType.StoredProcedure
                    );
                    connection.Close();
                    return result.AsList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<ProductAndCustomerDTO> ProductsAndCustomerAsync()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var sql = @";WITH ThumbnailImages AS ( 
                            SELECT u.UploadId AS ThumbnailImage, u.FileName AS ThumbnailFileName
                            FROM Uploads u  
                            WHERE u.UploadId IN (SELECT ThumbnailImage FROM Products))
                        SELECT TOP 4 P.ProductID, P.Name, P.Description, T.ThumbnailFileName, SUM(OD.Quantity) AS TotalQuantitySold 
                        FROM OrderDetails OD
                        INNER JOIN Products P ON OD.ProductID = P.ProductID
                        LEFT JOIN ThumbnailImages T ON P.ThumbnailImage = T.ThumbnailImage
                        GROUP BY P.ProductID, P.Name, T.ThumbnailFileName, P.Description 
                        ORDER BY TotalQuantitySold DESC;

                        SELECT TOP 4 p.ProductID,p.UnitPrice, p.Name, u.FileName AS ThumbnailFileName, dbo.GetLocalDate(p.Created_At) Created_At, p.Description 
                        FROM Products p
                        LEFT JOIN Uploads u ON p.ThumbnailImage = u.UploadId 
                        ORDER BY p.Created_At DESC;

                        SELECT TOP 4 C.Id,C.Avatar CustomerImg, C.FirstName CustomerName, C.Address, C.PhoneNumber, C.Email, SUM(O.GrandTotal) AS TotalSpent
                        FROM Orders O
                        LEFT JOIN AspNetUsers C ON O.UserId = C.Id
                        GROUP BY C.Id, C.FirstName, C.Address, C.PhoneNumber, C.Email,C.Avatar
                        ORDER BY TotalSpent DESC;";

                    using (var queryResult = await connection.QueryMultipleAsync(sql))
                    {
                        ProductAndCustomerDTO data = new ProductAndCustomerDTO
                        {
                            TopSellingItems = queryResult.Read<ProductDTO>().ToList(),
                            RecentlyCreatedProducts = queryResult.Read<ProductDTO>().ToList(),
                            TopCustomer = queryResult.Read<CustomerDTO>().ToList()
                        };
                        connection.Close();
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception if necessary
                    throw;
                }
            }
        }

        public async Task<List<OrderNotification>> NotificationAsync()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string sql = $@"SELECT 
                                OrdersId, 
                                NotificationMessage, 
                                dbo.GetLocalDate(NotificationDate) NotificationDate,
                                ISNULL(IsView, 0) AS IsView,
                                CASE 
                                WHEN DATEDIFF(MINUTE, NotificationDate, dbo.GetLocalDate(GetUtcDate())) > 60 THEN 
                                CAST(DATEDIFF(MINUTE, NotificationDate, dbo.GetLocalDate(GetUtcDate())) / 60 AS VARCHAR) + ' hour(s)'
                                ELSE 
                                CAST(DATEDIFF(MINUTE, dbo.GetLocalDate(NotificationDate), dbo.GetLocalDate(GetUtcDate())) AS VARCHAR) + ' minute(s)'
                                END AS TimeSinceNotification
                                FROM 
                                OrderNotification
                                WHERE 
                                ISNULL(IsView, 0) = 0 Order by NotificationId Desc;";
                    var result = await connection.QueryAsync<OrderNotification>(sql);
                    connection.Close();
                    return result.AsList();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        }



    }
}
