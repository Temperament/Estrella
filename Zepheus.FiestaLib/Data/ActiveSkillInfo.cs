using System.Data;
using Zepheus.Database.DataStore;

namespace Zepheus.FiestaLib.Data
{
    public sealed class ActiveSkillInfo
    {
        public ushort ID { get; private set; }
        public string Name { get; private set; }
        public byte Step { get; private set; }
        public string Required { get; private set; }
        public ushort SP { get; private set; }
        public ushort HP { get; private set; }
        public ushort Range { get; private set; }
        public uint CoolTime { get; private set; }
        public uint CastTime { get; private set; }
        public ushort SkillAniTime { get; set; }
        public uint MinDamage { get; private set; }
        public uint MaxDamage { get; private set; }
        public bool IsMagic { get; private set; }
        public byte DemandType { get; private set; }
        public byte MaxTargets { get; private set; }

        public static ActiveSkillInfo Load(DataRow row)
        {
            ActiveSkillInfo inf = new ActiveSkillInfo
            {
                           
                ID = GetDataTypes.GetUshort(row["ID"]),
                Name = (string)row["InxName"],
                Step = GetDataTypes.GetByte(row["Step"]),
                Required = (string)row["DemandSk"],
                SP = GetDataTypes.GetUshort(row["SP"]),
                HP = GetDataTypes.GetUshort(row["HP"]),
                Range = GetDataTypes.GetUshort(row["Range"]),
                CoolTime = GetDataTypes.GetUint(row["DlyTime"]),
                CastTime = GetDataTypes.GetUint(row["CastTime"]),
                DemandType = GetDataTypes.GetByte(row["DemandType"]),
                MaxTargets = GetDataTypes.GetByte(row["TargetNumber"]),
            };

            uint maxdamage =  GetDataTypes.GetUint(row["MaxWC"]);
            if (maxdamage == 0)
            {
                inf.IsMagic = true;
                inf.MinDamage =  GetDataTypes.GetUshort(row["MinMA"]);
                inf.MaxDamage =  GetDataTypes.GetUshort(row["MaxMA"]);
            }
            else
            {
                inf.MaxDamage = maxdamage;
                inf.MinDamage =  GetDataTypes.GetUint(row["MinWC"]);
            }
            return inf;
        }
    }
}
