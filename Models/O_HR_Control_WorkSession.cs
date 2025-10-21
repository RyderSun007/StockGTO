// Models/O_HR_Control_WorkSession.cs
namespace StockGTO.Models;
public class O_HR_Control_WorkSession
{
    public long Id { get; set; }
    public int EmpId { get; set; }
    public long SourceBatchId { get; set; }
    public DateTime StartDT { get; set; }
    public DateTime? EndDT { get; set; }                   // 缺卡時為 null
    public int MinutesTotal { get; set; }
    public bool IsMissingIn { get; set; }
    public bool IsMissingOut { get; set; }
}

