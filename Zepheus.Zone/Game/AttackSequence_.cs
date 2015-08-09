using System;

using Zepheus.Zone.Handlers;

namespace Zepheus.Zone.Game
{
	public class AttackSequence_
	{
		public enum AnimationState
		{
			Starting,
			Running,
			Ended
		}
		public struct Attack
		{
			public ushort Damage;
			public bool Crit;
			public ushort MoveTime;
			public ushort Skill;

			public Attack(ushort dmg, bool crit, ushort movetime, ushort skill = (ushort) 0)
			{
				Damage = dmg;
				Crit = crit;
				MoveTime = movetime;
				Skill = skill;
			}
		}

		private static byte counter = 0;
		public static byte Counter { get { return counter++; } }

		public AnimationState State { get; set; }
		private readonly MapObject from;
		private MapObject to; // Could be a player too (PvP or EvP)
		private readonly ushort toID;
		private DateTime nextSequence;

		private readonly ushort attackSpeed;
		private byte stance = 0;
		private readonly ushort skillid = 0xFFFF;
		private bool IsSkill
		{
			get
			{
				return skillid == 0xFFFF;
			}
		}

		public AttackSequence_(MapObject from, MapObject to, ushort skill = (ushort) 0xFFFF, ushort attackspeed = (ushort) 1400)
		{
			this.from = from;
			this.to = to;
			toID = to.MapObjectID;
			State = AnimationState.Running;
			nextSequence = Program.CurrentTime;
			attackSpeed = attackspeed;
			skillid = skill;
		}

		private void SetNewSequenceTime(ushort msecs)
		{
			nextSequence = Program.CurrentTime.AddMilliseconds(msecs);
		}

		private uint GetHPLeft()
		{
			if (to == null || to.IsDead)
			{
				return 0;
			}
			else
			{
				return to.HP;
			}
		}

		private void Handle()
		{
			if (to != null)
			{
				ushort seed = (ushort)Program.Randomizer.Next(0, 100); //we use one seed & base damage on it

				ushort damage = (ushort)Program.Randomizer.Next(0, seed);
				bool crit = seed >= 80;
				stance = (byte)(Program.Randomizer.Next(0, 3));
				to.Damage(from, damage);
				Handler9.SendAttackAnimation(from, toID, attackSpeed, stance);
				Handler9.SendAttackDamage(from, toID, damage, crit, GetHPLeft(), to.UpdateCounter);

				if (to.IsDead)
				{
					if (to is Mob && from is ZoneCharacter)
					{
						uint exp = (to as Mob).InfoServer.MonExp;
						(from as ZoneCharacter).GiveExp(exp, toID);
					}
					Handler9.SendDieAnimation(from, toID);
					State = AnimationState.Ended;
					to = null;
				}
				else
				{
					SetNewSequenceTime(attackSpeed);
				}
			}
		}

		public void Update(DateTime time)
		{
			if ((to != null && to.IsDead) || (from != null && from.IsDead))
			{
				State = AnimationState.Ended;
			}

			if (State == AnimationState.Ended || nextSequence > time) return;

			if (State == AnimationState.Running)
			{
				Handle();
			}
		}
		
	}
}
