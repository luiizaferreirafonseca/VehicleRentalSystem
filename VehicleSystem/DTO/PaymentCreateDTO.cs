using System;
using System.ComponentModel.DataAnnotations;

namespace VehicleRentalSystem.DTO
{
    public class PaymentCreateDTO
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor do pagamento deve ser maior que zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "O método de pagamento é obrigatório.")]
        public VehicleRentalSystem.Enums.EnumPaymentMethod? PaymentMethod { get; set; }
    }
}
