using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace StockGTO.Controllers
{
    // ✅ 限登入使用者才能上傳圖片
    [Authorize]
    public class UploadController : Controller
    {
        // ✅ 上傳圖片（給 CKEditor 用）
        [HttpPost]
        public async Task<IActionResult> Image(IFormFile upload)
        {
            // 🧱 驗證是否有檔案
            if (upload == null || upload.Length == 0)
            {
                return Json(new
                {
                    uploaded = false,
                    error = new { message = "沒有上傳任何檔案。" }
                });
            }

            // ✅ 產生唯一檔名（避免同名覆蓋）
            var fileExt = Path.GetExtension(upload.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExt}";

            // ✅ 設定儲存資料夾與檔案路徑
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var savePath = Path.Combine(uploadFolder, fileName);

            // ✅ 如果資料夾不存在就自動建立
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // ✅ 儲存檔案到 wwwroot/uploads
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }

            // ✅ 回傳圖片 URL 給 CKEditor
            var fileUrl = Url.Content($"~/uploads/{fileName}");
            return Json(new
            {
                uploaded = true,
                url = fileUrl
            });
        }
    }
}
