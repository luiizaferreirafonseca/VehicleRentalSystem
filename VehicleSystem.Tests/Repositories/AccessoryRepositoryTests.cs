using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories;

namespace VehicleSystem.Tests.Repositories
{
    [TestFixture]
    [Category("Repositories")]
    public class AccessoryRepositoryTests
    {
        private Mock<PostgresContext> _contextMock = null!;
        private AccessoryRepository _repository = null!;

        // Async query provider helpers
        internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;


            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                var result = _inner.Execute(expression);

                var returnType = typeof(TResult);

                // If the caller expects a Task<T>
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var taskResultType = returnType.GetGenericArguments()[0];
                    var fromResult = typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(taskResultType);
                    var converted = Convert.ChangeType(result, taskResultType);
                    return (TResult)fromResult.Invoke(null, new[] { converted });
                }

                // If the caller expects a ValueTask<T>
                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    var valueTaskArg = returnType.GetGenericArguments()[0];
                    var valueTaskType = typeof(ValueTask<>).MakeGenericType(valueTaskArg);
                    var ctor = valueTaskType.GetConstructor(new[] { valueTaskArg })!;
                    var converted = Convert.ChangeType(result, valueTaskArg);
                    return (TResult)ctor.Invoke(new[] { converted });
                }

                // If the caller expects a non-generic Task
                if (returnType == typeof(Task))
                {
                    return (TResult)(object)Task.CompletedTask;
                }

                // Otherwise return the result directly (for synchronous cases)
                return (TResult)result;
            }

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            {
                return new TestAsyncEnumerable<TResult>(expression);
            }
        }

        internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask(Task.CompletedTask);
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data, Func<T, object>? keySelector = null) where T : class
        {
            var queryable = data.AsQueryable();

            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(d => d.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken ct) => new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

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
                        return new ValueTask<T?>(item as T);
                    });
            }

            mockSet.Setup(d => d.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .Returns<T, CancellationToken>((entity, ct) =>
                {
                    data.Add(entity);
                    return new ValueTask<EntityEntry<T>>((EntityEntry<T>?)null);
                });

            // Also support the synchronous Add(T) call so tests don't fail if code uses Add instead of AddAsync
            mockSet.Setup(d => d.Add(It.IsAny<T>()))
                .Callback<T>(entity => data.Add(entity))
                .Returns((T entity) => (EntityEntry<T>?)null);

            mockSet.Setup(d => d.Remove(It.IsAny<T>()))
                .Callback<T>(entity => data.Remove(entity));

            return mockSet;
        }

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<PostgresContext>();
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _repository = new AccessoryRepository(_contextMock.Object);
        }

        [Test]
        public async Task AddAsync_AddsAccessory_ToDatabase()
        {
            var accessories = new List<TbAccessory>();
            var mockSet = CreateMockDbSet(accessories, a => ((TbAccessory)a).Id);
            _contextMock.Setup(c => c.TbAccessories).Returns(mockSet.Object);

            _repository = new AccessoryRepository(_contextMock.Object);

            var accessory = new TbAccessory { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 10m };

            await _repository.AddAsync(accessory);

            Assert.That(accessories.Count, Is.EqualTo(1));
            Assert.That(accessories[0].Name, Is.EqualTo("GPS"));
        }

        [Test]
        public async Task GetByNameAsync_ReturnsAccessory_WhenExists()
        {
            var accessories = new List<TbAccessory>();
            var accessory = new TbAccessory { Id = Guid.NewGuid(), Name = "Finder", DailyRate = 9.99m };
            accessories.Add(accessory);

            var mockAccessorySet = CreateMockDbSet(accessories, a => ((TbAccessory)a).Id);
            _contextMock.Setup(c => c.TbAccessories).Returns(mockAccessorySet.Object);

            var result = await _repository.GetByNameAsync("Finder");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(accessory.Id));
            Assert.That(result.Name, Is.EqualTo("Finder"));
        }

        [Test]
        public async Task GetByNameAsync_ReturnsNull_WhenNotFound()
        {
            var accessories = new List<TbAccessory>();
            var mockAccessorySet = CreateMockDbSet(accessories, a => ((TbAccessory)a).Id);
            _contextMock.Setup(c => c.TbAccessories).Returns(mockAccessorySet.Object);

            var result = await _repository.GetByNameAsync("DoesNotExist");
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllAccessories()
        {
            var accessory1 = new TbAccessory { Id = Guid.NewGuid(), Name = "A1", DailyRate = 1m };
            var accessory2 = new TbAccessory { Id = Guid.NewGuid(), Name = "A2", DailyRate = 2m };

            var accessories = new List<TbAccessory> { accessory1, accessory2 };
            var mockAccessorySet = CreateMockDbSet(accessories, a => ((TbAccessory)a).Id);
            _contextMock.Setup(c => c.TbAccessories).Returns(mockAccessorySet.Object);

            var list = await _repository.GetAllAsync();

            Assert.That(list, Is.Not.Null);
            Assert.That(list!.Count(), Is.EqualTo(2));
            Assert.That(list.Any(a => a.Name == "A1"), Is.True);
            Assert.That(list.Any(a => a.Name == "A2"), Is.True);
        }

        [Test]
        public async Task LinkToRentalAsync_CreatesRentalAccessory_WithCorrectPrices()
        {
            var accessory = new TbAccessory { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 12.5m };
            var rental = new TbRental { Id = Guid.NewGuid(), TotalAmount = 0m, StartDate = DateTime.Now.Date, ExpectedEndDate = DateTime.Now.Date, Status = "active" };

            var accessories = new List<TbAccessory> { accessory };
            var rentals = new List<TbRental> { rental };
            var rentalAccessories = new List<TbRentalAccessory>();

            var mockAccessorySet = CreateMockDbSet(accessories, a => ((TbAccessory)a).Id);
            var mockRentalSet = CreateMockDbSet(rentals, r => ((TbRental)r).Id);
            var mockRentalAccessorySet = CreateMockDbSet(rentalAccessories);

            _contextMock.Setup(c => c.TbAccessories).Returns(mockAccessorySet.Object);
            _contextMock.Setup(c => c.TbRentals).Returns(mockRentalSet.Object);
            _contextMock.Setup(c => c.TbRentalAccessories).Returns(mockRentalAccessorySet.Object);

            await _repository.LinkToRentalAsync(rental.Id, accessory.Id);

            Assert.That(rentalAccessories.Count, Is.EqualTo(1));
            var ra = rentalAccessories.First();
            Assert.That(ra.UnitPrice, Is.EqualTo(accessory.DailyRate));
            Assert.That(ra.TotalPrice, Is.EqualTo(accessory.DailyRate));
            Assert.That(ra.Quantity, Is.EqualTo(1));
        }

        [Test]
        public async Task LinkToRentalAsync_CreatesRentalAccessory_WithZeroPrices_WhenAccessoryNotFound()
        {
            var rental = new TbRental
            {
                Id = Guid.NewGuid(),
                TotalAmount = 0m,
                StartDate = DateTime.Now.Date,
                ExpectedEndDate = DateTime.Now.Date,
                Status = "active"
            };

            var rentals = new List<TbRental> { rental };
            var rentalAccessories = new List<TbRentalAccessory>();

            var mockAccessorySet = CreateMockDbSet(new List<TbAccessory>(), a => ((TbAccessory)a).Id);
            var mockRentalSet = CreateMockDbSet(rentals, r => ((TbRental)r).Id);
            var mockRentalAccessorySet = CreateMockDbSet(rentalAccessories);

            _contextMock.Setup(c => c.TbAccessories).Returns(mockAccessorySet.Object);
            _contextMock.Setup(c => c.TbRentals).Returns(mockRentalSet.Object);
            _contextMock.Setup(c => c.TbRentalAccessories).Returns(mockRentalAccessorySet.Object);

            await _repository.LinkToRentalAsync(rental.Id, Guid.NewGuid());

            Assert.That(rentalAccessories.Count, Is.EqualTo(1));
            var ra = rentalAccessories.First();
            Assert.That(ra.UnitPrice, Is.EqualTo(0m));
            Assert.That(ra.TotalPrice, Is.EqualTo(0m));
            Assert.That(ra.Quantity, Is.EqualTo(1));
        }

        [Test]
        public async Task IsLinkedToRentalAsync_ReturnsTrueWhenLinked_AndFalseWhenNot()
        {
            var accessory = new TbAccessory { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 5m };
            var rental = new TbRental { Id = Guid.NewGuid(), StartDate = DateTime.Now.Date, ExpectedEndDate = DateTime.Now.Date, TotalAmount = 0m, Status = "active" };
            var ra = new TbRentalAccessory { RentalId = rental.Id, AccessoryId = accessory.Id, Quantity = 1, UnitPrice = accessory.DailyRate, TotalPrice = accessory.DailyRate };

            var accessories = new List<TbAccessory> { accessory };
            var rentals = new List<TbRental> { rental };
            var rentalAccessories = new List<TbRentalAccessory> { ra };

            var mockAccessorySet = CreateMockDbSet(accessories, a => ((TbAccessory)a).Id);
            var mockRentalSet = CreateMockDbSet(rentals, r => ((TbRental)r).Id);
            var mockRentalAccessorySet = CreateMockDbSet(rentalAccessories);

            _contextMock.Setup(c => c.TbAccessories).Returns(mockAccessorySet.Object);
            _contextMock.Setup(c => c.TbRentals).Returns(mockRentalSet.Object);
            _contextMock.Setup(c => c.TbRentalAccessories).Returns(mockRentalAccessorySet.Object);

            var linked = await _repository.IsLinkedToRentalAsync(rental.Id, accessory.Id);
            Assert.That(linked, Is.True);

            var notLinked = await _repository.IsLinkedToRentalAsync(Guid.NewGuid(), accessory.Id);
            Assert.That(notLinked, Is.False);
        }

        [Test]
        public async Task RemoveLinkAsync_RemovesExistingLink()
        {
            var accessory = new TbAccessory { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 7m };
            var rental = new TbRental { Id = Guid.NewGuid(), StartDate = DateTime.Now.Date, ExpectedEndDate = DateTime.Now.Date, TotalAmount = 0m, Status = "active" };
            var ra = new TbRentalAccessory { RentalId = rental.Id, AccessoryId = accessory.Id, Quantity = 1, UnitPrice = accessory.DailyRate, TotalPrice = accessory.DailyRate };

            var accessories = new List<TbAccessory> { accessory };
            var rentals = new List<TbRental> { rental };
            var rentalAccessories = new List<TbRentalAccessory> { ra };

            var mockAccessorySet = CreateMockDbSet(accessories, a => ((TbAccessory)a).Id);
            var mockRentalSet = CreateMockDbSet(rentals, r => ((TbRental)r).Id);
            var mockRentalAccessorySet = CreateMockDbSet(rentalAccessories);

            _contextMock.Setup(c => c.TbAccessories).Returns(mockAccessorySet.Object);
            _contextMock.Setup(c => c.TbRentals).Returns(mockRentalSet.Object);
            _contextMock.Setup(c => c.TbRentalAccessories).Returns(mockRentalAccessorySet.Object);

            await _repository.RemoveLinkAsync(rental.Id, accessory.Id);

            Assert.That(rentalAccessories.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetByRentalIdAsync_ReturnsAccessories_LinkedToRental()
        {
            var accessory1 = new TbAccessory { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 5m };
            var accessory2 = new TbAccessory { Id = Guid.NewGuid(), Name = "Cadeira", DailyRate = 20m };
            var rental = new TbRental { Id = Guid.NewGuid(), StartDate = DateTime.Now.Date, ExpectedEndDate = DateTime.Now.Date, TotalAmount = 0m, Status = "active" };

            var ra1 = new TbRentalAccessory { RentalId = rental.Id, AccessoryId = accessory1.Id, Quantity = 1, UnitPrice = accessory1.DailyRate, TotalPrice = accessory1.DailyRate, Accessory = accessory1 };
            var ra2 = new TbRentalAccessory { RentalId = rental.Id, AccessoryId = accessory2.Id, Quantity = 1, UnitPrice = accessory2.DailyRate, TotalPrice = accessory2.DailyRate, Accessory = accessory2 };

            var accessories = new List<TbAccessory> { accessory1, accessory2 };
            var rentalAccessories = new List<TbRentalAccessory> { ra1, ra2 };

            var mockAccessorySet = CreateMockDbSet(accessories, a => ((TbAccessory)a).Id);
            var mockRentalAccessorySet = CreateMockDbSet(rentalAccessories);

            _contextMock.Setup(c => c.TbAccessories).Returns(mockAccessorySet.Object);
            _contextMock.Setup(c => c.TbRentalAccessories).Returns(mockRentalAccessorySet.Object);

            var list = await _repository.GetByRentalIdAsync(rental.Id);

            Assert.That(list, Is.Not.Null);
            Assert.That(list!.Count(), Is.EqualTo(2));
            Assert.That(list.Any(a => a.Name == "GPS"), Is.True);
            Assert.That(list.Any(a => a.Name == "Cadeira"), Is.True);
        }
    }
}
