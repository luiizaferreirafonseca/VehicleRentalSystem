using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories;
using System;
using System.Threading.Tasks;

namespace VehicleSystem.Tests.Repositories
{
    [TestFixture]
    public class RatingRepositoryTests
    {
        private PostgresContext _context;
        private RatingRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<PostgresContext>()
                .UseInMemoryDatabase(databaseName: "RatingDb_" + Guid.NewGuid().ToString())
                .Options;
            _context = new PostgresContext(options);
            _repository = new RatingRepository(_context);
        }

        [Test]
        public async Task AddAsync_ShouldPersistRating_WhenValid()
        {
            var rating = new TbRating 
            { 
                Id = Guid.NewGuid(), 
                Rating = 5, 
                Comment = "Excellent", 
                RentalId = Guid.NewGuid() 
            };
            
            await _repository.AddAsync(rating);
            var result = await _context.TbRatings.FindAsync(rating.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rating, Is.EqualTo(5));
        }

        [TearDown]
        public void TearDown() => _context.Dispose();
    }
}