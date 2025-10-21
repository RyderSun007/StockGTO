using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StockGTO.Data;
using StockGTO.Models;

namespace StockGTO.Services;

public class O_HR_ControlService
{
    private readonly AppDbContext _db;
    public O_HR_ControlService(AppDbContext db) { _db = db; }

    // 解析 HH:mm / H:mm / 8:3x 之類文字時間（保留一份就好）
    private static bool TryParseTime(string s, out TimeSpan t)
    {
        s = (s ?? "").Trim();
        if (TimeSpan.TryParse(s, out t)) return true;
        // 容錯：08：13（全形冒號）
        s = s.Replace('：', ':');
        return TimeSpan.TryParse(s, out t);
    }

    // ① 上傳 Excel -> 寫入 ImportBatch + RawTimePunch
    public async Task<long> ImportExcelAsync(Stream fileStream, string fileName, string user)
    {
        // EPPlus 授權（你目前用 v6.x，這行可用）
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var pkg = new ExcelPackage(fileStream);
        if (pkg.Workbook.Worksheets.Count == 0)
            throw new InvalidOperationException("Excel 內沒有工作表。");

        var ws = pkg.Workbook.Worksheets[0];

        var batch = new O_HR_Control_ImportBatch
        {
            FileName = fileName,
            UploadedBy = user
        };
        _db.Add(batch);
        await _db.SaveChangesAsync();

        int rows = ws.Dimension.End.Row;
        int ok = 0;

        // 期待欄位：1=工號 2=姓名 3=部門 4=刷卡日期 5=刷卡時間（第1列為標題）
        for (int r = 2; r <= rows; r++)
        {
            string empNo = ws.Cells[r, 1].GetValue<string>()?.Trim() ?? "";
            if (string.IsNullOrEmpty(empNo)) continue;

            string empName = ws.Cells[r, 2].GetValue<string>()?.Trim() ?? "";
            string dept = ws.Cells[r, 3].GetValue<string>()?.Trim() ?? "";

            DateTime? d = ws.Cells[r, 4].GetValue<DateTime?>();
            if (d is null)
            {
                var sd = ws.Cells[r, 4].GetValue<string>();
                if (DateTime.TryParse(sd, out var dt)) d = dt.Date;
            }

            string timeStr = ws.Cells[r, 5].GetValue<string>()?.Trim() ?? "";

            var rec = new O_HR_Control_RawTimePunch
            {
                BatchId = batch.Id,
                EmpNo = empNo,
                EmpName = empName,
                DeptName = dept,
                PunchTimeStr = timeStr,
                SourceRowNo = r
            };

            if (d is null || !TryParseTime(timeStr, out var t))
            {
                rec.PunchDate = new DateOnly(1900, 1, 1);
                rec.PunchDateTime = DateTime.MinValue;
                rec.ParseStatus = "ERROR";
                rec.ErrorMsg = "日期或時間格式錯誤";
            }
            else
            {
                rec.PunchDate = DateOnly.FromDateTime(d.Value.Date);
                rec.PunchDateTime = d.Value.Date + t;
                rec.ParseStatus = "OK";
                ok++;
            }

            _db.Add(rec);
        }

        // 回寫成功筆數
        batch.RowCount = ok;
        await _db.SaveChangesAsync();
        return batch.Id;
    }

    // ②（先保留，之後我們再改成純 EmpNo 流程）
    public async Task BuildSessionsAsync(long batchId, DateOnly start, DateOnly end)
    {
        var raw = await _db.O_HR_Control_RawTimePunches
            .Where(x => x.BatchId == batchId
                     && x.ParseStatus == "OK"
                     && x.PunchDate >= start && x.PunchDate <= end)
            .OrderBy(x => x.EmpNo).ThenBy(x => x.PunchDateTime)
            .ToListAsync();

        // EmpNo → EmpId map（之後我們會拿掉 EmpId 依賴）
        var empMap = await _db.O_HR_Control_Employees.ToDictionaryAsync(e => e.EmpNo, e => e.Id);

        string currentEmp = "";
        var buf = new List<O_HR_Control_RawTimePunch>();

        foreach (var p in raw.Append(new O_HR_Control_RawTimePunch { EmpNo = "_END_" }))
        {
            if (p.EmpNo != currentEmp)
            {
                if (buf.Count > 0)
                {
                    var empId = empMap[buf[0].EmpNo];
                    for (int i = 0; i < buf.Count; i += 2)
                    {
                        var startDt = buf[i].PunchDateTime;
                        DateTime? endDt = (i + 1 < buf.Count) ? buf[i + 1].PunchDateTime : null;

                        var s = new O_HR_Control_WorkSession
                        {
                            EmpId = empId,
                            SourceBatchId = batchId,
                            StartDT = startDt,
                            EndDT = endDt,
                            MinutesTotal = endDt.HasValue ? (int)(endDt.Value - startDt).TotalMinutes : 0,
                            IsMissingIn = false,
                            IsMissingOut = endDt == null
                        };
                        _db.Add(s);
                    }
                }
                currentEmp = p.EmpNo;
                buf.Clear();
            }
            buf.Add(p);
        }
        await _db.SaveChangesAsync();
    }

    // ③（先保留，之後再一起把 EmpId 換成 EmpNo + BatchId）
    public async Task ComputeDailyAsync(DateOnly start, DateOnly end)
    {
        var holidays = await _db.O_HR_Control_Holidays
            .Where(h => h.Date >= start.AddDays(-10) && h.Date <= end.AddDays(10))
            .ToDictionaryAsync(h => h.Date, h => true);

        var sessions = await _db.O_HR_Control_WorkSessions
            .Where(s => DateOnly.FromDateTime(s.StartDT) >= start.AddDays(-7)
                     && DateOnly.FromDateTime(s.StartDT) <= end)
            .AsNoTracking()
            .ToListAsync();

        var dayGroups = sessions
            .GroupBy(s => new { s.EmpId, D = DateOnly.FromDateTime(s.StartDT) })
            .OrderBy(g => g.Key.EmpId).ThenBy(g => g.Key.D);

        var existing = await _db.O_HR_Control_DailyAttendances
            .Where(a => a.WorkDate >= start && a.WorkDate <= end)
            .ToDictionaryAsync(a => new { a.EmpId, a.WorkDate });

        var byEmp = dayGroups.GroupBy(g => g.Key.EmpId);

        foreach (var empDays in byEmp)
        {
            int consecutive = 0;
            foreach (var g in empDays.OrderBy(x => x.Key.D))
            {
                var empId = g.Key.EmpId;
                var date = g.Key.D;

                int total = g.Sum(x => x.MinutesTotal);
                bool hasMissing = g.Any(x => x.IsMissingOut || x.MinutesTotal <= 0);
                bool isHoliday = holidays.ContainsKey(date);

                consecutive = total > 0 ? consecutive + 1 : 0;
                bool isRest7th = (!isHoliday && total > 0 && consecutive == 7);

                int breakMin =
                    total >= 480 ? 60 :
                    (total > 240 && total < 480) ? 30 : 0;

                int net = Math.Max(0, total - breakMin);

                int ot8to10 = 0, ot10to12 = 0, otRest = 0, otHoliday = 0;

                if (isHoliday) otHoliday = net;
                else if (isRest7th) otRest = net;
                else
                {
                    int baseOt = Math.Max(0, net - 480);
                    ot8to10 = Math.Min(baseOt, 120);
                    ot10to12 = Math.Max(0, baseOt - 120);
                }

                var key = new { EmpId = empId, WorkDate = date };
                if (!existing.TryGetValue(key, out var rec))
                {
                    rec = new O_HR_Control_DailyAttendance { EmpId = empId, WorkDate = date };
                    _db.O_HR_Control_DailyAttendances.Add(rec);
                    existing[key] = rec;
                }

                rec.MinutesBreak = breakMin;
                rec.MinutesWorkNet = net;
                rec.OT_Minutes_8to10 = ot8to10;
                rec.OT_Minutes_10to12 = ot10to12;
                rec.OT_Rest_Minutes = otRest;
                rec.OT_Holiday_Minutes = otHoliday;
                rec.ConsecutiveDays = consecutive;
                rec.IsRestDayBy7th = isRest7th;
                rec.IsHoliday = isHoliday;
                rec.HasMissingPunch = hasMissing;
                rec.CalculatedAt = DateTime.UtcNow;

                if (isRest7th) consecutive = 0;
            }
        }
        await _db.SaveChangesAsync();



    }
    // 給畫面與彙總都能共用的：以小時為單位
    public static double CalcWorkHours(DateTime? inDt, DateTime? outDt)
    {
        if (inDt == null || outDt == null) return 0;
        var minutes = (outDt.Value - inDt.Value).TotalMinutes;
        return minutes > 0 ? Math.Round(minutes / 60.0, 2) : 0;
    }

    // 你的餐扣規則（小時）
    public static double CalcBreakHours(double workHours)
    {
        if (workHours <= 4) return 0;
        if (workHours < 8) return 0.5;
        if (workHours <= 10) return 1;
        if (workHours <= 12) return 2;
        return 2; // 先封頂 2 小時（>12h 之後要改再調）
    }

}
