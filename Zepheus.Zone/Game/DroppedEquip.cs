
namespace Zepheus.Zone.Game
{
    public class DroppedEquip : DroppedItem
    {
        public ushort Dex { get; set; }
        public ushort Str { get; set; }
        public ushort End { get; set; }
        public ushort Int { get; set; }
        public ushort Spr { get; set; }
        public ushort Upgrades { get; set; }

        public DroppedEquip(Item pBase)
        {
            this.Amount = 1;
            //this.Expires = pBase.Expires;
            this.Dex = pBase.UpgradeStats.Dex;
            this.Str = pBase.UpgradeStats.Str;
            this.End = pBase.UpgradeStats.End;
            this.Int = pBase.UpgradeStats.Int;
            this.Upgrades = pBase.UpgradeStats.Upgrades;
            this.ItemID = pBase.ID;
        }
    }
}
