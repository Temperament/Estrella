using System.Collections.Generic;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.InterLib.Networking;
using Zepheus.Util;
using Zepheus.World.Handlers;
using Zepheus.World.Networking;
using Zepheus.World.Data;

namespace Zepheus.World.InterServer
{
	public sealed class InterHandler
	{
		[InterPacketHandler(InterHeader.Worldmsg)]
		public static void HandleWorldMessage(ZoneConnection zc, InterPacket packet)
		{
			string msg;
			bool wut;
			byte type;
			if (!packet.TryReadString(out msg) || !packet.TryReadByte(out type) || !packet.TryReadBool(out wut))
			{
				return;
			}
			if (wut)
			{
				string to;
				if (!packet.TryReadString(out to))
				{
					return;
				}
				WorldClient client;
				if ((client = ClientManager.Instance.GetClientByCharname(to)) == null)
				{
					Log.WriteLine(LogLevel.Warn, "Tried to send a WorldMessage to a character that is unknown. Charname: {0}", to);
				}
				else
				{
					using (var p = Handler25.CreateWorldMessage((WorldMessageTypes)type, msg))
					{
						client.SendPacket(p);
					}
				}
			}
			else
			{
				using (var p = Handler25.CreateWorldMessage((WorldMessageTypes)type, msg))
				{
					ClientManager.Instance.SendPacketToAll(p);
				}
			}
		}
        [InterPacketHandler(InterHeader.ReciveCoper)]
        public static void ReciveCoper(ZoneConnection zc, InterPacket packet)
        {
            string charname;
            long coper;
            bool typ;
            if (!packet.TryReadString(out charname, 16))
                return;

            if (!packet.TryReadLong(out coper))
                return;
            if (!packet.TryReadBool(out typ))
                return;

            WorldClient pClient = ClientManager.Instance.GetClientByCharname(charname);
            if (typ)
            {
              pClient.Character.Character.ReviveCoper += coper;

            }
            else
            {
              
                pClient.Character.RecviveCoperMaster += coper;
                pClient.Character.UpdateRecviveCoper();
            }
        }


        public static void SendChangeMap(WorldCharacter pChar,int OldMap)
        {

            Managers.CharacterManager.InvokeChangeMapEvent(pChar);
            ZoneConnection conn =   Program.GetZoneByMap(OldMap);
            using (var packet = new InterPacket(InterHeader.GetBroadcastList))
            {

                packet.WriteString(pChar.Character.Name, 16);
                packet.WriteInt(pChar.Character.PositionInfo.XPos);
                packet.WriteInt(pChar.Character.PositionInfo.YPos);
                conn.SendPacket(packet);
            }
        }
		[InterPacketHandler(InterHeader.BanAccount)]
		public static void BanAccount(ZoneConnection zc, InterPacket packet)
		{
			  string playername;
			  if (packet.TryReadString(out playername, 16))
			  {
				  WorldClient bannclient = ClientManager.Instance.GetClientByCharname(playername);
				  if (bannclient != null)
				  {
					  using (var p = new InterPacket(InterHeader.BanAccount))
					  {
						  p.WriteInt(bannclient.AccountID);
						  LoginConnector.Instance.SendPacket(p);
					  }
					  bannclient.Disconnect();
				  }
			  }
		}
		[InterPacketHandler(InterHeader.ChangeZone)]
		public static void ChangeZoneBeginn(ZoneConnection zc, InterPacket packet)
		{
            ushort mapid,randomid,port;
            string charname,ip;
            int x,y;
            if(!packet.TryReadUShort(out mapid))
                return;
            if(!packet.TryReadInt(out x))
                return;
            if(!packet.TryReadInt(out y))
                return;
            if(!packet.TryReadString(out charname,16))
                return;
            if(!packet.TryReadString(out ip,16))
                return;
            if(!packet.TryReadUShort(out port))
                return;
            if(!packet.TryReadUShort(out randomid))
                return;
            
            WorldClient client = ClientManager.Instance.GetClientByCharname(charname);
            if(client == null)
                return;
            int oldmap = client.Character.Character.PositionInfo.Map;
            client.Character.Character.PositionInfo.Map = mapid;
            client.Character.Character.PositionInfo.XPos = x;
            client.Character.Character.PositionInfo.YPos = y;
            Managers.CharacterManager.InvokeChangeMapEvent(client.Character);
            client.Character.ChangeFrendMap(DataProvider.GetMapname(mapid));//setup later to event
            
		}
		[InterPacketHandler(InterHeader.Assigned)]
		public static void HandleAssigned(LoginConnector lc, InterPacket packet)
		{
			Log.WriteLine(LogLevel.Info, "<3 LoginServer.");
		}

		[InterPacketHandler(InterHeader.Assign)]
		public static void HandleAssigning(ZoneConnection lc, InterPacket packet)
		{
			string ip;
			if (!packet.TryReadString(out ip))
			{
				return;
			}

			lc.IP = ip;

			// make idlist
			InterHandler.SendZoneStarted(lc.ID, lc.IP, lc.Port, lc.Maps);
			InterHandler.SendZoneList(lc);
			Log.WriteLine(LogLevel.Info, "Zone {0} listens @ {1}:{2}", lc.ID, lc.IP, lc.Port);
		}
		 [InterPacketHandler(InterHeader.ClientDisconect)]
		public static void DisconnectFromzoneserver(ZoneConnection zc, InterPacket packet)
		{
			string charname;
			if (packet.TryReadString(out charname,16))
			{
			  WorldClient client =  ClientManager.Instance.GetClientByCharname(charname);
              if (client == null)
                  return;

			  client.Character.Loggeout(client);
			  ClientManager.Instance.RemoveClient(client);
			}
		}
         [InterPacketHandler(InterHeader.UpdateMoney)]
         public static void UpdateMoneyInWorld(ZoneConnection lc, InterPacket packet)
         {
             string charname = string.Empty;
             long NewMoney = 0;
             if(!packet.TryReadString(out charname,16) || !packet.TryReadLong(out NewMoney))
             {
                 return;
             }
            WorldCharacter Pchar = ClientManager.Instance.GetClientByCharname(charname).Character;
            Pchar.Character.Money = NewMoney;
         }
		[InterPacketHandler(InterHeader.Clienttransfer)]
		public static void HandleTransfer(LoginConnector lc, InterPacket packet)
		{
			byte v;
			if (!packet.TryReadByte(out v))
			{
				return;
			}

			if (v == 0)
			{
				byte admin;
				int accountid;
				string username, hash, hostip;
				if (!packet.TryReadInt(out accountid) || !packet.TryReadString(out username) || !packet.TryReadString(out hash) || !packet.TryReadByte(out admin) || !packet.TryReadString(out hostip)) {
					return;
				}
				ClientTransfer ct = new ClientTransfer(accountid, username,0, admin, hostip, hash);
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

		[InterPacketHandler(InterHeader.Clienttransferzone)]
		public static void HandleClientTransferZone(ZoneConnection zc, InterPacket packet)
		{
			byte admin, zoneid;
			int accountid,CharID;
			string username, charname, hostip;
			ushort randid, mapid;
			if (!packet.TryReadByte(out zoneid) || !packet.TryReadInt(out accountid) || !packet.TryReadUShort(out mapid) || !packet.TryReadString(out username) ||
				!packet.TryReadString(out charname)||!packet.TryReadInt(out CharID) || !packet.TryReadUShort(out randid) || !packet.TryReadByte(out admin) ||
				!packet.TryReadString(out hostip))
			{
				return;
			}
			if (Program.Zones.ContainsKey(zoneid))
			{
				ZoneConnection z;
				if (Program.Zones.TryGetValue(zoneid, out z))
				{
					z.SendTransferClientFromZone(accountid, username, charname,CharID, randid, admin, hostip);

				}
			}
			else
			{
				Log.WriteLine(LogLevel.Warn, "Uh oh, Zone {0} tried to transfer {1} to zone {1} D:", zc.ID, charname, zoneid);
			}
		}
        public static void SendGetCharacterBroaucast(WorldCharacter pChar,FiestaLib.Networking.Packet pPacket)
        {
            ZoneConnection conn = Program.GetZoneByMap(pChar.Character.PositionInfo.Map);
            using (var packet = new InterPacket(InterHeader.GetBroadcastList))
            {
              
                packet.WriteString(pChar.Character.Name, 16);
                packet.WriteInt(pPacket.ToNormalArray().Length);
                packet.WriteBytes(pPacket.ToNormalArray());
                conn.SendPacket(packet);
            }
        }
        [InterPacketHandler(InterHeader.SendBroiadCastList)]
        public static void GetList(ZoneConnection pConnection, InterPacket pPacket)
        {
            int count, packetlenght;
            byte[] SendPacket;

            if (!pPacket.TryReadInt(out packetlenght))
                return;

            if (!pPacket.TryReadBytes(packetlenght, out SendPacket))
                return;

            if (!pPacket.TryReadInt(out count))
                return;
            
            for (int i = 0; i < count; i++)
            {
                string charname;
                if(!pPacket.TryReadString(out charname,16))
                return;
               WorldClient client=  ClientManager.Instance.GetClientByCharname(charname);
                if(client !=null)
                using (var packet = new FiestaLib.Networking.Packet())
                {
                    packet.WriteBytes(SendPacket);
                    client.SendPacket(packet);
                    Log.WriteLine(LogLevel.Debug, "Send borcast to {0}", charname);
                }
            }
        }
        
        [InterPacketHandler(InterHeader.CharacterLevelUP)]
		public static void UpdateLevel(ZoneConnection pConnection, InterPacket pPacket)
		{
            byte level;
            string Charname;
            if (!pPacket.TryReadByte(out level) || !pPacket.TryReadString(out Charname, 16))
                return;
            WorldClient pClient = ClientManager.Instance.GetClientByCharname(Charname);
            if(pClient == null)
                return;
            pClient.Character.Character.CharLevel = level;
            Managers.CharacterManager.invokeLevelUp(pClient.Character);
        }
		[InterPacketHandler(InterHeader.FunctionCharIsOnline)]
		public static void FunctionGetCharacterOnline(ZoneConnection pConnection, InterPacket pPacket)
		{
			long id;
			string charName;

			if(!pPacket.TryReadLong(out id) ||!pPacket.TryReadString(out charName, 16))
				throw new InvalidPacketException();

			bool isOnline = ClientManager.Instance.IsOnline(charName);
			using (InterPacket packet = new InterPacket(InterHeader.FunctionAnswer))
			{
				packet.WriteLong(id);
				packet.WriteBool(isOnline);
				pConnection.SendPacket(packet);
			}
		}

		public static void TryAssiging(LoginConnector lc)
		{
			using (var p = new InterPacket(InterHeader.Assign))
			{
				p.WriteByte(Settings.Instance.ID);
				p.WriteStringLen(Settings.Instance.WorldName);
				p.WriteStringLen(Settings.Instance.IP);
				p.WriteUShort(Settings.Instance.Port);
				lc.SendPacket(p);
			}
		}
        public static void SendAddReward(ZoneConnection ZC, ushort itemID, byte count,string CharName)
        {
            using (var packet = new InterPacket(InterHeader.SendAddRewardItem))
            {
                packet.WriteUShort(itemID);
                packet.WriteByte(count);
                packet.WriteString(CharName, 16);
                ZC.SendPacket(packet);
            }
        }
		public static void SendZoneStarted(byte zoneid, string ip, ushort port, List<MapInfo> maps)
		{
			using (var packet = new InterPacket(InterHeader.Zoneopened))
			{
				packet.WriteByte(zoneid);
				packet.WriteStringLen(ip);
				packet.WriteUShort(port);
				packet.WriteInt(maps.Count);
				foreach (var m in maps)
				{
					packet.WriteUShort(m.ID);
					packet.WriteStringLen(m.ShortName);
					packet.WriteStringLen(m.FullName);
					packet.WriteInt(m.RegenX);
					packet.WriteInt(m.RegenY);
					packet.WriteByte(m.Kingdom);
					packet.WriteUShort(m.ViewRange);
				}
				foreach (var c in Program.Zones.Values)
				{
					if (c.ID != zoneid)
						c.SendPacket(packet);
				}
			}
		}

		public static void SendZoneList(ZoneConnection zc)
		{
			using (var packet = new InterPacket(InterHeader.Zonelist))
			{
				packet.Write(Program.Zones.Values.Count);
				foreach (var z in Program.Zones.Values)
				{
					packet.Write(z.ID);
					packet.Write(z.IP);
					packet.Write(z.Port);
					packet.WriteInt(z.Maps.Count);
					foreach (var m in z.Maps)
					{
						packet.WriteUShort(m.ID);
						packet.WriteStringLen(m.ShortName);
						packet.WriteStringLen(m.FullName);
						packet.WriteInt(m.RegenX);
						packet.WriteInt(m.RegenY);
						packet.WriteByte(m.Kingdom);
						packet.WriteUShort(m.ViewRange);
					}
				}
				zc.SendPacket(packet);
			}
		}
		public static void SendZoneStopped(byte zoneid)
		{
			using (var packet = new InterPacket(InterHeader.Zoneclosed))
			{
				packet.Write(zoneid);
				foreach (var c in Program.Zones.Values)
					c.SendPacket(packet);
			}
		}
	}
}
