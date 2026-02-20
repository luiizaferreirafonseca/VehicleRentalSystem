using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleSystem.Tests.Repositories
{
    [TestFixture]
    [Category("Repositories")]
    public class PaymentRepositoryTests
    {
        private PostgresContext _context;
        private IPaymentRepository _repository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<PostgresContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new PostgresContext(options);
            _repository = new PaymentRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        /// <summary>
        /// Sucesso: Persiste o pagamento no banco de dados após a adição.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task AddPaymentAsync_PersistPayment()
        {
            var payment = new TbPayment
            {
                Id = Guid.NewGuid(),
                RentalId = Guid.NewGuid(),
                Amount = 100m,
                PaymentMethod = "pix",
                PaymentDate = DateTime.UtcNow
            };

            await _repository.AddPaymentAsync(payment);
            await _repository.SaveChangesAsync();

            var dbPayment = await _context.TbPayments.FirstOrDefaultAsync(p => p.Id == payment.Id);

            Assert.That(dbPayment, Is.Not.Null);
            Assert.That(dbPayment!.Amount, Is.EqualTo(payment.Amount));
            Assert.That(dbPayment.PaymentMethod, Is.EqualTo(payment.PaymentMethod));
        }

        /// <summary>
        /// Sucesso: Retorna a soma correta dos pagamentos registrados para a locação informada.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetTotalPaymentsAsync_ReturnsSumForRental()
        {
            var rentalId = Guid.NewGuid();

            _context.TbPayments.AddRange(
                new TbPayment { Id = Guid.NewGuid(), RentalId = rentalId, Amount = 50m, PaymentMethod = "pix", PaymentDate = DateTime.UtcNow },
                new TbPayment { Id = Guid.NewGuid(), RentalId = rentalId, Amount = 70m, PaymentMethod = "pix", PaymentDate = DateTime.UtcNow },
                new TbPayment { Id = Guid.NewGuid(), RentalId = Guid.NewGuid(), Amount = 999m, PaymentMethod = "credit_card", PaymentDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var total = await _repository.GetTotalPaymentsAsync(rentalId);

            Assert.That(total, Is.EqualTo(120m));
        }

        /// <summary>
        /// Sucesso: Retorna apenas os pagamentos da locação informada, ignorando os de outras locações.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetPaymentsByRentalIdAsync_FiltersByRental()
        {
            var rentalId = Guid.NewGuid();
            var otherRentalId = Guid.NewGuid();

            _context.TbPayments.AddRange(
                new TbPayment { Id = Guid.NewGuid(), RentalId = rentalId, Amount = 10m, PaymentMethod = "pix", PaymentDate = DateTime.UtcNow },
                new TbPayment { Id = Guid.NewGuid(), RentalId = rentalId, Amount = 20m, PaymentMethod = "pix", PaymentDate = DateTime.UtcNow },
                new TbPayment { Id = Guid.NewGuid(), RentalId = otherRentalId, Amount = 30m, PaymentMethod = "credit_card", PaymentDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetPaymentsByRentalIdAsync(rentalId);
            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list.All(p => p.RentalId == rentalId), Is.True);
        }

        /// <summary>
        /// Sucesso: Retorna todos os pagamentos quando nenhum filtro é informado.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetAllPaymentsAsync_NoFilters_ReturnsAll()
        {
            _context.TbPayments.AddRange(
                new TbPayment { Id = Guid.NewGuid(), RentalId = Guid.NewGuid(), Amount = 10m, PaymentMethod = "pix", PaymentDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new TbPayment { Id = Guid.NewGuid(), RentalId = Guid.NewGuid(), Amount = 20m, PaymentMethod = "credit_card", PaymentDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllPaymentsAsync(null, null, null, null);

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        /// <summary>
        /// Sucesso: Filtra corretamente os pagamentos quando locação, método e intervalo de datas são informados.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetAllPaymentsAsync_WithFilters_FiltersCorrectly()
        {
            var rentalId = Guid.NewGuid();

            _context.TbPayments.AddRange(
                new TbPayment { Id = Guid.NewGuid(), RentalId = rentalId, Amount = 10m, PaymentMethod = "pix", PaymentDate = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc) },
                new TbPayment { Id = Guid.NewGuid(), RentalId = rentalId, Amount = 20m, PaymentMethod = "credit_card", PaymentDate = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                new TbPayment { Id = Guid.NewGuid(), RentalId = Guid.NewGuid(), Amount = 30m, PaymentMethod = "pix", PaymentDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc) }
            );
            await _context.SaveChangesAsync();

            var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2024, 1, 12, 0, 0, 0, DateTimeKind.Utc);

            var result = await _repository.GetAllPaymentsAsync(rentalId, "pix", start, end);
            var list = result.ToList();

            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].RentalId, Is.EqualTo(rentalId));
            Assert.That(list[0].PaymentMethod, Is.EqualTo("pix"));
            Assert.That(list[0].PaymentDate, Is.GreaterThanOrEqualTo(start));
            Assert.That(list[0].PaymentDate, Is.LessThanOrEqualTo(end));
        }
    }
}
