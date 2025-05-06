public class AjaxIndexPost
{
    public int Id { get; set; }
    public string StockName { get; set; } = string.Empty;
    public string StockCode { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal YesterdayPrice { get; set; }
    public decimal AvgPrice { get; set; }
    public decimal EPS { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}