using System.ComponentModel.DataAnnotations;

namespace StockGTO.Models
{
    public class O_Ticket_DiyBooking
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public O_Ticket_Booking Booking { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }              // 與主檔同日，便於彙總

        [MaxLength(5)]
        public string TimeSlot { get; set; } = "09:00"; // 09:00/11:00/...

        public int Count { get; set; }                  // 該時段 DIY 張數
    }
}
