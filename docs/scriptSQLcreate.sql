/* ESQUEMA: sistema_locacao 
   STATUS: Versão consolidada com TIMESTAMPTZ (UTC)
*/

CREATE SCHEMA IF NOT EXISTS sistema_locacao;

---
-- 1. TABELAS INDEPENDENTES
---

-- Tabela de Usuários
CREATE TABLE sistema_locacao.tb_user (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(256) NOT NULL,
    email VARCHAR(256) UNIQUE NOT NULL,
    active BOOLEAN DEFAULT true
);

-- Tabela de Veículos
CREATE TABLE sistema_locacao.tb_vehicles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    model VARCHAR(100) NOT NULL,
    year INT4 NOT NULL,
    brand VARCHAR(100) NOT NULL,
    daily_rate NUMERIC(10, 2) NOT NULL,
    status VARCHAR(20) NOT NULL,
    license_plate VARCHAR(20) UNIQUE NOT NULL,
    CONSTRAINT chk_vehicle_status CHECK (status IN ('available', 'rented', 'maintenance'))
);

-- Tabela de Acessórios
CREATE TABLE sistema_locacao.tb_accessory (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    daily_rate NUMERIC(10, 2) NOT NULL
);

---
-- 2. TABELAS DEPENDENTES
---

-- Tabela de Locações
CREATE TABLE sistema_locacao.tb_rental (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    -- Ajustado para TIMESTAMPTZ com default em UTC
    start_date TIMESTAMPTZ NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'), 
    expected_end_date TIMESTAMPTZ NOT NULL,
    actual_end_date TIMESTAMPTZ,
    total_amount NUMERIC(10, 2),
    penalty_fee NUMERIC(10, 2),
    status VARCHAR(50) NOT NULL,
    vehicle_id UUID NOT NULL,
    user_id UUID NOT NULL,
    daily_rate NUMERIC(10, 2) NOT NULL,
    CONSTRAINT chk_rental_status CHECK (status IN ('active', 'completed', 'canceled')),
    CONSTRAINT fk_rental_vehicle FOREIGN KEY (vehicle_id) 
        REFERENCES sistema_locacao.tb_vehicles(id) ON DELETE RESTRICT,
    CONSTRAINT fk_rental_user FOREIGN KEY (user_id) 
        REFERENCES sistema_locacao.tb_user(id) ON DELETE RESTRICT
);

-- Tabela Intermediária: Acessórios da Locação
CREATE TABLE sistema_locacao.tb_rental_accessory (
    rental_id UUID NOT NULL,
    accessory_id UUID NOT NULL,
    quantity INT4 NOT NULL,
    unit_price NUMERIC(10, 2) NOT NULL,
    total_price NUMERIC(10, 2) NOT NULL,
    PRIMARY KEY (rental_id, accessory_id),
    CONSTRAINT fk_ra_rental FOREIGN KEY (rental_id) 
        REFERENCES sistema_locacao.tb_rental(id) ON DELETE CASCADE,
    CONSTRAINT fk_ra_accessory FOREIGN KEY (accessory_id) 
        REFERENCES sistema_locacao.tb_accessory(id) ON DELETE RESTRICT
);

-- Tabela de Pagamentos
CREATE TABLE sistema_locacao.tb_payment (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    rental_id UUID NOT NULL,
    amount NUMERIC(10, 2) NOT NULL,
    payment_method VARCHAR(50) NOT NULL,
    -- Ajustado para TIMESTAMPTZ com default em UTC
    payment_date TIMESTAMPTZ NOT NULL DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
    CONSTRAINT chk_payment_method CHECK (payment_method IN ('credit_card', 'debit_card', 'pix', 'cash')),
    CONSTRAINT fk_payment_rental FOREIGN KEY (rental_id) 
        REFERENCES sistema_locacao.tb_rental(id) ON DELETE CASCADE
);

-- Tabela de Avaliações
CREATE TABLE sistema_locacao.tb_rating (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    rental_id UUID NOT NULL,
    rating INT4 NOT NULL,
    comment TEXT,
    -- Ajustado para TIMESTAMPTZ com default em UTC
    created_at TIMESTAMPTZ DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC'),
    CONSTRAINT chk_rating_range CHECK (rating BETWEEN 1 AND 5),
    CONSTRAINT fk_rating_rental FOREIGN KEY (rental_id) 
        REFERENCES sistema_locacao.tb_rental(id) ON DELETE CASCADE
);