using Microsoft.AspNetCore.Mvc;
using StockGTO.Services;
using System.Threading.Tasks;

namespace StockGTO.Controllers
{
    public class StockController : Controller
    {
        private readonly AlphaVantageService _stockService = new();

        public async Task<IActionResult> GetPrice(string symbol = "AAPL")
        {
            var price = await _stockService.GetStockPrice(symbol);
            return Content($"股票：{symbol} 價格：{price}");
        }
    }
}
