using System.Data;

namespace Zepheus.Database.Storage
{
	public class LookInfo
	{
		public byte Hair { get; set; }
		public byte HairColor { get; set; }
		public byte Face { get; set; }
		public bool Male { get; set; }

		public void ReadFromDatabase(DataRow row)
		{
			this.Male = DataStore.ReadMethods.EnumToBool(row["Male"].ToString());
			this.Hair = byte.Parse(row["Hair"].ToString());
			this.HairColor = byte.Parse(row["HairColor"].ToString());
			this.Face = byte.Parse(row["Face"].ToString());
		}
	}
}