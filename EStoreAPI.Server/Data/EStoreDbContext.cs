using EStoreAPI.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EStoreAPI.Server.Data
{
    public class EStoreDbContext : DbContext
    {
        protected readonly IConfiguration _Configuration;

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<Job> Jobs { get; set; }

        public EStoreDbContext(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // connection string from appsettings.json
            optionsBuilder.UseNpgsql(_Configuration.GetConnectionString("WebAPIDatabase"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Job <-> Problem: many-to-many via explicit join table
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Problems)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "JobProblems",
                    r => r.HasOne<Problem>().WithMany().HasForeignKey("ProblemId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Job>().WithMany().HasForeignKey("JobId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasKey("JobId", "ProblemId")
                );
        }
    }
}
