using Zepheus.Util;
using Zepheus.Database;
using System.Collections.Generic;
using System.Data;
using Zepheus.Zone.Data;
using System.Collections;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.Zone.Game.Guilds;

namespace Zepheus.Zone.Game
{
    public sealed class GuildStorage
    {
        public Dictionary<byte,Item> GuildStorageItems { get; private set; }
        public Guild Guild { get; private set; }
        public GuildStorage(Guild G)
        {
            GuildStorageItems = new Dictionary<byte, Item>();
            Guild = G;
            LoadGuildStorageFromDatabase(G.ID);
        }
        public void SendAddGuildStore(GuildStoreAddFlags Flags,string Charname,long Value,long NewGuildMoney = 0,ushort ItemID = 0xFFFF)
        {
            using (var packet = new Packet(SH38Type.AddToGuildStore))
            {
                packet.WriteByte(0);//unk
                packet.WriteByte((byte)Flags);
                packet.WriteString(Charname, 16);
                packet.WriteUShort(ItemID);
                packet.WriteByte(0);
                packet.WriteLong(Value);
                packet.WriteLong(NewGuildMoney);//new GuildMoney
                Guild.Broadcast(packet);
            }
        }
        public void SendRemoveFromGuildStore(GuildStoreAddFlags Flags, string Charname, long Value, long NewGuildMoney = 0, ushort ItemID = 0xFFFF)
        {
            using (var packet = new Packet(SH38Type.RemoveFromGuildStore))
            {
                packet.WriteByte(0);//unk
                packet.WriteByte((byte)Flags);
                packet.WriteString(Charname, 16);
                packet.WriteUShort(ItemID);
                packet.WriteByte(0);
                packet.WriteLong(Value);
                packet.WriteLong(NewGuildMoney);//new GuildMoney
                Guild.Broadcast(packet);
            }
        }
        public void SaveStoreItem(int GuildID,ushort ItemID,byte pSlot)
        {
        }
        public void RemoveStoreItem(int GuildID,ushort ItemID)
        {
        }
        public bool GetHasFreeGuildStoreSlot()
        {
            for (byte i = 0; i < 92; i++)
            {
                if (!this.GuildStorageItems.ContainsKey(i))
                {
                    return true;
                }
            }
            return false;
        }
      
        private void LoadGuildStorageFromDatabase(int GuildID)
        {
            DataTable GuildItemData = null;
            using (DatabaseClient DBClient = Program.CharDBManager.GetClient())
            {
                GuildItemData = DBClient.ReadDataTable("SELECT * FROM GuildStorage WHERE GuildID=" + GuildID + "");
            }
            if (GuildItemData != null)
            {
                foreach(DataRow row in GuildItemData.Rows)
                {
                    ushort ItemID = System.Convert.ToUInt16(row["ItemID"]);
                    ushort Amount = System.Convert.ToUInt16(row["Amount"]);
                    byte pSlot = System.Convert.ToByte(row["Slot"]);
                    Item pItem = new Item(GuildID,ItemID, pSlot, Amount);
                    this.GuildStorageItems.Add(pSlot,pItem);
                }
            }
        }
    }
}
