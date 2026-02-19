using System;
using System.Collections.Generic;
using NUnit.Framework;
using VehicleRentalSystem.DTO;

namespace VehicleSystem.Tests.DTOs
{
    [TestFixture]
    [Category("DTOs")]
    public class RentalReportDtoTests
    {
        #region Defaults

        [Test]
        public void RentalReportResponseDTO_Defaults_AreExpected()
        {
            var dto = new RentalReportResponseDTO();

            Assert.Multiple(() =>
            {
                Assert.That(dto.RentalId, Is.EqualTo(Guid.Empty));
                Assert.That(dto.StartDate, Is.EqualTo(default(DateTime)));
                Assert.That(dto.EndDate, Is.EqualTo(default(DateTime)));
                Assert.That(dto.TotalAmount, Is.EqualTo(0m));
                Assert.That(dto.PenaltyFee, Is.EqualTo(0m));
                Assert.That(dto.Payments, Is.Empty);
                Assert.That(dto.Accessories, Is.Empty);
            });
        }

        #endregion

        #region AmountPaid

        [Test]
        public void AmountPaid_SemPagamentos_RetornaZero()
        {
            var dto = new RentalReportResponseDTO();

            Assert.That(dto.AmountPaid, Is.EqualTo(0m));
        }

        [Test]
        public void AmountPaid_ComUmPagamento_RetornaSomaDoPagamento()
        {
            var dto = new RentalReportResponseDTO
            {
                Payments = new List<PaymentResponseDto>
                {
                    new PaymentResponseDto { Amount = 150m }
                }
            };

            Assert.That(dto.AmountPaid, Is.EqualTo(150m));
        }

        [Test]
        public void AmountPaid_ComMultiplosPagamentos_RetornaSomaCorreta()
        {
            var dto = new RentalReportResponseDTO
            {
                Payments = new List<PaymentResponseDto>
                {
                    new PaymentResponseDto { Amount = 100m },
                    new PaymentResponseDto { Amount = 50m },
                    new PaymentResponseDto { Amount = 75m }
                }
            };

            Assert.That(dto.AmountPaid, Is.EqualTo(225m));
        }

        #endregion

        #region AccessoriesTotal

        [Test]
        public void AccessoriesTotal_SemAcessorios_RetornaZero()
        {
            var dto = new RentalReportResponseDTO();

            Assert.That(dto.AccessoriesTotal, Is.EqualTo(0m));
        }

        [Test]
        public void AccessoriesTotal_ComAcessorios_RetornaSomaDeTotalPrice()
        {
            var dto = new RentalReportResponseDTO
            {
                Accessories = new List<AccessoryReportDto>
                {
                    new AccessoryReportDto { TotalPrice = 40m },
                    new AccessoryReportDto { TotalPrice = 60m }
                }
            };

            Assert.That(dto.AccessoriesTotal, Is.EqualTo(100m));
        }

        #endregion

        #region BalanceDue

        [Test]
        public void BalanceDue_SemPagamentosNemAcessorios_RetornaTotalAmountMaisPenaltyFee()
        {
            var dto = new RentalReportResponseDTO
            {
                TotalAmount = 300m,
                PenaltyFee = 50m
            };

            Assert.That(dto.BalanceDue, Is.EqualTo(350m));
        }

        [Test]
        public void BalanceDue_PagamentoParcial_RetornaSaldoRestante()
        {
            var dto = new RentalReportResponseDTO
            {
                TotalAmount = 300m,
                PenaltyFee = 0m,
                Payments = new List<PaymentResponseDto>
                {
                    new PaymentResponseDto { Amount = 100m }
                }
            };

            Assert.That(dto.BalanceDue, Is.EqualTo(200m));
        }

        [Test]
        public void BalanceDue_PagamentoTotal_RetornaZero()
        {
            var dto = new RentalReportResponseDTO
            {
                TotalAmount = 300m,
                PenaltyFee = 0m,
                Payments = new List<PaymentResponseDto>
                {
                    new PaymentResponseDto { Amount = 200m },
                    new PaymentResponseDto { Amount = 100m }
                }
            };

            Assert.That(dto.BalanceDue, Is.EqualTo(0m));
        }

        [Test]
        public void BalanceDue_ComAcessoriosPenaltyEPagamento_RetornaCalculoCompleto()
        {
            var dto = new RentalReportResponseDTO
            {
                TotalAmount = 200m,
                PenaltyFee = 30m,
                Accessories = new List<AccessoryReportDto>
                {
                    new AccessoryReportDto { TotalPrice = 70m }
                },
                Payments = new List<PaymentResponseDto>
                {
                    new PaymentResponseDto { Amount = 150m }
                }
            };

            // (200 + 30 + 70) - 150 = 150
            Assert.That(dto.BalanceDue, Is.EqualTo(150m));
        }

        #endregion

        #region ReportNumber

        [Test]
        public void ReportNumber_FormatoCorreto_ComecaComRPT()
        {
            var dto = new RentalReportResponseDTO { RentalId = Guid.NewGuid() };

            Assert.That(dto.ReportNumber, Does.StartWith("RPT-"));
        }

        [Test]
        public void ReportNumber_ContemDataDeHoje()
        {
            var dto = new RentalReportResponseDTO { RentalId = Guid.NewGuid() };
            var expectedDate = DateTime.Today.ToString("yyyyMMdd");

            Assert.That(dto.ReportNumber, Does.Contain(expectedDate));
        }

        [Test]
        public void ReportNumber_ContemPrimeiros6CaracteresDoRentalIdEmMaiusculo()
        {
            var rentalId = Guid.NewGuid();
            var dto = new RentalReportResponseDTO { RentalId = rentalId };
            var expectedSuffix = rentalId.ToString()[..6].ToUpper();

            Assert.That(dto.ReportNumber, Does.EndWith(expectedSuffix));
        }

        [Test]
        public void ReportNumber_FormatoCompleto_RPT_Data_Sufixo()
        {
            var rentalId = Guid.NewGuid();
            var dto = new RentalReportResponseDTO { RentalId = rentalId };
            var expected = $"RPT-{DateTime.Today:yyyyMMdd}-{rentalId.ToString()[..6].ToUpper()}";

            Assert.That(dto.ReportNumber, Is.EqualTo(expected));
        }

        #endregion

        #region VehicleSummaryDTO

        [Test]
        public void VehicleSummaryDTO_PropriedadesSetadasCorretamente()
        {
            var vehicle = new VehicleSummaryDTO("Toyota", "Corolla", "ABC-1234");

            Assert.Multiple(() =>
            {
                Assert.That(vehicle.Brand, Is.EqualTo("Toyota"));
                Assert.That(vehicle.Model, Is.EqualTo("Corolla"));
                Assert.That(vehicle.LicensePlate, Is.EqualTo("ABC-1234"));
            });
        }

        #endregion

        #region CustomerSummaryDTO

        [Test]
        public void CustomerSummaryDTO_PropriedadesSetadasCorretamente()
        {
            var customer = new CustomerSummaryDTO("João Silva", "123.456.789-00");

            Assert.Multiple(() =>
            {
                Assert.That(customer.Name, Is.EqualTo("João Silva"));
                Assert.That(customer.Document, Is.EqualTo("123.456.789-00"));
            });
        }

        #endregion
    }
}
