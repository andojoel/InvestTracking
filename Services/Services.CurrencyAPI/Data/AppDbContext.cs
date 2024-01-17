using Microsoft.EntityFrameworkCore;
using Services.CurrencyAPI.Models;

namespace Services.CurrencyAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Currency> Currency { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Currency>().HasData(new Currency
            {
                id = "bitcoin",
                name = "Bitcoin",
                symbol = "btc"
            });
            modelBuilder.Entity<Currency>().HasData(new Currency
            {
                id = "ethereum",
                name = "Ethereum",
                symbol = "eth"
            });
        }
    }
}
