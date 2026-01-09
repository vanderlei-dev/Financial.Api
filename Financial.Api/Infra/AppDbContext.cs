using Financial.Api.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Financial.Api.Infra
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<CompanyIncome> Incomes { get; set; }        

        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
