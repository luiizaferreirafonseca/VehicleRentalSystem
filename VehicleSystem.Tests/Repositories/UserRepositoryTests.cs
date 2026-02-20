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
    public class UserRepositoryTests
    {
        private PostgresContext _context = null!;
        private UserRepository _repository = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<PostgresContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new PostgresContext(options);
            _repository = new UserRepository(_context);
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

        private static TbVehicle CreateVehicle()
            => new TbVehicle
            {
                Id = Guid.NewGuid(),
                Model = "Civic",
                Brand = "Honda",
                Year = 2022,
                DailyRate = 150m,
                Status = "available",
                LicensePlate = Guid.NewGuid().ToString("N")[..12].ToUpper()
            };

        private static TbRental CreateRental(Guid userId, Guid vehicleId)
            => new TbRental
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VehicleId = vehicleId,
                StartDate = DateTime.UtcNow,
                ExpectedEndDate = DateTime.UtcNow.AddDays(3),
                DailyRate = 150m,
                Status = "active"
            };

        // -----------------------------------------------------------------------
        // GetAllUsersAsync
        // -----------------------------------------------------------------------

        #region GetAllUsersAsync

        /// <summary>
        /// Sucesso: Retorna todos os usuários com a navegação de TbRentals preenchida quando existem registros.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetAllUsersAsync_WithUsers_ReturnsAllUsersWithRentalsIncluded()
        {
            var user = CreateUser("João Silva", "joao@test.com");
            var vehicle = CreateVehicle();
            _context.TbUsers.Add(user);
            _context.TbVehicles.Add(vehicle);
            _context.TbRentals.Add(CreateRental(user.Id, vehicle.Id));
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllUsersAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TbRentals, Is.Not.Null);
            Assert.That(result[0].TbRentals.Count, Is.EqualTo(1));
        }

        /// <summary>
        /// Sucesso: Retorna lista vazia quando não há usuários cadastrados no banco de dados.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetAllUsersAsync_EmptyDatabase_ReturnsEmptyList()
        {
            var result = await _repository.GetAllUsersAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        /// <summary>
        /// Sucesso: Retorna todos os registros corretamente quando existem múltiplos usuários no banco.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetAllUsersAsync_MultipleUsers_ReturnsAllRecords()
        {
            _context.TbUsers.AddRange(
                CreateUser("Alice", "alice@test.com"),
                CreateUser("Bob", "bob@test.com"),
                CreateUser("Carol", "carol@test.com"));
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllUsersAsync();

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Any(u => u.Email == "alice@test.com"), Is.True);
            Assert.That(result.Any(u => u.Email == "bob@test.com"), Is.True);
            Assert.That(result.Any(u => u.Email == "carol@test.com"), Is.True);
        }

        /// <summary>
        /// Sucesso: Retorna usuário com lista de locações vazia quando ele não possui locações cadastradas.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task GetAllUsersAsync_UserWithNoRentals_ReturnsUserWithEmptyRentalsList()
        {
            _context.TbUsers.Add(CreateUser("Maria", "maria@test.com"));
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllUsersAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TbRentals, Is.Empty);
        }

        /// <summary>
        /// Sucesso: Retorna todos os usuários com suas respectivas locações mapeadas corretamente.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetAllUsersAsync_MultipleUsersWithRentals_RetornaLocacoesPorUsuario()
        {
            var user1 = CreateUser("User1", "u1@test.com");
            var user2 = CreateUser("User2", "u2@test.com");
            var vehicle1 = CreateVehicle();
            var vehicle2 = CreateVehicle();
            _context.TbUsers.AddRange(user1, user2);
            _context.TbVehicles.AddRange(vehicle1, vehicle2);
            _context.TbRentals.AddRange(
                CreateRental(user1.Id, vehicle1.Id),
                CreateRental(user1.Id, vehicle2.Id));
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllUsersAsync();

            var resultUser1 = result.First(u => u.Id == user1.Id);
            var resultUser2 = result.First(u => u.Id == user2.Id);
            Assert.That(resultUser1.TbRentals.Count, Is.EqualTo(2));
            Assert.That(resultUser2.TbRentals.Count, Is.EqualTo(0));
        }

        #endregion

        // -----------------------------------------------------------------------
        // ExistsByEmailAsync
        // -----------------------------------------------------------------------

        #region ExistsByEmailAsync

        /// <summary>
        /// Sucesso: Retorna true quando o e-mail informado já está cadastrado no banco de dados.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task ExistsByEmailAsync_ExistingEmail_ReturnsTrue()
        {
            _context.TbUsers.Add(CreateUser("Ana", "ana@test.com"));
            await _context.SaveChangesAsync();

            var result = await _repository.ExistsByEmailAsync("ana@test.com");

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Falha: Retorna false quando o e-mail informado não corresponde a nenhum usuário cadastrado.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task ExistsByEmailAsync_NonExistingEmail_ReturnsFalse()
        {
            var result = await _repository.ExistsByEmailAsync("naoexiste@test.com");

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Falha: Retorna false quando o banco de dados está vazio.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task ExistsByEmailAsync_EmptyDatabase_ReturnsFalse()
        {
            var result = await _repository.ExistsByEmailAsync("qualquer@test.com");

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Sucesso: Retorna true somente para o e-mail exato, confirmando a busca por correspondência exata.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task ExistsByEmailAsync_ExactEmailMatch_ReturnsTrueOnlyForExactEmail()
        {
            _context.TbUsers.Add(CreateUser("Carlos", "carlos@test.com"));
            await _context.SaveChangesAsync();

            var existsExact = await _repository.ExistsByEmailAsync("carlos@test.com");
            var existsPartial = await _repository.ExistsByEmailAsync("carlos@");

            Assert.That(existsExact, Is.True);
            Assert.That(existsPartial, Is.False);
        }

        /// <summary>
        /// Falha: Retorna false quando o e-mail difere apenas na capitalização, confirmando busca sensível a maiúsculas.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task ExistsByEmailAsync_EmailWithDifferentCase_ReturnsFalse()
        {
            _context.TbUsers.Add(CreateUser("Pedro", "pedro@test.com"));
            await _context.SaveChangesAsync();

            var result = await _repository.ExistsByEmailAsync("PEDRO@TEST.COM");

            Assert.That(result, Is.False);
        }

        #endregion

        // -----------------------------------------------------------------------
        // CreateUserAsync
        // -----------------------------------------------------------------------

        #region CreateUserAsync

        /// <summary>
        /// Sucesso: Persiste o novo usuário no banco de dados após a criação.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task CreateUserAsync_ValidUser_PersistsInDatabase()
        {
            var user = CreateUser("Lucas", "lucas@test.com");

            await _repository.CreateUserAsync(user);

            var persisted = await _context.TbUsers.FindAsync(user.Id);
            Assert.That(persisted, Is.Not.Null);
        }

        /// <summary>
        /// Sucesso: Retorna o usuário criado com todos os campos corretamente preenchidos.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task CreateUserAsync_ValidUser_ReturnsUserWithCorrectFields()
        {
            var user = CreateUser("Fernanda", "fernanda@test.com");

            var result = await _repository.CreateUserAsync(user);

            Assert.That(result.Id, Is.EqualTo(user.Id));
            Assert.That(result.Name, Is.EqualTo("Fernanda"));
            Assert.That(result.Email, Is.EqualTo("fernanda@test.com"));
            Assert.That(result.Active, Is.True);
        }

        /// <summary>
        /// Sucesso: O usuário criado pode ser encontrado pelo e-mail após a persistência.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task CreateUserAsync_ValidUser_CanBeFoundByEmailAfterPersistence()
        {
            var user = CreateUser("Rafael", "rafael@test.com");

            await _repository.CreateUserAsync(user);

            var exists = await _context.TbUsers.AnyAsync(u => u.Email == "rafael@test.com");
            Assert.That(exists, Is.True);
        }

        /// <summary>
        /// Sucesso: Múltiplos usuários são persistidos individualmente e encontrados no banco.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task CreateUserAsync_MultipleUsers_AllPersistedInDatabase()
        {
            await _repository.CreateUserAsync(CreateUser("User A", "a@test.com"));
            await _repository.CreateUserAsync(CreateUser("User B", "b@test.com"));
            await _repository.CreateUserAsync(CreateUser("User C", "c@test.com"));

            var total = await _context.TbUsers.CountAsync();
            Assert.That(total, Is.EqualTo(3));
        }

        #endregion
    }

    // ---------------------------------------------------------------------------
    // Tests using Mock<PostgresContext> — verifies SaveChangesAsync interactions
    // ---------------------------------------------------------------------------
    [TestFixture]
    [Category("Repositories")]
    public class UserRepositoryMockTests
    {
        private Mock<PostgresContext> _contextMock = null!;
        private UserRepository _repository = null!;

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

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
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

            mockSet.Setup(d => d.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .Returns<T, CancellationToken>((entity, _) =>
                {
                    data.Add(entity);
                    return new ValueTask<EntityEntry<T>>((EntityEntry<T>)null!);
                });

            return mockSet;
        }

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<PostgresContext>();
            _contextMock
                .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _repository = new UserRepository(_contextMock.Object);
        }

        // -----------------------------------------------------------------------
        // CreateUserAsync — verifica chamadas ao contexto
        // -----------------------------------------------------------------------

        #region CreateUserAsync — Mock

        /// <summary>
        /// Sucesso: Verifica que SaveChangesAsync é chamado exatamente uma vez ao criar um novo usuário.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task CreateUserAsync_ValidUser_CallsSaveChangesAsyncOnce()
        {
            var users = new List<TbUser>();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.TbUsers).Returns(mockSet.Object);

            var user = new TbUser
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "test@test.com",
                Active = true
            };

            await _repository.CreateUserAsync(user);

            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Sucesso: Verifica que AddAsync é chamado com o usuário correto no DbSet de TbUsers.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task CreateUserAsync_ValidUser_CallsAddAsyncOnTbUsers()
        {
            var users = new List<TbUser>();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.TbUsers).Returns(mockSet.Object);

            var user = new TbUser
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "test@test.com",
                Active = true
            };

            await _repository.CreateUserAsync(user);

            mockSet.Verify(s => s.AddAsync(
                It.Is<TbUser>(u => u.Id == user.Id && u.Email == user.Email),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Sucesso: Verifica que o usuário retornado é exatamente o mesmo objeto passado como parâmetro.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task CreateUserAsync_ValidUser_ReturnsSameUserInstance()
        {
            var users = new List<TbUser>();
            var mockSet = CreateMockDbSet(users);
            _contextMock.Setup(c => c.TbUsers).Returns(mockSet.Object);

            var user = new TbUser
            {
                Id = Guid.NewGuid(),
                Name = "Instância",
                Email = "instancia@test.com",
                Active = true
            };

            var result = await _repository.CreateUserAsync(user);

            Assert.That(result, Is.SameAs(user));
        }

        #endregion
    }
}
