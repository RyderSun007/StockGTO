// PostController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockGTO.Data;
using StockGTO.Models;
using StockGTO.Models.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;

namespace StockGTO.Controllers
{
    public class PostController : Controller
    {
        private readonly AppDbContext _context;

        public PostController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ 前台：文章瀏覽頁（含搜尋與分類）允許匿名訪問
        [AllowAnonymous]
        public IActionResult Post(string category, string search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Posts.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Author.Contains(search));
            }

            var pagedPosts = query.OrderByDescending(p => p.CreatedAt).ToPagedList(page, pageSize);

            ViewBag.AllCategories = _context.Posts
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category;

            return View(pagedPosts);
        }

        // ✅ 後台：文章管理介面（登入限定）
        [Authorize]
        public IActionResult Manage(int page = 1)
        {
            int pageSize = 10;
            var posts = _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .ToPagedList(page, pageSize);

            var viewModel = new ManagePostViewModel
            {
                PostList = posts
            };

            return View(viewModel);
        }

        // ✅ 單篇詳細頁（公開）
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        // ✅ 建立頁（登入限定）
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(Post post)
        {
            if (ModelState.IsValid)
            {
                post.CreatedAt = DateTime.Now;
                _context.Posts.Add(post);
                _context.SaveChanges();
                return RedirectToAction("Manage");
            }
            return View(post);
        }

        // ✅ 編輯文章（登入限定）
        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Edit(Post post)
        {
            if (!ModelState.IsValid) return View(post);

            var old = _context.Posts.FirstOrDefault(p => p.Id == post.Id);
            if (old == null) return NotFound();

            old.Title = post.Title;
            old.Content = post.Content;
            old.Category = post.Category;
            old.Author = post.Author;
            old.ImageUrl = post.ImageUrl;

            _context.SaveChanges();
            return RedirectToAction("Manage");
        }

        // ✅ 刪除文章（登入限定）
        [HttpPost]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            _context.SaveChanges();
            return RedirectToAction("Manage");
        }

        // ✅ AJAX 建立（登入限定）
        [HttpPost]
        [Authorize]
        public IActionResult CreateFromAjax([FromBody] Post post)
        {
            if (string.IsNullOrEmpty(post.Title) || string.IsNullOrEmpty(post.Content))
                return BadRequest("請填寫標題與內容");

            post.CreatedAt = DateTime.Now;
            _context.Posts.Add(post);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ✅ 取得 JSON（登入限定，預備前端編輯）
        [HttpGet]
        [Authorize]
        public IActionResult GetContent(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();

            return Json(new
            {
                title = post.Title,
                content = post.Content,
                category = post.Category,
                createdAt = post.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            });
        }

        // ✅ 新版建立頁（整合至 Manage）登入限定
        [HttpGet]
        [Authorize]
        public IActionResult CreateManage()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult CreateManage(Post post)
        {
            if (ModelState.IsValid)
            {
                post.CreatedAt = DateTime.Now;
                _context.Posts.Add(post);
                _context.SaveChanges();
                return RedirectToAction("Post");
            }
            return View(post);
        }
    }
}
