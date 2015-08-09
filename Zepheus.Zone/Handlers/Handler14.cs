using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.Zone.Networking;
using System;

namespace Zepheus.Zone.Handlers
{
	public class Handler14
	{
		[PacketHandler(CH14Type.PartyInviteGame)]
		public static void GetPartyListFromCharserer(ZoneClient client, Packet packet)
		{
            // NOTE: seems as here just happens nothing I guess?
            // TODO: Implement if nesseccery
            // NOTE: This starts the update loop?
			if (client.Character.GroupMember == null)
			{
				// TODO: ask world for group
				return;
			}
			else
			{
				client.Character.GroupMember.IsReadyForUpdates = true;
				client.Character.Group.UpdateGroupStats();
			}
		}
	}
}