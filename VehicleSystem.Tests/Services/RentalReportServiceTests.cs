using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services;

namespace VehicleSystem.Tests.Services
{
    [TestFixture]
    [Category("Business Services")]
    public class RentalReportServiceTests
    {
        private Mock<IRentalReportRepository> _repositoryMock;
        private RentalReportService _service;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRentalReportRepository>();
            _service = new RentalReportService(_repositoryMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            var reportsDir = Path.Combine(AppContext.BaseDirectory, "reports");
            if (Directory.Exists(reportsDir))
                Directory.Delete(reportsDir, recursive: true);
        }

        private static TbRental BuildRental(
            Guid? id = null,
            string customerName = "João Silva",
            string vehicleBrand = "Toyota",
            string vehicleModel = "Corolla",
            string vehiclePlate = "ABC-1234",
            string status = "completed",
            decimal totalAmount = 300m,
            decimal penaltyFee = 0m,
            List<TbPayment>? payments = null,
            List<TbRentalAccessory>? accessories = null)
        {
            var rentalId = id ?? Guid.NewGuid();
            var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc);

            var rental = new TbRental
            {
                Id = rentalId,
                StartDate = start,
                ExpectedEndDate = end,
                ActualEndDate = end,
                TotalAmount = totalAmount,
                PenaltyFee = penaltyFee,
                Status = status,
                User = new TbUser { Id = Guid.NewGuid(), Name = customerName, Email = "teste@email.com" },
                Vehicle = new TbVehicle
                {
                    Id = Guid.NewGuid(),
                    Brand = vehicleBrand,
                    Model = vehicleModel,
                    LicensePlate = vehiclePlate,
                    Status = "available",
                    DailyRate = 100m
                }
            };

            if (payments != null)
                foreach (var p in payments)
                    rental.TbPayments.Add(p);

            if (accessories != null)
                foreach (var a in accessories)
                    rental.TbRentalAccessories.Add(a);

            return rental;
        }

        #region GetRentalReportAsync

        [Test]
        [Category("Unit")]
        public async Task GetRentalReportAsync_LocacaoNaoEncontrada_RetornaNull()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id)).ReturnsAsync((TbRental?)null);

            var result = await _service.GetRentalReportAsync(id);

            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Unit")]
        public async Task GetRentalReportAsync_LocacaoEncontrada_RetornaDtoMapeadoCorretamente()
        {
            var id = Guid.NewGuid();
            var rental = BuildRental(id: id, customerName: "Maria", vehicleBrand: "Honda",
                vehicleModel: "Civic", vehiclePlate: "XYZ-9999", totalAmount: 500m, penaltyFee: 20m);

            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id)).ReturnsAsync(rental);

            var result = await _service.GetRentalReportAsync(id);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.RentalId, Is.EqualTo(id));
                Assert.That(result.Customer.Name, Is.EqualTo("Maria"));
                Assert.That(result.Vehicle.Brand, Is.EqualTo("Honda"));
                Assert.That(result.Vehicle.Model, Is.EqualTo("Civic"));
                Assert.That(result.Vehicle.LicensePlate, Is.EqualTo("XYZ-9999"));
                Assert.That(result.TotalAmount, Is.EqualTo(500m));
                Assert.That(result.PenaltyFee, Is.EqualTo(20m));
                Assert.That(result.Status, Is.EqualTo("completed"));
            });
        }

        #endregion

        #region ExportRentalReportAsync

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportAsync_LocacaoNaoEncontrada_RetornaNull()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id)).ReturnsAsync((TbRental?)null);

            var result = await _service.ExportRentalReportAsync(id);

            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportAsync_LocacaoValida_RetornaBytesNaoVazios()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id)).ReturnsAsync(BuildRental(id: id));

            var result = await _service.ExportRentalReportAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Length, Is.GreaterThan(0));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportAsync_ConteudoContemDadosDaLocacao()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, customerName: "Carlos", vehicleBrand: "Ford",
                    vehiclePlate: "DEF-5678", status: "completed"));

            var bytes = await _service.ExportRentalReportAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("Carlos"));
                Assert.That(content, Does.Contain("Ford"));
                Assert.That(content, Does.Contain("DEF-5678"));
                Assert.That(content, Does.Contain("completed"));
                Assert.That(content, Does.Contain("RPT-"));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportAsync_SemAcessorios_ContemMensagemVazia()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, accessories: new List<TbRentalAccessory>()));

            var bytes = await _service.ExportRentalReportAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain("Não há acessórios nessa locação"));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportAsync_ComAcessorios_ListaAcessorios()
        {
            var id = Guid.NewGuid();
            var accessory = new TbRentalAccessory
            {
                AccessoryId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = 20m,
                TotalPrice = 60m,
                Accessory = new TbAccessory { Name = "GPS", DailyRate = 20m }
            };

            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, accessories: new List<TbRentalAccessory> { accessory }));

            var bytes = await _service.ExportRentalReportAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain("GPS"));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportAsync_SemPagamentos_ContemMensagemSemPagamento()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, payments: new List<TbPayment>()));

            var bytes = await _service.ExportRentalReportAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain("Ainda não foram realizados pagamentos"));
        }

        [TestCase("cash", "Dinheiro")]
        [TestCase("credit_card", "Cartão")]
        [TestCase("pix", "Pix")]
        [TestCase("boleto", "Boleto")]
        [TestCase("outro", "Desconhecido")]
        [Category("Unit")]
        public async Task ExportRentalReportAsync_MetodoPagamento_MapeadoCorretamente(string method, string expected)
        {
            var id = Guid.NewGuid();
            var payment = new TbPayment
            {
                Id = Guid.NewGuid(),
                RentalId = id,
                Amount = 100m,
                PaymentMethod = method,
                PaymentDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            };

            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, payments: new List<TbPayment> { payment }));

            var bytes = await _service.ExportRentalReportAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain(expected));
        }

        #endregion

        #region ExportRentalReportCsvAsync

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_LocacaoNaoEncontrada_RetornaNull()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id)).ReturnsAsync((TbRental?)null);

            var result = await _service.ExportRentalReportCsvAsync(id);

            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_LocacaoValida_RetornaBytesNaoVazios()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id)).ReturnsAsync(BuildRental(id: id));

            var result = await _service.ExportRentalReportCsvAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Length, Is.GreaterThan(0));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_ConteudoIniciaComBOM()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id)).ReturnsAsync(BuildRental(id: id));

            var bytes = await _service.ExportRentalReportCsvAsync(id);

            Assert.Multiple(() =>
            {
                Assert.That(bytes![0], Is.EqualTo(0xEF));
                Assert.That(bytes[1], Is.EqualTo(0xBB));
                Assert.That(bytes[2], Is.EqualTo(0xBF));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_ConteudoUsaSeparadorPontoVirgula()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id)).ReturnsAsync(BuildRental(id: id));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain(";"));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_ContemDadosDaLocacao()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, customerName: "Ana", vehicleBrand: "Chevrolet",
                    vehicleModel: "Onix"));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("Ana"));
                Assert.That(content, Does.Contain("Chevrolet"));
                Assert.That(content, Does.Contain("Onix"));
                Assert.That(content, Does.Contain("RPT-"));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_SemPagamentos_ContemMensagemNenhumPagamento()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, payments: new List<TbPayment>()));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain("Nenhum pagamento registrado"));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_ComPagamento_ListaPagamentosNoConteudo()
        {
            var id = Guid.NewGuid();
            var payment = new TbPayment
            {
                Id = Guid.NewGuid(),
                RentalId = id,
                Amount = 150m,
                PaymentMethod = "pix",
                PaymentDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            };

            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, payments: new List<TbPayment> { payment }));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("pix"));
                Assert.That(content, Does.Contain("150"));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_NomeComPontoVirgula_EscapaComAspas()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, customerName: "Silva; Ferreira"));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain("\"Silva; Ferreira\""));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_NomeComAspas_EscapaEDuplicaAspas()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, customerName: "João \"Zé\" Silva"));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain("\"João \"\"Zé\"\" Silva\""));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_NomeComNovaLinha_EscapaComAspas()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, customerName: "Nome\nSobrenome"));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain("\"Nome\nSobrenome\""));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_NomeBranco_RetornaCampoVazio()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, customerName: "   "));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Not.Contain("   ;"));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_ComMulta_ExibeValorDaMulta()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, penaltyFee: 50m));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain("50"));
        }

        [Test]
        [Category("Unit")]
        public async Task ExportRentalReportCsvAsync_ComAcessorios_ExibeTotalDeAcessorios()
        {
            var id = Guid.NewGuid();
            var accessory = new TbRentalAccessory
            {
                AccessoryId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = 77m,
                TotalPrice = 77m,
                Accessory = new TbAccessory { Name = "Cadeirinha", DailyRate = 77m }
            };

            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id, accessories: new List<TbRentalAccessory> { accessory }));

            var bytes = await _service.ExportRentalReportCsvAsync(id);
            var content = Encoding.UTF8.GetString(bytes!);

            Assert.That(content, Does.Contain("77"));
        }

        #endregion

        #region SaveRentalReportToRepositoryAsync

        [Test]
        [Category("Unit")]
        public async Task SaveRentalReportToRepositoryAsync_LocacaoNaoEncontrada_RetornaNull()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id)).ReturnsAsync((TbRental?)null);

            var result = await _service.SaveRentalReportToRepositoryAsync(id);

            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Unit")]
        public async Task SaveRentalReportToRepositoryAsync_FormatoTxt_SalvaArquivoERetornaCaminho()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id));

            var result = await _service.SaveRentalReportToRepositoryAsync(id, "txt");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(File.Exists(result!), Is.True);
                Assert.That(result, Does.EndWith(".txt"));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task SaveRentalReportToRepositoryAsync_FormatoCsv_SalvaArquivoERetornaCaminho()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id));

            var result = await _service.SaveRentalReportToRepositoryAsync(id, "csv");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(File.Exists(result!), Is.True);
                Assert.That(result, Does.EndWith(".csv"));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task SaveRentalReportToRepositoryAsync_FormatoInvalido_PadraoTxt()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id));

            var result = await _service.SaveRentalReportToRepositoryAsync(id, "pdf");

            Assert.That(result, Does.EndWith(".txt"));
        }

        [Test]
        [Category("Unit")]
        public async Task SaveRentalReportToRepositoryAsync_FormatoNull_UsaPadraoTxt()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id));

            var result = await _service.SaveRentalReportToRepositoryAsync(id, null);

            Assert.That(result, Does.EndWith(".txt"));
        }

        [Test]
        [Category("Unit")]
        public async Task SaveRentalReportToRepositoryAsync_CaminhoContemIdentificadorDaLocacao()
        {
            var id = Guid.NewGuid();
            var expectedSuffix = id.ToString()[..6].ToUpper();

            _repositoryMock.Setup(r => r.GetRentalWithDetailsAsync(id))
                .ReturnsAsync(BuildRental(id: id));

            var result = await _service.SaveRentalReportToRepositoryAsync(id, "txt");

            Assert.That(result, Does.Contain(expectedSuffix));
        }

        #endregion
    }
}
