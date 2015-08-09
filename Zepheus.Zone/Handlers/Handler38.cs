using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.Zone.Networking;
using System;

namespace Zepheus.Zone.Handlers
{
    public sealed class Handler38
    {
        [PacketHandler(CH38Type.GuildExtraRequest)]
        public static void GuildExtraRequest(ZoneClient client, Packet packet)
        {
            if (client.Character.Guild == null && client.Character.GuildAcademy == null)
                return;
            byte response;
            ushort ResponseCode;
            if (!packet.TryReadByte(out response))
                return;
            switch (response)
            {
                case 0:
                    if (client.Character.Guild == null)
                    {
                        //todo Response for is not in Guild end academymember
                        return;
                    }
                  ResponseCode = 6104;
                    break;
                case 1:

                    break;
                case 2:
                    break;
            }
            if (!client.Character.IsInaAcademy)
            {
                using (var pack = new Packet(SH38Type.GuildItemList))
                {
                
                    pack.WriteUShort(6104);//Responsecode //
                    pack.WriteLong(client.Character.Guild.GuildMoney);//guildmoney
                    pack.WriteByte((byte)client.Character.Guild.GuildStore.GuildStorageItems.Count);//ItemCount
                    foreach (var pItem in client.Character.Guild.GuildStore.GuildStorageItems.Values)
                    {
                        if (pItem.ItemInfo.Slot == ItemSlot.None)
                        {
                            pItem.WriteInfo(pack);
                            pack.WriteByte((byte)pItem.Ammount);//amount
                        }
                        else
                        {
                            pItem.WriteInfo(pack);
                        
                        }
                    }
                    client.SendPacket(pack);
                }
            }
            else
            {
                using (var pack = new Packet(SH38Type.GuildItemList))
                {

                    pack.WriteUShort(6104);//Responsecode //
                    pack.WriteLong(client.Character.Guild.GuildMoney);//guildmoney
                    pack.WriteByte((byte)client.Character.GuildAcademy.Guild.GuildStore.GuildStorageItems.Count);//ItemCount
                    foreach (var pItem in client.Character.GuildAcademy.Guild.GuildStore.GuildStorageItems.Values)
                    {
                        if (pItem.ItemInfo.Slot == ItemSlot.None)
                        {
                            pItem.WriteInfo(pack);
                            pack.WriteByte((byte)pItem.Ammount);//amount
                        }
                        else
                        {
                            pItem.WriteInfo(pack);

                        }
                    }
                    client.SendPacket(pack);
                }
            }
        }
    }
}
