using System.Text;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services.interfaces;
using VehicleRentalSystem.Services.Mappers;

namespace VehicleRentalSystem.Services
{
    public class RentalReportService : IRentalReportService
    {
        private readonly IRentalReportRepository _reportRepository;

        public RentalReportService(IRentalReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<RentalReportResponseDTO?> GetRentalReportAsync(Guid id)
        {
            var rental = await _reportRepository.GetRentalWithDetailsAsync(id);
            if (rental == null)
                return null;

            return rental.ToReportDto();
        }

        // MODELO TXT
        public async Task<byte[]?> ExportRentalReportAsync(Guid id)
        {
            var dto = await GetRentalReportAsync(id);
            if (dto == null)
                return null;

            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, new UTF8Encoding(true), leaveOpen: true);

            writer.WriteLine($"Relatório de Locação - {dto.ReportNumber}   -   Data: {DateTime.Now}");
            writer.WriteLine();
            writer.WriteLine("Dados do Contrato ---------------------");
            writer.WriteLine();
            writer.WriteLine($"Cliente:  {dto.Customer.Name}");
            writer.WriteLine();
            writer.WriteLine($"Início: {dto.StartDate:dd-MM-yyyy}");
            writer.WriteLine($"Término: {dto.EndDate:dd-MM-yyyy}");
            writer.WriteLine($"Total dias: {(dto.EndDate - dto.StartDate).Days}");
            writer.WriteLine($"Status: {dto.Status}");
            writer.WriteLine();

            writer.WriteLine("Dados da Locação ---------------------");
            writer.WriteLine();
            writer.WriteLine("Veículo:");
            writer.WriteLine($"  {dto.Vehicle.Brand} {dto.Vehicle.Model} - {dto.Vehicle.LicensePlate}");
            writer.WriteLine();

            writer.WriteLine("Acessórios:");
            if (dto.Accessories != null && dto.Accessories.Any())
            {
                foreach (var a in dto.Accessories)
                    writer.WriteLine($"  {a.Name} x{a.Quantity} - {a.TotalPrice} ({a.DailyRate}/dia)");
            }
            else
                writer.WriteLine("  Não há acessórios nessa locação.");

            writer.WriteLine();
            writer.WriteLine("-----------------------------------------");
            writer.WriteLine();

            writer.WriteLine($"(+)  Total Veículo: R$ {dto.TotalAmount}");
            writer.WriteLine($"(+)  Total Acessórios: R$ {dto.AccessoriesTotal}");
            writer.WriteLine($"(+)  Multa: R$ {dto.PenaltyFee}");
            writer.WriteLine($"(-)  Total Geral: R$ {(dto.TotalAmount + dto.AccessoriesTotal + dto.PenaltyFee)}");
            writer.WriteLine($"(-)  Total Pago: R$ {dto.AmountPaid}");
            writer.WriteLine();
            writer.WriteLine($"(=)  Saldo Devedor: R$ {dto.BalanceDue}");
            writer.WriteLine();
            writer.WriteLine("-----------------------------------------");
            writer.WriteLine();
            writer.WriteLine("Detalhamento de Pagamentos:");

            if (dto.Payments != null && dto.Payments.Any())
            {
                foreach (var pay in dto.Payments)
                {
                    var payMethod = pay.PaymentMethod switch
                    {
                        "cash" => "Dinheiro",
                        "credit_card" => "Cartão",
                        "pix" => "Pix",
                        "boleto" => "Boleto",
                        _ => "Desconhecido"
                    };

                    writer.WriteLine($"  {pay.PaymentDate:dd-MM-yyyy} - {payMethod} - {pay.Amount}");
                }
            }
            else
                writer.WriteLine("  Ainda não foram realizados pagamentos.");

            writer.Flush();
            ms.Position = 0;
            return ms.ToArray();
        }

        // MODELO CSV
        public async Task<byte[]?> ExportRentalReportCsvAsync(Guid id)
        {
            var dto = await GetRentalReportAsync(id);
            if (dto == null)
                return null;

            var sep = ";";

            using var ms = new MemoryStream();

            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            ms.Write(bom, 0, bom.Length);

            using var writer = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true);

            writer.WriteLine($"Relatório de Locação{sep}{dto.ReportNumber}{sep}Data{sep}{DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            writer.WriteLine();

            writer.WriteLine("Dados do Contrato");
            writer.WriteLine($"Cliente{sep}{sep}Início{sep}Término{sep}Status");
            writer.WriteLine($"{dto.Customer.Name}{sep}{sep}{dto.StartDate:dd/MM/yyyy}{sep}{dto.EndDate:dd/MM/yyyy}{sep}{dto.Status}");
            writer.WriteLine();

            writer.WriteLine("Dados da Locação");
            writer.WriteLine($"Veículo{sep}Total veículo{sep}acessórios{sep}Total acessórios{sep}multas");

            var accessoriesTotal = dto.AccessoriesTotal == 0 ? "-" : dto.AccessoriesTotal.ToString("C");
            var penalty = dto.PenaltyFee == 0 ? "-" : dto.PenaltyFee.ToString("C");

            writer.WriteLine($"{dto.Vehicle.Brand} - {dto.Vehicle.Model}{sep}{dto.TotalAmount}{sep}-{sep}{accessoriesTotal}{sep}{penalty}");
            writer.WriteLine();

            writer.WriteLine($"{sep}{sep}{sep}Total geral{sep}{dto.TotalAmount}");
            writer.WriteLine($"{sep}{sep}{sep}total pago{sep}{dto.AmountPaid}");
            writer.WriteLine($"{sep}{sep}{sep}saldo devedor{sep}{dto.BalanceDue}");
            writer.WriteLine();

            writer.WriteLine("Dados dos Pagamentos");
            writer.WriteLine($"Data{sep}Método{sep}Valor");

            if (dto.Payments != null && dto.Payments.Any())
            {
                foreach (var p in dto.Payments)
                    writer.WriteLine($"{p.PaymentDate:dd/MM/yyyy}{sep}{p.PaymentMethod}{sep}{p.Amount}");
            }
            else
            {
                writer.WriteLine($"Nenhum pagamento registrado");
            }

            writer.Flush();
            ms.Position = 0;
            return ms.ToArray();
        }


        public async Task<string?> SaveRentalReportToRepositoryAsync(Guid id, string format = "txt")
        {
            var fmt = (format ?? "txt").ToLowerInvariant();
            var safeFormat = fmt == "csv" ? "csv" : "txt";

            var dto = await GetRentalReportAsync(id);
            if (dto == null)
                return null;

            byte[] content = safeFormat == "csv"
                ? await ExportRentalReportCsvAsync(id) ?? []
                : await ExportRentalReportAsync(id) ?? [];

            var reportNumber = $"RPT-{DateTime.Today:yyyyMMdd}-{id.ToString()[..6].ToUpper()}";
            var safeName = SanitizeFileName(dto.Customer.Name);

            var fileName = $"{reportNumber}_{safeName}.{safeFormat}";

            var exportsDir = Path.Combine(AppContext.BaseDirectory, "reports", safeFormat);
            Directory.CreateDirectory(exportsDir);

            // Sobrescreve apenas se tiver o mesmo id
            var existing = Directory.GetFiles(exportsDir, $"*{id.ToString()[..6]}*.{safeFormat}")
                                    .FirstOrDefault();

            var filePath = existing ?? Path.Combine(exportsDir, fileName);

            await File.WriteAllBytesAsync(filePath, content);

            return filePath;
        }

        // ================= HELPERS =================
        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (value.Contains('"') || value.Contains(';') || value.Contains('\n') || value.Contains('\r'))
                return '"' + value.Replace("\"", "\"\"") + '"';

            return value;
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(name.Where(c => !invalid.Contains(c)).ToArray())
                .Replace(" ", "_");
        }
    }
}
