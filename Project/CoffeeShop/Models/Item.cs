using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class Item
{
    public int ItemId { get; set; }

    public string ItemName { get; set; } = null!;

    public string? ImagePath { get; set; }

    public string? Info { get; set; }

    public int CategoryId { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ItemPrice> ItemPrices { get; set; } = new List<ItemPrice>();
}
