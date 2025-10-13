using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int PriceId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public string? Note { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ItemPrice Price { get; set; } = null!;
}
