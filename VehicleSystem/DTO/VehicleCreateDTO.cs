namespace VehicleRentalSystem.DTO
{
    public class VehicleCreateDTO
    {
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal DailyRate { get; set; }
        public string LicensePlate { get; set; } = null!;
    }
}
