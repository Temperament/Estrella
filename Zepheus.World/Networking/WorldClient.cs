using System;
using System.Data;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using Zepheus.Database;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.World.Data;
using Zepheus.World.Handlers;
using Zepheus.Database.DataStore;

namespace Zepheus.World.Networking
{
	public sealed class WorldClient : Client
	{
		#region Properties
		public bool Authenticated { get; set; }
		public string Username { get; set; }
		public int AccountID { get; set; }
		public byte Admin { get; set; }
		public ushort RandomID { get; set; } //this ID is used to authenticate later on.
		public Dictionary<byte, WorldCharacter> Characters { get; private set; }
		public WorldCharacter Character { get; set; }
		public DateTime lastPing { get; set; }
		public bool Pong { get; set; }
		#endregion
		#region .ctor
		public WorldClient(Socket socket)
			: base(socket)
		{
			base.OnPacket += new EventHandler<PacketReceivedEventArgs>(WorldClient_OnPacket);
			base.OnDisconnect += new EventHandler<SessionCloseEventArgs>(WorldClient_OnDisconnect);
		}
    

		#endregion
		#region Methods
		void WorldClient_OnDisconnect(object sender, SessionCloseEventArgs e)
		{
			Log.WriteLine(LogLevel.Debug, "{0} Disconnected.", this.Host);
			ClientManager.Instance.RemoveClient(this);
		}
		void WorldClient_OnPacket(object sender, PacketReceivedEventArgs e)
		{
			if (!Authenticated && !(e.Packet.Header == 3 && e.Packet.Type == 15)) return; //do not handle packets if not authenticated!
			MethodInfo method = HandlerStore.GetHandler(e.Packet.Header, e.Packet.Type);
			if (method != null)
			{
				Action action = HandlerStore.GetCallback(method, this, e.Packet);
				Worker.Instance.AddCallback(action);
			}
			else
			{
				Console.WriteLine(e.Packet.Header);
				Log.WriteLine(LogLevel.Debug, "Unhandled packet: {0}", e.Packet);
			}
		}
		public bool LoadCharacters()
		{
			if (!Authenticated) return false;
			Characters = new Dictionary<byte, WorldCharacter>();
            try
            {
                DataTable charData = null;
                using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
                {
                    charData = dbClient.ReadDataTable("SELECT * FROM Characters WHERE AccountID='" + this.AccountID + "'");
                }

                if (charData != null)
                {
                    foreach (DataRow row in charData.Rows)
                    {
      
                        Database.Storage.Character ch = new Database.Storage.Character();
                        ch.PositionInfo.ReadFromDatabase(row);
                        ch.LookInfo.ReadFromDatabase(row);
                        ch.CharacterStats.ReadFromDatabase(row);
                        ch.Slot = (byte)row["Slot"];
                        ch.CharLevel = (byte)row["Level"];
                        ch.AccountID = this.AccountID;
                        ch.Name = (string)row["Name"];
                        ch.ID = GetDataTypes.GetInt(row["CharID"]);
                        ch.Job = (byte)row["Job"];
                        ch.Money = GetDataTypes.GetLong(row["Money"].ToString());
                        ch.Exp = long.Parse(row["Exp"].ToString());
                        ch.HP = int.Parse(row["CurHP"].ToString());
                        ch.HPStones = 10;
                        ch.MasterJoin = DateTime.Parse(row["MasterJoin"].ToString());
                        ch.SP = int.Parse(row["CurSP"].ToString());
                        ch.SPStones = 10;
                        ch.StatPoints = (byte)row["StatPoints"];
                        ch.UsablePoints = (byte)row["UsablePoints"];
                        ch.Fame = 0;	// TODO
                        ch.GameSettings = Database.DataStore.ReadMethods.GetGameSettings(ch.ID, Program.DatabaseManager);
                        ch.ClientSettings = Database.DataStore.ReadMethods.GetClientSettings(ch.ID, Program.DatabaseManager);
                        ch.Shortcuts = Database.DataStore.ReadMethods.GetShortcuts(ch.ID, Program.DatabaseManager);
                        ch.QuickBar = Database.DataStore.ReadMethods.GetQuickBar(ch.ID, Program.DatabaseManager);
                        ch.QuickBarState = Database.DataStore.ReadMethods.GetQuickBarState(ch.ID, Program.DatabaseManager);
                        ch.ReviveCoper = GetDataTypes.GetLong(row["MasterReciveMoney"]);
                        if (row.IsNull("GroupID"))
                            ch.GroupId = -1;
                        else
                            ch.GroupId = long.Parse(row["GroupID"].ToString());

                        if (ch.GroupId == -1 || row.IsNull("IsGroupMaster"))
                            ch.IsGroupMaster = false;
                        else
                            ch.IsGroupMaster = ReadMethods.EnumToBool(row["IsGroupMaster"].ToString());

                        Characters.Add(ch.Slot, new WorldCharacter(ch, this));
                    }
                }
            }
            catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Error loading characters from {0}: {1}", Username, ex.InnerException.ToString());
				return false;
			}
			return true;

		}
		public ClientTransfer GenerateTransfer(byte slot)
		{
			if (!Characters.ContainsKey(slot))
			{
				Log.WriteLine(LogLevel.Warn, "Generating transfer for slot {0} which {1} doesn't own.", slot, Username);
				return null;
			}
			WorldCharacter character;
			if (Characters.TryGetValue(slot, out character))
			{
				return new ClientTransfer(AccountID, Username, character.Character.Name,character.Character.ID, RandomID, Admin, this.Host);
			}
			else return null;
		}
		public WorldCharacter CreateCharacter(string name, byte slot, byte hair, byte color, byte face, Job job, bool ismale)
		{
			if (Characters.ContainsKey(slot) || slot > 5)
				return null;
			//TODO: check if hair etc are actual beginner ones! (premium hack)
			//NOTE: Check the SHN's for this -> Moved to database
			BaseStatsEntry stats = DataProvider.Instance.JobBasestats[job];
			if (stats == null)
			{
                //NOTE be serious.. please
				// Log.WriteLine(LogLevel.Warn, "Houston, we have a problem! Jobstats not found for job {0}", job.ToString()); 
                Log.WriteLine(LogLevel.Error, "Jobstats not found for job {0}", job.ToString());
				return null;
			}
			Database.Storage.LookInfo newLook = new Database.Storage.LookInfo();
			Database.Storage.PositionInfo newPos = new Database.Storage.PositionInfo();
			Database.Storage.Character newchar = new Database.Storage.Character();
			newchar.AccountID = this.AccountID;
			newchar.CharLevel = 1;
			newchar.Name = name;
			newLook.Face = face;
			newLook.Hair = hair;
			newLook.HairColor = color;
			newchar.Job = (byte)job;
			newLook.Male = ismale;
			newchar.Slot = slot;
			newPos.XPos = 7636;
			newPos.YPos = 4610;
			newchar.HP = (short)stats.MaxHP;
			newchar.SP = (short)stats.MaxSP;
			newchar.HPStones = (short)stats.MaxHPStones;
			newchar.SPStones = (short)stats.MaxSPStones;
			newchar.LookInfo = newLook;
			newchar.PositionInfo = newPos;
			int charID = newchar.ID;
			 DatabaseClient client = Program.DatabaseManager.GetClient();

             string query =
                 "INSERT INTO `characters` " +
                 "(`AccountID`,`Name`,`MasterJoin`,`Slot`,`Job`,`Male`,`Hair`,`HairColor`,`Face`," +
                 " `QuickBar`, `QuickBarState`, `ShortCuts`, `GameSettings`, `ClientSettings`) " +
                 "VALUES " +
                     "('" + newchar.AccountID +
                     "', '" + newchar.Name +
                     "', '" + DateTime.Now.ToDBString() +
						"', " +		newchar.Slot +
						", " +		newchar.Job  +
						", " +		Convert.ToByte(newchar.LookInfo.Male) +
						", " +		newchar.LookInfo.Hair +
						", " +		newchar.LookInfo.HairColor +
						", " +		newchar.LookInfo.Face +
                        ", " +      "0" +
                        ", " +      "0" +
                        ", " +      "0" +
                        ", " +      "0" +
                        ", " +      "0" +
						")";
				client.ExecuteQuery(query);
			
			WorldCharacter tadaa = new WorldCharacter(newchar,this);
            ushort begineqp = GetBeginnerEquip(job);

            if (begineqp > 0)
            {
                sbyte eqp_slot = (sbyte)((job == Job.Archer) ? -10 : -12); //, (job == Job.Archer) ? (byte)12 : (byte)10, begineqp)
                Equip eqp = new Equip((uint)newchar.ID, begineqp, eqp_slot);
                tadaa.Inventory.AddToEquipped(eqp);
                client.ExecuteQuery("INSERT INTO equips (owner,slot,EquipID) VALUES ('"+tadaa.ID+"','"+eqp_slot+"','"+eqp.EquipID+"')");
            }
			Characters.Add(slot, tadaa);
			return tadaa;
		}
		//TODO: move to helper class?
		//NOTE: DO IT.
		private ushort GetBeginnerEquip(Job job)
		{
			ushort equipID;
			switch (job)
			{
				case Job.Archer:
					equipID = 1250;
					break;
				case Job.Fighter:
					equipID = 250;
					break;
				case Job.Cleric:
					equipID = 750;
					break;
				case Job.Mage:
					equipID = 1750;
					break;
				case Job.Trickster:
					equipID = 57363;
					break;
				default:
					Log.WriteLine(LogLevel.Exception, "{0} is creating a wrong job (somehow)", this.Username);
					return 0;
			}
			return equipID;
		}

        public override bool Equals(object obj)
		{
			if (!(obj is WorldClient))
				return false;
			WorldClient other = (WorldClient)obj;
            return other.AccountID == this.AccountID;
		}
        public override int GetHashCode()
        {
            return this.AccountID;
        }
		#endregion
	}
}
