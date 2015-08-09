using System;
using System.Collections.Generic;
using System.Linq;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.Util;
using Zepheus.Database.Storage;
using Zepheus.Database;
using Zepheus.World.Data;
using System.Data;
using System.Diagnostics;

namespace Zepheus.World.Data
{
	[ServerModule(Util.InitializationStage.DataStore)]
	public sealed class DataProvider
	{
		public static DataProvider Instance { get; private set; }
		public List<string> BadNames { get; private set; }
		public Dictionary<ushort, MapInfo> Maps { get; private set; }
		public Dictionary<Job, BaseStatsEntry> JobBasestats { get; private set; }
        public List<MasterRewardItem> MasterRewards { get; private set; }

		public DataProvider()
		{
		   
			LoadMaps();
			LoadBasestats();
			LoadBadNames();
            LoadMasterReward();

		}
        private void LoadMasterReward()
        {
            this.MasterRewards = new List<MasterRewardItem>();
            DataTable RewardData = null;
            using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
            {
                RewardData = dbClient.ReadDataTable(string.Format("USE `{0}`; SELECT  *FROM MasterRewards;  USE `{1}`", Settings.Instance.zoneMysqlDatabase, Settings.Instance.WorldMysqlDatabase));
            }
            if (RewardData != null)
            {
                foreach (DataRow row in RewardData.Rows)
                {
                    MasterRewardItem Reward = new MasterRewardItem(row);
                    this.MasterRewards.Add(Reward);
                }
            }
            Log.WriteLine(LogLevel.Info, "Load  {0} MasterRewards", this.MasterRewards.Count);
        }
		private void LoadMaps()
		{
			Maps = new Dictionary<ushort, MapInfo>();
			DataTable mapData = null;
			using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
			{
				mapData = dbClient.ReadDataTable(string.Format("USE `{0}`; SELECT * FROM `mapinfo`; USE `{1}`", Settings.Instance.zoneMysqlDatabase, Settings.Instance.WorldMysqlDatabase));
			}

			if (mapData != null)
			{
				foreach (DataRow row in mapData.Rows)
				{
					MapInfo info = MapInfo.Load(row);
					if (Maps.ContainsKey(info.ID))
					{
						Log.WriteLine(LogLevel.Debug, "Duplicate map ID {0} ({1})", info.ID, info.FullName);
						Maps.Remove(info.ID);
					}
					Maps.Add(info.ID, info);
					//  Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE data_mobcoordinate SET mapname=" + info.ID + " WHERE mapname='" + info.ShortName + "'");
				}
			}
			else
			{
				Log.WriteLine(LogLevel.Error, "DataProvder::LoadMaps() mapData == null");
			}
			Log.WriteLine(LogLevel.Info, "Loaded {0} maps!", Maps.Count);
		}
        public static string GetMapname(ushort mapid)
        {
            MapInfo mapinfo;
            if (DataProvider.Instance.Maps.TryGetValue(mapid, out mapinfo))
            {
                return mapinfo.ShortName;
            }
            return "";
        }
		
		public void LoadBasestats()
		{
			JobBasestats = new Dictionary<Job, BaseStatsEntry>();
			JobBasestats.Add(Job.Archer, new BaseStatsEntry
			{
				Level = 1,
				Str = 4,
				End = 4,
				Dex = 6,
				Int = 1,
				Spr = 3,
				MaxHPStones = 13,
				MaxSPStones = 11,
				MaxHP = 46,
				MaxSP = 24
			});
			JobBasestats.Add(Job.Cleric, new BaseStatsEntry
			{
				Level = 1,
				Str = 5,
				End = 4,
				Dex = 3,
				Int = 1,
				Spr = 4,
				MaxHPStones = 15,
				MaxSPStones = 11,
				MaxHP = 46,
				MaxSP = 32
			});
			JobBasestats.Add(Job.Fighter, new BaseStatsEntry
			{
				Level = 1,
				Str = 6,
				End = 5,
				Dex = 3,
				Int = 1,
				Spr = 1,
				MaxHPStones = 15,
				MaxSPStones = 7,
				MaxHP = 52,
				MaxSP = 10
			});
			JobBasestats.Add(Job.Mage, new BaseStatsEntry
			{
				Level = 1,
				Str = 1,
				End = 3,
				Dex = 3,
				Int = 10,
				Spr = 5,
				MaxHPStones = 12,
				MaxSPStones = 15,
				MaxHP = 42,
				MaxSP = 46
			});
			JobBasestats.Add(Job.Trickster, new BaseStatsEntry
			{
				Level = 1,
				Str = 5,
				End = 5,
				Dex = 4,
				Int = 1,
				Spr = 3,
				MaxHPStones = 15,
				MaxSPStones = 11,
				MaxHP = 48,
				MaxSP = 21
			});
		}
        public string GetMapShortNameFromMapid(ushort id)
        {
            MapInfo mi = null;
            if (this.Maps.TryGetValue(id, out mi))
            {
                return mi.ShortName;
            }
            return "";
        }
		public List<ushort> GetMapsForZone(int id)
		{
			int zonecount = Settings.Instance.ZoneCount;
			if (id == zonecount - 1) //Kindom provider
			{
				List<ushort> toret = new List<ushort>();
				foreach (var map in Maps.Values.Where(m => m.Kingdom > 0))
				{
					toret.Add(map.ID);
				}
				return toret;
			}
			else
			{
				List<ushort> normalmaps = new List<ushort>();
				foreach (MapInfo map in Maps.Values.Where(m => m.Kingdom == 0))
				{
					normalmaps.Add(map.ID);
				}
				int splitmaps = normalmaps.Count / (zonecount - 1); //normal map zones = total - 1
				List<ushort> toret = new List<ushort>();
				for (int i = id * splitmaps; i < (splitmaps * id) + splitmaps; i++)
				{
					toret.Add(normalmaps[i]);
				}
				return toret;
			}
		}
		private void LoadBadNames()
		{
			BadNames = new List<string>();
			DataTable badNameData = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					badNameData = dbClient.ReadDataTable("SELECT *FROM badnames");
				}
				if (badNameData != null)
				{
					foreach (DataRow row in badNameData.Rows)
					{
						string bad = (string)row["BadName"];
						// Columns: BadName Type
						BadNames.Add(bad);
					}
				}
			Log.WriteLine(LogLevel.Info, "Loaded {0} bad names.", BadNames.Count);
		}
		public bool IsBadName(string input)
		{
			input = input.ToLower();
			foreach (var badname in BadNames)
			{
				if (input.Contains(badname))
				{
					return true;
				}
			}
			return false;
		}

		[InitializerMethod]
		public static bool Load()
		{
			try
			{
				Instance = new DataProvider();
				Log.WriteLine(LogLevel.Info, "DataProvider initialized successfully!");
				return true;
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Error loading DataProvider: {0}", ex.ToString());
				return false;
			}
		}
	}
}
