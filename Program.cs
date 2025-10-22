using System.Globalization;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;                    // ★ EPPlus
using StockGTO.Data;
using StockGTO.Models;
using StockGTO.Services;

namespace StockGTO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ==== 0) 載入 .env（優先於 appsettings），註冊編碼供 CSV/Big5 使用 ====
            Env.Load();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // ==== 1) 建置 Builder ====
            var builder = WebApplication.CreateBuilder(args);

            // ★ EPPlus 授權（避免正式機 LicenseException）
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 如有商用授權改成 Commercial

            // 綁定 5000（由 Nginx 反代）
            builder.WebHost.UseUrls("http://0.0.0.0:5000");
            if (builder.Environment.IsDevelopment())
            {
                builder.WebHost.ConfigureKestrel(o =>
                {
                    o.ListenLocalhost(5000);
                    o.ListenLocalhost(7045, lo => lo.UseHttps());
                });
            }
            else
            {
                builder.WebHost.ConfigureKestrel(o =>
                {
                    o.ListenAnyIP(5000);
                    // 可選：限制單連線/要求大小等（視需求打開）
                    // o.Limits.MaxRequestBodySize = 20 * 1024 * 1024; // 20MB
                });
            }

            // 設定載入順序：appsettings → appsettings.{ENV}.json → 環境變數
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // 連線字串（優先吃環境變數）
            var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                          ?? builder.Configuration.GetConnectionString("DefaultConnection");

            // ==== 2) Service ====
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(connStr));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = "Google";
            })
            .AddCookie()
            .AddGoogle("Google", o =>
            {
                o.ClientId = Environment.GetEnvironmentVariable("Authentication__Google__ClientId")
                             ?? builder.Configuration["Authentication:Google:ClientId"];
                o.ClientSecret = Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret")
                                 ?? builder.Configuration["Authentication:Google:ClientSecret"];
            });

            // 只需要註冊一次
            builder.Services.AddScoped<O_HR_ControlService>();

            // ★ 反代標頭（Nginx）—保留你的設定
            builder.Services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
                o.RequireHeaderSymmetry = false;
                o.KnownProxies.Clear(); // 在雲/動態 IP 情境較寬鬆
            });

            // ★ 上傳大小限制（20MB，避免匯入 Excel 被擋）
            builder.Services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20MB
            });

            // ★ 統一文化區（避免 Linux 日期/小數點差異）
            var zhTw = CultureInfo.GetCultureInfo("zh-TW");
            builder.Services.Configure<RequestLocalizationOptions>(opts =>
            {
                opts.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(zhTw);
                opts.SupportedCultures = new List<CultureInfo> { zhTw };
                opts.SupportedUICultures = new List<CultureInfo> { zhTw };
            });

            // ==== 3) Build App ====
            var app = builder.Build();

            // 角色初始化（沿用）
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                SeedRoles.InitializeAsync(services).GetAwaiter().GetResult();
            }

            // ★ 放在最前面：轉換反代標頭 → 正確產生 Redirect/絕對網址
            app.UseForwardedHeaders();

            // ★ 正式機錯誤處理：改走 /error/500（之後我們會給你 ErrorController 與 View）
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/error/500");                 // 全域未處理例外
                //app.UseStatusCodePagesWithReExecute("/error/{0}");     // 404/403… 統一頁
                app.UseHsts();
            }

            // HTTPS / 靜態檔
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // 文化區套用（要在路由前）
            app.UseRequestLocalization();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // ==== 4) Routing ====
            // HR 路由（先綁，避免被 default 蓋到）
            app.MapControllerRoute(
                name: "hr",
                pattern: "O_HR_Control/{action=Index}/{id?}",
                defaults: new { controller = "O_HR_Control" });

            // Default
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // 特例：/Logout
            app.MapControllerRoute(
                name: "logout",
                pattern: "Logout",
                defaults: new { controller = "Account", action = "Logout" });

            app.Run();
        }
    }
}
