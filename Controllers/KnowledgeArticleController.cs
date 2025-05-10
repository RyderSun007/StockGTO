using Microsoft.AspNetCore.Mvc;
using StockGTO.Data;
using StockGTO.Models;
using System;
using System.Linq;
using PagedList.Core;
using X.PagedList;


namespace StockGTO.Controllers
{
    public class KnowledgeArticleController : Controller
    {
        private readonly AppDbContext _context;

        public KnowledgeArticleController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ 顯示全部知識文章
        public IActionResult Index()
        {
            var articles = _context.ArticlePosts
                .Where(a => a.Category == "股票知識" && a.Content != null)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return View(articles);
        }

        // ✅ 顯示單篇文章
        public IActionResult Details(int id)
        {
            var article = _context.ArticlePosts.FirstOrDefault(a => a.Id == id);
            if (article == null || article.Category != "股票知識")
                return NotFound();

            return View(article);
        }

        // ✅ 建立新文章表單
        public IActionResult Create()
        {
            return View(new ArticlePost { Category = "股票知識" });
        }

        // ✅ 提交新文章
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ArticlePost post)
        {
            if (ModelState.IsValid)
            {
                post.CreatedAt = DateTime.Now;
                post.Category = "股票知識"; // 固定分類
                _context.ArticlePosts.Add(post);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // 🔧 編輯表單
        public IActionResult Edit(int id)
        {
            var post = _context.ArticlePosts.FirstOrDefault(p => p.Id == id && p.Category == "股票知識");
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ArticlePost post)
        {
            if (id != post.Id) return NotFound();

            if (ModelState.IsValid)
            {
                post.Category = "股票知識"; // 確保分類不被改掉
                _context.Update(post);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }



        // 🗑️ 刪除確認頁面
        public IActionResult Delete(int id)
        {
            var post = _context.ArticlePosts.FirstOrDefault(p => p.Id == id && p.Category == "股票知識");
            if (post == null) return NotFound();
            return View(post);
        }

        // ✅ 真正刪除資料
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var post = _context.ArticlePosts.Find(id);
            if (post != null && post.Category == "股票知識")
            {
                _context.ArticlePosts.Remove(post);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        // ✅ PublicList：支援搜尋 + 分類 + 分頁
        public IActionResult PublicList(string keyword, string category, int page = 1, int pageSize = 10)
        {
            var query = _context.ArticlePosts.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(a => a.Title.Contains(keyword));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(a => a.Category == category);

            var totalCount = query.Count();
            var articles = query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Keyword = keyword;
            ViewBag.Category = category;

            return View(articles);
        }



    }
}
