using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Zepheus.Database;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.InterLib.Networking;
using Zepheus.World.InterServer;
using Zepheus.World.Networking;

namespace Zepheus.World.Data
{
    public class Group
    {
        #region .ctor

        public Group(long id)
        {
            this.Members = new List<GroupMember>();
            this.openRequests = new List<GroupRequest>();
            this.Id = id;
            this.Members = new List<GroupMember>();
            this.DropState = DropState.FreeForAll;
            this.gotLastDrop = 0;
        }

        #endregion
        #region Properties

        public const int MaxMembers = 5;
        public readonly List<GroupMember> Members;
        private readonly List<GroupRequest> openRequests;
        public GroupMember this[string pName]
        {
            get
            {
                return this.Members.Single(m => m.Name == pName);
            }
        }
        public GroupMember Master { get { return Members.Single(m => m.Role == GroupRole.Master); } }
        public IEnumerable<GroupMember> NormalMembers { get { return from m in Members where m.Role != GroupRole.Master select m; } }
        public DropState DropState { get; private set; }
        public long Id { get; private set; }
        public bool Exists { get; private set; }

        private int gotLastDrop;
        #endregion
        #region Methods

        #region Public
        public bool HasMember(string pName)
        {
            return this.Members.Any(m => m.Name == pName);
        }
        public bool IsFull()
        {
            return Members.Count() >= MaxMembers;
        }
        public void InviteNewMember(WorldCharacter pSender, string pTarget)
        {
            if (!ClientManager.Instance.IsOnline(pTarget))
                return;
            if (Master.Name != pSender.Character.Name)
                return;		// only the master may invite new Members

            GroupManager.Instance.Invite(pSender.Client, pTarget); // trololol
        }
        public void ChangeDropType(WorldCharacter pBy, byte pDropState)
        {
            if (pBy.Character.Name != Master.Name)
                return;		// only the master may change drop state!
            this.DropState = (DropState)pDropState;

            UpdateDropStateToMembers();
        }
        public void BreakUp()
        {
            this.Exists = false;
            BreakUpInDatabase();
            using (Packet p = new Packet(SH14Type.BreakUp))
            {
                AnnouncePacket(p);
            }
            OnBrokeUp();
        }
        public void ChangeMaster(GroupMember pNewMaster)
        {
            ChangeMaster(this.Master, pNewMaster);
            AnnounceChangeMaster();
        }
        public bool HasOpenRequestFor(string pName)
        {
            return openRequests.Any(r => r.InvitedClient.Character.Character.Name == pName);
        }
        public void MemberLeaves(WorldClient pClient)
        {

            if (pClient.Character.GroupMember.Role == GroupRole.Master)
                ChangeMaster(NormalMembers.First().Character.GroupMember);
            SendMemberLeavesPacket(pClient.Character.Character.Name, Members.Where(m => m.IsOnline).Select(m => m.Client));

            Members.Remove(pClient.Character.GroupMember);
            pClient.Character.Group = null;
            pClient.Character.GroupMember = null;
            RemoveGroupDataInDatabase(pClient.Character.ID);

            UpdateInDatabase();
        }
        public void KickMember(string pMember)
        {
            SendMemberLeavesPacket(pMember, Members.Where(m => m.IsOnline).Select(m => m.Client));
            Members.Remove(Members.Single(m => m.Name == pMember));
            UpdateInDatabase();
        }
        public void MemberJoin(string pMember)
        {
            WorldClient client = ClientManager.Instance.GetClientByCharname(pMember);
            GroupMember gMember = new GroupMember(client, GroupRole.Member);
            client.Character.GroupMember = gMember;
            client.Character.Group = this;
            AddMember(gMember);

            AnnouncePartyList();
            UpdateInDatabase();
        }
        public void AnnouncePartyList()
        {
            using (var packet = new Packet(SH14Type.PartyList))
            {
                packet.WriteByte((byte)Members.Count);
                foreach (var groupMember in Members)
                {
                    packet.WriteString(groupMember.Name, 16);
                    packet.WriteBool(groupMember.IsOnline);
                }

                AnnouncePacket(packet);
            }
        }
        public void Chat(WorldClient pFrom, string pMessage)
        {
            using (var packet = new Packet(SH8Type.PartyChat))
            {
                packet.WriteString(pFrom.Character.Character.Name, 16);
                packet.WriteByte((byte)pMessage.Length);
                packet.WriteString(pMessage);

                AnnouncePacket(packet);
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Group))
                return false;
            var grp = (Group)obj;
            return grp.Id == this.Id;
        }
        public bool Equals(Group other)
        {
            return other.Id.Equals(this.Id);
        }
        public override int GetHashCode()
        {
            return (int)this.Id;
        }

        internal void AddMember(GroupMember pMember)
        {
            this.Members.Add(pMember);
            pMember.Group = this;
            UpdateInDatabase();
            SendAddMemberInterPacket(pMember);
        }
        internal void AddInvite(GroupRequest pRequest)
        {
            this.openRequests.Add(pRequest);
        }
        internal void RemoveMember(GroupMember pMember)
        {
            this.Members.Remove(pMember);
            pMember.Character.Group = null;
            pMember.Character.GroupMember = null;
            RemoveGroupDataInDatabase(pMember.CharId);
            // TEMP
            KickMember(pMember.Name);
            // NOTE: Send packet to other Members to update GroupList!
            AnnouncePartyList();
        }
        internal void RemoveInvite(GroupRequest pRequest)
        {
            this.openRequests.Remove(pRequest);
        }
        internal void UpdateInDatabase()
        {
            UpdateGroupTableInDatabase();
            UpdateMembersInDatabase();
        }
        internal void UpdateGroupTableInDatabase()
        {
            //--------------------------------------------------
            // Queries used in this function
            //--------------------------------------------------

            const string updateGroupTableQuery =
                "UPDATE `groups` " +
                "SET " +
                    "`Member1` = {1} ," +
                    "`Member2` = {2} ," +
                    "`Member3` = {3} ," +
                    "`Member4` = {4} ," +
                    "`Member5` = {5} " +
                "WHERE `Id` = {0}";

            //--------------------------------------------------
            // Update table
            //--------------------------------------------------

            using (var client = Program.DatabaseManager.GetClient())
            {
                string query = string.Format(updateGroupTableQuery,
                                this.Id,
                                this.Members[0].CharId,
                                (this.Members.Count >= 2 ? this.Members[1].CharId.ToString() : "NULL"),
                                (this.Members.Count >= 3 ? this.Members[2].CharId.ToString() : "NULL"),
                                (this.Members.Count >= 4 ? this.Members[3].CharId.ToString() : "NULL"),
                                (this.Members.Count >= 5 ? this.Members[4].CharId.ToString() : "NULL"));
                client.ExecuteQuery(query);
            }
        }
        internal void UpdateMembersInDatabase()
        {
            //--------------------------------------------------
            // Queries used in this function
            //--------------------------------------------------
            const string update_character_table_query =
                "UPDATE `characters` " +
                "SET " +
                    "`GroupID` = {1} ," +
                    "`IsGroupMaster` = {2} " +
                "WHERE `CharID` = {0}";

            //--------------------------------------------------
            // Update table
            //--------------------------------------------------
            using (var client = Program.DatabaseManager.GetClient())
            {
                foreach (var member in this.Members)
                {
                    string query = string.Format(update_character_table_query,
                                member.Character.ID,
                                this.Id,
                                member.Character.GroupMember.Role == GroupRole.Master);
                    client.ExecuteQuery(query);
                }
            }
        }
        internal void CreateInDatabase()
        {
            //--------------------------------------------------
            // Queries used in this function
            //--------------------------------------------------

            const string create_group_query =
                "INSERT INTO `groups` " +
                    "(`Id`, `Member1`, `Member2`, `Member3`, `Member4`, `Member5`) " +
                "VALUES " +
                    "({0}, {1}, {2}, {3}, {4}, {5})";
            //--------------------------------------------------
            // create entry in table
            //--------------------------------------------------
            using (var client = Program.DatabaseManager.GetClient())
            {
                string query = string.Format(create_group_query,
                                this.Id,
                                this.Members.Count > 0 ? this.Members[0].CharId.ToString() : "NULL",
                                this.Members.Count > 1 ? this.Members[1].CharId.ToString() : "NULL",
                                this.Members.Count > 2 ? this.Members[2].CharId.ToString() : "NULL",
                                this.Members.Count > 3 ? this.Members[3].CharId.ToString() : "NULL",
                                this.Members.Count > 4 ? this.Members[4].CharId.ToString() : "NULL");
                using (var cmd = new MySqlCommand(query, client.GetConnection()))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            // keep character also up to date
            UpdateMembersInDatabase();
        }
        internal static Group ReadFromDatabase(long pId)
        {
            // Note - put datatables int using statements!
            Group g = new Group(pId);
            DataTable gdata = null;
            using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
            {
                gdata = dbClient.ReadDataTable("SELECT * FROM `groups` WHERE Id = " + pId + "");
            }

            if (gdata != null)
                foreach (DataRow row in gdata.Rows)
                    for (int i = 1; i < 4; i++)
                    {
                        string memColName = string.Format("Member{0}", i);
                        if (row.IsNull(memColName))
                            continue;
                        UInt16 mem = (ushort)row[memColName];
                        g.Members.Add(GroupMember.LoadFromDatabase(mem));
                    }

            return g;
        }
        #endregion
        #region Private
        private void UpdateDropStateToMembers()
        {
            using (var packet = new Packet(SH14Type.PartyDropState))
            {
                packet.WriteByte((byte)DropState);

                foreach (var m in Members)
                {
                    m.Client.SendPacket(packet);
                }
            }
        }
        private void SendMemberLeavesPacket(string pLeaver, IEnumerable<WorldClient> pMembers)
        {
            using (var packet = new Packet(SH14Type.PartyLeave))
            {
                packet.WriteString(pLeaver, 16);
                packet.WriteUShort(1281);		// UNK

                foreach (var member in pMembers)
                {
                    member.SendPacket(packet);
                }
            }
        }
        private void DeleteGroupByNameInDatabase(string pName)
        {
            DatabaseHelper.RemoveCharacterGroup(pName);
        }
        private void AnnounceChangeMaster()
        {
            using (var packet = new Packet(SH14Type.ChangePartyMaster))
            {
                packet.WriteString(Master.Name, 16);
                packet.WriteUShort(1352);

                // Send to all online Members
                Members.ForEach(m => { if (m.IsOnline) m.Client.SendPacket(packet); });
            }
        }
        private void SendAddMemberInterPacket(GroupMember pMember)
        {
            ZoneConnection con = Program.GetZoneByMap(pMember.Character.Character.PositionInfo.Map);
            using (var pack = new InterPacket(InterHeader.AddPartyMember))
            {
                pack.WriteLong(this.Id);
                pack.WriteString(pMember.Name, 16);
                con.SendPacket(pack);
            }
        }
        private void AnnouncePacket(Packet pPacket)
        {
            foreach (var grpMem in Members.Where(m => m.IsOnline))
            {
                grpMem.Client.SendPacket(pPacket);
            }
        }
        private void BreakUpInDatabase()
        {
            //--------------------------------------------------
            // Queries used in function
            //--------------------------------------------------

            const string break_group_query =
                "UPDATE `groups` " +
                "SET `Exists` = 0 " +
                "WHERE `Id` = '{0}'";

            const string reset_char_group_query =
                "UPDATE `characters` " +
                "SET `GroupID` = NULL, " +
                    "`IsGroupMaster` = NULL " +
                "WHERE `GroupId` = '{0}'";

            //--------------------------------------------------
            // Execute queries
            //--------------------------------------------------

            using (var client = Program.DatabaseManager.GetClient())
            {
                string query = string.Format(break_group_query, this.Id);
                client.ExecuteQuery(query);

                query = string.Format(reset_char_group_query, this.Id);
                client.ExecuteQuery(query);
            }

        }
        private void RemoveGroupDataInDatabase(int pCharId)
        {
            //--------------------------------------------------
            // queries
            //--------------------------------------------------
            const string remove_group_data_query =
                "UPDATE `characters` " +
                "SET " +
                    "GroupID = NULL, " +
                    "IsGroupMaster = NULL " +
                "WHERE " +
                "CharId = {0}";
            //--------------------------------------------------
            // removing the data.
            //--------------------------------------------------
            string query = string.Format(remove_group_data_query, pCharId);
            using(var client = Program.DatabaseManager.GetClient())
            {
                client.ExecuteQuery(query);
            }
        }
        private void ChangeMaster(GroupMember pFrom, GroupMember pTo)
        {
            if(pFrom.Role != GroupRole.Master)
                return;
            pFrom.Role = GroupRole.Member;
            pTo.Role = GroupRole.Master;
            OnChangedMaster(pFrom, pTo);
        }
        #endregion
        #region EventExecuter
        protected virtual void OnBrokeUp()
        {
            if (BrokeUp != null)
                BrokeUp(this, new EventArgs());
            foreach (var mem in Members)
            {
                RemoveGroupDataInDatabase(mem.CharId);
                mem.Group = null;
                mem.Client.Character.Group = null;
                mem.Client.Character.GroupId = -1;
                mem.Client.Character.GroupMember = null;
            }
        }
        protected virtual void OnChangedMaster(GroupMember pOld, GroupMember pNew)
        {
            if(ChangedMaster != null)
                ChangedMaster(this, new ChangedMasterEventArgs(pOld, pNew));
        }
        #endregion
        #region EventHandler

        #endregion

        #endregion
        #region Events

        public event EventHandler BrokeUp;
        public event EventHandler<ChangedMasterEventArgs> ChangedMaster;

        #endregion
    }

    public class ChangedMasterEventArgs : EventArgs
    {
        public GroupMember OldMaster { get; set; }
        public GroupMember NewMaster { get; private set; }

        public ChangedMasterEventArgs(GroupMember pOld, GroupMember pNew)
        {
            this.OldMaster = pOld;
            this.NewMaster = pNew;
        }
    }
}
