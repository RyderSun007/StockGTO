using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace StockGTO.Controllers
{
    [Authorize] // ✅ 加在這裡：整個 MemberController 都需要登入
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


        // /Member/MyPosts
        [Authorize]
        public async Task<IActionResult> MyPosts()
        {
            var user = await _userManager.GetUserAsync(User);
            var posts = await _context.ArticlePosts
                .Include(a => a.Category)
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // /Member/Favorites
        public IActionResult Favorites()
        {
            var demoList = new List<ArticlePost>
    {
        new ArticlePost
        {
            Id = 1,
            Title = "台股技術面全解析",
            CreatedAt = DateTime.Now.AddDays(-2),
            Category = new Category { Name = "台股" }
        },
        new ArticlePost
        {
            Id = 2,
            Title = "美股財報週預測",
            CreatedAt = DateTime.Now.AddDays(-5),
            Category = new Category { Name = "美股" }
        }
    };

            return View(demoList);
        }

        // /Member/Settings
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [Authorize]
        public async Task<IActionResult> MemberCenter()
        {
            var user = await _userManager.GetUserAsync(User);
            var posts = await _context.ArticlePosts
                .Include(a => a.Category)
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View("MemberCenter", (posts, user));
        }
    }
}
