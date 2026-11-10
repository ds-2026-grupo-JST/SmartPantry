-- Datos de prueba locales para el worker de vencimientos (TP08, RF-15).
-- Ejecutar en DBeaver como script (Alt + X) contra la base SmartPantry.
-- Se puede ejecutar varias veces: primero borra la despensa de prueba anterior.
-- Las fechas son relativas a hoy en UTC, igual que el worker.

DELETE FROM AppExpirationWarnings
WHERE PantryId IN (SELECT Id FROM AppPantries WHERE Name = 'Despensa de prueba');

DELETE FROM AppPantries WHERE Name = 'Despensa de prueba'; -- sus ítems se borran en cascada

SET @pantryId = UUID();

INSERT INTO AppPantries (Id, UserId, Name, ExtraProperties, ConcurrencyStamp)
VALUES (@pantryId, UUID(), 'Despensa de prueba', '{}', REPLACE(UUID(), '-', ''));

-- Status: 1 = Available, 2 = Consumed
INSERT INTO AppPantryItems (Id, ProductId, ExpirationDate, Status, PantryId) VALUES
(UUID(), UUID(), DATE_ADD(UTC_DATE(), INTERVAL 2 DAY), 1, @pantryId),   -- vence en 2 días  -> ExpiringSoon
(UUID(), UUID(), DATE_SUB(UTC_DATE(), INTERVAL 1 DAY), 1, @pantryId),   -- venció ayer      -> Expired
(UUID(), UUID(), DATE_ADD(UTC_DATE(), INTERVAL 10 DAY), 1, @pantryId),  -- lejos del umbral -> sin advertencia
(UUID(), UUID(), NULL, 1, @pantryId),                                   -- sin fecha        -> sin advertencia
(UUID(), UUID(), DATE_SUB(UTC_DATE(), INTERVAL 5 DAY), 2, @pantryId);   -- consumido        -> sin advertencia