using System.ComponentModel.DataAnnotations;

namespace VehicleRentalSystem.DTO
{
    public class RentalCreateDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime ExpectedEndDate { get; set; }
    }
}
