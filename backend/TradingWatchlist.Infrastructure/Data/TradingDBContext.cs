// backend/TradingWatchlist.Infrastructure/Data/TradingDbContext.cs
using Microsoft.EntityFrameworkCore;
using TradingWatchlist.Core.Models;

namespace TradingWatchlist.Infrastructure.Data;

public class TradingDbContext : DbContext
{
    public TradingDbContext(DbContextOptions<TradingDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Stock> Stocks { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Screenshot> Screenshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Stock configuration
        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ticker)
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(e => e.Source)
                .HasMaxLength(500);
            entity.HasIndex(e => e.Ticker);
        });

        // Alert configuration
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetPrice)
                .HasPrecision(18, 2);
            
            entity.HasOne(e => e.Stock)
                .WithMany(s => s.Alerts)
                .HasForeignKey(e => e.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Screenshot configuration
        modelBuilder.Entity<Screenshot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.HasOne(e => e.Stock)
                .WithMany(s => s.Screenshots)
                .HasForeignKey(e => e.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}