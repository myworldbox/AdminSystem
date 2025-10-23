using AdminSystem.Application.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<InfoViewModel> 客戶資料 { get; set; }
        public DbSet<ContactViewModel> 客戶聯絡人 { get; set; }
        public DbSet<BankViewModel> 客戶銀行資訊 { get; set; }
        public DbSet<SummaryViewModel> vw_CustomerSummary { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SummaryViewModel>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_CustomerSummary");
            });
        }
    }
}