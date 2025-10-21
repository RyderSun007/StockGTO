using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace StockGTO.Controllers
{
    public class LabelItem
    {
        public string Code { get; set; } = "";
        public string Store { get; set; } = "";
        public string ImgUrl { get; set; } = "";
    }

    [Route("labels")]
    public class LabelsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public LabelsController(IWebHostEnvironment env) => _env = env;

        [HttpGet("ping")]
        public IActionResult Ping() => Content("labels ok");

        [HttpGet("upload")]
        public IActionResult Upload() => View();

        [HttpPost("upload")]
        [RequestSizeLimit(512L * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile csv, List<IFormFile> images)
        {
            if (csv == null || csv.Length == 0)
                return BadRequest("請選擇 CSV。");

            // 建立暫存資料夾
            var token = Guid.NewGuid().ToString("N");
            var root = _env.WebRootPath ?? "wwwroot";
            var workDir = Path.Combine(root, "labels", token);
            Directory.CreateDirectory(workDir);

            // 儲存圖片
            var saved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var img in images ?? new())
            {
                var ext = Path.GetExtension(img.FileName).ToLowerInvariant();
                if (ext is ".png" or ".jpg" or ".jpeg" or ".svg")
                {
                    var savePath = Path.Combine(workDir, Path.GetFileName(img.FileName));
                    using var fs = System.IO.File.Create(savePath);
                    await img.CopyToAsync(fs);
                    saved.Add(Path.GetFileName(img.FileName));
                }
            }

            // 解析 CSV
            var items = new List<LabelItem>();
            using var ms = new MemoryStream();
            await csv.CopyToAsync(ms);
            ms.Position = 0;
            using var sr = new StreamReader(ms, Encoding.UTF8, true);
            string? line;
            bool header = true;
            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (header) { header = false; continue; }
                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                var code = parts[0].Trim();
                var store = parts[1].Trim();
                var imgFile = parts[2].Trim();
                var imgUrl = saved.Contains(imgFile) ? $"/labels/{token}/{imgFile}" : "";
                items.Add(new LabelItem { Code = code, Store = store, ImgUrl = imgUrl });
            }

            // 寫入 data.json
            var jsonPath = Path.Combine(workDir, "data.json");
            await System.IO.File.WriteAllTextAsync(jsonPath,
                JsonSerializer.Serialize(items), Encoding.UTF8);

            // 導向列印頁
            return RedirectToAction("Print", new { token });
        }

        [HttpGet("print")]
        public IActionResult Print(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("缺少 token。");

            ViewBag.Token = token;
            return View();
        }
    }
}
