using System;
using Zepheus.Database.DataStore;
using System.Collections.Generic;
using System.Data;

namespace Zepheus.FiestaLib.Data
{
    public class MasterRewardState
    {
        public ushort ItemID { get; set; }
        public byte Upgrades { get; set; }
        public byte Count { get; set; }

        public ushort Str { get; private set; }
        public ushort End { get; private set; }
        public ushort Dex { get; private set; }
        public ushort Int { get; private set; }
        public ushort Spr { get; private set; }
        
        public MasterRewardState()
        {
        }
        public MasterRewardState(DataRow row)
        {
            this.Str = GetDataTypes.GetUshort(row["Str"]);
            this.End = GetDataTypes.GetUshort(row["End"]);
            this.Dex = GetDataTypes.GetUshort(row["Dex"]);
            this.Int = GetDataTypes.GetUshort(row["Int"]);
            this.Spr = GetDataTypes.GetUshort(row["Spr"]);
            this.ItemID = GetDataTypes.GetUshort(row["ItemID"]);
        }
    }
}
