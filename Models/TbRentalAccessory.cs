using System;
using System.Collections.Generic;

namespace VehicleRentalSystem.Models;

public partial class TbRentalAccessory
{
    public Guid RentalId { get; set; }

    public Guid AccessoryId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public virtual TbAccessory Accessory { get; set; } = null!;

    public virtual TbRental Rental { get; set; } = null!;
}
