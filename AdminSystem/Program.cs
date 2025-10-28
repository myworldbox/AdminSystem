using AdminSystem.Application.Helpers;
using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using AdminSystem.Infrastructure.Repositories;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Globalization;

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

builder.Services.AddDbContext<AppDbContext>(options =>
{
    switch (provider)
    {
        case "PostgreSQL":
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL.AppDbContext"));
            break;
        case "MSSQL":
            options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL.AppDbContext"));
            break;
        case "MySQL":
            options.UseMySql(
                builder.Configuration.GetConnectionString("MySQL.AppDbContext"),
                ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySQL.AppDbContext")));
            break;
        case "Oracle":
            options.UseOracle(builder.Configuration.GetConnectionString("Oracle.AppDbContext"));
            break;
        case "SQLite":
            options.UseSqlite(builder.Configuration.GetConnectionString("SQLite.AppDbContext"));
            break;
        default:
            throw new Exception("Unsupported database provider");
    }
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(config => config.AddProfile<MappingHelper>());

var app = builder.Build();

var defaultCulture = new CultureInfo("zh-CN"); // �� "zh-TW"
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
