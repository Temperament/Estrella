﻿
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.Util;
using Estrella.Zone.Game;
using Estrella.Zone.Networking;

namespace Estrella.Zone.Handlers
{
    public sealed class Handler15
    {
        public static void SendQuestion(ZoneCharacter character, Question question, ushort range)
        {
            using (var packet = new Packet(SH15Type.Question))
            {
                packet.WriteString(question.Text, 129);
                packet.WriteUShort(character.MapObjectID);     // Obj id
                packet.WriteInt(character.Position.X);
                packet.WriteInt(character.Position.Y);
                packet.WriteUShort(range);        // Distance how far your allowed to run when the question window is closed by Client
                packet.WriteByte((byte)question.Answers.Count);
                for (byte i = 0; i < question.Answers.Count; ++i)
                {
                    packet.WriteByte(i);
                    packet.WriteString(question.Answers[i], 32);
                }
                character.Client.SendPacket(packet);
            }
        }

        [PacketHandler(CH15Type.AnswerQuestion)]
        public static void QuestionHandler(ZoneClient client, Packet packet)
        {
            byte answer;
            if (!packet.TryReadByte(out answer))
            {
                Log.WriteLine(LogLevel.Warn, "Received invalid question response.");
                return;
            }

            ZoneCharacter character = client.Character;
            if (character.Question == null)
                return;
            else if (character.Question.Answers.Count <= answer)
                return;

            character.Question.Function(character, answer);
            character.Question = null;
        }
    }
}
