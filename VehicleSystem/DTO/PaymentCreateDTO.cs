using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalSystem.DTO
{
    public class PaymentCreateDTO
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "The payment amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "The payment method is required.")]
        public VehicleRentalSystem.Enums.EnumPaymentMethod? PaymentMethod { get; set; }
    }
}
