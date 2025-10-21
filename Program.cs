using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using StockGTO.Data;
using StockGTO.Models;
using StockGTO.Services;

namespace StockGTO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();
            var builder = WebApplication.CreateBuilder(args);

            // 綁定 5000（由 Nginx 轉發）
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
                builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(5000));
            }

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                          ?? builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connStr));
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
                o.ClientId = Environment.GetEnvironmentVariable("Authentication__Google__ClientId");
                o.ClientSecret = Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret");
            });

            // 只需要註冊一次
            builder.Services.AddScoped<O_HR_ControlService>();

            builder.Services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                o.RequireHeaderSymmetry = false;
                o.KnownProxies.Clear();
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                SeedRoles.InitializeAsync(services).GetAwaiter().GetResult();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseForwardedHeaders();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // ⭐ 先綁 HR 路由（底線名稱最保險）
            app.MapControllerRoute(
                name: "hr",
                pattern: "O_HR_Control/{action=Index}/{id?}",
                defaults: new { controller = "O_HR_Control" });

            // ⭐ 只保留一條 default
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // 特例：/Logout -> AccountController.Logout()
            app.MapControllerRoute(
                name: "logout",
                pattern: "Logout",
                defaults: new { controller = "Account", action = "Logout" });

            app.Run();
        }
    }
}
