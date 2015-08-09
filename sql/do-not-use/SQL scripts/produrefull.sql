-- Procedure structure for `give_equip`
-- ----------------------------
DROP PROCEDURE IF EXISTS `give_equip`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `give_equip`(
OUT puniqueid BIGINT,
IN powner INT,
IN pslot SMALLINT,
IN pequipID SMALLINT unsigned)
BEGIN
INSERT INTO equips (Owner, Slot, EquipID) VALUES (powner, pslot, pequipid);
SET puniqueid = LAST_INSERT_ID();
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `give_item`
-- ----------------------------
DROP PROCEDURE IF EXISTS `give_item`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `give_item`(
OUT puniqueid BIGINT,
IN powner INT,
IN pslot TINYINT,
IN pitemid SMALLINT unsigned,
IN pamount SMALLINT)
BEGIN
INSERT INTO items (Owner, Slot, ItemID, Amount) VALUES (powner, pslot, pitemid, pamount);
SET puniqueid = LAST_INSERT_ID();
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `GuildAcademyMember_Create`
-- ----------------------------
DROP PROCEDURE IF EXISTS `GuildAcademyMember_Create`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GuildAcademyMember_Create`(IN `pGuildID` int,IN `pCharacterID` int,IN `pRegisterDate` timestamp,IN `pRank` smallint)
proc:BEGIN
IF EXISTS(SELECT CharID FROM GuildAcademyMembers WHERE CharID=pCharacterID)
Then
SELECT -1;
LEAVE proc;
END IF;
START TRANSACTION;
INSERT INTO GuildAcademyMembers (GuildID,CharID,RegisterDate,Rank) VALUES (pGuildID,pCharacterID,pRegisterDate,pRank);
COMMIT;
IF @ROWCOUNT = 0 Then
   ROLLBACK;
   SELECT -2;
	LEAVE proc;
END IF;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `GuildAcademyMember_Remove`
-- ----------------------------
DROP PROCEDURE IF EXISTS `GuildAcademyMember_Remove`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GuildAcademyMember_Remove`(IN `pGuildID` int,IN `pCharacterID` int)
Removep:BEGIN
Start Transaction;
DELETE FROM GuildAcademyMembers WHERE CharacterID= pCharacterID;
COMMIT;
IF @ROWCOUNT = 0 Then
   ROLLBACK;
   SELECT -1;
	LEAVE Removep;
END IF;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `GuildAcademyMember_Save`
-- ----------------------------
DROP PROCEDURE IF EXISTS `GuildAcademyMember_Save`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GuildAcademyMember_Save`(IN `pGuildID` INT ,IN `pCharacterID` INT ,IN `pIsChatBlocked` tinyint ,IN `pRank` smallint)
proc_label:BEGIN
START TRANSACTION;
UPDATE GuildAcademyMembers SET GuildID=pGuildID,IsChatBlocked=pIsChatBlocked,Rank=pRank WHERE CharID=pCharacterID;
COMMIT;
IF @ROWCOUNT = 0 Then
   ROLLBACK;
	LEAVE proc_label;
END IF;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `GuildMember_Create`
-- ----------------------------
DROP PROCEDURE IF EXISTS `GuildMember_Create`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GuildMember_Create`(IN `pGuildID` int,IN `pCharacterID` int,IN `pRank` tinyint,IN `pCorp` smallint)
GMember_Create:BEGIN
IF EXISTS (SELECT CharID FROM Characters WHERE CharID = pCharacterID)
 THEN
SELECT -1;
END IF;
IF EXISTS (SELECT CharID FROM GuildMembers WHERE CharID = pCharacterID)
 THEN
SELECT -1;
END IF;
Start Transaction;
INSERT INTO GuildMembers (GuildID,CharID,Rank,Korp) VALUES (pGuildID,pCharacterID,pRank,pCorp);
COMMIT;
IF @ROWCOUNT = 0 Then
   ROLLBACK;
   SELECT -1;
	LEAVE GMember_Create;
END IF;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `GuildMember_Remove`
-- ----------------------------
DROP PROCEDURE IF EXISTS `GuildMember_Remove`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GuildMember_Remove`(IN `pGuildID` int,IN `pCharacterID` int)
Removep:BEGIN
Start Transaction;
DELETE FROM GuildMembers WHERE CharacterID= pCharacterID;
COMMIT;
IF @ROWCOUNT = 0 Then
   ROLLBACK;
   SELECT -1;
	LEAVE Removep;
END IF;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `GuildMember_Save`
-- ----------------------------
DROP PROCEDURE IF EXISTS `GuildMember_Save`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `GuildMember_Save`(IN `pGuildID` int,IN `pCharacterID` int,IN `pRank` tinyint,IN `pCorp` smallint)
proc_label:BEGIN
START TRANSACTION;
UPDATE GuildMembers SET GuildID=pGuildID,Rank=pRank,Korp=pCorp WHERE CharID=pCharacterID;
COMMIT;
IF @ROWCOUNT = 0 Then
   ROLLBACK;
	LEAVE proc_label;
END IF;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `Guild_Create`
-- ----------------------------
DROP PROCEDURE IF EXISTS `Guild_Create`;
DELIMITER ;;
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
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `update_equip`
-- ----------------------------
DROP PROCEDURE IF EXISTS `update_equip`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `update_equip`(
IN puniqueid BIGINT,
IN powner INT,
IN pslot SMALLINT,
IN pupgrades TINYINT,
IN pstr SMALLINT UNSIGNED,
IN pend SMALLINT UNSIGNED,
IN pdex SMALLINT UNSIGNED,
IN pspr SMALLINT UNSIGNED,
IN pint SMALLINT UNSIGNED)
BEGIN
UPDATE equips SET Slot=pslot,
Owner=powner,
Upgrades=pupgrades,
iSTR=pstr,
iEND=pend,
iDEX=pdex,
ispr=pspr,
iINT = pint
WHERE ID = puniqueid;
END
;;
DELIMITER ;

-- ----------------------------
-- Procedure structure for `update_item`
-- ----------------------------
DROP PROCEDURE IF EXISTS `update_item`;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `update_item`(IN puniqueid BIGINT,
IN powner INT,IN pEquipt tinyint,
IN pslot TINYINT,
IN pamount SMALLINT)
BEGIN
UPDATE items SET Slot=pslot, Owner=powner, Amount=pamount,Equipt=pEquipt
WHERE ID = puniqueid;
END
;;
DELIMITER ;
