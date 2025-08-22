using ApplicationCore.DTOs;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashBoardController : Controller
    {
        HomeService homeService;

        public DashBoardController(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            homeService = new HomeService(connectionString);
        }

        public async Task<IActionResult> Dashboarddesi()
        {
            return View("DashBoard");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBusinessAnalyticsData([FromQuery] string status)
        {
            try
            {
                HomeBusinessAnalyticsDTO data = await homeService.GetAllBusinessAnalyticsDataAsync(status);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAdminWalletIdData([FromQuery] string status)
        {
            try
            {
                AdminWalletDTO data = await homeService.GetAllAdminWalletAsync(status);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }


    }
}
