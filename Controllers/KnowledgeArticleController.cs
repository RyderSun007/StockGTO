using Microsoft.AspNetCore.Mvc;
using StockGTO.Data;
using StockGTO.Models;
using System;
using System.Linq;
using PagedList.Core;
using X.PagedList;
using Microsoft.EntityFrameworkCore;


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
        // ✅ 後台文章總表：支援分類與標題關鍵字搜尋
        // ✅ 顯示全部知識文章
        public IActionResult Index(string keyword, int? categoryId)
        {
            var query = _context.ArticlePosts.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(a => a.Title.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId);

            var articles = query
                .Where(a => a.Content != null)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryList = _context.Categories.Where(c => c.IsActive).ToList();

            return View(articles);
        }

        public IActionResult Details(int id)
        {
            var article = _context.ArticlePosts
                .Include(a => a.Category)
                .FirstOrDefault(a => a.Id == id);

            if (article == null)
                return NotFound();

            // ✅ 增加瀏覽次數
            article.ViewCount += 1;
            _context.SaveChanges(); // 記得儲存

            // 🔍 上一篇 / 下一篇文章（同分類）
            ViewBag.PreviousArticle = _context.ArticlePosts
                .Where(a => a.CategoryId == article.CategoryId && a.Id < article.Id)
                .OrderByDescending(a => a.Id)
                .FirstOrDefault();

            ViewBag.NextArticle = _context.ArticlePosts
                .Where(a => a.CategoryId == article.CategoryId && a.Id > article.Id)
                .OrderBy(a => a.Id)
                .FirstOrDefault();

            return View(article);
        }


        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();

            return View(new ArticlePost());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ArticlePost post)
        {
            if (ModelState.IsValid)
            {
                if (post.ImageFile != null && post.ImageFile.Length > 0)
                {
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "aznews", "uploads");
                    Directory.CreateDirectory(uploadDir);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(post.ImageFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        post.ImageFile.CopyTo(stream);
                    }

                    post.ImageUrl = "/aznews/uploads/" + fileName;
                }

                if (string.IsNullOrEmpty(post.ImageUrl))
                {
                    post.ImageUrl = "/images/default.jpg";
                }

                post.CreatedAt = DateTime.Now;
                _context.ArticlePosts.Add(post);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            // ❗修正這裡
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();

            return View(post);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var article = _context.ArticlePosts.FirstOrDefault(a => a.Id == id);
            if (article == null)
                return NotFound();

            ViewBag.CategoryList = _context.Categories.Where(c => c.IsActive).ToList();
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ArticlePost post)
        {
            if (id != post.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = _context.ArticlePosts.FirstOrDefault(a => a.Id == id);
                if (existing == null) return NotFound();

                if (post.ImageFile != null && post.ImageFile.Length > 0)
                {
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "aznews", "uploads");
                    Directory.CreateDirectory(uploadDir);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(post.ImageFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        post.ImageFile.CopyTo(stream);
                    }

                    existing.ImageUrl = "/aznews/uploads/" + fileName;
                }
                else
                {
                    existing.ImageUrl = post.ImageUrl;
                }

                existing.Title = post.Title;
                existing.Content = post.Content;
                existing.Author = post.Author;
                existing.IsPinned = post.IsPinned;
                existing.CategoryId = post.CategoryId;

                _context.SaveChanges();
                return RedirectToAction("Details", new { id = post.Id });
            }

            ViewBag.CategoryList = _context.Categories.Where(c => c.IsActive).ToList();
            return View(post);
        }

        public IActionResult Delete(int id)
        {
            var post = _context.ArticlePosts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var post = _context.ArticlePosts.Find(id);
            if (post != null)
            {
                _context.ArticlePosts.Remove(post);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult PublicList(string keyword, int? categoryId)
        {
            var query = _context.ArticlePosts
                .Include(a => a.Category) // 🧠 記得 Include 類別名稱
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(a => a.Title.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId.Value);

            var articles = query
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;

            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToList();

            return View(articles);
        }



    }
}