﻿
#nullable disable
using System;
using System.Collections.Generic;

namespace donk.Models;

public class OrderViewModel
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemViewModel> Items { get; set; }
}