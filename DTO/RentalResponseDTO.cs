namespace VehicleRentalSystem.DTO
{
    public class RentalResponseDTO
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
    }

}
