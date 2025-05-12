// ViewModels/HomeViewModel.cs

using StockGTO.Models;
using System.Collections.Generic;

namespace StockGTO.ViewModels
{
    public class HomeViewModel
    {
        // 📰 首頁公告（通常顯示在跑馬燈或 Carousel）
        public List<IndexPost> IndexPosts { get; set; } = new();

        // 🧠 靈魂語錄（可顯示名言佳句或哲理）
        public List<SoulQuote> SoulQuotes { get; set; } = new();

        // 📘 股票知識區（主要分類之一，可做為預設文章展示區）
        public List<ArticlePost> ArticlePosts { get; set; } = new();

        // 📊 財報分析（分析損益表、資產負債表等報表）
        public List<ArticlePost> FinancialStatements { get; set; } = new();

        // 📈 技術分析（K線、均線、指標分析）
        public List<ArticlePost> TechnicalAnalysis { get; set; } = new();

        // 🏢 基本面研究（分析公司營收、獲利能力、經營效率等）
        public List<ArticlePost> FundamentalAnalysis { get; set; } = new();

        // 💼 投資策略（價值投資、動能投資、長期持有等策略）
        public List<ArticlePost> InvestmentStrategy { get; set; } = new();

        // 💰 理財規劃（資金配置、退休規劃、保險等）
        public List<ArticlePost> WealthPlanning { get; set; } = new();

        // 🏭 產業分析（針對特定產業如半導體、AI、金融的深度探討）
        public List<ArticlePost> IndustryInsight { get; set; } = new();

        // 🌏 趨勢觀察（觀察市場動向、總經趨勢、資金流向）
        public List<ArticlePost> MarketTrends { get; set; } = new();

        // 🌐 國際市場（美股、港股、A股等全球市場概況）
        public List<ArticlePost> GlobalMarkets { get; set; } = new();

        // 🗺️ 全球投資（跨國投資、ETF全球配置）
        public List<ArticlePost> GlobalInvesting { get; set; } = new();

        // 🧠 投資心理（FOMO、恐慌、賠錢後的行為等情緒面分析）
        public List<ArticlePost> InvestorPsychology { get; set; } = new();

        // 📉 行為經濟學（結合心理學與金融市場的行為解釋）
        public List<ArticlePost> BehavioralFinance { get; set; } = new();

        // 💼 金融商品（債券、基金、REITs 等入門介紹）
        public List<ArticlePost> FinancialProducts { get; set; } = new();

        // 🧨 衍生性工具（期貨、選擇權、權證、槓桿 ETF 等）
        public List<ArticlePost> Derivatives { get; set; } = new();

        // 📣 重大新聞（顯示在首頁中段或焦點區）
        public List<IndexNews> IndexNews { get; set; }
    }
}
