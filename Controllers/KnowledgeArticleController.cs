using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace StockGTO.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // 簡單帳密比對
            if (username == "admin" && password == "123456")
            {
                HttpContext.Session.SetString("IsLoggedIn", "true");
                return RedirectToAction("ShowEmployee", "Employee");
            }

            ViewBag.Error = "帳號或密碼錯誤！";
            return View();
        }

        // 登出
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
