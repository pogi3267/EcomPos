using ApplicationCore.Entities;
using ApplicationCore.Entities.SetupAndConfigurations;
using Dapper;
using Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.SetupAndConfigurations
{
    public class StaffService : IStaffService
    {
        private readonly IDapperService<Staff> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public StaffService(IDapperService<Staff> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<Staff> GetAsync(int id)
        {
            var query = $@"SELECT S.*, CONCAT(U.FirstName, ' ', U.LastName,' [',U.UserName,']')  AS [Name], R.[Name] [Role]
                        FROM Staffs S
                        LEFT JOIN AspNetUsers U ON U.Id = S.UserId
                        LEFT JOIN AspNetRoles R ON R.Id = S.RoleId
                        WHERE S.StaffId = {id};

                        SELECT CAST(Id AS NVARCHAR(500)) Id, Name as Text FROM AspNetRoles;
                       SELECT CAST(Id AS NVARCHAR(500)) Id, CONCAT(U.FirstName, ' ', U.LastName,' [',U.UserName,']')   as Text FROM AspNetUsers U;

                        ";

            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Staff data = queryResult.Read<Staff>().FirstOrDefault();
                data.RoleList = queryResult.Read<Select2OptionModel>().ToList();

                data.UserList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<Staff> GetNewAsync()
        {
            var query = $@"
                        SELECT CAST(Id AS NVARCHAR(500)) Id, Name as Text FROM AspNetRoles;
                        SELECT CAST(Id AS NVARCHAR(500)) Id, CONCAT(U.FirstName, ' ', U.LastName,' [',U.UserName,']')   as Text FROM AspNetUsers U;
                        ";
            try
            {
                await _connection.OpenAsync();
                var queryResult = await _connection.QueryMultipleAsync(query);
                Staff data = new Staff();
                data.RoleList = queryResult.Read<Select2OptionModel>().ToList();
                data.UserList = queryResult.Read<Select2OptionModel>().ToList();
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

        public async Task<int> SaveAsync(Staff entity)
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

        public async Task<List<Staff>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir)
        {
            try
            {
                string orderBy = string.IsNullOrEmpty(sortBy) ? "ORDER BY StaffId DESC" : "ORDER BY " + sortBy + " " + sortDir;
                string pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", skip, take);
                string sql = $@"SELECT S.*,CONCAT(U.FirstName, ' ', U.LastName,' [',U.UserName,']')  AS [Name], R.[Name] [Role], Count(*) Over() TotalRows 
                                FROM Staffs S
                                LEFT JOIN AspNetUsers U ON U.Id = S.UserId
                                LEFT JOIN AspNetRoles R ON R.Id = S.RoleId";
                if (searchBy != "")
                    sql += " WHERE Name like '%" + searchBy + "%'";
                sql += $@"{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";
                var result = await _service.GetDataAsync<Staff>(sql);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}