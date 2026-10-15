-- Script gerado para revisão; não aplicado ao banco nesta entrega.
-- Origem: AddCustomTeamBadges; destino: AdicionarModeloDeUniforme.
-- Confirmar backup, histórico real e autorização antes de executar.
-- DDL MySQL faz commit implícito: a transação não desfaz ALTER TABLE.
START TRANSACTION;
ALTER TABLE `Teams` ADD `ModeloDoUniforme` varchar(32) CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261015001016_AdicionarModeloDeUniforme', '9.0.15');

COMMIT;
