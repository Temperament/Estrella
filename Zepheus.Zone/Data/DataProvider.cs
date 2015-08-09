using System;
using System.Collections.Generic;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.Util;
using Zepheus.Database;
using System.Data;
using Zepheus.Zone.Game;
using Zepheus.Zone.Data;
using Zepheus.Database.DataStore;
using Zepheus.Zone.Game.Guilds;
using Zepheus.Zone.Game.Guilds.Academy;

namespace Zepheus.Zone.Data
{
	[ServerModule(Util.InitializationStage.SpecialDataProvider)]
	public sealed class DataProvider
	{
		public Dictionary<ushort, MapInfo> MapsByID { get; private set; }
		public Dictionary<string, MapInfo> MapsByName { get; private set; }
		public Dictionary<string, LinkTable> NpcLinkTable { get; private set; }
		public Dictionary<string, MobInfoServer> MobData { get; private set; }
		public Dictionary<ushort, BlockInfo> Blocks { get; private set; }
		public Dictionary<Job, List<FiestaBaseStat>> JobInfos { get; private set; }
		public Dictionary<ushort, ItemInfo> ItemsByID { get; private set; }
		public Dictionary<string, ItemInfo> ItemsByName { get; private set; }
		public Dictionary<string, DropGroupInfo> DropGroups { get; private set; }
		public Dictionary<ushort, MobInfo> MobsByID { get; private set; }
		public Dictionary<string, MobInfo> MobsByName { get; private set; }
		public Dictionary<ushort, ItemUseEffectInfo> ItemUseEffects { get; private set; }
		public Dictionary<string, RecallCoordinate> RecallCoordinates { get; private set; }
		public Dictionary<byte, ulong> ExpTable { get; private set; }
		public Dictionary<ushort, MiniHouseInfo> MiniHouses { get; private set; }
        public Dictionary<ushort, Mount> MountyByItemID { get; private set; }
        public Dictionary<ushort, Mount> MountyByHandleID { get; private set; }
		public Dictionary<ushort, ActiveSkillInfo> ActiveSkillsByID { get; private set; }
		public Dictionary<string, ActiveSkillInfo> ActiveSkillsByName { get; private set; }
        public Dictionary<ushort,MasterRewardState> MasterRewardStates  { get; private set; }
        public Dictionary<int, Guild> GuildsByID { get; private set; }
        public Dictionary<string, Guild> GuildsByName { get; private set; }

		public static DataProvider Instance { get; private set; }

		public DataProvider()
		{
			//LoadMaps();
			LoadMaps(null); //this loads all the maps, but we get issues with zone spread (fix later)
			LoadJobStats();
			LoadExpTable();
			LoadItemInfo();
			LoadRecallCoordinates();
			LoadMobs();
			LoadDrops();
			LoadItemInfoServer();
            LoadItemStats();
			LoadMiniHouseInfo();
			LoadActiveSkills();
			LoadVendors();
            LoadMounts();
            LoadMasterRewardStates();
  
		}

		[InitializerMethod]
		public static bool Load()
		{
			try
			{

				Instance = new DataProvider();
				return true;
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Error loading dataprovider: {0}", ex.ToString());
				return false;
			}
		}

		private static readonly string[] dropGroupNames = new string[] { "DropGroupA", "DropGroupB", "RandomOptionDropGroup" };
		private void LoadItemInfoServer()
		{
			try
			{
				DataTable itemDataInf = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					itemDataInf = dbClient.ReadDataTable("SELECT  *FROM data_iteminfoserver");
				}
				foreach (DataRow row in itemDataInf.Rows)
				{
					ushort itemid = GetDataTypes.GetUshort(row["ID"]);
					ItemInfo item;
					if (ItemsByID.TryGetValue(itemid, out item))
					{
						for (int i = 0; i < 3; i++)
						{
							string groupname = (string)row[dropGroupNames[i]];
							if (groupname.Length > 2)
							{
								DropGroupInfo group;
								if (DropGroups.TryGetValue(groupname, out group))
								{
									group.Items.Add(item);
								}
								else
								{
									// Log.WriteLine(LogLevel.Warn, "{0} was assigned to unknown DropGroup {1}.", item.InxName, groupname);
								}
							}
						}
					}
					else Log.WriteLine(LogLevel.Warn, "ItemInfoServer has obsolete item ID: {0}.", itemid);
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Error loading ItemInfoServer.shn: {0}", ex);
			}
		}
        private void LoadItemStats()
        {
            	DataTable itemStats = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					itemStats = dbClient.ReadDataTable("SELECT  *FROM ItemStats");
				}
                foreach (DataRow row in itemStats.Rows)
                {
                   string Iteminx = row["itemIndex"].ToString();
                    ItemInfo Iteminf;
                    if (!this.ItemsByName.TryGetValue(Iteminx, out Iteminf))
                    {
                        Log.WriteLine(LogLevel.Warn, "Can not Find item {0} by ItemStatLoad", Iteminx);
                        continue;
                    }
                     Iteminf.Stats = ItemStats.LoadItemStatsFromDatabase(row);
                }
        }
        private void LoadMasterRewardStates()
        {
            MasterRewardStates = new Dictionary<ushort, MasterRewardState>();
                try
                {
                    DataTable RewardStates = null;
                    using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
                    {
                        RewardStates = dbClient.ReadDataTable("SELECT  *FROM MasterRewardStates");
                    }
                    if (RewardStates != null)
                    {
                        foreach (DataRow row in RewardStates.Rows)
                        {
                            MasterRewardState State = new MasterRewardState(row);
                            this.MasterRewardStates.Add(State.ItemID, State);
                        }
                    }
                    Log.WriteLine(LogLevel.Info, "Loaded {0} MasterRewardStates", this.MasterRewardStates.Count);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(LogLevel.Exception, "Error loading MasterRewardStatesTable: {0}", ex);
                }
        }
		private void LoadDrops()
		{
			DropGroups = new Dictionary<string, DropGroupInfo>();
			try
			{
				DataTable dropgroupinfoData = null;
				DataTable itemdroptableData = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					dropgroupinfoData = dbClient.ReadDataTable("SELECT  *FROM dropgroupinfo");
					itemdroptableData = dbClient.ReadDataTable("SELECT  *FROM itemdroptable");
				}
				if (dropgroupinfoData != null)
				{
					foreach (DataRow row in dropgroupinfoData.Rows)
					{
						DropGroupInfo info = DropGroupInfo.Load(row);
						if (DropGroups.ContainsKey(info.GroupID))
						{
							//Log.WriteLine(LogLevel.Warn, "Duplicate DropGroup ID found: {0}.", info.GroupID);
							continue;
						}
						DropGroups.Add(info.GroupID, info);
					}
				}
				int dropcount = 0;
				if (itemdroptableData != null)
				{
					foreach (DataRow row in itemdroptableData.Rows)
					{
						string mobid = (string)row["MobId"];
						MobInfo mob;
						if (MobsByName.TryGetValue(mobid, out mob))
						{
							mob.MinDropLevel = (byte)row["MinLevel"];
							mob.MaxDropLevel = (byte)row["MaxLevel"];
							string dropgroup = (string)row["GroupID"];
							if (dropgroup.Length <= 2) continue;
							DropGroupInfo group;
							if (DropGroups.TryGetValue(dropgroup, out group))
							{
								float rate = (float)row["Rate"];
								DropInfo info = new DropInfo(group, rate);
								mob.Drops.Add(info);
								++dropcount;
							}
							else
							{
								//this seems to happen a lot so disable this for the heck of it.
								//Log.WriteLine(LogLevel.Warn, "Could not find DropGroup {0}.", dropgroup);
							}
						}
						// else  Log.WriteLine(LogLevel.Warn, "Could not find mobname: {0} for drop.", mobid);
					}
				}
				//first we load the dropgroups
				Log.WriteLine(LogLevel.Info, "Loaded {0} DropGroups, with {1} drops in total.", DropGroups.Count, dropcount);
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Error loading DropTable: {0}", ex);
			}
		}
        private void LoadMounts()
        {
            MountyByItemID = new Dictionary<ushort, Mount>();
            MountyByHandleID = new Dictionary<ushort, Mount>();
            DataTable MountData = null;
            int Mountcounter = 0;
            using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
            {
                MountData = dbClient.ReadDataTable("SELECT  *FROM Mounts");
            }
            if (MountData != null)
            {
                foreach (DataRow row in MountData.Rows)
                {
                    Mount mount = Mount.LoadMount(row);

                    if (!MountyByItemID.ContainsKey(mount.ItemID))
                    {
                        MountyByItemID.Add(mount.ItemID, mount);
                        MountyByHandleID.Add(mount.Handle, mount);
                        Mountcounter++;
                    }
                }
                Log.WriteLine(LogLevel.Info, "Loaded {0} Mounts.", Mountcounter);
            }
        }

		private void LoadActiveSkills()
		{
			ActiveSkillsByID = new Dictionary<ushort, ActiveSkillInfo>();
			ActiveSkillsByName = new Dictionary<string, ActiveSkillInfo>();
			DataTable activeSkillData = null;
			using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
			{
				activeSkillData = dbClient.ReadDataTable("SELECT  *FROM activeskill");
			}
			if (activeSkillData != null)
			{
				foreach (DataRow row in activeSkillData.Rows)
				{
					ActiveSkillInfo info = ActiveSkillInfo.Load(row);
					if (ActiveSkillsByID.ContainsKey(info.ID) || ActiveSkillsByName.ContainsKey(info.Name))
					{

						Log.WriteLine(LogLevel.Warn, "Duplicate ActiveSkill found: {0} ({1})", info.ID, info.Name);
						continue;
					}
					ActiveSkillsByID.Add(info.ID, info);
					ActiveSkillsByName.Add(info.Name, info);
				}
			}
			Log.WriteLine(LogLevel.Info, "Loaded {0} ActiveSkills.", ActiveSkillsByID.Count);
		}
		private void LoadRecallCoordinates()
		{
			RecallCoordinates = new Dictionary<string, RecallCoordinate>();
			DataTable RecallData = null;
			using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
			{
				RecallData = dbClient.ReadDataTable("SELECT  *FROM recall");
			}
			if (RecallData != null)
			{
				foreach (DataRow row in RecallData.Rows)
				{
					var rc = RecallCoordinate.Load(row);
					RecallCoordinates.Add(rc.ItemIndex, rc);
				}
			}
			Log.WriteLine(LogLevel.Info, "Loaded {0} recall coordinates.", RecallCoordinates.Count);
		}
		private void LoadMobs()
		{
			try
			{
				MobsByID = new Dictionary<ushort, MobInfo>();
				MobsByName = new Dictionary<string, MobInfo>();
				DataTable data_mobinfo = null;
				DataTable data_MobInfoServer = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					data_mobinfo = dbClient.ReadDataTable("SELECT  *FROM data_mobinfo");
					data_MobInfoServer = dbClient.ReadDataTable("SELECT  *FROM data_MobInfoServer");
				}
				if (data_mobinfo != null)
				{
					foreach (DataRow row in data_mobinfo.Rows)
					{
						MobInfo info = MobInfo.Load(row);
						if (MobsByID.ContainsKey(info.ID) || MobsByName.ContainsKey(info.Name))
						{
							Log.WriteLine(LogLevel.Warn, "Duplicate mob ID found in MobInfo.shn: {0}.", info.ID);
							continue;
						}
						MobsByID.Add(info.ID, info);

						MobsByName.Add(info.Name, info);
					}
				}
				MobData = new Dictionary<string, MobInfoServer>();
				if (data_MobInfoServer != null)
				{
					foreach (DataRow row in data_MobInfoServer.Rows)
					{
						MobInfoServer info = MobInfoServer.Load(row);
						if (MobData.ContainsKey(info.InxName))
						{
							Log.WriteLine(LogLevel.Warn, "Duplicate mob ID found in MobInfoServer.shn: {0}.", info.InxName);
							continue;
						}
						//  Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE Vendors SET NPCID='" + info.ID + "' WHERE NPCID='" + info.InxName + "'");
						MobData.Add(info.InxName, info);
					}
				   /* foreach (string mobs in MobsByName.Keys)//check mobdata this is for database devs
					{
						if(!MobData.ContainsKey(mobs))
							{
								Console.WriteLine(mobs);
							}
					}*/
				}
				Log.WriteLine(LogLevel.Info, "Loaded {0} mobs.", MobsByID.Count);

			}
			catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Error loading MobInfo {0}", ex.Message);
			}
		}

		public MobInfo GetMobInfo(ushort id)
		{
			MobInfo toret;
			if (MobsByID.TryGetValue(id, out toret))
			{
				return toret;
			}
			else return null;
		}

		public ushort GetMobIDFromName(string name)
		{
			MobInfoServer mis = null;
			if (MobData.TryGetValue(name, out mis))
			{
				return (ushort)mis.ID;
			}
			return 0;
		}

		public FiestaBaseStat GetBaseStats(Job job, byte level)
		{
			return JobInfos[job][level - 1];
		}

		public void LoadJobStats()
		{
			// Temp set a dict for every job/filename
			Dictionary<string, Job> sj = new Dictionary<string, Job>();
			sj.Add("Archer", Job.Archer);
			sj.Add("Assassin", Job.Reaper);
			sj.Add("Chaser", Job.Gambit);
			sj.Add("Cleric", Job.Cleric);
			sj.Add("CleverFighter", Job.CleverFighter);
			sj.Add("Closer", Job.Spectre);
			sj.Add("Cruel", Job.Renegade);
			sj.Add("Enchanter", Job.Enchanter);
			sj.Add("Fighter", Job.Fighter);
			sj.Add("Gladiator", Job.Gladiator);
			sj.Add("Guardian", Job.Guardian);
			sj.Add("HawkArcher", Job.HawkArcher);
			sj.Add("HighCleric", Job.HighCleric);
			sj.Add("HolyKnight", Job.HolyKnight);
			sj.Add("Joker", Job.Trickster); // hah
			sj.Add("Knight", Job.Knight);
			sj.Add("Mage", Job.Mage);
			sj.Add("Paladin", Job.Paladin);
			sj.Add("Ranger", Job.Ranger);
			sj.Add("Scout", Job.Scout);
			sj.Add("SharpShooter", Job.SharpShooter);
			sj.Add("Warrock", Job.Warlock);
			sj.Add("Warrior", Job.Warrior);
			sj.Add("Wizard", Job.Wizard);
			sj.Add("WizMage", Job.WizMage);

			// DAMN THATS A LONG LIST
			// STOP COMPLAINING ABOUT SUCH SHIT DAMMIT

			Log.WriteLine(LogLevel.Debug, "Trying to load {0} jobs.", sj.Count);
			JobInfos = new Dictionary<Job, List<FiestaBaseStat>>();

			foreach (var kvp in sj)
			{
				DataTable baseData = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					baseData = dbClient.ReadDataTable("SELECT  * FROM BaseStats WHERE Class='" + kvp.Value + "'");
				}
				if (baseData != null)
				{
					List<FiestaBaseStat> stats = new List<FiestaBaseStat>();
					foreach (DataRow row in baseData.Rows)
					{
						stats.Add(FiestaBaseStat.Load(row, kvp.Value));
					}
					JobInfos.Add(kvp.Value, stats);
					Log.WriteLine(LogLevel.Debug, "Loaded {0} levels for job {1}", stats.Count, kvp.Value.ToString());
				}
			}
		}

		public void LoadItemInfo()
		{
			Dictionary<string, ItemUseEffectInfo> effectcache = new Dictionary<string, ItemUseEffectInfo>();
			ItemUseEffects = new Dictionary<ushort, ItemUseEffectInfo>();
			DataTable effectid = null;
			DataTable dataItem = null;
			using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
			{
				effectid = dbClient.ReadDataTable("SELECT  * FROM data_itemuseeffect");
				dataItem = dbClient.ReadDataTable("SELECT  * FROM data_iteminfo");
			}

			if (effectid != null)
			{
				foreach (DataRow row in effectid.Rows)
				{
					string inxname;
					ItemUseEffectInfo info = ItemUseEffectInfo.Load(row, out inxname);
					effectcache.Add(inxname, info);
				}
			}

			ItemsByID = new Dictionary<ushort, ItemInfo>();
			ItemsByName = new Dictionary<string, ItemInfo>();
			if (dataItem != null)
			{
				foreach (DataRow row in dataItem.Rows)
				{
					ItemInfo info = ItemInfo.Load(row);
					if (ItemsByID.ContainsKey(info.ItemID) || ItemsByName.ContainsKey(info.InxName))
					{
						Log.WriteLine(LogLevel.Warn, "Duplicate item found ID: {0} ({1}).", info.ItemID, info.InxName);

						continue;
					}
					ItemsByID.Add(info.ItemID, info);
					ItemsByName.Add(info.InxName, info);

					if (effectcache.ContainsKey(info.InxName))
					{
						if (info.Type != ItemType.Useable)
						{
							Log.WriteLine(LogLevel.Warn, "Invalid useable item: {0} ({1})", info.ItemID, info.InxName);
							continue;
						}
						ItemUseEffectInfo effectinfo = effectcache[info.InxName];
						effectinfo.ID = info.ItemID;
						ItemUseEffects.Add(effectinfo.ID, effectinfo);
					}
				}
			}
			effectcache.Clear();
			Log.WriteLine(LogLevel.Info, "Loaded {0} items.", ItemsByID.Count);
		}


		public ItemInfo GetItemInfo(ushort id)
		{
			ItemInfo info;
			if (ItemsByID.TryGetValue(id, out info))
			{
				return info;
			}
			else return null;
		}

		public void LoadExpTable()
		{
			try
			{

				ExpTable = new Dictionary<byte, ulong>();
				DataTable dataItem = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					dataItem = dbClient.ReadDataTable("SELECT  *FROM expTable");
				}

				if (dataItem != null)
				{
					foreach (DataRow row in dataItem.Rows)
					{
						byte level = (byte)row["Level"];
						ulong exp = (ulong)row["Exp"];
						ExpTable.Add(level, exp);
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Error loading ExpTable: {0}", ex.Message);

			}
		}
		public ulong GetMaxExpForLevel(byte pLevel)
		{
			ulong ret = 0;
			if (!ExpTable.TryGetValue(pLevel, out ret))
			{
				Log.WriteLine(LogLevel.Warn, "Something tried to get the amount of EXP for level {0} (which is higher than it's max, {1}). Please backTrade the calls to this function!", pLevel, ExpTable.Count);
				Log.WriteLine(LogLevel.Warn, Environment.StackTrace);
			}
			return ret;
		}

		public void LoadMaps(List<ushort> toload = null)
		{
			MapsByID = new Dictionary<ushort, MapInfo>();
			MapsByName = new Dictionary<string, MapInfo>();
			DataTable mapDataInf = null;
			DataTable linktableData = null;
			DataTable shineNpcData = null;
			using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
			{
				mapDataInf = dbClient.ReadDataTable("SELECT  *FROM mapinfo");
				linktableData = dbClient.ReadDataTable("SELECT *FROM LinkTable");
				shineNpcData = dbClient.ReadDataTable("SELECT *FROM ShineNpc");
			}
			if (mapDataInf != null)
			{
				foreach (DataRow row in mapDataInf.Rows)
				{
					MapInfo info = MapInfo.Load(row);
					info.NPCs = new List<ShineNpc>();
					if (MapsByID.ContainsKey(info.ID))
					{
						Log.WriteLine(LogLevel.Debug, "Duplicate map ID {0} ({1})", info.ID, info.FullName);
						MapsByID.Remove(info.ID);
						MapsByName.Remove(info.ShortName);
					}
					if (toload == null || toload.Contains(info.ID))
					{
						MapsByID.Add(info.ID, info);
						MapsByName.Add(info.ShortName, info);
					}
				}
			}
			Blocks = new Dictionary<ushort, BlockInfo>();
			foreach (var map in MapsByID.Values)
			{
				DataTable blockData = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					blockData = dbClient.ReadDataTable("SELECT  *FROM blockinfo WHERE MapID='" + map.ID + "'");
				}
				if (blockData != null)
				{
					if (blockData.Rows.Count > 0)
					{
						foreach (DataRow row in blockData.Rows)
						{
							BlockInfo info = new BlockInfo(row, map.ID);

							Blocks.Add(map.ID, info);
						}
					}
					else
					{
						Log.WriteLine(LogLevel.Warn, "No BlockInfo for Map {0}", map.ShortName);
					}
				}
			}
			NpcLinkTable = new Dictionary<string, LinkTable>();
			if (linktableData != null)
			{
				foreach (DataRow row in linktableData.Rows)
				{
					LinkTable link = LinkTable.Load(row);
					if (Program.IsLoaded(GetMapidFromMapShortName(link.MapClient)))
					{
						NpcLinkTable.Add(link.argument, link);
					}
				}
			}
			if (shineNpcData != null)
			{
				foreach (DataRow row in shineNpcData.Rows)
				{
					ShineNpc npc = ShineNpc.Load(row);
					MapInfo mi = null;
					if (Program.IsLoaded(GetMapidFromMapShortName(npc.Map)) && MapsByName.TryGetValue(npc.Map, out mi))
					{

						mi.NPCs.Add(npc);
					}
				}
			}
			Log.WriteLine(LogLevel.Info, "Loaded {0} maps.", MapsByID.Count);
		}
		public void LoadVendors()
		{
			foreach (var map in MapsByID.Values)
			{
				foreach (var npc in map.NPCs)
					if (npc.Flags == (ushort)Data.NpcFlags.Vendor)
					{
						DataTable vendorData = null;
						using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
						{
							vendorData = dbClient.ReadDataTable("SELECT *FROM Vendors WHERE NPCID='" + npc.MobID + "'");
						}
						if (vendorData != null)
						{
							foreach (DataRow row in vendorData.Rows)
							{
								Vendor vendor = new Vendor
								{
									ItemID = (ushort)row["ItemID"],
									InvSlot = (byte)row["InvSlot"]
								};
								ItemInfo item;
								if (ItemsByID.TryGetValue(vendor.ItemID, out item))
								{
									vendor.Item = item;
									vendor.VendorName = npc.MobName;
									npc.VendorItems.Add(vendor);
								}
							}
						}
					}
			}
		}
		public ushort GetMapidFromMapShortName(string name)
		{
			MapInfo mi = null;
			if (MapsByName.TryGetValue(name, out mi))
			{
				return mi.ID;
			}
			return 0;
		}
        public Mount GetMountByHandleid(ushort id)
        {
            Mount pMount = null;
            if (this.MountyByHandleID.TryGetValue(id, out pMount))
            {
                return pMount;
            }
            new DataException("Mount By ItemID " + id + " not found");
            return null;
        }
        public Mount GetMountByItemID(ushort id)
        {
            Mount pMount = null;
            if(this.MountyByItemID.TryGetValue(id,out pMount))
            {
                return pMount;
            }
            new DataException("Mount By ItemID " + id + " not found");
            return null;
        }
		public string GetMapShortNameFromMapid(ushort id)
		{
			MapInfo mi = null;
			if (MapsByID.TryGetValue(id, out mi))
			{
				return mi.ShortName;
			}
			return "";
		}
		public static bool GetItemInfo(ushort itemID, out ItemInfo info)
		{
			return Instance.ItemsByID.TryGetValue(itemID, out info);
		}
		public static bool GetItemType(ushort itemID, out ItemSlot pType)
		{
			ItemInfo item;
			pType = ItemSlot.None;
			bool haveValue = GetItemInfo(itemID, out item);
			if (haveValue)
			{
				pType = item.Slot;
			}
			return haveValue;
		}
		public string GetMapFullNameFromMapid(ushort id)
		{
			MapInfo mi = null;
			if (MapsByID.TryGetValue(id, out mi))
			{
				return mi.FullName;
			}
			return "";
		}
		public void LoadMiniHouseInfo()
		{
			MiniHouses = new Dictionary<ushort, MiniHouseInfo>();
			DataTable houseData = null;
			using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
			{
				houseData = dbClient.ReadDataTable("SELECT *FROM minihouse");
			}
			if (houseData != null)
			{
				foreach (DataRow row in houseData.Rows)
				{
					MiniHouseInfo mhi = new MiniHouseInfo(row);
					MiniHouses.Add(mhi.ID, mhi);
				}
			}
			Log.WriteLine(LogLevel.Info, "Loaded {0} Mini Houses.", MiniHouses.Count);
		}
	}

}