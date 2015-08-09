using System;
using Zepheus.InterLib;
using Zepheus.InterLib.Networking;
using Zepheus.World.Data;
using Zepheus.FiestaLib.Data;

namespace Zepheus.World.Managers
{
    public static class BuffManager
    {
        public static void SetBuff(AbStateInfo AbState, uint Strength, uint KeepTime, params WorldCharacter[] Receiver)
        {
            using (var packet = new InterPacket(InterHeader.ZONE_CharacterSetBuff))
            {
                packet.WriteUShort(AbState.ID);
                packet.WriteUInt(Strength);
                packet.WriteUInt(KeepTime);

                packet.WriteInt(Receiver.Length);
                Array.ForEach(Receiver, ch => packet.WriteInt(ch.ID));



                ZoneManager.Instance.Broadcast(packet);
            }
        }
        public static void RemoveBuff(AbStateInfo AbState, params WorldCharacter[] Receiver)
        {
            using (var packet = new InterPacket(InterHeader.ZONE_CharacterRemoveBuff))
            {
                packet.WriteUShort(AbState.ID);

                packet.WriteInt(Receiver.Length);
                Array.ForEach(Receiver, ch => packet.WriteInt(ch.ID));


                ZoneManager.Instance.Broadcast(packet);
            }
        }
    }
}