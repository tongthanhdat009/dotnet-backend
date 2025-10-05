﻿using System;
using System.Collections.Generic;

namespace dotnet_backend.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int? CategoryId { get; set; }

    public int? SupplierId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Barcode { get; set; }

    public decimal Price { get; set; }

    public string? Unit { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Supplier? Supplier { get; set; }
}
