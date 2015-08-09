using System;
using System.Collections.Generic;
using System.Data;
using Zepheus.Database.DataStore;

namespace Zepheus.FiestaLib.Data
{
    public sealed class MapInfo
    {
        public ushort ID { get; private set; }
        public string ShortName { get; private set; }
        public string FullName { get; private set; }
        public int RegenX { get; private set; }
        public int RegenY { get; private set; }
        public byte Kingdom { get; private set; }
        public ushort ViewRange { get; private set; }

        public List<ShineNpc> NPCs { get; set; }

        public MapInfo() { }
        public MapInfo(ushort id, string shortname, string fullname, int regenx, int regeny, byte kingdom, ushort viewrange)
        {
            this.ID = id;
            this.ShortName = shortname;
            this.FullName = fullname;
            this.RegenX = regenx;
            this.RegenY = regeny;
            this.Kingdom = kingdom;
            this.ViewRange = viewrange;
        }

        public static MapInfo Load(DataRow row)
        {
            MapInfo info = new MapInfo
            {
                ID = GetDataTypes.GetUshort(row["ID"]),
                ShortName = (string)row["MapName"],
                FullName = (string)row["Name"],
                RegenX = GetDataTypes.GetInt(row["RegenX"]),
                RegenY = GetDataTypes.GetInt(row["RegenY"]),
                Kingdom = GetDataTypes.GetByte(row["KingdomMap"]),
                ViewRange = GetDataTypes.GetUshort(row["Sight"]),
            };
            return info;
        }
    }
}
