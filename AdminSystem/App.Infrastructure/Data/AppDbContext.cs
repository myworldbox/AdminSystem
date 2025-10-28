using AdminSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AdminSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<客戶資料> 客戶資料 { get; set; }
        public DbSet<客戶聯絡人> 客戶聯絡人 { get; set; }
        public DbSet<客戶銀行資訊> 客戶銀行資訊 { get; set; }
        public DbSet<VwCustomerSummary> vw_CustomerSummary { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var boolToIntConverter = new ValueConverter<bool, int>(
                v => v ? 1 : 0,   // bool → int
                v => v == 1       // int → bool
            );

            modelBuilder.Entity<客戶資料>()
                .Property(e => e.是否已刪除)
                .HasConversion(boolToIntConverter);

            modelBuilder.Entity<客戶聯絡人>()
                .Property(e => e.是否已刪除)
                .HasConversion(boolToIntConverter);

            modelBuilder.Entity<客戶銀行資訊>()
                .Property(e => e.是否已刪除)
                .HasConversion(boolToIntConverter);

            modelBuilder.Entity<VwCustomerSummary>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_CustomerSummary");
            });
        }
    }
}