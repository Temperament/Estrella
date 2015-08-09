using System.Collections.Generic;
using System.Data;
using Zepheus.Database.DataStore;

namespace Zepheus.FiestaLib.Data
{
    public sealed class MobInfo
    {
        public string Name { get; private set; }
        public ushort ID { get; private set; }
        public byte Level { get; private set; }
        public uint MaxHP { get; private set; }
        public ushort RunSpeed { get; private set; }
        public bool IsNpc { get; private set; }
        public bool IsAggro { get; private set; }
        public byte Type { get; private set; }
        public ushort Size { get; private set; }

        public List<DropInfo> Drops { get; private set; }

        public byte MinDropLevel { get; set; }
        public byte MaxDropLevel { get; set; }

        public static MobInfo Load(DataRow row)
        {
            MobInfo inf = new MobInfo
            {
                Name = (string)row["InxName"],
                ID = GetDataTypes.GetUshort(row["ID"]),
                Level = GetDataTypes.GetByte(row["Level"]),
                MaxHP = GetDataTypes.GetUint(row["MaxHP"]),
                RunSpeed = GetDataTypes.GetUshort(row["RunSpeed"]),
                IsNpc = GetDataTypes.GetBool(row["IsNPC"]),
                Size =  GetDataTypes.GetUshort(row["Size"]),
                Type = GetDataTypes.GetByte(row["Type"]),
                IsAggro = GetDataTypes.GetBool(row["IsPlayerSide"]),
                Drops = new List<DropInfo>()
            };
            return inf;
        }
    }
}
