using System;
using System.ComponentModel.DataAnnotations;

namespace StockGTO.Models
{
    public class OldSunStockModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StockName { get; set; } // 股票名稱

        [Required]
        public string? StockCode { get; set; } // 股票代號

        public int? Quantity { get; set; }     // 現股數量

        public decimal? BuyPrice { get; set; } // 成交均價

        public decimal? CurrentPrice { get; set; } // 現在價

        public decimal? TotalCost { get; set; } // 付出成本

        public decimal? CurrentValue { get; set; } // 現在市值

        public DateTime? LastDividendDate { get; set; } // 上次配息日期

        public decimal? DividendYield { get; set; } // 殖利率 %

        public string? DividendMonth { get; set; } // 配息月份

        public string? IndustryCategory { get; set; } // 產業別

        public int? DividendFrequency { get; set; } // 配息次數（年）

        public decimal? FiveYearTrend { get; set; } // 5年均線趨勢 %

        public decimal? BetaValue { get; set; } // Beta值（波動率）

        public string? BuySellAction { get; set; } // 買賣方向（買 or 賣）
    }
}
