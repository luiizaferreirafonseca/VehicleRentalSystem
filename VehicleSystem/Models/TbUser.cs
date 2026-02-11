using System;
using System.Collections.Generic;

namespace VehicleRentalSystem.Models;

public partial class TbUser
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool? Active { get; set; }

    public virtual ICollection<TbRental> TbRentals { get; set; } = new List<TbRental>();
}
