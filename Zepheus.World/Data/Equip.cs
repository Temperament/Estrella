using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;
using Zepheus.FiestaLib.Data;
using Zepheus.FiestaLib;
using Zepheus.Database;
using Zepheus.Database.DataStore;
using Zepheus.World;

namespace Zepheus.World.Data
{
   public class Equip
    {
        public byte Upgrades { get; set; }
        public byte StatCount { get; private set; }
        public bool IsEquipped { get; set; }
        public ushort Str { get; private set; }
        public ushort End { get; private set; }
        public ushort Dex { get; private set; }
        public ushort Int { get; private set; }
        public ushort Spr { get; private set; }
        public sbyte Slot { get; set; }
        public ushort EquipID { get; set; }
        public ulong UniqueID { get; set; }
        public uint Owner { get; set; }
        public Equip(uint pOwner, ushort pEquipID, sbyte pSlot)
        {
            this.EquipID = pEquipID;
            this.Owner = pOwner;
            if (pSlot < 0)
            {
                this.Slot = (sbyte)pSlot;
                this.IsEquipped = true;
            }
            else
            {
                this.Slot = pSlot;
            }
        }
        public static Equip LoadEquip(DataRow row)
        {
            ulong uniqueID = GetDataTypes.GetUlong(row["ID"]);
            uint owner = GetDataTypes.GetUint(row["Owner"]);
            ushort equipID = GetDataTypes.GetUshort(row["EquipID"]);
            sbyte slot = GetDataTypes.GetSByte(row["Slot"]);
            byte upgrade = GetDataTypes.GetByte(row["Upgrades"]);

            ushort strByte = GetDataTypes.GetUshort(row["iSTR"]);
            ushort endByte = GetDataTypes.GetUshort(row["iEND"]);
            ushort dexByte = GetDataTypes.GetUshort(row["iDEX"]);
            ushort sprByte = GetDataTypes.GetUshort(row["iSPR"]);
            ushort intByte = GetDataTypes.GetUshort(row["iINT"]);
            Equip equip = new Equip(owner, equipID, slot)
            {
                UniqueID = uniqueID,
                Upgrades = upgrade,
                Str = strByte,
                End = endByte,
                Dex = dexByte,
                Spr = sprByte,
                Int = intByte
            };
            return equip;
        }
    }
}
