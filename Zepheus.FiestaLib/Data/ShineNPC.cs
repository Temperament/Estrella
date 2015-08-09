using System;
using System.Collections.Generic;
using System.Data;

namespace Zepheus.FiestaLib.Data
{
	public sealed class ShineNpc
	{
		public Int16 MobID { get; private set; }
		public String MobName { get; private set; }
		public String Map { get; private set; }
		public Int32 CoordX { get; private set; }
		public Int32 CoordY { get; private set; }
		public Int16 Direct { get; private set; }
		public Byte NpcMenu { get; private set; }
		public String Role { get; private set; }
		public String RoleArg0 { get; private set; }
		public ushort Flags { get; private set; }
		public List<Vendor> VendorItems { get; set; }

		public static ShineNpc Load(DataRow row)
		{
			ShineNpc info = new ShineNpc
			{
				MobID = (short)(Int32)row["MobID"],
				Flags = (ushort)row["Flags"],
				MobName = (string)row["MobName"],
				Map = (string)row["Map"],
				CoordX = (int)row["RegenX"],
				CoordY = (int)row["RegenY"],
				Direct = (short)(Int32)row["Direct"],
				NpcMenu = (byte)(SByte)row["NPCMenu"],
				Role = (string)row["Role"],
				RoleArg0 = (string)row["RoleArg0"],
			};
			if (info.Flags == 1)
			{
				info.VendorItems = new List<Vendor>();
			}
	  
			return info;
		}
	}
}
