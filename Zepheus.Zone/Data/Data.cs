using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zepheus.Zone.Data
{
    public enum NpcFlags : ushort
    {
        Normal = 0,
        Vendor = 1,
        Teleporter = 2
    }
    public enum ItemFlags : byte
    {
        Normal = 0,
        GuildItem = 1,
    }
    public enum GuildStoreAddFlags : byte
    {
        Equip = 0,
        Item = 1,
        Gold = 2,
    }
}
