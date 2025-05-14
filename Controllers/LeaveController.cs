// LeaveController.cs
using Microsoft.AspNetCore.Mvc;
using StockGTO.Data;
using StockGTO.Models;

namespace StockGTO.Controllers
{
    public class LeaveController : Controller
    {
        private readonly AppDbContext _context;

        public LeaveController(AppDbContext context)
        {
            _context = context;
        }

        // 顯示新增表單
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 處理表單送出
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LeaveRequest request)
        {
            // 防止未填 Approver 報錯
            if (string.IsNullOrEmpty(request.Approver))
                request.Approver = "";

            if (ModelState.IsValid)
            {
                _context.LeaveRequests.Add(request);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(request);
        }

        // 顯示所有紀錄
        public IActionResult Index()
        {
            var leaves = _context.LeaveRequests.ToList();
            return View(leaves);
        }
    }
}
