using System.Data;

namespace Zepheus.Database.Storage
{
	public class PositionInfo
	{
		public int XPos { get; set; }
		public int YPos { get; set; }
		public ushort Map { get; set; }

		public void ReadFromDatabase(DataRow row)
		{
			this.Map = (ushort)row["Map"];
			this.XPos = int.Parse(row["XPos"].ToString());
			this.YPos = int.Parse(row["YPos"].ToString());
		}
	}
}