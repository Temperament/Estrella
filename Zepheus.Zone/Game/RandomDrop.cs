using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zepheus.FiestaLib.Data;

namespace Zepheus.Zone.Game
{
    class RandomDrop
    {
        private Random ran = new Random();
        private Mob Monster { get; set; }
        private byte dropcounter { get; set; }
        public RandomDrop(Mob mob)
        {
            this.dropcounter = 0;
            this.Monster = mob;
            GenerateDrop();
        }
        void GenerateDrop()
        {
            foreach (var DropInfo in Monster.Info.Drops)
            {
                float rate = (float)(this.ran.NextDouble() * this.Monster.Info.Drops.Count);
                float RandomRate = rate * 100.0f / this.Monster.Info.Drops.Count;
                    if (RandomRate < DropInfo.Rate)
                    {
                        if (dropcounter >= DropInfo.Group.MaxCount) return;
                        DropItems(DropInfo.Group.Items, DropInfo.Rate, DropInfo.Group.MinCount, DropInfo.Group.MaxCount);
                    }
                    else
                    {
                        this.dropcounter = 0;
                        return;
                    }
            }
        }
        void DropItems(List<ItemInfo> Items, float Rate, byte Mincount, byte MaxCount)
        {
            foreach (var litem in Items)
            {
                if (dropcounter >= MaxCount) return;
                int index = (int)(ran.NextDouble() * Items.Count);
                float rate = (float)(ran.NextDouble() * Items.Count);
                float RandomRate = rate * 100.0f / Items.Count;
                if (RandomRate < Rate)
                {
                    if (litem.Type != ItemType.Equip)
                    {
                        ushort Amount = (ushort)new Random().Next(1, 255);
                        Item DropItem = new Item(0,0, Items[index].ItemID,0, Amount);
                        DropItem.UpgradeStats = new UpgradeStats();
                        this.Monster.DropItem(DropItem);
                    }
                    else
                    {
                        Item DropEq = new Item(0,0, Items[index].ItemID, 0);
                        DropEq.UpgradeStats = new UpgradeStats();
                        this.Monster.DropItem(DropEq);
                    }
                    this.dropcounter++;
                }
                else
                {
                    this.dropcounter = 0;
                    return;
                }
            }
        }
    }
}
