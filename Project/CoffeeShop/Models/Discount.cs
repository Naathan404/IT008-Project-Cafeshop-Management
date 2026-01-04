using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class Discount
{
    public int DiscountId { get; set; }

    public string DiscountCode { get; set; } = null!;

    public string DiscountName { get; set; } = null!;

    public int DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal? MinimumOrderValue { get; set; }

    public decimal? MaximumDiscountAmount { get; set; }

    public bool IsActive { get; set; }

    public int UsedCount { get; set; }

    public int UseLimit { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
