using System;
using System.Data;

namespace Zepheus.FiestaLib.Data
{
	public sealed class LinkTable
	{
		public String argument { get; private set; }
		public String MapServer { get; private set; }
		public String MapClient { get; private set; }
		public Int32 CoordX { get; private set; }
		public Int32 CoordY { get; private set; }
		public Int16 Direct { get; private set; }
		public Int16 LevelFrom { get; private set; }
		public Int16 LevelTo { get; private set; }
		public Byte Party { get; private set; }

		public static LinkTable Load(DataRow row)
		{
		
			LinkTable info = new LinkTable
			{
			   argument = (string)row["argument"],
				MapServer = (string)row["MapServer"],
				MapClient = (string)row["MapClient"],
				CoordX =(int)row["Coord_X"],
				CoordY = (int)row["Coord_Y"],
				Direct = (short)(Int32)row["Direct"],
				LevelFrom = (short)(Int32)row["LevelFrom"],
				LevelTo = (short)(Int32)row["LevelTo"],
				Party = (byte)(sbyte)row["Party"],
			};
			return info;
		}
	}
}
