using Zepheus.Database.DataStore;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.FiestaLib.Networking;
using Zepheus.Zone.Data;
using Zepheus.Database.Storage;

namespace Zepheus.Zone.Game
{
    public sealed class RewardItem : Item
    {
        
        public override ushort ID { get; set; }
        public override sbyte Slot { get; set; }
        public override UpgradeStats UpgradeStats { get; set; }
        public int CharID { get; set; }
        public ushort PageID { get; set; }
        public override ItemInfo ItemInfo { get { return DataProvider.Instance.GetItemInfo(this.ID); } }
        public  void AddToDatabase()
        {
            Program.CharDBManager.GetClient().ExecuteQuery("INSERT INTO  Rewarditems (CharID,Slot,ItemID,PageID) VALUES ('" + this.CharID + "','" + this.Slot + "','" + this.ID + "','" + this.PageID + "')");
        }
        public void RemoveFromDatabase()
        {
            Program.CharDBManager.GetClient().ExecuteQuery("DELETE FROM Rewarditems WHERE CharID='" + this.CharID + "' AND ItemID='" + this.ID + "'");
        }
        public override void WriteInfo(Packet pPacket, bool WriteStats = true)
        {
            byte length;
            byte statCount;

            if (ItemInfo.Slot == ItemSlot.None)
            {
                length = GetInfoLength(ItemInfo.Class);
                statCount = 0;
            }
            else
            {
                length = GetEquipLength(this);
                statCount = GetInfoStatCount(this);
            }
            byte lenght = 9;//later
            pPacket.WriteByte(lenght);
            pPacket.WriteByte((byte)this.Slot);//itemslot
            pPacket.WriteByte(0x08);//unk
            if (WriteStats)
            {
                if (ItemInfo.Slot == ItemSlot.None)
                    this.WriteStats(pPacket);
                else
                    WriteEquipStats(pPacket);
            }

        }

        public static  RewardItem LoadFromDatabase(System.Data.DataRow row)
        {
           RewardItem ppItem = new RewardItem
            {
                Slot = GetDataTypes.GetSByte(row["Slot"]),
                ID = GetDataTypes.GetUshort(row["ItemID"]),
                CharID = GetDataTypes.GetInt(row["CharID"]),
                PageID = GetDataTypes.GetByte(row["PageID"])
            };
            
            return ppItem;
        }
    }
}
