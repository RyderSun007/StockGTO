using System.Collections.Generic;

namespace StockGTO.Models
{
    public class Category
    {
        public int Id { get; set; } // 主鍵
        public string Code { get; set; } = string.Empty; // 分類代碼（英文）
        public string Name { get; set; } = string.Empty; // 中文名稱
        public string? Description { get; set; } // 說明文字
        public bool IsActive { get; set; } = true; // 是否啟用

        // 🔢 排序欄位
        public int SortOrder { get; set; } = 0; // 預設排序為 0

        // 🔁 反向導覽
        public ICollection<ArticlePost>? ArticlePosts { get; set; }


    }
}
