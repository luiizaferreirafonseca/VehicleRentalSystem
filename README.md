<p align="right">
  <img src="https://img.shields.io/badge/feito%20por-RentGo%20Team-purple" />
  <img src="https://img.shields.io/badge/.NET-8.0-blueviolet" />
  <img src="https://img.shields.io/badge/PostgreSQL-EF%20Core-blue" />
  <img src="https://img.shields.io/badge/testes-135%20passaram-brightgreen" />
</p>

---

<p align="right">
  <a href="./README.pt-br.md">
    <img src="https://img.shields.io/badge/🇧🇷_VER%20EM-PORTUGUÊS-009c3b?style=for-the-badge" alt="Versão em Português" />
  </a>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/luiizaferreirafonseca/VehicleRentalSystem/master/assets/rentgo.png" width="100%" height="390px">
</p>

---

<div align="center">

🚗 **RentGo** 🚗

[About](#-about-rentgo) •
[Features](#-features) •
[Architecture](#-architecture) •
[Documentation](#-documentation) •
[Technologies](#-technologies) •
[How to Run](#-how-to-run) •
[Contributors](#-contributors)

</div>

---

## 📌 About RentGo

**RentGo** is a vehicle rental REST API designed to offer a **simple, fast, and secure** car rental experience.

The platform allows users to browse available vehicles, create accounts, manage rentals, add accessories, register payments and rate the service — all through a well-structured and documented API.

> 🎓 This system was created as the final project of the **[CodeRDIversity Program](https://lp.prosperdigitalskills.com/coderdiversity-2025)** — a diversity-focused technology bootcamp.

---

## ⚙️ Features

| Module | Features |
|---|---|
| 🚗 **Vehicles** | Register, update, remove, search by status and list available with pagination |
| 📋 **Rentals** | Create, search, cancel, return and update rental dates |
| 🔧 **Accessories** | Register accessories and link / unlink them from rentals with automatic value calculation |
| 💳 **Payments** | Register partial or total payments via PIX, credit card, debit card, bank slip or cash |
| 👤 **Users** | Create and search users with rental history |
| ⭐ **Ratings** | Submit rating and comment when ending a rental |
| 📄 **Reports** | Export detailed rental report in `TXT` or `CSV` |

---

## 🏗 Architecture

The project follows the **Layered Architecture** pattern with clear separation of responsibilities across layers:

```
HTTP Client
     │
 Controllers   ← receives requests, validates ModelState, returns HTTP codes
     │
  Services     ← business rules, validations, DTO ↔ entity mappings
     │
Repositories   ← data access via EF Core, no business rules
     │
PostgresContext ← schema: sistema_locacao (PostgreSQL)
```

All dependencies are registered as **Scoped** via dependency injection — controllers and services depend only on interfaces, never on concrete implementations.

📐 [View full architecture documentation →](./docs/architecture.en.md)

---

## 🛠️ Technologies

| Technology | Version | Usage |
|---|---|---|
| .NET | 8.0 | Base runtime and framework |
| ASP.NET Core Web API | 8.0 | HTTP layer |
| Entity Framework Core | 8.0 | ORM |
| Npgsql | 8.0.0 | PostgreSQL provider |
| Swashbuckle (Swagger) | 6.4.0 | Interactive documentation |
| NUnit + xUnit | 3.13 / 2.9 | Automated tests |
| Moq | 4.20 | Test mocks |
| Coverlet | 6.0.4 | Code coverage |

---

## 📘 Documentation

All technical documentation is available in Portuguese and English:

| Document | Description | PT-BR | EN |
|---|---|---|---|
| 🏗 **Architecture** | Layers, data model, DI, business flows and testing strategy | [architecture.md](./docs/architecture.md) | [architecture.en.md](./docs/architecture.en.md) |
| 🔌 **Endpoints** | All 24 endpoints with routes, parameters, body and responses | [endpoints.md](./docs/endpoints.md) | [endpoints.en.md](./docs/endpoints.en.md) |
| 🧪 **Tests** | Complete report of 135 tests by layer and module | [tests.md](./docs/tests.md) | [tests.en.md](./docs/tests.en.md) |
| 🔬 **API Tests** | Insomnia collection with all endpoints pre-configured by module | [Insomnia.yaml](./docs/Insomnia.yaml) | — |
| 🗄 **Database** | DDL script for creating the `sistema_locacao` schema and 7 tables | [scriptSQLcreate.sql](./docs/scriptSQLcreate.sql) | — |
| 🌱 **Test Data** | Seed script with 3 vehicles, 3 users, 3 accessories and 1 complete rental | [scriptTestData.sql](./docs/scriptTestData.sql) | — |

---

## 🚀 How to Run

### Clone the repository

```bash
git clone https://github.com/luiizaferreirafonseca/VehicleRentalSystem.git
cd VehicleRentalSystem
```

### Configure the database

Create a local PostgreSQL server using **DBeaver** or **PGAdmin**.

#### 📥 Recommended option — import the script directly

Download the [`docs/scriptSQLcreate.sql`](./docs/scriptSQLcreate.sql) file and import it directly into your tool:

- **DBeaver:** right-click the database → `SQL Editor` → `Open SQL Script` → select the file
- **PGAdmin:** right-click the database → `Query Tool` → open the file via `File > Open`

Then, execute the script **in the following order**, as tables respect foreign key dependencies:

**Block 1 — Create the schema:**
```sql
CREATE SCHEMA IF NOT EXISTS sistema_locacao;
```

**Block 2 — Independent tables** (no foreign keys):
```sql
CREATE TABLE sistema_locacao.tb_user      ( ... );  -- Users
CREATE TABLE sistema_locacao.tb_vehicles  ( ... );  -- Vehicles
CREATE TABLE sistema_locacao.tb_accessory ( ... );  -- Accessories
```

**Block 3 — Dependent tables** (require the blocks above):
```sql
CREATE TABLE sistema_locacao.tb_rental            ( ... );  -- Rentals
CREATE TABLE sistema_locacao.tb_rental_accessory  ( ... );  -- Rental Accessories
CREATE TABLE sistema_locacao.tb_payment           ( ... );  -- Payments
CREATE TABLE sistema_locacao.tb_rating            ( ... );  -- Ratings
```

> ⚠️ Execute each block separately to identify errors easily.

#### 🌱 Seed the database with initial data

After creating the tables, run the [`docs/scriptTestData.sql`](./docs/scriptTestData.sql) script to validate the schema creation and insert the first test records:

- 3 vehicles (2 `available`, 1 `maintenance`)
- 3 users
- 3 accessories (`GPS`, `Child Seat`, `Extra Insurance`)
- 1 active rental with a linked accessory, payment and rating registered

> ✅ At the end of each script block there is a `SELECT` to confirm data was inserted correctly.

### Configure the Connection String

In `VehicleSystem/appsettings.json`, adjust the values according to your server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=vehicle_rental;Username=postgres;Password=postgres"
  }
}
```

### Run the API

```bash
dotnet run --project VehicleSystem
```

### Test the endpoints

#### 📄 Swagger — automatically generated by .NET

Access the interactive documentation directly in your browser:

```
http://localhost:{port}/swagger
```

Swagger is automatically enabled in the development environment and allows you to view and execute all API endpoints.

#### 🔬 Insomnia — complete pre-configured collection

Download the [`docs/Insomnia.yaml`](./docs/Insomnia.yaml) file and import it into [Insomnia](https://insomnia.rest/download):

- **Insomnia:** `File` → `Import` → select the `Insomnia.yaml` file

The collection contains all endpoints organized by module (Rentals, Payments, Accessories, Vehicles, Ratings, Users, Reports) with body examples and parameters ready to use.

### Run the tests

```bash
# All tests
dotnet test VehicleSystem.Tests

# With code coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### 📊 Coverage report — automated script

The project includes the [`VehicleSystem.Tests/coverage.ps1`](./VehicleSystem.Tests/coverage.ps1) script that automates the entire coverage workflow:

1. Runs the tests with `--collect:"XPlat Code Coverage"`
2. Locates the latest result directory inside `TestResults/`
3. Generates a full HTML report via `reportgenerator`
4. Automatically opens the report in the browser

```powershell
# From the project root
.\VehicleSystem.Tests\coverage.ps1
```

> ⚠️ Requires [ReportGenerator](https://github.com/danielpalme/ReportGenerator) installed as a global tool:
> ```bash
> dotnet tool install -g dotnet-reportgenerator-globaltool
> ```

---

## 👩‍💻 Contributors

<p>Developed with dedication by our team 💙</p>

<table>
  <tr>
    <td align="center">
      <img src="https://github.com/AlessandraBatistaJ.png" width="100px" style="border-radius: 50%;" /><br/>
      <a href="https://github.com/AlessandraBatistaJ">Alessandra Batista</a>
    </td>
    <td align="center">
      <img src="https://github.com/luiizaferreirafonseca.png" width="100px" style="border-radius: 50%;" /><br/>
      <a href="https://github.com/luiizaferreirafonseca">Luiza Ferreira</a>
    </td>
    <td align="center">
      <img src="https://github.com/priscillatrevizan.png" width="100px" style="border-radius: 50%;" /><br/>
      <a href="https://github.com/priscillatrevizan">Priscilla Trevizan</a>
    </td>
  </tr>
</table>

