using Microsoft.AspNetCore.Mvc;
using StockGTO.Data;
using StockGTO.Models;
using System.Linq;

namespace StockGTO.Controllers
{
    public class ArticlePostController : Controller
    {
        private readonly AppDbContext _context;

        public ArticlePostController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 建立與管理合一的頁面
        public IActionResult CreateIndexPost()
        {
            ViewBag.IndexPosts = _context.IndexPosts.OrderByDescending(p => p.CreatedAt).ToList();
            return View(new IndexPost()); // 傳空模型
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateIndexPost(IndexPost post)
        {
            if (ModelState.IsValid)
            {
                post.CreatedAt = DateTime.Now;
                _context.IndexPosts.Add(post);
                _context.SaveChanges();

                ViewData["Success"] = "✅ 公告已成功新增";
                ModelState.Clear();
                return RedirectToAction(nameof(CreateIndexPost));
            }

            ViewBag.IndexPosts = _context.IndexPosts.OrderByDescending(p => p.CreatedAt).ToList();
            return View(post);
        }

        // ✅ 刪除確認畫面
        public IActionResult DeleteIndexPostView(int id)
        {
            var post = _context.IndexPosts.Find(id);
            if (post == null) return NotFound();
            return View("DeleteIndexPost", post);
        }

        // ✅ 實際刪除資料（傳統）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteIndexPostConfirmed(int id)
        {
            var post = _context.IndexPosts.Find(id);
            if (post != null)
            {
                _context.IndexPosts.Remove(post);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(CreateIndexPost));
        }



        // 額外支援直接 /DeleteIndexPost/{id}
        [HttpGet]
        public IActionResult DeleteIndexPost(int id)
        {
            var post = _context.IndexPosts.Find(id);
            if (post == null) return NotFound();
            return View("DeleteIndexPost", post); // 使用相同 View 呈現
        }





        // 🔧 編輯頁面（傳統）
        public IActionResult EditIndexPost(int id)
        {
            var post = _context.IndexPosts.Find(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditIndexPost(int id, IndexPost post)
        {
            if (id != post.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(post);
                _context.SaveChanges();
                return RedirectToAction(nameof(CreateIndexPost));
            }
            return View(post);
        }

        // 🔎 詳細頁面
        public IActionResult DetailsIndexPost(int id)
        {
            var post = _context.IndexPosts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        // ✅ 管理專用分頁（備用）
        public IActionResult ManageIndexPost()
        {
            var posts = _context.IndexPosts.OrderByDescending(p => p.CreatedAt).ToList();
            return View(posts);
        }



        // ✅ AJAX 新增：送資料用
        [HttpPost]
        public JsonResult CreateFromAjax([FromBody] AjaxIndexPost post)
        {
            _context.AjaxIndexPosts.Add(post);
            _context.SaveChanges();
            return Json(post);
        }

        // ✅ AJAX 顯示：抓資料用
        [HttpGet]
        public JsonResult GetAjaxPosts()
        {
            var posts = _context.AjaxIndexPosts.OrderByDescending(p => p.CreatedAt).ToList();
            return Json(posts);
        }
    }
}
