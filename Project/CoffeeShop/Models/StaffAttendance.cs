using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class StaffAttendance
{
    public int AttendanceId { get; set; }

    public int StaffId { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
