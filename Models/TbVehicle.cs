using System;
using System.Collections.Generic;

namespace VehicleRentalSystem.Models;

public partial class TbVehicle
{
    public Guid Id { get; set; }

    public string Model { get; set; } = null!;

    public DateOnly Int { get; set; }

    public string Brand { get; set; } = null!;

    public decimal DailyRate { get; set; }

    public string Status { get; set; } = null!;

    public string LicensePlate { get; set; } = null!;

    public virtual ICollection<TbRental> TbRentals { get; set; } = new List<TbRental>();
}
