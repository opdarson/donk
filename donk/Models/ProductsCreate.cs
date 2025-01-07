﻿
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace donk.Models;

public partial class ProductsCreate
{

    public int Id { get; set; }
    [Required]
    [StringLength(100)]

    [Display(Name = "名稱")]
    public string Name { get; set; }
    [Required]
    [Display(Name = "描述")]
    public string Description { get; set; }

    [Required]

    [Display(Name = "價格")]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public string ImageData { get; set; }
}