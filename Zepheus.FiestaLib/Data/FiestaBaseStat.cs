using System;
using System.Data;

namespace Zepheus.FiestaLib.Data
{
    public sealed class FiestaBaseStat
    {
        public Job Job { get; private set; }
        public Int32 Level { get; private set; }
        public Int32 Strength { get; private set; }
        public Int32 Endurance { get; private set; }
        public Int32 Intelligence { get; private set; }
        public Int32 Dexterity { get; private set; }
        public Int32 Spirit { get; private set; }
        public Int32 SoulHP { get; private set; }
        public Int32 MaxSoulHP { get; private set; }
        public Int32 PriceHPStone { get; private set; }
        public Int32 SoulSP { get; private set; }
        public Int32 MaxSoulSP { get; private set; }
        public Int32 PriceSPStone { get; private set; }
        public Int32 AtkPerAP { get; private set; }
        public Int32 DmgPerAP { get; private set; }
        public Int32 MaxPwrStone { get; private set; }
        public Int32 NumPwrStone { get; private set; }
        public Int32 PricePwrStone { get; private set; }
        public Int32 PwrStoneWC { get; private set; }
        public Int32 PwrStoneMA { get; private set; }
        public Int32 MaxGrdStone { get; private set; }
        public Int32 NumGrdStone { get; private set; }
        public Int32 PriceGrdStone { get; private set; }
        public Int32 GrdStoneAC { get; private set; }
        public Int32 GrdStoneMR { get; private set; }
        public Int32 PainRes { get; private set; }
        public Int32 RestraintRes { get; private set; }
        public Int32 CurseRes { get; private set; }
        public Int32 ShockRes { get; private set; }
        public UInt32 MaxHP { get; private set; }
        public UInt32 MaxSP { get; private set; }
        public Int32 CharTitlePt { get; private set; }
        public Int32 SkillPwrPt { get; private set; }
        public Int32 HPStoneEffectID { get; private set; }
        public Int32 SPStoneEffectID { get; private set; }
        public static FiestaBaseStat Load(DataRow row, Job job)
        {
            FiestaBaseStat info = new FiestaBaseStat
            {
                Job = job,
                Level = (int)row["Level"],
                Strength = (int)row["Strength"],
                Endurance = (int)row["Constitution"],
                Intelligence = (int)row["Intelligence"],
                Dexterity = (int)row["Dexterity"],
                Spirit = (int)row["MentalPower"],
                SoulHP = (int)row["SoulHP"],
                MaxSoulHP = (int)row["MAXSoulHP"],
                PriceHPStone = (int)row["PriceHPStone"],
                SoulSP = (int)row["SoulSP"],
                MaxSoulSP = (int)row["MAXSoulSP"],
                PriceSPStone = (int)row["PriceSPStone"],
                AtkPerAP = (int)row["AtkPerAP"],
                DmgPerAP = (int)row["DmgPerAP"],
                MaxPwrStone = (int)row["MaxPwrStone"],
                NumPwrStone = (int)row["NumPwrStone"],
                PricePwrStone = (int)row["PricePwrStone"],
                PwrStoneWC = (int)row["PwrStoneWC"],
                PwrStoneMA = (int)row["PwrStoneMA"],
                MaxGrdStone = (int)row["MaxGrdStone"],
                NumGrdStone = (int)row["NumGrdStone"],
                PriceGrdStone = (int)row["PriceGrdStone"],
                GrdStoneAC = (int)row["GrdStoneAC"],
                GrdStoneMR = (int)row["GrdStoneMR"],
                PainRes = (int)row["PainRes"],
                RestraintRes = (int)row["RestraintRes"],
                CurseRes = (int)row["CurseRes"],
                ShockRes = (int)row["ShockRes"],
                MaxHP = (ushort)(int)row["MaxHP"],
                MaxSP = (ushort)(int)row["MaxSP"],
                CharTitlePt = (int)row["CharTitlePt"],
                SkillPwrPt = (int)row["SkillPwrPt"],
                SPStoneEffectID = (int)row["SPStoneEffectID"],
                HPStoneEffectID = (int)row["HPStoneEffectID"]

            };
           
            return info;
        }

    }
}
