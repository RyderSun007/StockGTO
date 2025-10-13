using System.ComponentModel.DataAnnotations;

namespace StockGTO.Models
{
    public class O_Ticket_Booking
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }          // yyyy-MM-dd

        public int SerialNo { get; set; }           // 當日序號 O01~O99

        [MaxLength(50)]
        public string? Area { get; set; }

        [MaxLength(5)]
        public string TimeSlot { get; set; } = "09:00";

        [MaxLength(20)]
        public string? GroupCode { get; set; }      // 後台確認者填

        [MaxLength(100)]
        public string Company { get; set; } = "";

        [MaxLength(100)]
        public string GroupName { get; set; } = "";

        [MaxLength(50)]
        public string UserName { get; set; } = "";

        [MaxLength(50)]
        public string UserPhone { get; set; } = "";

        public int BusCount { get; set; }           // 車數
        public bool Guide { get; set; }             // 是否導覽
        public string? Note { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Unverified";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ==== 導覽屬性 ====
        public ICollection<O_Ticket_BookingTicket> TicketLines { get; set; } = new List<O_Ticket_BookingTicket>();
        public ICollection<O_Ticket_DiyBooking> DiyDetails { get; set; } = new List<O_Ticket_DiyBooking>();
    }
}
