using System;
using System.Collections.Generic;

namespace VehicleRentalSystem.Models;

public partial class TbRental
{
    public Guid Id { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime ExpectedEndDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? PenaltyFee { get; set; }

    public string Status { get; set; } = null!;

    public Guid VehicleId { get; set; }

    public Guid UserId { get; set; }

    public decimal DailyRate { get; set; }

    public virtual ICollection<TbPayment> TbPayments { get; set; } = new List<TbPayment>();

    public virtual ICollection<TbRating> TbRatings { get; set; } = new List<TbRating>();

    public virtual ICollection<TbRentalAccessory> TbRentalAccessories { get; set; } = new List<TbRentalAccessory>();

    public virtual TbUser User { get; set; } = null!;

    public virtual TbVehicle Vehicle { get; set; } = null!;
}
