using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StockGTO.Controllers
{
    public class MemberController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 🧠 顯示目前使用者的所有文章（未登入也可瀏覽）
        public async Task<IActionResult> Profile(string keyword, int? categoryId)
        {
            var user = await _userManager.GetUserAsync(User);

            IQueryable<ArticlePost> query = _context.ArticlePosts;

            if (user != null)
            {
                query = query.Where(a => a.UserId == user.Id);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.Trim().ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(lowerKeyword));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId);
            }

            var articles = await query
                .Include(a => a.Category) // 👉 放在最後，才不會型別錯誤
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;

            return View(articles);
        }

    }
}
