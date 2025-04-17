using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data; // 你自己的資料庫上下文 AppDbContext

var builder = WebApplication.CreateBuilder(args);

// 🗄️ 資料庫連線（從 appsettings.json 取得）
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 📦 加入 MVC 架構支援（控制器 + Views）
builder.Services.AddControllersWithViews();

// 🔐（可選）Windows 驗證機制
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

// 🛡️ 預設所有請求都需經過授權
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// 🧱 Razor Pages（如果你會用）
builder.Services.AddRazorPages();

// ✅ ⭐ 加入 Session 支援
builder.Services.AddSession();

var app = builder.Build();

// ⚠️ 錯誤處理機制（正式環境）
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 🚦 中介軟體管線
app.UseHttpsRedirection();    // 自動導向 HTTPS
app.UseStaticFiles();         // 使用 wwwroot 靜態資源

app.UseRouting();             // 啟用路由功能

// ✅ ⭐ 啟用 Session（順序要對）
app.UseSession();

app.UseAuthentication();      // 啟用驗證（如果你用到）
app.UseAuthorization();       // 啟用授權保護

// 🧭 預設路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
