using System.ComponentModel.DataAnnotations.Schema;

namespace StockGTO.Models
{
    /// <summary>訂單 × 票種 明細（動態票種）</summary>
    public class O_Ticket_BookingTicket
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public O_Ticket_Booking? Booking { get; set; }

        public int TicketTypeId { get; set; }
        public O_Ticket_TicketType? TicketType { get; set; }

        public int Count { get; set; }                    // 張數（>=0）

        [Column(TypeName = "decimal(18,2)")]             // ← 單價 18,2
        public decimal UnitPrice { get; set; }            // 成交單價（下單當下的價）
    }
}
