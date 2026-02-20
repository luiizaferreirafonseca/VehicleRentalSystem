using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories;

namespace VehicleSystem.Tests.Repositories
{
    // ---------------------------------------------------------------------------
    // Tests using InMemory database (integration-style, covers real query behavior)
    // ---------------------------------------------------------------------------
    [TestFixture]
    [Category("Repositories")]
    public class RentalRepositoryTests
    {
        private PostgresContext _context = null!;
        private RentalRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<PostgresContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new PostgresContext(options);
            _repository = new RentalRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static TbUser CreateUser(string name = "Test User", string email = "test@test.com")
            => new TbUser { Id = Guid.NewGuid(), Name = name, Email = email, Active = true };

        private static TbVehicle CreateVehicle(string model = "Civic", string status = "available")
            => new TbVehicle
            {
                Id = Guid.NewGuid(),
                Model = model,
                Brand = "Honda",
                Year = 2022,
                DailyRate = 150m,
                Status = status,
                LicensePlate = Guid.NewGuid().ToString("N")[..12].ToUpper()
            };

        private static TbRental CreateRental(Guid userId, Guid vehicleId, string status = "active",
            DateTime? startDate = null)
            => new TbRental
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VehicleId = vehicleId,
                StartDate = startDate ?? DateTime.UtcNow,
                ExpectedEndDate = (startDate ?? DateTime.UtcNow).AddDays(3),
                DailyRate = 150m,
                Status = status
            };

        // -----------------------------------------------------------------------
        // GetRentalsAsync
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Retorna todas as locações com as navegações de User e Vehicle preenchidas quando existem registros.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetRentalsAsync_WithRentals_ReturnsAllRentalsWithIncludes()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            _context.TbRentals.Add(CreateRental(user.Id, vehicle.Id));
            await _context.SaveChangesAsync();

            var result = await _repository.GetRentalsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].User, Is.Not.Null);
            Assert.That(result[0].Vehicle, Is.Not.Null);
        }

        /// <summary>
        /// Sucesso: Retorna lista vazia quando não há locações cadastradas no banco de dados.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetRentalsAsync_EmptyDatabase_ReturnsEmptyList()
        {
            var result = await _repository.GetRentalsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        /// <summary>
        /// Sucesso: Retorna todos os registros corretamente quando existem múltiplas locações no banco.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetRentalsAsync_MultipleRentals_ReturnsAllRecords()
        {
            var user = CreateUser();
            var vehicle1 = CreateVehicle("Civic");
            var vehicle2 = CreateVehicle("Corolla");
            _context.TbUsers.Add(user);
            _context.TbVehicles.AddRange(vehicle1, vehicle2);
            _context.TbRentals.AddRange(
                CreateRental(user.Id, vehicle1.Id),
                CreateRental(user.Id, vehicle2.Id));
            await _context.SaveChangesAsync();

            var result = await _repository.GetRentalsAsync();

            Assert.That(result.Count, Is.EqualTo(2));
        }

        // -----------------------------------------------------------------------
        // GetUserById
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Retorna o usuário correto quando o ID informado existe no banco de dados.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetUserById_ExistingId_ReturnsUser()
        {
            var user = CreateUser("João Silva", "joao@test.com");
            _context.TbUsers.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.GetUserById(user.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(user.Id));
            Assert.That(result.Name, Is.EqualTo("João Silva"));
        }

        /// <summary>
        /// Falha: Retorna null quando o ID informado não corresponde a nenhum usuário cadastrado.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetUserById_NonExistingId_ReturnsNull()
        {
            var result = await _repository.GetUserById(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        // -----------------------------------------------------------------------
        // GetVehicleById
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Retorna o veículo correto quando o ID informado existe no banco de dados.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetVehicleById_ExistingId_ReturnsVehicle()
        {
            var vehicle = CreateVehicle("Corolla");
            _context.TbVehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var result = await _repository.GetVehicleById(vehicle.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(vehicle.Id));
            Assert.That(result.Model, Is.EqualTo("Corolla"));
        }

        /// <summary>
        /// Falha: Retorna null quando o ID informado não corresponde a nenhum veículo cadastrado.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetVehicleById_NonExistingId_ReturnsNull()
        {
            var result = await _repository.GetVehicleById(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        // -----------------------------------------------------------------------
        // CreateRentalAsync
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Persiste a nova locação no banco de dados após a criação.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task CreateRentalAsync_ValidRental_PersistsInDatabase()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var rental = CreateRental(user.Id, vehicle.Id);

            await _repository.CreateRentalAsync(rental);

            var persisted = await _context.TbRentals.FindAsync(rental.Id);
            Assert.That(persisted, Is.Not.Null);
        }

        /// <summary>
        /// Sucesso: Retorna a locação criada com todos os campos corretamente preenchidos.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task CreateRentalAsync_ValidRental_ReturnsRentalWithCorrectFields()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var rental = CreateRental(user.Id, vehicle.Id, "active");

            var result = await _repository.CreateRentalAsync(rental);

            Assert.That(result.Id, Is.EqualTo(rental.Id));
            Assert.That(result.UserId, Is.EqualTo(user.Id));
            Assert.That(result.VehicleId, Is.EqualTo(vehicle.Id));
            Assert.That(result.Status, Is.EqualTo("active"));
            Assert.That(result.DailyRate, Is.EqualTo(150m));
        }

        // -----------------------------------------------------------------------
        // UpdateVehicleStatusAsync
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Atualiza o status do veículo e retorna true quando o veículo existe no banco de dados.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task UpdateVehicleStatusAsync_ExistingVehicle_UpdatesStatusAndReturnsTrue()
        {
            var vehicle = CreateVehicle(status: "available");
            _context.TbVehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            var result = await _repository.UpdateVehicleStatusAsync(vehicle.Id, "rented");

            Assert.That(result, Is.True);
            var updated = await _context.TbVehicles.FindAsync(vehicle.Id);
            Assert.That(updated!.Status, Is.EqualTo("rented"));
        }

        /// <summary>
        /// Falha: Retorna false quando o veículo informado não existe no banco de dados.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task UpdateVehicleStatusAsync_NonExistingVehicle_ReturnsFalse()
        {
            var result = await _repository.UpdateVehicleStatusAsync(Guid.NewGuid(), "rented");

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Sucesso: Persiste o novo status do veículo no banco de dados após a atualização.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task UpdateVehicleStatusAsync_ExistingVehicle_PersistsNewStatus()
        {
            var vehicle = CreateVehicle(status: "rented");
            _context.TbVehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            await _repository.UpdateVehicleStatusAsync(vehicle.Id, "available");

            _context.ChangeTracker.Clear();
            var updated = await _context.TbVehicles.FindAsync(vehicle.Id);
            Assert.That(updated!.Status, Is.EqualTo("available"));
        }

        // -----------------------------------------------------------------------
        // GetRentalByIdAsync
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Retorna a locação com as navegações de User e Vehicle preenchidas quando o ID existe.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetRentalByIdAsync_ExistingId_ReturnsRentalWithIncludes()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            var rental = CreateRental(user.Id, vehicle.Id);
            _context.TbRentals.Add(rental);
            await _context.SaveChangesAsync();

            var result = await _repository.GetRentalByIdAsync(rental.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(rental.Id));
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.Vehicle, Is.Not.Null);
            Assert.That(result.User.Id, Is.EqualTo(user.Id));
            Assert.That(result.Vehicle.Id, Is.EqualTo(vehicle.Id));
        }

        /// <summary>
        /// Falha: Retorna null quando o ID informado não corresponde a nenhuma locação cadastrada.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetRentalByIdAsync_NonExistingId_ReturnsNull()
        {
            var result = await _repository.GetRentalByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        // -----------------------------------------------------------------------
        // UpdateAsync
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Persiste as alterações de status, data de encerramento e valor total na locação.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task UpdateAsync_ExistingRental_PersistsStatusChange()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            var rental = CreateRental(user.Id, vehicle.Id, "active");
            _context.TbRentals.Add(rental);
            await _context.SaveChangesAsync();

            rental.Status = "closed";
            rental.ActualEndDate = DateTime.UtcNow;
            rental.TotalAmount = 450m;

            await _repository.UpdateAsync(rental);

            _context.ChangeTracker.Clear();
            var updated = await _context.TbRentals.FindAsync(rental.Id);
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.Status, Is.EqualTo("closed"));
            Assert.That(updated.TotalAmount, Is.EqualTo(450m));
            Assert.That(updated.ActualEndDate, Is.Not.Null);
        }

        /// <summary>
        /// Sucesso: Persiste o valor da multa por atraso na locação após a atualização.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task UpdateAsync_ExistingRental_PersistsPenaltyFee()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            var rental = CreateRental(user.Id, vehicle.Id, "active");
            _context.TbRentals.Add(rental);
            await _context.SaveChangesAsync();

            rental.PenaltyFee = 75m;
            rental.Status = "closed";

            await _repository.UpdateAsync(rental);

            _context.ChangeTracker.Clear();
            var updated = await _context.TbRentals.FindAsync(rental.Id);
            Assert.That(updated!.PenaltyFee, Is.EqualTo(75m));
        }

        // -----------------------------------------------------------------------
        // SaveChangesAsync
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Persiste as alterações pendentes no contexto ao chamar SaveChangesAsync diretamente.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task SaveChangesAsync_WithPendingChanges_PersistsChanges()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            var rental = CreateRental(user.Id, vehicle.Id, "active");
            _context.TbRentals.Add(rental);
            await _context.SaveChangesAsync();

            rental.Status = "cancelled";
            _context.TbRentals.Update(rental);

            await _repository.SaveChangesAsync();

            _context.ChangeTracker.Clear();
            var updated = await _context.TbRentals.FindAsync(rental.Id);
            Assert.That(updated!.Status, Is.EqualTo("cancelled"));
        }

        // -----------------------------------------------------------------------
        // SearchRentalsByUserAsync
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Retorna apenas as locações do usuário informado, ignorando as de outros usuários.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task SearchRentalsByUserAsync_WithUserId_ReturnsOnlyUserRentals()
        {
            var user1 = CreateUser("User One", "user1@test.com");
            var user2 = CreateUser("User Two", "user2@test.com");
            var vehicle1 = CreateVehicle("Model A");
            var vehicle2 = CreateVehicle("Model B");
            var vehicle3 = CreateVehicle("Model C");
            _context.TbUsers.AddRange(user1, user2);
            _context.TbVehicles.AddRange(vehicle1, vehicle2, vehicle3);
            _context.TbRentals.AddRange(
                CreateRental(user1.Id, vehicle1.Id),
                CreateRental(user1.Id, vehicle2.Id),
                CreateRental(user2.Id, vehicle3.Id));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user1.Id, null, 1);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(r => r.UserId == user1.Id), Is.True);
        }

        /// <summary>
        /// Sucesso: Retorna apenas locações com o status informado quando o filtro de status é aplicado.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task SearchRentalsByUserAsync_WithStatusFilter_ReturnsOnlyMatchingStatusRentals()
        {
            var user = CreateUser();
            var vehicle1 = CreateVehicle("Model A");
            var vehicle2 = CreateVehicle("Model B");
            var vehicle3 = CreateVehicle("Model C");
            _context.TbUsers.Add(user);
            _context.TbVehicles.AddRange(vehicle1, vehicle2, vehicle3);
            _context.TbRentals.AddRange(
                CreateRental(user.Id, vehicle1.Id, "active"),
                CreateRental(user.Id, vehicle2.Id, "active"),
                CreateRental(user.Id, vehicle3.Id, "closed"));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user.Id, "active", 1);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(r => r.Status == "active"), Is.True);
        }

        /// <summary>
        /// Sucesso: Retorna todas as locações do usuário quando o filtro de status é null.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task SearchRentalsByUserAsync_WithNullStatus_ReturnsAllUserRentals()
        {
            var user = CreateUser();
            var vehicle1 = CreateVehicle("Model A");
            var vehicle2 = CreateVehicle("Model B");
            _context.TbUsers.Add(user);
            _context.TbVehicles.AddRange(vehicle1, vehicle2);
            _context.TbRentals.AddRange(
                CreateRental(user.Id, vehicle1.Id, "active"),
                CreateRental(user.Id, vehicle2.Id, "closed"));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user.Id, null, 1);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        /// <summary>
        /// Sucesso: Retorna todas as locações do usuário quando o filtro de status contém apenas espaços em branco.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task SearchRentalsByUserAsync_WithWhitespaceStatus_ReturnsAllUserRentals()
        {
            var user = CreateUser();
            var vehicle1 = CreateVehicle("Model A");
            var vehicle2 = CreateVehicle("Model B");
            _context.TbUsers.Add(user);
            _context.TbVehicles.AddRange(vehicle1, vehicle2);
            _context.TbRentals.AddRange(
                CreateRental(user.Id, vehicle1.Id, "active"),
                CreateRental(user.Id, vehicle2.Id, "closed"));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user.Id, "   ", 1);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        /// <summary>
        /// Sucesso: Trata o número de página menor que 1 como página 1 e retorna os resultados corretamente.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task SearchRentalsByUserAsync_PageLessThan1_TreatsAsPage1()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            _context.TbRentals.Add(CreateRental(user.Id, vehicle.Id));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user.Id, null, 0);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Sucesso: Retorna as locações ordenadas por data de início de forma decrescente.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task SearchRentalsByUserAsync_ReturnsOrderedByStartDateDescending()
        {
            var user = CreateUser();
            var vehicle1 = CreateVehicle("Model A");
            var vehicle2 = CreateVehicle("Model B");
            var vehicle3 = CreateVehicle("Model C");
            _context.TbUsers.Add(user);
            _context.TbVehicles.AddRange(vehicle1, vehicle2, vehicle3);

            var oldest = CreateRental(user.Id, vehicle1.Id, "closed", DateTime.UtcNow.AddDays(-10));
            var middle = CreateRental(user.Id, vehicle2.Id, "closed", DateTime.UtcNow.AddDays(-5));
            var newest = CreateRental(user.Id, vehicle3.Id, "active", DateTime.UtcNow);

            _context.TbRentals.AddRange(oldest, middle, newest);
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user.Id, null, 1);

            Assert.That(result[0].Id, Is.EqualTo(newest.Id));
            Assert.That(result[1].Id, Is.EqualTo(middle.Id));
            Assert.That(result[2].Id, Is.EqualTo(oldest.Id));
        }

        /// <summary>
        /// Sucesso: Retorna os registros da segunda página ao utilizar paginação com 5 itens por página.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task SearchRentalsByUserAsync_Page2_ReturnsSecondPageRecords()
        {
            var user = CreateUser();
            var vehicles = Enumerable.Range(0, 6).Select(_ => CreateVehicle()).ToList();
            _context.TbUsers.Add(user);
            _context.TbVehicles.AddRange(vehicles);

            for (int i = 0; i < 6; i++)
            {
                _context.TbRentals.Add(new TbRental
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    VehicleId = vehicles[i].Id,
                    StartDate = DateTime.UtcNow.AddDays(-i),
                    ExpectedEndDate = DateTime.UtcNow.AddDays(-i + 3),
                    DailyRate = 100m,
                    Status = "closed"
                });
            }
            await _context.SaveChangesAsync();

            var page1 = await _repository.SearchRentalsByUserAsync(user.Id, null, 1);
            var page2 = await _repository.SearchRentalsByUserAsync(user.Id, null, 2);

            Assert.That(page1.Count, Is.EqualTo(5));
            Assert.That(page2.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Falha: Retorna lista vazia quando nenhuma locação do usuário possui o status informado.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task SearchRentalsByUserAsync_NoMatchingStatus_ReturnsEmptyList()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            _context.TbRentals.Add(CreateRental(user.Id, vehicle.Id, "active"));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user.Id, "cancelled", 1);

            Assert.That(result, Is.Empty);
        }

        /// <summary>
        /// Sucesso: Retorna locações correspondentes ignorando diferenças de capitalização no status informado.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task SearchRentalsByUserAsync_StatusCaseInsensitive_ReturnsMatchingRentals()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            _context.TbRentals.Add(CreateRental(user.Id, vehicle.Id, "Active"));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user.Id, "active", 1);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Sucesso: Retorna lista vazia quando a página solicitada está além do total de registros disponíveis.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task SearchRentalsByUserAsync_PageBeyondTotal_ReturnsEmptyList()
        {
            var user = CreateUser();
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            _context.TbRentals.Add(CreateRental(user.Id, vehicle.Id));
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user.Id, null, 99);

            Assert.That(result, Is.Empty);
        }

        /// <summary>
        /// Sucesso: Retorna lista vazia quando o usuário não possui locações cadastradas.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task SearchRentalsByUserAsync_UserWithNoRentals_ReturnsEmptyList()
        {
            var user = CreateUser();
            _context.TbUsers.Add(user);
            await _context.SaveChangesAsync();

            var result = await _repository.SearchRentalsByUserAsync(user.Id, null, 1);

            Assert.That(result, Is.Empty);
        }
    }

    // ---------------------------------------------------------------------------
    // Tests using Mock<PostgresContext> — verifies SaveChangesAsync interactions
    // ---------------------------------------------------------------------------
    [TestFixture]
    [Category("Repositories")]
    public class RentalRepositoryMockTests
    {
        private Mock<PostgresContext> _contextMock = null!;
        private RentalRepository _repository = null!;

        internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

            public IQueryable CreateQuery(Expression expression)
                => new TestAsyncEnumerable<TEntity>(expression);

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => new TestAsyncEnumerable<TElement>(expression);

            public object Execute(Expression expression)
                => _inner.Execute(expression)!;

            public TResult Execute<TResult>(Expression expression)
                => _inner.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var result = _inner.Execute(expression);
                var returnType = typeof(TResult);

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var taskResultType = returnType.GetGenericArguments()[0];
                    var fromResult = typeof(Task).GetMethod(nameof(Task.FromResult))!
                        .MakeGenericMethod(taskResultType);
                    return (TResult)fromResult.Invoke(null, new[] { Convert.ChangeType(result, taskResultType) })!;
                }

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    var valueTaskArg = returnType.GetGenericArguments()[0];
                    var ctor = typeof(ValueTask<>).MakeGenericType(valueTaskArg)
                        .GetConstructor(new[] { valueTaskArg })!;
                    return (TResult)ctor.Invoke(new[] { Convert.ChangeType(result, valueTaskArg) });
                }

                if (returnType == typeof(Task))
                    return (TResult)(object)Task.CompletedTask;

                return (TResult)result!;
            }
        }

        internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
            public T Current => _inner.Current;
            public ValueTask DisposeAsync() { _inner.Dispose(); return new ValueTask(Task.CompletedTask); }
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data, Func<T, object>? keySelector = null)
            where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(d => d.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken _) => new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            if (keySelector != null)
            {
                mockSet.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                    .Returns<object[]>(ids =>
                    {
                        var key = ids[0];
                        var item = data.FirstOrDefault(x => keySelector(x)!.Equals(key));
                        return new ValueTask<T?>(item);
                    });
            }

            mockSet.Setup(d => d.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .Returns<T, CancellationToken>((entity, _) =>
                {
                    data.Add(entity);
                    return new ValueTask<EntityEntry<T>>((EntityEntry<T>)null!);
                });

            mockSet.Setup(d => d.Update(It.IsAny<T>()))
                .Returns((T entity) => (EntityEntry<T>)null!);

            return mockSet;
        }

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<PostgresContext>();
            _contextMock
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _repository = new RentalRepository(_contextMock.Object);
        }

        // -----------------------------------------------------------------------
        // CreateRentalAsync — verifica chamadas ao contexto
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Verifica que SaveChangesAsync é chamado exatamente uma vez ao criar uma nova locação.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task CreateRentalAsync_ValidRental_CallsSaveChangesAsyncOnce()
        {
            var rentals = new List<TbRental>();
            var mockSet = CreateMockDbSet(rentals);
            _contextMock.Setup(c => c.TbRentals).Returns(mockSet.Object);

            var rental = new TbRental
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow,
                ExpectedEndDate = DateTime.UtcNow.AddDays(3),
                DailyRate = 100m,
                Status = "active"
            };

            await _repository.CreateRentalAsync(rental);

            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Sucesso: Verifica que AddAsync é chamado com a locação correta no DbSet de TbRentals.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task CreateRentalAsync_ValidRental_CallsAddAsyncOnTbRentals()
        {
            var rentals = new List<TbRental>();
            var mockSet = CreateMockDbSet(rentals);
            _contextMock.Setup(c => c.TbRentals).Returns(mockSet.Object);

            var rental = new TbRental
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow,
                ExpectedEndDate = DateTime.UtcNow.AddDays(3),
                DailyRate = 100m,
                Status = "active"
            };

            await _repository.CreateRentalAsync(rental);

            mockSet.Verify(s => s.AddAsync(It.Is<TbRental>(r => r.Id == rental.Id),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // -----------------------------------------------------------------------
        // UpdateAsync — verifica chamadas ao contexto
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Verifica que Update é chamado com a locação correta no DbSet de TbRentals.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task UpdateAsync_ExistingRental_CallsUpdateOnTbRentals()
        {
            var rental = new TbRental
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow,
                ExpectedEndDate = DateTime.UtcNow.AddDays(3),
                DailyRate = 150m,
                Status = "closed"
            };

            var rentals = new List<TbRental> { rental };
            var mockSet = CreateMockDbSet(rentals);
            _contextMock.Setup(c => c.TbRentals).Returns(mockSet.Object);

            await _repository.UpdateAsync(rental);

            mockSet.Verify(s => s.Update(It.Is<TbRental>(r => r.Id == rental.Id)), Times.Once);
        }

        /// <summary>
        /// Sucesso: Verifica que SaveChangesAsync é chamado exatamente uma vez ao atualizar uma locação.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task UpdateAsync_ExistingRental_CallsSaveChangesAsyncOnce()
        {
            var rental = new TbRental
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow,
                ExpectedEndDate = DateTime.UtcNow.AddDays(3),
                DailyRate = 150m,
                Status = "closed"
            };

            var rentals = new List<TbRental> { rental };
            var mockSet = CreateMockDbSet(rentals);
            _contextMock.Setup(c => c.TbRentals).Returns(mockSet.Object);

            await _repository.UpdateAsync(rental);

            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // -----------------------------------------------------------------------
        // UpdateVehicleStatusAsync — verifica chamadas ao contexto
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Verifica que SaveChangesAsync é chamado exatamente uma vez ao atualizar o status de um veículo existente.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task UpdateVehicleStatusAsync_ExistingVehicle_CallsSaveChangesAsyncOnce()
        {
            var vehicleId = Guid.NewGuid();
            var vehicles = new List<TbVehicle>
            {
                new TbVehicle
                {
                    Id = vehicleId,
                    Model = "Civic",
                    Brand = "Honda",
                    Year = 2022,
                    DailyRate = 150m,
                    Status = "available",
                    LicensePlate = "XYZ9999"
                }
            };

            var mockSet = CreateMockDbSet(vehicles, v => ((TbVehicle)v).Id);
            _contextMock.Setup(c => c.TbVehicles).Returns(mockSet.Object);

            await _repository.UpdateVehicleStatusAsync(vehicleId, "rented");

            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Falha: Verifica que SaveChangesAsync nunca é chamado quando o veículo informado não é encontrado.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task UpdateVehicleStatusAsync_NonExistingVehicle_NeverCallsSaveChangesAsync()
        {
            var vehicles = new List<TbVehicle>();
            var mockSet = CreateMockDbSet(vehicles, v => ((TbVehicle)v).Id);
            _contextMock.Setup(c => c.TbVehicles).Returns(mockSet.Object);

            await _repository.UpdateVehicleStatusAsync(Guid.NewGuid(), "rented");

            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // -----------------------------------------------------------------------
        // SaveChangesAsync — verifica delegação ao contexto
        // -----------------------------------------------------------------------

        /// <summary>
        /// Sucesso: Verifica que SaveChangesAsync delega a chamada ao contexto do banco de dados exatamente uma vez.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task SaveChangesAsync_WhenCalled_DelegatesToContext()
        {
            await _repository.SaveChangesAsync();

            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
