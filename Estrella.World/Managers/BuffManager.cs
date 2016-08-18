using System;
using Estrella.InterLib;
using Estrella.InterLib.Networking;
using Estrella.World.Data;
using Estrella.FiestaLib.Data;

namespace Estrella.World.Managers
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