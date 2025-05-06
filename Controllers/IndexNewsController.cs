using Microsoft.AspNetCore.Mvc;
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

        // 🔹 前台顯示（例如首頁使用）
        public IActionResult Index()
        {
            var news = _context.IndexNews
                               .Where(n => n.IsActive)
                               .OrderBy(n => n.Position)
                               .ToList();
            return View(news);
        }

        // 🔧 後台：管理入口（清單＋新增）
        public IActionResult Manage()
        {
            var viewModel = new IndexNewsViewModel
            {
                NewsList = _context.IndexNews.OrderBy(n => n.Position).ToList()
            };
            return View(viewModel);
        }

        // ✅ 新增卡片（由 Manage 頁表單送出）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IndexNewsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.NewNews.CreatedAt = DateTime.Now;
                _context.IndexNews.Add(viewModel.NewNews);
                _context.SaveChanges();
                return RedirectToAction("Manage");
            }

            // 錯誤時重新顯示清單
            viewModel.NewsList = _context.IndexNews.OrderBy(n => n.Position).ToList();
            return View("Manage", viewModel);
        }

        // ✏️ 編輯畫面
        public IActionResult Edit(int id)
        {
            var news = _context.IndexNews.Find(id);
            if (news == null)
                return NotFound();
            return View(news);
        }

        // ✏️ 接收編輯表單送出
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(IndexNews model)
        {
            if (ModelState.IsValid)
            {
                _context.IndexNews.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Manage");
            }
            return View(model);
        }

        // 🗑️ 刪除
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
