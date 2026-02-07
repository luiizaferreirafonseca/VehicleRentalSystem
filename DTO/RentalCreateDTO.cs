namespace VehicleRentalSystem.DTO
{
    namespace VehicleRentalSystem.DTO
    {
        public class RentalCreateDTO
        {
            public Guid UserId { get; set; }
            public Guid VehicleId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime ExpectedEndDate { get; set; }
        }
    }

}
