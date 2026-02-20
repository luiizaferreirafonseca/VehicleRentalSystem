using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleSystem.Tests.Startup
{
    internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public IServiceCollection? OriginalServices { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                OriginalServices = new ServiceCollection();
                foreach (var descriptor in services)
                    OriginalServices.Add(descriptor);

                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<PostgresContext>))
                    .ToList();
                foreach (var d in descriptors)
                    services.Remove(d);

                services.AddDbContext<PostgresContext>(options =>
                    options.UseInMemoryDatabase("ProgramTestsDb"));
            });
        }
    }

    [TestFixture]
    [Category("Startup")]
    public class ProgramTests
    {
        private CustomWebApplicationFactory _factory = null!;

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _ = _factory.Services;
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
        }

        private T? Resolve<T>() where T : class
        {
            using var scope = _factory.Services.CreateScope();
            return scope.ServiceProvider.GetService<T>();
        }

        private ServiceDescriptor? GetOriginalDescriptor<TService>()
            => _factory.OriginalServices?.FirstOrDefault(d => d.ServiceType == typeof(TService));

        // -----------------------------------------------------------------------
        // IRentalRepository
        // -----------------------------------------------------------------------

        #region IRentalRepository

        /// <summary>
        /// Sucesso: IRentalRepository é resolvido como RentalRepository no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IRentalRepository_IsRegisteredAsRentalRepository()
        {
            var service = Resolve<IRentalRepository>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<RentalRepository>());
        }

        /// <summary>
        /// Sucesso: IRentalRepository é registrado com tempo de vida Scoped.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IRentalRepository_HasScopedLifetime()
        {
            var descriptor = GetOriginalDescriptor<IRentalRepository>();

            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor!.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        }

        #endregion

        // -----------------------------------------------------------------------
        // IRentalService
        // -----------------------------------------------------------------------

        #region IRentalService

        /// <summary>
        /// Sucesso: IRentalService é resolvido como RentalService no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IRentalService_IsRegisteredAsRentalService()
        {
            var service = Resolve<IRentalService>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<RentalService>());
        }

        /// <summary>
        /// Sucesso: IRentalService é registrado com tempo de vida Scoped.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IRentalService_HasScopedLifetime()
        {
            var descriptor = GetOriginalDescriptor<IRentalService>();

            Assert.That(descriptor, Is.Not.Null);
            Assert.That(descriptor!.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        }

        #endregion

        // -----------------------------------------------------------------------
        // IUserRepository / IUserService
        // -----------------------------------------------------------------------

        #region IUserRepository / IUserService

        /// <summary>
        /// Sucesso: IUserRepository é resolvido como UserRepository no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IUserRepository_IsRegisteredAsUserRepository()
        {
            var service = Resolve<IUserRepository>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<UserRepository>());
        }

        /// <summary>
        /// Sucesso: IUserService é resolvido como UserService no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IUserService_IsRegisteredAsUserService()
        {
            var service = Resolve<IUserService>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<UserService>());
        }

        #endregion

        // -----------------------------------------------------------------------
        // IAccessoryRepository / IAccessoryService
        // -----------------------------------------------------------------------

        #region IAccessoryRepository / IAccessoryService

        /// <summary>
        /// Sucesso: IAccessoryRepository é resolvido como AccessoryRepository no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IAccessoryRepository_IsRegisteredAsAccessoryRepository()
        {
            var service = Resolve<IAccessoryRepository>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<AccessoryRepository>());
        }

        /// <summary>
        /// Sucesso: IAccessoryService é resolvido como AccessoryService no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IAccessoryService_IsRegisteredAsAccessoryService()
        {
            var service = Resolve<IAccessoryService>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<AccessoryService>());
        }

        #endregion

        // -----------------------------------------------------------------------
        // IVehicleRepository / IVehicleService
        // -----------------------------------------------------------------------

        #region IVehicleRepository / IVehicleService

        /// <summary>
        /// Sucesso: IVehicleRepository é resolvido como VehicleRepository no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IVehicleRepository_IsRegisteredAsVehicleRepository()
        {
            var service = Resolve<IVehicleRepository>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<VehicleRepository>());
        }

        /// <summary>
        /// Sucesso: IVehicleService é resolvido como VehicleService no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IVehicleService_IsRegisteredAsVehicleService()
        {
            var service = Resolve<IVehicleService>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<VehicleService>());
        }

        #endregion

        // -----------------------------------------------------------------------
        // IPaymentRepository / IPaymentService
        // -----------------------------------------------------------------------

        #region IPaymentRepository / IPaymentService

        /// <summary>
        /// Sucesso: IPaymentRepository é resolvido como PaymentRepository no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IPaymentRepository_IsRegisteredAsPaymentRepository()
        {
            var service = Resolve<IPaymentRepository>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<PaymentRepository>());
        }

        /// <summary>
        /// Sucesso: IPaymentService é resolvido como PaymentService no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IPaymentService_IsRegisteredAsPaymentService()
        {
            var service = Resolve<IPaymentService>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<PaymentService>());
        }

        #endregion

        // -----------------------------------------------------------------------
        // IRatingRepository / IRatingService
        // -----------------------------------------------------------------------

        #region IRatingRepository / IRatingService

        /// <summary>
        /// Sucesso: IRatingRepository é resolvido como RatingRepository no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IRatingRepository_IsRegisteredAsRatingRepository()
        {
            var service = Resolve<IRatingRepository>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<RatingRepository>());
        }

        /// <summary>
        /// Sucesso: IRatingService é resolvido como RatingService no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IRatingService_IsRegisteredAsRatingService()
        {
            var service = Resolve<IRatingService>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<RatingService>());
        }

        #endregion

        // -----------------------------------------------------------------------
        // IRentalReportRepository / IRentalReportService
        // -----------------------------------------------------------------------

        #region IRentalReportRepository / IRentalReportService

        /// <summary>
        /// Sucesso: IRentalReportRepository é resolvido como RentalReportRepository no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IRentalReportRepository_IsRegisteredAsRentalReportRepository()
        {
            var service = Resolve<IRentalReportRepository>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<RentalReportRepository>());
        }

        /// <summary>
        /// Sucesso: IRentalReportService é resolvido como RentalReportService no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_IRentalReportService_IsRegisteredAsRentalReportService()
        {
            var service = Resolve<IRentalReportService>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.TypeOf<RentalReportService>());
        }

        #endregion

        // -----------------------------------------------------------------------
        // PostgresContext
        // -----------------------------------------------------------------------

        #region PostgresContext

        /// <summary>
        /// Sucesso: PostgresContext está registrado e é resolvido com sucesso no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public void ServiceRegistration_PostgresContext_IsRegistered()
        {
            var service = Resolve<PostgresContext>();

            Assert.That(service, Is.Not.Null);
        }

        /// <summary>
        /// Falha: PostgresContext está registrado mais de uma vez no Program.cs, causando duplicidade no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public void ServiceRegistration_PostgresContext_IsRegisteredExactlyOnce()
        {
            var count = _factory.OriginalServices?
                .Count(d => d.ServiceType == typeof(PostgresContext));

            Assert.That(count, Is.EqualTo(1));
        }

        #endregion

        // -----------------------------------------------------------------------
        // Swagger
        // -----------------------------------------------------------------------

        #region Swagger

        /// <summary>
        /// Sucesso: Swagger (Swashbuckle) está registrado no container de DI.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public void ServiceRegistration_Swagger_IsRegistered()
        {
            var descriptor = _factory.OriginalServices?
                .FirstOrDefault(d => d.ServiceType.FullName != null
                                  && d.ServiceType.FullName.Contains("Swagger"));

            Assert.That(descriptor, Is.Not.Null);
        }

        #endregion
    }
}
