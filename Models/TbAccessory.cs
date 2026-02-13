using System;
using System.Collections.Generic;

namespace VehicleRentalSystem.Models;

public partial class TbAccessory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal DailyRate { get; set; }

    public virtual ICollection<TbRentalAccessory> TbRentalAccessories { get; set; } = new List<TbRentalAccessory>();
}
