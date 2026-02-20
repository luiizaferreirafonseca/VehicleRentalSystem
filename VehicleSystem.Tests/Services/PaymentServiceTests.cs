using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Services;

namespace VehicleSystem.Tests.Services
{
    [TestFixture]
    [Category("Business Services")]
    public class PaymentServiceTests
    {
        private Mock<IRentalRepository> _rentalRepositoryMock;
        private Mock<IPaymentRepository> _paymentRepositoryMock;
        private PaymentService _service;

        [SetUp]
        public void SetUp()
        {
            _rentalRepositoryMock = new Mock<IRentalRepository>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _service = new PaymentService(_rentalRepositoryMock.Object, _paymentRepositoryMock.Object);
        }

        #region RegisterPaymentAsync

        /// <summary>
        /// Falha: Lança ArgumentException quando o identificador de locação informado é vazio.
        /// </summary>
        [Test]
        [Category("Validation")]
        [Property("Priority", 1)]
        public void RegisterPaymentAsync_RentalIdEmpty_ThrowsArgumentException()
        {
            var dto = new PaymentCreateDTO { Amount = 100m, PaymentMethod = EnumPaymentMethod.PIX };

            var ex = Assert.ThrowsAsync<ArgumentException>(() =>
                _service.RegisterPaymentAsync(Guid.Empty, dto));

            Assert.That(ex!.Message, Does.Contain("rental identifier"));
        }

        /// <summary>
        /// Falha: Lança KeyNotFoundException quando a locação informada não é encontrada no banco de dados.
        /// </summary>
        [Test]
        [Category("Validation")]
        [Property("Priority", 1)]
        public void RegisterPaymentAsync_RentalNotFound_ThrowsKeyNotFoundException()
        {
            var rentalId = Guid.NewGuid();
            var dto = new PaymentCreateDTO { Amount = 100m, PaymentMethod = EnumPaymentMethod.PIX };

            _rentalRepositoryMock
                .Setup(r => r.GetRentalByIdAsync(rentalId))
                .ReturnsAsync((TbRental?)null);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.RegisterPaymentAsync(rentalId, dto));

            Assert.That(ex!.Message, Is.EqualTo(Messages.RentalNotFound));
            _paymentRepositoryMock.Verify(p => p.AddPaymentAsync(It.IsAny<TbPayment>()), Times.Never);
        }

        /// <summary>
        /// Falha: Lança InvalidOperationException quando a locação informada está com status cancelado.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        [Property("Priority", 2)]
        public void RegisterPaymentAsync_RentalCanceled_ThrowsInvalidOperationException()
        {
            var rentalId = Guid.NewGuid();
            var dto = new PaymentCreateDTO { Amount = 100m, PaymentMethod = EnumPaymentMethod.PIX };
            var rental = new TbRental { Id = rentalId, Status = RentalStatus.canceled.ToString(), TotalAmount = 200m };

            _rentalRepositoryMock
                .Setup(r => r.GetRentalByIdAsync(rentalId))
                .ReturnsAsync(rental);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RegisterPaymentAsync(rentalId, dto));

            Assert.That(ex!.Message, Does.Contain("canceled rental"));
            _paymentRepositoryMock.Verify(p => p.AddPaymentAsync(It.IsAny<TbPayment>()), Times.Never);
        }

        /// <summary>
        /// Falha: Lança InvalidOperationException quando o valor do pagamento é igual ou inferior a zero.
        /// </summary>
        [Test]
        [Category("Validation")]
        [Property("Priority", 2)]
        public void RegisterPaymentAsync_AmountLessOrEqualZero_ThrowsInvalidOperationException()
        {
            var rentalId = Guid.NewGuid();
            var dto = new PaymentCreateDTO { Amount = 0m, PaymentMethod = EnumPaymentMethod.PIX };
            var rental = new TbRental { Id = rentalId, Status = "active", TotalAmount = 200m };

            _rentalRepositoryMock
                .Setup(r => r.GetRentalByIdAsync(rentalId))
                .ReturnsAsync(rental);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RegisterPaymentAsync(rentalId, dto));

            Assert.That(ex!.Message, Does.Contain("greater than zero"));
            _paymentRepositoryMock.Verify(p => p.AddPaymentAsync(It.IsAny<TbPayment>()), Times.Never);
        }

        /// <summary>
        /// Falha: Lança InvalidOperationException quando o valor do pagamento supera o saldo restante da locação.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        [Property("Priority", 2)]
        public void RegisterPaymentAsync_AmountGreaterThanRemaining_ThrowsInvalidOperationException()
        {
            var rentalId = Guid.NewGuid();
            var dto = new PaymentCreateDTO { Amount = 150m, PaymentMethod = EnumPaymentMethod.PIX };
            var rental = new TbRental { Id = rentalId, Status = "active", TotalAmount = 100m };

            _rentalRepositoryMock
                .Setup(r => r.GetRentalByIdAsync(rentalId))
                .ReturnsAsync(rental);

            _paymentRepositoryMock
                .Setup(p => p.GetTotalPaymentsAsync(rentalId))
                .ReturnsAsync(20m);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RegisterPaymentAsync(rentalId, dto));

            Assert.That(ex!.Message, Does.Contain("cannot exceed the total rental amount"));
            _paymentRepositoryMock.Verify(p => p.AddPaymentAsync(It.IsAny<TbPayment>()), Times.Never);
        }

        /// <summary>
        /// Sucesso: Persiste o pagamento e retorna o DTO com todos os campos corretamente preenchidos.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task RegisterPaymentAsync_ValidData_AddsPaymentAndReturnsResponse()
        {
            var rentalId = Guid.NewGuid();
            var dto = new PaymentCreateDTO
            {
                Amount = 80m,
                PaymentMethod = EnumPaymentMethod.CREDIT_CARD
            };

            var rental = new TbRental { Id = rentalId, Status = "active", TotalAmount = 200m };

            _rentalRepositoryMock
                .Setup(r => r.GetRentalByIdAsync(rentalId))
                .ReturnsAsync(rental);

            _paymentRepositoryMock
                .Setup(p => p.GetTotalPaymentsAsync(rentalId))
                .ReturnsAsync(50m);

            TbPayment? savedPayment = null;
            _paymentRepositoryMock
                .Setup(p => p.AddPaymentAsync(It.IsAny<TbPayment>()))
                .Callback<TbPayment>(p => savedPayment = p)
                .Returns(Task.CompletedTask);

            _paymentRepositoryMock
                .Setup(p => p.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var result = await _service.RegisterPaymentAsync(rentalId, dto);

            Assert.Multiple(() =>
            {
                Assert.That(savedPayment, Is.Not.Null);
                Assert.That(savedPayment!.RentalId, Is.EqualTo(rentalId));
                Assert.That(savedPayment.Amount, Is.EqualTo(dto.Amount));
                Assert.That(savedPayment.PaymentMethod, Is.EqualTo(EnumPaymentMethod.CREDIT_CARD.ToString().ToLower()));

                Assert.That(result.Id, Is.EqualTo(savedPayment.Id));
                Assert.That(result.RentalId, Is.EqualTo(savedPayment.RentalId));
                Assert.That(result.Amount, Is.EqualTo(savedPayment.Amount));
                Assert.That(result.PaymentMethod, Is.EqualTo(savedPayment.PaymentMethod));
                Assert.That(result.PaymentDate, Is.EqualTo(savedPayment.PaymentDate));
            });

            _paymentRepositoryMock.Verify(p => p.AddPaymentAsync(It.IsAny<TbPayment>()), Times.Once);
            _paymentRepositoryMock.Verify(p => p.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region GetAllPaymentsAsync

        /// <summary>
        /// Falha: Retorna enumerável vazio quando o repositório retorna null.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetAllPaymentsAsync_RepositoryReturnsNull_ReturnsEmptyEnumerable()
        {
            _paymentRepositoryMock
                .Setup(p => p.GetAllPaymentsAsync(null, null, null, null))
                .ReturnsAsync((IEnumerable<TbPayment>?)null!);

            var result = await _service.GetAllPaymentsAsync();

            Assert.That(result, Is.Empty);
        }

        /// <summary>
        /// Sucesso: Mapeia corretamente os pagamentos para DTOs e verifica a chamada ao repositório.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetAllPaymentsAsync_WithPayments_MapsToDtos()
        {
            var rentalId = Guid.NewGuid();
            var payments = new List<TbPayment>
            {
                new TbPayment
                {
                    Id = Guid.NewGuid(),
                    RentalId = rentalId,
                    Amount = 100m,
                    PaymentMethod = "PIX",
                    PaymentDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
                }
            };

            _paymentRepositoryMock
                .Setup(p => p.GetAllPaymentsAsync(rentalId, "PIX", null, null))
                .ReturnsAsync(payments);

            var result = await _service.GetAllPaymentsAsync(rentalId, "PIX", null, null);
            var list = result.ToList();

            Assert.Multiple(() =>
            {
                Assert.That(list.Count, Is.EqualTo(1));
                Assert.That(list[0].Id, Is.EqualTo(payments[0].Id));
                Assert.That(list[0].RentalId, Is.EqualTo(payments[0].RentalId));
                Assert.That(list[0].Amount, Is.EqualTo(payments[0].Amount));
                Assert.That(list[0].PaymentMethod, Is.EqualTo(payments[0].PaymentMethod));
                Assert.That(list[0].PaymentDate, Is.EqualTo(payments[0].PaymentDate));
            });

            _paymentRepositoryMock.Verify(
                p => p.GetAllPaymentsAsync(rentalId, "PIX", null, null),
                Times.Once);
        }

        #endregion
    }
}
