// 匯入 MVC 與資料存取的相關功能
using Microsoft.AspNetCore.Mvc;            // 提供 Controller、Action 等 Web 功能
using StockGTO.Data;                       // 匯入你自己專案的資料庫上下文（AppDbContext）
using StockGTO.Models;                     // 匯入 Post 模型（資料結構）

namespace StockGTO.Controllers              // 命名空間，對應 Controllers 資料夾
{
    // 定義 PostController 類別，繼承自 MVC 的 Controller 基底類別
    public class PostController : Controller
    {
        // 宣告私有變數，代表資料庫物件
        private readonly AppDbContext _context;

        // 建構式：當這個 Controller 被建立時，把資料庫注入進來
        public PostController(AppDbContext context)
        {
            _context = context; // 將傳進來的資料庫存起來，供整個 Controller 使用
        }

        // ======【前台】顯示所有文章列表 ======

        // 對應網址：/Post/Post
        // 把所有文章抓出來，傳給 View 顯示清單
        public IActionResult Post()
        {
            var posts = _context.Posts
                .OrderByDescending(p => p.CreatedAt) // 根據時間排序（最新在前）
                .ToList();                            // 轉成清單
            return View(posts);                       // 傳給 View 顯示
        }

        // ======【前台】顯示單篇文章內容（傳統非 AJAX） ======

        // 對應網址：/Post/Details/3
        // 根據文章 ID 抓出資料，送到詳細頁面
        public IActionResult Details(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id); // 找出這篇文章
            if (post == null) return NotFound();                       // 沒找到 → 回傳 404
            return View(post);                                         // 有找到 → 顯示詳細內容
        }

        // ======【後台】顯示新增文章的空白表單 ======

        public IActionResult Create()
        {
            return View(); // 回傳空表單頁面
        }

        // ======【後台】接收新增文章表單的資料，存進資料庫 ======

        [HttpPost] // 這段是處理表單提交（POST 請求）
        public IActionResult Create(Post post)
        {
            if (ModelState.IsValid) // 檢查欄位有沒有符合資料驗證規則（像是 Title 不可空白）
            {
                _context.Posts.Add(post);    // 把新文章加入到資料庫暫存區
                _context.SaveChanges();      // 寫入資料庫（正式存檔）
                return RedirectToAction("post"); // 存完後跳回清單頁
            }
            return View(post); // 若驗證失敗，顯示原頁面，並保留輸入資料
        }

        // ======【前台 AJAX 專用】根據 ID 抓文章，傳 JSON 回去 ======

        [HttpGet]
        public IActionResult GetContent(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound(); // 找不到就回傳 404

            // 回傳 JSON 給前端（包含標題、內容、分類、建立時間）
            return Json(new
            {
                title = post.Title,
                content = post.Content,
                category = post.Category,
                createdAt = post.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            });
        }

        // ======【後台】管理畫面：顯示所有文章，附操作按鈕 ======

        public IActionResult Manage()
        {
            var posts = _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
            return View(posts); // 傳送給 Views/Post/Manage.cshtml
        }

        // ======【後台】接收前端 AJAX 編輯後的資料，更新資料庫 ======

        [HttpPost]
        public IActionResult Edit([FromBody] Post post) // 用 AJAX 傳 JSON → [FromBody]
        {
            var old = _context.Posts.FirstOrDefault(p => p.Id == post.Id);
            if (old == null) return NotFound();

            // 更新欄位
            old.Title = post.Title;
            old.Content = post.Content;
            old.Category = post.Category;

            _context.SaveChanges(); // 存檔
            return Json(new { success = true }); // 回傳成功訊息
        }

        // ======【後台】刪除指定文章（AJAX 呼叫） ======

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var post = _context.Posts.FirstOrDefault(p => p.Id == id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post); // 從資料庫移除
            _context.SaveChanges();      // 存檔
            return Ok();                 // 回傳 200 OK 給前端
        }




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




    }
}
