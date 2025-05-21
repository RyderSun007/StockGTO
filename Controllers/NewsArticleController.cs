using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StockGTO.Data;
using StockGTO.Models;
using System;
using System.Linq;

namespace StockGTO.Controllers
{
    [Authorize] // ✅ 後台管理頁一律需登入
    public class NewsArticleController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public NewsArticleController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ 後台管理頁面（含搜尋、篩選、分頁）
        public IActionResult Manage(string? keyword, string? category, bool? showHome, bool? isActive, int page = 1)
        {
            var query = _context.NewsArticles.AsQueryable();

            // 🔍 關鍵字搜尋
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Title.Contains(keyword) || x.Summary.Contains(keyword));

            // 📂 分類篩選
            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.Category == category);

            // 🏠 是否顯示在首頁
            if (showHome.HasValue)
                query = query.Where(x => x.ShowOnHomepage == showHome);

            // ✅ 是否啟用
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive);

            // 分頁
            var totalCount = query.Count();
            var articles = query
                .OrderByDescending(x => x.PublishDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // 📦 包裝 ViewModel
            var viewModel = new NewsArticleViewModel
            {
                SearchKeyword = keyword,
                Category = category,
                ShowOnHomepage = showHome,
                IsActive = isActive,
                CurrentPage = page,
                PageSize = PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize),
                NewsList = articles
            };

            return View(viewModel);
        }

        // 🆕 新增文章表單（GET）
        public IActionResult Create() => View();

        // 📝 新增文章處理（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NewsArticle article)
        {
            if (!ModelState.IsValid)
                return View(article);

            article.PublishDate = DateTime.Now;
            article.Views = 0;
            _context.NewsArticles.Add(article);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "✅ 文章新增成功！";
            return RedirectToAction("Manage");
        }

        // ✏️ 編輯文章（GET）
        public IActionResult Edit(int id)
        {
            var article = _context.NewsArticles.Find(id);
            if (article == null) return NotFound();
            return View(article);
        }

        // ✏️ 編輯送出（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NewsArticle article)
        {
            if (!ModelState.IsValid)
                return View(article);

            _context.NewsArticles.Update(article);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "✅ 文章更新完成！";
            return RedirectToAction("Manage");
        }

        // 🗑️ 刪除文章
        public IActionResult Delete(int id)
        {
            var article = _context.NewsArticles.Find(id);
            if (article != null)
            {
                _context.NewsArticles.Remove(article);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "🗑️ 文章已刪除。";
            }

            return RedirectToAction("Manage");
        }

        // 🔍 檢視單篇文章（用於前台或開發預覽）
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var article = _context.NewsArticles.Find(id);
            if (article == null) return NotFound();
            return View(article);
        }

        // 📂 文章分類選項（用於下拉選單）
        public static List<SelectListItem> Categories => new List<SelectListItem>
        {
            new SelectListItem("台灣市場", "台灣市場"),
            new SelectListItem("中國市場", "中國市場"),
            new SelectListItem("國際財經", "國際財經"),
        };
    }
}
