using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services.interfaces
{
    public interface IRatingService
    {
        Task<bool> EvaluateRentalAsync(RatingCreateDTO dto);
    }
}