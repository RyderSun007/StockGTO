using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;      // 引用 AppDbContext
using StockGTO.Models;

namespace StockGTO.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context; // 🧠 新增：資料庫物件

        // 🧱 建構式同時注入 logger + 資料庫
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /Home/Index
        public IActionResult Index()
        {
            var count = _context.Employees.Count();
            Console.WriteLine("🔥 員工資料筆數：" + count); // 在輸出視窗會看到訊息
            return View(_context.Employees.ToList());   // 會傳資料到 Index.cshtml
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
