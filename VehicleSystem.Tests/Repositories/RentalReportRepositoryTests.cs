using System;
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
    public class RentalReportRepositoryTests
    {
        private PostgresContext _context;
        private IRentalReportRepository _repository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<PostgresContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new PostgresContext(options);
            _repository = new RentalReportRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ─── Factory ────────────────────────────────────────────────────────────

        private async Task<(TbVehicle vehicle, TbUser user, TbRental rental)> SeedRentalAsync(
            Guid rentalId,
            string customerName = "João Silva",
            string vehicleBrand = "Toyota",
            string vehicleModel = "Corolla",
            string vehiclePlate = "ABC-1234",
            string status = "completed",
            decimal totalAmount = 300m)
        {
            var vehicle = new TbVehicle
            {
                Id = Guid.NewGuid(),
                Brand = vehicleBrand,
                Model = vehicleModel,
                LicensePlate = vehiclePlate,
                Year = 2020,
                Status = "available",
                DailyRate = 100m
            };

            var user = new TbUser
            {
                Id = Guid.NewGuid(),
                Name = customerName,
                Email = "test@email.com",
                Active = true
            };

            var rental = new TbRental
            {
                Id = rentalId,
                VehicleId = vehicle.Id,
                UserId = user.Id,
                StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEndDate = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc),
                ActualEndDate = new DateTime(2024, 1, 4, 0, 0, 0, DateTimeKind.Utc),
                TotalAmount = totalAmount,
                PenaltyFee = 0m,
                DailyRate = 100m,
                Status = status
            };

            _context.TbVehicles.Add(vehicle);
            _context.TbUsers.Add(user);
            _context.TbRentals.Add(rental);
            await _context.SaveChangesAsync();

            return (vehicle, user, rental);
        }

        // ─── GetRentalWithDetailsAsync ───────────────────────────────────────────

        #region GetRentalWithDetailsAsync

        [Test]
        [Category("Unit")]
        public async Task GetRentalWithDetailsAsync_LocacaoNaoEncontrada_RetornaNull()
        {
            var result = await _repository.GetRentalWithDetailsAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Unit")]
        public async Task GetRentalWithDetailsAsync_LocacaoEncontrada_RetornaLocacaoCorreta()
        {
            var id = Guid.NewGuid();
            var otherId = Guid.NewGuid();

            await SeedRentalAsync(id, customerName: "Alice", totalAmount: 200m);
            await SeedRentalAsync(otherId, customerName: "Bob", totalAmount: 400m);

            var result = await _repository.GetRentalWithDetailsAsync(id);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.Id, Is.EqualTo(id));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task GetRentalWithDetailsAsync_LocacaoEncontrada_CarregaVeiculo()
        {
            var id = Guid.NewGuid();
            await SeedRentalAsync(id, vehicleBrand: "Honda", vehicleModel: "Civic", vehiclePlate: "XYZ-9999");

            var result = await _repository.GetRentalWithDetailsAsync(id);

            Assert.Multiple(() =>
            {
                Assert.That(result!.Vehicle, Is.Not.Null);
                Assert.That(result.Vehicle.Brand, Is.EqualTo("Honda"));
                Assert.That(result.Vehicle.Model, Is.EqualTo("Civic"));
                Assert.That(result.Vehicle.LicensePlate, Is.EqualTo("XYZ-9999"));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task GetRentalWithDetailsAsync_LocacaoEncontrada_CarregaUsuario()
        {
            var id = Guid.NewGuid();
            await SeedRentalAsync(id, customerName: "Carlos Souza");

            var result = await _repository.GetRentalWithDetailsAsync(id);

            Assert.Multiple(() =>
            {
                Assert.That(result!.User, Is.Not.Null);
                Assert.That(result.User.Name, Is.EqualTo("Carlos Souza"));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task GetRentalWithDetailsAsync_LocacaoEncontrada_CarregaPagamentos()
        {
            var id = Guid.NewGuid();
            await SeedRentalAsync(id);

            _context.TbPayments.AddRange(
                new TbPayment { Id = Guid.NewGuid(), RentalId = id, Amount = 100m, PaymentMethod = "pix", PaymentDate = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
                new TbPayment { Id = Guid.NewGuid(), RentalId = id, Amount = 200m, PaymentMethod = "cash", PaymentDate = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc) }
            );
            await _context.SaveChangesAsync();

            var result = await _repository.GetRentalWithDetailsAsync(id);

            Assert.That(result!.TbPayments, Has.Count.EqualTo(2));
        }

        [Test]
        [Category("Unit")]
        public async Task GetRentalWithDetailsAsync_SemPagamentos_RetornaColecaoVazia()
        {
            var id = Guid.NewGuid();
            await SeedRentalAsync(id);

            var result = await _repository.GetRentalWithDetailsAsync(id);

            Assert.That(result!.TbPayments, Is.Empty);
        }

        [Test]
        [Category("Unit")]
        public async Task GetRentalWithDetailsAsync_LocacaoEncontrada_CarregaAcessoriosComDetalhes()
        {
            var id = Guid.NewGuid();
            await SeedRentalAsync(id);

            var accessory = new TbAccessory { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 20m };
            _context.TbAccessories.Add(accessory);
            await _context.SaveChangesAsync();

            _context.TbRentalAccessories.Add(new TbRentalAccessory
            {
                RentalId = id,
                AccessoryId = accessory.Id,
                Quantity = 1,
                UnitPrice = 20m,
                TotalPrice = 60m
            });
            await _context.SaveChangesAsync();

            var result = await _repository.GetRentalWithDetailsAsync(id);

            Assert.Multiple(() =>
            {
                Assert.That(result!.TbRentalAccessories, Has.Count.EqualTo(1));
                Assert.That(result.TbRentalAccessories.First().Accessory, Is.Not.Null);
                Assert.That(result.TbRentalAccessories.First().Accessory!.Name, Is.EqualTo("GPS"));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task GetRentalWithDetailsAsync_SemAcessorios_RetornaColecaoVazia()
        {
            var id = Guid.NewGuid();
            await SeedRentalAsync(id);

            var result = await _repository.GetRentalWithDetailsAsync(id);

            Assert.That(result!.TbRentalAccessories, Is.Empty);
        }

        [Test]
        [Category("Unit")]
        public async Task GetRentalWithDetailsAsync_NaoRetornaOutrasLocacoes()
        {
            var id = Guid.NewGuid();
            var otherId = Guid.NewGuid();

            await SeedRentalAsync(id, totalAmount: 100m);
            await SeedRentalAsync(otherId, totalAmount: 999m);

            var result = await _repository.GetRentalWithDetailsAsync(id);

            Assert.That(result!.TotalAmount, Is.EqualTo(100m));
        }

        #endregion
    }
}
