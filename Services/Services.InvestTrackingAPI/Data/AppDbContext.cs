using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Services.InvestTrackingAPI.Models;

namespace Services.InvestTrackingAPI.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Wallet> WalletTable { get; set; } = null!;
    public DbSet<Currency> CurrencyTable { get; set; } = null!;

}