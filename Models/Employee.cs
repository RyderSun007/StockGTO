// Models/Employee.cs

using System;

namespace StockGTO.Models
{
    public class Employee
    {
        public int Id { get; set; }               // 主鍵 ID
        public string? Name { get; set; }          // 員工姓名
        public string? Phone { get; set; }         // 電話
        public string? Email { get; set; }         // Email
        public string? Department { get; set; }    // 部門
        public DateTime HireDate { get; set; }    // 到職日
        public decimal Salary { get; set; }       // 薪資
        public bool IsActive { get; set; }        // 是否在職
    }
}
