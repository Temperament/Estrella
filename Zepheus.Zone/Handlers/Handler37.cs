using Zepheus.FiestaLib;
using Zepheus.InterLib.Networking;
using Zepheus.FiestaLib.Networking;
using Zepheus.Zone.Networking;

namespace Zepheus.Zone.Handlers
{
    public sealed class Handler37
    {
        [PacketHandler(CH37Type.MasterRewardCoperRquest)]
        public static void MasterRequestCoper(ZoneClient client, Packet pPacket)
        {
            byte unk;
            if (!pPacket.TryReadByte(out unk))
                return;
            using (var packet = new Packet(SH37Type.SendRecivveCopper))
            {
                packet.WriteUShort(7264);//unk
                packet.WriteLong(client.Character.Character.ReviveCoper);
                client.SendPacket(packet);
            }
        }
          [PacketHandler(CH37Type.SendReciveCoperAccept)]
        public static void MasterRequestAcceptCoper(ZoneClient client, Packet pPacket)
        {
            client.Character.Character.ReviveCoper = 0;
             InterServer.InterHandler.SendReciveCoper(client.Character.Character.Name, client.Character.Character.ReviveCoper,true);
             using(var packet = new Packet(37,65))
             {
 
                packet.WriteUShort(7272);//unk
                packet.WriteLong(client.Character.RecviveCoper);
                client.SendPacket(packet);
             }
          
        }
    }
}
