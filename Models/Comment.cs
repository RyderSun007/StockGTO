using StockGTO.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int ArticlePostId { get; set; }
    public ArticlePost ArticlePost { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}