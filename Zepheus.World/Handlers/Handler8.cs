using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.World.Networking;

namespace Zepheus.World.Handlers
{
    public class Handler8
    {
        [PacketHandler(CH8Type.ChatParty)]
        public static void PartyChat(WorldClient client, Packet packet)
        {
            if (client.Character.Group == null)
                return;

            byte msgLen;
            string msg = string.Empty;

            if (!packet.TryReadByte(out msgLen) || !packet.TryReadString(out msg, msgLen))
                return;

            client.Character.Group.Chat(client, msg);

        }
        [PacketHandler(CH8Type.WisperTo)]
        public static void Wisper(WorldClient client, Packet packet)
        {
            string toname;
            byte messagelenght;
            if (packet.TryReadString(out toname, 16) && packet.TryReadByte(out messagelenght))
            {
                string message;
                if (!packet.TryReadString(out message, messagelenght))
                {
                    return;
                }
                WorldClient toChar = ClientManager.Instance.GetClientByCharname(toname);
                if (toChar != null)
                {
                    using (var frompacket = new Packet(SH8Type.WisperFrom))
                    {
                        frompacket.WriteString(client.Character.Character.Name, 16);
                        if (!toChar.Character.BlocketUser.Contains(client.Character.Character.Name))
                        {
                            frompacket.WriteByte(0);
                        }
                        else
                        {
                            frompacket.WriteByte(12);//blocket notdisplay message
                        }
                        frompacket.WriteByte(messagelenght);
                        frompacket.WriteString(message, messagelenght);
                        toChar.SendPacket(frompacket);
                    }
                    using (var pack = new Packet(SH8Type.WisperTo))
                    {
                        pack.WriteString(toname, 16);
                        pack.WriteByte(messagelenght);
                        pack.WriteString(message, messagelenght);
                        client.SendPacket(pack);
                    }
                }
                else
                {
                    //target not found
                    using (var pp = new Packet(SH8Type.WisperTargetNotfound))
                    {
                        pp.WriteUShort(3945);//unk
                        pp.WriteString(toname, 16);
                        client.SendPacket(pp);
                    }
                }
            }
        }

    }
}
