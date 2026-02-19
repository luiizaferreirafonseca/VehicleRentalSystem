/* SCRIPT DE MASSA DE TESTES - PROJETO VEICULORENTALSYSTEM
   OBJETIVO: Popular o banco para testes de endpoints de Pagamento e Acessórios.
   Ajustes realizados:
   1. Inclusão do schema 'sistema_locacao' em todos os comandos.
   2. Correção da coluna 'year' em tb_vehicles: de string/date para INT.
   3. Uso de CURRENT_TIMESTAMP para compatibilidade com o tipo TIMESTAMP.
   4. Subconsultas com LIMIT 1 para evitar erros de múltiplas linhas.
*/

-- 1. POPULANDO VEÍCULOS
-- Ajuste: Nome da coluna alterado de 'int' para 'year' e valor para inteiro[cite: 70, 135].
INSERT INTO sistema_locacao.tb_vehicles (id, model, year, brand, daily_rate, status, license_plate) 
VALUES 
(gen_random_uuid(), 'Onix', 2020, 'Chevrolet', 120.00, 'available', 'ABC1D23'), 
(gen_random_uuid(), 'Etios', 2019, 'Toyota', 110.00, 'available', 'DEF4G56'), 
(gen_random_uuid(), 'Renegade', 2021, 'Jeep', 250.00, 'maintenance', 'HIJ7K89');

SELECT * FROM sistema_locacao.tb_vehicles;


-- 2. POPULANDO USUÁRIOS
INSERT INTO sistema_locacao.tb_user (id, name, email, active) 
VALUES
(gen_random_uuid(), 'Tamiris Novaes', 'tamirisnovaes@gmail.com', true),
(gen_random_uuid(), 'Luiz Nogueira', 'luizinho@hotmail.com', true),
(gen_random_uuid(), 'Pedro de Jesus', 'pedro_jesus@yahoo.com', true);

SELECT * FROM sistema_locacao.tb_user;


-- 3. POPULANDO ACESSÓRIOS
INSERT INTO sistema_locacao.tb_accessory (id, name, daily_rate) 
VALUES
(gen_random_uuid(), 'GPS', 15.00),
(gen_random_uuid(), 'Cadeirinha Infantil', 20.00),
(gen_random_uuid(), 'Seguro Extra', 30.00);

SELECT * FROM sistema_locacao.tb_accessory;


-- 4. CRIANDO UMA LOCAÇÃO ATIVA
-- Ajuste: Uso de CURRENT_TIMESTAMP para campos TIMESTAMP e subconsultas com schema[cite: 85, 161].
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


-- 5. VINCULANDO ACESSÓRIO À LOCAÇÃO
-- Ajuste: Cálculo de total_price (unit_price * dias) conforme lógica do PDF[cite: 175, 181].
INSERT INTO sistema_locacao.tb_rental_accessory (rental_id, accessory_id, quantity, unit_price, total_price) 
VALUES (
    (SELECT id FROM sistema_locacao.tb_rental LIMIT 1),
    (SELECT id FROM sistema_locacao.tb_accessory WHERE name = 'GPS' LIMIT 1), 
    1, 15.00, 45.00
);

SELECT * FROM sistema_locacao.tb_rental_accessory;


-- 6. REGISTRANDO PAGAMENTO
-- Ajuste: O valor total contempla locação (360) + acessórios (45) = 405.
INSERT INTO sistema_locacao.tb_payment (id, rental_id, amount, payment_method, payment_date) 
VALUES (
    gen_random_uuid(), 
    (SELECT id FROM sistema_locacao.tb_rental LIMIT 1), 
    405.00, 
    'credit_card', 
    CURRENT_TIMESTAMP
);

SELECT * FROM sistema_locacao.tb_payment;


-- 7. ADICIONANDO UMA AVALIAÇÃO
INSERT INTO sistema_locacao.tb_rating (id, rental_id, rating, comment, created_at) 
VALUES (
    gen_random_uuid(), 
    (SELECT id FROM sistema_locacao.tb_rental LIMIT 1), 
    5, 
    'Carro em ótimo estado e atendimento excelente.', 
    CURRENT_TIMESTAMP
);

SELECT * FROM sistema_locacao.tb_rating;