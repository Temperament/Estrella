ALTER TABLE `guildacademymembers`
CHANGE COLUMN `ChatBlock` `IsChatBlocked`  tinyint(1) NOT NULL DEFAULT 0 AFTER `Rank`;
