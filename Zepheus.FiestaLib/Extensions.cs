using System;
using System.Globalization;
using MySql.Data.Types;

namespace Zepheus.FiestaLib
{
    public static class Extensions
    {
        public static uint ToFiestaTime(this DateTime pValue)
        {
            // Copyright Diamondo25 & CSharp
            uint val = 0;
            val |= (uint)(pValue.Minute << 25);
            val |= (uint)((pValue.Hour & 0x3F) << 19);
            val |= (uint)((pValue.Day & 0x3F) << 13);
            val |= (uint)((pValue.Month & 0x1F) << 8);
            val |= (byte)(pValue.Year - 2000);
            return val;
        }
        public static byte ToFiestaMonth(this DateTime pValue)
        {
            return (byte)(pValue.Month << 4);  
        }
        public static byte ToFiestaYear(this DateTime pValue)
        {
            int year = pValue.Year  - 1900;
            return (byte)year;
        }
        public static string ToDBString(this DateTime Datetime)
        {
            return Datetime.ToString("yyyy/MM/dd HH/mm/ss");
        }

    }
}
