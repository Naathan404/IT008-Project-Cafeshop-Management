using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? TableId { get; set; }

    public int? CustomerId { get; set; }

    public int StaffId { get; set; }

    public DateTime OrderDate { get; set; }

    public string OrderStatus { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual CafeTable? Table { get; set; }
}
