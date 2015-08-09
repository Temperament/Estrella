using System.Data;
using Zepheus.Database.DataStore;

namespace Zepheus.FiestaLib.Data
{
    public sealed class MiniHouseInfo
    {
        public ushort ID { get; private set; }
        public ushort KeepTimeHour { get; private set; }
        public ushort HPTick { get; private set; }
        public ushort SPTick { get; private set; }
        public ushort HPRecovery { get; private set; }
        public ushort SPRecovery { get; private set; }

        // public int Slot { get; set; } // No idea, only 5 or 10
        // public string Name { get; set; } // Not needed for now
        // public ushort CastTime { get; set; } // Not needed for now

        public MiniHouseInfo(DataRow row)
        {
            ID = GetDataTypes.GetUshort(row["Handle"]);
            KeepTimeHour = GetDataTypes.GetUshort(row["KeepTime_Hour"]);
            HPTick = GetDataTypes.GetUshort(row["HPTick"]);
            SPTick = GetDataTypes.GetUshort(row["SPTick"]);
            HPRecovery = GetDataTypes.GetUshort(row["HPRecovery"]);
            SPRecovery =GetDataTypes.GetUshort(row["SPRecovery"]);
        }
    }
}
