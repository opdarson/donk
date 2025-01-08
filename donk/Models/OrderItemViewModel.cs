
#nullable disable
using System;
using System.Collections.Generic;

namespace donk.Models;

public class OrderItemViewModel
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}