namespace VehicleRentalSystem.DTO 
{
    public class RatingCreateDTO
    {
        public Guid RentalId { get; set; } 
        public int Rating { get; set; }    
        public string? Comment { get; set; } 
    }
}
