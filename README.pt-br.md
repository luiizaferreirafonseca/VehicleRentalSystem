<p align="right">
  <img src="https://img.shields.io/badge/feito%20por-RentGo%20Team-purple" />
  <img src="https://img.shields.io/badge/.NET-8.0-blueviolet" />
  <img src="https://img.shields.io/badge/PostgreSQL-EF%20Core-blue" />
  <img src="https://img.shields.io/badge/testes-135%20passaram-brightgreen" />
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/luiizaferreirafonseca/VehicleRentalSystem/master/assets/rentgo.png" width="100%" height="390px">
</p>

---

<div align="center">

🚗 **RentGo** 🚗

[Sobre](#-sobre-o-rentgo) •
[Funcionalidades](#-funcionalidades) •
[Arquitetura](#-arquitetura) •
[Documentação](#-documentação) •
[Tecnologias](#-tecnologias) •
[Como Executar](#-como-executar) •
[Contribuidores](#-contribuidores)

</div>

---

## 📌 Sobre o RentGo

O **RentGo** é uma API REST de locação de veículos desenvolvida para oferecer uma experiência de aluguel de carros **simples, rápida e segura**.

A plataforma permite que usuários consultem veículos disponíveis, criem contas, gerenciem locações, adicionem acessórios, registrem pagamentos e avaliem o serviço — tudo através de uma API bem estruturada e documentada.

> 🎓 Este sistema foi criado como projeto final do **[Programa CodeRDIversity](https://lp.prosperdigitalskills.com/coderdiversity-2025)** — bootcamp de diversidade em tecnologia.

---

## ⚙️ Funcionalidades

| Módulo | Funcionalidades |
|---|---|
| 🚗 **Veículos** | Cadastrar, atualizar, remover, buscar por status e listar disponíveis com paginação |
| 📋 **Locações** | Criar, consultar, cancelar, devolver e atualizar datas de locação |
| 🔧 **Acessórios** | Cadastrar acessórios e vinculá-los / desvinculá-los de locações com cálculo automático de valor |
| 💳 **Pagamentos** | Registrar pagamentos parciais ou totais via PIX, cartão de crédito, débito, boleto ou dinheiro |
| 👤 **Usuários** | Criar e consultar usuários com histórico de locações |
| ⭐ **Avaliações** | Submeter nota e comentário ao encerrar uma locação |
| 📄 **Relatórios** | Exportar relatório detalhado da locação em `TXT` ou `CSV` |

---

## 🏗 Arquitetura

O projeto segue o padrão **Layered Architecture** com separação clara de responsabilidades entre as camadas:

```
Cliente HTTP
     │
 Controllers   ← recebe requisições, valida ModelState, retorna HTTP codes
     │
  Services     ← regras de negócio, validações, mapeamentos DTO ↔ entidade
     │
Repositories   ← acesso a dados via EF Core, sem regras de negócio
     │
PostgresContext ← schema: sistema_locacao (PostgreSQL)
```

Todas as dependências são registradas como **Scoped** via injeção de dependência — controllers e services dependem apenas de interfaces, nunca de implementações concretas.

📐 [Ver documentação completa de arquitetura →](./docs/architecture.md)

---

## 🛠️ Tecnologias

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET | 8.0 | Runtime e framework base |
| ASP.NET Core Web API | 8.0 | Camada HTTP |
| Entity Framework Core | 8.0 | ORM |
| Npgsql | 8.0.0 | Provider PostgreSQL |
| Swashbuckle (Swagger) | 6.4.0 | Documentação interativa |
| NUnit + xUnit | 3.13 / 2.9 | Testes automatizados |
| Moq | 4.20 | Mocks nos testes |
| Coverlet | 6.0.4 | Cobertura de código |

---

## 📘 Documentação

Toda a documentação técnica do projeto está disponível em português e inglês:

| Documento | Descrição | PT-BR | EN |
|---|---|---|---|
| 🏗 **Arquitetura** | Camadas, modelo de dados, DI, fluxos de negócio e estratégia de testes | [architecture.md](./docs/architecture.md) | [architecture.en.md](./docs/architecture.en.md) |
| 🔌 **Endpoints** | Todos os 24 endpoints com rotas, parâmetros, body e respostas | [endpoints.md](./docs/endpoints.md) | [endpoints.en.md](./docs/endpoints.en.md) |
| 🧪 **Testes** | Relatório completo de 135 testes por camada e módulo | [tests.md](./docs/tests.md) | [tests.en.md](./docs/tests.en.md) |
| 🔬 **Testes de API** | Coleção Insomnia com todos os endpoints pré-configurados por módulo | [Insomnia.yaml](./docs/Insomnia.yaml) | — |
| 🗄 **Banco de Dados** | Script DDL para criação do schema `sistema_locacao` e das 7 tabelas | [scriptSQLcreate.sql](./docs/scriptSQLcreate.sql) | — |
| 🌱 **Dados de Teste** | Script de seed com 3 veículos, 3 usuários, 3 acessórios e 1 locação completa | [scriptMassaTeste.sql](./docs/scriptMassaTeste.sql) | — |

---

## 🚀 Como Executar

### Clone o repositório

```bash
git clone https://github.com/luiizaferreirafonseca/VehicleRentalSystem.git
cd VehicleRentalSystem
```

### Configure o banco de dados

Crie um servidor PostgreSQL local usando **DBeaver** ou **PGAdmin**.

#### 📥 Opção recomendada — importar o script diretamente

Faça o download do arquivo [`docs/scriptSQLcreate.sql`](./docs/scriptSQLcreate.sql) e importe-o diretamente na sua ferramenta:

- **DBeaver:** clique com o botão direito no banco → `SQL Editor` → `Open SQL Script` → selecione o arquivo
- **PGAdmin:** clique com o botão direito no banco → `Query Tool` → abra o arquivo pelo menu `File > Open`

Em seguida, execute o script **na seguinte ordem**, pois as tabelas respeitam dependências de chaves estrangeiras:

**Bloco 1 — Criar o schema:**
```sql
CREATE SCHEMA IF NOT EXISTS sistema_locacao;
```

**Bloco 2 — Tabelas independentes** (sem chaves estrangeiras):
```sql
CREATE TABLE sistema_locacao.tb_user      ( ... );  -- Usuários
CREATE TABLE sistema_locacao.tb_vehicles  ( ... );  -- Veículos
CREATE TABLE sistema_locacao.tb_accessory ( ... );  -- Acessórios
```

**Bloco 3 — Tabelas dependentes** (requerem os blocos anteriores):
```sql
CREATE TABLE sistema_locacao.tb_rental            ( ... );  -- Locações
CREATE TABLE sistema_locacao.tb_rental_accessory  ( ... );  -- Acessórios da locação
CREATE TABLE sistema_locacao.tb_payment           ( ... );  -- Pagamentos
CREATE TABLE sistema_locacao.tb_rating            ( ... );  -- Avaliações
```

> ⚠️ Execute cada bloco separadamente para identificar erros com facilidade.

#### 🌱 Popular o banco com dados iniciais

Após a criação das tabelas, execute o script [`docs/scriptMassaTeste.sql`](./docs/scriptMassaTeste.sql) para validar a criação do schema e inserir os primeiros registros de teste:

- 3 veículos (2 `available`, 1 `maintenance`)
- 3 usuários
- 3 acessórios (`GPS`, `Cadeirinha Infantil`, `Seguro Extra`)
- 1 locação ativa com acessório vinculado, pagamento e avaliação registrados

> ✅ Ao final de cada bloco do script há um `SELECT` para confirmar que os dados foram inseridos corretamente.

### 3️⃣ Configure a Connection String

Em `VehicleSystem/appsettings.json`, ajuste os dados de acordo com o servidor criado:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=vehicle_rental;Username=postgres;Password=postgres"
  }
}
```

### Execute a API

```bash
dotnet run --project VehicleSystem
```

### Teste os endpoints

#### 📄 Swagger — gerado automaticamente pelo .NET

Acesse a documentação interativa diretamente no navegador:

```
http://localhost:{porta}/swagger
```

O Swagger é habilitado automaticamente em ambiente de desenvolvimento e permite visualizar e executar todos os endpoints da API.

#### 🔬 Insomnia — coleção completa pré-configurada

Faça o download do arquivo [`docs/Insomnia.yaml`](./docs/Insomnia.yaml) e importe-o no [Insomnia](https://insomnia.rest/download):

- **Insomnia:** `File` → `Import` → selecione o arquivo `Insomnia.yaml`

A coleção contém todos os endpoints organizados por módulo (Rentals, Payments, Accessories, Vehicles, Ratings, Users, Reports) com exemplos de body e parâmetros prontos para uso.

### Execute os testes

```bash
# Todos os testes
dotnet test VehicleSystem.Tests

# Com cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

#### 📊 Relatório de cobertura — script automatizado

O projeto inclui o script [`VehicleSystem.Tests/coverage.ps1`](./VehicleSystem.Tests/coverage.ps1) que automatiza todo o fluxo:

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

---

## 👩‍💻 Contribuidores

<p>Desenvolvido com dedicação pela nossa equipe 💙</p>

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
