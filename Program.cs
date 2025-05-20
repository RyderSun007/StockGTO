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

            // ✅ 僅在「本機開發模式」才綁定 5000 / 7045 Port，避免 VM 上炸 port
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

                // ✅ 顯示目前環境狀態（方便偵錯）
                Console.WriteLine("✅ 開發環境：開啟 5000/7045 Port for Localhost");
            }
            else
            {
                Console.WriteLine("🚀 生產環境（VM）：由 Nginx 接管 Port");
            }

            // ✅ 加入 JSON + 環境變數設定來源（環境變數優先）
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables(); // 環境變數機制

            // ✅ 從環境變數取得連線字串（可切換環境）
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

            // ✅ Cookie 驗證 + Google 登入（支援多平台驗證）
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
