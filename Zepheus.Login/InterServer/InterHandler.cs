using Zepheus.InterLib.Networking;
using Zepheus.Util;

namespace Zepheus.Login.InterServer
{
    public sealed class InterHandler
    {
        [InterPacketHandler(InterHeader.Assign)]
        public static void HandleServerAssignement(WorldConnection wc, InterPacket packet)
        {
            byte wid;
            string name, ip;
            ushort port;
            if (!packet.TryReadByte(out wid) || !packet.TryReadString(out name) || !packet.TryReadString(out ip) || !packet.TryReadUShort(out port))
            {
                Log.WriteLine(LogLevel.Error, "Could not read World ID in inter server packet.");
                wc.Disconnect();
                return;
            }

            if (WorldManager.Instance.Worlds.ContainsKey(wid))
            {
                Log.WriteLine(LogLevel.Error, "Already loaded this world?");
                wc.Disconnect();
                return;
            }

            wc.Name = name;
            wc.ID = wid;
            wc.IP = ip;
            wc.Port = port;
            wc.IsAWorld = true;

            if (WorldManager.Instance.Worlds.TryAdd(wc.ID, wc))
            {
                Log.WriteLine(LogLevel.Info, "Assigned world {0}!", wc.ID);
                SendAssigned(wc);
            }
            else
            {
                Log.WriteLine(LogLevel.Error, "Couldn't assign world {0}..", wc.ID);
            }
        }

        [InterPacketHandler(InterHeader.BanAccount)]
        public static void Banaccount(WorldConnection wc, InterPacket packet)
        {
            int acountid;
            if (packet.TryReadInt(out acountid))
            {
                Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE Accounts Set Blocked='1' WHERE ID="+acountid+"");
            }
        }
        public static void SendAssigned(WorldConnection wc)
        {
            using (var p = new InterPacket(InterHeader.Assigned))
            {
                wc.SendPacket(p);
            }
        }
    }
}
