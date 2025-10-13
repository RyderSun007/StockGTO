using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockGTO.Data;
using StockGTO.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StockGTO.Controllers
{
    public class OtherworldController : Controller
    {
        private readonly AppDbContext _context;

        public OtherworldController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ✅ Booking 首頁：初始載入票種（第一次載入顯示最大容量）
        /// </summary>
        /// ////
        public async Task<IActionResult> Booking()
        {
            var tickets = await _context.DiyTicketTypes
                .Where(t => t.IsActive)
                .ToListAsync();

            return View(tickets);
        }




        /// <summary>
        /// ✅ Booking2 首頁：新版預約 UI
        /// </summary>
        public async Task<IActionResult> Booking2()
        {
            var tickets = await _context.DiyTicketTypes
                .Where(t => t.IsActive)
                .ToListAsync();

            return View(tickets);  // ⚡ 會自動找 Views/Otherworld/Booking2.cshtml
        }
        /// <summary>
        /// ✅ 後台查看所有預約資料（新版本 ManageBookings2）
        /// </summary>
        public async Task<IActionResult> ManageBookings2()
        {
            var bookings = await _context.DiyBookings
                .Include(b => b.TicketType)   // 把票種一起載入
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings); // 對應 Views/Otherworld/ManageBookings2.cshtml
        }






        /// <summary>
        /// ✅ 根據日期動態載入剩餘數量（AJAX 呼叫）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTicketsByDate(DateTime date)
        {
            var tickets = await _context.DiyTicketTypes
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Price,
                    t.ImageUrl,
                    Remaining = t.MaxCapacity -
                        (_context.DiyBookings
                            .Where(b => b.TicketTypeId == t.Id && b.Date == date)
                            .Sum(b => (int?)b.BookedCount) ?? 0)
                })
                .ToListAsync();

            return Json(tickets);
        }

        /// <summary>
        /// ✅ 預約邏輯（寫入 DB，後端強化驗證）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BookTicket([FromBody] BookTicketRequest request)
        {
            try
            {
                // -------- 防呆：request 是否為 null --------
                if (request == null)
                    return Json(new { success = false, message = "系統錯誤：未接收到資料" });

                // -------- 後端欄位驗證 --------
                if (request.TicketTypeId <= 0)
                    return Json(new { success = false, message = "票種錯誤" });

                if (request.Date == default)
                    return Json(new { success = false, message = "請選擇有效日期" });

                if (string.IsNullOrWhiteSpace(request.TimeSlot))
                    return Json(new { success = false, message = "請選擇時段" });

                if (request.BookedCount <= 0)
                    return Json(new { success = false, message = "預約人數必須大於 0" });

                if (string.IsNullOrWhiteSpace(request.UserName))
                    return Json(new { success = false, message = "請輸入姓名" });

                if (string.IsNullOrWhiteSpace(request.UserPhone))
                    return Json(new { success = false, message = "請輸入電話" });

                // -------- 查詢票種 --------
                var ticket = await _context.DiyTicketTypes.FindAsync(request.TicketTypeId);
                if (ticket == null)
                    return Json(new { success = false, message = "找不到票種" });

                // -------- 計算剩餘名額 --------
                var bookedCount = await _context.DiyBookings
                    .Where(b => b.TicketTypeId == request.TicketTypeId &&
                                b.Date == request.Date.Date &&
                                b.TimeSlot == request.TimeSlot)
                    .SumAsync(b => (int?)b.BookedCount ?? 0);

                if (bookedCount + request.BookedCount > ticket.MaxCapacity)
                    return Json(new
                    {
                        success = false,
                        message = $"該時段剩餘名額不足 (剩餘 {ticket.MaxCapacity - bookedCount} 人)"
                    });

                // -------- 寫入預約 --------
                var booking = new DiyBooking
                {
                    TicketTypeId = request.TicketTypeId,
                    Date = request.Date.Date,
                    TimeSlot = request.TimeSlot,
                    BookedCount = request.BookedCount,
                    CompanyName = request.CompanyName?.Trim(),
                    UserName = request.UserName.Trim(),
                    UserPhone = request.UserPhone.Trim(),
                    Email = request.Email?.Trim(),
                    Note = request.Note?.Trim(),
                    Status = "Pending", // 預設狀態
                    CreatedAt = DateTime.Now
                };

                _context.DiyBookings.Add(booking);
                await _context.SaveChangesAsync();

                var remaining = ticket.MaxCapacity - (bookedCount + request.BookedCount);

                return Json(new { success = true, remaining });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "系統錯誤：" + ex.Message });
            }
        }

        /// <summary>
        /// ✅ 後台查看所有預約資料（管理用）
        /// </summary>
        public async Task<IActionResult> ManageBookings()
        {
            var bookings = await _context.DiyBookings
                .Include(b => b.TicketType)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }
    }

    /// <summary>
    /// ✅ AJAX 傳遞結構
    /// </summary>
    public class BookTicketRequest
    {
        public int TicketTypeId { get; set; }
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; }
        public int BookedCount { get; set; }
        public string CompanyName { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
    }
}
