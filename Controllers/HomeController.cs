using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Models;
using StockGTO.ViewModels;

namespace StockGTO.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 撈取分類與對應文章（每分類取前 5 筆）
            var categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToList();

            var categoryArticles = new Dictionary<string, List<ArticlePost>>();
            foreach (var cat in categories)
            {
                var posts = _context.ArticlePosts
                    .Include(a => a.Category)
                    .Where(a => a.CategoryId == cat.Id)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToList();

                categoryArticles[cat.Code] = posts;
            }

            // 建立 ViewModel 傳到 View
            var viewModel = new HomeViewModel
            {
                IndexPosts = _context.IndexPosts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3)
                    .ToList(),

                SoulQuotes = _context.SoulQuotes
                    .OrderByDescending(q => q.CreatedAt)
                    .Take(3)
                    .ToList(),

                IndexNews = _context.IndexNews
                    .Where(n => n.IsActive)
                    .OrderBy(n => n.Position)
                    .ToList(),

                ArticlePosts = _context.ArticlePosts
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToList(),

                TopViewedArticles = _context.ArticlePosts
                    .OrderByDescending(a => a.ViewCount)
                    .Take(5)
                    .ToList(),

                CategoryArticles = categoryArticles
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
