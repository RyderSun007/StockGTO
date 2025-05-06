// ✅ OldSunStockController.cs（完整含 Create/Edit 自動計算）

using Microsoft.AspNetCore.Mvc;
using StockGTO.Data;
using StockGTO.Models;
using System.Linq;

namespace StockGTO.Controllers
{
    public class OldSunStockController : Controller
    {
        private readonly AppDbContext _context;

        public OldSunStockController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var stocks = _context.OldSunStocks.ToList();
            return View(stocks);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(OldSunStockModel model)
        {
            if (ModelState.IsValid)
            {
                model.TotalCost = model.Quantity * (model.BuyPrice ?? 0);
                model.CurrentValue = model.Quantity * (model.CurrentPrice ?? 0);

                _context.OldSunStocks.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var stock = _context.OldSunStocks.Find(id);
            if (stock == null)
                return NotFound();
            return View(stock);
        }

        [HttpPost]
        public IActionResult Edit(int id, OldSunStockModel model)
        {
            if (ModelState.IsValid)
            {
                model.TotalCost = model.Quantity * (model.BuyPrice ?? 0);
                model.CurrentValue = model.Quantity * (model.CurrentPrice ?? 0);

                _context.OldSunStocks.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}
