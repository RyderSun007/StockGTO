// Models/SoulQuote.cs
using System;

namespace StockGTO.Models
{
    public class SoulQuote
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Author { get; set; }
    }
}
