﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // 為了支援 IFormFile
using Microsoft.AspNetCore.Identity; // ✅ 加入 Identity 支援

namespace StockGTO.Models
{
    public class ArticlePost
    {
        public int Id { get; set; }  // 主鍵 ID

        public string Title { get; set; } = string.Empty; // 文章標題

        public string Content { get; set; } = string.Empty; // 文章內容（支援 HTML 富內容）

        public string? ImageUrl { get; set; } = "/images/default.jpg"; // 封面圖片網址，預設為 default

        public string? Author { get; set; } = "Stock | 股市周刊"; // 作者名稱，預設為管理員

        // ✅ 分類欄位（FK 關聯）
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now; // 建立時間

        public string? Slug { get; set; } // SEO 專用的網址代稱（未使用可忽略）

        public bool IsPinned { get; set; } = false; // 是否置頂顯示（用於首頁精選）

        [NotMapped] // 不寫入資料庫，只在表單使用
        public IFormFile ImageFile { get; set; } // 圖片上傳（表單）

        public int ViewCount { get; set; } = 0; // 👁️ 瀏覽次數統計

        public string Tags { get; set; } = string.Empty; // 🏷️ 標籤字串（用逗號分隔，例如 "ETF,技術分析,長期投資"）

        // ✅ 加入會員帳號欄位（UserId 為 AspNetUsers.Id）
        public string? UserId { get; set; }  // 會員關聯

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }  // ✅ EF Core 會自動連到 AspNetUsers



        // ➕ 新增草稿與審核欄位
        public bool IsPublished { get; set; } = false; // 預設未公開
        public bool IsApproved { get; set; } = false; // 預設未審核

        // ➕ 新增留言與收藏
        public List<Comment> Comments { get; set; } = new();
        public List<ArticleFavorite> Favorites { get; set; } = new();

        
    }
}
