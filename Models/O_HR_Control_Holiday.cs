// Models/O_HR_Control_Holiday.cs
using System.ComponentModel.DataAnnotations;

namespace StockGTO.Models;
public class O_HR_Control_Holiday
{
    [Key]                         // ← 明確告訴 PK
    public DateOnly Date { get; set; }   // PK

    public string Name { get; set; } = string.Empty; // 可留著當「名稱」
    public string? Memo { get; set; }                //表單裡的「說明」

}
