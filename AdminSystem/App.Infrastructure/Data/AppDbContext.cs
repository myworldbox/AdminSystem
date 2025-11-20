using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static AdminSystem.Domain.Enums;

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

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = config["AppDbContext"];
            Database db = (Database)Enum.Parse(typeof(Database), connectionString, ignoreCase: true);

            switch (db)
            {
                case Enums.Database.Oracle:
                    modelBuilder.Entity<客戶資料>(entity =>
                    {
                        entity.ToTable("CUSTOMERS");

                        entity.Property(e => e.Id).HasColumnName("ID");
                        entity.Property(e => e.客戶名稱).HasColumnName("NAME").HasMaxLength(50).IsRequired();
                        entity.Property(e => e.統一編號).HasColumnName("TAX_ID").HasMaxLength(8).IsUnicode(false).IsRequired();
                        entity.Property(e => e.電話).HasColumnName("PHONE").HasMaxLength(50).IsRequired();
                        entity.Property(e => e.傳真).HasColumnName("FAX").HasMaxLength(50);
                        entity.Property(e => e.地址).HasColumnName("ADDRESS").HasMaxLength(100);
                        entity.Property(e => e.Email).HasColumnName("EMAIL").HasMaxLength(250);
                        entity.Property(e => e.是否已刪除).HasColumnName("IS_DELETED").HasConversion(boolToIntConverter);
                        entity.Property(e => e.客戶分類).HasColumnName("CATEGORY").HasMaxLength(50);

                        entity.HasMany(e => e.客戶聯絡人s)
                              .WithOne(e => e.客戶)
                              .HasForeignKey(e => e.客戶Id)
                              .OnDelete(DeleteBehavior.Cascade);

                        entity.HasMany(e => e.客戶銀行資訊s)
                              .WithOne(e => e.客戶)
                              .HasForeignKey(e => e.客戶Id)
                              .OnDelete(DeleteBehavior.Cascade);
                    });

                    modelBuilder.Entity<客戶聯絡人>(entity =>
                    {
                        entity.ToTable("CUSTOMER_CONTACTS");

                        entity.HasIndex(e => e.客戶Id).HasDatabaseName("IX_CONTACTS_CUSTOMERID");

                        entity.Property(e => e.Id).HasColumnName("ID");
                        entity.Property(e => e.客戶Id).HasColumnName("CUSTOMER_ID");
                        entity.Property(e => e.職稱).HasColumnName("TITLE").HasMaxLength(50);
                        entity.Property(e => e.姓名).HasColumnName("NAME").HasMaxLength(50).IsRequired();
                        entity.Property(e => e.Email).HasColumnName("EMAIL").HasMaxLength(250).IsRequired();
                        entity.Property(e => e.手機).HasColumnName("MOBILE").HasMaxLength(50);
                        entity.Property(e => e.電話).HasColumnName("PHONE").HasMaxLength(50);
                        entity.Property(e => e.是否已刪除).HasColumnName("IS_DELETED").HasConversion(boolToIntConverter);

                        entity.HasOne(d => d.客戶)
                              .WithMany(p => p.客戶聯絡人s)
                              .HasForeignKey(d => d.客戶Id)
                              .OnDelete(DeleteBehavior.Cascade)
                              .HasConstraintName("FK_CUSTOMER_CONTACTS_CUSTOMER");
                    });

                    modelBuilder.Entity<客戶銀行資訊>(entity =>
                    {
                        entity.ToTable("CUSTOMER_BANK_INFOS");

                        entity.Property(e => e.Id).HasColumnName("ID");
                        entity.Property(e => e.客戶Id).HasColumnName("CUSTOMER_ID");
                        entity.Property(e => e.銀行名稱).HasColumnName("BANK_NAME").HasMaxLength(50).IsRequired();
                        entity.Property(e => e.銀行代碼).HasColumnName("BANK_CODE");
                        entity.Property(e => e.分行代碼).HasColumnName("BRANCH_CODE");
                        entity.Property(e => e.帳戶名稱).HasColumnName("ACCOUNT_NAME").HasMaxLength(50).IsRequired();
                        entity.Property(e => e.帳戶號碼).HasColumnName("ACCOUNT_NUMBER").HasMaxLength(20).IsRequired();
                        entity.Property(e => e.是否已刪除).HasColumnName("IS_DELETED").HasConversion(boolToIntConverter);

                        entity.HasOne(d => d.客戶)
                              .WithMany(p => p.客戶銀行資訊s)
                              .HasForeignKey(d => d.客戶Id)
                              .OnDelete(DeleteBehavior.Cascade)
                              .HasConstraintName("FK_CUSTOMER_BANK_INFOS_CUSTOMER");
                    });
                    break;
                case Enums.Database.PostgreSQL:
                case Enums.Database.SqlServer:
                case Enums.Database.MySQL:
                case Enums.Database.SQLite:
                    // 讓這三個實體的 Id 可以手動指定（關鍵！）
                    modelBuilder.Entity<客戶資料>()
                        .Property(c => c.Id)
                        .ValueGeneratedOnAdd();                    // 正常情況自動產生

                    modelBuilder.Entity<客戶聯絡人>()
                        .Property(c => c.Id)
                        .ValueGeneratedOnAdd();

                    modelBuilder.Entity<客戶銀行資訊>()
                        .Property(c => c.Id)
                        .ValueGeneratedOnAdd();
                    break;
                default:
                    throw new Exception("Unsupported database provider");
            }

            modelBuilder.Entity<VwCustomerSummary>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_CustomerSummary");
            });
        }
    }
}