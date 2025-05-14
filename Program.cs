using Microsoft.EntityFrameworkCore;      // 資料庫 EF Core
using StockGTO.Data;                      // 你的 DbContext 類別
using StockGTO.Hubs; // ← WebSocket


namespace StockGTO
{
    public class Program
    {
        public static void Main(string[] args)
        {


            var builder = WebApplication.CreateBuilder(args);

            // =======================
            // 服務註冊區（Service Container）
            // =======================

            // 加入 MVC 控制器與視圖支援（標準）
            builder.Services.AddControllersWithViews();

            // 加入資料庫連線服務（從 appsettings.json 抓 DefaultConnection）
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 加入 Session 功能（伺服器記憶使用者資訊）
            builder.Services.AddSession();

            // 加入授權機制（如果有用到 [Authorize] 屬性）
            builder.Services.AddAuthorization();

            //註冊 SignalR 的關鍵
            builder.Services.AddSignalR(); // WebSocket 


            var app = builder.Build();






            // =======================
            // 中介層設定區（Middleware Pipeline）
            // =======================

            // 生產環境例外處理設定
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // 強制使用 HTTPS
            app.UseHttpsRedirection();

            // 啟用讀取 wwwroot 的靜態檔案（CSS、JS、圖片）
            app.UseStaticFiles();

            // 啟用路由系統
            app.UseRouting();

            // 啟用 Session 中介層（必須放在 UseRouting() 後面）
            app.UseSession();

            // 啟用授權驗證（如果 Controller 上有寫 [Authorize]）
            app.UseAuthorization();



            

            // =======================
            // 路由對應設定
            // =======================

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // 啟動網站應用程式（開始接受外部請求）
            // 🔥 ArticleHub WebSocket！
            app.MapHub<ArticleHub>("/articleHub");


            app.Run();
           
        }

    }
}
