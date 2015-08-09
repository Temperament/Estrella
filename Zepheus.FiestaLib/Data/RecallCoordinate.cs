using System;
using Zepheus.Util;
using System.Data;

namespace Zepheus.FiestaLib.Data
{
    public sealed class RecallCoordinate
    {
        public String ItemIndex { get; private set; }
        public String MapName { get; private set; }
        public Int16 LinkX { get; private set; }
        public Int16 LinkY { get; private set; }

        public static RecallCoordinate Load(DataRow row)
        {
            RecallCoordinate info = new RecallCoordinate
            {
                ItemIndex = row["ItemIndex"].ToString(),
                MapName = row["MapName"].ToString(),
                LinkX = Int16.Parse(row["LinkX"].ToString()),
                LinkY = Int16.Parse(row["LinkY"].ToString()),
            };
            return info;
        }
    }
}