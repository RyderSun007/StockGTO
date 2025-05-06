// /ViewModels/IndexNewsViewModel.cs
using StockGTO.Models;
using System.Collections.Generic;

namespace StockGTO.ViewModels
{
    public class IndexNewsViewModel
    {
        public IndexNews NewNews { get; set; } = new IndexNews();

        public List<IndexNews> NewsList { get; set; } = new List<IndexNews>();
    }
}
