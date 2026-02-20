using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.Controllers;
using VehicleRentalSystem.Services.interfaces;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.DTO;
using System;
using System.Threading.Tasks;

namespace VehicleSystem.Tests.Controllers
{
    [TestFixture]
    public class RatingControllerTests
    {
        private Mock<IRatingService> _ratingServiceMock;
        private RatingsController _controller;

        [SetUp]
        public void Setup()
        {
            _ratingServiceMock = new Mock<IRatingService>();
            _controller = new RatingsController(_ratingServiceMock.Object);
        }

        [Test]
        public async Task PostRating_ShouldReturnOk_WhenRatingIsSuccessful()
        {
            var dto = new RatingCreateDTO { RentalId = Guid.NewGuid(), Rating = 5 };
            _ratingServiceMock.Setup(s => s.EvaluateRentalAsync(dto)).ReturnsAsync(true);

            var result = await _controller.PostRating(dto) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task PostRating_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            var dto = new RatingCreateDTO { RentalId = Guid.NewGuid(), Rating = 5 };
            _ratingServiceMock.Setup(s => s.EvaluateRentalAsync(dto))
                .ThrowsAsync(new Exception("Only completed rentals can be rated."));

            var result = await _controller.PostRating(dto) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task PostRating_ShouldReturnBadRequest_WithExpectedMessage_WhenExceptionIsThrown()
        {
            var dto = new RatingCreateDTO { RentalId = Guid.NewGuid(), Rating = 5 };
            _ratingServiceMock.Setup(s => s.EvaluateRentalAsync(It.IsAny<RatingCreateDTO>()))
                .ThrowsAsync(new Exception("error message"));

            var result = await _controller.PostRating(dto);

            var bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            Assert.That(bad!.StatusCode, Is.EqualTo(400));
            Assert.That(bad.Value, Is.Not.Null);

            var messageProp = bad.Value!.GetType().GetProperty("message");
            Assert.That(messageProp, Is.Not.Null);

            var message = messageProp!.GetValue(bad.Value) as string;
            Assert.That(message, Is.EqualTo("error message"));

            _ratingServiceMock.Verify(s => s.EvaluateRentalAsync(It.IsAny<RatingCreateDTO>()), Times.Once);
        }

        [Test]
        public async Task PostRating_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            _ratingServiceMock.Setup(s => s.EvaluateRentalAsync(null!))
                .ThrowsAsync(new Exception("dto is null"));

            var result = await _controller.PostRating(null!);

            var bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            Assert.That(bad!.StatusCode, Is.EqualTo(400));
        }
    }
}