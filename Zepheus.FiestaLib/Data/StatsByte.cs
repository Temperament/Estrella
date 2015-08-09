namespace Zepheus.FiestaLib.Data
{
	public enum StatsByte : byte
	{
		MinMelee = 0x06,
		MaxMelee = 0x07,
		WDef = 0x08,

		Aim = 0x09,
		Evasion = 0x0a,

		MinMagic = 0x0b,
		MaxMagic = 0x0c,
		MDef = 0x0d,

		StrBonus = 0x13,
		EndBonus = 0x19
	}
}