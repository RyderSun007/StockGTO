using StockGTO.Models;

public class ArticleFavorite
{
    public int Id { get; set; }

    public int ArticlePostId { get; set; }
    public ArticlePost ArticlePost { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}