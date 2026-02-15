namespace VehicleRentalSystem.DTO;

public class VehicleUpdateDTO
{
    public decimal DailyRate { get; set; }
    public int Year { get; set; }
    public string Status { get; set; } = string.Empty;
}