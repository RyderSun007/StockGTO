using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.SignalR; // 2025/10/02 停用 SignalR（若要恢復，移除註解）
// using StockGTO.Hubs;               // 2025/10/02 停用 Hub 注入
using StockGTO.Data;
using StockGTO.Models;
using StockGTO.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace StockGTO.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        // private readonly IHubContext<ArticleHub> _hub; // 2025/10/02 停用：Program.cs 未註冊 AddSignalR，避免 DI 解析失敗

        // 2025/10/02：移除 IHubContext<ArticleHub> 參數，避免
        // InvalidOperationException: Unable to resolve service for type IHubContext<ArticleHub>
        public HomeController(ILogger<HomeController> logger, AppDbContext context /*, IHubContext<ArticleHub> hub */)
        {
            _logger = logger;
            _context = context;
            // _hub = hub; // 2025/10/02 停用 SignalR
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
                    // .Where(a => a.CategoryId == cat.Id && a.IsPublished && a.IsApproved) // 暫時不用這條件
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

                // ⚠️ 測試期間暫時拿掉 IsPublished、IsApproved
                ArticlePosts = _context.ArticlePosts
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToList(),

                // ⚠️ 測試期間暫時拿掉 IsPublished、IsApproved
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

        // 🔄（暫停）SignalR 推播功能：目前只回傳資料，不做推播
        //    若要恢復：請在 Program.cs 加回 builder.Services.AddSignalR() 與 app.MapHub<ArticleHub>("/ArticleHub");
        //    然後把下方 _hub.Clients... 的註解移除，並在建構子加回 IHubContext<ArticleHub> 注入。
        [HttpGet]
        [AllowAnonymous] // ✅ 目前開放；若要限制就改成 [Authorize]
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

            // 2025/10/02 停用 SignalR 推播
            // await _hub.Clients.All.SendAsync("ReceiveArticles", articles);

            await Task.CompletedTask; // 佔位，避免沒有 await 的警告
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
