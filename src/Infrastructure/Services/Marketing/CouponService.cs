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
    public class CouponService : ICouponService
    {
        private readonly IDapperService<Coupon> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public CouponService(IDapperService<Coupon> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Coupon> GetAsync(int id)
        {
            var query = $@"Select C.CouponId, C.UserId, C.Type, C.Code, C.Details, C.Discount, C.DiscountType, dbo.GetLocalDate(C.StartDate) StartDate, dbo.GetLocalDate(C.EndDate) EndDate, ISNULL(C.IsActive, 0) IsActive
                        from Coupons C
                        WHERE C.CouponId = {id};
                            ";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Coupon data = queryResult.Read<Coupon>().FirstOrDefault();
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

        public async Task<int> SaveAsync(Coupon entity)
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

        public async Task<List<Coupon>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY CouponId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"
                        Select C.CouponId, C.UserId, C.Type, C.Code, C.Details, C.Discount, C.DiscountType, dbo.GetLocalDate(C.StartDate) StartDate, dbo.GetLocalDate(C.EndDate) EndDate, ISNULL(C.IsActive, 0) IsActive, Count(1) Over() TotalRows
                        from Coupons C";
                if (searchBy != "")
                    sql += " WHERE C.Code like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Coupon>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<SpecialOfferEmails>> GetSubscriberListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY Id DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"
                        Select Id, Email, Count(1) Over() TotalRows from SpecialOfferEmails C";
                if (searchBy != "")
                    sql += " WHERE C.Email like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<SpecialOfferEmails>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}