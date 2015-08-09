using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zepheus.Database.DataStore;

namespace Zepheus.FiestaLib.Data
{
    public sealed class Mount
    {
        public byte MinLevel { get; set; }
        public ushort ItemID { get; set; }
        public long TickSpeed { get; set; }
        public ushort Handle { get; set; }
        public ushort speed { get; set; }
        public DateTime Tick { get; set; }
        public ushort Food { get; set; }
        public int CastTime { get; set; }
        public ushort Cooldown { get; set; }
        public byte ItemSlot { get; set; }
        public bool permanent { get; set; }
        public static Mount LoadMount(DataRow Data)
        {
            
            Mount Mouninf = new Mount
            {
                MinLevel = GetDataTypes.GetByte(Data["Level"]),
                ItemID = GetDataTypes.GetUshort(Data["ItemID"]),
                TickSpeed = GetDataTypes.GetInt(Data["Tickspeed"]),
                Handle = GetDataTypes.GetUshort(Data["Handle"]),
                Food = GetDataTypes.GetUshort(Data["Food"]),
                speed = GetDataTypes.GetUshort(Data["Speed"]),
                CastTime = GetDataTypes.GetInt(Data["CastTime"]),
                Cooldown = GetDataTypes.GetUshort(Data["Cooldown"]),
                permanent = GetDataTypes.GetBool(Data["permanent"]),
            };
            return Mouninf;
        }
    }
}
