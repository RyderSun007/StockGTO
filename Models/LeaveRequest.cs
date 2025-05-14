using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockGTO.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public string EmployeeName { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        public string Department { get; set; }

        public string Position { get; set; }

        [Required]
        public string LeaveType { get; set; }  // 事假、病假、婚假等

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [NotMapped]
        public int Days => (EndDate - StartDate).Days + 1;  // 自動計算請假天數

        public string Substitute { get; set; }  // 代理人

        public string Reason { get; set; }

        public string Status { get; set; } = "尚未審核";  // 預設狀態


        
        public string? Approver { get; set; } 

        public string Note { get; set; }
    }
}
