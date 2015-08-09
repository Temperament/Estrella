﻿/*File for this file Basic Copyright 2012 no0dl */
using System.Collections.Generic;

namespace Zepheus.Zone.Game.Buffs
{
	public class Buffs
    {
        private ZoneCharacter Character {get;set;}

        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int MinMagic { get; set; }
        public int MaxMagic { get; set; }
        public int WeaponDefense { get; set; }
        public int WeaponDamage { get; set; }
        public int MagicDefense { get; set; }
        public int MagicDamage { get; set; }
        public int Evasion { get; set; }
        public int Str { get; set; }
        public int End { get; set; }
        public int Dex { get; set; }
        public int Int { get; set; }
        public int Spr { get; set; }
        public int MaxHP { get; set; }
        public int MaxSP { get; set; }

        private List<Buff> CurrentBuffs { get; set; }

        public Buffs(ZoneCharacter pChar)
        {
            Character = pChar;
            CurrentBuffs = new List<Buff>();
        }
    }
}
