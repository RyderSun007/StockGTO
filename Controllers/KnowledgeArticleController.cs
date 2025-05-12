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
        // ✅ 後台文章總表：支援分類與標題關鍵字搜尋
        public IActionResult Index(string keyword, string category)
        {
            // 1️⃣ 從資料表建立查詢（初始全部文章）
            var query = _context.ArticlePosts.AsQueryable();

            // 2️⃣ 如果有輸入關鍵字，就過濾標題含該字的文章
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(a => a.Title.Contains(keyword));

            // 3️⃣ 如果有選擇分類，就過濾該分類文章
            if (!string.IsNullOrEmpty(category))
                query = query.Where(a => a.Category == category);

            // 4️⃣ 排除空白內容，並依建立日期倒序排列
            var articles = query
                .Where(a => a.Content != null)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            // 5️⃣ 把搜尋條件傳回前端，讓欄位與選單記住狀態
            ViewBag.Keyword = keyword;
            ViewBag.Category = category;

            // 6️⃣ 把結果傳給 Razor View 顯示
            return View(articles);
        }


        // ✅ 顯示單篇文章
        // ✅ 顯示單篇文章（支援上一篇／下一篇）
        public IActionResult Details(int id)
        {
            // 1️⃣ 找出指定 ID 的文章
            var article = _context.ArticlePosts.FirstOrDefault(a => a.Id == id);
            if (article == null)
                return NotFound();

            // 2️⃣ 更新瀏覽次數
            article.ViewCount++;
            _context.SaveChanges(); // 記得儲存更新

            // 3️⃣ 找出同分類下的上一篇（ID 較小）
            ViewBag.PreviousArticle = _context.ArticlePosts
                .Where(a => a.Category == article.Category && a.Id < article.Id)
                .OrderByDescending(a => a.Id)
                .FirstOrDefault();

            // 4️⃣ 找出同分類下的下一篇（ID 較大）
            ViewBag.NextArticle = _context.ArticlePosts
                .Where(a => a.Category == article.Category && a.Id > article.Id)
                .OrderBy(a => a.Id)
                .FirstOrDefault();

            // 5️⃣ 可選：找同分類推薦文章（排除自己）
            ViewBag.RelatedArticles = _context.ArticlePosts
                .Where(a => a.Category == article.Category && a.Id != article.Id)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList();

            // 6️⃣ 傳給前端
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
                // 1️⃣ 儲存圖片（如果有上傳）
                if (post.ImageFile != null && post.ImageFile.Length > 0)
                {
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "aznews", "uploads");
                    Directory.CreateDirectory(uploadDir); // 確保資料夾存在

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(post.ImageFile.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        post.ImageFile.CopyTo(stream);
                    }

                    post.ImageUrl = "/aznews/uploads/" + fileName;
                }

                // 2️⃣ 沒填圖片網址，也沒上傳，就給預設圖
                if (string.IsNullOrEmpty(post.ImageUrl))
                {
                    post.ImageUrl = "/images/default.jpg";
                }

                post.CreatedAt = DateTime.Now;
                post.Category = "股票知識";
                _context.ArticlePosts.Add(post);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }


        // ✅ 顯示編輯畫面（GET）
        // 🔧 編輯表單（POST）
        // ✅ 顯示編輯畫面（GET）
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var article = _context.ArticlePosts.FirstOrDefault(a => a.Id == id && a.Category == "股票知識");
            if (article == null)
                return NotFound();

            return View(article);
        }

        // 🔧 儲存編輯內容（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ArticlePost post)
        {
            if (id != post.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = _context.ArticlePosts.FirstOrDefault(a => a.Id == id);
                if (existing == null) return NotFound();

                // ✅ 有上傳新圖片的話
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
                    // ✅ 即使沒選圖片，也更新圖片網址（使用者可能手動輸入或修改過）
                    existing.ImageUrl = post.ImageUrl;
                }

                // ✅ 更新其餘欄位
                existing.Title = post.Title;
                existing.Content = post.Content;
                existing.Author = post.Author;
                existing.IsPinned = post.IsPinned;
                existing.Category = post.Category;

                _context.SaveChanges();
                return RedirectToAction("Details", new { id = post.Id });
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
        public IActionResult PublicList(string keyword, string category)
        {
            var query = _context.ArticlePosts.AsQueryable();




            // 🔍 如果有輸入關鍵字，就套用模糊搜尋（LIKE）
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(a => a.Title.Contains(keyword));

            // 📂 如果有選分類，就依分類篩選，否則顯示全部
            if (!string.IsNullOrEmpty(category))
                query = query.Where(a => a.Category == category);

            // ✅ 不再預設強制顯示「股票知識」，改為顯示所有資料
            var articles = query
                .OrderByDescending(a => a.CreatedAt)
                .ToList();



            ViewBag.Keyword = keyword;
            ViewBag.Category = category;

            return View(articles);
        }





    }
}
