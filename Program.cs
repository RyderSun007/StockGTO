using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StockGTO.Data;
using StockGTO.Hubs;
using DotNetEnv;
using StockGTO.Models;

namespace StockGTO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ✅ 載入 .env 檔（支援本機與 VM 部署）
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

             //✅ 強制指定 URL，避免 fallback 到 launchSettings.json 的 7045
            builder.WebHost.UseUrls("http://0.0.0.0:5000");

            // ✅ 根據環境選擇要綁定的 port
            if (builder.Environment.IsDevelopment())
            {
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ListenLocalhost(5000); // 本機 HTTP 測試
                    serverOptions.ListenLocalhost(7045, listenOptions =>
                    {
                        listenOptions.UseHttps(); // 本機 HTTPS 測試
                    });
                });

                Console.WriteLine("✅ 開發環境：開啟 5000/7045 Port for Localhost");
            }
            else
            {
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ListenAnyIP(5000); // VM 用 Nginx Proxy
                });

                Console.WriteLine("🚀 生產環境：Nginx Proxy 接管，Kestrel 綁定 5000");
            }

            // ✅ 設定組態
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // ✅ 資料庫連線字串
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            // ✅ 註冊服務（依需求加入）
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddSignalR();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Google";
            })
            .AddCookie()
            .AddGoogle("Google", options =>
            {
                options.ClientId = Environment.GetEnvironmentVariable("Authentication__Google__ClientId");
                options.ClientSecret = Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret");
            });

            // ✅ ForwardedHeaders 支援：讓 ASP.NET Core 知道外面是 HTTPS
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                options.RequireHeaderSymmetry = false;
                options.KnownProxies.Clear(); // 如果你有特定 Proxy IP 可加進來
            });

            var app = builder.Build();


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                SeedRoles.InitializeAsync(services).GetAwaiter().GetResult(); // 🪛 用同步呼叫 await
            }


            // =======================
            // 中介層 Pipeline 設定區
            // =======================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // ✅ 最重要：處理 Nginx 傳進來的 HTTPS 代理頭
            app.UseForwardedHeaders();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            // 🔥 額外手動註冊 /Logout → 對應 AccountController.Logout()
            app.MapControllerRoute(
                name: "logout",
                pattern: "Logout",
                defaults: new { controller = "Account", action = "Logout" });

            app.MapHub<ArticleHub>("/ArticleHub");

            app.Run();
        }
    }
}
