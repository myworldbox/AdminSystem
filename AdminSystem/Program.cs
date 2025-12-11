using AdminSystem.App.Infrastructure.Data;
using AdminSystem.Application.Helpers;
using AdminSystem.Application.Services;
using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using AdminSystem.Infrastructure.Repositories;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Globalization;
using static AdminSystem.Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

/*
// Explicitly add config files from Web/
builder.Configuration
    .AddJsonFile(Path.Combine("App.Web", "appsettings.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine("App.Web", $"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true)
    .AddEnvironmentVariables();
*/

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("App.UI/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("App.UI/Views/Shared/{0}.cshtml");
    });

var provider = builder.Configuration["AppDbContext"];
string dbContext = $"{provider}.AppDbContext";
Database db = (Database)Enum.Parse(typeof(Database), provider, ignoreCase: true);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (db)
    {
        case Database.PostgreSQL:
            options.UseNpgsql(builder.Configuration.GetConnectionString(dbContext));
            break;
        case Database.SqlServer:
            options.UseSqlServer(builder.Configuration.GetConnectionString(dbContext));
            break;
        case Database.MySQL:
            options.UseMySql(
                builder.Configuration.GetConnectionString(dbContext),
                ServerVersion.AutoDetect(builder.Configuration.GetConnectionString(dbContext)));
            break;
        case Database.Oracle:
            options.UseOracle(builder.Configuration.GetConnectionString(dbContext));
            break;
        case Database.SQLite:
            options.UseSqlite(builder.Configuration.GetConnectionString(dbContext));
            break;
    }
});

builder.Services.AddScoped<MockDataSeed>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IInfoService, InfoService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IBankService, BankService>();

builder.Services.AddAutoMapper(config => config.AddProfile<MappingHelper>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<MockDataSeed>();
    await seeder.SeedAsync();
}

var defaultCulture = new CultureInfo("zh-CN"); // ©Î "zh-TW"
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
