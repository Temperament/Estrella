using System.Collections.Generic;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.World.Networking;

namespace Zepheus.World.Handlers
{
	public sealed class Handler14
	{
		[PacketHandler(CH14Type.PartyReqest)]
		public static void PartyReqest(WorldClient client, Packet packet)
		{
			string invitedChar;
			if (packet.TryReadString(out invitedChar, 16))
			{
				GroupManager.Instance.Invite(client, invitedChar);
			}
		}
		[PacketHandler(CH14Type.PartyLeave)]
		public static void PartyLeave(WorldClient client, Packet packet)
		{
			GroupManager.Instance.LeaveParty(client);
		}
		[PacketHandler(CH14Type.PartyDecline)]
		public static void PartyDecline(WorldClient client, Packet packet)
		{
			string inviteChar;
			if (packet.TryReadString(out inviteChar, 0x10))
			{
				GroupManager.Instance.DeclineInvite(client, inviteChar);
			}
		}
		[PacketHandler(CH14Type.PartyMaster)]
		public static void MasterList(WorldClient client, Packet packet)
		{
			Dictionary<string, string> list = new Dictionary<string, string>
			{
				{"Char1", "hier ist Char1"},
				{"Char2", "hier ist Char2"}
			};
			using (var ppacket = new Packet(SH14Type.GroupList))
			{
				ppacket.WriteHexAsBytes("00 00 14 01 01 00 01 00 00 00");
				ppacket.WriteInt(list.Count);
				foreach (KeyValuePair<string, string> stat in list)
				{
					// Note - teh fuck?
					ppacket.WriteHexAsBytes("");
					ppacket.WriteString("haha", 16);
					ppacket.WriteString("1234567890123456789012345678901234567890123456", 46);
					ppacket.WriteHexAsBytes("00 00 00 00 44 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 8C 8E CD 00 88 49 DF 4E B3 08 4C 00 78 26 43 00 01 00 00 00 5A 68 42 00 18 FE 64 02 40 55 DF 4E 08 27 4D 00 94 FF 64 02 24 00 00 00 BD 68 42 00 87 BE");
				}

				list.Clear();
				client.SendPacket(ppacket);
			}


		}
		[PacketHandler(CH14Type.KickPartyMember)]
		public static void KickPartyMember(WorldClient client, Packet packet)
		{
			string removeName;
			if (packet.TryReadString(out removeName, 16))
			{
				if(!client.Character.Group.HasMember(removeName))
					return;

				GroupManager.Instance.KickMember(client, removeName);
			}
		}
		[PacketHandler(CH14Type.ChangePartyDrop)]
		public static void ChangeDropMode(WorldClient client, Packet packet)
		{
			byte dropState;
			if (packet.TryReadByte(out dropState)) {
				client.Character.Group.ChangeDropType(client.Character, dropState);
			}
		}
		[PacketHandler(CH14Type.ChangePartyMaster)]
		public static void ChangePartyMaster(WorldClient client, Packet packet)
		{
			string mastername;
			if (packet.TryReadString(out mastername, 16))
			{
				if(client.Character.Group.Master.Name != client.Character.Character.Name)
					return;

				GroupManager.Instance.ChangeMaster(client, mastername);
			}
		}
		[PacketHandler(CH14Type.PartyAccept)]
		public static void AcceptParty(WorldClient client, Packet packet)
		{
			string inviteChar;
			if (packet.TryReadString(out inviteChar, 16))
			{
				GroupManager.Instance.AcceptInvite(client, inviteChar);
			}
		}
	}
}