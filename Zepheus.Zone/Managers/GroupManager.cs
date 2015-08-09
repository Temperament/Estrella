using System.Collections.Generic;
using Zepheus.Zone.Game;
using System.Linq;
using System;
using MySql.Data.MySqlClient;
using Zepheus.Util;
using System.Data;

namespace Zepheus.Zone
{
	[ServerModule(InitializationStage.Clients)]
	public class GroupManager
	{
		#region .ctor
		[Util.InitializerMethod]
		public static bool Initialize()
		{
			Instance = new GroupManager();
			Log.WriteLine(LogLevel.Debug, "GroupManager initialized");
			return true;
		}
		private GroupManager()
		{
			this.groups = new List<Group>();
			this.groupsById = new Dictionary<long, Group>();
			this.groupsByMaster = new Dictionary<string, Group>();
			this.updateQueue = new Queue<Group>();
		}
		#endregion
		#region Properties
		public static readonly TimeSpan GroupUpdateInterval = TimeSpan.FromSeconds(3); 
		public static GroupManager Instance { get; private set; }

		private readonly List<Group> groups;
		private readonly Dictionary<string, Group> groupsByMaster;
		private readonly Dictionary<long, Group> groupsById;
		private readonly Queue<Group> updateQueue;
		#endregion
		#region Methods
		public void NewGroupCreated(long pGroupId)
		{
			LoadGroupFromDatabase(pGroupId);
			if(!groupsById.ContainsKey(pGroupId))
				return;
			Group group = groupsById[pGroupId];
			foreach (var member in group.Members)
			{
				if(ClientManager.Instance.HasClient(member.Name))
				{
					var client = ClientManager.Instance.GetClientByCharName(member.Name);
					var chara = client.Character;
                    
					member.Character = chara;
					chara.Group = group;
					chara.GroupMember = member;
                    chara.GroupMember.IsOnline = true;
				}
			}
		}
		public void AddGroup(Group grp)
		{
			groups.Add(grp);
			groupsByMaster.Add(grp.Master.Name, grp);
			groupsById.Add(grp.Id, grp);
			updateQueue.Enqueue(grp);
		}
		public void LoadGroupFromDatabase(long pId)
		{
			if(groups.Any(g => g.Id == pId))
				return;
			if(pId == -1) // means null-group
				return;
			Group group = Group.LoadGroupFromDatabaseById(pId);
			this.AddGroup(group);
		}
		public void Update()
		{
            if(updateQueue.Count <= 0)
                return;
			// while the front group is has to be updated
			while (updateQueue.Peek().LastUpdate + GroupUpdateInterval >= DateTime.Now)
			{
				Group grp = updateQueue.Dequeue();
				UpdateGroup(grp);
			}

			// this will make it into a loop w/ the worker
			Worker.Instance.AddCallback(Update);
		}
		public void AddMemberToGroup(long pGroupId, string pCharName)
		{
			if(!this.groupsById.ContainsKey(pGroupId))
				LoadGroupFromDatabase(pGroupId);
			if(!this.groupsById.ContainsKey(pGroupId))
				return;

			Group group = this.groupsById[pGroupId];
			group.AddMember(pCharName, false);
		}
		public Group GetGroupForCharacter(long pCharId)
		{
			long groupId = GetGroupIdForCharacter(pCharId);
			if(!groupsById.ContainsKey(groupId))
				LoadGroupFromDatabase(groupId);
			return groupsById.ContainsKey(groupId) ? groupsById[groupId] : null;
		}

		internal bool CheckCharacterHasGroup(long pCharId)
		{
			//--------------------------------------------------
			// Queries used 
			//--------------------------------------------------
			const string get_group_id_query = 
				"SELECT `GroupId` " +
				"FROM `characters` " +
				"WHERE `CharId` = {0}";
			//--------------------------------------------------
			// Get group id and check if char haz group
			//--------------------------------------------------
			string query = string.Format(get_group_id_query, pCharId);
			using(var client = Program.DatabaseManager.GetClient())
			using(var cmd = new MySqlCommand(query, client.GetConnection()))
			using(var reader = cmd.ExecuteReader())
			{
				long? id = null;
				while(reader.Read())
					id = reader.GetInt64(0);

				if(id == -1 || id == null)
					return false;
			}
			return true;
		}
		internal Group GetGroupForCharacter(ZoneCharacter pCharacter)
		{
			//--------------------------------------------------
			// Quries used in function
			//--------------------------------------------------
			const string get_group_id_query = 
				"SELECT `GroupId` FROM `characters` " + 
				"WHERE `CharId` = {0} ";
			
			//--------------------------------------------------
			// get group id
			//--------------------------------------------------
			string query = string.Format(get_group_id_query, pCharacter.ID);
			long groupId = -1;
			using(var client = Program.DatabaseManager.GetClient())
			using(var cmd = new MySqlCommand(query, client.GetConnection()))
			using(var reader = cmd.ExecuteReader())
			{
				while(reader.Read())
					groupId = reader.GetInt64(0);
			}

			LoadGroupFromDatabase(groupId);
			return groupsById[groupId];
		}
		internal void OnCharacterRemove(ZoneCharacter pCharacter)
		{
			if(pCharacter.Group == null)
				return;

			pCharacter.GroupMember.IsOnline = false;
			pCharacter.GroupMember.Character = null;
			pCharacter.GroupMember = null;
			if(pCharacter.Group.Members.Where(m => m.Name != pCharacter.Name && m.IsOnline).Count() > 0)
				return;
			RemoveGroup(pCharacter.Group);			
		}
        internal void GroupBrokeUp(long groupId)
        {
            if(!this.groupsById.ContainsKey(groupId))
                return;
            Group grp = this.groupsById[groupId];
            foreach (var member in grp.Members.Where(m => m.Character != null).Select(m => m.Character))
            {
                member.Group = null;
                member.GroupMember = null;
            }

            RemoveGroup(grp);
        }

		private void UpdateGroup(Group grp)
		{
			if(!groups.Contains(grp))
				return;
			grp.Update();
			updateQueue.Enqueue(grp);
		}
		private void RemoveGroup(Group grp)
		{
			this.groups.Remove(grp);
			this.groupsByMaster.Remove(grp.Master.Name);
			this.groupsById.Remove(grp.Id);
		}
		private long GetGroupIdForCharacter(long pCharacterId)
		{
			//--------------------------------------------------
			// Queries used in this function
			//--------------------------------------------------
			const string get_group_id_query = 
				"USE `fiesta_world`; " +
				"SELECT `GroupId` FROM `characters` " +
				"WHERE `CharId` =  '{0}'";

			//--------------------------------------------------
			// get groupId
			//--------------------------------------------------
			using (var client = Program.DatabaseManager.GetClient())
			using (var cmd = new MySqlCommand(string.Format(get_group_id_query, pCharacterId), client.GetConnection()))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read()){
					if(reader.IsDBNull(0))
						return -1;
					return reader.GetInt64("GroupId");
				}
			
			return -1;
		}
		#endregion
    }
}
