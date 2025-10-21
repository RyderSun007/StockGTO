namespace StockGTO.Models;
public class O_HR_Control_DailyAttendance
{
    public int EmpId { get; set; }
    public DateOnly WorkDate { get; set; }                 // PK 複合
    public int MinutesBreak { get; set; }                  // 午休實扣
    public int MinutesWorkNet { get; set; }                // 淨工時(扣午休)
    public int OT_Minutes_8to10 { get; set; }              // 平日加班 8–10
    public int OT_Minutes_10to12 { get; set; }             // 平日加班 10–12
    public int OT_Rest_Minutes { get; set; }               // 第七天
    public int OT_Holiday_Minutes { get; set; }            // 國假
    public int ConsecutiveDays { get; set; }
    public bool IsRestDayBy7th { get; set; }
    public bool IsHoliday { get; set; }
    public bool HasMissingPunch { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}