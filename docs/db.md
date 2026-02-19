# RentGo — Banco de Dados: Entidades, Relacionamentos e Ciclos de Vida

> Documentação complementar a [business_rules.md](./business_rules.md).

## Sumário

1. [Diagrama de Relacionamentos](#1-diagrama-de-relacionamentos)
2. [Tabelas do Banco de Dados](#2-tabelas-do-banco-de-dados)
3. [Campos Principais por Entidade](#3-campos-principais-por-entidade)
   - 3.1 [TbUser](#31-tbuser)
   - 3.2 [TbVehicle](#32-tbvehicle)
   - 3.3 [TbRental](#33-tbrental)
   - 3.4 [TbPayment](#34-tbpayment)
   - 3.5 [TbRating](#35-tbrating)
   - 3.6 [TbAccessory](#36-tbaccessory)
   - 3.7 [TbRentalAccessory](#37-tbrentalaccessory)
4. [Status e Ciclos de Vida](#4-status-e-ciclos-de-vida)
   - 4.1 [Status do Veículo](#41-status-do-veículo)
   - 4.2 [Status da Locação](#42-status-da-locação)

---

## 1. Diagrama de Relacionamentos

```
TbUser (1) ────────── (N) TbRental
TbVehicle (1) ──────── (N) TbRental
TbRental (1) ────────── (N) TbPayment
TbRental (1) ──────── (0..1) TbRating
TbRental (N) ──────────── (N) TbAccessory
                ↑ via TbRentalAccessory
```

![UML Diagram — Vehicle Rental System](./ERdiagram.png)

| Relacionamento |
|----------------|---------------|-----------|
| `TbUser` → `TbRental` | 1 : N | Um usuário realiza várias locações |
| `TbVehicle` → `TbRental` | 1 : N | Um veículo aparece em várias locações ao longo do tempo |
| `TbRental` → `TbPayment` | 1 : N | Uma locação pode ter vários pagamentos parciais |
| `TbRental` → `TbRating` | 1 : 0..1 | Uma locação pode ter no máximo uma avaliação |
| `TbRental` ↔ `TbAccessory` | N : N | Via tabela associativa `TbRentalAccessory` |

---

## 2. Tabelas do Banco de Dados

| Tabela | Descrição |
|--------|-----------|
| `tb_user` | Clientes cadastrados no sistema |
| `tb_vehicle` | Veículos disponíveis para locação |
| `tb_rental` | Contratos de locação |
| `tb_payment` | Pagamentos vinculados a uma locação |
| `tb_rating` | Avaliação do cliente sobre a locação |
| `tb_accessory` | Catálogo de acessórios disponíveis |
| `tb_rental_accessory` | Tabela associativa N:N entre locações e acessórios |

---

## 3. Campos Principais por Entidade

### 3.1 TbUser

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | `Guid` | ✅ | Identificador único, gerado automaticamente |
| `name` | `string` | ✅ | Nome do cliente |
| `email` | `string` | ✅ | E-mail único do cliente |
| `active` | `bool` | ✅ | Indica se o usuário está ativo (`true` por padrão) |

### 3.2 TbVehicle

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | `Guid` | ✅ | Identificador único, gerado automaticamente |
| `brand` | `string` | ✅ | Marca do veículo |
| `model` | `string` | ✅ | Modelo do veículo |
| `year` | `int` | ✅ | Ano de fabricação (> 0) |
| `dailyRate` | `decimal` | ✅ | Valor da diária (> 0) |
| `licensePlate` | `string` | ✅ | Placa única no sistema |
| `status` | `string` | ✅ | `available`, `rented` ou `maintenance` |

### 3.3 TbRental

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | `Guid` | ✅ | Identificador único, gerado automaticamente |
| `userId` | `Guid` | ✅ | FK para `tb_user` |
| `vehicleId` | `Guid` | ✅ | FK para `tb_vehicle` |
| `startDate` | `DateTime` | ✅ | Data de início (UTC; `UtcNow` se não informado) |
| `expectedEndDate` | `DateTime` | ✅ | Data de devolução prevista |
| `actualEndDate` | `DateTime?` | ❌ | Data real de devolução (preenchida na devolução) |
| `dailyRate` | `decimal` | ✅ | Diária capturada do veículo no momento da criação |
| `totalAmount` | `decimal?` | ✅ | Valor total bruto (diária × dias + acessórios) |
| `penaltyFee` | `decimal?` | ✅ | Multa por atraso (`0` até a devolução) |
| `status` | `string` | ✅ | `active`, `completed` ou `canceled` |

### 3.4 TbPayment

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | `Guid` | ✅ | Identificador único, gerado automaticamente |
| `rentalId` | `Guid` | ✅ | FK para `tb_rental` |
| `amount` | `decimal` | ✅ | Valor do pagamento (> 0) |
| `paymentMethod` | `string` | ✅ | `CREDIT_CARD`, `DEBIT_CARD`, `CASH`, `PIX` ou `BOLETO` |
| `paymentDate` | `DateTime` | ✅ | Gerado automaticamente em UTC |

### 3.5 TbRating

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | `Guid` | ✅ | Identificador único, gerado automaticamente |
| `rentalId` | `Guid` | ✅ | FK para `tb_rental` (único — 1:1) |
| `rating` | `int` | ✅ | Nota de 1 a 5 |
| `comment` | `string?` | ❌ | Comentário opcional |
| `createdAt` | `DateTime` | ✅ | Gerado automaticamente em UTC |

### 3.6 TbAccessory

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | `Guid` | ✅ | Identificador único, gerado automaticamente |
| `name` | `string` | ✅ | Nome único do acessório |
| `dailyRate` | `decimal` | ✅ | Valor diário do acessório |

### 3.7 TbRentalAccessory

Tabela associativa que implementa o relacionamento N:N entre `tb_rental` e `tb_accessory`.

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `rentalId` | `Guid` | ✅ | FK + parte da PK composta |
| `accessoryId` | `Guid` | ✅ | FK + parte da PK composta |
| `quantity` | `int` | ✅ | Quantidade do acessório na locação |
| `unitPrice` | `decimal` | ✅ | Preço unitário no momento do vínculo |
| `totalPrice` | `decimal` | ✅ | `unitPrice × quantity` |

> A chave primária composta `(rentalId, accessoryId)` garante que o mesmo acessório não seja vinculado mais de uma vez à mesma locação.

---

## 4. Status e Ciclos de Vida

### 4.1 Status do Veículo

| Status | Descrição |
|--------|-----------|
| `available` | Disponível para nova locação |
| `rented` | Atualmente locado |
| `maintenance` | Indisponível para locação |

```
available ──[criar locação]──→ rented
rented    ──[devolver]──────→ available
rented    ──[cancelar]──────→ available
available / rented ──[admin]──→ maintenance
```

### 4.2 Status da Locação

| Status | Descrição |
|--------|-----------|
| `active` | Locação em andamento |
| `completed` | Devolução realizada com sucesso |
| `canceled` | Locação cancelada antes da devolução |

```
active ──[devolver]──→ completed
active ──[cancelar]──→ canceled
```

> Locações com status `completed` ou `canceled` são imutáveis — nenhuma operação de atualização, cancelamento ou novo pagamento é permitida sobre elas.
