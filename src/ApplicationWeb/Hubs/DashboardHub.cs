using ApplicationCore.DTOs;
using Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;

namespace ApplicationWeb.Hubs
{
    public class DashboardHub : Hub
    {
        private HomeService homeService;

        public DashboardHub(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            homeService = new HomeService(connectionString);
        }

        public async Task SendProducts()
        {
            HomeBusinessAnalyticsDTO products = await homeService.GetAllBusinessAnalyticsDataAsync("today");
            await Clients.All.SendAsync("ReceivedProducts", products);
            AdminWalletDTO walletData = await homeService.GetAllAdminWalletAsync("today");
            await Clients.All.SendAsync("AdminWallet", walletData);
            var salesForGraph = await homeService.SalesForGraphAsync();
            await Clients.All.SendAsync("ReceivedSalesForGraph", salesForGraph);
            var PaymentForGraph = await homeService.PaymentForGraphAsync();
            await Clients.All.SendAsync("PaymentGraph", PaymentForGraph);
        }

        public async Task SendProductAndCustomer()
        {
            var productsAndCustomer = await homeService.ProductsAndCustomerAsync();
            await Clients.All.SendAsync("ProductsAndCustomer", productsAndCustomer);
        }

        public async Task ProductNotification()
        {
            var productNotification = await homeService.NotificationAsync();
            await Clients.All.SendAsync("ProductNotification", productNotification);
        }
    }
}