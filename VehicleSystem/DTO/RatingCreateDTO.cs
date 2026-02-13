namespace VehicleRentalSystem.DTO // Adicionei o "Rental" aqui
{
    public class RatingCreateDTO
    {
        public Guid RentalId { get; set; } 
        public int Rating { get; set; }    
        public string? Comment { get; set; } 
    }
}
