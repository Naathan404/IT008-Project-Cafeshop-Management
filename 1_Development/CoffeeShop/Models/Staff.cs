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

    public int? ShiftId { get; set; }

    public decimal? BaseSalary { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<InventoryHistory> InventoryHistories { get; set; } = new List<InventoryHistory>();

    public virtual Shift? Shift { get; set; }

    public virtual ICollection<StaffAttendance> StaffAttendances { get; set; } = new List<StaffAttendance>();
}
