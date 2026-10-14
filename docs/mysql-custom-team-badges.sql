-- SQL gerado para revisão; NÃO aplicado ao banco nesta entrega.
-- Origem: AddPlayerTeamRelationship; destino: AddCustomTeamBadges.
-- Exige conferir o histórico real, backup e autorização antes da execução.
-- ALTER TABLE provoca commit implícito no MySQL: START TRANSACTION não torna
-- a migration de schema atômica nem substitui backup/revisão em uma cópia.
START TRANSACTION;
ALTER TABLE `Teams` ADD `BadgeTemplate` varchar(32) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Teams` ADD `IsCustom` tinyint(1) NOT NULL DEFAULT FALSE;

ALTER TABLE `Teams` ADD `PrimaryColor` varchar(7) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Teams` ADD `SecondaryColor` varchar(7) CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261014003412_AddCustomTeamBadges', '9.0.15');

COMMIT;
