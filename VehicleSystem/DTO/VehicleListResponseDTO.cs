namespace VehicleRentalSystem.DTO
{
    public class VehicleListResponseDTO
    {
        public List<VehicleResponseDTO> Vehicles { get; set; } = new List<VehicleResponseDTO>();
        public string? Message { get; set; }
    }
}
