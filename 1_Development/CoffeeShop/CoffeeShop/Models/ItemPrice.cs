using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class ItemPrice
{
    public int PriceId { get; set; }

    public int ItemId { get; set; }

    public int? SizeId { get; set; }

    public decimal Price { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Size? Size { get; set; }
}
