namespace VehicleRentalSystem.Resources
{
    public static class Messages
    {
        public const string UserIdRequired =
            "The user identifier is required.";

        public const string VehicleIdRequired =
            "The vehicle identifier is required.";

        public const string ExpectedEndDateRequired =
            "The rental end date is required.";

        public const string ExpectedEndDateInvalid =
            "The rental end date must be greater than the start date.";

        public const string UserNotFound =
            "User not found.";

        public const string VehicleNotFound =
            "Vehicle not found.";

        public const string VehicleNotAvailable =
            "Vehicle is not available for rental.";

        public const string VehicleStatusUpdateFailed =
            "Failed to update vehicle status.";

        public const string RentalIdRequired =
            "The rental identifier is required.";

        public const string RentalNotFound =
            "Rental not found.";

        public const string RentalNotActive =
            "It is not possible to return a rental that is not active.";

        public const string VehicleBrandRequired =
            "Vehicle brand is required.";

        public const string VehicleModelRequired =
            "Vehicle model is required.";

        public const string VehicleYearInvalid =
            "Vehicle year is invalid.";

        public const string VehicleDailyRateInvalid =
            "The daily rate must be greater than zero.";

        public const string VehicleLicensePlateRequired =
            "Vehicle license plate is required.";

        public const string VehicleLicensePlateAlreadyExists =
            "A vehicle with this license plate already exists.";

        public const string VehicleCannotBeDeletedWhenRented =
            "It is not possible to remove a vehicle with status 'rented'.";

        public const string VehicleDeleteFailed =
            "Failed to delete vehicle.";

        public const string UserNameMissing =
            "There is a registered user without a name.";

        public const string UserEmailMissing =
            "There is a registered user without an email.";

        public const string PageInvalid =
            "Page must be greater than or equal to 1.";

        public const string PaymentRegisteredSuccess =
            "Payment registered successfully.";

        public const string PaymentRegisterFailed =
            "Failed to register payment.";

        public const string AccessoryLinkedSuccess =
            "Accessory successfully linked to rental.";

        public const string AccessoryUnlinkedSuccess =
            "Accessory successfully unlinked from rental.";

        // Report/export specific messages
        public const string InvalidFormat = "Invalid format.";
        public const string ReportNotFound = "Report not found.";
        public const string ReportNotFoundDetailFormat = "Report {0} not found.";

        // Generic/controller-level messages for standardization
        public const string InvalidOperation = "Invalid operation.";
        public const string ServerError = "Server error.";
        public const string NotFound = "Not found";
        public const string Conflict = "Conflict";
        public const string ServerInternalError = "Internal server error";
        public const string UnexpectedServerErrorDetail = "An unexpected error occurred while processing your request.";
        public const string RequestInvalid = "Invalid request";
        public const string RequestBodyEmptyDetail = "The request body is empty or invalid.";
        public const string IdsInvalid = "Invalid identifier";

        public const string InvalidStatus = "Invalid status.";
    }
}
