using System;
using System.ComponentModel.DataAnnotations;

namespace StockGTO.Models
{
    public class IndexPost
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "請輸入標題")]
        [Display(Name = "標題")]
        public string Title { get; set; } = string.Empty;

        // ✅ 支援 HTML 內文（圖文混排）
        [Display(Name = "內容")]
        public string Content { get; set; } = string.Empty;


        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;



        [Display(Name = "分類")]
        public string? Category { get; set; } = "公告"; // 分類


        [Display(Name = "圖片網址")]
        public string? ImageUrl { get; set; } = "/images/default.jpg"; // 首圖



        [Display(Name = "作者")]
        public string? Author { get; set; } = "管理員"; // 誰發的



        public bool IsPinned { get; set; } = false; // 是否置頂

        public string? Slug { get; set; } // SEO 用網址（選填）
    }
}