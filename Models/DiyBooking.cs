using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockGTO.Models
{
    /// <summary>
    /// DiyBooking：紀錄每一筆 DIY 預約資訊
    /// 後端可用 CRUD 修改或刪除資料（如有誤填或取消）
    /// </summary>
    public class DiyBooking
    {
        /// <summary>
        /// 主鍵 (PK)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 預約日期（活動當天日期）
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// 預約時段，例如：09:00、11:00
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string TimeSlot { get; set; }

        /// <summary>
        /// 外鍵：票種 ID（對應 DiyTicketType.Id）
        /// </summary>
        [Required]
        public int TicketTypeId { get; set; }

        /// <summary>
        /// 導覽屬性（EF Core 用於關聯查詢票種資料）
        /// </summary>
        [ForeignKey("TicketTypeId")]
        public DiyTicketType TicketType { get; set; }

        /// <summary>
        /// 總參加人數（大人+小孩統計總數）
        /// 【兒童陪同一名大人】的安排由後端人員在實際現場統計後再修改
        /// </summary>
        public int BookedCount { get; set; }

        /// <summary>
        /// 公司名稱（若為團體預約）
        /// </summary>
        [MaxLength(100)]
        public string CompanyName { get; set; }

        /// <summary>
        /// 預約人姓名
        /// </summary>
        [MaxLength(50)]
        public string UserName { get; set; }

        /// <summary>
        /// 預約人電話
        /// </summary>
        [MaxLength(20)]
        public string UserPhone { get; set; }

        /// <summary>
        /// Email（如需後續寄送通知，可留空）
        /// </summary>
        [MaxLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// 備註（例如：特殊需求、是否需要安排大人陪同等）
        /// </summary>
        [MaxLength(500)]
        public string Note { get; set; }

        /// <summary>
        /// 狀態：Pending=待確認、Confirmed=已確認、Cancelled=已取消
        /// 可由後端 CRUD 修改
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// 預約建立時間（自動寫入）
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
