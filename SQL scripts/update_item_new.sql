CREATE DEFINER=`root`@`localhost` PROCEDURE `update_item`(IN puniqueid BIGINT,
IN powner INT,IN pEquipt tinyint,
IN pslot TINYINT,
IN pamount SMALLINT)
BEGIN
UPDATE items SET Slot=pslot, Owner=powner, Amount=pamount,Equipt=pEquipt
WHERE ID = puniqueid;
END