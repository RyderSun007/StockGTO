using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace StockGTO.Controllers
{
    public class KnowledgeArticleController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public KnowledgeArticleController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ 後台管理列表頁
        [Authorize]
        public async Task<IActionResult> Manage(string keyword, int? categoryId)
        {
            var query = _context.ArticlePosts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(a => a.Title.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId);

            var articles = await query
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();

            return View("Manage", articles);
        }

        // ✅ 前台公開列表頁
        [AllowAnonymous]
        public async Task<IActionResult> Browse(string keyword, int? categoryId)
        {
            var query = _context.ArticlePosts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(a => a.Title.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId);

            var articles = await query
                .Include(a => a.Category)
                //.Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();

            return View("Browse", articles);
        }

        public IActionResult Details(int id)
        {
            var article = _context.ArticlePosts
                .Include(a => a.Category)
                .FirstOrDefault(a => a.Id == id);

            if (article == null)
                return NotFound();

            article.ViewCount += 1;
            _context.SaveChanges();

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

        [HttpPost]
        public async Task<IActionResult> AddComment(int articleId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", new { id = articleId });

            var comment = new Comment
            {
                ArticlePostId = articleId,
                Content = content,
                UserId = user.Id,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = articleId });
        }

        [Authorize]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();

            return View(new ArticlePost());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticlePost post)
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
                        await post.ImageFile.CopyToAsync(stream);
                    }

                    post.ImageUrl = "/aznews/uploads/" + fileName;
                }
                else if (string.IsNullOrWhiteSpace(post.ImageUrl))
                {
                    post.ImageUrl = "/images/default.jpg";
                }

                var user = await _userManager.GetUserAsync(User);
                post.Author = user?.DisplayName ?? "未命名";
                post.UserId = user?.Id;
                post.CreatedAt = DateTime.Now;

                _context.ArticlePosts.Add(post);
                await _context.SaveChangesAsync();

                return RedirectToAction("Manage");
            }

            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();

            return View(post);
        }

        [Authorize]
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
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArticlePost post)
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
                        await post.ImageFile.CopyToAsync(stream);
                    }

                    existing.ImageUrl = "/aznews/uploads/" + fileName;
                }
                else if (!string.IsNullOrWhiteSpace(post.ImageUrl))
                {
                    existing.ImageUrl = post.ImageUrl;
                }
                else
                {
                    existing.ImageUrl = "/images/default.jpg";
                }

                existing.Title = post.Title;
                existing.Content = post.Content;
                existing.Author = post.Author;
                existing.IsPinned = post.IsPinned;
                existing.CategoryId = post.CategoryId;

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = post.Id });
            }

            ViewBag.CategoryList = _context.Categories.Where(c => c.IsActive).ToList();
            return View(post);
        }

        [Authorize]
        public IActionResult Delete(int id)
        {
            var post = _context.ArticlePosts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var post = _context.ArticlePosts.Find(id);
            if (post != null)
            {
                _context.ArticlePosts.Remove(post);
                _context.SaveChanges();
            }
            return RedirectToAction("Manage");
        }
    }
}
