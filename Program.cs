using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Hubs;

namespace StockGTO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // 網站從 Linux 的外部也可以連
            
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenLocalhost(5000); // ✅ 只開放給 Nginx 內部連線用
            });

            // =======================
            // 服務註冊區（Service Container）
            // =======================

            // 加入 MVC 控制器與視圖支援（標準）
            builder.Services.AddControllersWithViews();

            // 加入 Session（必須早於 Authentication）
            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 加入資料庫連線服務
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 加入 Identity 使用者驗證
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // 註冊 Cookie + 外部登入（Google / Facebook）
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = "你的 ClientId";
                googleOptions.ClientSecret = "你的 ClientSecret";
                googleOptions.CallbackPath = "/signin-google";  // ⬅️ 這行一定要有
            })
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = "你的 Facebook AppId";
                facebookOptions.AppSecret = "你的 Facebook AppSecret";
            });

            // 授權驗證
            builder.Services.AddAuthorization();

            // SignalR（WebSocket）
            builder.Services.AddSignalR();

            var app = builder.Build();

            // =======================
            // 中介層設定區（Middleware Pipeline）
            // =======================

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();         // ✅ Session 必須在 Routing 後，Authentication 前
            app.UseAuthentication();  // ✅ 登入流程
            app.UseAuthorization();   // ✅ 權限驗證

            // 預設路由
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // SignalR Hub 路由
            app.MapHub<ArticleHub>("/articleHub");

            app.Run();
        }
    }
}
