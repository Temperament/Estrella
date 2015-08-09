using System;
using System.Collections.Generic;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.Zone.Handlers;

namespace Zepheus.Zone.Game
{
    public abstract class MapObject
    {
        #region .ctor
        public MapObject()
        {
            IsAttackable = true;
            SelectedBy = new List<ZoneCharacter>();
        }
        ~MapObject()
        {
            SelectedBy.Clear();
        }
        #endregion
        #region Properties
        public bool IsAdded { get; set; }
        public bool IsAttackable { get; set; }
        public bool IsDead { get { return HP == 0; } }


        public Map Map { get; set; }
        public Sector MapSector { get; set; }
        public Vector2 Position { get; set; }
        public byte Rotation { get; set; }
        public ushort MapObjectID { get; set; }

        public virtual uint HP { get; set; }
        public virtual uint MaxHP { get; set; }
        public virtual uint SP { get; set; }
        public virtual uint MaxSP { get; set; }

        public List<ZoneCharacter> SelectedBy { get; private set; }
        public ushort UpdateCounter { get { return ++statUpdateCounter; } }

        // HP/SP update counter thingy
        private ushort statUpdateCounter = 0;
        public static readonly TimeSpan HpSpUpdateInterval = TimeSpan.FromSeconds(3);
        protected DateTime lastHpSpUpdate = DateTime.Now; 
        #endregion
        #region Methods
        public virtual void Attack(MapObject victim)
        {
            if (victim != null && !victim.IsAttackable) return;
        }
        public virtual void AttackSkill(ushort skillid, MapObject victim)
        {
            if (victim != null && !victim.IsAttackable) return;
        }
        public virtual void AttackSkillAoE(ushort skillid, uint x, uint y)
        {
        }
        public virtual void Revive(bool totally = false)
        {
            if (totally)
            {
                HP = MaxHP;
                SP = MaxSP;
            }
            else
            {
                // Note - Why not take e.g. 10% of your MaxHp?
                // HP = MaxHP * 0.1;
                HP = 50;
            }
        }
        public virtual void Damage(MapObject bully, uint amount, bool isSP = false)
        {
            if (isSP)
            {
                if (SP < amount) SP = 0;
                else SP -= amount;
            }
            else
            {
                if (HP < amount) HP = 0;
                else HP -= amount;
            }

            if (bully == null)
            {
                if (this is ZoneCharacter)
                {
                    ZoneCharacter character = this as ZoneCharacter;
                    if (isSP)
                        Handler9.SendUpdateSP(character);
                    else
                        Handler9.SendUpdateHP(character);
                }
            }
            else
            {
                if (this is Mob && ((Mob)this).AttackingSequence == null)
                {
                    this.Attack(bully);
                }
                else if (this is ZoneCharacter && !((ZoneCharacter)this).IsAttacking)
                {
                    this.Attack(bully);
                }
            }
        }

        public abstract void Update(DateTime date);
        public abstract Packet Spawn();
        #endregion
        #region Event-Stuff
        // Event trigger
        protected virtual void OnHpSpChanged()
        {
            if (HpSpChanged != null)
            {
                HpSpChanged(this, new EventArgs());
            }
        }

        // Event-Variables
        public event EventHandler<EventArgs> HpSpChanged;
        #endregion
    }
}
