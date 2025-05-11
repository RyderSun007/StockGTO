using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockGTO.Models
{
    public class ArticlePost
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty; // 支援 HTML 富內容

        public string? ImageUrl { get; set; } = "/images/default.jpg";

        public string? Author { get; set; } = "管理員";

        public string? Category { get; set; } = "文章";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Slug { get; set; }

        public bool IsPinned { get; set; } = false;

        [NotMapped] // 上傳檔案不進資料庫
        public IFormFile ImageFile { get; set; }
    }
}
