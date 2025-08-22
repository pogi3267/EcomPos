using ApplicationCore.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Linq;
using ApplicationCore.Entities.Products;
using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Entities.Orders;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
            var conStr = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(conStr);
        }

        public async Task<dynamic> GetUserInfoAsync(string userId)
        {
            string sql = $@"
            Select 
            AU.UserName, ISNULL(AU.Email, '') Email, AU.EmailConfirmed, ISNULL(AU.PhoneNumber, '') PhoneNumber, AU.PhoneNumberConfirmed, 
            ISNULL(AU.FirstName, '') FirstName, ISNULL(AU.LastName, '') LastName, ISNULL(AU.Avatar, '') Avatar, ISNULL(AU.[Address], '') [Address],
            ISNULL(AU.Country, '') Country, ISNULL(AU.[State], '') [State], ISNULL(AU.City, '') City, ISNULL(AU.PostalCode, '') PostalCode
            FROM AspNetUsers AU
            WHERE AU.Id = '{userId}'
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

        public async Task SaveProfileAsync(User model)
        {
            try
            {
                string sql = $@"
                UPDATE AspNetUsers SET FirstName = '{model.FirstName}', LastName = '{model.LastName}', Email = '{model.Email}', PhoneNumber = '{model.PhoneNumber}',
                Avatar = '{model.Avatar}', [Address] = '{model.Address}', Country = '{model.Country}', [State] = '{model.State}', City = '{model.City}', 
                PostalCode = '{model.PostalCode}'
                WHERE Id = '{model.Id}';
                ";

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                var id = await _connection.ExecuteAsync(sql, new { }, transaction);
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