using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Service
{
    // Tạo file này trong thư mục Messages của bạn
    public class OrderCompletedMessage
    {
        public int? TableId { get; set; }
    }
}
