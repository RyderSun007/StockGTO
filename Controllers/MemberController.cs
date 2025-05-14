using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Models;
using System.Security.Claims;

namespace StockGTO.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Profile()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var articles = await _context.ArticlePosts
                .Include(a => a.Category)
                .Where(a => a.Author == userEmail)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(articles);
        }
    }
}
