using System;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.World.Networking;
using Zepheus.World.Data;

namespace Zepheus.World.Handlers
{
    public sealed class Handler37
    {
        [PacketHandler(CH37Type.MasterRequest)]
        public static void MasterRequest(WorldClient client, Packet packet)
        {
            string playername = string.Empty;
            string target = string.Empty;
            if (!packet.TryReadString(out playername, 16))
                return;
            if (!packet.TryReadString(out target, 16))
                return;
            MasterManager.Instance.AddMasterRequest(client, target);

        }
        [PacketHandler(CH37Type.RemoveMasterByApprentice)]
        public static void MasterRemoveByApprentice(WorldClient client, Packet packet)
        {
            MasterManager.Instance.RemoveMasterMember(client);
        }
        [PacketHandler(CH37Type.MasterRemove)]
        public static void MasterRemove(WorldClient client, Packet packet)
        {
            string removename;
            if(!packet.TryReadString(out removename,16))
                return;
            MasterManager.Instance.RemoveMasterMember(client.Character, removename);
        }
        [PacketHandler(CH37Type.MasterRequestResponse)]
        public static void MasterRequestResponse(WorldClient client, Packet packet)
        {
            string requester = string.Empty;
            string target = string.Empty;
            byte response;
            if (!packet.TryReadString(out requester, 16))
                return;
            if (!packet.TryReadString(out target, 16))
                return;

            if (!packet.TryReadByte(out response))
                return;
            if (response == 0)
            {
                MasterManager.Instance.RemoveMasterRequest(client);
            }
            else if(response == 1)
            {
                MasterManager.Instance.MasterRequestAccept(requester, target);
            }
        }
    }
}
