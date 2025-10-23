using AdminSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            modelBuilder.Entity<VwCustomerSummary>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_CustomerSummary");
            });
        }
    }
}