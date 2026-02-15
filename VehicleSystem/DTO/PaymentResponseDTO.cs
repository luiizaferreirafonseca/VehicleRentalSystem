namespace VehicleRentalSystem.DTO;

public class PaymentResponseDto
{
    public Guid Id { get; set; }

    public Guid RentalId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime PaymentDate { get; set; }

}