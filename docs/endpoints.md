# Documentação de Endpoints — Vehicle Rental System

> Base URL: `http://localhost:{porta}`
> Formato padrão: `application/json`

---

## Sumário

| Controller | Rota base | Endpoints |
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
Cria um novo veículo.

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

**Respostas:**

| Status | Descrição |
|---|---|
| `201 Created` | Veículo criado com sucesso. Retorna o DTO do veículo criado. |
| `400 Bad Request` | ModelState inválido. |
| `409 Conflict` | Placa já cadastrada (`InvalidOperationException`). |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `DELETE /vehicle/{id}`
Remove um veículo pelo identificador.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `guid` | Identificador único do veículo. |

**Respostas:**

| Status | Descrição |
|---|---|
| `204 No Content` | Veículo removido com sucesso. |
| `400 Bad Request` | Operação inválida (`InvalidOperationException`). |
| `404 Not Found` | Veículo não encontrado (`KeyNotFoundException`). |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `GET /vehicle/search`
Busca veículos por status com paginação.

**Query params:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `status` | `string` | Não | Filtra pelo status do veículo (ex.: `"available"`, `"rented"`). |
| `page` | `int` | Não (padrão: `1`) | Número da página. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Lista paginada de veículos. |
| `400 Bad Request` | Operação inválida. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `GET /vehicle/available`
Retorna os veículos disponíveis para locação com paginação.

**Query params:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `page` | `int` | Não (padrão: `1`) | Número da página (pageSize fixo: 5). |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | `VehicleListResponseDTO` com lista de veículos disponíveis. Se vazia, inclui `message`. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `PUT /vehicle/{id}`
Atualiza os dados de um veículo existente.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `guid` | Identificador único do veículo. |

**Request body:**
```json
{
  "dailyRate": 0.0,
  "year": 0,
  "status": "string"
}
```

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Veículo atualizado com sucesso. Retorna o DTO atualizado. |
| `400 Bad Request` | ModelState inválido ou operação inválida. |
| `404 Not Found` | Veículo não encontrado. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

## <a name="rental"></a> 📋 Rental

### `GET /rental`
Retorna a lista de todas as locações.

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Lista de locações. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `GET /rental/{id}`
Retorna uma locação pelo identificador.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `guid` | Identificador único da locação. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Dados da locação. |
| `400 Bad Request` | Operação inválida. |
| `404 Not Found` | Locação não encontrada. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `POST /rental`
Cria uma nova locação.

**Request body:**
```json
{
  "userId": "guid",
  "vehicleId": "guid",
  "startDate": "2025-01-01T00:00:00",
  "expectedEndDate": "2025-01-10T00:00:00"
}
```

**Respostas:**

| Status | Descrição |
|---|---|
| `201 Created` | Locação criada. Retorna o DTO da locação com `Location` header. |
| `400 Bad Request` | ModelState inválido. |
| `404 Not Found` | Usuário ou veículo não encontrado. |
| `409 Conflict` | Veículo já está locado ou regra de negócio violada. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `PATCH /rental/{id}/cancel`
Cancela uma locação existente.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `guid` | Identificador único da locação. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Locação cancelada com sucesso. Retorna o DTO atualizado. |
| `400 Bad Request` | Operação inválida (ex.: locação já cancelada). |
| `404 Not Found` | Locação não encontrada. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `PATCH /rental/{id}/update-dates`
Atualiza a data prevista de devolução de uma locação.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `guid` | Identificador único da locação. |

**Request body:**
```json
{
  "newExpectedEndDate": "2025-01-15T00:00:00"
}
```

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Datas atualizadas com sucesso. Retorna o DTO atualizado. |
| `400 Bad Request` | ModelState inválido ou operação inválida. |
| `404 Not Found` | Locação não encontrada. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `PATCH /rental/{id}/return`
Registra a devolução de um veículo.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `guid` | Identificador único da locação. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Devolução registrada com sucesso. Retorna o DTO da locação. |
| `400 Bad Request` | Operação inválida (ex.: locação já encerrada). |
| `404 Not Found` | Locação não encontrada. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `GET /rental/search`
Busca locações de um usuário específico com filtro opcional de status e paginação.

**Query params:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `userId` | `guid` | Sim | Identificador do usuário. |
| `status` | `string` | Não | Filtra pelo status da locação (ex.: `"active"`, `"canceled"`). |
| `page` | `int` | Não (padrão: `1`) | Número da página. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Lista paginada de locações do usuário. |
| `400 Bad Request` | Operação inválida. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

## <a name="accessory"></a> 🔧 Accessory

### `GET /accessories`
Retorna a lista de todos os acessórios disponíveis no sistema.

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Lista de `AccessoryResponseDto`. |

---

### `GET /accessories/{id}`
Retorna um acessório pelo identificador.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `guid` | Identificador único do acessório. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | `AccessoryResponseDto` com `Id`, `Name` e `DailyRate`. |
| `400 Bad Request` | ModelState inválido. |
| `404 Not Found` | Acessório não encontrado. |
| `409 Conflict` | Conflito de operação. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `POST /accessories/add`
Cria um novo acessório.

**Request body:**
```json
{
  "name": "string",
  "dailyRate": 0.0
}
```

**Respostas:**

| Status | Descrição |
|---|---|
| `201 Created` | Acessório criado. Retorna `AccessoryResponseDto` com `Location` header. |
| `400 Bad Request` | ModelState inválido. |
| `409 Conflict` | Nome de acessório já cadastrado. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `POST /accessories`
Vincula um acessório a uma locação existente.

**Request body:**
```json
{
  "rentalId": "guid",
  "accessoryId": "guid"
}
```

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Acessório vinculado com sucesso. Retorna `{ "message": "..." }`. |
| `400 Bad Request` | Body nulo, ModelState inválido ou IDs vazios (`Guid.Empty`). |
| `404 Not Found` | Locação ou acessório não encontrado. |
| `409 Conflict` | Acessório já vinculado à locação ou locação cancelada. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `GET /rental/{id}/accessories`
Lista todos os acessórios vinculados a uma locação específica.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `guid` | Identificador único da locação. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Lista de `AccessoryResponseDto` vinculados à locação. |
| `404 Not Found` | Locação não encontrada. |
| `409 Conflict` | Conflito de operação. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `DELETE /rental/{rentalId}/accessories/{accessoryId}`
Remove o vínculo entre um acessório e uma locação.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `rentalId` | `guid` | Identificador único da locação. |
| `accessoryId` | `guid` | Identificador único do acessório. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Vínculo removido com sucesso. Retorna `{ "message": "..." }`. |
| `404 Not Found` | Locação, acessório ou vínculo não encontrado. |
| `409 Conflict` | Conflito de operação. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

## <a name="payment"></a> 💳 Payment

### `GET /payments`
Retorna todos os pagamentos com filtros opcionais.

**Query params:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `rentalId` | `guid` | Não | Filtra pelo ID da locação. |
| `paymentMethod` | `string` | Não | Filtra pelo método de pagamento (ex.: `"pix"`, `"credit_card"`). |
| `startDate` | `datetime` | Não | Início do intervalo de datas. |
| `endDate` | `datetime` | Não | Fim do intervalo de datas. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Lista de `PaymentResponseDto`. |

---

### `PATCH /payments/{rentalId}`
Registra um pagamento para uma locação.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `rentalId` | `guid` | Identificador único da locação. |

**Request body:**
```json
{
  "amount": 0.0,
  "paymentMethod": "PIX | CREDIT_CARD"
}
```

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Pagamento registrado. Retorna `PaymentResponseDto`. |
| `400 Bad Request` | ModelState inválido, valor `<= 0`, locação cancelada ou valor excede o saldo restante. |
| `404 Not Found` | Locação não encontrada. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

## <a name="user"></a> 👤 User

### `GET /user`
Retorna todos os usuários com suas locações associadas (`rentalId` + `vehicleId`).

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Lista de usuários com locações. |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

### `POST /user`
Cria um novo usuário no sistema.

**Request body:**
```json
{
  "name": "string",
  "email": "string"
}
```

**Respostas:**

| Status | Descrição |
|---|---|
| `201 Created` | Usuário criado com sucesso. Retorna o DTO com `Location` header. |
| `400 Bad Request` | Dados inválidos (`InvalidOperationException`). |
| `500 Internal Server Error` | Erro inesperado no servidor. |

---

## <a name="ratings"></a> ⭐ Ratings

### `POST /api/ratings`
Submete uma avaliação para uma locação concluída.

**Request body:**
```json
{
  "rentalId": "guid",
  "rating": 0,
  "comment": "string"
}
```

> `comment` é opcional. `rating` deve ser um valor inteiro (ex.: 1 a 5).

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | Avaliação registrada com sucesso. Retorna `{ "message": "Review submitted successfully! Thank you for collaborating with us." }`. |
| `400 Bad Request` | Dados inválidos ou regra de negócio violada. |

---

## <a name="report"></a> 📄 Rental Report

### `GET /report/{rentalId}/export/{format?}`
Exporta o relatório de uma locação nos formatos `txt` ou `csv` e realiza o download do arquivo gerado.

**Parâmetros de rota:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `rentalId` | `guid` | Sim | Identificador único da locação. |
| `format` | `string` | Não (padrão: `txt`) | Formato do relatório: `txt` ou `csv`. |

**Respostas:**

| Status | Content-Type | Descrição |
|---|---|---|
| `200 OK` | `text/plain` ou `text/csv` | Arquivo gerado e disponibilizado para download. |
| `400 Bad Request` | `application/json` | Formato inválido (diferente de `txt` ou `csv`). |
| `404 Not Found` | `application/json` | Locação não encontrada. |
| `500 Internal Server Error` | `application/json` | Erro inesperado no servidor. |

---

### `GET /report/{rentalId}`
Retorna os dados do relatório de uma locação específica.

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `rentalId` | `guid` | Identificador único da locação. |

**Respostas:**

| Status | Descrição |
|---|---|
| `200 OK` | `RentalReportResponseDTO` com todos os dados consolidados da locação. |
| `404 Not Found` | Relatório não encontrado para o ID informado. |

---

## Códigos de erro comuns

| Status | Situação |
|---|---|
| `400 Bad Request` | ModelState inválido, GUIDs vazios, regras de negócio violadas. |
| `404 Not Found` | Entidade não encontrada por ID. |
| `409 Conflict` | Duplicidade ou estado incompatível (ex.: locação cancelada). |
| `500 Internal Server Error` | Erro inesperado — não exposto ao cliente. |
