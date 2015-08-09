using System;
using System.Data;
using Zepheus.Database.DataStore;

namespace Zepheus.FiestaLib.Data
{
    public sealed class MobInfoServer
    {
        public UInt32 ID { get; private set; }
        public String InxName { get; private set; }
        public Byte Visible { get; private set; }
        public UInt16 AC { get; private set; }
        public UInt16 TB { get; private set; }
        public UInt16 MR { get; private set; }
        public UInt16 MB { get; private set; }
        public UInt32 EnemyDetectType { get; private set; }
        public UInt32 MobKillInx { get; private set; }
        public UInt32 MonExp { get; private set; }
        public UInt16 ExpRange { get; private set; }
        public UInt16 DetectCha { get; private set; }
        public Byte ResetInterval { get; private set; }
        public UInt16 CutInterval { get; private set; }
        public UInt32 CutNonAT { get; private set; }
        public UInt32 FollowCha { get; private set; }
        public UInt16 PceHPRcvDly { get; private set; }
        public UInt16 PceHPRcv { get; private set; }
        public UInt16 AtkHPRcvDly { get; private set; }
        public UInt16 AtkHPRcv { get; private set; }
        public UInt16 Str { get; private set; }
        public UInt16 Dex { get; private set; }
        public UInt16 Con { get; private set; }
        public UInt16 Int { get; private set; }
        public UInt16 Men { get; private set; }
        public UInt32 MobRaceType { get; private set; }
        public Byte Rank { get; private set; }
        public UInt32 FamilyArea { get; private set; }
        public UInt32 FamilyRescArea { get; private set; }
        public Byte FamilyRescCount { get; private set; }
        public UInt16 BloodingResi { get; private set; }
        public UInt16 StunResi { get; private set; }
        public UInt16 MoveSpeedResi { get; private set; }
        public UInt16 FearResi { get; private set; }
        public String ResIndex { get; private set; }
        public UInt16 KQKillPoint { get; private set; }
        public Byte Return2Regen { get; private set; }
        public Byte IsRoaming { get; private set; }
        public Byte RoamingNumber { get; private set; }
        public UInt16 RoamingDistance { get; private set; }
        public UInt16 MaxSP { get; private set; }
        public Byte BroadAtDead { get; private set; }
        public UInt16 TurnSpeed { get; private set; }
        public UInt16 WalkChase { get; private set; }
        public Byte AllCanLoot { get; private set; }
        public UInt16 DmgByHealMin { get; private set; }
        public UInt16 DmgByHealMax { get; private set; }

        public static MobInfoServer Load(DataRow row)
        {
            MobInfoServer info = new MobInfoServer
            {
                ID = GetDataTypes.GetUint(row["ID"]),
                InxName = (string)row["InxName"],
                Visible = GetDataTypes.GetByte(row["Visible"]),
                AC = GetDataTypes.GetUshort(row["AC"]),
                TB = GetDataTypes.GetUshort(row["TB"]),
                MR = GetDataTypes.GetUshort(row["MR"]),
                MB = GetDataTypes.GetUshort(row["MB"]),
                EnemyDetectType = GetDataTypes.GetUint(row["EnemyDetectType"]),
                MobKillInx = GetDataTypes.GetUint(row["MobKillInx"]),
                MonExp = GetDataTypes.GetUint(row["MonEXP"]),
                ExpRange = GetDataTypes.GetUshort(row["EXPRange"]),
                DetectCha = GetDataTypes.GetUshort(row["DetectCha"]),
                ResetInterval =GetDataTypes.GetByte(row["ResetInterval"]),
                CutInterval = GetDataTypes.GetUshort(row["CutInterval"]),
                CutNonAT = GetDataTypes.GetUint(row["CutNonAT"]),
                FollowCha = GetDataTypes.GetUint(row["FollowCha"]),
                PceHPRcvDly = GetDataTypes.GetUshort(row["PceHPRcvDly"]),
                PceHPRcv = GetDataTypes.GetUshort(row["PceHPRcv"]),
                AtkHPRcvDly = GetDataTypes.GetUshort(row["AtkHPRcvDly"]),
                AtkHPRcv = GetDataTypes.GetUshort(row["AtkHPRcv"]),
                Str = GetDataTypes.GetUshort(row["Str"]),
                Dex = GetDataTypes.GetUshort(row["Dex"]),
                Con = GetDataTypes.GetUshort(row["Con"]),
                Int = GetDataTypes.GetUshort(row["Int"]),
                Men = GetDataTypes.GetUshort(row["Men"]),
                MobRaceType = GetDataTypes.GetUint(row["MobRaceType"]),
                Rank = GetDataTypes.GetByte(row["Rank"]),
                FamilyArea = GetDataTypes.GetUint(row["FamilyArea"]),
                FamilyRescArea = GetDataTypes.GetUint(row["FamilyRescArea"]),
                FamilyRescCount = GetDataTypes.GetByte(row["FamilyRescCount"]),
                BloodingResi = GetDataTypes.GetUshort(row["BloodingResi"]),
                StunResi = GetDataTypes.GetUshort(row["StunResi"]),
                MoveSpeedResi = GetDataTypes.GetUshort(row["MoveSpeedResi"]),
                FearResi = GetDataTypes.GetUshort(row["FearResi"]),
                ResIndex = (string)row["ResIndex"],
                KQKillPoint = GetDataTypes.GetUshort(row["KQKillPoint"]),
                Return2Regen = GetDataTypes.GetByte(row["Return2Regen"]),
                IsRoaming = GetDataTypes.GetByte(row["IsRoaming"]),
                RoamingNumber = GetDataTypes.GetByte(row["RoamingNumber"]),
                RoamingDistance = GetDataTypes.GetUshort(row["RoamingDistance"]),
                MaxSP = GetDataTypes.GetUshort(row["MaxSP"]),
                BroadAtDead = GetDataTypes.GetByte(row["BroadAtDead"]),
                TurnSpeed = GetDataTypes.GetUshort(row["TurnSpeed"]),
                WalkChase = GetDataTypes.GetUshort(row["WalkChase"]),
                AllCanLoot = GetDataTypes.GetByte(row["AllCanLoot"]),
                DmgByHealMin =GetDataTypes.GetUshort(row["DmgByHealMin"]),
                DmgByHealMax = GetDataTypes.GetUshort(row["DmgByHealMax"]),
            };
            return info;
        }
    }
}
