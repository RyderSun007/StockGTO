using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace StockGTO.Services
{
    public class AlphaVantageService
    {
        private readonly string _apiKey = "demo"; // ⛔ 請換成你自己的金鑰！

        public async Task<string> GetStockPrice(string symbol)
        {
            using var client = new HttpClient();
            var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}";

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var price = json["Global Quote"]?["05. price"]?.ToString();

                if (string.IsNullOrWhiteSpace(price))
                {
                    // 🔍 除錯用：回傳內容印出來
                    System.Console.WriteLine("⚠️ API 回傳但無價格：" + content);
                }

                return price ?? "N/A";
            }
            catch (HttpRequestException ex)
            {
                System.Console.WriteLine("❌ API 請求錯誤：" + ex.Message);
                return "Error";
            }
        }
    }
}
