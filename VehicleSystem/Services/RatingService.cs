using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleRentalSystem.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IRentalRepository _rentalRepository;

        public RatingService(IRatingRepository ratingRepository, IRentalRepository rentalRepository)
        {
            _ratingRepository = ratingRepository;
            _rentalRepository = rentalRepository;
        }

        public async Task<bool> EvaluateRentalAsync(RatingCreateDTO dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                throw new Exception("The rating must be between 1 and 5.");
            }

            var rental = await _rentalRepository.GetRentalByIdAsync(dto.RentalId);
            if (rental == null)
            {
                throw new Exception("Rental not found.");
            }

            if (rental.Status != RentalStatus.completed.ToString()) 
            {
                throw new Exception("You can only rate rentals that have been completed.");
            }

            var existingRating = await _ratingRepository.GetByRentalIdAsync(dto.RentalId);
            if (existingRating != null)
            {
                throw new Exception("This rental has already been rated.");
            }

            var newRating = new TbRating
            {
                Id = Guid.NewGuid(),
                RentalId = dto.RentalId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _ratingRepository.AddAsync(newRating);
            await _ratingRepository.SaveChangesAsync();

            return true;
        }
    }
}