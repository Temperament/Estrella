
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.World.Networking;
using System;

namespace Zepheus.World.Handlers
{
    public sealed class Handler42
    {
        [PacketHandler(CH42Type.AddToBlockList)]
        public static void AddBlock(WorldClient client, Packet packet)
        {
            string AddBlockname;
            if (packet.TryReadString(out AddBlockname, 16))
            {
                client.Character.BlocketUser.Add(AddBlockname);
                using (var pp = new Packet(SH42Type.AddToBlockList))
                {
                    pp.WriteUShort(7168);//unk
                    pp.WriteString(AddBlockname, 16);
                    pp.WriteUShort(0);//unk
                    client.SendPacket(pp);
                }
                Program.DatabaseManager.GetClient().ExecuteQuery("INSERT INTO BlockUser (CharID,BlockCharname) VALUES ('" + client.Character.ID + "','" + AddBlockname + "')");
            }
        }
        [PacketHandler(CH42Type.RemoveFromBlockList)]
        public static void RemoveFromBlockList(WorldClient client, Packet packet)
        {
            string removename;
            if (packet.TryReadString(out removename, 16))
            {
                if (client.Character.BlocketUser.Contains(removename))
                {
                    using (var pack = new Packet(SH42Type.RemoveFromBlockList))
                    {
                        pack.WriteUShort(7184);//unk
                        pack.WriteString(removename, 16);
                        client.SendPacket(pack);
                    }
                    Program.DatabaseManager.GetClient().ExecuteQuery("DELETE FROM BlockUser WHERE CharID = '" + client.Character.ID + "' AND BlockCharname= '" + removename + "'");
                    client.Character.BlocketUser.Remove(removename);
                }
            }
        }
        [PacketHandler(CH42Type.ClearBlockList)]
        public static void clearBlock(WorldClient client, Packet packet)
        {

            using (var pp = new Packet(SH42Type.ClearBlockList))
            {
                pp.WriteUShort(7200);//unk
                client.SendPacket(packet);
            }
            Program.DatabaseManager.GetClient().ExecuteQuery("DELETE FROM BlockUser WHERE CharID = '" + client.Character.ID + "'");
            client.Character.BlocketUser.Clear();
        }
    }
}
