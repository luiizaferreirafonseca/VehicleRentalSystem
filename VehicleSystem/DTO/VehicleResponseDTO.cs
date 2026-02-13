namespace VehicleRentalSystem.DTO
{
    public class VehicleResponseDTO
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal DailyRate { get; set; }
        public string Status { get; set; } = null!;
        public string LicensePlate { get; set; } = null!;
    }
}
