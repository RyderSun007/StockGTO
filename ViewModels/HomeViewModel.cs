// ViewModels/HomeViewModel.cs

using StockGTO.Models;
using System.Collections.Generic;

namespace StockGTO.ViewModels
{
    public class HomeViewModel
    {
        public List<IndexPost> IndexPosts { get; set; } = new();
        public List<SoulQuote> SoulQuotes { get; set; } = new(); // 如果有語錄功能

        public List<ArticlePost> ArticlePosts { get; set; } = new();
        public List<IndexNews> IndexNews { get; set; }
    }
}
