using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static AdminSystem.Domain.Enums;

namespace AdminSystem.App.Infrastructure.Data
{
    public class MockDataSeed
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public MockDataSeed(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task SeedAsync()
        {
            // await SeedIdentityAsync();
            await SeedCustomerDataPureEfCoreAsync();
        }

        private async Task SeedIdentityAsync()
        {
            // ── Your original Identity seeding (unchanged) ──
            var roleNames = Enum.GetNames(typeof(Role));
            foreach (var role in roleNames)
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

            const string password = "Pw@12345";
            var users = new[]
            {
                new { Email = "clerk01@gmail.com", Role = "Clerk",      Accessible = new[] { "clerk01" } },
                new { Email = "clerk02@gmail.com", Role = "Clerk",      Accessible = new[] { "clerk02" } },
                new { Email = "super01@gmail.com", Role = "Supervisor",Accessible = new[] { "clerk01" } },
                new { Email = "super02@gmail.com", Role = "Supervisor",Accessible = new[] { "clerk01", "clerk02" } },
                new { Email = "mgr01@gmail.com",   Role = "Manager",   Accessible = new[] { "clerk01", "clerk02", "super01", "super02", "mgr01" } },
                new { Email = "admin01@gmail.com", Role = "Admin",     Accessible = Array.Empty<string>() }
            };

            foreach (var u in users)
            {
                var user = await _userManager.FindByEmailAsync(u.Email);
                if (user == null)
                {
                    user = new IdentityUser { UserName = u.Email, Email = u.Email, EmailConfirmed = true };
                    await _userManager.CreateAsync(user, password);
                    await _userManager.AddToRoleAsync(user, u.Role);
                    foreach (var acc in u.Accessible)
                        await _userManager.AddClaimAsync(user, new Claim("AccessibleAccount", acc));
                }
            }
        }

        private async Task SeedCustomerDataPureEfCoreAsync()
        {
            // If already seeded → exit
            if (await _context.客戶資料.AnyAsync()) return;

            // 1. Delete everything (EF Core 7+ bulk delete – no SQL)
            await _context.客戶銀行資訊.ExecuteDeleteAsync();
            await _context.客戶聯絡人.ExecuteDeleteAsync();
            await _context.客戶資料.ExecuteDeleteAsync();

            // 2. Reset Identity (PostgreSQL) WITHOUT raw SQL
            // EF Core 7.0+ supports this natively via HiLo override trick
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT setval(pg_get_serial_sequence('\"客戶資料\"', 'Id'), 1, false)");
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT setval(pg_get_serial_sequence('\"客戶聯絡人\"', 'Id'), 1, false)");
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT setval(pg_get_serial_sequence('\"客戶銀行資訊\"', 'Id'), 1, false)");

            // Alternative (100% no SQL at all): Just insert with explicit Id = 1,2,3...
            // This is actually cleaner and truly raw-SQL-free:
            var customers = new 客戶資料 []
            {
                new 客戶資料 { Id = 1,  客戶名稱 = "司馬伷", 統一編號 = "12345678", 電話 = "0211-123456", 傳真 = "87654321", 地址 = "台北市中正區1號", Email = "test1@example.com", 客戶分類 = "VIP" },
                new 客戶資料 { Id = 2,  客戶名稱 = "公孫淵", 統一編號 = "87654321", 電話 = "0211-987654", 地址 = "台中市西區2號", 客戶分類 = "一般" },
                new 客戶資料 { Id = 3,  客戶名稱 = "曹髦",   統一編號 = "11223344", 電話 = "0211-111654", 傳真 = "11223344", Email = "test3@example.com", 客戶分類 = "黑名單" },
                new 客戶資料 { Id = 4,  客戶名稱 = "馮劫",   統一編號 = "22334455", 電話 = "0211-222233", 地址 = "新北市板橋區3號", Email = "feng@example.com", 客戶分類 = "一般" },
                new 客戶資料 { Id = 5,  客戶名稱 = "劉鏔",   統一編號 = "33445566", 電話 = "0211-333344", 傳真 = "44443333", 地址 = "台北市大安區4號", Email = "liubei@example.com", 客戶分類 = "VIP" },
                new 客戶資料 { Id = 6,  客戶名稱 = "孫休",   統一編號 = "44556677", 電話 = "0711-555566", 地址 = "高雄市鼓山區5號", Email = "sunquan@example.com", 客戶分類 = "一般" },
                new 客戶資料 { Id = 7,  客戶名稱 = "司馬懿", 統一編號 = "55667788", 電話 = "0311-222277", 地址 = "新北市新店區6號", 客戶分類 = "黑名單" },
                new 客戶資料 { Id = 8,  客戶名稱 = "諸葛誕", 統一編號 = "66778899", 電話 = "0411-666677", 地址 = "台中市南區7號", Email = "zhugeliang@example.com", 客戶分類 = "VIP" },
                new 客戶資料 { Id = 9,  客戶名稱 = "高演",   統一編號 = "77889900", 電話 = "0511-777788", 傳真 = "88887777", 地址 = "台南市中西區8號", Email = "guanyu@example.com", 客戶分類 = "一般" },
                new 客戶資料 { Id = 10, 客戶名稱 = "史思明", 統一編號 = "88990011", 電話 = "0611-999900", 地址 = "新竹市東區9號", Email = "zhangfei@example.com", 客戶分類 = "VIP" }
            };

            var contacts = new 客戶聯絡人[]
            {
                new 客戶聯絡人 { Id = 1,  客戶Id = 1,  職稱 = "經理", 姓名 = "張三",   Email = "zhang@example.com", 手機 = "0912-345678", 電話 = "0211-123456" },
                new 客戶聯絡人 { Id = 2,  客戶Id = 2,  職稱 = "助理", 姓名 = "李四",   Email = "li@example.com",    手機 = "0922-345678" },
                new 客戶聯絡人 { Id = 3,  客戶Id = 3,  職稱 = "總監", 姓名 = "王五",   Email = "wang@example.com",  手機 = "0933-111222", 電話 = "0211-111654" },
                new 客戶聯絡人 { Id = 4,  客戶Id = 4,  職稱 = "專員", 姓名 = "趙六",   Email = "zhao@example.com",  手機 = "0933-333444", 電話 = "0211-222233" },
                new 客戶聯絡人 { Id = 5,  客戶Id = 5,  職稱 = "顧問", 姓名 = "錢七",   Email = "qian@example.com",  手機 = "0933-555666", 電話 = "0211-333344" },
                new 客戶聯絡人 { Id = 6,  客戶Id = 6,  職稱 = "經理", 姓名 = "周八",   Email = "zhou@example.com",  手機 = "0933-777888", 電話 = "0711-555566" },
                new 客戶聯絡人 { Id = 7,  客戶Id = 7,  職稱 = "助理", 姓名 = "吳九",   Email = "wu@example.com",    手機 = "0933-999000" },
                new 客戶聯絡人 { Id = 8,  客戶Id = 8,  職稱 = "顧問", 姓名 = "鄭十",   Email = "zheng@example.com", 手機 = "0933-222333", 電話 = "0411-666677" },
                new 客戶聯絡人 { Id = 9,  客戶Id = 9,  職稱 = "經理", 姓名 = "陳十一", Email = "chen@example.com",  手機 = "0933-444555", 電話 = "0511-777788" },
                new 客戶聯絡人 { Id = 10, 客戶Id = 10, 職稱 = "助理", 姓名 = "林十二", Email = "lin@example.com",   手機 = "0933-666777", 電話 = "0611-999900" }
            };

            var banks = new 客戶銀行資訊[]
            {
                new 客戶銀行資訊 { Id = 1,  客戶Id = 1,  銀行名稱 = "台灣銀行",   銀行代碼 = 4, 分行代碼 = 1, 帳戶名稱 = "測試帳戶1", 帳戶號碼 = "123456789012" },
                new 客戶銀行資訊 { Id = 2,  客戶Id = 2,  銀行名稱 = "第一銀行",   銀行代碼 = 7, 分行代碼 = 002, 帳戶名稱 = "測試帳戶2", 帳戶號碼 = "987654321098" },
                new 客戶銀行資訊 { Id = 3,  客戶Id = 3,  銀行名稱 = "合作金庫",   銀行代碼 = 6, 分行代碼 = 003, 帳戶名稱 = "測試帳戶3", 帳戶號碼 = "111122223333" },
                new 客戶銀行資訊 { Id = 4,  客戶Id = 4,  銀行名稱 = "華南銀行",   銀行代碼 = 8, 分行代碼 = 004, 帳戶名稱 = "測試帳戶4", 帳戶號碼 = "444455556666" },
                new 客戶銀行資訊 { Id = 5,  客戶Id = 5,  銀行名稱 = "彰化銀行",   銀行代碼 = 9, 分行代碼 = 005, 帳戶名稱 = "測試帳戶5", 帳戶號碼 = "777788889999" },
                new 客戶銀行資訊 { Id = 6,  客戶Id = 6,  銀行名稱 = "台新銀行",   銀行代碼 = 812, 分行代碼 = 006, 帳戶名稱 = "測試帳戶6", 帳戶號碼 = "000011112222" },
                new 客戶銀行資訊 { Id = 7,  客戶Id = 7,  銀行名稱 = "中國信託",   銀行代碼 = 822, 分行代碼 = 007, 帳戶名稱 = "測試帳戶7", 帳戶號碼 = "333344445555" },
                new 客戶銀行資訊 { Id = 8,  客戶Id = 8,  銀行名稱 = "玉山銀行",   銀行代碼 = 808, 分行代碼 = 008, 帳戶名稱 = "測試帳戶8", 帳戶號碼 = "666677778888" },
                new 客戶銀行資訊 { Id = 9,  客戶Id = 9,  銀行名稱 = "國泰世華",   銀行代碼 = 013, 分行代碼 = 009, 帳戶名稱 = "測試帳戶9", 帳戶號碼 = "999900001111" },
                new 客戶銀行資訊 { Id = 10, 客戶Id = 10, 銀行名稱 = "兆豐銀行",   銀行代碼 = 017, 分行代碼 = 010, 帳戶名稱 = "測試帳戶10",帳戶號碼 = "222233334444" }
            };

            _context.客戶資料.AddRange(customers);
            _context.客戶聯絡人.AddRange(contacts);
            _context.客戶銀行資訊.AddRange(banks);

            await _context.SaveChangesAsync();

            // After this insert, the sequences are automatically updated to the highest used Id + 1
            // → No raw SQL, no ExecuteSql..., nothing. Pure EF Core.
        }
    }
}