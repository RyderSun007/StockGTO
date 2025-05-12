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
        private readonly ILogger<HomeController> _logger; // 用來記錄系統 log，例如錯誤
        private readonly AppDbContext _context; // 資料庫物件，讓我們能操作資料表

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ✅ 首頁：網站一打開就會呼叫這個方法，回傳首頁的資料
        public IActionResult Index()
        {
            // 建立一個首頁用的 ViewModel，裡面包含多個區塊的資料
            var viewModel = new HomeViewModel
            {
                // 取得最新 3 筆「首頁公告」，用於跑馬燈或焦點新聞
                IndexPosts = _context.IndexPosts
                             .OrderByDescending(p => p.CreatedAt)
                             .Take(3)
                             .ToList(),

                // 取得最新 3 筆「靈魂語錄」，用於首頁下方語錄輪播
                SoulQuotes = _context.SoulQuotes
                             .OrderByDescending(q => q.CreatedAt)
                             .Take(3)
                             .ToList(),

                // 取得所有啟用中的「重大消息」，依照 Position 排序
                IndexNews = _context.IndexNews
                             .Where(n => n.IsActive)
                             .OrderBy(n => n.Position)
                             .ToList(),

                // 取得最新 10 筆「股票知識文章」，用於首頁 Weekly Top News 區塊
                ArticlePosts = _context.ArticlePosts
                             .Where(a => a.Category == "股票知識")
                             .OrderByDescending(a => a.CreatedAt)
                             .Take(10)
                             .ToList(),



                // 📘 財報分析
                FinancialStatements = _context.ArticlePosts
    .Where(a => a.Category == "FinancialStatements")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 技術分析
                TechnicalAnalysis = _context.ArticlePosts
    .Where(a => a.Category == "TechnicalAnalysis")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 基本面研究
                FundamentalAnalysis = _context.ArticlePosts
    .Where(a => a.Category == "FundamentalAnalysis")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 投資策略
                InvestmentStrategy = _context.ArticlePosts
    .Where(a => a.Category == "InvestmentStrategy")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 理財規劃
                WealthPlanning = _context.ArticlePosts
    .Where(a => a.Category == "WealthPlanning")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 產業分析
                IndustryInsight = _context.ArticlePosts
    .Where(a => a.Category == "IndustryInsight")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 趨勢觀察
                MarketTrends = _context.ArticlePosts
    .Where(a => a.Category == "MarketTrends")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 國際市場
                GlobalMarkets = _context.ArticlePosts
    .Where(a => a.Category == "GlobalMarkets")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 全球投資
                GlobalInvesting = _context.ArticlePosts
    .Where(a => a.Category == "GlobalInvesting")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 投資心理
                InvestorPsychology = _context.ArticlePosts
    .Where(a => a.Category == "InvestorPsychology")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 行為經濟學
                BehavioralFinance = _context.ArticlePosts
    .Where(a => a.Category == "BehavioralFinance")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 金融商品
                FinancialProducts = _context.ArticlePosts
    .Where(a => a.Category == "FinancialProducts")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),

                // 📘 衍生性工具
                Derivatives = _context.ArticlePosts
    .Where(a => a.Category == "Derivatives")
    .OrderByDescending(a => a.CreatedAt)
    .Take(5)
    .ToList(),


            };

            // 把整個 viewModel 傳到首頁 View 中（Index.cshtml）
            return View(viewModel);
        }

        // ✅ 隱私權聲明頁面：用於顯示個資保護政策等
        public IActionResult Privacy()
        {
            return View();
        }

        // ✅ 錯誤處理頁面：當網站發生錯誤會跳到這裡
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // 建立一個錯誤頁 ViewModel，顯示錯誤代碼
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
