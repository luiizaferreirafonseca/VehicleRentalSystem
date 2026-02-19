# RentGo — Business Model and Business Rules

## Table of Contents

1. [Business Model](#1-business-model)
2. [Business Rules by Domain](#2-business-rules-by-domain)
   - 2.1 [User](#21-user)
   - 2.2 [Vehicle](#22-vehicle)
   - 2.3 [Rental](#23-rental)
   - 2.4 [Return](#24-return)
   - 2.5 [Payment](#25-payment)
   - 2.6 [Accessory](#26-accessory)
   - 2.7 [Rating](#27-rating)
   - 2.8 [Reports](#28-reports)
3. [Formulas and Calculations](#3-formulas-and-calculations)
4. [Future Implementations](#4-future-implementations)

> Database structure (entities, tables and life cycles): [db.en.md](./db.en.md)

---

## 1. Business Model

**RentGo** is a vehicle rental management platform operated by administrators. The core of the system is the vehicle rental flow, which goes from vehicle and customer registration all the way to return, late fee charging, and experience rating.

### Actors

| Actor | Responsibility |
|-------|----------------|
| Administrator | Manages vehicles, users, rentals, accessories and reports |
| User (customer) | Entity registered by the administrator; associated with rentals |

### Main Flow

```
Vehicle Registration → User Registration → Rental Creation → (Add Accessories)
→ Payment Registration → Return → Rating → Report
```

---

## 2. Business Rules by Domain

### 2.1 User

- `name` is required: cannot be null, empty or whitespace only.
- `email` is required: cannot be null, empty or whitespace only.
- The e-mail must be **unique** in the system; duplicate e-mail registration attempts are rejected.
- The `email` is normalized with `Trim().ToLower()` before being persisted.
- The user is always created with `active = true`.
- When listing users, any record with a null/empty `name` or `email` causes the operation to be rejected (inconsistent data).

### 2.2 Vehicle

- `brand`, `model` and `licensePlate` are required (not null, empty or whitespace only).
- `year` must be greater than 0.
- `dailyRate` must be greater than 0.
- The `licensePlate` is normalized with `Trim()` before being persisted and must be **unique** in the system.
- Every vehicle is created with status `available`.
- **Removing** a vehicle with status `rented` is not allowed.
- The status can be manually changed by the administrator (e.g.: to `maintenance`).

### 2.3 Rental

**Input validations:**

- `userId` is required and cannot be `Guid.Empty`.
- `vehicleId` is required and cannot be `Guid.Empty`.
- `expectedEndDate` is required (cannot be the `default` value).
- `expectedEndDate` must be strictly greater than `startDate`; otherwise, the creation is rejected.

**Existence validations:**

- The user referenced by `userId` must exist in the database; if not found, a "not found" error is returned.
- The vehicle referenced by `vehicleId` must exist in the database; if not found, a "not found" error is returned.

**Automatic calculations on creation:**

- If `startDate` is not provided, the system automatically sets `DateTime.UtcNow`.
- The number of days is calculated as `(expectedEndDate.Date - startDate.Date).Days`; if the result is `<= 0`, **1 day** is used.
- The initial `totalAmount` is calculated as `dailyRate × days`.
- The `dailyRate` is captured from the vehicle at creation time and stored in the rental.
- The `penaltyFee` is initialized to `0`.

**Side effects on creation:**

- After creating the rental, the vehicle status is updated to `rented`.
- If the vehicle status update fails, the flow is aborted with a conflict error.

**Rental update:**

- Only rentals with status `active` may be updated.
- The new `expectedEndDate` must be greater than `startDate`.
- The `totalAmount` is automatically recalculated based on the new number of days.

**Cancellation:**

- Only rentals with status `active` may be canceled.
- Cancellation does not remove the record — it changes the status to `canceled`.
- The vehicle status is reverted to `available`.

### 2.4 Return

- The rental `id` cannot be `Guid.Empty`.
- The rental must exist in the database; if not found, "rental not found" is returned.
- Only rentals with status `active` may be returned.
- The `actualEndDate` is automatically set to UTC at the time of return (`DateTime.UtcNow`).
- **Late calculation:**
  - `penaltyDays = (actualEndDate.Date - expectedEndDate.Date).Days`
  - If `penaltyDays < 0`, it is treated as `0`.
- **Late fee calculation:**
  - If `penaltyDays > 0`: `penaltyFee = dailyRate × penaltyDays`
  - If no delay: `penaltyFee = 0`
- The rental status is updated to `completed`.
- The associated vehicle status is updated to `available`.

### 2.5 Payment

- The `rentalId` must correspond to an existing rental in the database.
- Registering a payment for rentals with status `canceled` is **not allowed**.
- The `amount` must be strictly greater than zero.
- The sum of all previous payments plus the new amount cannot exceed the rental's `totalAmount`.
- The payment method (`paymentMethod`) is required and must be one of the valid values: `CREDIT_CARD`, `DEBIT_CARD`, `CASH`, `PIX` or `BOLETO`.
- The `paymentDate` is auto-generated in UTC at the time of registration.
- A rental can have **multiple payments** (partial payment is allowed).

**Available filters for listing:**

- By `rentalId`
- By payment method (`method`)
- By date range (`startDate` / `endDate`) — dates are converted to UTC before querying

### 2.6 Accessory

**Catalog:**

- Accessories are registered globally with `name` and `dailyRate`.
- Registering two accessories with the same `name` is not allowed.

**Examples of available accessories:**

| Accessory | Typical use |
|-----------|-------------|
| GPS / Navigator | Travel orientation |
| Baby Chair / Booster Seat | Child safety |
| Roof Rack | Travel with excess luggage |
| Bicycle Rack | Leisure and sport |
| Connectivity Kit (Mobile Wi-Fi) | Executive rentals |
| Extra Tire (Special Spare) | Off-road or long-distance trips |

**Link with rental (N:N):**

- `rentalId` and `accessoryId` cannot be `Guid.Empty`.
- The rental and the accessory must exist in the database.
- Linking an accessory to a rental with status `canceled` is **not allowed**.
- The same accessory cannot be linked twice to the same rental.
- A rental can have multiple accessories; an accessory can be in multiple rentals.
- On linking: the rental's `totalAmount` is **incremented** by the proportional accessory value (`dailyRate × effective days`).

**Removing the link:**

- The rental, the accessory, and the link must all exist.
- On removal: the rental's `totalAmount` is **decremented** by the proportional accessory value.

**Effective days calculation for accessories:**

- Uses the difference between `expectedEndDate` and `startDate`; if `<= 0`, 1 day is used.

### 2.7 Rating

- The rental must exist in the database.
- The rental must have status `completed` (finalized); `active` or `canceled` rentals cannot be rated.
- Only **one rating per rental** is allowed (1:1 relationship).
- The score (`rating`) must be between **1 and 5** (inclusive); values outside this range are rejected.
- The comment (`comment`) is **optional**.
- The `createdAt` is auto-generated in UTC.

### 2.8 Reports

**Rental report (file export):**

- Generated by rental `id`.
- Exported as a `.txt` file containing:
  - Contract data (customer, dates, status, total days)
  - Vehicle data
  - Accessories used with quantity and amounts
  - Financial summary: vehicle total, accessories total, late fee, outstanding balance
  - Breakdown of payments made
- File identified by a unique `reportNumber` (UUID).

**Payment listing by period:**

- Allows filtering all payments by month/date range.
- Payments without a defined status are considered inconsistent data and are not returned.
- All dates received via filter are converted to UTC before querying the database, ensuring compatibility with PostgreSQL.

---

## 3. Formulas and Calculations

| Calculation | Formula |
|-------------|---------|
| Rental days | `(expectedEndDate.Date - startDate.Date).Days` (minimum 1) |
| Rental gross total | `dailyRate × days` |
| Total with accessories | `totalAmount += accessory.dailyRate × days` |
| Late days | `(actualEndDate.Date - expectedEndDate.Date).Days` (minimum 0) |
| Late fee | `dailyRate × penaltyDays` (0 if no delay) |
| Outstanding balance | `totalAmount + accessoriesTotal + penaltyFee - amountPaid` |
| Payment limit | `totalAmount - sumOfPreviousPayments` |

---

## 4. Future Implementations

The following features have been identified as planned improvements but are not yet implemented:

- **Accessory availability control:** limit the available quantity of each accessory in stock.
- **Compatibility restriction:** link accessories only to compatible vehicle models.
- **Customer portal:** allow customers to view their own rentals, make payments, and submit ratings directly, without depending on an administrator.
- **Notifications:** automatic alerts for rental expiration and delays.
