/* TEST DATA SCRIPT - VEHICLERENTALSYSTEM PROJECT
   PURPOSE: Populate the database for Payment and Accessories endpoint testing.
   Adjustments made:
   1. Added 'sistema_locacao' schema prefix to all commands.
   2. Fixed 'year' column in tb_vehicles: changed from string/date to INT.
   3. Used CURRENT_TIMESTAMP for compatibility with TIMESTAMP type.
   4. Subqueries with LIMIT 1 to avoid multiple-row errors.
*/
/* PT-BR: SCRIPT DE MASSA DE TESTES - PROJETO VEICULORENTALSYSTEM
   PT-BR: OBJETIVO: Popular o banco para testes de endpoints de Pagamento e Acessórios.
   PT-BR: Ajustes realizados:
   PT-BR: 1. Inclusão do schema 'sistema_locacao' em todos os comandos.
   PT-BR: 2. Correção da coluna 'year' em tb_vehicles: de string/date para INT.
   PT-BR: 3. Uso de CURRENT_TIMESTAMP para compatibilidade com o tipo TIMESTAMP.
   PT-BR: 4. Subconsultas com LIMIT 1 para evitar erros de múltiplas linhas.
*/

-- 1. POPULATING VEHICLES
-- PT-BR: 1. POPULANDO VEÍCULOS
-- Adjustment: Column name changed from 'int' to 'year' and value to integer.
-- PT-BR: Ajuste: Nome da coluna alterado de 'int' para 'year' e valor para inteiro.
INSERT INTO sistema_locacao.tb_vehicles (id, model, year, brand, daily_rate, status, license_plate) 
VALUES 
(gen_random_uuid(), 'Onix', 2020, 'Chevrolet', 120.00, 'available', 'ABC1D23'), 
(gen_random_uuid(), 'Etios', 2019, 'Toyota', 110.00, 'available', 'DEF4G56'), 
(gen_random_uuid(), 'Renegade', 2021, 'Jeep', 250.00, 'maintenance', 'HIJ7K89');

SELECT * FROM sistema_locacao.tb_vehicles;


-- 2. POPULATING USERS
-- PT-BR: 2. POPULANDO USUÁRIOS
INSERT INTO sistema_locacao.tb_user (id, name, email, active) 
VALUES
(gen_random_uuid(), 'Tamiris Novaes', 'tamirisnovaes@gmail.com', true),
(gen_random_uuid(), 'Luiz Nogueira', 'luizinho@hotmail.com', true),
(gen_random_uuid(), 'Pedro de Jesus', 'pedro_jesus@yahoo.com', true);

SELECT * FROM sistema_locacao.tb_user;


-- 3. POPULATING ACCESSORIES
-- PT-BR: 3. POPULANDO ACESSÓRIOS
INSERT INTO sistema_locacao.tb_accessory (id, name, daily_rate) 
VALUES
(gen_random_uuid(), 'GPS', 15.00),
(gen_random_uuid(), 'Cadeirinha Infantil', 20.00),
(gen_random_uuid(), 'Seguro Extra', 30.00);

SELECT * FROM sistema_locacao.tb_accessory;


-- 4. CREATING AN ACTIVE RENTAL
-- PT-BR: 4. CRIANDO UMA LOCAÇÃO ATIVA
-- Adjustment: Using CURRENT_TIMESTAMP for TIMESTAMP fields and subqueries with schema.
-- PT-BR: Ajuste: Uso de CURRENT_TIMESTAMP para campos TIMESTAMP e subconsultas com schema.
INSERT INTO sistema_locacao.tb_rental (
    id, start_date, expected_end_date, actual_end_date, total_amount, 
    penalty_fee, status, vehicle_id, user_id, daily_rate
) VALUES (
    gen_random_uuid(), 
    CURRENT_TIMESTAMP, 
    CURRENT_TIMESTAMP + INTERVAL '3 days', 
    NULL, 
    360.00, 
    0.00, 
    'active', 
    (SELECT id FROM sistema_locacao.tb_vehicles WHERE model = 'Onix' LIMIT 1), 
    (SELECT id FROM sistema_locacao.tb_user WHERE name = 'Tamiris Novaes' LIMIT 1), 
    120.00
);

SELECT * FROM sistema_locacao.tb_rental;


-- 5. LINKING ACCESSORY TO RENTAL
-- PT-BR: 5. VINCULANDO ACESSÓRIO À LOCAÇÃO
-- Adjustment: Calculation of total_price (unit_price * days) according to PDF logic.
-- PT-BR: Ajuste: Cálculo de total_price (unit_price * dias) conforme lógica do PDF.
INSERT INTO sistema_locacao.tb_rental_accessory (rental_id, accessory_id, quantity, unit_price, total_price) 
VALUES (
    (SELECT id FROM sistema_locacao.tb_rental LIMIT 1),
    (SELECT id FROM sistema_locacao.tb_accessory WHERE name = 'GPS' LIMIT 1), 
    1, 15.00, 45.00
);

SELECT * FROM sistema_locacao.tb_rental_accessory;


-- 6. REGISTERING PAYMENT
-- PT-BR: 6. REGISTRANDO PAGAMENTO
-- Adjustment: The total amount covers rental (360) + accessories (45) = 405.
-- PT-BR: Ajuste: O valor total contempla locação (360) + acessórios (45) = 405.
INSERT INTO sistema_locacao.tb_payment (id, rental_id, amount, payment_method, payment_date) 
VALUES (
    gen_random_uuid(), 
    (SELECT id FROM sistema_locacao.tb_rental LIMIT 1), 
    405.00, 
    'credit_card', 
    CURRENT_TIMESTAMP
);

SELECT * FROM sistema_locacao.tb_payment;


-- 7. ADDING A RATING
-- PT-BR: 7. ADICIONANDO UMA AVALIAÇÃO
INSERT INTO sistema_locacao.tb_rating (id, rental_id, rating, comment, created_at) 
VALUES (
    gen_random_uuid(), 
    (SELECT id FROM sistema_locacao.tb_rental LIMIT 1), 
    5, 
    'Carro em ótimo estado e atendimento excelente.', 
    CURRENT_TIMESTAMP
);

SELECT * FROM sistema_locacao.tb_rating;