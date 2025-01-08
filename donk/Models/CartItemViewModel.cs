
#nullable disable
using System;
using System.Collections.Generic;

namespace donk.Models;

public class CartItemViewModel
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }


    public virtual Products Product { get; set; }
}