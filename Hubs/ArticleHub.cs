using Microsoft.AspNetCore.SignalR;

namespace StockGTO.Hubs  
{
    public class ArticleHub : Hub
    {
        public async Task SendArticles(object articleList)
        {
            await Clients.All.SendAsync("ReceiveArticles", articleList);
        }
    }
}
