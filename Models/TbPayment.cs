using System;
using System.Collections.Generic;

namespace VehicleRentalSystem.Models;

public partial class TbPayment
{
    public Guid Id { get; set; }

    public Guid RentalId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime PaymentDate { get; set; }

    public virtual TbRental Rental { get; set; } = null!;
}
