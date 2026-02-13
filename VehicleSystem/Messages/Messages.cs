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

        public const string RentalIdRequired =
            "O identificador da locação é obrigatório.";

        public const string RentalNotFound =
            "Locação não encontrada.";

        public const string RentalNotActive =
            "Não é possível devolver uma locação que não esteja ativa.";

        public const string VehicleBrandRequired =
            "A marca do veículo é obrigatória.";

        public const string VehicleModelRequired =
            "O modelo do veículo é obrigatório.";

        public const string VehicleYearInvalid =
            "O ano do veículo é inválido.";

        public const string VehicleDailyRateInvalid =
            "O valor da diária deve ser maior que zero.";

        public const string VehicleLicensePlateRequired =
            "A placa do veículo é obrigatória.";

        public const string VehicleLicensePlateAlreadyExists =
            "Já existe um veículo cadastrado com essa placa.";

        public const string VehicleCannotBeDeletedWhenRented =
            "Não é possível remover um veículo com status 'rented'.";

        public const string VehicleDeleteFailed =
            "Falha ao remover o veículo.";
    }
}
