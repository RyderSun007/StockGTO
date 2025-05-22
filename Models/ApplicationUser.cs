using Microsoft.AspNetCore.Identity;
using System;


namespace StockGTO.Models
{
    public class ApplicationUser : IdentityUser
    {
        // 顯示用的名稱，例如 Google/Facebook 帳號名稱
        public string? DisplayName { get; set; }

        // 使用者大頭貼網址，登入時可自動取得
        public string? ProfileImage { get; set; }

        // 註冊來源，例如 "Google"、"Facebook"、"Line"、"Local"
        public string? RegisterSource { get; set; }

        // 註冊時間，用於記錄會員加入時間
        public DateTime? RegisterTime { get; set; }

        // 是否為 VIP 會員，可用於會員分級系統
        public bool IsVIP { get; set; }

        // 系統管理者備註，可寫封鎖理由、審核備註等
        public string? UserNote { get; set; }
    }
}