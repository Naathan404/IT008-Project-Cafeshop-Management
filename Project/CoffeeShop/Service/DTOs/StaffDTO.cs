using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Service.DTOs
{
    public class StaffDTO : BaseDTO
    {
        public int StaffId { get; set; }
        public string DisplayID
        {
            get
            {
                return DateTime.Now.Year + StaffId.ToString("D3");
            }
        }

        public string StaffName { get; set; } = null!;

        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string StaffRole { get; set; } = null!;

        public string Phonenumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime? Birthday { get; set; } = null!;

        public DateTime? StartDate { get; set; } = null!;

        public string Gender { get; set; } = null!;

        public string? ShiftId { get; set; } = null!;

        public string ShiftName { get; set; } = null!;

        public string BaseSalary { get; set; } = null!;
    }
}
