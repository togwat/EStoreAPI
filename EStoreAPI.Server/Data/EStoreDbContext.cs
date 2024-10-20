using EStoreAPI.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EStoreAPI.Server.Data
{
    public class EStoreDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<Job> Jobs { get; set; }

        public EStoreDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // connection string from appsettings.json
            optionsBuilder.UseNpgsql(Configuration.GetConnectionString("WebAPIDatabase"));
        }
    }
}
