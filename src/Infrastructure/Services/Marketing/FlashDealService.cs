using ApplicationCore.Entities.Marketing;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Marketing;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Marketing
{
    public class FlashDealService : IFlashDealService
    {
        private readonly IDapperService<FlashDeal> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public FlashDealService(IDapperService<FlashDeal> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<List<FlashDeal>> GetAll()
        {
            var query = $@"select
	                        fd.FlashDealId,
	                        fd.Title,
                            fd.ProductId,
	                        ProductName = pd.Name,
	                        fd.BackgroundColor,
	                        fd.TextColor,
	                        fd.Banner,
	                        dbo.GetLocalDate(fd.StartDate) StartDate,
	                        dbo.GetLocalDate(fd.EndDate) EndDate,
	                        fd.Status,
	                        fd.Featured,
	                        fd.Slug
                        from FlashDeals fd
                        LEFT JOIN Products pd on pd.ProductId = fd.ProductId;";

            try
            {
                var result = await _service.GetDataAsync<FlashDeal>(query);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<FlashDeal> GetAsync(int id)
        {
            var query = $@"select
	                        fd.FlashDealId,
	                        fd.Title,
                            fd.ProductId,
	                        fd.BackgroundColor,
	                        fd.TextColor,
	                        fd.Banner,
	                        dbo.GetLocalDate(fd.StartDate) StartDate,
	                        dbo.GetLocalDate(fd.EndDate) EndDate,
	                        fd.Status,
	                        fd.Featured,
	                        fd.Slug
                        from FlashDeals fd
                        WHERE fd.FlashDealId = {id};
                            ";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                FlashDeal data = queryResult.Read<FlashDeal>().FirstOrDefault();
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

        public async Task<int> SaveAsync(FlashDeal entity)
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

        public async Task<List<FlashDeal>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY FlashDealId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"
                            select
	                        fd.FlashDealId,
	                        fd.Title,
                            fd.ProductId,
	                        ProductName = pd.Name,
	                        fd.BackgroundColor,
	                        fd.TextColor,
	                        fd.Banner,
	                        dbo.GetLocalDate(fd.StartDate) StartDate,
                            dbo.GetLocalDate(fd.EndDate) EndDate,
	                        fd.Status,
	                        fd.Featured,
	                        fd.Slug,
                            Count(1) Over() TotalRows
                        from FlashDeals fd
                        LEFT JOIN Products pd on pd.ProductId = fd.ProductId  ";
                if (searchBy != "")
                    sql += " WHERE fd.Title like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<FlashDeal>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<FlashDeal>> GetActiveFlashDEals()
        {
            var query = $@"select
	                        fd.FlashDealId,
	                        fd.Title,
                            fd.ProductId,
	                        fd.BackgroundColor,
	                        fd.TextColor,
	                        fd.Banner,
	                        dbo.GetLocalDate(fd.StartDate) StartDate,
	                        dbo.GetLocalDate(fd.EndDate) EndDate,
	                        fd.Status,
	                        fd.Featured,
	                        fd.Slug
                        from FlashDeals fd
						WHERE fd.[Status] = 1;";

            try
            {
                var result = await _service.GetDataAsync<FlashDeal>(query);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}