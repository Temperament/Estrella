using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zepheus.Zone.Game
{
    public class UpgradeStats
    {
        public byte Upgrades { get; set; }
        public ushort Str { get; set; }
        public ushort End { get; set; }
        public ushort Dex { get; set; }
        public ushort Int { get; set; }
        public ushort Spr { get; set; }
    }
}
