using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Services;
using OfficeOpenXml;

using System.Text;


namespace StockGTO.Controllers
{
    public class O_HR_ControlController : Controller
    {
        private readonly O_HR_ControlService _svc;
        private readonly AppDbContext _db;

        public O_HR_ControlController(O_HR_ControlService svc, AppDbContext db)
        {
            _svc = svc;
            _db = db;
        }

        // === Index：可帶 batchId 顯示剛匯入資料 + 當月國假 ===
        [HttpGet("/O_HR_Control")]
        public async Task<IActionResult> Index(
            long? batchId,
            int take = 200,
            int? year = null,
            int? month = null,
            int page = 1,
            int pageSize = 1000,
            int takeRaw = 2000
        )
        {
            ViewBag.BatchId = batchId;
            ViewBag.Take = take;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TakeRaw = takeRaw;

            var rows = new List<StockGTO.Models.O_HR_Control_RawTimePunch>();
            if (batchId.HasValue)
            {
                var query = _db.O_HR_Control_RawTimePunches
                               .AsNoTracking()
                               .Where(x => x.BatchId == batchId.Value)
                               .OrderBy(x => x.SourceRowNo); // 和 Excel 列序一致

                ViewBag.Total = await query.CountAsync();
                rows = await query.ToListAsync();
            }

            int y = year ?? DateTime.Today.Year;
            int m = month ?? DateTime.Today.Month;
            ViewBag.Year = y;
            ViewBag.Month = m;

            var first = new DateOnly(y, m, 1);
            var last = first.AddMonths(1).AddDays(-1);

            var monthHolidays = await _db.O_HR_Control_Holidays
                .Where(h => h.Date >= first && h.Date <= last)
                .OrderBy(h => h.Date)
                .ToListAsync();

            ViewBag.Holidays = monthHolidays;
            ViewBag.HolidayMemoMap = monthHolidays.ToDictionary(
                h => h.Date,
                h => string.IsNullOrWhiteSpace(h.Memo) ? (h.Name ?? "") : h.Memo!
            );

            return View(rows);
        }

        // === 匯入 Excel：完成後導回 Index 並帶 batchId ===
        [HttpPost("/O_HR_Control/Import")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("請選擇檔案");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            var batchId = await _svc.ImportExcelAsync(ms, file.FileName, User?.Identity?.Name ?? "HR");
            TempData["msg"] = $"匯入成功，批次 {batchId}";
            return RedirectToAction(nameof(Index), new { batchId });
        }

        /// <summary>匯入/覆寫某年某月的假日</summary>
        [HttpPost("/O_HR_Control/UpsertHolidays")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpsertHolidays(int year, int month, string dates)
        {
            if (string.IsNullOrWhiteSpace(dates))
            {
                TempData["msg"] = "未輸入任何日期。";
                return RedirectToAction(nameof(Index), new { year, month });
            }

            var items = new List<StockGTO.Models.O_HR_Control_Holiday>();
            foreach (var raw in dates.Replace("\r", "")
                                     .Split(new[] { '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (!DateOnly.TryParse(parts[0], out var d)) continue;
                string memo = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "";

                items.Add(new StockGTO.Models.O_HR_Control_Holiday
                {
                    Date = d,
                    Name = "",
                    Memo = memo
                });
            }

            var first = new DateOnly(year, month, 1);
            var last = first.AddMonths(1).AddDays(-1);

            var olds = await _db.O_HR_Control_Holidays
                .Where(h => h.Date >= first && h.Date <= last)
                .ToListAsync();

            _db.O_HR_Control_Holidays.RemoveRange(olds);
            await _db.SaveChangesAsync();

            foreach (var g in items.GroupBy(x => x.Date))
                _db.O_HR_Control_Holidays.Add(g.First());

            await _db.SaveChangesAsync();

            TempData["msg"] = $"已更新 {year}-{month:D2} 的假日，共 {items.Select(i => i.Date).Distinct().Count()} 天。";
            return RedirectToAction(nameof(Index), new { year, month });
        }

        // === 建立 Session ===
        [HttpPost("/O_HR_Control/BuildSessions")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuildSessions(long batchId, DateOnly startDate, DateOnly endDate)
        {
            await _svc.BuildSessionsAsync(batchId, startDate, endDate);
            TempData["msg"] = $"Session 產生完成：批次 {batchId}";
            return RedirectToAction(nameof(Index), new { batchId });
        }

        // === 計算日結 ===
        [HttpPost("/O_HR_Control/Compute")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compute(DateOnly startDate, DateOnly endDate)
        {
            await _svc.ComputeDailyAsync(startDate, endDate);
            TempData["msg"] = $"已計算 {startDate} ~ {endDate}";
            return RedirectToAction(nameof(Index));
        }

        // === 快速查詢 JSON（驗證用）===
        [HttpGet("/O_HR_Control/Query")]
        public async Task<IActionResult> Query(DateOnly startDate, DateOnly endDate)
        {
            var q = await _db.O_HR_Control_DailyAttendances
                .Where(x => x.WorkDate >= startDate && x.WorkDate <= endDate)
                .Join(_db.O_HR_Control_Employees, a => a.EmpId, e => e.Id, (a, e) => new { a, e })
                .Join(_db.O_HR_Control_Departments, ae => ae.e.DeptId, d => d.Id, (ae, d) => new
                {
                    Dept = d.DeptName,
                    EmpNo = ae.e.EmpNo,
                    Name = ae.e.EmpName,
                    ae.a.WorkDate,
                    HoursNet = ae.a.MinutesWorkNet / 60.0,
                    BreakH = ae.a.MinutesBreak / 60.0,
                    OT8to10 = ae.a.OT_Minutes_8to10 / 60.0,
                    OT10to12 = ae.a.OT_Minutes_10to12 / 60.0,
                    OTRest = ae.a.OT_Rest_Minutes / 60.0,
                    OTHoli = ae.a.OT_Holiday_Minutes / 60.0,
                    ae.a.IsHoliday,
                    ae.a.IsRestDayBy7th,
                    ae.a.HasMissingPunch
                })
                .OrderBy(x => x.EmpNo).ThenBy(x => x.WorkDate)
                .ToListAsync();

            return Json(q);
        }


        // 直接用「已計算好的日結表」輸出 CSV（零再計算）
        [HttpGet("/O_HR_Control/ExportDailyCsv")]
        public async Task<IActionResult> ExportDailyCsv(int year, int month)
        {
            // 這裡用當月第一天到最後一天（含結尾）
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // 從日結表撈資料（你之前 Query 用的欄位我都沿用）
            var data = await _db.O_HR_Control_DailyAttendances
                .Where(x => x.WorkDate >= startDate && x.WorkDate <= endDate)
                .Join(_db.O_HR_Control_Employees, a => a.EmpId, e => e.Id, (a, e) => new { a, e })
                .Join(_db.O_HR_Control_Departments, ae => ae.e.DeptId, d => d.Id, (ae, d) => new
                {
                    Dept = d.DeptName,
                    EmpNo = ae.e.EmpNo,
                    Name = ae.e.EmpName,
                    Date = ae.a.WorkDate,

                    // 這三個對應：系統時數(未扣餐) / 扣餐 / 扣餐後
                    SysH = (ae.a.MinutesWorkNet + ae.a.MinutesBreak) / 60.0,
                    BreakH = ae.a.MinutesBreak / 60.0,
                    PayH = ae.a.MinutesWorkNet / 60.0,

                    // 平日加班分段與國假
                    OT8to10 = ae.a.OT_Minutes_8to10 / 60.0,
                    OT10to12 = ae.a.OT_Minutes_10to12 / 60.0,
                    OTHoliday = ae.a.OT_Holiday_Minutes / 60.0,

                    // 額外資訊
                    Status = ae.a.HasMissingPunch ? "ERROR" : "OK",
                    Memo = ae.a.IsHoliday ? "國假" : ""
                })
                .OrderBy(x => x.EmpNo).ThenBy(x => x.Date)
                .ToListAsync();

            if (data.Count == 0)
            {
                TempData["msg"] = $"本月（{year}-{month:D2}）沒有日結資料可匯出，請先執行計算。";
                return RedirectToAction(nameof(Index), new { year, month });
            }


            // === 用「Tab 分隔 + BOM」產生檔案，Excel 不會亂碼 ===
            var sb = new StringBuilder();
            sb.Append('\uFEFF'); // 在檔案最前面加 BOM（關鍵）

            // 標題列（用 Tab 分隔）
            sb.AppendLine("工號\t姓名\t部門\t日期\t系統時數\t扣餐\t扣餐後\t>8且≤10\t>10且≤12\t國假上班時數\t狀態\t備註");

            // 明細列（用 Tab 分隔）
            foreach (var r in data)
            {
                sb.AppendLine(string.Join("\t", new[]
                {
        r.EmpNo,
        r.Name,
        r.Dept,
        r.Date.ToString("yyyy-MM-dd"),
        r.SysH.ToString("0.##"),
        r.BreakH.ToString("0.##"),
        r.PayH.ToString("0.##"),
        r.OT8to10.ToString("0.##"),
        r.OT10to12.ToString("0.##"),
        r.OTHoliday.ToString("0.##"),
        r.Status,
        r.Memo
    }));
            }

            // 回傳檔案（用 Excel 的 MIME，讓瀏覽器直接用 Excel 開啟）
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Daily_{year}-{month:00}.csv"; // 副檔名仍用 .csv，Excel 會正常開啟
            return File(bytes, "application/vnd.ms-excel", fileName);








        }

        
        // ======= 共用計算小工具（類別層級；避免方法內宣告欄位的錯誤） =======
        private static readonly TimeSpan FirstShift = TimeSpan.FromHours(7);      // 07:00
        private static readonly TimeSpan LastShift = TimeSpan.FromHours(21);     // 21:00
        private const int EarlyWindowMin = 30;                                    // 提前 30
        private const int GraceLateMin = 5;                                     // 寬限 5
        private static readonly TimeSpan TooEarlyCut = TimeSpan.FromMinutes(450); // 07:30

        private static DateTime? RoundInToShiftWindow(DateTime? dt)
        {
            if (dt == null) return null;
            var v = dt.Value;
            if (v.TimeOfDay < TooEarlyCut) return null;
            for (var t = v.Date + FirstShift; t <= v.Date + LastShift; t = t.AddMinutes(30))
            {
                var winStart = t.AddMinutes(-EarlyWindowMin);
                var winEnd = t.AddMinutes(GraceLateMin);
                if (v >= winStart && v <= winEnd) return t;
            }
            return null;
        }

        private static DateTime? RoundOutToHalfHourFloor(DateTime? dt)
        {
            if (dt == null) return null;
            var v = dt.Value;
            var baseHour = new DateTime(v.Year, v.Month, v.Day, v.Hour, 0, 0);
            return (v.Minute < 30) ? baseHour : baseHour.AddMinutes(30);
        }

        private static double CalcBreakHours(double workH)
        {
            if (workH <= 4.0) return 0.0;
            if (workH < 8.0) return 0.5;
            return 1.0;
        }

        private static void SplitOvertime(double payH, out double h8to10, out double h10to12)
        {
            h8to10 = 0; h10to12 = 0;
            if (payH <= 8) return;
            var extra = payH - 8;
            h8to10 = Math.Min(extra, 2.0);
            h10to12 = Math.Max(0.0, Math.Min(extra - 2.0, 2.0));
        }
    }
}
