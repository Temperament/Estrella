namespace Zepheus.FiestaLib.Data
{
	public sealed class BaseStatsEntry
	{
		public byte Level { get; set; }
		public ushort Str { get; set; }
		public ushort End { get; set; }
		public ushort Dex { get; set; }
		public ushort Int { get; set; }
		public ushort Spr { get; set; }
		public ushort MaxHPStones { get; set; }
		public ushort MaxSPStones { get; set; }
		public ushort MaxHP { get; set; }
		public ushort MaxSP { get; set; }
	}
}