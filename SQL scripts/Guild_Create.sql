CREATE DEFINER=`root`@`localhost` PROCEDURE `Guild_Create`(IN pName VARCHAR(16), IN pPassword VARCHAR(12), IN pAllowGuildWar SMALLINT(1),IN pCreaterID INT(11),IN pCreateTime TIMESTAMP,OUT pID INT)
proc_label:BEGIN
SET pID = FLOOR(1+RAND() * 200000);
IF EXISTS (SELECT GuildName FROM Guilds WHERE GuildName = pName)
	THEN
  SELECT -1;
	LEAVE proc_label;
	END IF;
WHILE EXISTS (SELECT ID FROM Guilds WHERE ID = pID) DO
SET pID = FLOOR(1+RAND() * 200000);
END WHILE;

START TRANSACTION;
INSERT INTO `Guilds` (ID,GuildName, pPassword, AllowGuildWar, CreaterID, CreateTime) 
VALUES (pID,pName, pPassword, pAllowGuildWar, pCreaterID, pCreateTime);
COMMIT;
IF @ROWCOUNT = 0 Then
   ROLLBACK;
   SELECT -2;
	LEAVE proc_label;
END IF;

START TRANSACTION;
INSERT INTO GuildAcademy (GuildID,Message,Points) VALUES (pID,'',0);
COMMIT;
IF @ROWCOUNT = 0 Then
   ROLLBACK;
   SELECT -3;
 	LEAVE proc_label;
END IF;
END