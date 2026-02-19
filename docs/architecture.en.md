# Project Architecture — Vehicle Rental System

---

## Overview

The **Vehicle Rental System** is a REST API built with **ASP.NET Core (.NET 8)**, following a **Layered Architecture** pattern with clear separation of responsibilities across Controllers, Services, and Repositories. The database is **PostgreSQL**, accessed via **Entity Framework Core** using the Npgsql provider.

---

## Solution projects

| Project | Type | Description |
|---|---|---|
| `VehicleRentalSystem` | ASP.NET Core Web API | Main application |
| `VehicleSystem.Tests` | Test project | Unit and integration tests |

---

## Technology stack

### Main API

| Technology | Version | Usage |
|---|---|---|
| .NET | 8.0 | Runtime and base framework |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0 | ORM |
| Npgsql.EF Core PostgreSQL | 8.0.0 | PostgreSQL provider |
| Swashbuckle (Swagger) | 6.4.0 | Interactive API documentation |

### Test project

| Technology | Version | Usage |
|---|---|---|
| NUnit | 3.13.3 | Test framework |
| xUnit | 2.9.3 | Test framework |
| Moq | 4.20.72 | Mocks and stubs |
| EF Core InMemory | 8.0.24 | In-memory database for repository tests |
| coverlet | 6.0.4 | Code coverage |

---

## Folder structure

```
vehicles/
├── docs/                          # Project documentation
│   ├── endpoints.md
│   ├── endpoints.en.md
│   ├── architecture.md
│   ├── architecture.en.md
│   └── test-reports/
│       ├── tests.md
│       └── tests.en.md
├── VehicleSystem/                 # Main project
│   ├── Controllers/               # Presentation layer (HTTP)
│   ├── Services/                  # Business logic layer
│   │   ├── interfaces/            # Service contracts
│   │   └── Mappers/               # Entity → DTO mapping
│   ├── Repositories/              # Data access layer
│   │   └── interfaces/            # Repository contracts
│   ├── Models/                    # EF Core entities
│   ├── DTO/                       # Data Transfer Objects
│   ├── Enums/                     # Domain enumerations
│   ├── Validator/                 # Business validations
│   ├── Messages/                  # Centralized message resources
│   └── Program.cs                 # Entry point and DI configuration
└── VehicleSystem.Tests/           # Test project
    ├── Controllers/               # Controller tests
    ├── Services/                  # Service tests
    ├── Repositories/              # Repository tests
    ├── DTOs/                      # DTO tests
    └── Application/Validators/    # Validator tests
```

---

## Layered architecture

```
┌─────────────────────────────────────────────┐
│               Client / HTTP                 │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│              Controllers                    │  ← Receives requests, validates
│  VehicleController  RentalController        │    ModelState, delegates to Service,
│  AccessoryController PaymentController      │    returns ActionResult with HTTP codes
│  UserController     RatingsController       │
│  RentalReportController                     │
└──────────────────────┬──────────────────────┘
                       │ Interface
┌──────────────────────▼──────────────────────┐
│                 Services                    │  ← Business rules, validations,
│  VehicleService   RentalService             │    DTO ↔ entity mappings,
│  AccessoryService PaymentService            │    cross-repository orchestration
│  UserService      RatingService             │
│  RentalReportService                        │
└──────────────────────┬──────────────────────┘
                       │ Interface
┌──────────────────────▼──────────────────────┐
│              Repositories                   │  ← Direct database access,
│  VehicleRepository   RentalRepository       │    EF Core queries,
│  AccessoryRepository PaymentRepository      │    no business rules
│  UserRepository      RatingRepository       │
│  RentalReportRepository                     │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│          PostgresContext (EF Core)          │  ← DbContext, ORM mapping
│          schema: sistema_locacao            │    → PostgreSQL
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│            PostgreSQL Database              │
└─────────────────────────────────────────────┘
```

---

## Dependency injection

All services and repositories are registered with **Scoped** lifetime in `Program.cs`, following the Dependency Inversion Principle (DIP) — upper layers depend only on interfaces, never on concrete implementations.

```
IVehicleService         ← VehicleService
IVehicleRepository      ← VehicleRepository
IRentalService          ← RentalService
IRentalRepository       ← RentalRepository
IAccessoryService       ← AccessoryService
IAccessoryRepository    ← AccessoryRepository
IPaymentService         ← PaymentService
IPaymentRepository      ← PaymentRepository
IRentalReportService    ← RentalReportService
IRentalReportRepository ← RentalReportRepository
IRatingService          ← RatingService
IRatingRepository       ← RatingRepository
IUserService            ← UserService
IUserRepository         ← UserRepository
```

---

## Data model

### Schema: `sistema_locacao`

#### Entity diagram

```
 ┌──────────┐         ┌───────────────────┐         ┌────────────┐
 │ tb_user  │ 1     N │    tb_rental       │ N     1 │ tb_vehicles│
 │──────────│─────────│───────────────────│─────────│────────────│
 │ id (PK)  │         │ id (PK)           │         │ id (PK)    │
 │ name     │         │ user_id (FK)      │         │ brand      │
 │ email    │         │ vehicle_id (FK)   │         │ model      │
 │ active   │         │ start_date        │         │ year       │
 └──────────┘         │ expected_end_date │         │ daily_rate │
                      │ actual_end_date   │         │ status     │
                      │ status            │         │ license_   │
                      │ daily_rate        │         │  plate     │
                      │ total_amount      │         └────────────┘
                      │ penalty_fee       │
                      └─────────┬─────────┘
                                │
            ┌───────────────────┼───────────────────┐
            │                   │                   │
            │ 1:N               │ 1:N               │ N:M
            ▼                   ▼                   ▼
    ┌────────────┐     ┌──────────────┐   ┌─────────────────────┐
    │ tb_payment │     │  tb_rating   │   │ tb_rental_accessory │
    │────────────│     │──────────────│   │─────────────────────│
    │ id (PK)    │     │ id (PK)      │   │ rental_id (PK, FK)  │
    │ rental_id  │     │ rental_id(FK)│   │ accessory_id(PK,FK) │
    │ amount     │     │ rating       │   │ quantity            │
    │ method     │     │ comment      │   │ unit_price          │
    │ date       │     │ created_at   │   │ total_price         │
    └────────────┘     └──────────────┘   └──────────┬──────────┘
                                                     │
                                                     │ N:1
                                                     ▼
                                           ┌──────────────────┐
                                           │   tb_accessory   │
                                           │──────────────────│
                                           │ id (PK)          │
                                           │ name             │
                                           │ daily_rate       │
                                           └──────────────────┘
```

#### Table details

| Table | Primary key | Description |
|---|---|---|
| `tb_user` | `id` (UUID) | System users; unique email |
| `tb_vehicles` | `id` (UUID) | Vehicles available for rental; unique license plate |
| `tb_rental` | `id` (UUID) | Rentals — links user and vehicle |
| `tb_accessory` | `id` (UUID) | Accessory catalog |
| `tb_rental_accessory` | (`rental_id`, `accessory_id`) | Junction table with calculated prices |
| `tb_payment` | `id` (UUID) | Payments linked to a rental |
| `tb_rating` | `id` (UUID) | Ratings linked to a rental |

---

## Domain enumerations

### `VehicleStatus`
| Value | Description |
|---|---|
| `available` | Vehicle available for rental |
| `rented` | Vehicle currently rented |
| `maintenance` | Vehicle under maintenance |

### `RentalStatus`
| Value | Description |
|---|---|
| `active` | Rental in progress |
| `completed` | Rental closed after return |
| `canceled` | Rental canceled |

### `EnumPaymentMethod`
| Value | Description |
|---|---|
| `PIX` | Pix instant payment |
| `CREDIT_CARD` | Credit card |
| `DEBIT_CARD` | Debit card |
| `CASH` | Cash |
| `BOLETO` | Bank slip |

> `EnumPaymentMethod` values are persisted in **lowercase** in the database.

---

## Key flows

### Create rental (`POST /rental`)

```
Controller
  └── validates ModelState
  └── RentalService.CreateRentalAsync(dto)
        └── RentalValidator.CheckIfUserExistsAsync()           → KeyNotFoundException if not found
        └── RentalValidator.CheckExpectedEndDateIsAfterStart() → InvalidOperationException if invalid
        └── Repository.GetVehicleById()                        → KeyNotFoundException if not found
        └── checks Status == "available"                       → InvalidOperationException if unavailable
        └── Repository.CreateRentalAsync(tbRental)
        └── Repository.UpdateVehicleStatusAsync("rented")
        └── returns RentalResponseDTO
```

### Link accessory to rental (`POST /accessories`)

```
Controller
  └── validates body / ModelState / GUIDs
  └── AccessoryService.AddAccessoryToRentalAsync(rentalId, accessoryId)
        └── Repository.GetRentalByIdAsync()        → KeyNotFoundException
        └── checks Status != "canceled"            → InvalidOperationException
        └── Repository.GetByIdAsync(accessoryId)   → KeyNotFoundException
        └── Repository.IsLinkedToRentalAsync()     → InvalidOperationException if duplicate
        └── calculates days * dailyRate → updates TotalAmount
        └── Repository.LinkToRentalAsync()
        └── Repository.UpdateAsync(rental)
```

### Register payment (`PATCH /payments/{rentalId}`)

```
Controller
  └── validates ModelState
  └── PaymentService.RegisterPaymentAsync(rentalId, dto)
        └── checks RentalId != Guid.Empty          → ArgumentException
        └── Repository.GetRentalByIdAsync()        → KeyNotFoundException
        └── checks Status != "canceled"            → InvalidOperationException
        └── checks Amount > 0                      → InvalidOperationException
        └── Repository.GetTotalPaymentsAsync()
        └── checks Amount <= (Total - Paid)        → InvalidOperationException
        └── Repository.AddPaymentAsync(tbPayment)
        └── Repository.SaveChangesAsync()
        └── returns PaymentResponseDto
```

---

## Testing strategy

```
┌─────────────┬─────────────────────────────────┬─────────────────────┐
│    Layer    │            Tool                 │     Isolation        │
├─────────────┼─────────────────────────────────┼─────────────────────┤
│ Controller  │ Mock<IService>  (Moq)            │ No database         │
│ Service     │ Mock<IRepository> (Moq)          │ No database         │
│ Repository  │ PostgresContext InMemory (EF)    │ Unique Guid per test│
│ DTO         │ Direct instantiation             │ No dependencies     │
└─────────────┴─────────────────────────────────┴─────────────────────┘
```

- Repository tests use `Guid.NewGuid()` as the InMemory database name — total isolation between tests.
- `TearDown` calls `EnsureDeleted()` + `Dispose()` for guaranteed cleanup.
- Controller tests verify HTTP status codes, `ProblemDetails` content and service calls via `Verify()`.

### Current coverage

| Module | Tests |
|---|---|
| Vehicle (DTO + Service + Repository + Controller) | 26 |
| Accessory (DTO + Service + Repository + Controller) | 89 |
| Payment (Service + Repository + Controller) | 20 |
| **Total** | **135** |

---

## Configuration and execution

### Prerequisites

- .NET 8 SDK
- PostgreSQL (local or remote instance)

### Connection string

Defined in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  }
}
```

### Running

```bash
# Restore dependencies and run the API
dotnet run --project VehicleSystem/VehicleRentalSystem.csproj

# Run all tests
dotnet test VehicleSystem.Tests/VehicleSystem.Tests.csproj

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Swagger UI

Available in development at:
```
http://localhost:{port}/swagger
```
