CREATE TABLE `guildstorage` (
  `GuildID` int(11) NOT NULL,
  `ItemID` smallint(6) NOT NULL,
  `Slot` tinyint(4) NOT NULL,
  `Amount` int(11) NOT NULL DEFAULT '1'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
