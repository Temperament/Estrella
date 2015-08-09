using System.Collections.Generic;
using Zepheus.FiestaLib.Data;
using Zepheus.FiestaLib;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Data;
using Zepheus.Database;
using Zepheus.Database.DataStore;
using Zepheus.Zone.Handlers;

namespace Zepheus.Zone.Game
{
    public class Inventory
    {
        public long Money { get; set; }
        public List<Item> EquippedItems { get; private set; }
        public Dictionary<byte, Item> InventoryItems { get; private set; }
        public byte InventoryCount { get; private set; }
        private Mutex locker = new Mutex();
        private ZoneCharacter InventoryOwner { get; set; }
        public  Inventory(ZoneCharacter pChar)
        {
            InventoryCount = 2;
            InventoryOwner = pChar;
            InventoryItems = new Dictionary<byte, Item>();
            EquippedItems = new List<Item>();
        }
        public Inventory()
        {
         
        }
        public void Enter()
        {
            locker.WaitOne();
        }

        public void Release()
        {
            try
            {
                locker.ReleaseMutex();
            }
            catch { }
        }

        public void LoadFull(ZoneCharacter pChar)
        {
            try
         
            {
                locker.WaitOne();
                DataTable items = null;
                  using (DatabaseClient dbClient = Program.CharDBManager.GetClient())
                {
                    items = dbClient.ReadDataTable("SELECT * FROM items WHERE Owner=" + pChar.ID + "");
                }
                  //we load all equippeditem

                  if (items != null)
                  {
                      foreach (DataRow row in items.Rows)
                      {
                          Item loaded = Item.LoadItem(row);
                          loaded.Owner = (uint)pChar.ID;
                          loaded.UpgradeStats = new UpgradeStats();
                          if(loaded.IsEquipped)
                          {
                              loaded.Slot = (sbyte)loaded.ItemInfo.Slot;
                              this.EquippedItems.Add(loaded);
                          }
                          else
                          {
                              this.InventoryItems.Add((byte)loaded.Slot, loaded);
                          }
                      }
                  }
                //we load inventory slots
                  if (items != null)
                  {
                      foreach (DataRow row in items.Rows)
                      {
                        Item loaded = Item.LoadItem(row);
                      /*  if (loaded.ItemInfo.Class == ItemClass.Rider)
                        {
                            Mount mount = Data.DataProvider.Instance.GetMountByItemID(loaded.ID);
                            if (mount != null)
                            {
                                loaded.Mount = mount;
                                loaded.Mount.Food = GetDataTypes.GetUshort(row["fuelcount"]);
                                loaded.Mount.ItemSlot = (byte)loaded.Slot;
                            }
                            this.AddToInventory(loaded);
                        }
                        else
                        {*/
                            this.AddToInventory(loaded);
                        //}
                    }
                }
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }
        public Item GetEquiptBySlot(byte slot, out Item Eq)
        {

            Eq = this.EquippedItems.Find(d => d.Slot == slot);
            return Eq;
        }
        public void RemoveInventory(Item pItem)
        {
            try
            {
                locker.WaitOne();
                Handler12.ModifyInventorySlot(InventoryOwner, 0x24, (byte)pItem.Slot, 0, null);
                pItem.Delete();
                this.InventoryItems.Remove((byte)pItem.Slot);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }
        public void AddToInventory(Item pItem)
        {
            try
            {
                locker.WaitOne();
                if (this.InventoryItems.ContainsKey((byte)pItem.Slot))
                {
                    this.InventoryItems[(byte)pItem.Slot].Delete(); //removes from DB
                    this.InventoryItems.Remove((byte)pItem.Slot);
                }
                this.InventoryItems.Add((byte)pItem.Slot, pItem);
            } finally {
                locker.ReleaseMutex();
            }
        }
        public  void AddToEquipped(Item pEquip)
        {
            try
            {
                locker.WaitOne();
               Item old = EquippedItems.Find(equip => equip.Slot == pEquip.Slot);
                if (old != null)
                {
                    old.IsEquipped = false;
                    AddToInventory(old);
                    EquippedItems.Remove(old);
                }
                EquippedItems.Add(pEquip);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }
        public ushort GetEquippedBySlot(ItemSlot pSlot)
        {
            //double check if found
           Item equip = EquippedItems.Find(d => d.Slot == (sbyte)pSlot && d.IsEquipped);
            if (equip == null)
            {
                return 0xffff;
            }
            else
            {
                return equip.ID;
            }
        }
        public bool GetEmptySlot(out byte pSlot) //cpu intensive?
        {
            pSlot = 0;
            for (byte i = 0; i < (InventoryCount * 24); ++i)
            {
                if (!InventoryItems.ContainsKey(i))
                {
                    pSlot = i;
                    return true;
                }
            }
            return false; //no more empty slots found
        }
    }
}
