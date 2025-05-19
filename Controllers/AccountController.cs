using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockGTO.Models;
using System.Security.Claims;

namespace StockGTO.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 啟動 Google 登入流程
        //[HttpGet("LoginWithGoogle")]
        //public IActionResult LoginWithGoogle()
        //{
        //    var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
        //    var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        //    return Challenge(properties, "Google");
        //}

        // 啟動 Facebook 登入流程
        [HttpGet("LoginWithFacebook")]
        public IActionResult LoginWithFacebook()
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);
            return Challenge(properties, "Facebook");
        }

        // 通用登入完成後的回呼處理
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "登入失敗，請再試一次。";
                return RedirectToAction("Index", "Home");
            }

            // 已經登入過就直接登入
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false);

            if (signInResult.Succeeded)
            {
                TempData["Message"] = $"歡迎回來，{info.Principal.FindFirstValue(ClaimTypes.Name)}！";
                return RedirectToAction("Profile", "Member");
            }

            // 第一次登入，建立帳號
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            // ✅ 圖片抓法多平台 fallback
            var img = info.Principal.FindFirstValue("picture")
                   ?? info.Principal.FindFirstValue("urn:google:picture")
                   ?? info.Principal.FindFirstValue("urn:facebook:picture")
                   ?? "";

            // 🔐 Email 防呆機制
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "無法從第三方登入取得 Email，請改用其他登入方式";
                return RedirectToAction("Index", "Home");
            }

            // 建立新使用者
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = name,
                ProfileImage = img,
                RegisterSource = info.LoginProvider,
                RegisterTime = DateTime.Now,
                IsVIP = false,
                UserNote = ""
            };

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Message"] = $"歡迎新朋友 {user.DisplayName} 加入！";
            }
            else
            {
                if (createResult.Errors.Any(e => e.Code == "DuplicateUserName"))
                {
                    TempData["Error"] = "這個 Email 已經註冊過，請改用登入方式。";
                }
                else
                {
                    TempData["Error"] = "註冊失敗，請聯絡系統管理員。";
                }
            }

            return RedirectToAction("Index", "Home");
        }


        // 登出
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
