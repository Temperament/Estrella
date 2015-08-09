using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.InterLib.Networking;
using Zepheus.Util;
using Zepheus.World.Data;
using Zepheus.World.Networking;

namespace Zepheus.World
{
    [ServerModule(InitializationStage.Clients)]
    public class GroupManager
    {
        #region .ctor

        public GroupManager()
        {
            groups = new List<Group>();
            requestsWithoutGroup = new List<GroupRequest>();
            groupsByMaster = new Dictionary<string, Group>();
            groupsById = new Dictionary<long, Group>();
            requestsByGroup = new Dictionary<Group, List<GroupRequest>>();
        }
        [InitializerMethod]
        public static bool Initialize()
        {
            Instance = new GroupManager();
            Instance.maxId = GetMaxGroupIdFromDatabase();
            return true;
        }

        #endregion .ctor
        #region Properties

        public static GroupManager Instance { get; private set; }

        private readonly List<Group> groups;
        private readonly Dictionary<string, Group> groupsByMaster;
        private readonly Dictionary<long, Group> groupsById;
        private readonly List<GroupRequest> requestsWithoutGroup;
        private readonly Dictionary<Group, List<GroupRequest>> requestsByGroup;

        private long maxId = 0;

        #endregion Properties
        #region Methods

        public long GetNextId()
        {
            long tmp = maxId;
            maxId++;
            return tmp;
        }
        public void Invite(WorldClient pClient, string pInvited)
        {
            Log.WriteLine(LogLevel.Debug, "{0} Invited {1}", pClient.Character.Character.Name, pInvited);
            if (!ClientManager.Instance.IsOnline(pInvited))
                return; // not online

            WorldClient invitedClient = ClientManager.Instance.GetClientByCharname(pInvited);

            //if(pClient.Character.Group == null)
            //    pClient.Character.Group = CreateNewGroup(pClient);

            GroupRequest request = new GroupRequest(pClient, pClient.Character.Group, pInvited);
            AddRequest(request);
            if (pClient.Character.Group != null)
                pClient.Character.Group.AddInvite(request);
            SendInvitedPacket(invitedClient, pClient);
        }
        public void DeclineInvite(WorldClient pClient, string pFrom)
        {
            if (!ClientManager.Instance.IsOnline(pFrom))
                return;			// Inviter / master not online!
            WorldClient from = ClientManager.Instance.GetClientByCharname(pFrom);
            if (!groupsByMaster.ContainsKey(pFrom))
                return;			// No such party
            Group grp = groupsByMaster[pFrom];
            GroupRequest request = requestsByGroup[grp].Find(r => r.InvitedClient == pClient);

            RemoveRequest(request);
            grp.RemoveInvite(request);
        }
        public void AcceptInvite(WorldClient pClient, string pFrom)
        {
            WorldClient from = ClientManager.Instance.GetClientByCharname(pFrom);
            if (from.Character.Group == null)
            {
                // New group
                Group g = CreateNewGroup(from, pClient);
                var req = requestsWithoutGroup.Find(r => r.InvitedClient == pClient);
                requestsWithoutGroup.Remove(req);
            }
            else
            {
                Group g = groupsByMaster[pFrom];
                var req = requestsByGroup[g].Find(r => r.InvitedClient == pClient);
                RemoveRequest(req);
                g.RemoveInvite(req);
                g.MemberJoin(pClient.Character.Character.Name);
            }
        }
        public void LeaveParty(WorldClient pClient)
        {
            Group g = pClient.Character.Group;
            g.MemberLeaves(pClient);
            if (g.Members.Count < 2) // Not enough members for party to stay
            {
                g.BreakUp();
            }
        }
        public void KickMember(WorldClient pClient, string pKicked)
        {
            if (pClient.Character.GroupMember.Role != GroupRole.Master)
                return; // Only master may kick ppl

            if (pClient.Character.Group.NormalMembers.Count() <= 1)
            {
                pClient.Character.Group.BreakUp();
            }
            else
            {
                pClient.Character.Group.KickMember(pKicked);
            }
        }
        public void ChangeMaster(WorldClient pClient, string pMastername)
        {
            if (pClient.Character.GroupMember.Role != GroupRole.Master)
                return;
            pClient.Character.Group.ChangeMaster(pClient.Character.Group.NormalMembers.Single(m => m.Name == pMastername));
        }
        public void LoadGroupById(long pId)
        {
            Group grp = Group.ReadFromDatabase(pId);
            AddGroup(grp);
        }
        public Group CreateNewGroup(WorldClient pMaster, WorldClient pMember)
        {
            var grp = CreateNewGroup(pMaster);
            SendNewGroupMasterPacket(pMaster, pMember.Character.Character.Name);
            grp.MemberJoin(pMember.Character.Character.Name);
            return grp;
        }
        public Group GetGroupById(long pId)
        {
            if (this.groupsById.ContainsKey(pId))
                return this.groupsById[pId];
            else
                return null;
        }
        public Group CreateNewGroup(WorldClient pMaster)
        {
            Group grp = new Group(GetNextId());
            GroupMember mstr = new GroupMember(pMaster, GroupRole.Master);
            pMaster.Character.GroupMember = mstr;
            pMaster.Character.Group = grp;
            grp.AddMember(mstr);

            AddGroup(grp);
            grp.CreateInDatabase();

			SendNewPartyInterPacket(grp.Id);
            return grp;
        }

        internal void OnGroupBrokeUp(object sender, EventArgs e)
        {
            Group grp = sender as Group;
            if (grp == null)
                return;

            groups.Remove(grp);
            var byMasterEntry = groupsByMaster.Single(pair => pair.Value.Id == grp.Id);
            groupsByMaster.Remove(byMasterEntry.Key);
            groupsById.Remove(grp.Id);
            requestsByGroup.Remove(grp);
            SendGroupBrokeUpInterPacket(grp.Id);
        }

        private void SendGroupBrokeUpInterPacket(long pId)
        {
            using (var packet = new InterPacket(InterHeader.PartyBrokeUp))
            {
                packet.WriteLong(pId);

                foreach (var zone in Program.Zones.Select(m => m.Value))
                {
                    zone.SendPacket(packet);
                }
            }
        }
        internal void OnGroupChangedMaster(object sender, ChangedMasterEventArgs e)
        {
            Group group = sender as Group;
            if(group == null)
                return;
            groupsByMaster.Remove(e.OldMaster.Name);
            groupsByMaster.Add(e.NewMaster.Name, group);
        }

        private void AddRequest(GroupRequest pRequest)
        {
            if (pRequest.Group == null)
            {
                requestsWithoutGroup.Add(pRequest);
            }
            else
                if (!this.requestsByGroup.ContainsKey(pRequest.Group))
                {
                    this.requestsByGroup.Add(pRequest.Group, new List<GroupRequest>());

                    this.requestsByGroup[pRequest.Group].Add(pRequest);
                }
        }
        private void RemoveRequest(GroupRequest pRequest)
        {
            this.requestsByGroup[pRequest.Group].Remove(pRequest);
        }
        private void SendInvitedPacket(WorldClient pInvited, WorldClient pFrom)
        {
            using (var ppacket = new Packet(SH14Type.PartyInvite))
            {
                ppacket.WriteString(pFrom.Character.Character.Name, 0x10);
                pInvited.SendPacket(ppacket);
            }
        }
        private void SendInviteDeclinedPacket(WorldClient pInviter, WorldClient pInvited)
        {
            using (var packet = new Packet(SH14Type.InviteDeclined))
            {
                packet.WriteString(pInvited.Character.Character.Name, 16);
                packet.WriteUShort(1217);   // UNKNOWN
                pInviter.SendPacket(packet);
            }

            throw new NotImplementedException();
        }
        private void SendNewGroupMasterPacket(WorldClient pMaster, string pMemberName)
        {
            using (var packet = new Packet(SH14Type.InviteDeclined))
            {
                packet.WriteString(pMemberName, 16);
                packet.WriteHexAsBytes("C1 04");
                pMaster.SendPacket(packet);
            }
        }
		private void SendNewPartyInterPacket(long pId)
		{
			using (var packet = new InterPacket(InterHeader.NewPartyCreated))
			{
				packet.WriteLong(pId);

				foreach (var connection in Program.Zones.Select(pair => pair.Value))
					connection.SendPacket(packet);
			}
		}
        private void AddGroup(Group pGroup)
        {
            this.groups.Add(pGroup);
            this.groupsByMaster.Add(pGroup.Master.Name, pGroup);
            this.groupsById.Add(pGroup.Id, pGroup);
            pGroup.BrokeUp += this.OnGroupBrokeUp;
            pGroup.ChangedMaster += OnGroupChangedMaster;
        }
        private static long GetMaxGroupIdFromDatabase()
        {
            //--------------------------------------------------
            // Queries used in function
            //--------------------------------------------------
            const string get_max_group_id_query =
                            "SELECT MAX(`Id`) AS `MAX` FROM `groups`";

            //--------------------------------------------------
            // get max id from database
            //--------------------------------------------------

            long max = 0;
            using (var client = Program.DatabaseManager.GetClient())
            using (var cmd = new MySqlCommand(get_max_group_id_query, client.GetConnection()))
            using (var rdr = cmd.ExecuteReader())
                while (rdr.Read())
                {
                    if (!rdr.IsDBNull(0))
                        max = rdr.GetInt64(0) + 1;
                }

            return max;
        }

        #endregion Methods
    }
}