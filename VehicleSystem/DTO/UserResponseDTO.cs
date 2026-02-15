namespace VehicleRentalSystem.DTO
{
    public class UserResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool? Active { get; set; }
        public List<UserRentalItemDTO> Rentals { get; set; } = new();
    }
}
