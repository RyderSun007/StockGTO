using Microsoft.AspNetCore.SignalR;

namespace StockGTO.Hubs  // ← 如果你專案不是叫 StockGTO 就換成你的
{
    public class ArticleHub : Hub
    {
        public async Task SendArticles(object articleList)
        {
            await Clients.All.SendAsync("ReceiveArticles", articleList);
        }
    }
}
