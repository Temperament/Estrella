using System;
using Zepheus.Database.DataStore;
using System.Collections.Generic;
using System.Data;

namespace Zepheus.FiestaLib.Data
{
    public class MasterRewardItem : MasterRewardState
    {
        public byte Level { get; private set; }
        public Job Job { get; private set; }

        public MasterRewardItem()
        {
        }
        public MasterRewardItem(DataRow row)
         {
             this.ItemID = GetDataTypes.GetUshort(row["ItemID"]);
             this.Level = GetDataTypes.GetByte(row["Level"]);
             this.Job = (FiestaLib.Job)GetDataTypes.GetByte(row["Job"]);
             this.Count = GetDataTypes.GetByte(row["Count"]);
         }
    }
}
