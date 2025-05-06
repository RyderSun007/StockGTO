using Microsoft.AspNetCore.Mvc;
using StockGTO.Data;
using StockGTO.Models;
using System.Linq;

namespace StockGTO.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // 顯示所有員工資料
        public IActionResult ShowEmployee()
        {
            //2025/04/16 加入驗證才可以進入
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            {
                return RedirectToAction("Login", "Account");
            }



            var employees = _context.Employees.ToList();
            return View(employees);
        }

        // ======== 新增 CreateEMP ========
        public IActionResult CreateEMP()
        {
            //2025/04/16 加入驗證才可以進入 CreateEMP 頁面
            //if (HttpContext.Session.GetString("IsLoggedIn") != "true")
            //{
            //    return RedirectToAction("Login", "Account");
            //}

            return View();
        }

        [HttpPost]
        public IActionResult CreateEMP(Employee employee)
        {
            Console.WriteLine("👀 送過來的 Model：Name = " + employee?.Name);
            if (ModelState.IsValid)
            {
                _context.Employees.Add(employee);
                _context.SaveChanges();
                return RedirectToAction("ShowEmployee");
            }
            return View(employee);
        }

        // ======== 編輯 EditEMP ========
        // 顯示編輯表單
        public IActionResult EditEMP(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
                return NotFound();

            return View(employee);
        }

        // 處理送出的編輯表單
        [HttpPost]
        public IActionResult EditEMP(Employee employee)
        {
            Console.WriteLine("🛠️ 接收到修改資料：" + employee.Name);
            Console.WriteLine("🧪 接收到 IsActive：" + employee.IsActive);

            if (ModelState.IsValid)
            {
                // 🔧 改成「先查舊資料」
                var original = _context.Employees.Find(employee.Id);
                if (original == null)
                    return NotFound();

                // ✅ 手動更新欄位（這樣就不會覆蓋掉沒送進來的欄位）
                original.Name = employee.Name;
                original.Phone = employee.Phone;
                original.Email = employee.Email;
                original.Department = employee.Department;
                original.HireDate = employee.HireDate;
                original.Salary = employee.Salary;
                original.IsActive = employee.IsActive; // ✅ ✅ ✅ 這裡保證能成功更新！

                _context.SaveChanges();
                Console.WriteLine("✅ 修改成功：" + employee.Name);
                return RedirectToAction("ShowEmployee");
            }

            Console.WriteLine("❌ ModelState 無效，請檢查輸入欄位！");
            return View(employee);
        }


        // ======== 刪除 DeleteEMP ========
        public IActionResult DeleteEMP(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost, ActionName("DeleteEMP")]
        public IActionResult DeleteConfirmedEMP(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                _context.SaveChanges();
            }
            return RedirectToAction("ShowEmployee");
        }
    }
}
