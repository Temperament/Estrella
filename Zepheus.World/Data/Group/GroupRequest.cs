using System;
using Zepheus.World.Networking;

namespace Zepheus.World.Data
{
	public class GroupRequest
	{
		#region .ctor
		public GroupRequest(WorldClient pFrom, Group pGroup, string pInvited)
		{

			this.CrationTimeStamp = DateTime.Now;
			this.InvitedClient = ClientManager.Instance.GetClientByCharname(pInvited);
			this.InviterClient = pFrom;
			this.Group = pGroup;
		}
		#endregion

		#region Properties
		public DateTime CrationTimeStamp { get; private set; }
		public Group Group { get; internal set; }
		public WorldClient InvitedClient { get; private set; }
		public WorldClient InviterClient { get; private set; }
		#endregion

		#region Methods
		#endregion
	}
}
