
using ApplicationCore.Entities.Orders;
using ApplicationWeb.Hubs;
using TableDependency.SqlClient;

namespace ApplicationWeb.SubscribeTableDependencies
{
    public class ProductNotificationTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<OrderNotification> tableDependency;
        DashboardHub dashboardHub;

        public ProductNotificationTableDependency(DashboardHub dashboardHub)
        {
            this.dashboardHub = dashboardHub;
        }

        public void SubscribeTableDependency(string connectionString)
        {
            tableDependency = new SqlTableDependency<OrderNotification>(connectionString);
            tableDependency.OnChanged += TableDependency_OnChanged;
            tableDependency.OnError += TableDependency_OnError;
            tableDependency.Start();
        }

        private async void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<OrderNotification> e)
        {
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                await dashboardHub.ProductNotification();
            }
        }

        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(Orders)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
