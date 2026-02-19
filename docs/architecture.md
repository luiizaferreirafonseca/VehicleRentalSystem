# Arquitetura do Projeto — Vehicle Rental System

---

## Visão geral

O **Vehicle Rental System** é uma API REST desenvolvida em **ASP.NET Core (.NET 8)**, seguindo o padrão arquitetural em camadas (**Layered Architecture**) com separação de responsabilidades entre Controllers, Services e Repositories. O banco de dados utilizado é o **PostgreSQL**, acessado via **Entity Framework Core** com o provider Npgsql.

---

## Projetos da solução

| Projeto | Tipo | Descrição |
|---|---|---|
| `VehicleRentalSystem` | ASP.NET Core Web API | Aplicação principal |
| `VehicleSystem.Tests` | Projeto de testes | Testes unitários e de integração |

---

## Stack tecnológica

### API principal

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET | 8.0 | Runtime e framework base |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0 | ORM |
| Npgsql.EF Core PostgreSQL | 8.0.0 | Provider para PostgreSQL |
| Swashbuckle (Swagger) | 6.4.0 | Documentação interativa |

### Projeto de testes

| Tecnologia | Versão | Uso |
|---|---|---|
| NUnit | 3.13.3 | Framework de testes |
| xUnit | 2.9.3 | Framework de testes |
| Moq | 4.20.72 | Mocks e stubs |
| EF Core InMemory | 8.0.24 | Banco em memória para testes de repositório |
| coverlet | 6.0.4 | Cobertura de código |

---

## Estrutura de pastas

```
vehicles/
├── docs/                          # Documentação do projeto
│   ├── endpoints.md
│   ├── endpoints.en.md
│   ├── architecture.md
│   ├── architecture.en.md
│   ├── tests.md
│   ├── tests.en.md
│   ├── scriptSQLcreate.sql
│   ├── scriptTestData.sql
│   └── Insomnia.yaml
├── VehicleSystem/                 # Projeto principal
│   ├── Controllers/               # Camada de apresentação (HTTP)
│   ├── Services/                  # Camada de negócio
│   │   ├── interfaces/            # Contratos de serviço
│   │   └── Mappers/               # Mapeamento entidade → DTO
│   ├── Repositories/              # Camada de acesso a dados
│   │   └── interfaces/            # Contratos de repositório
│   ├── Models/                    # Entidades EF Core
│   ├── DTO/                       # Data Transfer Objects
│   ├── Enums/                     # Enumerações de domínio
│   ├── Validator/                 # Validações de negócio
│   ├── Messages/                  # Recursos de mensagens centralizados
│   └── Program.cs                 # Ponto de entrada e configuração DI
└── VehicleSystem.Tests/           # Projeto de testes
    ├── Controllers/               # Testes de controller
    ├── Services/                  # Testes de service
    ├── Repositories/              # Testes de repositório
    ├── DTOs/                      # Testes de DTO
    ├── Application/Validators/    # Testes de validação
    └── coverage.ps1               # Script de relatório de cobertura
```

---

## Arquitetura em camadas

```
┌─────────────────────────────────────────────┐
│               Cliente / HTTP                │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│              Controllers                    │  ← Recebe requisições, valida
│  VehicleController  RentalController        │    ModelState, delega ao Service,
│  AccessoryController PaymentController      │    retorna ActionResult com HTTP codes
│  UserController     RatingsController       │
│  RentalReportController                     │
└──────────────────────┬──────────────────────┘
                       │ Interface
┌──────────────────────▼──────────────────────┐
│                 Services                    │  ← Regras de negócio, validações,
│  VehicleService   RentalService             │    mapeamentos DTO ↔ entidade,
│  AccessoryService PaymentService            │    orquestração entre repositórios
│  UserService      RatingService             │
│  RentalReportService                        │
└──────────────────────┬──────────────────────┘
                       │ Interface
┌──────────────────────▼──────────────────────┐
│              Repositories                   │  ← Acesso direto ao banco,
│  VehicleRepository   RentalRepository       │    queries EF Core,
│  AccessoryRepository PaymentRepository      │    sem regras de negócio
│  UserRepository      RatingRepository       │
│  RentalReportRepository                     │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│          PostgresContext (EF Core)          │  ← DbContext, mapeamento
│          schema: sistema_locacao            │    ORM → PostgreSQL
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│            PostgreSQL Database              │
└─────────────────────────────────────────────┘
```

---

## Injeção de dependência

Todos os serviços e repositórios são registrados com escopo **Scoped** no `Program.cs`, seguindo o princípio da inversão de dependência (DIP) — as camadas superiores dependem apenas das interfaces, nunca das implementações concretas.

```
IVehicleService       ← VehicleService
IVehicleRepository    ← VehicleRepository
IRentalService        ← RentalService
IRentalRepository     ← RentalRepository
IAccessoryService     ← AccessoryService
IAccessoryRepository  ← AccessoryRepository
IPaymentService       ← PaymentService
IPaymentRepository    ← PaymentRepository
IRentalReportService  ← RentalReportService
IRentalReportRepository ← RentalReportRepository
IRatingService        ← RatingService
IRatingRepository     ← RatingRepository
IUserService          ← UserService
IUserRepository       ← UserRepository
```

---

## Modelo de dados

### Schema: `sistema_locacao`

#### Diagrama de entidades

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

#### Detalhamento das tabelas

| Tabela | Chave primária | Descrição |
|---|---|---|
| `tb_user` | `id` (UUID) | Usuários do sistema; e-mail único |
| `tb_vehicles` | `id` (UUID) | Veículos disponíveis para locação; placa única |
| `tb_rental` | `id` (UUID) | Locações — relaciona usuário e veículo |
| `tb_accessory` | `id` (UUID) | Catálogo de acessórios disponíveis |
| `tb_rental_accessory` | (`rental_id`, `accessory_id`) | Tabela de junção com preços calculados |
| `tb_payment` | `id` (UUID) | Pagamentos vinculados a uma locação |
| `tb_rating` | `id` (UUID) | Avaliações vinculadas a uma locação |

---

## Enumerações de domínio

### `VehicleStatus`
| Valor | Descrição |
|---|---|
| `available` | Veículo disponível para locação |
| `rented` | Veículo atualmente locado |
| `maintenance` | Veículo em manutenção |

### `RentalStatus`
| Valor | Descrição |
|---|---|
| `active` | Locação em andamento |
| `completed` | Locação encerrada com devolução |
| `canceled` | Locação cancelada |

### `EnumPaymentMethod`
| Valor | Descrição |
|---|---|
| `PIX` | Pagamento via Pix |
| `CREDIT_CARD` | Cartão de crédito |
| `DEBIT_CARD` | Cartão de débito |
| `CASH` | Dinheiro |
| `BOLETO` | Boleto bancário |

> Os valores de `EnumPaymentMethod` são persistidos em **lowercase** no banco de dados.

---

## Fluxos principais

### Criação de locação (`POST /rental`)

```
Controller
  └── valida ModelState
  └── RentalService.CreateRentalAsync(dto)
        └── RentalValidator.CheckIfUserExistsAsync()      → KeyNotFoundException se não existe
        └── RentalValidator.CheckExpectedEndDateIsAfterStart() → InvalidOperationException se inválido
        └── Repository.GetVehicleById()                   → KeyNotFoundException se não existe
        └── verifica Status == "available"                → InvalidOperationException se indisponível
        └── Repository.CreateRentalAsync(tbRental)
        └── Repository.UpdateVehicleStatusAsync("rented")
        └── retorna RentalResponseDTO
```

### Vinculação de acessório (`POST /accessories`)

```
Controller
  └── valida body/ModelState/GUIDs
  └── AccessoryService.AddAccessoryToRentalAsync(rentalId, accessoryId)
        └── Repository.GetRentalByIdAsync()               → KeyNotFoundException
        └── verifica Status != "canceled"                 → InvalidOperationException
        └── Repository.GetByIdAsync(accessoryId)          → KeyNotFoundException
        └── Repository.IsLinkedToRentalAsync()            → InvalidOperationException se duplicado
        └── calcula dias * dailyRate → atualiza TotalAmount
        └── Repository.LinkToRentalAsync()
        └── Repository.UpdateAsync(rental)
```

### Registro de pagamento (`PATCH /payments/{rentalId}`)

```
Controller
  └── valida ModelState
  └── PaymentService.RegisterPaymentAsync(rentalId, dto)
        └── verifica RentalId != Guid.Empty               → ArgumentException
        └── Repository.GetRentalByIdAsync()               → KeyNotFoundException
        └── verifica Status != "canceled"                 → InvalidOperationException
        └── verifica Amount > 0                           → InvalidOperationException
        └── Repository.GetTotalPaymentsAsync()
        └── verifica Amount <= (Total - Pago)             → InvalidOperationException
        └── Repository.AddPaymentAsync(tbPayment)
        └── Repository.SaveChangesAsync()
        └── retorna PaymentResponseDto
```

---

## Estratégia de testes

```
┌─────────────┬─────────────────────────────────┬─────────────────────┐
│   Camada    │         Ferramenta              │      Isolamento      │
├─────────────┼─────────────────────────────────┼─────────────────────┤
│ Controller  │ Mock<IService>  (Moq)            │ Sem banco           │
│ Service     │ Mock<IRepository> (Moq)          │ Sem banco           │
│ Repository  │ PostgresContext InMemory (EF)    │ Guid único por teste│
│ DTO         │ Instanciação direta              │ Sem dependências    │
└─────────────┴─────────────────────────────────┴─────────────────────┘
```

- Testes de repositório usam `Guid.NewGuid()` como nome do banco InMemory — isolamento total entre testes.
- `TearDown` chama `EnsureDeleted()` + `Dispose()` para limpeza garantida.
- Testes de controller verificam HTTP status codes, `ProblemDetails` e chamadas ao serviço via `Verify()`.

### Cobertura atual

| Módulo | Testes |
|---|---|
| Vehicle (DTO + Service + Repository + Controller) | 26 |
| Accessory (DTO + Service + Repository + Controller) | 89 |
| Payment (Service + Repository + Controller) | 20 |
| **Total** | **135** |

---

## Configuração e execução

### Pré-requisitos

- .NET 8 SDK
- PostgreSQL (instância local ou remota)

### Connection string

Definida em `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  }
}
```

### Execução

```bash
# Restaurar dependências e executar a API
dotnet run --project VehicleSystem/VehicleRentalSystem.csproj

# Executar todos os testes
dotnet test VehicleSystem.Tests/VehicleSystem.Tests.csproj

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

#### 📊 Relatório de cobertura — script automatizado

O script [`VehicleSystem.Tests/coverage.ps1`](../VehicleSystem.Tests/coverage.ps1) automatiza todo o fluxo:

1. Executa os testes com `--collect:"XPlat Code Coverage"`
2. Localiza o diretório de resultado mais recente em `TestResults/`
3. Gera um relatório HTML completo via `reportgenerator`
4. Abre o relatório automaticamente no navegador

```powershell
# Na raiz do projeto
.\VehicleSystem.Tests\coverage.ps1
```

> ⚠️ Requer o [ReportGenerator](https://github.com/danielpalme/ReportGenerator) instalado como ferramenta global:
> ```bash
> dotnet tool install -g dotnet-reportgenerator-globaltool
> ```

### Swagger UI

Disponível em desenvolvimento em:
```
http://localhost:{porta}/swagger
```
