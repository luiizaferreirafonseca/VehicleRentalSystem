# RentGo — Database: Entities, Relationships and Life Cycles

> Complementary documentation to [business_rules.en.md](./business_rules.en.md).

## Table of Contents

1. [Relationship Diagram](#1-relationship-diagram)
2. [Database Tables](#2-database-tables)
3. [Main Fields by Entity](#3-main-fields-by-entity)
   - 3.1 [TbUser](#31-tbuser)
   - 3.2 [TbVehicle](#32-tbvehicle)
   - 3.3 [TbRental](#33-tbrental)
   - 3.4 [TbPayment](#34-tbpayment)
   - 3.5 [TbRating](#35-tbrating)
   - 3.6 [TbAccessory](#36-tbaccessory)
   - 3.7 [TbRentalAccessory](#37-tbrentalaccessory)
4. [Status and Life Cycles](#4-status-and-life-cycles)
   - 4.1 [Vehicle Status](#41-vehicle-status)
   - 4.2 [Rental Status](#42-rental-status)

---

## 1. Relationship Diagram

```
TbUser (1) ────────── (N) TbRental
TbVehicle (1) ──────── (N) TbRental
TbRental (1) ────────── (N) TbPayment
TbRental (1) ──────── (0..1) TbRating
TbRental (N) ──────────── (N) TbAccessory
                ↑ via TbRentalAccessory
```

![UML Diagram — Vehicle Rental System](./ERdiagram.png)

| Relationship | Cardinality | Description |
|--------------|-------------|-------------|
| `TbUser` → `TbRental` | 1 : N | A user performs multiple rentals |
| `TbVehicle` → `TbRental` | 1 : N | A vehicle appears in multiple rentals over time |
| `TbRental` → `TbPayment` | 1 : N | A rental can have multiple partial payments |
| `TbRental` → `TbRating` | 1 : 0..1 | A rental can have at most one rating |
| `TbRental` ↔ `TbAccessory` | N : N | Via associative table `TbRentalAccessory` |

---

## 2. Database Tables

| Table | Description |
|-------|-------------|
| `tb_user` | Registered customers in the system |
| `tb_vehicle` | Vehicles available for rental |
| `tb_rental` | Rental contracts |
| `tb_payment` | Payments linked to a rental |
| `tb_rating` | Customer rating for a rental |
| `tb_accessory` | Catalog of available accessories |
| `tb_rental_accessory` | N:N associative table between rentals and accessories |

---

## 3. Main Fields by Entity

### 3.1 TbUser

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `Guid` | ✅ | Unique identifier, auto-generated |
| `name` | `string` | ✅ | Customer name |
| `email` | `string` | ✅ | Unique customer e-mail |
| `active` | `bool` | ✅ | Indicates whether the user is active (`true` by default) |

### 3.2 TbVehicle

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `Guid` | ✅ | Unique identifier, auto-generated |
| `brand` | `string` | ✅ | Vehicle brand |
| `model` | `string` | ✅ | Vehicle model |
| `year` | `int` | ✅ | Manufacturing year (> 0) |
| `dailyRate` | `decimal` | ✅ | Daily rate value (> 0) |
| `licensePlate` | `string` | ✅ | Unique plate number in the system |
| `status` | `string` | ✅ | `available`, `rented` or `maintenance` |

### 3.3 TbRental

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `Guid` | ✅ | Unique identifier, auto-generated |
| `userId` | `Guid` | ✅ | FK to `tb_user` |
| `vehicleId` | `Guid` | ✅ | FK to `tb_vehicle` |
| `startDate` | `DateTime` | ✅ | Start date (UTC; `UtcNow` if not provided) |
| `expectedEndDate` | `DateTime` | ✅ | Expected return date |
| `actualEndDate` | `DateTime?` | ❌ | Actual return date (filled in on return) |
| `dailyRate` | `decimal` | ✅ | Daily rate captured from the vehicle at creation time |
| `totalAmount` | `decimal?` | ✅ | Gross total (daily rate × days + accessories) |
| `penaltyFee` | `decimal?` | ✅ | Late fee (`0` until return) |
| `status` | `string` | ✅ | `active`, `completed` or `canceled` |

### 3.4 TbPayment

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `Guid` | ✅ | Unique identifier, auto-generated |
| `rentalId` | `Guid` | ✅ | FK to `tb_rental` |
| `amount` | `decimal` | ✅ | Payment amount (> 0) |
| `paymentMethod` | `string` | ✅ | `CREDIT_CARD`, `DEBIT_CARD`, `CASH`, `PIX` or `BOLETO` |
| `paymentDate` | `DateTime` | ✅ | Auto-generated in UTC |

### 3.5 TbRating

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `Guid` | ✅ | Unique identifier, auto-generated |
| `rentalId` | `Guid` | ✅ | FK to `tb_rental` (unique — 1:1) |
| `rating` | `int` | ✅ | Score from 1 to 5 |
| `comment` | `string?` | ❌ | Optional comment |
| `createdAt` | `DateTime` | ✅ | Auto-generated in UTC |

### 3.6 TbAccessory

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | `Guid` | ✅ | Unique identifier, auto-generated |
| `name` | `string` | ✅ | Unique accessory name |
| `dailyRate` | `decimal` | ✅ | Daily rate for the accessory |

### 3.7 TbRentalAccessory

Associative table that implements the N:N relationship between `tb_rental` and `tb_accessory`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `rentalId` | `Guid` | ✅ | FK + part of the composite PK |
| `accessoryId` | `Guid` | ✅ | FK + part of the composite PK |
| `quantity` | `int` | ✅ | Quantity of the accessory in the rental |
| `unitPrice` | `decimal` | ✅ | Unit price at the time of linking |
| `totalPrice` | `decimal` | ✅ | `unitPrice × quantity` |

> The composite primary key `(rentalId, accessoryId)` ensures the same accessory cannot be linked more than once to the same rental.

---

## 4. Status and Life Cycles

### 4.1 Vehicle Status

| Status | Description |
|--------|-------------|
| `available` | Available for a new rental |
| `rented` | Currently rented |
| `maintenance` | Unavailable for rental |

```
available ──[create rental]──→ rented
rented    ──[return]─────────→ available
rented    ──[cancel]─────────→ available
available / rented ──[admin]──→ maintenance
```

### 4.2 Rental Status

| Status | Description |
|--------|-------------|
| `active` | Rental in progress |
| `completed` | Return successfully completed |
| `canceled` | Rental canceled before return |

```
active ──[return]──→ completed
active ──[cancel]──→ canceled
```

> Rentals with status `completed` or `canceled` are immutable — no update, cancellation, or new payment operations are allowed on them.
