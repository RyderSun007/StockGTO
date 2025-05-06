using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Models;

using System.IO;
using System.Diagnostics.Metrics;
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

        // 首頁
        public IActionResult IndexV2()
        {
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

                ArticlePosts = _context.ArticlePosts
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(3)
                    .ToList()
            };

            return View(viewModel);
        }


        public IActionResult Index()
        {
            var viewModel = new HomeViewModel
            {
                IndexPosts = _context.IndexPosts
                             .OrderByDescending(p => p.CreatedAt)
                             .Take(3).ToList(),

                SoulQuotes = _context.SoulQuotes
                             .OrderByDescending(q => q.CreatedAt)
                             .Take(3).ToList(),

                IndexNews = _context.IndexNews
                             .Where(n => n.IsActive)
                             .OrderBy(n => n.Position)
                             .ToList()
            };

            return View(viewModel);
        }



        // 隱私權頁
        public IActionResult Privacy()
        {
            return View();
        }

        // 錯誤頁
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // 模板總覽頁（圖文牆）
        public IActionResult Templates()
        {
            var templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");

            var folders = Directory
                .GetDirectories(templatesPath)
                .Where(dir => System.IO.File.Exists(Path.Combine(dir, "index.html")))
                .Select(dir => new TemplateViewModel
                {
                    FolderName = Path.GetFileName(dir),
                    Category = GetCategoryByFolder(Path.GetFileName(dir))
                })
                .OrderBy(x => x.FolderName)
                .ToList();

            ViewBag.TemplateFolders = folders;
            return View();
        }

        // 分類邏輯：可未來換成 JSON、資料庫、AI 分析也行
        private static  string GetCategoryByFolder(string folderName)
        {
            folderName = folderName.ToLower();
            if (folderName.Contains("admin")) return "後台管理";
            if (folderName.Contains("shop")) return "電商購物";
            if (folderName.Contains("blog")) return "部落格";
            if (folderName.Contains("dashboard") || folderName.Contains("dash")) return "資料儀表板";
            if (folderName.Contains("host") || folderName.Contains("eco")) return "主機託管";
            if (folderName.Contains("food") || folderName.Contains("cafe")) return "餐飲美食";
            if (folderName.Contains("master") || folderName.Contains("cafe")) return "專業設計";
            if (folderName.Contains("main") || folderName.Contains("cafe")) return "主題設計";
            
            return "未分類";
        }

        
    }
}
