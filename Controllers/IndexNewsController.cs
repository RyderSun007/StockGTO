using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockGTO.Data;
using StockGTO.Models;
using StockGTO.ViewModels;
using System.Linq;

namespace StockGTO.Controllers
{
    public class IndexNewsController : Controller
    {
        private readonly AppDbContext _context;

        public IndexNewsController(AppDbContext context)
        {
            _context = context;
        }

        // 🔓 前台顯示九宮格廣告（公開訪客可看）
        // 👉 用於網站首頁，只顯示「啟用狀態」的廣告內容
        [AllowAnonymous]
        public IActionResult Index()
        {
            var news = _context.IndexNews
                               .Where(n => n.IsActive) // 只撈啟用的
                               .OrderBy(n => n.Position)
                               .ToList();
            return View(news);
        }

        // 🔐 後台廣告清單頁（管理介面）
        // 👉 顯示所有廣告並可新增，僅限登入使用者進入
        [Authorize]
        public IActionResult Manage()
        {
            var viewModel = new IndexNewsViewModel
            {
                NewsList = _context.IndexNews
                                   .OrderBy(n => n.Position)
                                   .ToList()
            };
            return View(viewModel);
        }

        // ➕ 新增一則廣告卡片（由表單送出）
        // 👉 新卡片會寫入資料庫，設定當下時間為建立時間
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Create(IndexNewsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.NewNews.CreatedAt = DateTime.Now;
                _context.IndexNews.Add(viewModel.NewNews);
                _context.SaveChanges();
                return RedirectToAction("Manage");
            }

            // ❗表單錯誤，重新帶回清單資料避免 Null
            viewModel.NewsList = _context.IndexNews.OrderBy(n => n.Position).ToList();
            return View("Manage", viewModel);
        }

        // ✏️ 編輯廣告內容（進入畫面）
        // 👉 用於 GET，顯示單筆內容到編輯頁面
        [Authorize]
        public IActionResult Edit(int id)
        {
            var news = _context.IndexNews.Find(id);
            if (news == null)
                return NotFound();

            return View(news);
        }

        // 💾 編輯送出更新（儲存修改）
        // 👉 用於 POST，更新後導回管理畫面
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(IndexNews model)
        {
            if (ModelState.IsValid)
            {
                _context.IndexNews.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Manage");
            }

            // 表單驗證失敗時，保留原資料重畫畫面
            return View(model);
        }

        // ❌ 刪除一筆廣告
        // 👉 傳入 ID，刪除資料並導回後台
        [Authorize]
        public IActionResult Delete(int id)
        {
            var news = _context.IndexNews.Find(id);
            if (news != null)
            {
                _context.IndexNews.Remove(news);
                _context.SaveChanges();
            }
            return RedirectToAction("Manage");
        }
    }
}
