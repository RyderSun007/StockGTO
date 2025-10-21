namespace StockGTO.Models;
public class O_HR_Control_RawTimePunch
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public string EmpNo { get; set; } = string.Empty;       // 文字
    public string EmpName { get; set; } = string.Empty;     // 文字（原始）
    public string DeptName { get; set; } = string.Empty;    // 文字（原始）
    public DateOnly PunchDate { get; set; }                 // 日期
    public string PunchTimeStr { get; set; } = string.Empty;// 文字（HH:mm）
    public DateTime PunchDateTime { get; set; }             // 解析後
    public int? SourceRowNo { get; set; }
    public string ParseStatus { get; set; } = "OK";         // OK/WARN/ERROR
    public string? ErrorMsg { get; set; }
}