using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockGTO.Data;
using StockGTO.Models;

namespace StockGTO.Controllers
{
    [Authorize] // ✅ 只有登入會員可以請假與查看紀錄
    public class LeaveController : Controller
    {
        private readonly AppDbContext _context;

        public LeaveController(AppDbContext context)
        {
            _context = context;
        }

        // 📄 顯示請假申請表單（GET）
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 📝 接收請假表單送出（POST）
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LeaveRequest request)
        {
            // 🔐 確保 Approver 不為 null（避免錯誤）
            if (string.IsNullOrEmpty(request.Approver))
                request.Approver = "";

            if (ModelState.IsValid)
            {
                _context.LeaveRequests.Add(request);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "✅ 請假單已提交！";
                return RedirectToAction("Index");
            }

            return View(request);
        }

        // 📋 顯示所有請假紀錄（GET）
        public IActionResult Index()
        {
            var leaves = _context.LeaveRequests
                .OrderByDescending(l => l.StartDate)
                .ToList();

            return View(leaves);
        }
    }
}
