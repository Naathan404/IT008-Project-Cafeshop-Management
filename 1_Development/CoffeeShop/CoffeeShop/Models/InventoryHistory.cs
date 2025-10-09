using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class InventoryHistory
{
    public int HistoryId { get; set; }

    public int MaterialId { get; set; }

    public int ActionTypeId { get; set; }

    public decimal Quantity { get; set; }

    public DateTime Date { get; set; }

    public int StaffId { get; set; }

    public virtual ActionType ActionType { get; set; } = null!;

    public virtual Inventory Material { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
