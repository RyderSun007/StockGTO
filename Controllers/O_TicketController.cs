using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Models;
using System.Data;
using System.Linq;

namespace StockGTO.Controllers
{
    public class O_TicketController : Controller
    {
        private readonly AppDbContext _db;
        private const int DIY_CAP_PER_SLOT = 96; // 每時段上限

        public O_TicketController(AppDbContext db) => _db = db;

        // 前台：建單頁
        [HttpGet]
        public IActionResult O_Ticket_Booking() => View();

        // 後台：管理列表（先沿用舊畫面，資料來源已包含動態票種）
        [HttpGet]
        public async Task<IActionResult> O_Ticket_Manage(DateTime? start, DateTime? end)
        {
            var q = _db.O_Ticket_Bookings
                       .Include(x => x.DiyDetails)
                       .Include(x => x.TicketLines).ThenInclude(l => l.TicketType)
                       .AsQueryable();

            if (start.HasValue) q = q.Where(x => x.Date >= start.Value.Date);
            if (end.HasValue) q = q.Where(x => x.Date <= end.Value.Date);

            var list = await q.OrderByDescending(x => x.Date)
                              .ThenBy(x => x.TimeSlot)
                              .ToListAsync();
            return View(list);
        }

        // 建單：前台送出（AJAX JSON）
        [HttpPost]
        public async Task<IActionResult> BookingCreate([FromBody] BookingCreateVm vm)
        {
            try
            {
                if (vm == null) return Json(new { ok = false, message = "資料不可為空。" });

                var day = vm.Date.Date;
                vm.TimeSlot = (vm.TimeSlot ?? "09:00").Trim();
                vm.Area = string.IsNullOrWhiteSpace(vm.Area) ? null : vm.Area.Trim();
                vm.Company = string.IsNullOrWhiteSpace(vm.Company) ? "" : vm.Company.Trim();
                vm.GroupName = string.IsNullOrWhiteSpace(vm.GroupName) ? "" : vm.GroupName.Trim();
                vm.UserName = string.IsNullOrWhiteSpace(vm.UserName) ? "" : vm.UserName.Trim();
                vm.UserPhone = string.IsNullOrWhiteSpace(vm.UserPhone) ? "" : vm.UserPhone.Trim();

                // 1) 票種明細（支援新舊 payload）
                var lines = (vm.Lines ?? new List<TicketLineVm>())
                            .Where(l => l != null && l.TicketTypeId > 0 && l.Count > 0)
                            .GroupBy(l => l.TicketTypeId)
                            .Select(g => new TicketLineVm
                            {
                                TicketTypeId = g.Key,
                                Count = g.Sum(x => Math.Max(0, x.Count))
                            })
                            .ToList();

                // 過渡：若前端仍送 Adult/Discount/Night，映射到同名票種
                if (!lines.Any() && (vm.AdultCount + vm.DiscountCount + vm.NightCount) > 0)
                {
                    var needNames = new[] { "成人票", "優待票", "星光票" };
                    var typeMap = await _db.O_Ticket_TicketTypes
                                           .Where(t => needNames.Contains(t.Name) && t.IsActive)
                                           .ToDictionaryAsync(t => t.Name, t => t);

                    var temp = new List<TicketLineVm>();
                    if (vm.AdultCount > 0 && typeMap.TryGetValue("成人票", out var t1)) temp.Add(new TicketLineVm { TicketTypeId = t1.Id, Count = vm.AdultCount });
                    if (vm.DiscountCount > 0 && typeMap.TryGetValue("優待票", out var t2)) temp.Add(new TicketLineVm { TicketTypeId = t2.Id, Count = vm.DiscountCount });
                    if (vm.NightCount > 0 && typeMap.TryGetValue("星光票", out var t3)) temp.Add(new TicketLineVm { TicketTypeId = t3.Id, Count = vm.NightCount });

                    // 與既有 lines 合併再去重
                    lines = lines.Concat(temp)
                                 .GroupBy(l => l.TicketTypeId)
                                 .Select(g => new TicketLineVm { TicketTypeId = g.Key, Count = g.Sum(x => Math.Max(0, x.Count)) })
                                 .ToList();
                }

                if (!lines.Any())
                    return Json(new { ok = false, message = "請至少選擇一種票種（張數 > 0）。" });

                // 驗證票種存在 & 取得 IsEntrance / 單價
                var typeIds = lines.Select(l => l.TicketTypeId).Distinct().ToList();
                var types = await _db.O_Ticket_TicketTypes
                                     .Where(t => typeIds.Contains(t.Id) && t.IsActive)
                                     .ToDictionaryAsync(t => t.Id, t => t);
                if (types.Count != typeIds.Count)
                    return Json(new { ok = false, message = "含不存在或已停用的票種。" });

                // 基數：只計入 IsEntrance 的票種
                var baseTickets = lines.Sum(l => types[l.TicketTypeId].IsEntrance ? l.Count : 0);
                var diyTotal = vm.DiySlots?.Sum(s => Math.Max(0, s.Count)) ?? 0;

                if (baseTickets <= 0)
                    return Json(new { ok = false, message = "請先選擇至少 1 張入園票（IsEntrance=true 的票種）。" });
                if (diyTotal > baseTickets)
                    return Json(new { ok = false, message = $"DIY 人數({diyTotal}) 不可超過入園票人數({baseTickets})。" });

                // 2) 交易 + DIY 容量檢查
                await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                if (vm.DiySlots != null)
                {
                    foreach (var s in vm.DiySlots.Where(x => x.Count > 0))
                    {
                        var ts = (s.TimeSlot ?? "09:00").Trim();
                        var booked = await _db.O_Ticket_DiyBookings
                                              .Where(d => d.Date == day && d.TimeSlot == ts)
                                              .SumAsync(d => (int?)d.Count) ?? 0;

                        if (booked + s.Count > DIY_CAP_PER_SLOT)
                        {
                            var remain = DIY_CAP_PER_SLOT - booked;
                            return Json(new { ok = false, message = $"{day:yyyy-MM-dd} {ts} DIY 已 {booked}，尚餘 {Math.Max(remain, 0)}，本次 {s.Count} 超過上限 {DIY_CAP_PER_SLOT}。" });
                        }
                    }
                }

                // 3) 當日序號
                var maxSerial = await _db.O_Ticket_Bookings
                                         .Where(x => x.Date == day)
                                         .MaxAsync(x => (int?)x.SerialNo) ?? 0;
                var nextSerial = maxSerial + 1;
                if (nextSerial > 99)
                    return Json(new { ok = false, message = "當日序號已滿（O01~O99）。" });

                // 4) 建立主檔 + 票種明細 + DIY 明細
                var booking = new O_Ticket_Booking
                {
                    Date = day,
                    SerialNo = nextSerial,
                    Area = vm.Area,
                    TimeSlot = vm.TimeSlot,
                    GroupCode = null, // 後台再填
                    Company = vm.Company!,
                    GroupName = vm.GroupName!,
                    UserName = vm.UserName!,
                    UserPhone = vm.UserPhone!,
                    BusCount = Math.Max(0, vm.BusCount),
                    Guide = vm.Guide,
                    Note = vm.Note,
                    Status = "Unverified",
                    CreatedAt = DateTime.Now
                };

                foreach (var l in lines)
                {
                    var t = types[l.TicketTypeId];
                    booking.TicketLines.Add(new O_Ticket_BookingTicket
                    {
                        TicketTypeId = l.TicketTypeId,
                        Count = Math.Max(0, l.Count),
                        UnitPrice = t.UnitPrice
                    });
                }

                if (vm.DiySlots != null)
                {
                    foreach (var s in vm.DiySlots.Where(x => x.Count > 0))
                    {
                        booking.DiyDetails.Add(new O_Ticket_DiyBooking
                        {
                            Date = day,
                            TimeSlot = (s.TimeSlot ?? "09:00").Trim(),
                            Count = Math.Max(0, s.Count)
                        });
                    }
                }

                _db.O_Ticket_Bookings.Add(booking);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return Json(new { ok = true, id = booking.Id, message = "建立成功" });
            }
            catch (DbUpdateException ex)
            {
                return Json(new { ok = false, message = "資料庫寫入失敗：" + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = "例外：" + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> TicketTypes()
        {
            var list = await _db.O_Ticket_TicketTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.Sort).ThenBy(t => t.Id)
                .Select(t => new {
                    id = t.Id,
                    name = t.Name,
                    unitPrice = t.UnitPrice,   // 若 DB 是 Price，已在 OnModelCreating 對應
                    isEntrance = t.IsEntrance
                })
                .ToListAsync();

            return Json(list);
        }

        // ===== 前端接收 VM（含過渡欄位）=====
        public class BookingCreateVm
        {
            public DateTime Date { get; set; }
            public string? Area { get; set; }
            public string TimeSlot { get; set; } = "09:00";

            public string? Company { get; set; }
            public string? GroupName { get; set; }
            public string? UserName { get; set; }
            public string? UserPhone { get; set; }

            public int BusCount { get; set; }
            public bool Guide { get; set; }
            public string? Note { get; set; }

            // 新：動態票種
            public List<TicketLineVm>? Lines { get; set; }

            // 過渡：舊欄位
            public int AdultCount { get; set; }
            public int DiscountCount { get; set; }
            public int NightCount { get; set; }

            public List<DiySlotVm>? DiySlots { get; set; }
        }

        public class TicketLineVm
        {
            public int TicketTypeId { get; set; }
            public int Count { get; set; }
        }

        public class DiySlotVm
        {
            public string TimeSlot { get; set; } = "09:00";
            public int Count { get; set; }
        }
    }
}
