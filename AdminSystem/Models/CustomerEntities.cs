using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Models
{
    public class CustomerEntities : DbContext
    {
        public CustomerEntities(DbContextOptions<CustomerEntities> options) : base(options)
        {
        }

        public DbSet<客戶資料> 客戶資料 { get; set; }
        public DbSet<客戶聯絡人> 客戶聯絡人 { get; set; }
        public DbSet<客戶銀行資訊> 客戶銀行資訊 { get; set; }
        public DbSet<vw_CustomerSummary> vw_CustomerSummary { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<vw_CustomerSummary>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_CustomerSummary");
            });
        }
    }
}