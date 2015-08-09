using System;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.Zone.Data;
using Zepheus.Zone.Handlers;

namespace Zepheus.Zone.Game
{
    class Mob : MapObject
    {
        public ushort ID { get; set; }
        public byte Level { get; set; }
        public bool Moving { get; set; }

        public MapObject Target { get; set; }
        public const int MinMovement = 60;
        public const int MaxMovement = 180;
        private MobBreedLocation spawnplace;
        private bool deathTriggered;
        private bool DropState { get; set; }
        public MobInfo Info { get; private set; }

        public MobInfoServer InfoServer { get; private set; }
        public override uint MaxSP { get { if (InfoServer == null) return 100; return InfoServer.MaxSP; } set { return; } }
        public override uint MaxHP { get { if (Info == null) return 100; return Info.MaxHP; } set { return; } }
        private DateTime nextUpdate;
        private Vector2 boundryLT;
        private Vector2 boundryRB;

        public AttackSequence AttackingSequence { get; private set; }

        public Mob(MobBreedLocation mbl)
        {
            ID = mbl.MobID;
        
            Init();

            // Make random location
            if (!mbl.Map.AssignObjectID(this))
            {
                Log.WriteLine(LogLevel.Warn, "Couldn't spawn mob, out of ID's");
                return;
            }
            Map = mbl.Map;
            spawnplace = mbl;
           
            while (true)
            {
                Position = Vector2.GetRandomSpotAround(Program.Randomizer, mbl.Position, 30);
                if (Map.Block != null)
                {
                    if (Map.Block.CanWalk(Position.X, Position.Y))
                    {
                        break;
                    }
                    
                }
                else
                {
                    Map = mbl.Map;
                 // Map.Block =
                    spawnplace = mbl;
                    break;
                }
            }
            SetBoundriesFromPointAndRange(Position, 100);

            spawnplace.CurrentMobs++;
        }

        public Mob(ushort pID, Vector2 pos)
        {
            ID = pID;
            Position = pos;
            Init();
      
            SetBoundriesFromPointAndRange(pos, 700);
        }

        private void Init()
        {

            Info = DataProvider.Instance.GetMobInfo(ID);
            MobInfoServer temp;
            DataProvider.Instance.MobData.TryGetValue(Info.Name, out temp);
            InfoServer = temp;
            Moving = false;
            Target = null;
            spawnplace = null;
           
            nextUpdate = Program.CurrentTime;
            deathTriggered = false;

            HP = MaxHP;
            SP = MaxSP;
            Level = Info.Level;

        }

        private bool PositionIsInBoundries(Vector2 pos)
        {
            return !(pos.X < boundryLT.X || pos.X > boundryRB.X || pos.Y < boundryLT.Y || pos.Y > boundryRB.Y);
        }

        private void SetBoundriesFromPointAndRange(Vector2 startpos, int range)
        {
            boundryLT = new Vector2(startpos.X - range, startpos.Y - range);
            boundryRB = new Vector2(startpos.X + range, startpos.Y + range);
        }
        public void DropItem(Item Item)
        {

            Drop mDrop = new Drop(Item, this, this.Position.X, this.Position.Y, 300);

            this.Map.AddDrop(mDrop);

        }

        public void Die()
        {
            HP = 0;

            if (!DropState)
            {
                DropState = true;
                new RandomDrop(this);
                //:TODO mindroplevel && maxdroplevel
            }
            Moving = false;
            boundryLT = null;
            boundryRB = null;
            AttackingSequence = null;
            Target = null;
            deathTriggered = true;

            if (spawnplace != null)
            {
                spawnplace.CurrentMobs--;
            }
            nextUpdate = Program.CurrentTime.AddSeconds(3);
        }


        public override void Attack(MapObject victim)
        {
            base.Attack(victim); // lol

            if (AttackingSequence != null) return;
            AttackingSequence = new AttackSequence(this, victim, 0, InfoServer.Str, 1400);
            Target = victim;
        }

        public override void AttackSkill(ushort skillid, MapObject victim)
        {
            base.AttackSkill(skillid, victim); // lol

            if (AttackingSequence != null) return;
            AttackingSequence = new AttackSequence(this, victim, 0, InfoServer.Str, skillid, true);
            Target = victim;
        }

        public override void AttackSkillAoE(ushort skillid, uint x, uint y)
        {
            base.AttackSkillAoE(skillid, x, y); // lol

            if (AttackingSequence != null) return;
            AttackingSequence = new AttackSequence(this, 0, InfoServer.Str, skillid, x, y);
        }

        public override Packet Spawn()
        {
            Packet packet = new Packet(SH7Type.SpawnSingleObject);
            Write(packet);
            return packet;
        }

        public void Write(Packet packet)
        {
            packet.WriteUShort(this.MapObjectID);
            packet.WriteByte(2);
            packet.WriteUShort(ID);
            packet.WriteInt(this.Position.X);
            packet.WriteInt(this.Position.Y);
            packet.WriteByte(this.Rotation);
            packet.Fill(55, 0);
        }

        public void WriteUpdateStats(Packet packet)
        {
            packet.WriteUInt(HP);
            packet.WriteUInt(MaxHP); // Max HP
            packet.WriteUInt(SP);
            packet.WriteUInt(MaxSP); // Max SP
            packet.WriteByte(Level);
            packet.WriteUShort(this.UpdateCounter);
        }

        public override void Update(DateTime date)
        {
            if (Position == null)
            {
                return;
            }

            if (IsDead)
            {
                if (!deathTriggered)
                {
                    Die();
                    return; // Wait till 3 seconds are over, then remove
                }
                else if (nextUpdate <= date)
                {
                    Map.RemoveObject(this.MapObjectID);
                    Position = null;
                    return;
                }
                return;
            }

            if (AttackingSequence != null && Target != null)
            {
                if (Vector2.Distance(Target.Position, Position) < 50)
                {
                    AttackingSequence.Update(date);
                    if (AttackingSequence.State == AttackSequence.AnimationState.Ended)
                    {
                        AttackingSequence = null;
                        Target = null;

                    }
                }
                else
                {
                    nextUpdate = nextUpdate.AddDays(-1);
                }
            }

            if (nextUpdate > date) return;


            if (Target != null)
            {
                nextUpdate = Program.CurrentTime.AddSeconds(1);

                // Try to move to target's pos
                // Might glitch the fuck out. lol
                if (Target.Map != Map)
                {
                    Target = null; // Stop aggro-ing >:(
                }
                else
                {
                    if (Vector2.Distance(Target.Position, Position) < 800)
                    {
                        if (Map.Block.CanWalk(Target.Position.X, Target.Position.Y))
                        {
                            Move(Position.X, Position.Y, Target.Position.X, Target.Position.Y, false, false);
                        }
                    }
                    else
                    {
                        Target = null; // Stop aggro-ing >:(
                    }
                }
                return;
            }
            else
            {
                nextUpdate = Program.CurrentTime.AddSeconds(Program.Randomizer.Next(10, 60)); // Around 10 seconds to 1 minute before new movement is made

                // Move to random spot.
                Vector2 newpos = new Vector2(Position);
                bool ok = false;
                for (int i = 1; i <= 20; i++)
                {
                    // Generate new position, and check if it's in valid bounds, else recheck
                    
                        newpos = Vector2.GetRandomSpotAround(Program.Randomizer, newpos, 60);
                        if (newpos.X > 0 && newpos.Y > 0 && Map.Block.CanWalk(newpos.X, newpos.Y) && PositionIsInBoundries(newpos))
                        {
                            ok = true;
                            break;
                        }
                    
                    /*
                    int t = Program.Randomizer.Next() % 11;

                    if (t <= 2)
                    {
                        // All +

                        newx += Program.Randomizer.Next(MinMovement, MaxMovement);
                        newy += Program.Randomizer.Next(MinMovement, MaxMovement);
                    }
                    else if (t <= 5)
                    {
                        newx -= Program.Randomizer.Next(MinMovement, MaxMovement);
                        newy += Program.Randomizer.Next(MinMovement, MaxMovement);
                    }
                    else if (t <= 8)
                    {
                        newx += Program.Randomizer.Next(MinMovement, MaxMovement);
                        newy -= Program.Randomizer.Next(MinMovement, MaxMovement);
                    }
                    else
                    {
                        newx -= Program.Randomizer.Next(MinMovement, MaxMovement);
                        newy -= Program.Randomizer.Next(MinMovement, MaxMovement);
                    }
                    Vector2 test = newpos + new Vector2(newx, newy);
                    if (Map.Block.CanWalk(test.X, test.Y) && PositionIsInBoundries(test))
                    {
                        newpos = test;
                        break;
                    }
                    */
                }

                if (ok)
                {
                    Move(Position.X, Position.Y, newpos.X, newpos.Y, false, false);
                }
            }
        }

        public void Move(int oldx, int oldy, int newx, int newy, bool walk, bool stop)
        {
            Position.X = newx;
            Position.Y = newy;
            Sector movedin = Map.GetSectorByPos(Position);
            if (movedin != MapSector)
            {
                MapSector.Transfer(this, movedin);
            }

            if (stop)
            {
                using (var packet = Handler8.StopObject(this))
                {
                    Map.Broadcast(packet);
                }
            }
            else
            {
                ushort speed = 0;
                if (walk) speed = 60;
                else speed = 115;

                using (var packet = Handler8.MoveObject(this, oldx, oldy, walk, speed))
                {
                    Map.Broadcast(packet);
                }
            }
        }
    }
}
