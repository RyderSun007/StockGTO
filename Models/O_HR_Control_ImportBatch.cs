// Models/O_HR_Control_ImportBatch.cs
namespace StockGTO.Models;
public class O_HR_Control_ImportBatch
{
    public long Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public int RowCount { get; set; }
}
