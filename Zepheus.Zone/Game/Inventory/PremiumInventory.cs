using System.Data;
using System.Collections.Generic;
using System.Threading;
using Zepheus.Database;

namespace Zepheus.Zone.Game
{
    public class PremiumInventory
    {
        public Dictionary<ushort,List<PremiumItem>> PremiumItems { get; private set; }
        public ushort Count { get; private set; }
        private Mutex locker = new Mutex();
        private ushort MaxPageCount { get; set; }
        public void LoadPremiumItems(int pChar)
        {
            try
            {
                this.locker.WaitOne();
                DataTable Premiumdata = null;
                using (DatabaseClient dbClient = Program.CharDBManager.GetClient())
                {
                    Premiumdata = dbClient.ReadDataTable("SELECT *FROM PremiumItem WHERE CharID='" + pChar + "'");
                }
                if (Premiumdata != null)
                {
                    foreach (DataRow row in Premiumdata.Rows)
                    {
                        PremiumItem pItem = PremiumItem.LoadFromDatabase(row);
                        this.PremiumItems[pItem.PageID].Add(pItem);
                    }
                }
            }
            finally
            {
                this.locker.ReleaseMutex();
            }
       }
        public PremiumInventory()
        {
            this.PremiumItems = new Dictionary<ushort, List<PremiumItem>>();
            this.MaxPageCount = 1;
            for (byte i = 0; i < this.MaxPageCount; ++i)
            {
               PremiumItems[i] = new List<PremiumItem>();
            }
        }
        public void RemovePremiumItem(PremiumItem pItem)
        {

            try
            {
                locker.WaitOne();
                this.PremiumItems[pItem.PageID].Remove(pItem);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }
        public void AddPremiumItem(PremiumItem pItem)
        {
            pItem.AddToDatabase();
            this.PremiumItems[pItem.PageID].Add(pItem);
        }
        public void Enter()
        {
            this.locker.WaitOne();
        }

        public bool GetEmptySlot(out byte pSlot,out ushort PageID) //cpu intensive?
        {
            pSlot = 0;
            PageID = 0;
            for (byte i = 0; i < this.Count; ++i)
            {
                if (!this.PremiumItems.ContainsKey(i))
                {
                    for (byte i2 = 0; i2 < (this.PremiumItems[i].Count * 24); ++i2)
                    {
                        PremiumItem Item = this.PremiumItems[i].Find(ss => ss.Slot == i2);
                        if (Item == null)
                        {
                            pSlot = i2;
                            PageID = i;
                            return true;
                        }
                    }
                }
            }
            return false; //no more empty slots found
        }
        public void Release()
        {
            try
            {
                this.locker.ReleaseMutex();
            }
            catch { }
        }
    }
}
