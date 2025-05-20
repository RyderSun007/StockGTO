using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StockGTO.Data;
using StockGTO.Hubs;
using DotNetEnv;

namespace StockGTO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ✅ 載入 .env 檔（支援本機與 VM 部署）
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            // ✅ 強制指定 URL，避免 fallback 到 launchSettings.json 的 7045
            builder.WebHost.UseUrls("http://localhost:5000");

            // ✅ 僅在本機開發模式下綁定 5000 / 7045 Port（含 HTTPS）
            if (builder.Environment.IsDevelopment())
            {
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ListenLocalhost(5000); // HTTP 測試
                    serverOptions.ListenLocalhost(7045, listenOptions =>
                    {
                        listenOptions.UseHttps(); // HTTPS 測試
                    });
                });

                Console.WriteLine("✅ 開發環境：開啟 5000/7045 Port for Localhost");
            }
            else
            {
                // ✅ 生產環境不綁 port，由 Nginx Proxy 負責處理
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ListenAnyIP(5000); // Nginx 代理 HTTP
                });

                Console.WriteLine("🚀 生產環境（VM）：由 Nginx 接管 Port");
            }

            // ✅ 加入 JSON + 環境變數設定來源
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // ✅ 從環境變數取得連線字串
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            // =======================
            // 服務註冊區（Service Container）
            // =======================
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddSignalR(); // ✅ 加入 SignalR for WebSocket

            // ✅ Cookie 驗證 + Google 登入
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

            var app = builder.Build();

            // =======================
            // 中介層 Pipeline 設定區
            // =======================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<ArticleHub>("/ArticleHub"); // ✅ SignalR 路由

            app.Run();
        }
    }
}
