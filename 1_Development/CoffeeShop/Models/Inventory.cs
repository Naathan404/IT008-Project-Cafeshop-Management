using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class Inventory
{
    public int MaterialId { get; set; }

    public string? MaterialName { get; set; }

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = null!;

    public decimal Threshold { get; set; }

    public virtual ICollection<InventoryHistory> InventoryHistories { get; set; } = new List<InventoryHistory>();
}
