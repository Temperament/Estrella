using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zepheus.Database.DataStore;

namespace Zepheus.FiestaLib.Data
{
    public class AbStateInfo
    {
        public ushort ID { get; set; }
        public string InxName { get; set; }

        public static AbStateInfo LoadFromDatabase(DataRow row)
        {
            AbStateInfo info = new AbStateInfo
            {
                ID = GetDataTypes.GetUshort(row["ID"]),
                InxName = (string)row["InxName"],
            };
            return info;
        }
    }
}
