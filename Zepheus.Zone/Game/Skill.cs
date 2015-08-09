
using Zepheus.FiestaLib.Data;
using Zepheus.FiestaLib.Networking;
using Zepheus.Zone.Data;
using Zepheus.Database.Storage;
using System;

namespace Zepheus.Zone.Game
{
	public class Skill
	{
		private readonly DatabaseSkill skill;
		public ushort ID
		{
			get
			{
				return (ushort)skill.SkillID;
			}
			set
			{
				skill.SkillID = (short)value;
			}
		}
		public short Upgrades { get { return skill.Upgrades; } set { skill.Upgrades = value; } }
		public bool IsPassive { get { return skill.IsPassive; } private set { skill.IsPassive = value; } }
		public ActiveSkillInfo Info { get { return DataProvider.Instance.ActiveSkillsByID[ID]; } }

		public Skill(DatabaseSkill skill)
		{
			this.skill = skill;
		}

		public Skill(ZoneCharacter c, ushort id)
		{
			DatabaseSkill db = new DatabaseSkill();
			db.Owner = c.ID;
			db.SkillID = (short)id;
			db.Upgrades = 0;
			db.IsPassive = false;
			db.Character = c.Character;
			Program.CharDBManager.GetClient().ExecuteQuery("INSERT INTO Skillist (ID,Owner,SkillID,Upgrades,IsPassive) VALUES ('" + c.Character.ID + "','" + db.SkillID + "','" + db.Upgrades + "','" + Convert.ToInt32(IsPassive) + "')");
			skill = db;
		}

		public void Write(Packet pPacket)
		{
			pPacket.WriteUShort(ID);
			pPacket.WriteInt(60000); // Cooldown
			//pPacket.WriteShort(Upgrades);
			pPacket.WriteUShort(GetUpgrades(4, 3, 2, 1));

			pPacket.WriteInt(9000);         // Skill exp???
		}

		public static ushort GetUpgrades(byte val1, byte val2, byte val3, byte val4)
		{
			int ret = 0;
			ret |= (val1 & 0xF);
			ret |= ((val2 & 0xF) << 4);
			ret |= ((val3 & 0xF) << 8);
			ret |= (val4 << 12);
			return (ushort)ret;
		}
	}
}
