using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockGTO.Models
{
    /// <summary>票種主檔</summary>
    public class O_Ticket_TicketType
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;   // 例：成人票、優待票…

        /// <summary>是否列入「總人數」（入園票=true；DIY/加購=false）</summary>
        public bool IsEntrance { get; set; } = true;

        /// <summary>目前牌價（訂單明細會帶當下價過去）</summary>
        [Column("Price", TypeName = "decimal(18,2)")]      // ← 對應資料庫欄位 Price，型別 18,2
        public decimal UnitPrice { get; set; } = 0m;

        public bool IsActive { get; set; } = true;

        /// <summary>排序用（非必要，可選）</summary>
        public int Sort { get; set; } = 0;

        /// <summary>建立時間（方便查詢/排序，可由 DB 預設）</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
