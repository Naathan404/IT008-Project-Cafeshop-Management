using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? TableId { get; set; }

    public int? CustomerId { get; set; }

    public int StaffId { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal SubTotal { get; set; }

    public int? DiscountId { get; set; }

    public decimal? DiscountMoney { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public virtual Customer? Customer { get; set; }

    public virtual Discount? Discount { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Staff Staff { get; set; } = null!;

    public virtual CafeTable? Table { get; set; }
}
