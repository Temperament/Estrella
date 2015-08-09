using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zepheus.Database.DataStore
{
    public  class GetDataTypes
    {
        public static int GetInt(object Row)
        {
            return Convert.ToInt32(Row);
        }
        public static uint GetUint(object Row)
        {            
            return Convert.ToUInt32(Row);
        }
        public static short Getshort(object Row)
        {
            return Convert.ToInt16(Row);
        }
        public static ushort GetUshort(object Row)
        {
            return Convert.ToUInt16(Row);
        }
        public static byte GetByte(object Row)
        {

            if (Convert.ToUInt32(Row) >= 255) return 255;
            else return Convert.ToByte(Row); 
        }
        public static sbyte GetSByte(object Row)
        {
            return Convert.ToSByte(Row);
        }
        public static long GetLong(object Row)
        {
            return Convert.ToInt64(Row);
        }
        public static ulong GetUlong(object Row)
        {
            return Convert.ToUInt64(Row);
        }
        public static bool GetBool(object Row)
        {
            return Convert.ToBoolean(Row);
        }
    }
}
