using AdminSystem.Application.Helpers;
using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using AdminSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

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

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL.AppDbContext")));
// builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL.AppDbContext")));
// builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("MySQL.AppDbContext"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySQL.AppDbContext"))));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(config => config.AddProfile<MappingHelper>());

var app = builder.Build();

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
