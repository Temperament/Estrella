
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.World.Networking;
using System;
namespace Zepheus.World.Handlers
{
    public sealed class Handler2
    {
        //this is incorrect, somehow?
        [PacketHandler(CH2Type.Pong)]
        public static void Pong(WorldClient client, Packet packet)
        {
            client.Pong = true;
        }
        public static void SendClientTime(WorldClient client, DateTime time)
        {
        
            using (var packet = new Packet(SH2Type.UpdateClientTime))
            {
                //pPacket.WriteUInt((59 << 25) | (23 << 19) | (31 << 13) | (12 << 8) | (254));
            // *                    Minutes   | Hours      | Days       | Months    | Years
                packet.WriteInt(3);
                packet.WriteInt(time.Minute);//minutes
                packet.WriteInt(time.Hour);//hourses
                packet.WriteInt(time.Day);
                packet.WriteInt(time.Month-1);
                packet.WriteInt((time.Year - 1900));
                packet.WriteInt((int)time.DayOfWeek);//wekday?
                packet.WriteInt(105);
                packet.WriteInt(2);
                
                packet.WriteByte(1); //GMT 0-130 positive 130 -254 negative
              //  packet.WriteLong(2012);
               // packet.WriteInt(4);//unk
                //packet.WriteInt(1);//unk
                //packet.WriteInt(3);//unk
                //packet.WriteInt(46);
              //  packet.Fill(3, 0);//unk
               // packet.WriteByte(2);
                client.SendPacket(packet);
            }
        }
        [PacketHandler(CH2Type.Unk1)]
        public static void Handunk1(WorldClient character, Packet packet)
        {
            using (var to = new Packet(SH2Type.Unk1))
            {
                DateTime now = DateTime.Now;
                to.WriteByte(Convert.ToByte(now.Hour));
                to.WriteByte(Convert.ToByte(now.Minute));
                to.WriteByte(Convert.ToByte(now.Second));
                character.SendPacket(to);
            }
        }
        public static void SendPing(WorldClient client)
        {
            using (var packet = new Packet(SH2Type.Ping))
            {
                client.SendPacket(packet);
            }
        }
    }
}
