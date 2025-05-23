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
            var user = await _userManager.GetUserAsync(User);
            var query = _context.ArticlePosts.AsQueryable();

            // ✅ 如果不是 Admin，只能看自己的文章
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(a => a.UserId == user.Id);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(a => a.Title.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId);

            var articles = await query
                .Include(a => a.Category)
                 .Include(a => a.User)

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
                 .Include(a => a.User)
                .Include(a => a.Category)
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
        .Include(a => a.User) // 帶出作者
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

            // ✅ 加上相關文章邏輯（最多4篇，同分類，排除自己）
            ViewBag.RelatedArticles = _context.ArticlePosts
                .Include(a => a.User) // ✅ 帶出作者
                .Where(a => a.CategoryId == article.CategoryId && a.Id != article.Id)
                .OrderByDescending(a => a.CreatedAt)
                .Take(4)
                .ToList();

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
        public async Task<IActionResult> Create(ArticlePost post, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
                return View(post);
            }

            // 加上目前使用者 ID
            var user = await _userManager.GetUserAsync(User);
            post.UserId = user.Id;
            post.CreatedAt = DateTime.Now;

            // 處理圖片上傳
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "aznews", "assets", "img", "posts");
                Directory.CreateDirectory(uploadDir);
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                post.ImageUrl = "/aznews/assets/img/posts/" + fileName;
            }
            else if (string.IsNullOrEmpty(post.ImageUrl))
            {
                // 如果沒填網址也沒上傳檔案，給預設圖
                post.ImageUrl = "/images/default.jpg";
            }

            _context.ArticlePosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage");
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var post = await _context.ArticlePosts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            if (post.UserId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            ViewBag.CategoryList = _context.Categories.Where(c => c.IsActive).ToList();
        
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, ArticlePost post)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.ArticlePosts.FirstOrDefaultAsync(a => a.Id == id);

            if (existing == null) return NotFound();

            if (existing.UserId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                existing.Title = post.Title;
                existing.Content = post.Content;
                existing.CategoryId = post.CategoryId;

                // ✅ 無論有沒有選圖，都保留或更新圖片網址
                if (post.ImageFile != null && post.ImageFile.Length > 0)
                {
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "aznews", "assets", "img", "posts");
                    Directory.CreateDirectory(uploadDir);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(post.ImageFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await post.ImageFile.CopyToAsync(stream);
                    }

                    existing.ImageUrl = "/aznews/assets/img/posts/" + fileName;
                }
                else
                {
                    // ✅ 沒選圖就保留原來網址（表單要有 <input type="hidden" asp-for="ImageUrl" />）
                    existing.ImageUrl = post.ImageUrl;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("MemberCenter", "Member");
            }

            // ⛑ 防止 ViewBag null 錯誤
            ViewBag.CategoryList = _context.Categories.Where(c => c.IsActive).ToList();
            return View(post);
        }



        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.ArticlePosts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (post.UserId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.ArticlePosts.FindAsync(id);
            if (post == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (post.UserId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.ArticlePosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("MemberCenter", "Member");
        }
    }
}
