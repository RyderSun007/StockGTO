// Models/O_HR_Control_Employee.cs
namespace StockGTO.Models;
public class O_HR_Control_Employee
{
    public int Id { get; set; }
    public string EmpNo { get; set; } = string.Empty;   // UNIQUE
    public string EmpName { get; set; } = string.Empty;
    public int DeptId { get; set; }
    public string EmpType { get; set; } = "PT";         // FT/PT
}