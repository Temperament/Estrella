using System;
using Zepheus.Database.DataStore;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zepheus.FiestaLib.Data
{
    public class ItemStats
    {
        public ushort Str { get; set; }
        public ushort End { get; set; }
        public ushort Dex { get; set; }
        public ushort Int { get; set; }
        public ushort Spr { get; set; }

        public static ItemStats LoadItemStatsFromDatabase(DataRow row)
        {
            ItemStats Stats = new ItemStats
            {
                Dex = GetDataTypes.GetUshort(row["Dex"]),
                End = GetDataTypes.GetUshort(row["con"]),
                Int = GetDataTypes.GetUshort(row["Int"]),
                Str = GetDataTypes.GetUshort(row["Str"]),
            };
            return Stats;
        }
    }
}
