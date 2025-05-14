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

      

    }
}
