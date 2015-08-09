namespace Zepheus.Database.Storage
{
	public class DatabaseSkill
	{
		public long ID { get; set; }
		public int Owner { get; set; }
		public short SkillID { get; set; }
		public short Upgrades { get; set; }
		public bool IsPassive { get; set; }
		public Character Character { get; set; }
	}
}