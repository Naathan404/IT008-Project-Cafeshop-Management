using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string StaffName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string StaffRole { get; set; } = null!;

    public string Phonenumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime Birthday { get; set; }

    public DateTime StartDate { get; set; }

    public string Gender { get; set; } = null!;

    public int? ShiftId { get; set; }

    public decimal? BaseSalary { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<InventoryHistory> InventoryHistories { get; set; } = new List<InventoryHistory>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Shift? Shift { get; set; }
}
