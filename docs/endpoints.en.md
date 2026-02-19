# Endpoint Documentation — Vehicle Rental System

> Base URL: `http://localhost:{port}`
> Default format: `application/json`

---

## Summary

| Controller | Base route | Endpoints |
|---|---|---|
| [VehicleController](#vehicle) | `/vehicle` | 5 |
| [RentalController](#rental) | `/rental` | 7 |
| [AccessoryController](#accessory) | `/accessories` | 5 |
| [PaymentController](#payment) | `/payments` | 2 |
| [UserController](#user) | `/user` | 2 |
| [RatingsController](#ratings) | `/api/ratings` | 1 |
| [RentalReportController](#report) | `/report` | 2 |

---

## <a name="vehicle"></a> 🚗 Vehicle

### `POST /vehicle`
Creates a new vehicle.

**Request body:**
```json
{
  "brand": "string",
  "model": "string",
  "year": 0,
  "dailyRate": 0.0,
  "licensePlate": "string"
}
```

**Responses:**

| Status | Description |
|---|---|
| `201 Created` | Vehicle created successfully. Returns the vehicle DTO. |
| `400 Bad Request` | Invalid ModelState. |
| `409 Conflict` | License plate already registered (`InvalidOperationException`). |
| `500 Internal Server Error` | Unexpected server error. |

---

### `DELETE /vehicle/{id}`
Removes a vehicle by its identifier.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | `guid` | Unique vehicle identifier. |

**Responses:**

| Status | Description |
|---|---|
| `204 No Content` | Vehicle removed successfully. |
| `400 Bad Request` | Invalid operation (`InvalidOperationException`). |
| `404 Not Found` | Vehicle not found (`KeyNotFoundException`). |
| `500 Internal Server Error` | Unexpected server error. |

---

### `GET /vehicle/search`
Searches vehicles by status with pagination.

**Query params:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `status` | `string` | No | Filters by vehicle status (e.g. `"available"`, `"rented"`). |
| `page` | `int` | No (default: `1`) | Page number. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Paginated list of vehicles. |
| `400 Bad Request` | Invalid operation. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `GET /vehicle/available`
Returns vehicles available for rental with pagination.

**Query params:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `page` | `int` | No (default: `1`) | Page number (fixed pageSize: 5). |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | `VehicleListResponseDTO` with available vehicles. If empty, includes a `message` field. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `PUT /vehicle/{id}`
Updates an existing vehicle.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | `guid` | Unique vehicle identifier. |

**Request body:**
```json
{
  "dailyRate": 0.0,
  "year": 0,
  "status": "string"
}
```

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Vehicle updated successfully. Returns the updated DTO. |
| `400 Bad Request` | Invalid ModelState or invalid operation. |
| `404 Not Found` | Vehicle not found. |
| `500 Internal Server Error` | Unexpected server error. |

---

## <a name="rental"></a> 📋 Rental

### `GET /rental`
Returns the list of all rentals.

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | List of rentals. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `GET /rental/{id}`
Returns a rental by its identifier.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | `guid` | Unique rental identifier. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Rental data. |
| `400 Bad Request` | Invalid operation. |
| `404 Not Found` | Rental not found. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `POST /rental`
Creates a new rental.

**Request body:**
```json
{
  "userId": "guid",
  "vehicleId": "guid",
  "startDate": "2025-01-01T00:00:00",
  "expectedEndDate": "2025-01-10T00:00:00"
}
```

**Responses:**

| Status | Description |
|---|---|
| `201 Created` | Rental created. Returns the rental DTO with `Location` header. |
| `400 Bad Request` | Invalid ModelState. |
| `404 Not Found` | User or vehicle not found. |
| `409 Conflict` | Vehicle already rented or business rule violated. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `PATCH /rental/{id}/cancel`
Cancels an existing rental.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | `guid` | Unique rental identifier. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Rental canceled successfully. Returns the updated DTO. |
| `400 Bad Request` | Invalid operation (e.g. rental already canceled). |
| `404 Not Found` | Rental not found. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `PATCH /rental/{id}/update-dates`
Updates the expected return date of a rental.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | `guid` | Unique rental identifier. |

**Request body:**
```json
{
  "newExpectedEndDate": "2025-01-15T00:00:00"
}
```

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Dates updated successfully. Returns the updated DTO. |
| `400 Bad Request` | Invalid ModelState or invalid operation. |
| `404 Not Found` | Rental not found. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `PATCH /rental/{id}/return`
Registers the return of a vehicle.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | `guid` | Unique rental identifier. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Return registered successfully. Returns the rental DTO. |
| `400 Bad Request` | Invalid operation (e.g. rental already closed). |
| `404 Not Found` | Rental not found. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `GET /rental/search`
Searches rentals for a specific user with optional status filter and pagination.

**Query params:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `userId` | `guid` | Yes | User identifier. |
| `status` | `string` | No | Filters by rental status (e.g. `"active"`, `"canceled"`). |
| `page` | `int` | No (default: `1`) | Page number. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Paginated list of user rentals. |
| `400 Bad Request` | Invalid operation. |
| `500 Internal Server Error` | Unexpected server error. |

---

## <a name="accessory"></a> 🔧 Accessory

### `GET /accessories`
Returns the list of all accessories available in the system.

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | List of `AccessoryResponseDto`. |

---

### `GET /accessories/{id}`
Returns an accessory by its identifier.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | `guid` | Unique accessory identifier. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | `AccessoryResponseDto` with `Id`, `Name` and `DailyRate`. |
| `400 Bad Request` | Invalid ModelState. |
| `404 Not Found` | Accessory not found. |
| `409 Conflict` | Operation conflict. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `POST /accessories/add`
Creates a new accessory.

**Request body:**
```json
{
  "name": "string",
  "dailyRate": 0.0
}
```

**Responses:**

| Status | Description |
|---|---|
| `201 Created` | Accessory created. Returns `AccessoryResponseDto` with `Location` header. |
| `400 Bad Request` | Invalid ModelState. |
| `409 Conflict` | Accessory name already registered. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `POST /accessories`
Links an accessory to an existing rental.

**Request body:**
```json
{
  "rentalId": "guid",
  "accessoryId": "guid"
}
```

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Accessory linked successfully. Returns `{ "message": "..." }`. |
| `400 Bad Request` | Null body, invalid ModelState or empty IDs (`Guid.Empty`). |
| `404 Not Found` | Rental or accessory not found. |
| `409 Conflict` | Accessory already linked to the rental or rental is canceled. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `GET /rental/{id}/accessories`
Lists all accessories linked to a specific rental.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `id` | `guid` | Unique rental identifier. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | List of `AccessoryResponseDto` linked to the rental. |
| `404 Not Found` | Rental not found. |
| `409 Conflict` | Operation conflict. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `DELETE /rental/{rentalId}/accessories/{accessoryId}`
Removes the link between an accessory and a rental.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `rentalId` | `guid` | Unique rental identifier. |
| `accessoryId` | `guid` | Unique accessory identifier. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Link removed successfully. Returns `{ "message": "..." }`. |
| `404 Not Found` | Rental, accessory or link not found. |
| `409 Conflict` | Operation conflict. |
| `500 Internal Server Error` | Unexpected server error. |

---

## <a name="payment"></a> 💳 Payment

### `GET /payments`
Returns all payments with optional filters.

**Query params:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `rentalId` | `guid` | No | Filters by rental ID. |
| `paymentMethod` | `string` | No | Filters by payment method (e.g. `"pix"`, `"credit_card"`). |
| `startDate` | `datetime` | No | Start of the date range. |
| `endDate` | `datetime` | No | End of the date range. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | List of `PaymentResponseDto`. |

---

### `PATCH /payments/{rentalId}`
Registers a payment for a rental.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `rentalId` | `guid` | Unique rental identifier. |

**Request body:**
```json
{
  "amount": 0.0,
  "paymentMethod": "PIX | CREDIT_CARD"
}
```

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Payment registered. Returns `PaymentResponseDto`. |
| `400 Bad Request` | Invalid ModelState, amount `<= 0`, canceled rental or amount exceeds remaining balance. |
| `404 Not Found` | Rental not found. |
| `500 Internal Server Error` | Unexpected server error. |

---

## <a name="user"></a> 👤 User

### `GET /user`
Returns all users with their associated rentals (`rentalId` + `vehicleId`).

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | List of users with rentals. |
| `500 Internal Server Error` | Unexpected server error. |

---

### `POST /user`
Creates a new user in the system.

**Request body:**
```json
{
  "name": "string",
  "email": "string"
}
```

**Responses:**

| Status | Description |
|---|---|
| `201 Created` | User created successfully. Returns the DTO with `Location` header. |
| `400 Bad Request` | Invalid data (`InvalidOperationException`). |
| `500 Internal Server Error` | Unexpected server error. |

---

## <a name="ratings"></a> ⭐ Ratings

### `POST /api/ratings`
Submits a rating for a completed rental.

**Request body:**
```json
{
  "rentalId": "guid",
  "rating": 0,
  "comment": "string"
}
```

> `comment` is optional. `rating` must be an integer (e.g. 1 to 5).

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Rating submitted successfully. Returns `{ "message": "Review submitted successfully! Thank you for collaborating with us." }`. |
| `400 Bad Request` | Invalid data or business rule violated. |

---

## <a name="report"></a> 📄 Rental Report

### `GET /report/{rentalId}/export/{format?}`
Exports a rental report in `txt` or `csv` format and downloads the generated file.

**Route parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `rentalId` | `guid` | Yes | Unique rental identifier. |
| `format` | `string` | No (default: `txt`) | Report format: `txt` or `csv`. |

**Responses:**

| Status | Content-Type | Description |
|---|---|---|
| `200 OK` | `text/plain` or `text/csv` | File generated and available for download. |
| `400 Bad Request` | `application/json` | Invalid format (other than `txt` or `csv`). |
| `404 Not Found` | `application/json` | Rental not found. |
| `500 Internal Server Error` | `application/json` | Unexpected server error. |

---

### `GET /report/{rentalId}`
Returns the report data for a specific rental.

**Route parameters:**

| Parameter | Type | Description |
|---|---|---|
| `rentalId` | `guid` | Unique rental identifier. |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | `RentalReportResponseDTO` with all consolidated rental data. |
| `404 Not Found` | Report not found for the given ID. |

---

## Common error codes

| Status | Situation |
|---|---|
| `400 Bad Request` | Invalid ModelState, empty GUIDs, business rules violated. |
| `404 Not Found` | Entity not found by ID. |
| `409 Conflict` | Duplicate or incompatible state (e.g. canceled rental). |
| `500 Internal Server Error` | Unexpected error — not exposed to the client. |
