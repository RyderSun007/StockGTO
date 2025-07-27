using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using StockGTO.Data;
using StockGTO.Models;
using StockGTO.ViewModels;
using StockGTO.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace StockGTO.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly IHubContext<ArticleHub> _hub;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IHubContext<ArticleHub> hub)
        {
            _logger = logger;
            _context = context;
            _hub = hub;
        }

        // 🏠 首頁：回傳分類與多模組文章給 ViewModel
        [AllowAnonymous]
        public IActionResult Index()
        {
            var categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToList();

            var categoryArticles = new Dictionary<string, List<ArticlePost>>();
            foreach (var cat in categories)
            {
                var posts = _context.ArticlePosts
                    .Include(a => a.Category)
                    //.Where(a => a.CategoryId == cat.Id && a.IsPublished && a.IsApproved)   // 暫時不用這條件
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToList();

                categoryArticles[cat.Code] = posts;
            }

            var viewModel = new HomeViewModel
            {
                
              
                IndexNews = _context.IndexNews
                    .Where(n => n.IsActive)
                    .OrderBy(n => n.Position)
                    .ToList(),
                // ⚠️ 測試期間暫時拿掉 IsPublished、IsApproved  .Where(a => a.IsPublished && a.IsApproved)
                ArticlePosts = _context.ArticlePosts
                    
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToList(),

                // ⚠️ 測試期間暫時拿掉 IsPublished、IsApproved    .Where(a => a.IsPublished && a.IsApproved)
                TopViewedArticles = _context.ArticlePosts
                   
                    .OrderByDescending(a => a.ViewCount)
                    .Take(5)
                    .ToList(),

                CategoryArticles = categoryArticles
            };

            return View(viewModel);
        }

        // 📃 關於我們
        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        // 🔄 SignalR 推播功能：將最新文章推送至前端
        // 📌 建議實際上線加上 [Authorize(Roles = "Admin")] 或 ApiKey 檢查
        [HttpGet]
        [AllowAnonymous] // ✅ 若你想開放給任何人觸發 SignalR 推播
        // [Authorize] // 🔒 若改為登入者或管理者才能推播，就啟用這行
        public async Task<IActionResult> PushArticles()
        {
            var articles = _context.ArticlePosts
                .Include(a => a.Category)
                .Where(a => a.IsPublished && a.IsApproved)
                .OrderByDescending(a => a.CreatedAt)
                .Take(9)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.ImageUrl,
                    CategoryName = a.Category.Name
                })
                .ToList();

            await _hub.Clients.All.SendAsync("ReceiveArticles", articles);
            return Json(articles);
        }

        // 🔐 隱私政策（可選）
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // ⚠️ 系統錯誤頁（會用到 ErrorViewModel.cs）
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public IActionResult Show()
        {
            return View();
        }
    }
}
