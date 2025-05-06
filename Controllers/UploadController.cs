using Microsoft.AspNetCore.Mvc;

namespace StockGTO.Controllers
{
    public class UploadController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Image(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
            {
                return Json(new { uploaded = false, error = new { message = "沒有上傳任何檔案" } });
            }

            // 👉 取得檔案名稱
            var fileName = Path.GetFileName(upload.FileName);

            // 👉 組成儲存路徑 (把檔案存到 wwwroot/uploads/)
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            // 👉 確保 uploads 資料夾存在
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads"));
            }

            // 👉 寫入檔案
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }

            // 👉 組成圖片的公開網址
            var fileUrl = Url.Content($"~/uploads/{fileName}");

            // 👉 回傳給 CKEditor 的格式
            return Json(new { uploaded = true, url = fileUrl });
        }
    }
}
