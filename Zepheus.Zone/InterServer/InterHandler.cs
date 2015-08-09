using System;
using System.Collections.Generic;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.InterLib.Networking;
using Zepheus.Util;
using Zepheus.Zone.Data;
using Zepheus.Zone.Game;
using Zepheus.Zone.Networking;

namespace Zepheus.Zone.InterServer
{
	public sealed class InterHandler
	{
		[InterPacketHandler(InterHeader.FunctionAnswer)]
		public static void FunctionAnswer(WorldConnector pConnector, InterPacket pPacket)
		{
			long id;
			if(!pPacket.TryReadLong(out id))
				throw new InvalidPacketException();
			object result = InterFunctionCallbackProvider.Instance.GetReadFunc(id)(pPacket);
			InterFunctionCallbackProvider.Instance.OnResult(id, result);
		}
        [InterPacketHandler(InterHeader.GetBroadcastList)]
        public static void Broadcast(WorldConnector pConnector, InterPacket pPacket)
        {
            int packetlenght;
            byte[] packet;
            string charname;
            if (!pPacket.TryReadString(out charname, 16))
                return;

            if (!pPacket.TryReadInt(out packetlenght))
                return;

            if (!pPacket.TryReadBytes(packetlenght, out packet))
                return;

            ZoneClient pClient = ClientManager.Instance.GetClientByCharName(charname);
            if (pClient == null)
                return;
            using (var ipacket = new InterPacket(InterHeader.SendBroiadCastList))
            {
                List<ZoneCharacter> Sender = pClient.Character.Map.GetCharactersBySectors(pClient.Character.MapSector.SurroundingSectors);
                if (Sender.Count == 0)
                    return;
                ipacket.WriteInt(packetlenght);
                ipacket.WriteBytes(packet);
                ipacket.WriteInt(Sender.Count);
                foreach (var character in Sender)
                {
                    ipacket.WriteString(character.Name,16);
                }
                pConnector.SendPacket(ipacket);
            }
        }
        [InterPacketHandler(InterHeader.SendAddRewardItem)]
        public static void AddRewardItem(WorldConnector pConnector, InterPacket pPacket)
        {
            byte count;
            ushort ItemID;
            string Charname;
            if (!pPacket.TryReadUShort(out ItemID))
                return;

            if (!pPacket.TryReadByte(out count))
                return;

            if(!pPacket.TryReadString(out Charname,16))
                return;

             ZoneClient pClient =  ClientManager.Instance.GetClientByName(Charname);
            if(pClient == null)
                return;

            pClient.Character.GiveMasterRewardItem(ItemID, count);

        }
		[InterPacketHandler(InterHeader.Assigned)]
		public static void HandleAssigned(WorldConnector lc, InterPacket packet)
		{
			string name;
			byte id;
			ushort port;
			int mapidcout;
			if (!packet.TryReadByte(out id) || !packet.TryReadString(out name) ||
				!packet.TryReadUShort(out port) || !packet.TryReadInt(out mapidcout))
			{
				return;
			}

			Program.ServiceInfo = new ZoneData
			{
		  
				ID = id,
				Port = port,
				MapsToLoad = new List<FiestaLib.Data.MapInfo>()
			};

			for (int i = 0; i < mapidcout; i++)
			{
				ushort mapid, viewrange;
				string shortname, fullname;
				int regenx, regeny;
				byte kingdom;
				if (!packet.TryReadUShort(out mapid) || !packet.TryReadString(out shortname) || !packet.TryReadString(out fullname) || !packet.TryReadInt(out regenx) || !packet.TryReadInt(out regeny) || !packet.TryReadByte(out kingdom) || !packet.TryReadUShort(out viewrange))
				{
					break;
				}
				Program.ServiceInfo.MapsToLoad.Add(new MapInfo(mapid, shortname, fullname, regenx, regeny, kingdom, viewrange));
			}

			Log.WriteLine(LogLevel.Info, "Successfully linked with worldserver. [Zone: {0} | Port: {1}]", id, port);
			ZoneAcceptor.Load();

		}
		[InterPacketHandler(InterHeader.Zoneclosed)]
		public static void HandleZoneClosed(WorldConnector lc, InterPacket packet)
		{
			byte id;
			if (!packet.TryReadByte(out id))
			{
				return;
			}
			ZoneData zd;
			if (Program.Zones.TryRemove(id, out zd))
			{
				Log.WriteLine(LogLevel.Info, "Removed zone {0} from zones (disconnected)", id);
			}

		}
        public static void SendReciveCoper(string name, long Coper,bool CoperType)
        {
            using(var packet = new InterPacket(InterHeader.ReciveCoper))
            {
                packet.WriteString(name, 16);
                packet.WriteLong(Coper);
                packet.WriteBool(CoperType);
                WorldConnector.Instance.SendPacket(packet);
            }
        }
        public static void SendLevelUpToWorld(byte Level, string charname)
        {
            using (var packet = new InterPacket(InterHeader.CharacterLevelUP))
            {
                packet.WriteByte(Level);
                packet.WriteString(charname, 16);
                WorldConnector.Instance.SendPacket(packet);
            }
        }
        public static void UpdateMoneyWorld(long Money,string charname)
        {
            using (var packet = new InterPacket(InterHeader.UpdateMoney))
            {
                packet.WriteString(charname, 16);
                packet.WriteLong(Money);
                WorldConnector.Instance.SendPacket(packet);
            }
        }
		[InterPacketHandler(InterHeader.Zoneopened)]
		public static void HandleZoneOpened(WorldConnector lc, InterPacket packet)
		{
			byte id;
			string ip;
			ushort port;
			int mapcount;
			if (!packet.TryReadByte(out id) || !packet.TryReadString(out ip) || !packet.TryReadUShort(out port) || !packet.TryReadInt(out mapcount))
			{
				return;
			}

			List<MapInfo> maps = new List<MapInfo>();
			for (int j = 0; j < mapcount; j++)
			{
				ushort mapid, viewrange;
				string shortname, fullname;
				int regenx, regeny;
				byte kingdom;
				if (!packet.TryReadUShort(out mapid) || !packet.TryReadString(out shortname) || !packet.TryReadString(out fullname) || !packet.TryReadInt(out regenx) || !packet.TryReadInt(out regeny) || !packet.TryReadByte(out kingdom) || !packet.TryReadUShort(out viewrange))
				{
					break;
				}
				maps.Add(new MapInfo(mapid, shortname, fullname, regenx, regeny, kingdom, viewrange));
			}

			ZoneData zd;
			if (!Program.Zones.TryGetValue(id, out zd))
			{
				zd = new ZoneData();
			}
			zd.ID = id;
			zd.IP = ip;
			zd.Port = port;
			zd.MapsToLoad = maps;
			Program.Zones[id] = zd;
			Log.WriteLine(LogLevel.Info, "Added zone {0} to zonelist. {1}:{2}", zd.ID, zd.IP, zd.Port);
		}
		[InterPacketHandler(InterHeader.AddPartyMember)]
		public static void AddPartyMember(WorldConnector lc, InterPacket packet)
		{
			long groupId = 0;
			string charName = "";
			if (!packet.TryReadLong(out groupId) && 
				!packet.TryReadString(out charName, 16))
			{
				throw new InvalidPacketException();
			}
			GroupManager.Instance.AddMemberToGroup(groupId, charName);
		}
		[InterPacketHandler(InterHeader.RemovePartyMember)]
		public static void RemovePartyMember(WorldConnector lc, InterPacket packet)
		{
            string name = "";
            if (!packet.TryReadString(out name, 16))
            {
                throw new InvalidPacketException();
            }

            if (!ClientManager.Instance.HasClient(name))
                return;
            var client = ClientManager.Instance.GetClientByCharName(name);
            var group = GroupManager.Instance.GetGroupForCharacter(client.Character.ID);
            group.RemoveMember(name);
		}
		[InterPacketHandler(InterHeader.NewPartyCreated)]
		public static void NewPartyCreated(WorldConnector lc, InterPacket packet)
		{
			long id;
			if (!packet.TryReadLong(out id))
			{
				throw new InvalidPacketException();
			}
			GroupManager.Instance.NewGroupCreated(id);
		}
		[InterPacketHandler(InterHeader.Zonelist)]
		public static void HandleZoneList(WorldConnector lc, InterPacket packet)
		{
			int amount;
			if (!packet.TryReadInt(out amount))
			{
				return;
			}

			for (int i = 0; i < amount; i++)
			{
				byte id;
				string ip;
				ushort port;
				int mapcount;
				if (!packet.TryReadByte(out id) || !packet.TryReadString(out ip) || !packet.TryReadUShort(out port) || !packet.TryReadInt(out mapcount))
				{
					return;
				}
				var maps = new List<MapInfo>();
				for (int j = 0; j < mapcount; j++)
				{
					ushort mapid, viewrange;
					string shortname, fullname;
					int regenx, regeny;
					byte kingdom;
					if (!packet.TryReadUShort(out mapid) || !packet.TryReadString(out shortname) || !packet.TryReadString(out fullname) || !packet.TryReadInt(out regenx) || !packet.TryReadInt(out regeny) || !packet.TryReadByte(out kingdom) || !packet.TryReadUShort(out viewrange))
					{
						break;
					}
					maps.Add(new MapInfo(mapid, shortname, fullname, regenx, regeny, kingdom, viewrange));
				}

				ZoneData zd;
				if (!Program.Zones.TryGetValue(id, out zd))
				{
					zd = new ZoneData();
				}
				zd.ID = id;
				zd.IP = ip;
				zd.Port = port;
				zd.MapsToLoad = maps;
				Program.Zones[id] = zd;
				Log.WriteLine(LogLevel.Info, "Added zone {0} to zonelist. {1}:{2}", zd.ID, zd.IP, zd.Port);
			}
		}
        [InterPacketHandler(InterHeader.UpdateMoneyFromWorld)]
        public static void ChangeMoney(WorldConnector pConnector, InterPacket pPacket)
        {
            int CharID;
            long NewMoney;
            if (!pPacket.TryReadInt(out CharID))
                return;
            if (!pPacket.TryReadLong(out NewMoney))
                return;
            ZoneClient pClient = ClientManager.Instance.GetClientByCharID(CharID);
            if (pClient == null)
                return;
            pClient.Character.ChangeMoney(NewMoney);
        }
		[InterPacketHandler(InterHeader.NewPartyCreated)]
		public static void NewGroupCreated(WorldConnector pConnector, InterPacket pPacket)
		{
			long groupId = -1;
			if (!pPacket.TryReadLong(out groupId))
			{
				throw new InvalidPacketException();
			}
			GroupManager.Instance.NewGroupCreated(groupId);
		}
		[InterPacketHandler(InterHeader.Clienttransfer)]
		public static void HandleTransfer(WorldConnector lc, InterPacket packet)
		{
			byte v;
			if (!packet.TryReadByte(out v))
			{
				return;
			}

			if (v == 0)
			{
				byte admin;
				int accountid, CharID;
				string username, hash, hostip;
				if (!packet.TryReadInt(out accountid) || !packet.TryReadString(out username) || !packet.TryReadInt(out CharID)|| !packet.TryReadString(out hash) || !packet.TryReadByte(out admin) || !packet.TryReadString(out hostip))
				{
					return;
				}
				ClientTransfer ct = new ClientTransfer(accountid, username,CharID, admin, hostip, hash);
				ClientManager.Instance.AddTransfer(ct);
			}
			else if (v == 1)
			{
				byte admin;
				int accountid,CharID;
				string username, charname, hostip;
				ushort randid;
                if (!packet.TryReadInt(out accountid) || !packet.TryReadString(out username) || !packet.TryReadString(out charname) || !packet.TryReadInt(out CharID) ||
					!packet.TryReadUShort(out randid) || !packet.TryReadByte(out admin) || !packet.TryReadString(out hostip))
				{
					return;
				}
				ClientTransfer ct = new ClientTransfer(accountid, username, charname,CharID, randid, admin, hostip);
				ClientManager.Instance.AddTransfer(ct);
			}
		}

        [InterPacketHandler(InterHeader.PartyBrokeUp)]
        public static void GroupBrokeUp(WorldConnector lc, InterPacket packet)
        {
            long groupId;
            if(!packet.TryReadLong(out groupId))
                return;
            GroupManager.Instance.GroupBrokeUp(groupId);
        }

		public static void TryAssiging(WorldConnector lc)
		{
			using (var p = new InterPacket(InterHeader.Assign))
			{
				p.WriteStringLen(Settings.Instance.IP);
				lc.SendPacket(p);
			}
		}
        public static void SendChangeZoneToWorld(ZoneCharacter character, ushort mapid, int x, int y, string ip, ushort port, ushort randomid)
        {
           
              using (var packet = new InterPacket(InterHeader.ChangeZone))
              {
                  packet.WriteUShort(mapid);
                  packet.WriteInt(x);
                  packet.WriteInt(y);
                  packet.WriteString(character.Name,16);
                  packet.WriteString(Settings.Instance.IP, 16);
                  packet.WriteUShort(port);
                  packet.WriteUShort(randomid);
                  WorldConnector.Instance.SendPacket(packet);
              }
        }
		public static void TransferClient(byte zoneID,ushort mapid, int accountID, string userName,int CharID, string charName, ushort randid, byte admin, string hostIP)
		{
			using (var packet = new InterPacket(InterHeader.Clienttransferzone))
			{
				packet.WriteByte(zoneID);
				packet.WriteInt(accountID);
				packet.WriteUShort(mapid);
				packet.WriteStringLen(userName);
				packet.WriteStringLen(charName);
                packet.WriteInt(CharID);
				packet.WriteUShort(randid);
				packet.WriteByte(admin);
				packet.WriteStringLen(hostIP);
				WorldConnector.Instance.SendPacket(packet);
			}
		}

		public static void SendWorldMessage(WorldMessageTypes type, string message, string to = "")
		{
			using (var packet = new InterPacket(InterHeader.Worldmsg))
			{
				packet.WriteStringLen(message);
				packet.WriteByte((byte)type);
				packet.WriteBool(to != "");
				if (to != "")
				{
					packet.WriteStringLen(to);
				}
				WorldConnector.Instance.SendPacket(packet);
			}
		}
	}
}
