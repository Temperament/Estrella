using System.Data;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Zepheus.Database;

namespace Zepheus.Zone.Game
{
    public class RewardInventory
    {
        public  Dictionary<ushort,List<RewardItem>> RewardItems { get; set; }
        private Mutex locker = new Mutex();
        private ushort MaxPageCount { get; set; }
        public RewardInventory()
        {
            RewardItems = new Dictionary<ushort, List<RewardItem>>();
            MaxPageCount = 1;
            for (byte i = 0; i < this.MaxPageCount; ++i)
            {
                RewardItems[i] = new List<RewardItem>();
            }
        }
        public void LoadRewardItems(int pCharID)
        {
        
            try
            {
                locker.WaitOne();
                DataTable Rewarddata = null;
                using (DatabaseClient dbClient = Program.CharDBManager.GetClient())
                {
                    Rewarddata = dbClient.ReadDataTable("SELECT *FROM RewardItems WHERE CharID='" + pCharID + "'");
                }
                if (Rewarddata != null)
                {
                    foreach (DataRow row in Rewarddata.Rows)
                    {
                        RewardItem pItem = RewardItem.LoadFromDatabase(row);
                        if (!this.RewardItems.ContainsKey(pItem.PageID))
                        {
                            this.RewardItems[pItem.PageID] = new List<RewardItem>();
                        }
                        this.RewardItems[pItem.PageID].Add(pItem);
                    }
                }
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }
        public void RemoveRewardItem(RewardItem pItem)
        {
            try
            {
                locker.WaitOne();
                pItem.RemoveFromDatabase();
                this.RewardItems[pItem.PageID].Remove(pItem);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }
     
        public void AddRewardItem(RewardItem pItem)
        {
            try
            {
                locker.WaitOne();
                if (!this.RewardItems.ContainsKey(pItem.PageID))
                {
                    this.RewardItems[pItem.PageID] = new List<RewardItem>();
                
                }
                pItem.AddToDatabase();
                this.RewardItems[pItem.PageID].Add(pItem);
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }
        public void Enter()
        {
            locker.WaitOne();
        }

        public bool GetEmptySlot(out byte pSlot, out ushort PageID) //cpu intensive?
        {
            pSlot = 0;
            PageID = 0;
            for (byte i = 0; i < this.RewardItems.Count; ++i)
            {
                    for (byte i2 = 0; i2 < 24; ++i2)
                    {
                        RewardItem Item = this.RewardItems[i].Find(ss => ss.Slot == i2);
                        if (Item == null)
                        {
                            pSlot = i2;
                            PageID = i;
                            return true;
                        }
                    }
            }
            return false; //no more empty slots found
        }
        public void Release()
        {
            try
            {
                locker.ReleaseMutex();
            }
            catch { }
        }

    }
}
