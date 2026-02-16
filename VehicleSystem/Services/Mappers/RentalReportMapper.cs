using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using System.Linq;

namespace VehicleRentalSystem.Services.Mappers
{
    public static class RentalMapper
    {
        /// <summary>
        /// Extensão para converter a entidade TbRental em um DTO de Relatório completo.
        /// </summary>
        public static RentalReportResponseDTO? ToReportDto(this TbRental? r)
        {
            if (r == null) return null;

            return new RentalReportResponseDTO
            {
                RentalId = r.Id,
                StartDate = r.StartDate,
                EndDate = r.ActualEndDate ?? r.ExpectedEndDate,
                    TotalAmount = r.TotalAmount ?? 0m,
                    PenaltyFee = r.PenaltyFee ?? 0m,
                Status = r.Status,

                // Mapeamento do Bloco do Veículo
                Vehicle = r.Vehicle != null
                    ? new VehicleSummaryDTO(r.Vehicle.Brand, r.Vehicle.Model, r.Vehicle.LicensePlate)
                    : null!,

                // Mapeamento do Bloco do Cliente (User)
                Customer = r.User != null
                    ? new CustomerSummaryDTO(r.User.Name, string.Empty)
                    : null!,

                // Mapeamento da Lista de Pagamentos
                Payments = r.TbPayments?.Select(p => new PaymentResponseDto
                {
                    Id = p.Id,
                    RentalId = p.RentalId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate
                }).ToList() ?? new List<PaymentResponseDto>()
                ,
                Accessories = r.TbRentalAccessories?.Select(a =>
                {
                    var start = r.StartDate;
                    var end = r.ActualEndDate ?? r.ExpectedEndDate;
                    var days = Math.Max(1, (int)Math.Ceiling((end - start).TotalDays));

                    var unitPrice = a.UnitPrice != 0m ? a.UnitPrice : a.Accessory?.DailyRate ?? 0m;
                    var quantity = a.Quantity;
                    var totalPrice = a.TotalPrice != 0m ? a.TotalPrice : unitPrice * quantity * days;

                    return new AccessoryReportDto
                    {
                        Id = a.AccessoryId,
                        Name = a.Accessory?.Name ?? string.Empty,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice,
                        DailyRate = a.Accessory?.DailyRate ?? unitPrice,
                        StartDate = start,
                        EndDate = end
                    };
                }).ToList() ?? new List<AccessoryReportDto>()
            };
        }
    }
}