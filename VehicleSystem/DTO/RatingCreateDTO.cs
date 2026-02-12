namespace VehicleRentalSystem.DTO // Adicionei o "Rental" aqui
{
    public class RatingCreateDTO
    {
        public Guid RentalId { get; set; } // Mudamos para Guid
        public int Rating { get; set; }    // Mudamos de Score para Rating
        public string? Comment { get; set; } 
    }
}
