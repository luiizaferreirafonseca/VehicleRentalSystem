using System;
using System.Collections.Generic;
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
    public class VehicleRepositoryTests
    {
        private PostgresContext _context = null!;
        private IVehicleRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<PostgresContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new PostgresContext(options);
            _repository = new VehicleRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ─── Factory ─────────────────────────────────────────────────────────────

        private static TbVehicle BuildVehicle(
            string brand = "Toyota",
            string model = "Corolla",
            string status = "available",
            string licensePlate = "ABC-1234",
            int year = 2022,
            decimal dailyRate = 150m)
        {
            return new TbVehicle
            {
                Id = Guid.NewGuid(),
                Brand = brand,
                Model = model,
                Year = year,
                DailyRate = dailyRate,
                Status = status,
                LicensePlate = licensePlate
            };
        }

        // ─── SearchVehiclesAsync ──────────────────────────────────────────────────

        #region SearchVehiclesAsync

        [Test]
        [Category("Unit")]
        public async Task SearchVehiclesAsync_StatusAvailable_RetornaApenasVeiculosDisponiveis()
        {
            _context.TbVehicles.AddRange(
                BuildVehicle(status: "available", licensePlate: "AAA-0001"),
                BuildVehicle(status: "available", licensePlate: "AAA-0002"),
                BuildVehicle(status: "rented",    licensePlate: "BBB-0003"),
                BuildVehicle(status: "maintenance", licensePlate: "CCC-0004")
            );
            await _context.SaveChangesAsync();

            var result = await _repository.SearchVehiclesAsync("available", 1);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result, Has.All.Property("Status").EqualTo("available"));
        }

        [Test]
        [Category("Unit")]
        public async Task SearchVehiclesAsync_StatusNull_RetornaTodosVeiculos()
        {
            _context.TbVehicles.AddRange(
                BuildVehicle(status: "available",   licensePlate: "AAA-0001"),
                BuildVehicle(status: "rented",      licensePlate: "BBB-0002"),
                BuildVehicle(status: "maintenance", licensePlate: "CCC-0003")
            );
            await _context.SaveChangesAsync();

            var result = await _repository.SearchVehiclesAsync(null, 1);

            Assert.That(result, Has.Count.EqualTo(3));
        }

        [Test]
        [Category("Unit")]
        public async Task SearchVehiclesAsync_StatusVazio_RetornaTodosVeiculos()
        {
            _context.TbVehicles.AddRange(
                BuildVehicle(status: "available", licensePlate: "AAA-0001"),
                BuildVehicle(status: "rented",    licensePlate: "BBB-0002")
            );
            await _context.SaveChangesAsync();

            var result = await _repository.SearchVehiclesAsync("   ", 1);

            Assert.That(result, Has.Count.EqualTo(2));
        }

        [Test]
        [Category("Unit")]
        public async Task SearchVehiclesAsync_NenhumVeiculoComStatus_RetornaListaVazia()
        {
            _context.TbVehicles.Add(BuildVehicle(status: "rented", licensePlate: "AAA-0001"));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchVehiclesAsync("available", 1);

            Assert.That(result, Is.Empty);
        }

        [Test]
        [Category("Unit")]
        public async Task SearchVehiclesAsync_FiltroStatusCaseInsensitivo_RetornaVeiculos()
        {
            _context.TbVehicles.Add(BuildVehicle(status: "available", licensePlate: "AAA-0001"));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchVehiclesAsync("AVAILABLE", 1);

            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        [Category("Unit")]
        public async Task SearchVehiclesAsync_ResultadosOrdenadosPorMarca()
        {
            _context.TbVehicles.AddRange(
                BuildVehicle(brand: "Toyota",    status: "available", licensePlate: "AAA-0001"),
                BuildVehicle(brand: "Audi",      status: "available", licensePlate: "BBB-0002"),
                BuildVehicle(brand: "Chevrolet", status: "available", licensePlate: "CCC-0003")
            );
            await _context.SaveChangesAsync();

            var result = await _repository.SearchVehiclesAsync("available", 1);

            Assert.Multiple(() =>
            {
                Assert.That(result[0].Brand, Is.EqualTo("Audi"));
                Assert.That(result[1].Brand, Is.EqualTo("Chevrolet"));
                Assert.That(result[2].Brand, Is.EqualTo("Toyota"));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task SearchVehiclesAsync_PaginaMenorQueUm_UsaPaginaUm()
        {
            _context.TbVehicles.AddRange(
                BuildVehicle(status: "available", licensePlate: "AAA-0001"),
                BuildVehicle(status: "available", licensePlate: "BBB-0002")
            );
            await _context.SaveChangesAsync();

            var resultPageZero = await _repository.SearchVehiclesAsync("available", 0);
            var resultPageOne  = await _repository.SearchVehiclesAsync("available", 1);

            Assert.That(resultPageZero, Is.EqualTo(resultPageOne).Using<List<TbVehicle>>(
                (a, b) => a.Count == b.Count ? 0 : -1));
        }

        [Test]
        [Category("Unit")]
        public async Task SearchVehiclesAsync_Paginacao_RetornaSegundaPaginaCorretamente()
        {
            for (var i = 1; i <= 6; i++)
                _context.TbVehicles.Add(BuildVehicle(
                    brand: $"Marca{i:D2}",
                    status: "available",
                    licensePlate: $"XYZ-{i:D4}"));

            await _context.SaveChangesAsync();

            var page1 = await _repository.SearchVehiclesAsync("available", 1);
            var page2 = await _repository.SearchVehiclesAsync("available", 2);

            Assert.Multiple(() =>
            {
                Assert.That(page1, Has.Count.EqualTo(5));
                Assert.That(page2, Has.Count.EqualTo(1));
            });
        }

        #endregion
    }
}
