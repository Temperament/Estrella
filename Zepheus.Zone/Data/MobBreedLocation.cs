using System;
using System.Xml.Serialization;

using Zepheus.Util;
using Zepheus.Zone.Game;

namespace Zepheus.Zone.Data
{
    public class MobBreedLocation
    {
        public ushort MobID { get; set; }
        public ushort MapID { get; set; }
        public short InstanceID { get; set; }
        public Vector2 Position { get; set; }
        public const byte MaxMobs = 10;
        public const byte MinMobs = 3;

        [XmlIgnore]
        public byte CurrentMobs { get; set; }
        
        [XmlIgnore]
        private Map map;

        [XmlIgnore]
        public Map Map { get { return map ?? (map = MapManager.Instance.GetMap(DataProvider.Instance.MapsByID[MapID], InstanceID)); } }

        [XmlIgnore]
        private DateTime nextUpdate;

        public MobBreedLocation()
        {
            CurrentMobs = 0;
            MobID = 0;
            MapID = 0;
            InstanceID = -1;
            Position = null;
            nextUpdate = Program.CurrentTime;
        }

        public static MobBreedLocation CreateLocationFromPlayer(ZoneCharacter pCharacter, ushort mobID)
        {
            MobBreedLocation mbl = new MobBreedLocation();
            mbl.MobID = mobID;
            mbl.MapID = pCharacter.MapID;
            mbl.InstanceID = pCharacter.Map.InstanceID;
            mbl.Position = new Vector2(pCharacter.Position);
            return mbl;
        }

        public void SpawnMob()
        {
            if (CurrentMobs == MaxMobs) return; // ...
            Mob mob = new Mob(this);
           
            Map.FinalizeAdd(mob);
        }

        public void Update(DateTime date)
        {
            if (Position == null) return;
            if (this.nextUpdate > date) return;

            if (CurrentMobs < MinMobs)
            {
                SpawnMob();
            }
            else if (CurrentMobs < MaxMobs && Program.Randomizer.Next() % 1000 == 1)
            {
                SpawnMob();
            }

            this.nextUpdate = date.AddSeconds(Program.Randomizer.Next(30, 120)); // Around 30 seconds to 2 minutes for next check.
        }
    }
}
