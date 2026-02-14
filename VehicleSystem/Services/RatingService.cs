using VehicleRentalSystem.DTO;
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
                throw new Exception("A nota deve estar entre 1 e 5.");
            }

            var rental = await _rentalRepository.GetRentalByIdAsync(dto.RentalId);
            if (rental == null)
            {
                throw new Exception("Locação não encontrada.");
            }

            if (rental.Status != "Finalizada")
            {
                throw new Exception("Você só pode avaliar locações que já foram finalizadas.");
            }

            var existingRating = await _ratingRepository.GetByRentalIdAsync(dto.RentalId);
            if (existingRating != null)
            {
                throw new Exception("Esta locação já foi avaliada anteriormente.");
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