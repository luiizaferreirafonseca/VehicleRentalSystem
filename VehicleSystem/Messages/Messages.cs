namespace VehicleRentalSystem.Resources
{
    public static class Messages
    {
        public const string UserIdRequired =
            "O identificador do usuário é obrigatório.";

        public const string VehicleIdRequired =
            "O identificador do veículo é obrigatório.";

        public const string ExpectedEndDateRequired =
            "A data final da locação é obrigatória.";

        public const string ExpectedEndDateInvalid =
            "A data final da locação deve ser maior que a data inicial.";

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
