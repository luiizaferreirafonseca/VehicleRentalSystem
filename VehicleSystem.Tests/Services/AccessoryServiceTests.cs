using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums.VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services;

namespace VehicleSystem.Tests.Services
{
    [TestFixture]
    [Category("Business Services")]
    public class AccessoryServiceTests
    {
        private Mock<IRentalRepository> _rentalRepositoryMock;
        private Mock<IAccessoryRepository> _accessoryRepositoryMock;
        private AccessoryService _service;

        [SetUp]
        public void SetUp()
        {
            _rentalRepositoryMock = new Mock<IRentalRepository>();
            _accessoryRepositoryMock = new Mock<IAccessoryRepository>();
            _service = new AccessoryService(_rentalRepositoryMock.Object, _accessoryRepositoryMock.Object);
        }

        #region Get Methods (Success & Failure)

        /// <summary>
        /// Sucesso: Retorna lista vazia se o repositório retornar null.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task GetAccessoriesAsync_NoAccessoriesFound_ReturnsEmptyList()
        {
            _accessoryRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync((IEnumerable<TbAccessory>)null!);
            var result = await _service.GetAccessoriesAsync();
            Assert.That(result, Is.Empty);
        }

        // Tests for GetAccessoryByIdAsync (moved to follow service order)
        /// <summary>
        /// Falha: GetAccessoryByIdAsync deve lançar KeyNotFoundException se o ID não existir.
        /// </summary>
        [Test]
        [Category("Unit")]
        public void GetAccessoryByIdAsync_IdNotFound_ThrowsKeyNotFoundException()
        {
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TbAccessory)null!);
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAccessoryByIdAsync(Guid.NewGuid()));
        }

        /// <summary>
        /// Sucesso: Garante que GetAccessoryByIdAsync mapeie as propriedades para o DTO e retorne corretamente.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task GetAccessoryByIdAsync_ValidId_ReturnsMappedDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var accessory = new TbAccessory { Id = id, Name = "GPS", DailyRate = 10.0m };
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(accessory);

            // Act
            var result = await _service.GetAccessoryByIdAsync(id);

            // Assert - Valida cada campo para cobrir as linhas vermelhas de projeção
            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("GPS"));
                Assert.That(result.DailyRate, Is.EqualTo(10.0m));
            });
        }

        /// <summary>
        /// Sucesso: Verifica se GetAccessoryByIdAsync projeta corretamente os dados da entidade para o DTO de resposta.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task GetAccessoryByIdAsync_ExistingId_ReturnsCorrectMapping()
        {
            // Arrange
            var accessoryId = Guid.NewGuid();
            var accessory = new TbAccessory { Id = accessoryId, Name = "GPS Plus", DailyRate = 12.0m };
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(accessoryId)).ReturnsAsync(accessory);

            // Act
            var result = await _service.GetAccessoryByIdAsync(accessoryId);

            // Assert - Valida cada campo para cobrir as linhas de projeção (new AccessoryResponseDto)
            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(accessory.Id));
                Assert.That(result.Name, Is.EqualTo(accessory.Name));
                Assert.That(result.DailyRate, Is.EqualTo(accessory.DailyRate));
            });
        }

        /// <summary>
        /// Sucesso: Garante que GetAccessoryByIdAsync executa o mapeamento das propriedades para o DTO.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task GetAccessoryByIdAsync_ExistingId_ExecutesMapping()
        {
            // Arrange
            var id = Guid.NewGuid();
            var accessory = new TbAccessory { Id = id, Name = "GPS", DailyRate = 10m };
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(accessory);

            // Act
            var result = await _service.GetAccessoryByIdAsync(id);

            // Assert - Validação direta para cobrir as linhas de projeção
            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("GPS"));
                Assert.That(result.DailyRate, Is.EqualTo(10m));
            });
        }

        /// <summary>
        /// Sucesso: Garante que GetAccessoryByIdAsync execute o mapeamento completo para o DTO de resposta.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task GetAccessoryByIdAsync_ExistingAccessory_ReturnsMappedDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var accessory = new TbAccessory { Id = id, Name = "GPS", DailyRate = 15.0m };
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(accessory);

            // Act
            var result = await _service.GetAccessoryByIdAsync(id);

            // Assert - Validação campo a campo para cobrir as linhas vermelhas
            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(id));
                Assert.That(result.Name, Is.EqualTo("GPS"));
                Assert.That(result.DailyRate, Is.EqualTo(15.0m));
            });
        }

        // Tests for GetAccessoriesByRentalIdAsync (moved after GetAccessoryByIdAsync)
        /// <summary>
        /// Sucesso: Verifica se GetAccessoriesByRentalIdAsync projeta corretamente a lista de entidades para DTOs quando existem dados.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task GetAccessoriesByRentalIdAsync_ExistingAccessories_ReturnsMappedList()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var accessories = new List<TbAccessory>
    {
        new TbAccessory { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 15m }
    };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(new TbRental());
            _accessoryRepositoryMock.Setup(r => r.GetByRentalIdAsync(rentalId)).ReturnsAsync(accessories);

            // Act
            var result = await _service.GetAccessoriesByRentalIdAsync(rentalId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Count(), Is.EqualTo(1));
                Assert.That(result.First().Name, Is.EqualTo("GPS"));
            });
        }

        /// <summary>
        /// Sucesso: Verifica se GetAccessoriesByRentalIdAsync projeta corretamente os acessórios encontrados para a lista de DTOs.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task GetAccessoriesByRentalIdAsync_WithAccessories_ReturnsMappedList()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var accessories = new List<TbAccessory>
    {
        new TbAccessory { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 10.0m },
        new TbAccessory { Id = Guid.NewGuid(), Name = "Cadeira Bebê", DailyRate = 20.0m }
    };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(new TbRental());
            _accessoryRepositoryMock.Setup(r => r.GetByRentalIdAsync(rentalId)).ReturnsAsync(accessories);

            // Act
            var result = await _service.GetAccessoriesByRentalIdAsync(rentalId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.Any(a => a.Name == "GPS"), Is.True);
                Assert.That(result.Any(a => a.Name == "Cadeira Bebê"), Is.True);
            });
        }

        /// <summary>
        /// Falha: GetAccessoriesByRentalIdAsync deve validar se a locação existe antes de buscar acessórios.
        /// </summary>
        [Test]
        [Category("Unit")]
        public void GetAccessoriesByRentalIdAsync_RentalNotFound_ThrowsKeyNotFoundException()
        {
            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TbRental)null!);
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAccessoriesByRentalIdAsync(Guid.NewGuid()));
        }

        /// <summary>
        /// Sucesso: Força a execução da ramificação 'Enumerable.Empty' quando o repositório retorna null.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task GetAccessoriesByRentalIdAsync_RepositoryReturnsNull_CoversNullBranch()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(new TbRental());
            _accessoryRepositoryMock
                .Setup(r => r.GetByRentalIdAsync(rentalId))
                .ReturnsAsync((IEnumerable<TbAccessory>)null!);

            // Act
            var result = await _service.GetAccessoriesByRentalIdAsync(rentalId);

            // Assert
            Assert.That(result, Is.Empty);
        }

        /// <summary>
        /// Sucesso: Verifica se GetAccessoriesByRentalIdAsync retorna a lista de acessórios mapeada corretamente para AccessoryResponseDto.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task GetAccessoriesByRentalIdAsync_ValidRental_ReturnsMappedDtoList()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var accessories = new List<TbAccessory>
    {
        new TbAccessory { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 15.0m }
    };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(new TbRental());
            _accessoryRepositoryMock.Setup(r => r.GetByRentalIdAsync(rentalId)).ReturnsAsync(accessories);

            // Act
            var result = await _service.GetAccessoriesByRentalIdAsync(rentalId);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.First().Name, Is.EqualTo("GPS"));
                Assert.That(result.First().DailyRate, Is.EqualTo(15.0m));
            });
        }

        #endregion

        #region Create Methods (Success & Failure)

        /// <summary>
        /// Sucesso: Verifica se CreateAccessoryAsync instancia corretamente o modelo TbAccessory e o persiste via repositório.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task CreateAccessoryAsync_ValidDto_ExecutesInternalInstantiation()
        {
            // Arrange
            var dto = new AccessoryCreateDto { Name = "Assento Elevação", DailyRate = 15.5m };
            _accessoryRepositoryMock.Setup(r => r.GetByNameAsync(dto.Name)).ReturnsAsync((TbAccessory)null!);

            // Act
            var result = await _service.CreateAccessoryAsync(dto);

            // Assert - Valida se o repositório recebeu o objeto com os dados do DTO e o Guid gerado
            _accessoryRepositoryMock.Verify(r => r.AddAsync(It.Is<TbAccessory>(a =>
                a.Name == dto.Name &&
                a.DailyRate == dto.DailyRate &&
                a.Id != Guid.Empty)), Times.Once);
        }

        /// <summary>
        /// Sucesso: Verifica se CreateAccessoryAsync persiste o acessório e retorna o DTO com ID gerado.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task CreateAccessoryAsync_ValidData_PersistsAndReturnsDto()
        {
            // Arrange
            var dto = new AccessoryCreateDto { Name = "Cadeira Infantil", DailyRate = 20.0m };
            _accessoryRepositoryMock.Setup(r => r.GetByNameAsync(dto.Name)).ReturnsAsync((TbAccessory)null!);

            // Act
            var result = await _service.CreateAccessoryAsync(dto);

            // Assert
            _accessoryRepositoryMock.Verify(r => r.AddAsync(It.Is<TbAccessory>(a => a.Name == dto.Name)), Times.Once);
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        }

        /// <summary>
        /// Sucesso: Verifica se CreateAccessoryAsync instancia a entidade TbAccessory e chama o AddAsync.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task CreateAccessoryAsync_ValidData_CoversEntityInstantiation()
        {
            // Arrange
            var dto = new AccessoryCreateDto { Name = "Cadeira Bebê", DailyRate = 20m };
            _accessoryRepositoryMock.Setup(r => r.GetByNameAsync(dto.Name)).ReturnsAsync((TbAccessory)null!);

            // Act
            var result = await _service.CreateAccessoryAsync(dto);

            // Assert - Verifica se AddAsync foi chamado com um objeto contendo os dados do DTO
            _accessoryRepositoryMock.Verify(r => r.AddAsync(It.Is<TbAccessory>(a =>
                a.Name == dto.Name &&
                a.DailyRate == dto.DailyRate &&
                a.Id != Guid.Empty)), Times.Once);
        }
        /// <summary>
        /// Sucesso: Cria um novo acessório com dados válidos e retorna o DTO correspondente.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task CreateAccessoryAsync_ValidData_ReturnsAccessoryResponseDto()
        {
            // Arrange
            var dto = new AccessoryCreateDto { Name = "Cadeira de Bebê", DailyRate = 25.50m };
            _accessoryRepositoryMock.Setup(r => r.GetByNameAsync(dto.Name)).ReturnsAsync((TbAccessory)null!);

            // Act
            var result = await _service.CreateAccessoryAsync(dto);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Name, Is.EqualTo(dto.Name));
                Assert.That(result.DailyRate, Is.EqualTo(dto.DailyRate));
                Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            });
            _accessoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TbAccessory>()), Times.Once);
        }

        /// <summary>
        /// Falha: Impede criação se o nome do acessório já estiver em uso.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public void CreateAccessoryAsync_DuplicateName_ThrowsInvalidOperationException()
        {
            var dto = new AccessoryCreateDto { Name = "GPS" };
            _accessoryRepositoryMock.Setup(r => r.GetByNameAsync(dto.Name)).ReturnsAsync(new TbAccessory());

            Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAccessoryAsync(dto));
            _accessoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TbAccessory>()), Times.Never);
        }

        /// <summary>
        /// Sucesso: Verifica se CreateAccessoryAsync instancia corretamente o modelo e chama o repositório.
        /// </summary>
        [Test]
        [Category("Unit")]
        public async Task CreateAccessoryAsync_NewAccessory_CallsRepositoryWithCorrectData()
        {
            // Arrange
            var dto = new AccessoryCreateDto { Name = "WiFi Hotspot", DailyRate = 5.0m };
            _accessoryRepositoryMock.Setup(r => r.GetByNameAsync(dto.Name)).ReturnsAsync((TbAccessory)null!);

            // Act
            var result = await _service.CreateAccessoryAsync(dto);

            // Assert
            _accessoryRepositoryMock.Verify(r => r.AddAsync(It.Is<TbAccessory>(a =>
                a.Name == dto.Name &&
                a.DailyRate == dto.DailyRate)), Times.Once);
            Assert.That(result.Name, Is.EqualTo(dto.Name));
        }

        #endregion

        #region Add Accessory Logic (Success & Failure)

        /// <summary>
        /// Falha: Verifica se AddAccessoryToRentalAsync lança ArgumentException quando IDs são vazios.
        /// </summary>
        [Test]
        [Category("Validation")]
        public void AddAccessoryToRentalAsync_GuidsAreEmpty_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddAccessoryToRentalAsync(Guid.Empty, Guid.Empty));
        }

        /// <summary>
        /// Sucesso: Verifica se o cálculo do TotalAmount considera múltiplos dias corretamente (ex: 3 dias).
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_MultipleDays_CalculatesTotalCorrectly()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var end = start.AddDays(3);
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 100m };
            var accessory = new TbAccessory { DailyRate = 10m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - calcula o esperado usando a mesma lógica do serviço
            var days = (rental.ExpectedEndDate.Date - rental.StartDate.Date).Days;
            if (days <= 0) days = 1;
            var expectedTotal = 100m + (accessory.DailyRate * days);
            Assert.That(rental.TotalAmount, Is.EqualTo(expectedTotal));
        }

        /// <summary>
        /// Sucesso: Verifica se AddAccessoryToRentalAsync calcula o valor total corretamente para múltiplos dias.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_MultipleDays_CalculatesAndUpdatesTotal()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var end = start.AddDays(5); // 5 dias
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 100m };
            var accessory = new TbAccessory { DailyRate = 10m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - calcula o esperado usando a mesma lógica do serviço
            var days = (rental.ExpectedEndDate.Date - rental.StartDate.Date).Days;
            if (days <= 0) days = 1;
            var expectedTotal = 100m + (accessory.DailyRate * days);
            Assert.That(rental.TotalAmount, Is.EqualTo(expectedTotal));
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }

        /// <summary>
        /// Falha: Valida se IDs são vazios (ArgumentException).
        /// </summary>
        [Test]
        [Category("Validation")]
        public void AddAccessoryToRentalAsync_EmptyGuids_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _service.AddAccessoryToRentalAsync(Guid.Empty, Guid.NewGuid()));
        }

        /// <summary>
        /// Falha: Impede vínculo se a locação estiver cancelada.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_RentalCanceled_ThrowsInvalidOperationException()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var accessoryId = Guid.NewGuid();
            var rental = new TbRental { Status = RentalStatus.canceled.ToString() }; // usar o mesmo valor do enum
            var accessory = new TbAccessory { Id = accessoryId, DailyRate = 10m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(accessoryId)).ReturnsAsync(accessory);
            // Setups extras para não falhar em chamadas posteriores (mesmo que exceção ocorra antes)
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(rentalId, accessoryId)).ReturnsAsync(false);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAccessoryToRentalAsync(rentalId, accessoryId));
            Assert.That(ex.Message, Is.EqualTo("Não é possível atribuir acessórios a uma locação cancelada."));
            _accessoryRepositoryMock.Verify(r => r.LinkToRentalAsync(rentalId, accessoryId), Times.Never);
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Never);
        }

        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_ValidRentalAndAccessory_UpdatesTotalAndLinksCorrectly()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var accessoryId = Guid.NewGuid();
            var startDate = DateTime.Now.Date;
            var endDate = startDate.AddDays(2);
            var rental = new TbRental { Id = rentalId, StartDate = startDate, ExpectedEndDate = endDate, TotalAmount = 100m, Status = "active" };
            var accessory = new TbAccessory { Id = accessoryId, DailyRate = 20m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(accessoryId)).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(rentalId, accessoryId)).ReturnsAsync(false);

            // Act
            await _service.AddAccessoryToRentalAsync(rentalId, accessoryId);

            // Assert
            var expectedAdd = 20m * 2; // 40
            Assert.That(rental.TotalAmount, Is.EqualTo(140m));
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(rental), Times.Once);
            _accessoryRepositoryMock.Verify(r => r.LinkToRentalAsync(rentalId, accessoryId), Times.Once);
        }

        [Test]
        [Category("BusinessRule")]
        public async Task RemoveAccessoryFromRentalAsync_ValidLink_DecreasesTotalAndRemovesLink()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var accessoryId = Guid.NewGuid();
            var startDate = DateTime.Now.Date;
            var endDate = startDate.AddDays(3);
            var rental = new TbRental { Id = rentalId, StartDate = startDate, ExpectedEndDate = endDate, TotalAmount = 200m };
            var accessory = new TbAccessory { Id = accessoryId, DailyRate = 30m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(accessoryId)).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(rentalId, accessoryId)).ReturnsAsync(true);

            // Act
            await _service.RemoveAccessoryFromRentalAsync(rentalId, accessoryId);

            // Assert
            var expectedSubtract = 30m * 3; // 90
            Assert.That(rental.TotalAmount, Is.EqualTo(110m));
            _accessoryRepositoryMock.Verify(r => r.RemoveLinkAsync(rentalId, accessoryId), Times.Once);
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(rental), Times.Once);
        }

        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_DaysGreaterThanZero_CallsLinkAndUpdateAndUpdatesTotal()
        {
            // Arrange - força days > 0
            var start = DateTime.Now.Date;
            var end = start.AddDays(3);
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 50m, Status = "active" };
            var accessory = new TbAccessory { DailyRate = 10m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);
            _accessoryRepositoryMock.Setup(r => r.LinkToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
            _rentalRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TbRental>())).Returns(Task.CompletedTask);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - 50 + (10 * 3) = 80
            Assert.That(rental.TotalAmount, Is.EqualTo(80m));
            _accessoryRepositoryMock.Verify(r => r.LinkToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }

        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_DatesEqual_UsesOneDayAndCallsLinkAndUpdate()
        {
            // Arrange - força days == 0
            var date = DateTime.Now.Date;
            var rental = new TbRental { StartDate = date, ExpectedEndDate = date, TotalAmount = 0m, Status = "active" };
            var accessory = new TbAccessory { DailyRate = 20m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);
            _accessoryRepositoryMock.Setup(r => r.LinkToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
            _rentalRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TbRental>())).Returns(Task.CompletedTask);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - 0 + (20 * 1) = 20
            Assert.That(rental.TotalAmount, Is.EqualTo(20m));
            _accessoryRepositoryMock.Verify(r => r.LinkToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }



        /// <summary>
        /// Falha: Verifica se AddAccessoryToRentalAsync lança a exceção correta quando a locação é nula.
        /// </summary>
        [Test]
        [Category("Validation")]
        public void AddAccessoryToRentalAsync_RentalNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TbRental)null!);

            // Act & Assert
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid()));

            Assert.That(ex.Message, Is.EqualTo("Locação não encontrada."));
        }

        /// <summary>
        /// Falha: Verifica se AddAccessoryToRentalAsync lança KeyNotFoundException quando a locação informada não existe.
        /// </summary>
        [Test]
        [Category("Validation")]
        public void AddAccessoryToRentalAsync_RentalIdNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync((TbRental)null!);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AddAccessoryToRentalAsync(rentalId, Guid.NewGuid()));
        }

        /// <summary>
        /// Falha: Impede vincular o mesmo acessório duas vezes na mesma locação.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public void AddAccessoryToRentalAsync_AlreadyLinked_ThrowsInvalidOperationException()
        {
            var rentalId = Guid.NewGuid();
            var accessoryId = Guid.NewGuid();
            var rental = new TbRental { Status = "Active" };
            var accessory = new TbAccessory { Id = accessoryId };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(accessoryId)).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(rentalId, accessoryId)).ReturnsAsync(true);

            Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAccessoryToRentalAsync(rentalId, accessoryId));
        }

        /// <summary>
        /// Sucesso: Verifica se o cálculo de valor total considera pelo menos 1 dia se as datas forem iguais.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_SameDayDates_CalculatesMinimumOneDay()
        {
            var today = DateTime.Now.Date;
            var rental = new TbRental { StartDate = today, ExpectedEndDate = today, TotalAmount = 0 };
            var accessory = new TbAccessory { DailyRate = 50m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);

            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.That(rental.TotalAmount, Is.EqualTo(50m)); // 50 * 1 dia
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }

        #endregion

        #region Remove Accessory Logic (Success & Failure)

        /// <summary>
        /// Sucesso: Cobre o cenário de remoção com período positivo, garantindo que o ramo 'else' da validação de dias seja executado.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task RemoveAccessoryFromRentalAsync_DaysGreaterThanZero_DecreasesTotalCorrectly()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var end = start.AddDays(2); // 2 dias de diferença
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 200m };
            var accessory = new TbAccessory { Id = Guid.NewGuid(), DailyRate = 50m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            await _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), accessory.Id);

            // Assert - Redução de (50 * 2 dias) = 100. Total final: 200 - 100 = 100.
            Assert.That(rental.TotalAmount, Is.EqualTo(100m));
        }

        /// <summary>
        /// Sucesso: Garante cobertura do ramo onde a diferença de dias é maior que zero no método de remoção.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task RemoveAccessoryFromRentalAsync_DaysGreaterThanZero_CalculatesReductionCorrectly()
        {
            // Arrange
            var startDate = DateTime.Now.Date;
            var endDate = startDate.AddDays(4); // 4 dias de diferença
            var rental = new TbRental { StartDate = startDate, ExpectedEndDate = endDate, TotalAmount = 100m };
            var accessory = new TbAccessory { DailyRate = 10m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            await _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - Redução deve ser 10 * 4 = 40. Total final: 100 - 40 = 60.
            Assert.That(rental.TotalAmount, Is.EqualTo(60m));
        }

        /// <summary>
        /// Sucesso: Cobre a ramificação onde 'days' é maior que zero no método de remoção.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task RemoveAccessoryFromRentalAsync_MultipleDays_ExecutesCorrectCalculation()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var end = start.AddDays(5); // 5 dias (força days > 0)
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 200m };
            var accessory = new TbAccessory { Id = Guid.NewGuid(), DailyRate = 10m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            await _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), accessory.Id);

            // Assert - Redução de 10 * 5 = 50. Total: 200 - 50 = 150.
            Assert.That(rental.TotalAmount, Is.EqualTo(150m));
        }

        /// <summary>
        /// Sucesso: Cobre a ramificação onde 'days' é maior que zero para garantir cobertura total da lógica de datas.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task RemoveAccessoryFromRentalAsync_PositivePeriod_ExecutesNormalCalculation()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var end = start.AddDays(3); // 3 dias de diferença
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 300m };
            var accessory = new TbAccessory { Id = Guid.NewGuid(), DailyRate = 50m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            await _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), accessory.Id);

            // Assert - Redução de 50 * 3 = 150. Total: 300 - 150 = 150.
            Assert.That(rental.TotalAmount, Is.EqualTo(150m));
        }

        /// <summary>
        /// Sucesso: Garante cobertura do ramo onde a diferença de dias é positiva (maior que zero).
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task RemoveAccessoryFromRentalAsync_PositiveDays_CalculatesCorrectReduction()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var end = start.AddDays(2); // Diferença de 2 dias (cai no else implícito do if days <= 0)
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 100m };
            var accessory = new TbAccessory { Id = Guid.NewGuid(), DailyRate = 10m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            await _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), accessory.Id);

            // Assert - (10 * 2 dias) = 20 de redução. Total: 100 - 20 = 80.
            Assert.That(rental.TotalAmount, Is.EqualTo(80m));
        }

        /// <summary>
        /// Falha: RemoveAccessoryFromRentalAsync deve lançar KeyNotFoundException se a locação não for encontrada.
        /// </summary>
        [Test]
        [Category("Validation")]
        public void RemoveAccessoryFromRentalAsync_RentalNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TbRental?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        /// <summary>
        /// Falha: RemoveAccessoryFromRentalAsync deve lançar KeyNotFoundException se o acessório não for encontrado.
        /// </summary>
        [Test]
        [Category("Validation")]
        public void RemoveAccessoryFromRentalAsync_AccessoryNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new TbRental());
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TbAccessory?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), Guid.NewGuid()));
        }



        /// <summary>
        /// Falha: RemoveAccessory deve lançar erro se o acessório não estiver vinculado.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public void RemoveAccessoryFromRentalAsync_NotLinked_ThrowsKeyNotFoundException()
        {
            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new TbRental());
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new TbAccessory());
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), Guid.NewGuid()));
            _accessoryRepositoryMock.Verify(r => r.RemoveLinkAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        /// <summary>
        /// Sucesso: Verifica se o valor total da locação é reduzido corretamente na remoção.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task RemoveAccessoryFromRentalAsync_ValidData_DecreasesTotalAmount()
        {
            var start = DateTime.Now.Date;
            var rental = new TbRental { StartDate = start, ExpectedEndDate = start.AddDays(2), TotalAmount = 200m };
            var accessory = new TbAccessory { DailyRate = 50m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

            await _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            var days = (rental.ExpectedEndDate.Date - rental.StartDate.Date).Days;
            if (days <= 0) days = 1;
            var expected = 200m - (accessory.DailyRate * days);
            Assert.That(rental.TotalAmount, Is.EqualTo(expected));
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(rental), Times.Once);
        }

        [Test]
        [Category("BusinessRule")]
        public async Task RemoveAccessoryFromRentalAsync_DatesAreEqual_UsesOneDayMinimum()
        {
            // Arrange: força days == 0 para executar o ramo days <= 0
            var date = DateTime.Now.Date;
            var rental = new TbRental { StartDate = date, ExpectedEndDate = date, TotalAmount = 100m };
            var accessory = new TbAccessory { DailyRate = 20m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);
            _accessoryRepositoryMock.Setup(r => r.RemoveLinkAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
            _rentalRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TbRental>())).Returns(Task.CompletedTask);

            // Act
            await _service.RemoveAccessoryFromRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - deve subtrair 1 dia * dailyRate
            Assert.That(rental.TotalAmount, Is.EqualTo(80m)); // 100 - (20 * 1)
            _accessoryRepositoryMock.Verify(r => r.RemoveLinkAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }



        /// <summary>
        /// Sucesso: Cobre a ramificação onde o período é maior que zero no cálculo de acréscimo de valor.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_PositivePeriod_UpdatesTotalCorrectly()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var end = start.AddDays(3); // 3 dias de diferença (cobre o ramo 'false' do if days <= 0)
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 100m };
            var accessory = new TbAccessory { DailyRate = 20.0m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - 100 (original) + (20 * 3 dias) = 160
            Assert.That(rental.TotalAmount, Is.EqualTo(160m));
        }

        /// <summary>
        /// Sucesso: Garante cobertura total da ramificação de dias ao fornecer um período positivo (maior que zero).
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_PositivePeriod_CalculatesAndUpdatesTotal()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var end = start.AddDays(4); // 4 dias de diferença
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 100m };
            var accessory = new TbAccessory { DailyRate = 10m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - (10 * 4 dias) + 100 = 140
            Assert.That(rental.TotalAmount, Is.EqualTo(140m));
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(rental), Times.Once);
        }

        /// <summary>
        /// Sucesso: Verifica se o valor total da locação é incrementado corretamente ao adicionar um acessório.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_ValidData_UpdatesRentalTotalAmount()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var rental = new TbRental { StartDate = start, ExpectedEndDate = start.AddDays(2), TotalAmount = 100m };
            var accessory = new TbAccessory { DailyRate = 50m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - calcula o esperado a partir do próprio rental
            var days = (rental.ExpectedEndDate.Date - rental.StartDate.Date).Days;
            if (days <= 0) days = 1;
            var expected = 100m + (accessory.DailyRate * days);
            Assert.That(rental.TotalAmount, Is.EqualTo(expected));
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(rental), Times.Once);
        }

        /// <summary>
        /// Falha: AddAccessoryToRentalAsync deve lançar KeyNotFoundException se o acessório não existir.
        /// </summary>
        [Test]
        [Category("Validation")]
        public void AddAccessoryToRentalAsync_AccessoryNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(new TbRental());
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TbAccessory?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AddAccessoryToRentalAsync(rentalId, Guid.NewGuid()));
        }




        /// <summary>
        /// Sucesso: Garante cobertura da ramificação onde a diferença de dias é zero, forçando o cálculo para 1 dia.
        /// </summary>
        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_DatesAreEqual_UsesOneDayMinimum()
        {
            // Arrange
            var date = DateTime.Now.Date;
            var rental = new TbRental { StartDate = date, ExpectedEndDate = date, TotalAmount = 50m };
            var accessory = new TbAccessory { DailyRate = 20m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);
            _accessoryRepositoryMock.Setup(r => r.LinkToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
            _rentalRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TbRental>())).Returns(Task.CompletedTask);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert - Deve somar 20m (1 dia) ao total original
            Assert.That(rental.TotalAmount, Is.EqualTo(70m));
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }

        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_ExpectedEndGreaterThanStart_UsesDaysAndUpdatesTotal()
        {
            // Arrange
            var start = DateTime.Now.Date;
            var end = start.AddDays(2);
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 10m };
            var accessory = new TbAccessory { DailyRate = 15m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);
            _accessoryRepositoryMock.Setup(r => r.LinkToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
            _rentalRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TbRental>())).Returns(Task.CompletedTask);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // 10 + (15 * 2) = 40
            Assert.That(rental.TotalAmount, Is.EqualTo(40m));
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }
        
        [Test]
        [Category("BusinessRule")]
        public async Task AddAccessoryToRentalAsync_EndBeforeStart_UsesOneDayAndUpdatesTotal()
        {
            // Arrange: ExpectedEndDate anterior a StartDate deve ser tratado como 1 dia
            var start = DateTime.Now.Date;
            var end = start.AddDays(-1);
            var rental = new TbRental { StartDate = start, ExpectedEndDate = end, TotalAmount = 5m };
            var accessory = new TbAccessory { DailyRate = 8m };

            _rentalRepositoryMock.Setup(r => r.GetRentalByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rental);
            _accessoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(accessory);
            _accessoryRepositoryMock.Setup(r => r.IsLinkedToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);
            _accessoryRepositoryMock.Setup(r => r.LinkToRentalAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
            _rentalRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TbRental>())).Returns(Task.CompletedTask);

            // Act
            await _service.AddAccessoryToRentalAsync(Guid.NewGuid(), Guid.NewGuid());

            // 5 + (8 * 1) = 13
            Assert.That(rental.TotalAmount, Is.EqualTo(13m));
            _rentalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }

        #endregion
    }
}