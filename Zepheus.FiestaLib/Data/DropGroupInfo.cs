using System.Collections.Generic;
using System.Data;

namespace Zepheus.FiestaLib.Data
{
    public sealed class DropGroupInfo
    {
        public string GroupID { get; private set; }
        public byte MinCount { get; private set; }
        public byte MaxCount { get; private set; }
        public List<ItemInfo> Items { get; private set; }

        public static DropGroupInfo Load(DataRow row)
        {
            DropGroupInfo info = new DropGroupInfo()
            {
                GroupID = (string)row["GroupID"],
                MinCount = (byte)row["MinCount"],
                MaxCount = (byte)row["MaxCount"],
                Items = new List<ItemInfo>()
            };
            return info;
        }
    }
}
