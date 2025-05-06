using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Models;
using System;
using System.Linq;

namespace StockGTO.Controllers
{
    public class NewsArticleController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public NewsArticleController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ 管理頁：加上分類、是否首頁、是否啟用、關鍵字搜尋、分頁
        public IActionResult Manage(string? keyword, string? category, bool? showHome, bool? isActive, int page = 1)
        {
            var query = _context.NewsArticles.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Title.Contains(keyword) || x.Summary.Contains(keyword));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.Category == category);

            if (showHome.HasValue)
                query = query.Where(x => x.ShowOnHomepage == showHome);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive);

            var totalCount = query.Count();

            var articles = query
                .OrderByDescending(x => x.PublishDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

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

        public IActionResult Create() => View();

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
            TempData["SuccessMessage"] = "文章新增成功！";
            return RedirectToAction("Manage");
        }

        public IActionResult Edit(int id)
        {
            var article = _context.NewsArticles.Find(id);
            if (article == null) return NotFound();
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(NewsArticle article)
        {
            if (!ModelState.IsValid)
                return View(article);

            _context.NewsArticles.Update(article);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "文章更新完成！";
            return RedirectToAction("Manage");
        }

        public IActionResult Delete(int id)
        {
            var article = _context.NewsArticles.Find(id);
            if (article != null)
            {
                _context.NewsArticles.Remove(article);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "文章已刪除。";
            }
            return RedirectToAction("Manage");
        }

        public IActionResult Details(int id)
        {
            var article = _context.NewsArticles.Find(id);
            if (article == null) return NotFound();
            return View(article);
        }

        public static List<SelectListItem> Categories => new List<SelectListItem>
        {
            new SelectListItem("台灣市場", "台灣市場"),
            new SelectListItem("中國市場", "中國市場"),
            new SelectListItem("國際財經", "國際財經"),
        };
    }
}
