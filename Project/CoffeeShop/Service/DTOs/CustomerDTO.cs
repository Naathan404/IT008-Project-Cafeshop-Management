using System;

namespace CoffeeShop.Service.DTOs
{
    public class CustomerDTO : BaseDTO
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int Point { get; set; }
        public string Tier { get; set; } = "VIP1";
        public DateTime JoinDate { get; set; }
        // Tổng tiền đã chi tiêu tính thông qua Orders
        public string TotalSpentFormat { get; set; } = null!;
    }
}