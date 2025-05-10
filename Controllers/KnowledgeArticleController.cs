using Microsoft.AspNetCore.Mvc;
using StockGTO.Data;
using StockGTO.Models;
using System;
using System.Linq;

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
    }
}
