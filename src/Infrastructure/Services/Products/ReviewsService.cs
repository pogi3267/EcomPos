using ApplicationCore.Entities;
using ApplicationCore.Entities.Products;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Products;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Products
{
    public class ReviewsService : IReviewsService
    {
        private readonly IDapperService<Reviews> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public ReviewsService(IDapperService<Reviews> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<List<Reviews>> GetListAsync(int pageNo = 1, int limit = 10, string filterBy = null, string orderBy = null)
        {
            orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY ReviewId DESC" : orderBy;
            string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (pageNo - 1) * limit, limit);

            string sql = $@"select R.ReviewId, R.Rating, R.Comment, R.[Status], R.Viewed, R.ProductId, CONCAT(U.FirstName, ' ', U.LastName) UserName,
                         P.[Name] ProductName
                         from Reviews R
                         INNER JOIN Products P ON P.ProductId = R.ProductId
                         INNER JOIN AspNetUsers U ON U.Id = R.UserId";


            sql += $@"{Environment.NewLine}{filterBy}{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
            return await _service.GetDataAsync<Reviews>(sql);
        }

        public async Task<Reviews> GetNewAsync()
        {
            var query = $@"
                SELECT Id = Id, Text = CONCAT(FirstName, ' ', LastName, ' [', UserName, ']') FROM AspNetUsers ORDER BY CONCAT(FirstName, LastName) DESC;
               SELECT Id = ProductId, Text = Name FROM Products ORDER BY  Name DESC;
            ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Reviews data = new Reviews();
                data.UserList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<Reviews> GetAsync(int id)
        {
            var query = $@"
                SELECT * FROM Reviews WHERE ReviewId = {id};

                SELECT Id = Id, Text = CONCAT(FirstName, ' ', LastName, ' [', UserName, ']') FROM AspNetUsers ORDER BY CONCAT(FirstName, LastName) DESC;
               SELECT Id = ProductId, Text = Name FROM Products ORDER BY  Name DESC; "
                ;

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Reviews data = queryResult.Read<Reviews>().FirstOrDefault();
                data.UserList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<int> SaveAsync(Reviews entity)
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

        public async Task<List<Reviews>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY ReviewId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"select R.ReviewId, R.Rating, R.Comment, R.[Status], R.Viewed, R.ProductId, CONCAT(U.FirstName, ' ', U.LastName) UserName,
                            P.[Name] ProductName, Count(1) Over() TotalRows
                            from Reviews R
                            INNER JOIN Products P ON P.ProductId = R.ProductId
                            INNER JOIN AspNetUsers U ON U.Id = R.UserId";
                if (searchBy != "")
                    sql += " WHERE P.[Name] like '%" + searchBy + "%' or R.Rating like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Reviews>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
