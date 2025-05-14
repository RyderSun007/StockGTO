using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;           // ✅ 引入 SignalR 功能
using StockGTO.Data;                          // ✅ 資料庫 DbContext
using StockGTO.Models;                        // ✅ 使用的資料模型
using StockGTO.ViewModels;                    // ✅ 首頁用的 ViewModel
using StockGTO.Hubs;                          // ✅ 我們自定義的 SignalR Hub

namespace StockGTO.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;         // ✅ 系統日誌用（錯誤或訊息記錄）
        private readonly AppDbContext _context;                   // ✅ 資料庫操作用
        private readonly IHubContext<ArticleHub> _hub;            // ✅ SignalR 推播用的物件（注入自 ArticleHub）

        // 🔧 建構子：在這裡注入 Logger、DbContext、SignalR Hub
        public HomeController(ILogger<HomeController> logger, AppDbContext context, IHubContext<ArticleHub> hub)
        {
            _logger = logger;
            _context = context;
            _hub = hub;
        }

        // 🏠 首頁：回傳 ViewModel 給 Razor 頁面
        public IActionResult Index()
        {
            // 1️⃣ 撈出啟用中的分類（IsActive = true）
            var categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToList();

            // 2️⃣ 建立分類與文章清單的對應表 Dictionary
            var categoryArticles = new Dictionary<string, List<ArticlePost>>();
            foreach (var cat in categories)
            {
                // 每一個分類取最新 5 篇文章（包含分類資料）
                var posts = _context.ArticlePosts
                    .Include(a => a.Category)
                    .Where(a => a.CategoryId == cat.Id)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToList();

                // 將結果存入 Dictionary，key 用分類代碼（cat.Code）
                categoryArticles[cat.Code] = posts;
            }

            // 3️⃣ 建立一個 ViewModel 傳給前端 Razor 頁面使用
            var viewModel = new HomeViewModel
            {
                // 首頁主視覺輪播新聞（拉 3 筆）
                IndexPosts = _context.IndexPosts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3)
                    .ToList(),

                // 靈魂語錄清單（最新 3 筆）
                SoulQuotes = _context.SoulQuotes
                    .OrderByDescending(q => q.CreatedAt)
                    .Take(3)
                    .ToList(),

                // 首頁小快訊清單（顯示順序按 Position 排）
                IndexNews = _context.IndexNews
                    .Where(n => n.IsActive)
                    .OrderBy(n => n.Position)
                    .ToList(),

                // 全部最新文章（只取 10 筆）
                ArticlePosts = _context.ArticlePosts
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToList(),

                // 熱門文章（依瀏覽數倒序）
                TopViewedArticles = _context.ArticlePosts
                    .OrderByDescending(a => a.ViewCount)
                    .Take(5)
                    .ToList(),

                // 分類對應的文章列表（剛剛建立的 Dictionary）
                CategoryArticles = categoryArticles
            };

            return View(viewModel); // ✅ 回傳首頁 Razor 頁面與資料
        }



       





        // 🔄 SignalR 推播功能（可以從前端或其他 Action 呼叫來「即時送資料」）
        public async Task<IActionResult> PushArticles()
        {
            // 1️⃣ 從資料庫撈最新 5 篇文章（包含分類資料）
            var articles = _context.ArticlePosts
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.ImageUrl,
                    CategoryName = a.Category.Name
                })
                .ToList();

            // 2️⃣ 使用 SignalR 廣播文章資料給所有前端連線者（叫 ReceiveArticles）
            await _hub.Clients.All.SendAsync("ReceiveArticles", articles);

            // 3️⃣ 回傳 JSON 告知推送成功（可以被前端 AJAX 呼叫）
            return Ok(new { status = "pushed", count = articles.Count });
        }

        // 🔐 內建的隱私頁面（不影響主流程）
        public IActionResult Privacy()
        {
            return View();
        }

        // ❗ 內建錯誤頁面（顯示例外錯誤用）
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
