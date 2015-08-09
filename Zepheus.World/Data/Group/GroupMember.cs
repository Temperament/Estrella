using MySql.Data.MySqlClient;
using Zepheus.World.Networking;

namespace Zepheus.World.Data
{
	public class GroupMember
	{
		#region .ctor

		private GroupMember()
		{

		}
		public GroupMember(WorldClient client, GroupRole role)
		{
			this.Client = client;
			this.Character = client.Character;
			this.CharId = client.Character.ID;
			this.Role = role;
			this.Name = client.Character.Character.Name;
			this.IsOnline = true;
		}
		#endregion
		#region Properties
		public WorldCharacter Character { get; private set; }
		public string Name { get;  set; }
		public Group Group { get; internal set; }
		public GroupRole Role { get; internal set; }
		public WorldClient Client { get; private set; }
		public int CharId { get; private set; }
		public bool IsOnline { get; set; }
		#endregion
		#region Methods

        public override int GetHashCode()
        {
            return this.CharId;
        }
		public override bool Equals(object obj)
		{
			if(!(obj is GroupMember))
				return false;
			return ((GroupMember) obj).Name == this.Name;
		}

		public static GroupMember LoadFromDatabase(ushort pCharId)
		{
			const string query = "SELECT * FROM `characters` WHERE CharId = @cid";
			GroupMember member = new GroupMember();

			using (var con = Program.DatabaseManager.GetClient())
			using (var cmd = new MySqlCommand(query, con.GetConnection()))
			{
				cmd.Parameters.AddWithValue("@cid", pCharId);
				using (var rdr = cmd.ExecuteReader())
				{
					while (rdr.Read())
					{
						member.Name = rdr.GetString("Name");
						member.IsOnline = ClientManager.Instance.IsOnline(member.Name);
						member.Role = rdr.GetBoolean("IsGroupMaster") 
											? GroupRole.Master 
											: GroupRole.Member;
						if (member.IsOnline)
						{
							member.Client = ClientManager.Instance.GetClientByCharname(member.Name);
							member.Character = member.Client.Character;
						}

						return member;
					}
				}
			}
			return member;
		}
		#endregion
	}
}
