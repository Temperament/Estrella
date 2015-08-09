using System;
using System.Collections.Generic;
using System.Linq;
using Zepheus.FiestaLib;
using Zepheus.Util;
using Zepheus.Database.Storage;
using System.Data;
using Zepheus.Database;
using Zepheus.World.Networking;
using Zepheus.FiestaLib.Networking;
using Zepheus.FiestaLib.Data;
using Zepheus.World.Data;
using Zepheus.World.Managers;
using Zepheus.World.Data.Guilds;
using Zepheus.World.Data.Guilds.Academy;
using Zepheus.InterLib.Networking;

namespace Zepheus.World.Data
{
	public class WorldCharacter
	{
		public Character Character { get; set;  }
		public WorldClient Client { get; set; }
	//	public Character Character { get { return _character ?? (_character = LazyLoadMe()); } set { _character = value; } }
		public int ID { get; private set; }
		public Dictionary<byte, ushort> Equips { get; private set; }
		public bool IsDeleted { get; private set; }
		public bool IsIngame { get;  set; }
		public bool IsPartyMaster { get; set;  }
		public Group Group { get; internal set; }
		public long GroupId {get; internal set;}
        public List<MasterMember> MasterList = new List<MasterMember>();
		public GroupMember GroupMember { get; internal set; }
		private List<Friend> friends;
		private List<Friend> friendsby;
        public long RecviveCoperMaster  { get; set;}


        public bool IsInGuildAcademy { get; set; }
        public bool IsInGuild { get; set; }
        public Guild Guild { get; set; }
        public GuildMember GuildMember { get; set; }
        public GuildAcademy GuildAcademy { get; set; }
        public GuildAcademyMember GuildAcademyMember { get; set; }
        public DateTime LastGuildListRefresh { get; set; }

        public List<string> BlocketUser = new List<string>();
		public Inventory Inventory = new Inventory();
        public event EventHandler GotIngame;

        public bool IsOnline
        {
            get
            {
                return ClientManager.Instance.IsOnline(this.Character.Name);
            }
        }

		public WorldCharacter(Character ch,WorldClient client)
		{
			Character = ch;
            this.Client = client;
			ID = Character.ID;
			Equips = new Dictionary<byte, ushort>();
			Inventory.LoadBasic(this);
			LoadEqupippet();
          ;
          
		}
		public List<Friend> Friends
		{
			get
			{
				if (this.friends == null)
				{
                    LoadFriends();
				}
				return this.friends;
			}
		}
		public void LoadFriends()
		{
		   
			this.friends = new List<Friend>();
			this.friendsby = new List<Friend>();
			DataTable frenddata = null;
			DataTable frenddataby = null;

			using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
			{
				frenddata = dbClient.ReadDataTable("SELECT * FROM friends WHERE CharID='" + this.Character.ID + "'");
				frenddataby = dbClient.ReadDataTable("SELECT * FROM friends WHERE FriendID='"+this.Character.ID+"'");
			}

			if (frenddata != null)
			{
				foreach (DataRow row in frenddata.Rows)
				{
					this.friends.Add(Friend.LoadFromDatabase(row));
				}
			}
			if (frenddataby != null)
			{
				foreach (DataRow row in frenddataby.Rows)
				{
					this.friendsby.Add(Friend.LoadFromDatabase(row));
				}
			}
			foreach (var friend in this.friends)
			{
				DataTable frendsdata = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					frendsdata = dbClient.ReadDataTable("SELECT * FROM Characters WHERE CharID='" + friend.ID + "'");
				}
				if (frenddata != null)
				{
					foreach (DataRow row in frendsdata.Rows)
					{
						friend.UpdateFromDatabase(row);
					}
				}
			}
			foreach (var friend in this.friendsby)
			{
				DataTable frendsdata = null;
				using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
				{
					frendsdata = dbClient.ReadDataTable("SELECT * FROM Characters WHERE CharID='" + friend.ID + "'");
				}
				if (frenddata != null)
				{
					foreach (DataRow row in frendsdata.Rows)
					{
						friend.UpdateFromDatabase(row);
					}
				}
			}
			UpdateFriendStates();
		}

        public void LoadMasterList()
        {
            DataTable Masterdata = null;
            using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
            {
                Masterdata = dbClient.ReadDataTable("SELECT * FROM Masters WHERE CharID='" + this.ID + "'");
            }
            if (Masterdata != null)
            {
                foreach (DataRow row in Masterdata.Rows)
                {
                    MasterMember DBMember = MasterMember.LoadFromDatabase(row);
                    this.MasterList.Add(DBMember);
                    if(DBMember.IsOnline)
                    {
                        DBMember.SetMemberStatus(true,this.Client.Character.Character.Name);
                    }
                }
            }
        }
        public void BroucastPacket(Packet pPacket)
        {
            InterServer.InterHandler.SendGetCharacterBroaucast(this, pPacket);
        }
		public void ChangeFrendMap(string mapname)
		{
      
			foreach (var friend in friends)
			{
				WorldClient client = ClientManager.Instance.GetClientByCharname(friend.Name);
				if (client == null) return;
				using (var packet = new Packet(SH21Type.FriendChangeMap))
				{
					packet.WriteString(this.Character.Name, 16);
					packet.WriteString(mapname, 12);
					client.SendPacket(packet);
				}
			}
        }
        public void ChangeMap(int oldmap)
        {
            InterServer.InterHandler.SendChangeMap(this, oldmap);
        }
        public void WriteBlockList()
        {
            if (this.BlocketUser.Count > 0)
            {
                using (var packet = new Packet(SH42Type.BlockList))
                {
                    
                    packet.WriteUShort((ushort)this.BlocketUser.Count);
                    foreach (string charname in this.BlocketUser)
                    {
                        packet.WriteString(charname, 16);
                    }
                    this.Client.SendPacket(packet);
                }
            }
        }
        public void LoadGroup()
		{
			this.Group = GroupManager.Instance.GetGroupById(this.Character.GroupId);
            if (this.Group != null)
            {
                this.GroupMember = this.Group[this.Character.Name];
                this.UpdateGroupStatus();
            }
		}

		public void LoadEqupippet()
		{
			foreach (var eqp in this.Inventory.EquippedItems.Where(eq => eq.Slot < 0))
			{
				byte realslot = (byte)(eqp.Slot * -1);
				if (Equips.ContainsKey(realslot))
				{
					Log.WriteLine(LogLevel.Warn, "{0} has duplicate equip in slot {1}", eqp.EquipID, realslot);
					Equips.Remove(realslot);
				}
				Equips.Add(realslot, (ushort)eqp.EquipID);
			}
		}
        public void UpdateMasterJoin()
        {
            this.Character.MasterJoin = DateTime.Now;
            Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE characters SET MasterJoin='" + DateTime.Now.ToString("yyyy-MM-dd hh:mm") + "' WHERE CharID='" + this.ID + "'");
        }
        public void SendPacketToAllOnlineMasters(Packet packet)
        {
            foreach (var pMember in this.MasterList)
            {
                pMember.pMember.SendPacket(packet);
            }
        }
		public Friend AddFriend(WorldCharacter pChar)
		{

			Friend pFrend = pChar.friends.Find(f => f.Name == pChar.Character.Name);
			Friend pFrendby = pChar.friendsby.Find(f => f.Name == pChar.Character.Name);
			Friend friend = Friend.Create(pChar);
			if (pFrend != null)
			{
				Program.DatabaseManager.GetClient().ExecuteQuery("INSERT INTO Friends (CharID,FriendID,Pending) VALUES ('" + pChar.Character.ID + "','" + this.Character.ID + "','1')");

			}
			if (pFrendby == null)
			{
				this.friendsby.Add(friend);
			}
			Program.DatabaseManager.GetClient().ExecuteQuery("INSERT INTO Friends (CharID,FriendID) VALUES ('" + this.Character.ID + "','" + pChar.Character.ID + "')");
			friends.Add(friend);

			return friend;
		}
		public bool DeleteFriend(string pName)
		{
			Friend friend = this.friends.Find(f => f.Name == pName);
			Friend friendby = this.friendsby.Find(f => f.Name == pName);
			if (friend != null)
			{
				bool result = this.friends.Remove(friend);
				if (result)
				{
					if (friendsby != null)
					{
						Program.DatabaseManager.GetClient().ExecuteQuery("DELETE FROM friends WHERE CharID=" + friend.ID + " AND FriendID=" + this.ID);
                        this.friendsby.Remove(friendby);
					}
					Program.DatabaseManager.GetClient().ExecuteQuery("DELETE FROM friends WHERE CharID=" + this.ID + " AND FriendID=" + friend.ID);
				}
				UpdateFriendStates();
				return result;
			}
			return false;
		}
       public void LevelUp(byte level)
        {
            CharacterManager.invokeLevelUp(this);
		}
        public void SendReciveMasterCoper()
        {
            if(this.Character.ReviveCoper > 0)

            using (var packet = new Packet(SH37Type.SendRecivveCopper))
            {
                packet.WriteLong(this.Character.ReviveCoper);
                this.Client.SendPacket(packet);
            }
        }
		public void UpdateFriendsStatus(bool state, WorldClient sender)
        {
            if (friendsby == null)
                return;

			foreach (Friend frend in friendsby)
			{
                WorldClient client = ClientManager.Instance.GetClientByCharID((int)frend.UniqueID);
				
				if (client != null)
				{
					if (state)
					{  
                            if(client != sender)
                             frend.IsOnline = true;
							frend.Online(client,sender);
 
					}
					else
					{
                        frend.IsOnline = false;
						frend.Offline(client, this.Character.Name);
					}
				}
			}
		}
        public void UpdateRecviveCoper()
        {
            MasterMember Master = this.MasterList.Find(m => m.IsMaster == true);
            if (Master != null)
            {
                Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE character SET ReviveCoper=" + RecviveCoperMaster + " WHERE CharID =" + Master.CharID + "");
            }

        }
		public void UpdateFriendStates()
		{
			List<Friend> unknowns = new List<Friend>();
			foreach (var friend in this.Friends)
			{
				if (friend.Name == null)
				{
					unknowns.Add(friend);
					continue;
				}
				WorldClient friendCharacter = ClientManager.Instance.GetClientByCharname(friend.Name);
				if (friendCharacter != null)
				{
					friend.Update(friendCharacter.Character);
				}
				else
				{
					friend.IsOnline = false;
				}
			}
			foreach (var friend in unknowns)
			{
				this.Friends.Remove(friend);
			}
			unknowns.Clear();
		}
		
		public void WriteFriendData(Packet pPacket)
		{
			foreach (var friend in this.Friends)
			{
				friend.WritePacket(pPacket);
			}
		}
        public void SetMasterOffline()
        {
            foreach (var Member in MasterList)
            {
                if (Member.pMember != null)
                {
                    Member.SetMemberStatus(false, this.Client.Character.Character.Name);
                }
            }
        }
      /*  public void SetGuildMemberStatusOffline()
        {
            try
            {
              
               if (this.Guild != null)
                {
                    GuildMember mMember = this.Guild.GuildMembers.Find(m => m.CharID == this.ID);
                    mMember.isOnline = false;
                    mMember.pClient = null;
                    foreach (var pMember in this.Guild.GuildMembers)
                    {
                        if (pMember.isOnline)
                        {
                            pMember.SendMemberStatus(false, this.Character.Name);
                        }
                    }
                }
                if(this.Academy != null)
                {
                    AcademyMember mMember = this.Academy.AcademyMembers.Find(m => m.CharID == this.ID);
                    mMember.isOnline = false;
                    mMember.pClient = null;
                    foreach (var pMember in this.Academy.AcademyMembers)
                    {
                        if (pMember.isOnline)
                        {
                            pMember.SendMemberStatus(false, this.Character.Name);
                        }
                    }
                    foreach (var pMember in  this.Academy.Guild.GuildMembers)
                    {
                        if (pMember.isOnline)
                        {
                            AcademyMember.SetOffline(this.Character.Name, pMember.pClient);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Error, "Failed Load Guild {0} {1}", this.ID, ex.Message.ToString());
            }
        }*/
		public void Loggeout(WorldClient pChar)
		{
            /*this.IsIngame = false;
            this.UpdateRecviveCoper();
			this.UpdateFriendsStatus(false,pChar);
			this.UpdateFriendStates();
            this.SetGuildMemberStatusOffline();*/
		}
		public void RemoveGroup()
		{
			this.Group = null;
			this.GroupMember = null;
			string query = string.Format(
                "UPDATE `characters` SET GroupID = 'NULL' WHERE CharID =  '{0}'", this.ID);
			Program.DatabaseManager.GetClient().ExecuteQuery(query);
		}

		public bool Delete()
		{
			if (IsDeleted) return false;
			try
			{
				Program.DatabaseManager.GetClient().ExecuteQuery("DELETE FROM characters WHERE CharID='" + this.Character.ID + "'");
			 
				IsDeleted = true;
				return true;
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Error deleting character: {0}", ex.ToString());
				return false;
			}
		}

		public WorldCharacter(Character ch, byte eqpslot, ushort eqpid)
		{
			Character = ch;
			ID = Character.ID;
			Equips = new Dictionary<byte, ushort>();
			Equips.Add(eqpslot, eqpid);
		}

		public ushort GetEquipBySlot(ItemSlot slot)
		{
			if (Equips.ContainsKey((byte)slot))
			{
				return Equips[(byte)slot];
			}
			else
			{
				return ushort.MaxValue;
			}
		}
		public static string ByteArrayToStringForBlobSave(byte[] ba)
		{
			string hex = BitConverter.ToString(ba);
			return hex.Replace("-", ",");
		}

		public void SetQuickBarData(byte[] pData)
		{
			Character.QuickBar = pData;
			string data = ByteArrayToStringForBlobSave(Character.QuickBar) ?? ByteArrayToStringForBlobSave(new byte[1024]);
			Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE Characters SET QuickBar='" +data+ "' WHERE CharID='" + Character.ID + "';");
		}
		public void SetQuickBarStateData(byte[] pData)
		{
			Character.QuickBarState = pData;
			string data = ByteArrayToStringForBlobSave(Character.QuickBarState) ?? ByteArrayToStringForBlobSave(new byte[24]);
			 Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE Characters SET QuickBarState='"+data+"' WHERE CharID='" + Character.ID + "'");
		}
		public void SetGameSettingsData(byte[] pData)
		{
			Character.GameSettings = pData;
			string data =  ByteArrayToStringForBlobSave(Character.GameSettings) ?? ByteArrayToStringForBlobSave(new byte[64]);
			Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE Characters SET GameSettings='" + data + "' WHERE CharID='" + Character.ID + "';");
		}
		public void SetClientSettingsData(byte[] pData)
		{
			Character.ClientSettings = pData;
			string data = ByteArrayToStringForBlobSave(Character.ClientSettings) ?? ByteArrayToStringForBlobSave(new byte[392]);
			 Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE Characters SET ClientSettings='"+data + "' WHERE CharID='" + Character.ID + "';");
		}
		public void SetShortcutsData(byte[] pData)
		{
			Character.Shortcuts = pData;
			string data = ByteArrayToStringForBlobSave(Character.Shortcuts) ?? ByteArrayToStringForBlobSave(new byte[308]);
			 Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE Characters SET Shortcuts='" + data+ "' WHERE CharID='" + Character.ID + "';");
		}
        internal void OnGotIngame()
        {
            LoadGroup();
    
            if (GotIngame != null)
                GotIngame(this, new EventArgs());
        }
       public void OneIngameLoginLoad()
        {
        
            this.UpdateFriendsStatus(true, this.Client);//Write Later As Event
            this.WriteBlockList();
            this.LoadMasterList();
            this.SendReciveMasterCoper();
            /* 
             LoadGuild();*/
             World.Handlers.Handler2.SendClientTime(this.Client, DateTime.Now);


        }
       public  void ChangeMoney(long NewMoney)
       {
           this.Character.Money = NewMoney;
           using (InterPacket packet = new InterPacket(InterHeader.UpdateMoneyFromWorld))
           {
               packet.WriteInt(this.Character.ID);
               packet.WriteLong(NewMoney);
           }
       }
		private void UpdateGroupStatus()
		{
			this.GroupMember.IsOnline = this.IsIngame;
			this.Group.AnnouncePartyList();
		}
	}
}
