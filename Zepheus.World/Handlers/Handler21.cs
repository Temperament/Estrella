using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.World.Networking;
using Zepheus.World.Data;

namespace Zepheus.World.Handlers
{
    public sealed class Handler21
    {
        [PacketHandler(CH21Type.FriendListDelete)]
        public static void FriendListDelete(WorldClient pClient, Packet pPacket)
        {
            string sender, target;
            if (!pPacket.TryReadString(out sender, 16) ||
                !pPacket.TryReadString(out target, 16))
            {
                Log.WriteLine(LogLevel.Warn, "Error parsing friend delete request.");
                return;
            }

            if (pClient.Character.DeleteFriend(target))
            {
                WorldClient client = ClientManager.Instance.GetClientByCharname(target);
                if (client != null)
                {
                    using (var pack = new Packet(SH21Type.FriendDeleteSend))
                    {
                        pack.WriteString(sender, 16);
                        client.SendPacket(pack);
                    }
                    Friend friend = pClient.Character.Friends.Find(f => f.Name == target);
                    if(friend != null)
                    pClient.Character.Friends.Remove(friend);
                }

                using (var pack = new Packet(SH21Type.FriendDeleteSend))
                {
                    pack.WriteString(sender, 16);
                    pack.WriteString(target, 16);
                    pack.WriteShort(0x0951);
                   pClient.SendPacket(pack);
                }

            }
            else
            {
                using (var pack = new Packet(SH21Type.FriendInviteResponse))
                {
                    pack.WriteString(target, 16);
                    pack.WriteString(sender, 16);
                    pack.WriteUShort(0x0946);	// Cannot find ${Target}
                    pClient.SendPacket(pack);
                }
            }
        }
        [PacketHandler(CH21Type.FriendInviteResponse)]
        public static void FriendInviteResponse(WorldClient pClient, Packet pPacket)
        {
            string target, sender;
            bool response;
            if (!pPacket.TryReadString(out target, 16) ||
                !pPacket.TryReadString(out sender, 16) ||
                !pPacket.TryReadBool(out response))
            {
                Log.WriteLine(LogLevel.Warn, "Could not reat friend invite response.");
                return;
            }
            WorldClient sendchar = ClientManager.Instance.GetClientByCharname(sender);
            if (sendchar == null)
            {
                Log.WriteLine(LogLevel.Warn, "Invalid friend reject received.");
                return;
            }
            if (response)
            {
                Friend sendfriend = sendchar.Character.AddFriend(pClient.Character);
                if (sendfriend != null)
                {
                    using (var packet = new Packet(SH21Type.FriendInviteResponse))
                    {
                        packet.WriteString(sender, 16);
                        packet.WriteString(target, 16);
                        packet.WriteByte(0);
                        sendchar.SendPacket(packet);
                    }

                    using (var packet = new Packet(SH21Type.FriendExtraInformation))
                    {
                        sendfriend.WritePacket(packet);
                        sendchar.SendPacket(packet);
                    }
                }
            }
            else
            {
                using (var packet = new Packet(SH21Type.FriendInviteReject))
                {
                    packet.WriteString(target, 16);
                    sendchar.SendPacket(packet);
                }
            }

        }
        [PacketHandler(CH21Type.FriendInvite)]
        public static void FriendInvite(WorldClient pClient, Packet pPacket)
        {
            string sender, receiver;
            if (!pPacket.TryReadString(out sender, 16) ||
                !pPacket.TryReadString(out receiver, 16))
            {
                Log.WriteLine(LogLevel.Warn, "Error reading friend invite.");
                return;
            }

            WorldCharacter inviter = pClient.Character;
            WorldClient invitee = ClientManager.Instance.GetClientByCharname(receiver);
            if (invitee == null)
            {
                //character not found
                using (var pack = new Packet(SH21Type.FriendInviteResponse))
                {
                    pack.WriteString(sender, 16);
                    pack.WriteString(receiver, 16);
                    pack.WriteUShort(0x0946);	// Cannot find ${Target}

                    pClient.SendPacket(pack);
                }
            }
            else if (receiver == sender)
            {
                using (var pack = new Packet(SH21Type.FriendInviteResponse))
                {
                    pack.WriteString(sender, 16);
                    pack.WriteString(receiver, 16);
                    pack.WriteUShort(0x0942);	// You cannot add yourself to your Buddy List.

                    pClient.SendPacket(pack);
                }
            }
            else if (inviter.Friends.Find(f => f.Name == receiver) != null)
            {
                using (var pack = new Packet(SH21Type.FriendInviteResponse))
                {
                    pack.WriteString(sender, 16);
                    pack.WriteString(receiver, 16);
                    pack.WriteUShort(0x0945);	// {Target} is already registered in the friends list.
                    pClient.SendPacket(pack);
                }
            }
            else
            {
                using (var pack = new Packet(SH21Type.FriendInviteRequest))
                {
                    pack.WriteString(receiver, 16);
                    pack.WriteString(sender, 16);

                    invitee.SendPacket(pack);
                }
            }
        }
    }
}
