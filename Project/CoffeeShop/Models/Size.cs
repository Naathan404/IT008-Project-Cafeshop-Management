using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class Size
{
    public int SizeId { get; set; }

    public string SizeName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<ItemPrice> ItemPrices { get; set; } = new List<ItemPrice>();
}
