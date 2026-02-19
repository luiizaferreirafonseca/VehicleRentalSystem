# Relatório de Testes — `[HttpGet("available")]`

> Camada coberta: **DTO · Service · Repository**
> Resultado: ✅ **26 testes passaram** — sem nenhuma falha

---

## Arquivos criados / modificados

| Arquivo | O que foi feito | Testes adicionados |
|---|---|---|
| `VehicleSystem.Tests/Services/VehicleServiceTests.cs` | Adicionada região `GetAvailableVehiclesAsync` | 5 |
| `VehicleSystem.Tests/Repositories/VehicleRepositoryTests.cs` | Criado do zero (InMemory EF) | 8 |
| `VehicleSystem.Tests/DTOs/VehicleListResponseDTOTests.cs` | Criado do zero | 6 |
| `VehicleSystem.Tests/Controllers/VehicleControllerTests.cs` | Adicionados testes do endpoint (sessão anterior) | 4 |

**Total de testes novos:** 23 · **Total executados no filtro:** 26

---

## Cobertura por camada

### 🗂 DTO — `VehicleListResponseDTO` / `VehicleResponseDTO`

| Teste | Comportamento verificado |
|---|---|
| `Default_VehiclesNaoEhNulo` | `Vehicles` nunca é `null` ao instanciar o DTO |
| `Default_VehiclesEhListaVazia` | `Vehicles` começa como lista vazia |
| `Default_MessageEhNula` | `Message` começa como `null` |
| `ComVeiculos_VehiclesContemElementos` | `Vehicles` aceita e retém elementos adicionados |
| `ComMensagem_MessageEhDefinida` | `Message` pode ser definida com valor string |
| `VehicleResponseDTO_PropriedadesMapeadas` | Todos os 7 campos (`Id`, `Brand`, `Model`, `Year`, `DailyRate`, `Status`, `LicensePlate`) são mapeados corretamente |

---

### ⚙️ Service — `VehicleService.GetAvailableVehiclesAsync`

| Teste | Comportamento verificado |
|---|---|
| `ComVeiculosDisponiveis_RetornaListaComVeiculos` | Retorna `VehicleListResponseDTO` com veículos e `Message = null` |
| `SemVeiculosDisponiveis_RetornaMensagemVazia` | Lista vazia retorna mensagem `"Não há veículos disponíveis para locação."` |
| `ChamaRepositorioComStatusAvailable` | Repositório é chamado exatamente com `"available"` |
| `PaginaCustomizada_PassaPaginaParaRepositorio` | Página customizada é repassada corretamente ao repositório |
| `MapeiaPropriedadesCorretamente` | Todos os campos do `TbVehicle` são mapeados para `VehicleResponseDTO` |

---

### 🗄 Repository — `VehicleRepository.SearchVehiclesAsync`

| Teste | Comportamento verificado |
|---|---|
| `ComStatusAvailable_RetornaApenasDisponiveis` | Filtra apenas veículos com `Status = "available"` |
| `ComStatusNulo_RetornaTodosVeiculos` | `status = null` retorna todos os veículos |
| `ComStatusVazio_RetornaTodosVeiculos` | `status = ""` retorna todos os veículos |
| `SemCorrespondencia_RetornaListaVazia` | Status sem match retorna lista vazia |
| `FiltroStatusCaseInsensitive` | Filtro ignora capitalização (`"AVAILABLE"` == `"available"`) |
| `ResultadoOrdenadoPorBrand` | Resultado sempre ordenado por `Brand` |
| `PaginaMenorQueUm_UsaPaginaUm` | Página `< 1` é normalizada para `1` |
| `Paginacao_Pagina2_RetornaProximosItens` | Página 2 retorna os próximos itens corretamente (pageSize = 5) |

---

## Estratégia de testes utilizada

```
Controller  →  Mock<IVehicleService>         (Moq)
Service     →  Mock<IVehicleRepository>      (Moq)
Repository  →  PostgresContext InMemory      (EF Core InMemory)
DTO         →  Instanciação direta           (sem dependências)
```

- Cada teste de repositório usa um banco InMemory com `Guid.NewGuid()` como nome para garantir isolamento total.
- `TearDown` chama `EnsureDeleted()` + `Dispose()` para limpar o estado entre testes.
- O `pageSize` fixo do repositório é `5` — os testes de paginação respeitam esse valor.

---

```
Resumo do teste: total: 26; falhou: 0; bem-sucedido: 26; ignorado: 0
```

---

# Relatório de Testes — Accessory (todas as camadas)

> Camadas cobertas: **DTO · Service · Repository · Controller**
> Resultado: ✅ **89 testes passaram** — sem nenhuma falha

---

## Arquivos de teste

| Arquivo | Testes |
|---|---|
| `VehicleSystem.Tests/Services/AccessoryServiceTests.cs` | 47 |
| `VehicleSystem.Tests/Controllers/AccessoryControllerTests.cs` | 27 |
| `VehicleSystem.Tests/Repositories/AccessoryRepositoryTests.cs` | 9 |
| `VehicleSystem.Tests/DTOs/AccessoryDtoTests.cs` | 6 |

---

## Cobertura por camada

### 🗂 DTO — `AccessoryCreateDto` / `AccessoryResponseDto` / `AccessoryReportDto`

| Teste | Comportamento verificado |
|---|---|
| `AccessoryCreateDto_Defaults_AreExpected` | `Name = ""` e `DailyRate = 0` ao instanciar |
| `AccessoryCreateDto_CanSetProperties` | Atribui e lê `Name` e `DailyRate` corretamente |
| `AccessoryResponseDto_Defaults_AreExpected` | `Id = Guid.Empty`, `Name = ""`, `DailyRate = 0` ao instanciar |
| `AccessoryResponseDto_CanSetProperties` | Mapeamento dos 3 campos (`Id`, `Name`, `DailyRate`) |
| `AccessoryReportDto_Defaults_AreExpected` | Todos os 8 campos têm valor padrão correto |
| `AccessoryReportDto_CanSetProperties` | Mapeamento completo incluindo `Quantity`, `UnitPrice`, `TotalPrice`, datas |

---

### ⚙️ Service — `AccessoryService`

#### GET
| Teste | Comportamento verificado |
|---|---|
| `GetAccessoriesAsync_NoAccessoriesFound_ReturnsEmptyList` | Repositório retorna `null` → serviço retorna lista vazia |
| `GetAccessoryByIdAsync_IdNotFound_ThrowsKeyNotFoundException` | ID inexistente lança `KeyNotFoundException` |
| `GetAccessoryByIdAsync_ValidId_ReturnsMappedDto` | Mapeamento de `Id`, `Name`, `DailyRate` para `AccessoryResponseDto` |
| `GetAccessoryByIdAsync_ExistingId_ReturnsCorrectMapping` | Campos mapeados corretamente (caso alternativo) |
| `GetAccessoryByIdAsync_ExistingId_ExecutesMapping` | Cobertura da linha de projeção `new AccessoryResponseDto` |
| `GetAccessoryByIdAsync_ExistingAccessory_ReturnsMappedDto` | Validação campo a campo do mapeamento |
| `GetAccessoriesByRentalIdAsync_RentalNotFound_ThrowsKeyNotFoundException` | Locação inexistente lança `KeyNotFoundException` |
| `GetAccessoriesByRentalIdAsync_ExistingAccessories_ReturnsMappedList` | Lista de 1 acessório mapeada para DTO |
| `GetAccessoriesByRentalIdAsync_WithAccessories_ReturnsMappedList` | Lista de 2 acessórios mapeada corretamente |
| `GetAccessoriesByRentalIdAsync_RepositoryReturnsNull_CoversNullBranch` | Repositório retorna `null` → resultado é vazio |
| `GetAccessoriesByRentalIdAsync_ValidRental_ReturnsMappedDtoList` | Locação válida retorna DTOs mapeados |

#### CREATE
| Teste | Comportamento verificado |
|---|---|
| `CreateAccessoryAsync_DuplicateName_ThrowsInvalidOperationException` | Nome duplicado lança `InvalidOperationException`; `AddAsync` nunca é chamado |
| `CreateAccessoryAsync_ValidDto_ExecutesInternalInstantiation` | `TbAccessory` criado com `Name`, `DailyRate` e `Id != Guid.Empty` |
| `CreateAccessoryAsync_ValidData_PersistsAndReturnsDto` | `AddAsync` chamado; `Id` retornado é válido |
| `CreateAccessoryAsync_ValidData_CoversEntityInstantiation` | Cobertura da linha `new TbAccessory { ... }` |
| `CreateAccessoryAsync_ValidData_ReturnsAccessoryResponseDto` | DTO retornado tem `Name`, `DailyRate` e `Id` corretos |
| `CreateAccessoryAsync_NewAccessory_CallsRepositoryWithCorrectData` | `AddAsync` recebe entidade com dados exatos do DTO |

#### ADD TO RENTAL
| Teste | Comportamento verificado |
|---|---|
| `AddAccessoryToRentalAsync_GuidsAreEmpty_ThrowsArgumentException` | Ambos os GUIDs vazios lançam `ArgumentException` |
| `AddAccessoryToRentalAsync_EmptyGuids_ThrowsArgumentException` | Somente `RentalId` vazio lança `ArgumentException` |
| `AddAccessoryToRentalAsync_RentalNotFound_ThrowsKeyNotFoundException` | Locação `null` lança `KeyNotFoundException` com mensagem correta |
| `AddAccessoryToRentalAsync_RentalIdNotFound_ThrowsKeyNotFoundException` | ID de locação inexistente lança `KeyNotFoundException` |
| `AddAccessoryToRentalAsync_AccessoryNotFound_ThrowsKeyNotFoundException` | Acessório inexistente lança `KeyNotFoundException` |
| `AddAccessoryToRentalAsync_RentalCanceled_ThrowsInvalidOperationException` | Locação cancelada bloqueia vínculo com mensagem correta |
| `AddAccessoryToRentalAsync_AlreadyLinked_ThrowsInvalidOperationException` | Vínculo duplo lança `InvalidOperationException` |
| `AddAccessoryToRentalAsync_ValidRentalAndAccessory_UpdatesTotalAndLinksCorrectly` | 2 dias × R$20 = R$140; `UpdateAsync` e `LinkToRentalAsync` chamados |
| `AddAccessoryToRentalAsync_MultipleDays_CalculatesTotalCorrectly` | 3 dias × R$10 = R$30 adicionados ao total |
| `AddAccessoryToRentalAsync_MultipleDays_CalculatesAndUpdatesTotal` | 5 dias × R$10; `UpdateAsync` chamado uma vez |
| `AddAccessoryToRentalAsync_DaysGreaterThanZero_CallsLinkAndUpdateAndUpdatesTotal` | 3 dias: total 50 + 30 = 80 |
| `AddAccessoryToRentalAsync_DatesEqual_UsesOneDayAndCallsLinkAndUpdate` | Datas iguais → mínimo 1 dia: 0 + 20 = 20 |
| `AddAccessoryToRentalAsync_SameDayDates_CalculatesMinimumOneDay` | Mesmo dia → 50 × 1 = 50 |
| `AddAccessoryToRentalAsync_DatesAreEqual_UsesOneDayMinimum` | Datas iguais → 50 + 20 = 70 |
| `AddAccessoryToRentalAsync_PositivePeriod_UpdatesTotalCorrectly` | 3 dias × R$20: 100 + 60 = 160 |
| `AddAccessoryToRentalAsync_PositivePeriod_CalculatesAndUpdatesTotal` | 4 dias × R$10: 100 + 40 = 140 |
| `AddAccessoryToRentalAsync_ValidData_UpdatesRentalTotalAmount` | Cálculo com `ExpectedEndDate - StartDate`; `UpdateAsync` chamado |
| `AddAccessoryToRentalAsync_ExpectedEndGreaterThanStart_UsesDaysAndUpdatesTotal` | 2 dias × R$15: 10 + 30 = 40 |
| `AddAccessoryToRentalAsync_EndBeforeStart_UsesOneDayAndUpdatesTotal` | Data final antes do início → 1 dia: 5 + 8 = 13 |

#### REMOVE FROM RENTAL
| Teste | Comportamento verificado |
|---|---|
| `RemoveAccessoryFromRentalAsync_RentalNotFound_ThrowsKeyNotFoundException` | Locação `null` lança `KeyNotFoundException` |
| `RemoveAccessoryFromRentalAsync_AccessoryNotFound_ThrowsKeyNotFoundException` | Acessório `null` lança `KeyNotFoundException` |
| `RemoveAccessoryFromRentalAsync_NotLinked_ThrowsKeyNotFoundException` | Vínculo inexistente lança `KeyNotFoundException`; `RemoveLinkAsync` nunca chamado |
| `RemoveAccessoryFromRentalAsync_ValidLink_DecreasesTotalAndRemovesLink` | 3 dias × R$30: 200 − 90 = 110; `RemoveLinkAsync` e `UpdateAsync` chamados |
| `RemoveAccessoryFromRentalAsync_ValidData_DecreasesTotalAmount` | Cálculo baseado em `ExpectedEndDate - StartDate`; `UpdateAsync` chamado |
| `RemoveAccessoryFromRentalAsync_DatesAreEqual_UsesOneDayMinimum` | Datas iguais → 1 dia: 100 − 20 = 80 |
| `RemoveAccessoryFromRentalAsync_DaysGreaterThanZero_DecreasesTotalCorrectly` | 2 dias × R$50: 200 − 100 = 100 |
| `RemoveAccessoryFromRentalAsync_DaysGreaterThanZero_CalculatesReductionCorrectly` | 4 dias × R$10: 100 − 40 = 60 |
| `RemoveAccessoryFromRentalAsync_MultipleDays_ExecutesCorrectCalculation` | 5 dias × R$10: 200 − 50 = 150 |
| `RemoveAccessoryFromRentalAsync_PositivePeriod_ExecutesNormalCalculation` | 3 dias × R$50: 300 − 150 = 150 |
| `RemoveAccessoryFromRentalAsync_PositiveDays_CalculatesCorrectReduction` | 2 dias × R$10: 100 − 20 = 80 |

---

### 🗄 Repository — `AccessoryRepository`

| Teste | Método coberto | Comportamento verificado |
|---|---|---|
| `AddAsync_AddsAccessory_ToDatabase` | `AddAsync` | Entidade adicionada ao `DbSet` com `Name = "GPS"` |
| `GetByNameAsync_ReturnsAccessory_WhenExists` | `GetByNameAsync` | Retorna acessório com `Id` e `Name` corretos |
| `GetByNameAsync_ReturnsNull_WhenNotFound` | `GetByNameAsync` | Retorna `null` quando nome não existe |
| `GetAllAsync_ReturnsAllAccessories` | `GetAllAsync` | Lista completa com os dois nomes esperados |
| `LinkToRentalAsync_CreatesRentalAccessory_WithCorrectPrices` | `LinkToRentalAsync` | `UnitPrice = DailyRate`, `TotalPrice = DailyRate`, `Quantity = 1` |
| `LinkToRentalAsync_CreatesRentalAccessory_WithZeroPrices_WhenAccessoryNotFound` | `LinkToRentalAsync` | Acessório não encontrado → preços `0m`, vínculo criado mesmo assim |
| `IsLinkedToRentalAsync_ReturnsTrueWhenLinked_AndFalseWhenNot` | `IsLinkedToRentalAsync` | `true` para vínculo existente; `false` para ID inexistente |
| `RemoveLinkAsync_RemovesExistingLink` | `RemoveLinkAsync` | Vínculo removido; lista fica vazia |
| `GetByRentalIdAsync_ReturnsAccessories_LinkedToRental` | `GetByRentalIdAsync` | Retorna os 2 acessórios vinculados à locação |

---

### 🎮 Controller — `AccessoryController`

#### `GET /accessories`
| Teste | Status esperado | Comportamento verificado |
|---|---|---|
| `Get_ShouldReturn_200OkWithEmptyList_WhenNoAccessoriesExist` | 200 | Lista vazia retornada sem erro |
| `Get_ShouldReturnList_WhenAccessoriesExist` | 200 | 2 acessórios retornados; primeiro é "GPS" |

#### `GET /accessories/{id}`
| Teste | Status esperado | Comportamento verificado |
|---|---|---|
| `GetById_ShouldReturn_200Ok_WhenExists` | 200 | ID válido retorna `OkObjectResult` |
| `GetById_ShouldReturn_400_WhenModelStateIsInvalid` | 400 | `ModelState` inválido bloqueia chamada ao serviço |
| `GetById_ShouldReturn_404NotFound_WhenIdDoesNotExist` | 404 | `KeyNotFoundException` → `ProblemDetails` com título correto |
| `GetById_ShouldReturn_409_WhenInvalidOperationExceptionOccurs` | 409 | `InvalidOperationException` → `ConflictObjectResult` |
| `GetById_ShouldReturn_500_WhenUnexpectedExceptionOccurs` | 500 | Exceção genérica → `ProblemDetails` com `ServerInternalError` |

#### `POST /accessories/add`
| Teste | Status esperado | Comportamento verificado |
|---|---|---|
| `Create_ShouldReturn_201Created_WhenAccessoryCreatedSuccessfully` | 201 | `CreatedAtActionResult` com `Id` correto |
| `Create_ShouldReturn_409Conflict_WhenNameIsDuplicate` | 409 | Nome duplicado → `ProblemDetails` com `Conflict` |
| `Create_ShouldReturn_400BadRequest_WhenModelStateIsInvalid` | 400 | `Name = ""` no `ModelState` bloqueia criação |
| `Create_ShouldReturn_500_WhenUnexpectedException` | 500 | Erro fatal → `StatusCode 500` |

#### `POST /accessories` (vínculo)
| Teste | Status esperado | Comportamento verificado |
|---|---|---|
| `AddAccessoryToRental_ShouldReturn_400BadRequest_WhenRequestIsNull` | 400 | Body `null` → `ProblemDetails` com `RequestInvalid` |
| `AddAccessoryToRental_ShouldReturn_400_WhenModelStateIsInvalid` | 400 | `ModelState` inválido bloqueia vínculo |
| `AddAccessoryToRental_ShouldReturn_400_WhenOnlyRentalIdIsEmpty` | 400 | `RentalId = Guid.Empty` → 400 |
| `AddAccessoryToRental_ShouldReturn_400_WhenOnlyAccessoryIdIsEmpty` | 400 | `AccessoryId = Guid.Empty` → 400 |
| `AddAccessoryToRental_ShouldReturn_200Ok_WhenRequestIsValid` | 200 | Vínculo criado; resposta contém `AccessoryLinkedSuccess`; serviço chamado 1x |
| `AddAccessoryToRental_ShouldReturn_404_AndLogWarning_WhenKeyNotFound` | 404 | `KeyNotFoundException` → 404 + `LogWarning` emitido |
| `AddAccessoryToRental_ShouldReturn_409_WhenInvalidOperation` | 409 | `InvalidOperationException` → `ConflictObjectResult` |
| `AddAccessoryToRental_ShouldReturn_500_AndLogError_WhenUnexpectedException` | 500 | Exceção genérica → `StatusCode 500` |

#### `GET /rental/{id}/accessories`
| Teste | Status esperado | Comportamento verificado |
|---|---|---|
| `GetAccessoriesByRental_ShouldReturnList_WhenAccessoriesExist` | 200 | Lista retornada com `OkObjectResult` |
| `GetAccessoriesByRental_ShouldReturn_404_WhenNotFound` | 404 | `KeyNotFoundException` → `NotFoundObjectResult` |
| `GetAccessoriesByRental_ShouldReturn_409_WhenInvalidOperation` | 409 | Conflito de negócio → `ConflictObjectResult` |
| `GetAccessoriesByRental_ShouldReturn_500_WhenUnexpectedException` | 500 | Erro interno → `StatusCode 500` |

#### `DELETE /rental/{rentalId}/accessories/{accessoryId}`
| Teste | Status esperado | Comportamento verificado |
|---|---|---|
| `RemoveAccessoryFromRental_ShouldReturn_200Ok` | 200 | Vínculo removido; serviço chamado 1x |
| `RemoveAccessoryFromRental_ShouldReturn_404_WhenKeyNotFound` | 404 | `KeyNotFoundException` → `NotFoundObjectResult` |
| `RemoveAccessoryFromRental_ShouldReturn_409_WhenInvalidOperation` | 409 | Conflito → `ConflictObjectResult` |
| `RemoveAccessoryFromRental_ShouldReturn_500_WhenUnexpectedException` | 500 | Exceção genérica → `StatusCode 500` |

---

## Estratégia de testes utilizada

```
Controller  →  Mock<IAccessoryService>            (Moq)
Service     →  Mock<IAccessoryRepository>
               Mock<IRentalRepository>             (Moq)
Repository  →  Mock<PostgresContext> + DbSet mock  (Moq + TestAsyncQueryProvider)
DTO         →  Instanciação direta                 (sem dependências)
```

- O repositório usa `Mock<PostgresContext>` com `TestAsyncQueryProvider<T>` customizado para suportar `ToListAsync` / `AnyAsync` / `FirstOrDefaultAsync` em listas em memória.
- `IsLinkedToRentalAsync` e `RemoveLinkAsync` verificam vínculos via `TbRentalAccessory` mockado.
- Testes de cálculo de dias cobrem os dois ramos do `if (days <= 0) days = 1`: datas iguais (mínimo 1 dia) e diferença positiva.

---

```
Aprovado! — Com falha: 0, Aprovado: 89, Ignorado: 0, Total: 89
```

---

# Relatório de Testes — Payment (todas as camadas)

> Camadas cobertas: **Service · Repository · Controller**
> Resultado: ✅ **20 testes passaram** — sem nenhuma falha

---

## Arquivos de teste

| Arquivo | Testes |
|---|---|
| `VehicleSystem.Tests/Services/PaymentServiceTests.cs` | 8 |
| `VehicleSystem.Tests/Controllers/PaymentControllerTests.cs` | 7 |
| `VehicleSystem.Tests/Repositories/PaymentRepositoryTests.cs` | 5 |

> ℹ️ Não há arquivo de testes de DTO para Payment — os DTOs (`PaymentCreateDTO`, `PaymentResponseDto`) são cobertos indiretamente pelos testes de Service e Controller.

---

## Cobertura por camada

### ⚙️ Service — `PaymentService`

#### `RegisterPaymentAsync`

| Teste | Categoria | Comportamento verificado |
|---|---|---|
| `RegisterPaymentAsync_RentalIdEmpty_ThrowsArgumentException` | Validation | `RentalId = Guid.Empty` lança `ArgumentException` com mensagem contendo `"identificador da locação"` |
| `RegisterPaymentAsync_RentalNotFound_ThrowsKeyNotFoundException` | Validation | Locação não encontrada lança `KeyNotFoundException`; `AddPaymentAsync` nunca chamado |
| `RegisterPaymentAsync_RentalCanceled_ThrowsInvalidOperationException` | BusinessRule | Locação cancelada lança `InvalidOperationException` com `"locação cancelada"`; `AddPaymentAsync` nunca chamado |
| `RegisterPaymentAsync_AmountLessOrEqualZero_ThrowsInvalidOperationException` | Validation | `Amount = 0` lança `InvalidOperationException` com `"maior que zero"`; `AddPaymentAsync` nunca chamado |
| `RegisterPaymentAsync_AmountGreaterThanRemaining_ThrowsInvalidOperationException` | BusinessRule | Valor excede saldo restante → `InvalidOperationException` com `"não pode exceder o valor total"`; `AddPaymentAsync` nunca chamado |
| `RegisterPaymentAsync_ValidData_AddsPaymentAndReturnsResponse` | Unit | Pagamento persistido; método `PIX/CREDIT_CARD` salvo em lowercase; DTO retornado com todos os campos corretos; `AddPaymentAsync` + `SaveChangesAsync` chamados 1× cada |

#### `GetAllPaymentsAsync`

| Teste | Categoria | Comportamento verificado |
|---|---|---|
| `GetAllPaymentsAsync_RepositoryReturnsNull_ReturnsEmptyEnumerable` | Unit | Repositório retorna `null` → serviço retorna enumerável vazio |
| `GetAllPaymentsAsync_WithPayments_MapsToDtos` | Unit | Lista com 1 pagamento mapeada para `PaymentResponseDto`; todos os 5 campos (`Id`, `RentalId`, `Amount`, `PaymentMethod`, `PaymentDate`) verificados; repositório chamado com filtros exatos |

---

### 🗄 Repository — `PaymentRepository`

| Teste | Método coberto | Comportamento verificado |
|---|---|---|
| `AddPaymentAsync_PersistPayment` | `AddPaymentAsync` + `SaveChangesAsync` | Pagamento gravado no InMemory; `Amount` e `PaymentMethod` preservados |
| `GetTotalPaymentsAsync_ReturnsSumForRental` | `GetTotalPaymentsAsync` | Soma apenas os pagamentos da locação alvo (50 + 70 = 120); pagamento de outra locação ignorado |
| `GetPaymentsByRentalIdAsync_FiltersByRental` | `GetPaymentsByRentalIdAsync` | Retorna 2 registros da locação correta; registro de outra locação excluído |
| `GetAllPaymentsAsync_NoFilters_ReturnsAll` | `GetAllPaymentsAsync` | Sem filtros → todos os 2 pagamentos retornados |
| `GetAllPaymentsAsync_WithFilters_FiltersCorrectly` | `GetAllPaymentsAsync` | Combinação de `rentalId` + `method = "pix"` + intervalo de datas → retorna exatamente 1 registro dentro da janela |

---

### 🎮 Controller — `PaymentController`

#### `GET /payments`

| Teste | Status esperado | Comportamento verificado |
|---|---|---|
| `Get_SemFiltros_DeveRetornarOkComListaDePagamentos` | 200 | Lista de 2 pagamentos retornada; serviço chamado com todos os parâmetros `null` |
| `Get_ComFiltros_DevePassarParametrosCorretamenteParaServico` | 200 | `rentalId`, `method`, `startDate`, `endDate` repassados intactos ao serviço |

#### `PATCH /payments/{rentalId}`

| Teste | Status esperado | Comportamento verificado |
|---|---|---|
| `RegisterPayment_DadosValidos_DeveRetornarOkComResultado` | 200 | Todos os campos do `PaymentResponseDto` verificados (incluindo `PaymentDate`); serviço chamado 1× |
| `RegisterPayment_ModelStateInvalido_DeveRetornarBadRequest` | 400 | `ModelState` inválido bloqueia chamada ao serviço; `RegisterPaymentAsync` nunca invocado |
| `RegisterPayment_AluguelNaoEncontrado_DeveRetornarNotFound` | 404 | `KeyNotFoundException` → `ProblemDetails` com `Status = 404` e `Title = RentalNotFound` |
| `RegisterPayment_OperacaoInvalida_DeveRetornarBadRequest` | 400 | `InvalidOperationException` → `ProblemDetails` com `Status = 400` e `Title = InvalidOperation` |
| `RegisterPayment_ExcecaoNaoTratada_DeveRetornarErro500` | 500 | Exceção genérica → `ProblemDetails` com `Status = 500` e `Title = ServerError` |

---

## Estratégia de testes utilizada

```
Controller  →  Mock<IPaymentService>          (Moq)
Service     →  Mock<IPaymentRepository>
               Mock<IRentalRepository>         (Moq)
Repository  →  PostgresContext InMemory        (EF Core InMemory)
```

- Todos os testes de repositório usam banco InMemory com `Guid.NewGuid()` como nome — isolamento total por teste.
- `TearDown` chama `EnsureDeleted()` + `Dispose()` após cada teste.
- `GetAllPaymentsAsync` testa filtro combinado: `rentalId` + `method` + janela de datas (`startDate` / `endDate`).
- A regra de negócio de `amount > remaining` é verificada com `GetTotalPaymentsAsync` mockado retornando 20m sobre total de 100m e pagamento tentado de 150m.

---

```
Aprovado! — Com falha: 0, Aprovado: 20, Ignorado: 0, Total: 20
```
