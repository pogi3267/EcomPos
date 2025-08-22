using ApplicationCore.Entities;
using ApplicationCore.Entities.SetupAndConfigurations;
using Dapper;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.SetupAndConfigurations
{
    public class PickupPointsService : IPickupPointsService
    {
        private readonly IDapperService<PickupPoint> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public PickupPointsService(IDapperService<PickupPoint> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<PickupPoint> GetAsync(int id)
        {
            var query = $@"SELECT P.*, U.FirstName StaffName
                        FROM PickupPoints P
                        LEFT JOIN Staffs S ON S.StaffId = P.StaffId
                        LEFT JOIN AspNetUsers U ON U.Id = S.UserId
                        WHERE P.PickupPointId = {id};

                        SELECT CAST(S.StaffId AS VARCHAR) Id, U.FirstName as Text 
                        FROM Staffs S
                        LEFT JOIN AspNetUsers U ON U.Id = S.UserId;";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                PickupPoint data = queryResult.Read<PickupPoint>().FirstOrDefault();
                data.StaffList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<PickupPoint> GetNewAsync()
        {
            var query = $@"
                        SELECT CAST(S.StaffId AS VARCHAR) Id, U.FirstName as Text 
                        FROM Staffs S
                        LEFT JOIN AspNetUsers U ON U.Id = S.UserId;
                        ";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                PickupPoint data = new PickupPoint();
                data.StaffList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<int> SaveAsync(PickupPoint entity)
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
        public async Task<List<PickupPoint>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY PickupPointId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT *, U.FirstName StaffName, Count(*) Over() TotalRows 
                            FROM PickupPoints P
                            LEFT JOIN Staffs S ON S.StaffId = P.StaffId
                            LEFT JOIN AspNetUsers U ON U.Id = S.UserId";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<PickupPoint>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
