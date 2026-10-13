START TRANSACTION;
CREATE INDEX `IX_Players_TeamId` ON `Players` (`TeamId`);

ALTER TABLE `Players` ADD CONSTRAINT `FK_Players_Teams_TeamId` FOREIGN KEY (`TeamId`) REFERENCES `Teams` (`Id`) ON DELETE RESTRICT;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261013014146_AddPlayerTeamRelationship', '9.0.15');

COMMIT;

