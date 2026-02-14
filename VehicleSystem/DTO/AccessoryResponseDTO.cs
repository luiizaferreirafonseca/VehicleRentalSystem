namespace VehicleRentalSystem.DTO;

public class AccessoryResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal DailyRate { get; set; }
}