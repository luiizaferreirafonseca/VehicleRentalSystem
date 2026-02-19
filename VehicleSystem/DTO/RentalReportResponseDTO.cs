using System.Linq;

namespace VehicleRentalSystem.DTO
{
    public class RentalReportResponseDTO
    {
        public Guid RentalId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PenaltyFee { get; set; }
        public string Status { get; set; } = null!;

        // Related data blocks (Composition)
        public VehicleSummaryDTO Vehicle { get; set; } = null!;
        public CustomerSummaryDTO Customer { get; set; } = null!;
        public List<PaymentResponseDto> Payments { get; set; } = new();
        public List<AccessoryReportDto> Accessories { get; set; } = new();

        // Calculated properties (Report Business Rules)
        public decimal AmountPaid => Payments.Sum(p => p.Amount);
        public decimal AccessoriesTotal => Accessories.Sum(a => a.TotalPrice);
        public decimal BalanceDue => (TotalAmount + PenaltyFee + AccessoriesTotal) - AmountPaid;
        public string ReportNumber => $"RPT-{DateTime.Today:yyyyMMdd}-{RentalId.ToString()[..6].ToUpper()}";
    }

    public class AccessoryReportDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DailyRate { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Records for summarized join data
    public record VehicleSummaryDTO(string Brand, string Model, string LicensePlate);
    public record CustomerSummaryDTO(string Name, string Document);
}