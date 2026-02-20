# Test Report — `[HttpGet("available")]`

> Layer covered: **DTO · Service · Repository**
> Result: ✅ **26 tests passed** — zero failures

---

## Files created / modified

| File | What was done | Tests added |
|---|---|---|
| `VehicleSystem.Tests/Services/VehicleServiceTests.cs` | Added `GetAvailableVehiclesAsync` region | 5 |
| `VehicleSystem.Tests/Repositories/VehicleRepositoryTests.cs` | Created from scratch (InMemory EF) | 8 |
| `VehicleSystem.Tests/DTOs/VehicleListResponseDTOTests.cs` | Created from scratch | 6 |
| `VehicleSystem.Tests/Controllers/VehicleControllerTests.cs` | Added endpoint tests (previous session) | 4 |

**New tests added:** 23 · **Total executed in filter:** 26

---

## Coverage by layer

### 🗂 DTO — `VehicleListResponseDTO` / `VehicleResponseDTO`

| Test | Verified behavior |
|---|---|
| `Default_VehiclesNaoEhNulo` | `Vehicles` is never `null` when instantiating the DTO |
| `Default_VehiclesEhListaVazia` | `Vehicles` starts as an empty list |
| `Default_MessageEhNula` | `Message` starts as `null` |
| `ComVeiculos_VehiclesContemElementos` | `Vehicles` accepts and retains added elements |
| `ComMensagem_MessageEhDefinida` | `Message` can be set with a string value |
| `VehicleResponseDTO_PropriedadesMapeadas` | All 7 fields (`Id`, `Brand`, `Model`, `Year`, `DailyRate`, `Status`, `LicensePlate`) are mapped correctly |

---

### ⚙️ Service — `VehicleService.GetAvailableVehiclesAsync`

| Test | Verified behavior |
|---|---|
| `ComVeiculosDisponiveis_RetornaListaComVeiculos` | Returns `VehicleListResponseDTO` with vehicles and `Message = null` |
| `SemVeiculosDisponiveis_RetornaMensagemVazia` | Empty list returns message `"Não há veículos disponíveis para locação."` |
| `ChamaRepositorioComStatusAvailable` | Repository is called exactly with `"available"` |
| `PaginaCustomizada_PassaPaginaParaRepositorio` | Custom page is correctly forwarded to the repository |
| `MapeiaPropriedadesCorretamente` | All `TbVehicle` fields are mapped to `VehicleResponseDTO` |

---

### 🗄 Repository — `VehicleRepository.SearchVehiclesAsync`

| Test | Verified behavior |
|---|---|
| `ComStatusAvailable_RetornaApenasDisponiveis` | Filters only vehicles with `Status = "available"` |
| `ComStatusNulo_RetornaTodosVeiculos` | `status = null` returns all vehicles |
| `ComStatusVazio_RetornaTodosVeiculos` | `status = ""` returns all vehicles |
| `SemCorrespondencia_RetornaListaVazia` | Status with no match returns an empty list |
| `FiltroStatusCaseInsensitive` | Filter is case-insensitive (`"AVAILABLE"` == `"available"`) |
| `ResultadoOrdenadoPorBrand` | Result is always ordered by `Brand` |
| `PaginaMenorQueUm_UsaPaginaUm` | Page `< 1` is normalized to `1` |
| `Paginacao_Pagina2_RetornaProximosItens` | Page 2 correctly returns the next items (pageSize = 5) |

---

## Testing strategy

```
Controller  →  Mock<IVehicleService>         (Moq)
Service     →  Mock<IVehicleRepository>      (Moq)
Repository  →  PostgresContext InMemory      (EF Core InMemory)
DTO         →  Direct instantiation          (no dependencies)
```

- Each repository test uses an InMemory database with `Guid.NewGuid()` as its name to guarantee total isolation.
- `TearDown` calls `EnsureDeleted()` + `Dispose()` to clean state between tests.
- The repository's fixed `pageSize` is `5` — pagination tests respect this value.

---

```
Test summary: total: 26; failed: 0; succeeded: 26; skipped: 0
```

---

# Test Report — Accessory (all layers)

> Layers covered: **DTO · Service · Repository · Controller**
> Result: ✅ **89 tests passed** — zero failures

---

## Test files

| File | Tests |
|---|---|
| `VehicleSystem.Tests/Services/AccessoryServiceTests.cs` | 47 |
| `VehicleSystem.Tests/Controllers/AccessoryControllerTests.cs` | 27 |
| `VehicleSystem.Tests/Repositories/AccessoryRepositoryTests.cs` | 9 |
| `VehicleSystem.Tests/DTOs/AccessoryDtoTests.cs` | 6 |

---

## Coverage by layer

### 🗂 DTO — `AccessoryCreateDto` / `AccessoryResponseDto` / `AccessoryReportDto`

| Test | Verified behavior |
|---|---|
| `AccessoryCreateDto_Defaults_AreExpected` | `Name = ""` and `DailyRate = 0` on instantiation |
| `AccessoryCreateDto_CanSetProperties` | Assigns and reads `Name` and `DailyRate` correctly |
| `AccessoryResponseDto_Defaults_AreExpected` | `Id = Guid.Empty`, `Name = ""`, `DailyRate = 0` on instantiation |
| `AccessoryResponseDto_CanSetProperties` | Mapping of 3 fields (`Id`, `Name`, `DailyRate`) |
| `AccessoryReportDto_Defaults_AreExpected` | All 8 fields have the correct default value |
| `AccessoryReportDto_CanSetProperties` | Full mapping including `Quantity`, `UnitPrice`, `TotalPrice`, dates |

---

### ⚙️ Service — `AccessoryService`

#### GET
| Test | Verified behavior |
|---|---|
| `GetAccessoriesAsync_NoAccessoriesFound_ReturnsEmptyList` | Repository returns `null` → service returns empty list |
| `GetAccessoryByIdAsync_IdNotFound_ThrowsKeyNotFoundException` | Non-existent ID throws `KeyNotFoundException` |
| `GetAccessoryByIdAsync_ValidId_ReturnsMappedDto` | Mapping of `Id`, `Name`, `DailyRate` to `AccessoryResponseDto` |
| `GetAccessoryByIdAsync_ExistingId_ReturnsCorrectMapping` | Fields mapped correctly (alternative case) |
| `GetAccessoryByIdAsync_ExistingId_ExecutesMapping` | Coverage of the `new AccessoryResponseDto` projection line |
| `GetAccessoryByIdAsync_ExistingAccessory_ReturnsMappedDto` | Field-by-field mapping validation |
| `GetAccessoriesByRentalIdAsync_RentalNotFound_ThrowsKeyNotFoundException` | Non-existent rental throws `KeyNotFoundException` |
| `GetAccessoriesByRentalIdAsync_ExistingAccessories_ReturnsMappedList` | List of 1 accessory mapped to DTO |
| `GetAccessoriesByRentalIdAsync_WithAccessories_ReturnsMappedList` | List of 2 accessories mapped correctly |
| `GetAccessoriesByRentalIdAsync_RepositoryReturnsNull_CoversNullBranch` | Repository returns `null` → result is empty |
| `GetAccessoriesByRentalIdAsync_ValidRental_ReturnsMappedDtoList` | Valid rental returns mapped DTOs |

#### CREATE
| Test | Verified behavior |
|---|---|
| `CreateAccessoryAsync_DuplicateName_ThrowsInvalidOperationException` | Duplicate name throws `InvalidOperationException`; `AddAsync` is never called |
| `CreateAccessoryAsync_ValidDto_ExecutesInternalInstantiation` | `TbAccessory` created with `Name`, `DailyRate` and `Id != Guid.Empty` |
| `CreateAccessoryAsync_ValidData_PersistsAndReturnsDto` | `AddAsync` called; returned `Id` is valid |
| `CreateAccessoryAsync_ValidData_CoversEntityInstantiation` | Coverage of the `new TbAccessory { ... }` line |
| `CreateAccessoryAsync_ValidData_ReturnsAccessoryResponseDto` | Returned DTO has correct `Name`, `DailyRate` and `Id` |
| `CreateAccessoryAsync_NewAccessory_CallsRepositoryWithCorrectData` | `AddAsync` receives entity with exact DTO data |

#### ADD TO RENTAL
| Test | Verified behavior |
|---|---|
| `AddAccessoryToRentalAsync_GuidsAreEmpty_ThrowsArgumentException` | Both GUIDs empty throw `ArgumentException` |
| `AddAccessoryToRentalAsync_EmptyGuids_ThrowsArgumentException` | Only `RentalId` empty throws `ArgumentException` |
| `AddAccessoryToRentalAsync_RentalNotFound_ThrowsKeyNotFoundException` | `null` rental throws `KeyNotFoundException` with correct message |
| `AddAccessoryToRentalAsync_RentalIdNotFound_ThrowsKeyNotFoundException` | Non-existent rental ID throws `KeyNotFoundException` |
| `AddAccessoryToRentalAsync_AccessoryNotFound_ThrowsKeyNotFoundException` | Non-existent accessory throws `KeyNotFoundException` |
| `AddAccessoryToRentalAsync_RentalCanceled_ThrowsInvalidOperationException` | Canceled rental blocks link with correct message |
| `AddAccessoryToRentalAsync_AlreadyLinked_ThrowsInvalidOperationException` | Duplicate link throws `InvalidOperationException` |
| `AddAccessoryToRentalAsync_ValidRentalAndAccessory_UpdatesTotalAndLinksCorrectly` | 2 days × $20 = $140; `UpdateAsync` and `LinkToRentalAsync` called |
| `AddAccessoryToRentalAsync_MultipleDays_CalculatesTotalCorrectly` | 3 days × $10 = $30 added to total |
| `AddAccessoryToRentalAsync_MultipleDays_CalculatesAndUpdatesTotal` | 5 days × $10; `UpdateAsync` called once |
| `AddAccessoryToRentalAsync_DaysGreaterThanZero_CallsLinkAndUpdateAndUpdatesTotal` | 3 days: total 50 + 30 = 80 |
| `AddAccessoryToRentalAsync_DatesEqual_UsesOneDayAndCallsLinkAndUpdate` | Equal dates → minimum 1 day: 0 + 20 = 20 |
| `AddAccessoryToRentalAsync_SameDayDates_CalculatesMinimumOneDay` | Same day → 50 × 1 = 50 |
| `AddAccessoryToRentalAsync_DatesAreEqual_UsesOneDayMinimum` | Equal dates → 50 + 20 = 70 |
| `AddAccessoryToRentalAsync_PositivePeriod_UpdatesTotalCorrectly` | 3 days × $20: 100 + 60 = 160 |
| `AddAccessoryToRentalAsync_PositivePeriod_CalculatesAndUpdatesTotal` | 4 days × $10: 100 + 40 = 140 |
| `AddAccessoryToRentalAsync_ValidData_UpdatesRentalTotalAmount` | Calculation with `ExpectedEndDate - StartDate`; `UpdateAsync` called |
| `AddAccessoryToRentalAsync_ExpectedEndGreaterThanStart_UsesDaysAndUpdatesTotal` | 2 days × $15: 10 + 30 = 40 |
| `AddAccessoryToRentalAsync_EndBeforeStart_UsesOneDayAndUpdatesTotal` | End date before start → 1 day: 5 + 8 = 13 |

#### REMOVE FROM RENTAL
| Test | Verified behavior |
|---|---|
| `RemoveAccessoryFromRentalAsync_RentalNotFound_ThrowsKeyNotFoundException` | `null` rental throws `KeyNotFoundException` |
| `RemoveAccessoryFromRentalAsync_AccessoryNotFound_ThrowsKeyNotFoundException` | `null` accessory throws `KeyNotFoundException` |
| `RemoveAccessoryFromRentalAsync_NotLinked_ThrowsKeyNotFoundException` | Non-existent link throws `KeyNotFoundException`; `RemoveLinkAsync` never called |
| `RemoveAccessoryFromRentalAsync_ValidLink_DecreasesTotalAndRemovesLink` | 3 days × $30: 200 − 90 = 110; `RemoveLinkAsync` and `UpdateAsync` called |
| `RemoveAccessoryFromRentalAsync_ValidData_DecreasesTotalAmount` | Calculation based on `ExpectedEndDate - StartDate`; `UpdateAsync` called |
| `RemoveAccessoryFromRentalAsync_DatesAreEqual_UsesOneDayMinimum` | Equal dates → 1 day: 100 − 20 = 80 |
| `RemoveAccessoryFromRentalAsync_DaysGreaterThanZero_DecreasesTotalCorrectly` | 2 days × $50: 200 − 100 = 100 |
| `RemoveAccessoryFromRentalAsync_DaysGreaterThanZero_CalculatesReductionCorrectly` | 4 days × $10: 100 − 40 = 60 |
| `RemoveAccessoryFromRentalAsync_MultipleDays_ExecutesCorrectCalculation` | 5 days × $10: 200 − 50 = 150 |
| `RemoveAccessoryFromRentalAsync_PositivePeriod_ExecutesNormalCalculation` | 3 days × $50: 300 − 150 = 150 |
| `RemoveAccessoryFromRentalAsync_PositiveDays_CalculatesCorrectReduction` | 2 days × $10: 100 − 20 = 80 |

---

### 🗄 Repository — `AccessoryRepository`

| Test | Method covered | Verified behavior |
|---|---|---|
| `AddAsync_AddsAccessory_ToDatabase` | `AddAsync` | Entity added to `DbSet` with `Name = "GPS"` |
| `GetByNameAsync_ReturnsAccessory_WhenExists` | `GetByNameAsync` | Returns accessory with correct `Id` and `Name` |
| `GetByNameAsync_ReturnsNull_WhenNotFound` | `GetByNameAsync` | Returns `null` when name does not exist |
| `GetAllAsync_ReturnsAllAccessories` | `GetAllAsync` | Full list with both expected names |
| `LinkToRentalAsync_CreatesRentalAccessory_WithCorrectPrices` | `LinkToRentalAsync` | `UnitPrice = DailyRate`, `TotalPrice = DailyRate`, `Quantity = 1` |
| `LinkToRentalAsync_CreatesRentalAccessory_WithZeroPrices_WhenAccessoryNotFound` | `LinkToRentalAsync` | Accessory not found → prices `0m`, link still created |
| `IsLinkedToRentalAsync_ReturnsTrueWhenLinked_AndFalseWhenNot` | `IsLinkedToRentalAsync` | `true` for existing link; `false` for non-existent ID |
| `RemoveLinkAsync_RemovesExistingLink` | `RemoveLinkAsync` | Link removed; list becomes empty |
| `GetByRentalIdAsync_ReturnsAccessories_LinkedToRental` | `GetByRentalIdAsync` | Returns the 2 accessories linked to the rental |

---

### 🎮 Controller — `AccessoryController`

#### `GET /accessories`
| Test | Expected status | Verified behavior |
|---|---|---|
| `Get_ShouldReturn_200OkWithEmptyList_WhenNoAccessoriesExist` | 200 | Empty list returned without error |
| `Get_ShouldReturnList_WhenAccessoriesExist` | 200 | 2 accessories returned; first is "GPS" |

#### `GET /accessories/{id}`
| Test | Expected status | Verified behavior |
|---|---|---|
| `GetById_ShouldReturn_200Ok_WhenExists` | 200 | Valid ID returns `OkObjectResult` |
| `GetById_ShouldReturn_400_WhenModelStateIsInvalid` | 400 | Invalid `ModelState` blocks service call |
| `GetById_ShouldReturn_404NotFound_WhenIdDoesNotExist` | 404 | `KeyNotFoundException` → `ProblemDetails` with correct title |
| `GetById_ShouldReturn_409_WhenInvalidOperationExceptionOccurs` | 409 | `InvalidOperationException` → `ConflictObjectResult` |
| `GetById_ShouldReturn_500_WhenUnexpectedExceptionOccurs` | 500 | Generic exception → `ProblemDetails` with `ServerInternalError` |

#### `POST /accessories/add`
| Test | Expected status | Verified behavior |
|---|---|---|
| `Create_ShouldReturn_201Created_WhenAccessoryCreatedSuccessfully` | 201 | `CreatedAtActionResult` with correct `Id` |
| `Create_ShouldReturn_409Conflict_WhenNameIsDuplicate` | 409 | Duplicate name → `ProblemDetails` with `Conflict` |
| `Create_ShouldReturn_400BadRequest_WhenModelStateIsInvalid` | 400 | `Name = ""` in `ModelState` blocks creation |
| `Create_ShouldReturn_500_WhenUnexpectedException` | 500 | Fatal error → `StatusCode 500` |

#### `POST /accessories` (link)
| Test | Expected status | Verified behavior |
|---|---|---|
| `AddAccessoryToRental_ShouldReturn_400BadRequest_WhenRequestIsNull` | 400 | `null` body → `ProblemDetails` with `RequestInvalid` |
| `AddAccessoryToRental_ShouldReturn_400_WhenModelStateIsInvalid` | 400 | Invalid `ModelState` blocks link |
| `AddAccessoryToRental_ShouldReturn_400_WhenOnlyRentalIdIsEmpty` | 400 | `RentalId = Guid.Empty` → 400 |
| `AddAccessoryToRental_ShouldReturn_400_WhenOnlyAccessoryIdIsEmpty` | 400 | `AccessoryId = Guid.Empty` → 400 |
| `AddAccessoryToRental_ShouldReturn_200Ok_WhenRequestIsValid` | 200 | Link created; response contains `AccessoryLinkedSuccess`; service called 1x |
| `AddAccessoryToRental_ShouldReturn_404_AndLogWarning_WhenKeyNotFound` | 404 | `KeyNotFoundException` → 404 + `LogWarning` emitted |
| `AddAccessoryToRental_ShouldReturn_409_WhenInvalidOperation` | 409 | `InvalidOperationException` → `ConflictObjectResult` |
| `AddAccessoryToRental_ShouldReturn_500_AndLogError_WhenUnexpectedException` | 500 | Generic exception → `StatusCode 500` |

#### `GET /rental/{id}/accessories`
| Test | Expected status | Verified behavior |
|---|---|---|
| `GetAccessoriesByRental_ShouldReturnList_WhenAccessoriesExist` | 200 | List returned with `OkObjectResult` |
| `GetAccessoriesByRental_ShouldReturn_404_WhenNotFound` | 404 | `KeyNotFoundException` → `NotFoundObjectResult` |
| `GetAccessoriesByRental_ShouldReturn_409_WhenInvalidOperation` | 409 | Business conflict → `ConflictObjectResult` |
| `GetAccessoriesByRental_ShouldReturn_500_WhenUnexpectedException` | 500 | Internal error → `StatusCode 500` |

#### `DELETE /rental/{rentalId}/accessories/{accessoryId}`
| Test | Expected status | Verified behavior |
|---|---|---|
| `RemoveAccessoryFromRental_ShouldReturn_200Ok` | 200 | Link removed; service called 1x |
| `RemoveAccessoryFromRental_ShouldReturn_404_WhenKeyNotFound` | 404 | `KeyNotFoundException` → `NotFoundObjectResult` |
| `RemoveAccessoryFromRental_ShouldReturn_409_WhenInvalidOperation` | 409 | Conflict → `ConflictObjectResult` |
| `RemoveAccessoryFromRental_ShouldReturn_500_WhenUnexpectedException` | 500 | Generic exception → `StatusCode 500` |

---

## Testing strategy

```
Controller  →  Mock<IAccessoryService>            (Moq)
Service     →  Mock<IAccessoryRepository>
               Mock<IRentalRepository>             (Moq)
Repository  →  Mock<PostgresContext> + DbSet mock  (Moq + TestAsyncQueryProvider)
DTO         →  Direct instantiation                (no dependencies)
```

- The repository uses `Mock<PostgresContext>` with a custom `TestAsyncQueryProvider<T>` to support `ToListAsync` / `AnyAsync` / `FirstOrDefaultAsync` on in-memory lists.
- `IsLinkedToRentalAsync` and `RemoveLinkAsync` verify links via mocked `TbRentalAccessory`.
- Day calculation tests cover both branches of `if (days <= 0) days = 1`: equal dates (minimum 1 day) and positive difference.

---

```
Passed! — Failed: 0, Passed: 89, Skipped: 0, Total: 89
```

---

# Test Report — Payment (all layers)

> Layers covered: **Service · Repository · Controller**
> Result: ✅ **20 tests passed** — zero failures

---

## Test files

| File | Tests |
|---|---|
| `VehicleSystem.Tests/Services/PaymentServiceTests.cs` | 8 |
| `VehicleSystem.Tests/Controllers/PaymentControllerTests.cs` | 7 |
| `VehicleSystem.Tests/Repositories/PaymentRepositoryTests.cs` | 5 |

> ℹ️ There is no DTO test file for Payment — the DTOs (`PaymentCreateDTO`, `PaymentResponseDto`) are covered indirectly by Service and Controller tests.

---

## Coverage by layer

### ⚙️ Service — `PaymentService`

#### `RegisterPaymentAsync`

| Test | Category | Verified behavior |
|---|---|---|
| `RegisterPaymentAsync_RentalIdEmpty_ThrowsArgumentException` | Validation | `RentalId = Guid.Empty` throws `ArgumentException` with message containing `"identificador da locação"` |
| `RegisterPaymentAsync_RentalNotFound_ThrowsKeyNotFoundException` | Validation | Rental not found throws `KeyNotFoundException`; `AddPaymentAsync` never called |
| `RegisterPaymentAsync_RentalCanceled_ThrowsInvalidOperationException` | BusinessRule | Canceled rental throws `InvalidOperationException` with `"locação cancelada"`; `AddPaymentAsync` never called |
| `RegisterPaymentAsync_AmountLessOrEqualZero_ThrowsInvalidOperationException` | Validation | `Amount = 0` throws `InvalidOperationException` with `"maior que zero"`; `AddPaymentAsync` never called |
| `RegisterPaymentAsync_AmountGreaterThanRemaining_ThrowsInvalidOperationException` | BusinessRule | Amount exceeds remaining balance → `InvalidOperationException` with `"não pode exceder o valor total"`; `AddPaymentAsync` never called |
| `RegisterPaymentAsync_ValidData_AddsPaymentAndReturnsResponse` | Unit | Payment persisted; method `PIX/CREDIT_CARD` saved in lowercase; DTO returned with all correct fields; `AddPaymentAsync` + `SaveChangesAsync` each called 1× |

#### `GetAllPaymentsAsync`

| Test | Category | Verified behavior |
|---|---|---|
| `GetAllPaymentsAsync_RepositoryReturnsNull_ReturnsEmptyEnumerable` | Unit | Repository returns `null` → service returns empty enumerable |
| `GetAllPaymentsAsync_WithPayments_MapsToDtos` | Unit | List with 1 payment mapped to `PaymentResponseDto`; all 5 fields (`Id`, `RentalId`, `Amount`, `PaymentMethod`, `PaymentDate`) verified; repository called with exact filters |

---

### 🗄 Repository — `PaymentRepository`

| Test | Method covered | Verified behavior |
|---|---|---|
| `AddPaymentAsync_PersistPayment` | `AddPaymentAsync` + `SaveChangesAsync` | Payment saved in InMemory; `Amount` and `PaymentMethod` preserved |
| `GetTotalPaymentsAsync_ReturnsSumForRental` | `GetTotalPaymentsAsync` | Sums only the target rental's payments (50 + 70 = 120); payment from another rental ignored |
| `GetPaymentsByRentalIdAsync_FiltersByRental` | `GetPaymentsByRentalIdAsync` | Returns 2 records from the correct rental; record from another rental excluded |
| `GetAllPaymentsAsync_NoFilters_ReturnsAll` | `GetAllPaymentsAsync` | No filters → all 2 payments returned |
| `GetAllPaymentsAsync_WithFilters_FiltersCorrectly` | `GetAllPaymentsAsync` | Combination of `rentalId` + `method = "pix"` + date range → returns exactly 1 record within the window |

---

### 🎮 Controller — `PaymentController`

#### `GET /payments`

| Test | Expected status | Verified behavior |
|---|---|---|
| `Get_SemFiltros_DeveRetornarOkComListaDePagamentos` | 200 | List of 2 payments returned; service called with all `null` parameters |
| `Get_ComFiltros_DevePassarParametrosCorretamenteParaServico` | 200 | `rentalId`, `method`, `startDate`, `endDate` forwarded intact to the service |

#### `PATCH /payments/{rentalId}`

| Test | Expected status | Verified behavior |
|---|---|---|
| `RegisterPayment_DadosValidos_DeveRetornarOkComResultado` | 200 | All `PaymentResponseDto` fields verified (including `PaymentDate`); service called 1× |
| `RegisterPayment_ModelStateInvalido_DeveRetornarBadRequest` | 400 | Invalid `ModelState` blocks service call; `RegisterPaymentAsync` never invoked |
| `RegisterPayment_AluguelNaoEncontrado_DeveRetornarNotFound` | 404 | `KeyNotFoundException` → `ProblemDetails` with `Status = 404` and `Title = RentalNotFound` |
| `RegisterPayment_OperacaoInvalida_DeveRetornarBadRequest` | 400 | `InvalidOperationException` → `ProblemDetails` with `Status = 400` and `Title = InvalidOperation` |
| `RegisterPayment_ExcecaoNaoTratada_DeveRetornarErro500` | 500 | Unhandled exception → `ProblemDetails` with `Status = 500` and `Title = ServerError` |

---

## Testing strategy

```
Controller  →  Mock<IPaymentService>          (Moq)
Service     →  Mock<IPaymentRepository>
               Mock<IRentalRepository>         (Moq)
Repository  →  PostgresContext InMemory        (EF Core InMemory)
```

- All repository tests use an InMemory database with `Guid.NewGuid()` as the name — total isolation per test.
- `TearDown` calls `EnsureDeleted()` + `Dispose()` after each test.
- `GetAllPaymentsAsync` tests a combined filter: `rentalId` + `method` + date range (`startDate` / `endDate`).
- The `amount > remaining` business rule is verified with `GetTotalPaymentsAsync` mocked returning 20m against a total of 100m and a payment attempt of 150m.

---

```
Passed! — Failed: 0, Passed: 20, Skipped: 0, Total: 20
```

---

## 📊 Code Coverage — Overall Summary

#image:'Tests.png'
