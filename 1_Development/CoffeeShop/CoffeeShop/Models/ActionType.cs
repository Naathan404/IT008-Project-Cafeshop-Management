using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class ActionType
{
    public int ActionTypeId { get; set; }

    public string ActionName { get; set; } = null!;

    public virtual ICollection<InventoryHistory> InventoryHistories { get; set; } = new List<InventoryHistory>();
}
