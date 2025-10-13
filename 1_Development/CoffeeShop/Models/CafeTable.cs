using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class CafeTable
{
    public int TableId { get; set; }

    public string TableName { get; set; } = null!;

    public int TableStatus { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
