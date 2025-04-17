// Data/AppDbContext.cs

using Microsoft.EntityFrameworkCore;
using StockGTO.Models; // 引用 Employee 模型的位置

namespace StockGTO.Data
{
    // AppDbContext 是用來與資料庫互動的橋樑
    public class AppDbContext : DbContext
    {
        // 建構式：從外部注入資料庫設定參數
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet 對應到資料庫中的一張表格（Employees）
        public DbSet<Employee> Employees { get; set; }



        // DbSet 對應到資料庫中的一張表格（Posts）
        public DbSet<Post> Posts { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2); // 設定薪資欄位的精度為 18 位數，小數點後 2 位
        }


    }

}
