using System;

namespace Zepheus.Zone.Game
{
	public class LevelUpEventArgs : EventArgs
	{
		public LevelUpEventArgs(int oldLevel, int newLevel, ushort mobId)
		{
			this.OldLevel = oldLevel;
			this.NewLevel = newLevel;
			this.MobId = mobId;
		}

		public int OldLevel { get; private set; }
		public int NewLevel { get; private set; }
		public ushort MobId { get; set; }
	}
}