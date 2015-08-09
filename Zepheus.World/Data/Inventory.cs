using System;
using System.Data;
using System.Collections.Generic;
using System.Threading;
using Zepheus.Database;

namespace Zepheus.World.Data
{
    public sealed class Inventory
    {
        public List<Equip> EquippedItems { get; private set; }
        private Mutex locker = new Mutex();

        public Inventory()
        {
            EquippedItems = new List<Equip>();
        }
        public void Enter()
        {
            locker.WaitOne();
        }

        public void Release()
        {
            locker.ReleaseMutex();
        }
        public void AddToEquipped(Equip pEquip)
        {
            try
            {
                locker.WaitOne();
                EquippedItems.Add(pEquip);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }
        public void LoadBasic(WorldCharacter pChar)
        {
            try
            {
                locker.WaitOne();
                DataTable equips = null;
                using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
                {
                    equips = dbClient.ReadDataTable("SELECT * FROM equips WHERE Owner=" + pChar.ID + " AND Slot < 0");
                }
                if (equips != null)
                {
                    foreach (DataRow row in equips.Rows)
                    {
                        Equip loaded = Equip.LoadEquip(row);
                        EquippedItems.Add(loaded);
                    }
                }
            }
            finally
            {
                locker.ReleaseMutex();
            }

        }
    }

}
