using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class Shift
{
    public int ShiftId { get; set; }

    public string ShiftName { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
