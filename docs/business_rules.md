# RentGo — Modelo de Negócio e Regras de Negócio

## Sumário

1. [Modelo de Negócio](#1-modelo-de-negócio)
2. [Regras de Negócio por Domínio](#2-regras-de-negócio-por-domínio)
   - 2.1 [Usuário (User)](#21-usuário-user)
   - 2.2 [Veículo (Vehicle)](#22-veículo-vehicle)
   - 2.3 [Locação (Rental)](#23-locação-rental)
   - 2.4 [Devolução (Return)](#24-devolução-return)
   - 2.5 [Pagamento (Payment)](#25-pagamento-payment)
   - 2.6 [Acessório (Accessory)](#26-acessório-accessory)
   - 2.7 [Avaliação (Rating)](#27-avaliação-rating)
   - 2.8 [Relatórios (Reports)](#28-relatórios-reports)
3. [Fórmulas e Cálculos](#3-fórmulas-e-cálculos)
4. [Implementações Futuras](#4-implementações-futuras)

> Estrutura do banco de dados (entidades, tabelas e ciclos de vida): [db.md](./db.md)

---

## 1. Modelo de Negócio

O **RentGo** é uma plataforma de gestão de locação de veículos operada por administradores. O núcleo do sistema é o fluxo de locação de veículos, que vai desde o cadastro do veículo e do cliente até a devolução, cobrança de multas e avaliação da experiência.

### Atores

| Ator | Responsabilidade |
|------|-----------------|
| Administrador | Gerencia veículos, usuários, locações, acessórios e relatórios |
| Usuário (cliente) | Entidade cadastrada pelo administrador; associado às locações |

### Fluxo Principal

```
Cadastro de Veículo → Cadastro de Usuário → Criação de Locação → (Adição de Acessórios)
→ Registro de Pagamentos → Devolução → Avaliação → Relatório
```

---

## 2. Regras de Negócio por Domínio

### 2.1 Usuário (User)

- `name` é obrigatório: não pode ser nulo, vazio ou apenas espaços.
- `email` é obrigatório: não pode ser nulo, vazio ou apenas espaços.
- O e-mail deve ser **único** no sistema; tentativas de cadastro com e-mail duplicado são rejeitadas.
- O `email` é normalizado com `Trim().ToLower()` antes de ser persistido.
- O usuário é criado sempre com `active = true`.
- Na listagem de usuários, qualquer registro com `name` ou `email` nulo/vazio causa rejeição da operação (dado inconsistente).

### 2.2 Veículo (Vehicle)

- `brand`, `model` e `licensePlate` são obrigatórios (não nulos, vazios ou apenas espaços).
- `year` deve ser maior que 0.
- `dailyRate` deve ser maior que 0.
- A `licensePlate` é normalizada com `Trim()` antes de ser persistida e deve ser **única** no sistema.
- Todo veículo é cadastrado com status `available`.
- **Não é permitido remover** um veículo com status `rented`.
- O status pode ser alterado manualmente pelo administrador (ex.: para `maintenance`).

### 2.3 Locação (Rental)

**Validações de entrada:**

- `userId` é obrigatório e não pode ser `Guid.Empty`.
- `vehicleId` é obrigatório e não pode ser `Guid.Empty`.
- `expectedEndDate` é obrigatório (não pode ser o valor `default`).
- `expectedEndDate` deve ser estritamente maior que `startDate`; caso contrário, a criação é rejeitada.

**Validações de existência:**

- O usuário referenciado por `userId` deve existir na base; se não existir, retorna erro de "não encontrado".
- O veículo referenciado por `vehicleId` deve existir na base; se não existir, retorna erro de "não encontrado".

**Cálculos automáticos na criação:**

- Se `startDate` não for informado, o sistema define automaticamente `DateTime.UtcNow`.
- O número de dias é calculado como `(expectedEndDate.Date - startDate.Date).Days`; se o resultado for `<= 0`, considera-se **1 dia**.
- O `totalAmount` inicial é calculado como `dailyRate × days`.
- O `dailyRate` é capturado do veículo no momento da criação e armazenado na locação.
- O `penaltyFee` é inicializado como `0`.

**Efeitos colaterais na criação:**

- Após criar a locação, o status do veículo é atualizado para `rented`.
- Se a atualização do status do veículo falhar, o fluxo é cancelado com erro de conflito.

**Atualização da locação:**

- Só é permitido atualizar locações com status `active`.
- A nova `expectedEndDate` deve ser maior que `startDate`.
- O `totalAmount` é recalculado automaticamente com base nos novos dias.

**Cancelamento:**

- Só é permitido cancelar locações com status `active`.
- O cancelamento não remove o registro — altera o status para `canceled`.
- O status do veículo é revertido para `available`.

### 2.4 Devolução (Return)

- O `id` da locação não pode ser `Guid.Empty`.
- A locação deve existir na base; se não existir, retorna "locação não encontrada".
- Só é permitido devolver locações com status `active`.
- O `actualEndDate` é gerado automaticamente em UTC no momento da devolução (`DateTime.UtcNow`).
- **Cálculo de atraso:**
  - `penaltyDays = (actualEndDate.Date - expectedEndDate.Date).Days`
  - Se `penaltyDays < 0`, considera-se `0`.
- **Cálculo de multa:**
  - Se `penaltyDays > 0`: `penaltyFee = dailyRate × penaltyDays`
  - Se não houver atraso: `penaltyFee = 0`
- O status da locação é atualizado para `completed`.
- O status do veículo associado é atualizado para `available`.

### 2.5 Pagamento (Payment)

- O `rentalId` deve corresponder a uma locação existente na base.
- **Não é permitido** registrar pagamento para locações com status `canceled`.
- O `amount` deve ser estritamente maior que zero.
- A soma de todos os pagamentos já realizados acrescida do novo valor não pode exceder o `totalAmount` da locação.
- O método de pagamento (`paymentMethod`) é obrigatório e deve ser um dos valores válidos: `CREDIT_CARD`, `DEBIT_CARD`, `CASH`, `PIX` ou `BOLETO`.
- O `paymentDate` é gerado automaticamente em UTC no momento do registro.
- Uma locação pode ter **múltiplos pagamentos** (pagamento parcial é permitido).

**Filtros disponíveis para listagem:**

- Por `rentalId`
- Por método de pagamento (`method`)
- Por intervalo de datas (`startDate` / `endDate`) — datas convertidas para UTC antes da consulta

### 2.6 Acessório (Accessory)

**Catálogo:**

- Acessórios são cadastrados globalmente com `name` e `dailyRate`.
- Não é permitido cadastrar dois acessórios com o mesmo `name`.

**Exemplos de acessórios disponíveis:**

| Acessório | Uso típico |
|-----------|-----------|
| GPS/Navegador | Orientação em viagem |
| Cadeira de Bebê / Assento de Elevação | Segurança infantil |
| Bagageiro de Teto | Viagens com excesso de bagagem |
| Suporte para Bicicletas | Lazer e esporte |
| Kit de Conectividade (Wi-Fi Móvel) | Locações executivas |
| Pneu Extra (Estepe Especial) | Trilhas ou viagens longas |

**Vínculo com locação (N:N):**

- `rentalId` e `accessoryId` não podem ser `Guid.Empty`.
- A locação e o acessório devem existir na base.
- **Não é permitido** vincular acessório a locação com status `canceled`.
- Um mesmo acessório não pode ser vinculado duas vezes à mesma locação.
- Uma locação pode ter múltiplos acessórios; um acessório pode estar em várias locações.
- Ao vincular: o `totalAmount` da locação é **incrementado** pelo valor proporcional do acessório (`dailyRate × dias efetivos`).

**Remoção do vínculo:**

- A locação, o acessório e o vínculo devem existir.
- Ao remover: o `totalAmount` da locação é **decrementado** pelo valor proporcional do acessório.

**Cálculo dos dias efetivos para acessórios:**

- Usa a diferença entre `expectedEndDate` e `startDate`; se `<= 0`, considera-se 1 dia.

### 2.7 Avaliação (Rating)

- A locação deve existir na base.
- A locação deve estar com status `completed` (finalizada); locações `active` ou `canceled` não podem ser avaliadas.
- É permitida **apenas uma avaliação por locação** (relacionamento 1:1).
- A nota (`rating`) deve estar entre **1 e 5** (inclusive); valores fora desse intervalo são rejeitados.
- O comentário (`comment`) é **opcional**.
- O `createdAt` é gerado automaticamente em UTC.

### 2.8 Relatórios (Reports)

**Relatório de locação (exportação de arquivo):**

- Gerado por `id` de locação.
- Exportado como arquivo `.txt` contendo:
  - Dados do contrato (cliente, datas, status, total de dias)
  - Dados do veículo
  - Acessórios utilizados com quantidade e valores
  - Resumo financeiro: total do veículo, total de acessórios, multa, saldo devedor
  - Detalhamento de pagamentos realizados
- Arquivo identificado por um `reportNumber` único (UUID).

**Listagem de pagamentos por período:**

- Permite filtrar todos os pagamentos por mês/intervalo de datas.
- Pagamentos sem status definido são considerados dados inconsistentes e não são retornados.
- Todas as datas recebidas via filtro são convertidas para UTC antes da consulta ao banco, garantindo compatibilidade com PostgreSQL.

---

## 3. Fórmulas e Cálculos

| Cálculo | Fórmula |
|---------|---------|
| Dias de locação | `(expectedEndDate.Date - startDate.Date).Days` (mínimo 1) |
| Total bruto da locação | `dailyRate × days` |
| Total com acessórios | `totalAmount += accessory.dailyRate × days` |
| Dias de atraso | `(actualEndDate.Date - expectedEndDate.Date).Days` (mínimo 0) |
| Multa por atraso | `dailyRate × penaltyDays` (0 se sem atraso) |
| Saldo devedor | `totalAmount + accessoriesTotal + penaltyFee - amountPaid` |
| Limite de pagamento | `totalAmount - somaDePageamentosAnteriores` |

---

## 4. Implementações Futuras

As seguintes funcionalidades foram identificadas como melhorias planejadas, mas ainda não implementadas:

- **Controle de disponibilidade de acessórios:** limitar a quantidade disponível de cada acessório no estoque.
- **Restrição por compatibilidade:** vincular acessórios apenas a modelos de veículo compatíveis.
- **Portal do usuário:** permitir que o próprio cliente visualize suas locações, realize pagamentos e avalie diretamente, sem depender do administrador.
- **Notificações:** alertas automáticos para vencimento de locações e atrasos.
