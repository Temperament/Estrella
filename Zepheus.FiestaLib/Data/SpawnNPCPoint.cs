namespace Zepheus.FiestaLib.Data
{
	public sealed class SpawnNpcPoint
	{
		public ushort ID { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public byte Rotation { get; set; }
		public LinkTable Gate { get; set; }
	}
}