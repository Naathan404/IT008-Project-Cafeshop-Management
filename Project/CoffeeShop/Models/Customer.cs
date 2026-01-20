using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public int Point { get; set; }

    public string? Tier { get; set; }

    public DateTime JoinDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
