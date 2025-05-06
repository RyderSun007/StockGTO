// PostController.cs
using Microsoft.AspNetCore.Mvc;
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

        // ✅ 主頁：可篩選分類與關鍵字搜尋 + 分頁
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

        // ✅ 管理介面（CRUD 管理頁）
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

        // ✅ 單篇詳細頁
        public IActionResult Details(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        // ✅ 新增文章頁
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
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

        // ✅ 編輯文章
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
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

        // ✅ 刪除文章
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            _context.SaveChanges();
            return RedirectToAction("Manage");
        }

        // ✅ AJAX 新增文章
        [HttpPost]
        public IActionResult CreateFromAjax([FromBody] Post post)
        {
            if (string.IsNullOrEmpty(post.Title) || string.IsNullOrEmpty(post.Content))
                return BadRequest("請填寫標題與內容");

            post.CreatedAt = DateTime.Now;
            _context.Posts.Add(post);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ✅ 取得文章 JSON（預備編輯用）
        [HttpGet]
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

        // ✅ CreateManage 取代傳統 Create 功能（表單在 Post/Manage 裡）
        [HttpGet]
        public IActionResult CreateManage()
        {
            return View();
        }

        [HttpPost]
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