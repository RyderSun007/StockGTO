using StockGTO.Models;
using System.Collections.Generic;

namespace StockGTO.ViewModels
{
    public class HomeViewModel
    {
        

        

        // 📣 焦點新聞
        public List<IndexNews> IndexNews { get; set; } = new();

        // 🆕 最新文章（不限分類）
        public List<ArticlePost> ArticlePosts { get; set; } = new();

        // 🔝 熱門文章（依據 ViewCount）
        public List<ArticlePost> TopViewedArticles { get; set; } = new();

        // 🔁 各分類對應文章（Key 是分類名稱）
        public Dictionary<string, List<ArticlePost>> CategoryArticles { get; set; } = new();
    }
}
