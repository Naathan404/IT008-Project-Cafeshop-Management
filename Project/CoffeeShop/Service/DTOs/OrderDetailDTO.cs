using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Service.DTOs
{
    public class OrderDetailDTO : BaseDTO
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int PriceId { get; set; }
        public string ItemName { get; set; } = null!;
        public string? SizeName { get; set; }
        public int Quantity { get; set; }
        public string UnitPrice { get; set; } = null!;
        public string TotalPrice { get; set; } = null!;
        public string? Note { get; set; }
    }
}
