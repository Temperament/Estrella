using System;
using Zepheus.Zone.Game;

namespace Zepheus.Zone.Data
{
   public class TradeItem
    {
       public ZoneCharacter Owner { get; set; }
       public byte InventorySlot { get; set; }
       public byte TradeSlot { get; set; }
       public Item Item { get; set; }
      public  TradeItem(ZoneCharacter owner,byte InventorySlot,byte Tradeslot,Item pItem)
      {
          this.Owner = owner;
          this.Item = pItem;
          this.InventorySlot = InventorySlot;
          this.TradeSlot = Tradeslot;
      }
    }

}
