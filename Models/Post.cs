using System;
using System.ComponentModel.DataAnnotations;

namespace StockGTO.Models
{
    // 📌 Post 代表一篇文章或公告的資料結構
    public class Post
    {
        public int Id { get; set; }
        // 👉 主鍵，自動編號，每篇文章都有唯一 ID


        [Required]
        public string Title { get; set; } = string.Empty;
        // 👉 文章標題，例如「台積電爆量上漲！」。預設是空字串避免為 null


        [Required]
        public string Content { get; set; } = string.Empty;
        // 👉 文章內容，可以放完整描述或新聞內容。預設也是空字串 // 👉 自動對應 nvarchar(MAX)

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // 👉 建立時間，預設就是現在這一刻。幫你記錄這篇文章是什麼時候發表的🕒

        public string? Category { get; set; } = string.Empty;
        // 👉 文章分類，例如「財經」、「ETF」、「重要消息」等。允許是 null


        public string? ImageUrl { get; set; } // ✅ 新增圖片欄位
        public string Author { get; set; } = "未命名";
        public int LikeCount { get; set; } = 0;
    }
}
