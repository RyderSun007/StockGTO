namespace StockGTO.Models
{
    public class NewsArticleViewModel
    {
        // ✅ 查詢用欄位
        public string? SearchKeyword { get; set; }
        public string? Category { get; set; }
        public bool? IsActive { get; set; }
        public bool? ShowOnHomepage { get; set; }

        // ✅ 分頁資訊
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // ✅ 查詢結果
        public List<NewsArticle> NewsList { get; set; } = new List<NewsArticle>();
    }
}
