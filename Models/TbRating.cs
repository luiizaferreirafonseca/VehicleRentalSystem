using System;
using System.Collections.Generic;

namespace VehicleRentalSystem.Models;

public partial class TbRating
{
    public Guid Id { get; set; }

    public Guid RentalId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TbRental Rental { get; set; } = null!;
}
