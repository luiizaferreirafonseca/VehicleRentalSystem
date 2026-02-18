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

        public const string UserNameMissing =
            "Existe usuário cadastrado sem nome.";

        public const string UserEmailMissing =
            "Existe usuário cadastrado sem e-mail.";

        public const string PageInvalid =
            "A página deve ser maior ou igual a 1.";

        public const string PaymentRegisteredSuccess = 
            "Pagamento registrado com sucesso.";

        public const string PaymentRegisterFailed = 
            "Falha ao registrar o pagamento.";
        public const string AccessoryLinkedSuccess = 
            "Acessório vinculado com sucesso à locação.";

        public const string AccessoryUnlinkedSuccess = 
            "Acessório desvinculado com sucesso da locação.";

        // Mensagens específicas para relatórios/exports
        public const string InvalidFormat = "Formato inválido.";
        public const string ReportNotFound = "Relatório não encontrado.";
        public const string ReportNotFoundDetailFormat = "Relatório {0} não encontrado.";


        // Mensagens genéricas/situacionais para controllers adicionadas para padronização
        public const string InvalidOperation = "Operação inválida.";
        public const string ServerError = "Erro de servidor.";
        public const string NotFound = "Não encontrado";
        public const string Conflict = "Conflito";
        public const string ServerInternalError = "Erro interno do servidor";
        public const string UnexpectedServerErrorDetail = "Ocorreu um erro inesperado ao processar sua solicitação.";
        public const string RequestInvalid = "Requisição inválida";
        public const string RequestBodyEmptyDetail = "O corpo da requisição está vazio ou inválido.";
        public const string IdsInvalid = "Identificador inválido";

        public const string InvalidStatus = "Status inválido.";

    }
}
