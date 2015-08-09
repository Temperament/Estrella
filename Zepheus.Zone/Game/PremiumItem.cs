using System;
using System.Data;
using Zepheus.Database.DataStore;
using Zepheus.FiestaLib.Networking;
using Zepheus.Zone.Game;

namespace Zepheus.Zone
{
    public class PremiumItem
    {
        public int UniqueID { get; set; }
        public int ShopID { get; set; }
        public int CharID { get; set; }
        public byte PageID { get; set; }
        public byte Slot { get; set; }

        public void WritePremiumInfo(Packet packet)
        {
            packet.WriteInt(this.UniqueID);
            packet.WriteInt(this.ShopID);
            packet.WriteInt(0);//unk
            packet.WriteInt(0);//unk
      
        }
        public virtual void RemoveFromDatabase()
        {
            Program.CharDBManager.GetClient().ExecuteQuery("DELETE FROM PremiumItem WHERE CharID='" + this.CharID + "' AND UniqueID='" + this.UniqueID + "'");
        }
        public virtual void AddToDatabase()
        {
            Program.CharDBManager.GetClient().ExecuteQuery("INSERT INTO PremiumItems (CharID,ShopID,UniqueID,PageID) VALUES ('"+this.CharID+"','"+this.ShopID+"','"+this.UniqueID+"','"+this.PageID+"')");
        }
        public static PremiumItem LoadFromDatabase(DataRow row)
        {
            PremiumItem ppItem= new PremiumItem
            {
                UniqueID = GetDataTypes.GetInt(row["UniqueID"]),
                Slot = GetDataTypes.GetByte(row["PageID"]),
                ShopID = GetDataTypes.GetInt(row["ShopID"]),
                CharID = GetDataTypes.GetInt(row["CharID"]),
                PageID = GetDataTypes.GetByte(row["PageID"])
            };
            return ppItem;
        }

    }
}
