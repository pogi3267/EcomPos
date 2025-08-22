using ApplicationCore.Entities.GeneralSettings;
using Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ClaimInformationService : IClaimInformationService
    {
        private readonly IDapperService<ClaimInformation> _service;
        private readonly SqlConnection _connection;

        public ClaimInformationService(IDapperService<ClaimInformation> service) : base()
        {
            _service = service;
            _connection = service.Connection;
        }

        public async Task<List<ClaimInformation>> GetListAsync()
        {
            try
            {
                var query = $@"SELECT * FROM ClaimInformation;";
                var data = await _service.GetDataAsync<ClaimInformation>(query);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}