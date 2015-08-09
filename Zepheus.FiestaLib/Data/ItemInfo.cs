using System;
using System.Data;
using Zepheus.Database.DataStore;

namespace Zepheus.FiestaLib.Data
{
	public sealed class ItemInfo
	{
		public ushort ItemID { get; private set; }
		public ItemSlot Slot { get; private set; }
		public bool TwoHand { get; private set; }
		public string InxName { get; private set; }
		public int MaxLot { get; private set; }
		public ushort AttackSpeed { get; private set; }
		public byte Level { get; private set; }
		public ItemType Type { get; private set; }
		public ItemClass Class { get; private set; }
		public byte UpgradeLimit { get; private set; }
		public Job Jobs { get; private set; }
		public ushort MinMagic { get; private set; }
		public ushort MaxMagic { get; private set; }
		public ushort MinMelee { get; private set; }
		public ushort MaxMelee { get; private set; }
		public ushort WeaponDef { get; private set; }
		public ushort MagicDef { get; private set; }
		public long BuyPrice { get; private set; }
		public long SellPrice { get; private set; }
        public ItemStats Stats { get; set; }
		//item upgrade
		public ushort UpSucRation { get; private set; }
		public ushort UpResource { get; private set; }

		/// <summary>
		/// Needs serious fixing in the reader, as it throws invalid casts (files all use uint, but fuck those)
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static ItemInfo Load(DataRow row)
		{
			ItemInfo itemInfo = new ItemInfo
			{
				ItemID = GetDataTypes.GetUshort(row["id"]),
				Slot = (ItemSlot)GetDataTypes.GetByte(row["equip"]),
				InxName = (string)row["inxname"],
				MaxLot = GetDataTypes.GetInt(row["maxlot"]),
				AttackSpeed = GetDataTypes.GetUshort(row["atkspeed"]),
				Level = GetDataTypes.GetByte(row["demandlv"]),
				Type = (ItemType)GetDataTypes.GetByte(row["type"]),
				Class = (ItemClass)GetDataTypes.GetByte(row["class"]),
				UpgradeLimit = (byte)GetDataTypes.GetByte(row["uplimit"]),
				Jobs = UnpackWhoEquip(GetDataTypes.GetUint(row["whoequip"])),
				TwoHand = GetDataTypes.GetBool(row["TwoHand"]),
				MinMagic = GetDataTypes.GetUshort(row["minma"]),
				MaxMagic = GetDataTypes.GetUshort(row["maxma"]),
				MinMelee = GetDataTypes.GetUshort(row["minwc"]),
				MaxMelee = GetDataTypes.GetUshort(row["maxwc"]),
				WeaponDef = GetDataTypes.GetUshort(row["ac"]),
				MagicDef = GetDataTypes.GetUshort(row["mr"]),
				UpSucRation = GetDataTypes.GetUshort(row["UpSucRatio"]),
				UpResource = GetDataTypes.GetUshort(row["UpResource"]),
				SellPrice =  GetDataTypes.GetUint(row["SellPrice"]),
				BuyPrice = GetDataTypes.GetUint(row["BuyPrice"]),
              
			};
            itemInfo.Stats = new ItemStats();
			return itemInfo;
		}

	   // [Obsolete("Too slow / incorrect?")]
		private static Job UnpackWhoEquip(uint value)
		{
			Job job = Job.None;
		  //  string jobnames = "";
			for (int i = 0; i < 26; i++)
			{
				if ((value & (uint)Math.Pow(2, i)) != 0)
				{
					job |= (Job)i;
			//        jobnames += ((Job)i).ToString() + " ";
				}
			}
			return job;
		}
	}
}
