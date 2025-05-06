using System;
using System.ComponentModel.DataAnnotations;

namespace StockGTO.Models
{
    public class NewsArticle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "請輸入標題")]
        [Display(Name = "標題*")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "摘要")]
        public string Summary { get; set; } = string.Empty;
        [Required(ErrorMessage = "請輸入內容")]
        [Display(Name = "內容*")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "原始連結")]
        public string? SourceUrl { get; set; }

        [Display(Name = "來源名稱")]
        public string? SourceName { get; set; }
        [Required(ErrorMessage = "請輸入類")]
        [Display(Name = "分類*")]
        public string? Category { get; set; }

        [Display(Name = "發佈時間")]
        public DateTime PublishDate { get; set; } = DateTime.Now;

        [Display(Name = "顯示在首頁")]
        public bool ShowOnHomepage { get; set; } = false;

        [Display(Name = "主圖網址")]
        public string? ImageUrl { get; set; }

        [Display(Name = "影片連結")]
        public string? VideoUrl { get; set; }

        [Display(Name = "作者")]
        public string? Author { get; set; } = "管理員";

        [Display(Name = "標籤（用 , 隔開）")]
        public string? Tags { get; set; }

        [Display(Name = "點閱次數")]
        public int Views { get; set; } = 0;

        [Display(Name = "啟用")]
        public bool IsActive { get; set; } = true;
    }
}
