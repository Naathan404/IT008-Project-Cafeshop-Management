using System;
using System.Collections.Generic;

namespace CoffeeShop.Models;

public partial class Otprequest
{
    public int RequestId { get; set; }

    public string Email { get; set; } = null!;

    public string Code { get; set; } = null!;

    public DateTime ExpireTime { get; set; }
}
