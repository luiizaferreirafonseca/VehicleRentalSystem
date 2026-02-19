using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_SistemaLocacao.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.Controllers;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleSystem.Tests.Controllers
{
    [TestFixture]
    public class RentalControllerTests
    {
        private Mock<IRentalService> _service;
        private RentalController _controller;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IRentalService>();
            _controller = new RentalController(_service.Object);
        }

        // --- TESTES DAS MENINAS (MANTIDOS E TRADUZIDOS) ---

        [Test]
        public async Task Create_ShouldReturn_400BadRequest_WhenModelStateIsInvalid()
        {
            var dto = new RentalCreateDTO();
            _controller.ModelState.AddModelError("UserId", "Required");

            var result = await _controller.Create(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Create_ShouldReturn_201CreatedAtAction_WhenRentalCreatedSuccessfully()
        {
            var createdId = Guid.NewGuid();
            var dto = new RentalCreateDTO { UserId = Guid.NewGuid(), VehicleId = Guid.NewGuid() };
            var created = new RentalResponseDTO { Id = createdId, UserId = dto.UserId };

            _service.Setup(s => s.CreateRentalAsync(dto)).ReturnsAsync(created);

            var result = await _controller.Create(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
                var createdAt = result as CreatedAtActionResult;
                Assert.That(createdAt!.ActionName, Is.EqualTo("GetById"));
            });
        }

        [Test]
        public async Task Create_ShouldReturn_404NotFound_WhenKeyNotFoundExceptionOccurs()
        {
            var dto = new RentalCreateDTO { UserId = Guid.NewGuid() };
            _service.Setup(s => s.CreateRentalAsync(dto))
                    .ThrowsAsync(new KeyNotFoundException("User not found"));

            var result = await _controller.Create(dto);

            var notFound = result as NotFoundObjectResult;
            var problem = notFound!.Value as ProblemDetails;
            Assert.That(problem!.Title, Is.EqualTo("No encontrado")); 
            Assert.That(problem.Detail, Is.EqualTo("User not found"));
        }

        [Test]
        public async Task Return_ShouldReturn_200Ok_WhenReturnIsSuccessful()
        {
            var rentalId = Guid.NewGuid();
            var dto = new RentalResponseDTO { Id = rentalId, Status = "Completed" };

            _service.Setup(s => s.ReturnRentalAsync(rentalId)).ReturnsAsync(dto);

            var result = await _controller.Return(rentalId);
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task Search_ShouldReturn_200Ok_WhenSearchIsSuccessful()
        {
            var userId = Guid.NewGuid();
            var fakeList = new List<RentalResponseDTO> { new RentalResponseDTO { UserId = userId } };
            _service.Setup(s => s.SearchRentalsByUserAsync(userId, "Active", 1)).ReturnsAsync(fakeList);

            var result = await _controller.Search(userId, "Active", 1);
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task Cancel_ShouldReturn_200Ok_WhenCancelIsSuccessful()
        {
            var rentalId = Guid.NewGuid();
            var response = new RentalResponseDTO { Id = rentalId, Status = "Canceled" };

            _service.Setup(s => s.CancelRentalAsync(rentalId)).ReturnsAsync(response);

            var result = await _controller.Cancel(rentalId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<OkObjectResult>());
                var ok = result as OkObjectResult;
                var body = ok!.Value as RentalResponseDTO;
                Assert.That(body!.Status, Is.EqualTo("Canceled"));
            });
        }

        [Test]
        public async Task Cancel_ShouldReturn_404NotFound_WhenRentalDoesNotExist()
        {
            var rentalId = Guid.NewGuid();
            _service.Setup(s => s.CancelRentalAsync(rentalId))
                    .ThrowsAsync(new KeyNotFoundException("Rental not found"));

            var result = await _controller.Cancel(rentalId);

            var notFound = result as NotFoundObjectResult;
            var problem = notFound!.Value as ProblemDetails;
            Assert.That(problem!.Title, Is.EqualTo("Erro!! Locação não encontrada"));
        }

        [Test]
        public async Task UpdateDates_ShouldReturn_200Ok_WhenUpdateIsSuccessful()
        {
            var rentalId = Guid.NewGuid();
            // AJUSTE: Usando NewExpectedEndDate conforme seu DTO
            var dto = new UpdateRentalDTO { NewExpectedEndDate = new DateTime(2026, 03, 01) };
            var response = new RentalResponseDTO { Id = rentalId, ExpectedEndDate = dto.NewExpectedEndDate };

            _service.Setup(s => s.UpdateRentalDatesAsync(rentalId, dto)).ReturnsAsync(response);

            var result = await _controller.UpdateDates(rentalId, dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateDates_ShouldReturn_400BadRequest_WhenRentalIsNotActive()
        {
            var rentalId = Guid.NewGuid();
            var dto = new UpdateRentalDTO { NewExpectedEndDate = new DateTime(2026, 03, 01) };

            _service.Setup(s => s.UpdateRentalDatesAsync(rentalId, dto))
                    .ThrowsAsync(new InvalidOperationException("Rental is not active"));

            var result = await _controller.UpdateDates(rentalId, dto);

            var badRequest = result as BadRequestObjectResult;
            var problem = badRequest!.Value as ProblemDetails;
            Assert.That(problem!.Title, Is.EqualTo("Operação Inválida"));
            Assert.That(problem.Detail, Is.EqualTo("Rental is not active"));
        }
    }
}