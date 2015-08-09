/****************************************
 * 		 README			*
 ****************************************
 * Use this script whenever the groups  *
 * in the database are broken. It will  *
 * repair them automatically.			*
 ****************************************/

-- mark every group without members as 
-- non-existent

UPDATE
	`fiesta_world`.`groups`
SET
	`Exists`	= 	FALSE
WHERE
    (	`Member1`	=	NULL	)
AND (	`Member2`	= 	NULL	)
AND (	`Member3`	=	NULL	)
AND (	`Member4`	=	NULL	)
AND (	`Member5`	=	NULL	);

-- Remove non-existent group references
-- from the character table

UPDATE
	`fiesta_world`.`characters`
SET
	`GroupID`	=	NULL,
	`IsGroupMaster`	=	NULL
WHERE
	(
		SELECT COUNT(*) FROM `fiesta_world`.`groups`
		WHERE
		(	`groups`.`Id` = `characters`.`GroupID`)
		AND (`groups`.`Exists` = TRUE )
	) <= 0;

-- delete all group columns, that are non existant
DELETE
FROM	`fiesta_world`.`groups`
WHERE	`Exists` = false;



