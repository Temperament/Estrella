using System;
using Zepheus.InterLib;
using Zepheus.InterLib.Networking;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;

namespace Zepheus.World.Managers
{
    [ServerModule(InitializationStage.Clients)]
    public class ZoneManager
    {
        public static ZoneManager Instance { get; set; }

        [InitializerMethod]
        public static bool init()
        {
            Instance = new ZoneManager();
            return true;
        }
        public void Broadcast(InterPacket pPacket)
        {
            foreach (var zone in Program.Zones.Values)
            {
                zone.SendPacket(pPacket);
            }
        }
    }
}
