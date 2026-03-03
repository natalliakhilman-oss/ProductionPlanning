using System.Collections.Generic;
using ProductionPlanning.Models;
using ProductionPlanning.Models.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using System.Reflection;
using ProductionPlanning.Hubs;

var builder = WebApplication.CreateBuilder(args);

#region Logger
// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// ????????? ?????????? ?????
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);

// Get log level from appsettings
var logLevelSection = builder.Configuration.GetSection("Logging:LogLevel");
var defaultLogLevel = logLevelSection.GetValue<LogLevel?>("Genezis") ?? LogLevel.Information;

// Add custom file logger
builder.Logging.AddFileLogger(options =>
{
    options.FilePath = $"{AppInfo.GetAppPath()}/Logs/Genezis.log";
    options.MaxFileSize = 5 * 1024 * 1024; // 5MB
    options.RetainedFileCount = 10;
    options.DefaultLogLevel = defaultLogLevel;
});
#endregion Logger

#region Sevices
var isService = args.Contains("--service") || WindowsServiceHelpers.IsWindowsService();
if (isService)
    builder.Host.UseWindowsService();
#endregion Services

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
    mvcBuilder.AddRazorRuntimeCompilation();

// DBContext
#region ConnectionDB
var isDevelopment = builder.Environment.IsDevelopment();
if (isDevelopment)
{
    var sqlitePath = Path.Combine(AppInfo.GetAppPath(), "app.db");
    builder.Services.AddDbContext<DBContext>(options => options.UseSqlite($"Data Source={sqlitePath}"));
}
else
{
    var config = new ConfigurationBuilder()
                    .SetBasePath(AppInfo.GetAppPath())
                    .AddJsonFile("hosting.json", optional: true)
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["db_host"] = Environment.GetEnvironmentVariable("DB_HOST"),
                        ["db_port"] = Environment.GetEnvironmentVariable("DB_PORT"),
                        ["db_name"] = Environment.GetEnvironmentVariable("DB_NAME"),
                        ["db_user"] = Environment.GetEnvironmentVariable("DB_USER"),
                        ["db_timeout"] = Environment.GetEnvironmentVariable("DB_TIMEOUT"),
                    })
                    .Build();

    var db_host = config.GetValue<string>("db_host") ?? "localhost";
    var db_port = int.TryParse(config.GetValue<string>("db_port"), out var port)
        ? port
        : AppInfo.db_port_default;
    var db_name = config.GetValue<string>("db_name") ?? AppInfo.db_name_default;
    var db_user = config.GetValue<string>("db_user") ?? AppInfo.db_user_default;
    var db_pass_raw = config.GetValue<string>("db_pass");
    var db_pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? (db_pass_raw != null ? Crypt.Decrypt(db_pass_raw, AppInfo.encryptionKey) : AppInfo.db_password_default);
    var db_timeout = int.TryParse(config.GetValue<string>("db_timeout"), out var timeout)
        ? timeout
        : AppInfo.db_timeout_default;

    var db_connection_mySql = $"Server={db_host};Port={db_port};Database={db_name};User={db_user};Password={db_pass};Connection Timeout={db_timeout};Default Command Timeout={db_timeout}";
    builder.Services.AddDbContext<DBContext>(options => options.UseMySql(db_connection_mySql, ServerVersion.AutoDetect(db_connection_mySql)));
}
#endregion ConnectionDB

#region Identity Configuration
builder.Services.AddIdentity<User, IdentityRole>(opts =>
{
    opts.Password.RequiredLength = 4; // ??????????? ?????
    opts.Password.RequireNonAlphanumeric = false; // ????????? ?? ?? ?????????-???????? ???????
    opts.Password.RequireLowercase = false; // ????????? ?? ??????? ? ?????? ????????
    opts.Password.RequireUppercase = false; // ????????? ?? ??????? ? ??????? ????????
    opts.Password.RequireDigit = false; // ????????? ?? ?????
    opts.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<DBContext>()
.AddDefaultTokenProviders();

//builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
//    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // ???? ? ????? ?????
    options.AccessDeniedPath = "/Account/AccessDenied"; // ???? ??? ?????? ? ???????
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // ????? ????? ????
    options.SlidingExpiration = true; // ????????? ???? ????????
});
#endregion Identity Configuration

// ??????????? DbInitializer
builder.Services.AddScoped<DbInitializer>();

// ?????????? ??????? ???????????
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
//    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
//});

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<RequestHub>("/requestHub");

#region DbInitialization
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
    if (app.Environment.IsDevelopment())
        await dbContext.Database.EnsureCreatedAsync();
    else
        await dbContext.Database.MigrateAsync();
}

// ????????????? ???? ?????? (add roles/user if needed)
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.InitializeAsync();
}
#endregion DbInitialization


var _logger = app.Services.GetRequiredService<ILogger<Program>>();
_logger.LogInformation($"??????? ????????? : {AppInfo.FraemworkInfo()}");
_logger.LogInformation($"?????? : {AppInfo.VersionApp()} ({AppInfo.DateRelease()})");


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// ?????????????? ???????? ??? Account controller
//app.MapControllerRoute(
//    name: "account",
//    pattern: "Account/{action=Login}/{id?}");

// hosting
var hostConfigBuilder = new ConfigurationBuilder()
    .SetBasePath(AppInfo.GetAppPath())
    .AddJsonFile("hosting.json", optional: true);
if (app.Environment.IsDevelopment())
    hostConfigBuilder.AddJsonFile("hosting.Development.json", optional: true);
var hostConfig = hostConfigBuilder.Build();
var hostString = hostConfig.GetValue<string>("server_urls") ?? AppInfo.server_urls_default;

app.Run(hostString);
