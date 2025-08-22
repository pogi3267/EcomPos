using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace ApplicationWeb.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // For console/migration
            .AddJsonFile("appsettings.json")
            .Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            //optionsBuilder.UseSqlServer("Server=DESKTOP-SV6K6FD\\SQLEXPRESS;Database=BDEcommerce;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
            //optionsBuilder.UseSqlServer("Server=103.125.255.10,9433;Database=ecomarcebd;User Id=devense;password=@abc_123;TrustServerCertificate=true;MultipleActiveResultSets=true;");
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
