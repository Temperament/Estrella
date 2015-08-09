using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zepheus.Zone.Game;
using Zepheus.FiestaLib.Data;

namespace Zepheus.Zone.Data
{
    public class BaseStats
    {
        public static int GetStatValue(ZoneCharacter pCharacter, StatsByte pByte)
        {
            switch (pByte)
            {
                case StatsByte.MinMelee:
                    return pCharacter.MinDamage;
                case StatsByte.MaxMelee:
                    return pCharacter.MaxDamage;
                case StatsByte.MinMagic:
                    return pCharacter.MinMagic;
                case StatsByte.MaxMagic:
                    return pCharacter.MaxMagic;
                case StatsByte.WDef:
                    return pCharacter.WeaponDef;
                case StatsByte.MDef:
                    return pCharacter.MagicDef;
                case StatsByte.Aim:
                    return 5; //TODO load additional equip stats
                case StatsByte.Evasion:
                    return 5;
                case StatsByte.StrBonus:
                    return pCharacter.StrBonus;
                case StatsByte.EndBonus:
                    return pCharacter.EndBonus;
                default:
                    return 0;
            }
        }
    }
}
