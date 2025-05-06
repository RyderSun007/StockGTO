using System;
using System.ComponentModel.DataAnnotations;

namespace StockGTO.Models
{
    public class IndexNews
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Summary { get; set; }

        public string ImageUrl { get; set; }

        public int Position { get; set; }  // 位置1~9

        public string LinkUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
