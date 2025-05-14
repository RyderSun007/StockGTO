// 引用 EF Core 套件，提供資料庫操作功能
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockGTO.Models;



namespace StockGTO.Data
{
    // 🧱 AppDbContext 是整個應用程式「資料庫的總管」
    // 它繼承自 Entity Framework Core 的 DbContext 類別
    // 負責連線資料庫、操作資料表
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // 📦 建構式：DI 容器會自動注入 DbContextOptions（包含連線字串、提供者等資訊）
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) // 將 options 傳給父類別 DbContext 使用
        {
        }

       

        // 🧾 定義 Posts 表格對應的資料模型
        public DbSet<Post> Posts { get; set; }

        // 🛠️ 自訂模型建立時的規則
        // 比如設定欄位型別、關聯、索引等等
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ⚠️ 請記得保留這行，確保內建規則不被覆蓋

           

        }
        // Data/AppDbContext.cs 2025/04/22
        


       

        // 🧾 定義 ArticlePosts 表格對應的資料模型2025/04/22
        public DbSet<ArticlePost> ArticlePosts { get; set; }

       

        

        // 🧾 定義 IndexNews 表格對應的資料模型2025/04/30
        public DbSet<IndexNews> IndexNews { get; set; }


        // 🧾 定義 NewsArticles 表格對應的資料模型2025/05/06
        public DbSet<NewsArticle> NewsArticles { get; set; }


        // 🧾 定義 Categories 分類 表格對應的資料模型2025/05/12
        public DbSet<Category> Categories { get; set; }




        public DbSet<LeaveRequest> LeaveRequests { get; set; }  // ✅ 請假資料表



    }
}
