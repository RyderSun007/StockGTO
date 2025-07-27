using System.ComponentModel.DataAnnotations;

namespace StockGTO.Models
{
    /// <summary>
    /// DiyTicketType：票種資料表
    /// 用來記錄每一種票的基本資訊（名稱、價格、最大容量等）
    /// 例如：成人票、兒童票、VIP 套票
    /// </summary>
    public class DiyTicketType
    {
        /// <summary>
        /// 主鍵 (PK)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 票種名稱，例如 成人票、兒童票、VIP
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 單人票價
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// 每個時段的最大可容納人數
        /// 例如 DIY 教室最大 96 人
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// 卡片顯示用圖片
        /// 可以放假圖或正式活動宣傳圖
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 是否啟用這個票種
        /// true = 可售票, false = 停用
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
