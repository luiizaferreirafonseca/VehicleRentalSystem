namespace VehicleRentalSystem.Resources
{
    public static class Messages
    {
        public const string UserIdRequired =
            "UserId é obrigatório.";

        public const string VehicleIdRequired =
            "VehicleId é obrigatório.";

        public const string ExpectedEndDateRequired =
            "ExpectedEndDate é obrigatório.";

        public const string ExpectedEndDateInvalid =
            "ExpectedEndDate deve ser maior que StartDate.";

        public const string UserNotFound =
            "Usuário não encontrado.";

        public const string VehicleNotFound =
            "Veículo não encontrado.";

        public const string VehicleNotAvailable =
            "Veículo não está disponível para locação.";

        public const string VehicleStatusUpdateFailed =
            "Falha ao atualizar o status do veículo.";
    }
}
