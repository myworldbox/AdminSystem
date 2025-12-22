using AdminSystem.App.Application.Common;
using AdminSystem.App.Application.Interfaces;
using AdminSystem.App.Application.Services;
using AdminSystem.App.Infrastructure;
using AdminSystem.App.Infrastructure.Data;
using AdminSystem.App.Infrastructure.Interfaces;
using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using AdminSystem.Infrastructure.Repositories;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
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
        options.ViewLocationFormats.Add("App.Web/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("App.Web/Views/Shared/{0}.cshtml");
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
builder.Services.AddScoped<ISummaryService, SummaryService>();

builder.Services.AddAutoMapper(config => config.AddProfile<Mappings>());

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "zh-HK", "zh-CN", "en-US" }
        .Select(c => new CultureInfo(c))
        .ToList();

    options.DefaultRequestCulture = new RequestCulture("zh-HK");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<MockDataSeed>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
