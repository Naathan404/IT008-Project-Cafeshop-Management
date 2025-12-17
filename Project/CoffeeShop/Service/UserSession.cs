using CoffeeShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Service
{
    public class UserSession
    {
        private static readonly UserSession _instance = new UserSession();
        public static UserSession Instance => _instance;

        // Constructor private -> ko thể tạo mới từ bên ngoài
        private UserSession() { }

        // Readonly
        public int StaffId { get; private set; }
        public string StaffName { get; private set; } = string.Empty;

        // Ghi dữ liệu vào ghi login
        public void SetUser(Staff staff)
        {
            this.StaffId = staff.StaffId;
            this.StaffName = staff.StaffName;
        }

        // Xóa dữ liệu khi logout
        public void ClearSession()
        {
            this.StaffId = 0;
            this.StaffName = string.Empty;
        }
    }
}
