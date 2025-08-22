using ApplicationCore.Entities;
using Dapper;
using Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class MenuMasterService : IMenuMasterService
    {
        private readonly IDapperService<MenuMaster> _service;
        private readonly SqlConnection _connection;
        private SqlTransaction transaction = null;

        public MenuMasterService(IDapperService<MenuMaster> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<List<MenuMaster>> GetMenuMaster(string userId)
        {
            try
            {
                var query = $@"select *
                                from (
                                    select mm.MenuMasterId, ParentId, Name, Url, SerialNo, IsActive, dbo.GetLocalDate(mm.CreatedDate) CreatedDate, PageName, Icon,
                                    HasPermission = case when mm.ParentId = 0 then 1
			                                        when mp.UserId = '{userId}' AND mp.HasPermission =  1 THEN 1
				                                    ELSE 0
				                                    end
                                    from MenuMaster mm
                                    left join MenuPermission mp on mp.MenuMasterId = mm.MenuMasterId
                                    ) tbl
                                    where tbl.HasPermission = 1 AND IsActive = 1
                                    ORDER BY ParentId, SerialNo;";
                var data = await _service.GetDataAsync<MenuMaster>(query);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<MenuMaster> GetMenubyId(int id)
        {
            var query = $@"SELECT M.*, P.MenuParamId, P.ParamValue
                            FROM MenuMaster M
                            LEFT JOIN MenuParam P ON P.MenuMasterId = M.MenuMasterId
                            WHERE M.MenuMasterId = {id};";
            try
            {
                await _connection.OpenAsync();
                MenuMaster data = await _connection.QuerySingleAsync<MenuMaster>(query);
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

        public async Task<MenuMaster> GetMenuByPageName(string pageName)
        {
            var query = $@" SELECT M.*, P.MenuParamId, P.ParamValue
                            FROM MenuMaster M
                            LEFT JOIN MenuParam P ON P.MenuMasterId = M.MenuMasterId
                            WHERE M.PageName = '{pageName}';";
            try
            {
                await _connection.OpenAsync();
                MenuMaster data = await _connection.QuerySingleAsync<MenuMaster>(query);
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

        public async Task<List<MenuMaster>> GetMenusForClaims(string userId)
        {
            try
            {
                var query = $@"select *
                                from (
                                    select mm.MenuMasterId, ParentId, Name, Url, SerialNo, IsActive, dbo.GetLocalDate(mm.CreatedDate) CreatedDate, PageName,
                                    HasPermission = case when mm.ParentId = 0 then 1
			                                        when mp.UserId = '{userId}' AND mp.HasPermission =  1 THEN 1
				                                    ELSE 0
				                                    end
                                    from MenuMaster mm
                                    left join MenuPermission mp on mp.MenuMasterId = mm.MenuMasterId
                                    --WHERE mm.ParentId <> 0 AND mm.[Url] <> '0' AND mm.[Url] is not null AND mm.[Url] != ''
                                    ) tbl
                                    where tbl.HasPermission = 1 AND IsActive = 1;";
                var data = await _service.GetDataAsync<MenuMaster>(query);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<MenuMaster>> GetMenusForPermission(string userId)
        {
            try
            {
                var query = $@"select *
                            from (
                            select mm.MenuMasterId, ParentId, Name, Url, SerialNo, IsActive, dbo.GetLocalDate(mm.CreatedDate) CreatedDate, PageName,
                            HasPermission = case when mm.ParentId = 0 then 1
	                            when mp.UserId = '{userId}' AND mp.HasPermission =  1 THEN 1
	                            ELSE 0
	                            end
                            from MenuMaster mm
                            left join MenuPermission mp on mp.MenuMasterId = mm.MenuMasterId AND mp.UserId = '{userId}'
                            WHERE (mm.ParentId = 0 OR (mm.[Url] <> '0' AND mm.[Url] is not null AND mm.[Url] != ''))
                            ) tbl
                            where IsActive = 1 --AND tbl.HasPermission = 1
                            ORDER BY ParentId, SerialNo;";
                var data = await _service.GetDataAsync<MenuMaster>(query);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SaveMenuPermissionAsync(List<MenuMaster> menues, string userid)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                foreach (var menue in menues)
                {
                    string sql = $@"
                    IF EXISTS(Select 1 from MenuPermission WHERE MenuMasterId = {menue.MenuMasterId} AND UserId = '{userid}')
                    BEGIN
	                    UPDATE MenuPermission SET HasPermission = {menue.HasPermission} WHERE MenuPermissionId = 
	                    (Select TOP 1 MenuPermissionId from MenuPermission WHERE MenuMasterId = {menue.MenuMasterId} AND UserId = '{userid}')
                    END
                    ELSE
                    BEGIN
	                    INSERT INTO MenuPermission (MenuMasterId, UserId, RoleId, HasPermission, CreatedDate)
	                    VALUES ({menue.MenuMasterId}, '{userid}', '', {menue.HasPermission}, GetUtcDate())
                    END
                    ";
                    await _connection.ExecuteAsync(sql, new { }, transaction);
                }

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