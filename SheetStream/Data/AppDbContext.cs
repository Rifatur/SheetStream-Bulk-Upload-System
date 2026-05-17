using Microsoft.EntityFrameworkCore;
using SheetStream.Models;

namespace SheetStream.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<PartyDealer> PartyDealers => Set<PartyDealer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.Sku).IsUnique();
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });
            modelBuilder.Entity<PartyDealer>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.GstNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            });
        }
    }
}
