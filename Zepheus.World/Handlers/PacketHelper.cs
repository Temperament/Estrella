using System;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.World.Data;

namespace Zepheus.World.Handlers
{
    public sealed class PacketHelper
    {
        public static void WriteBasicCharInfo(WorldCharacter wchar, Packet packet)
        {
            
            packet.WriteInt(wchar.Character.ID); //charid
            packet.FillPadding(wchar.Character.Name, 0x10);
            packet.WriteInt(0);//unk
            packet.WriteShort((short)wchar.Character.CharLevel); //level
            packet.WriteByte(wchar.Character.Slot);
            MapInfo mapinfo;
            if (!DataProvider.Instance.Maps.TryGetValue(wchar.Character.PositionInfo.Map, out mapinfo))
            {
                Log.WriteLine(LogLevel.Warn, "{0} has an invalid MapID ({1})", wchar.Character.Name, wchar.Character.PositionInfo.Map);
                wchar.Character.PositionInfo.Map = 0; 
                packet.FillPadding(mapinfo.ShortName, 0x0D); //townname
            }
            else
            {
                packet.FillPadding(mapinfo.ShortName, 0x0D); //townname
            }

            //packet.WriteByte(0); // UNK
            packet.WriteInt(0); // Random seed
            WriteLook(wchar,packet);
            WriteEquipment(wchar,packet);
            WriteRefinement(wchar,packet);
            packet.WriteByte(0);

            packet.WriteByte(0xF0);
            packet.WriteByte(0xFF);//unk
            packet.WriteByte(0xFF);

            packet.FillPadding(mapinfo.ShortName, 0x0c);
            packet.WriteInt(0); //pos
            packet.WriteInt(0); //pos
            packet.WriteUShort(0xdb78);
            packet.WriteUShort(4910);//unk
            packet.WriteUShort(25600);
            packet.Fill(4, 0);
            
        }

        public static void WriteLook(WorldCharacter wchar, Packet packet)
        {
            packet.WriteByte(Convert.ToByte(0x01 | (wchar.Character.Job << 2) | (Convert.ToByte(wchar.Character.LookInfo.Male)) << 7));
            packet.WriteByte(wchar.Character.LookInfo.Hair);
            packet.WriteByte(wchar.Character.LookInfo.HairColor);
            packet.WriteByte(wchar.Character.LookInfo.Face);
        }

        public static void WriteEquipment(WorldCharacter wchar, Packet packet)
        {
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.Helm));
            packet.WriteUShort(Settings.Instance.ShowEquips ? wchar.GetEquipBySlot(ItemSlot.Weapon) : (ushort)0xffff);
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.Armor));
            packet.WriteUShort(Settings.Instance.ShowEquips ? wchar.GetEquipBySlot(ItemSlot.Weapon2) : (ushort)0xffff);
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.Pants));
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.Boots));
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.CostumeBoots));
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.CostumePants));
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.CostumeArmor));
            packet.Fill(6, 0xff);              // UNK
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.Glasses));
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.CostumeHelm));
            packet.Fill(2, 0xff);              // UNK
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.CostumeWeapon));
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.Wing));
            packet.Fill(2, 0xff);              // UNK
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.Tail));
            packet.WriteUShort(wchar.GetEquipBySlot(ItemSlot.Pet));
        }

        public static void WriteRefinement(WorldCharacter wchar, Packet pPacket)
        {
            //TODO: pPacket.WriteByte(Convert.ToByte(this.Inventory.GetEquippedUpgradesByType(ItemType.Weapon) << 4 | this.Inventory.GetEquippedUpgradesByType(ItemType.Shield))); 
            pPacket.WriteByte(0xff); //this must be the above, but currently not cached
            pPacket.WriteByte(0xff);    		// UNK
            pPacket.WriteByte(0xff);    		// UNK
        }
    }
}
