
using ApplicationCore.Entities.Orders;
using ApplicationWeb.Hubs;
using TableDependency.SqlClient;

namespace ApplicationWeb.SubscribeTableDependencies
{
    public class ProductsAndCustomerTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<Orders> tableDependency;
        DashboardHub dashboardHub;

        public ProductsAndCustomerTableDependency(DashboardHub dashboardHub)
        {
            this.dashboardHub = dashboardHub;
        }

        public void SubscribeTableDependency(string connectionString)
        {
            tableDependency = new SqlTableDependency<Orders>(connectionString);
            tableDependency.OnChanged += TableDependency_OnChanged;
            tableDependency.OnError += TableDependency_OnError;
            tableDependency.Start();
        }

        private async void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<Orders> e)
        {
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
               await dashboardHub.SendProductAndCustomer();
            }
        }

        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(Orders)} SqlTableDependency error: {e.Error.Message}");
        }
    }
}
