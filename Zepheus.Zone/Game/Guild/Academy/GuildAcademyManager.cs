/*File for this file Basic Copyright 2012 no0dl */

using System;
using Zepheus.InterLib.Networking;
using Zepheus.Zone.Game;
using Zepheus.InterLib;
using Zepheus.Zone.Managers;
using Zepheus.Zone.InterServer;

namespace Zepheus.Zone.Game.Guilds.Academy
{
    public static class GuildAcademyManager
    {
        #region Internal Client Handlers
        [InterPacketHandler(InterHeader.ZONE_AcademyMemberJoined)]
        public static void On_WorldClient_AcademyMemberJoined(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            DateTime registerDate;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID)
                || !pPacket.TryReadDateTime(out registerDate))
            {
                return;
            }



            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                var member = new GuildAcademyMember(guild.Academy, characterID, GuildAcademyRank.Member, registerDate)
                {
                    IsOnline = true,
                };
                guild.Academy.Members.Add(member);


                ZoneCharacter character;
                if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                {
                    member.Character = character;
                    
                    character.Guild = guild;
                    character.GuildAcademy = guild.Academy;
                    character.GuildAcademyMember = member;


                    GuildManager.SetGuildBuff(character);
                }
            }
        }

        [InterPacketHandler(InterHeader.ZONE_AcademyMemberLeft)]
        public static void On_WorldClient_AcademyMemberLeft(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }


            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                GuildAcademyMember member;
                if (guild.Academy.GetMember(characterID, out member))
                {
                    guild.Academy.Members.Remove(member);
                    member.Dispose();


                    ZoneCharacter character;
                    if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                    {
                        character.Guild = null;
                        character.GuildAcademy = null;
                        character.GuildAcademyMember = null;


                        GuildManager.RemoveGuildBuff(character);
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZONE_AcademyMemberOnline)]
        public static void On_WorldClient_AcademyMemberOnline(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }


            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                GuildAcademyMember member;
                if (guild.Academy.GetMember(characterID, out member))
                {
                    member.IsOnline = true;


                    ZoneCharacter character;
                    if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                    {
                        character.Guild = guild;
                        character.GuildAcademy = guild.Academy;
                        character.GuildAcademyMember = member;

                        member.Character = character;
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZONE_AcademyMemberOffline)]
        public static void On_WorldClient_AcademyMemberOffline(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID, characterID;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadInt(out characterID))
            {
                return;
            }


            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                GuildAcademyMember member;
                if (guild.Academy.GetMember(characterID, out member))
                {
                    member.IsOnline = false;


                    ZoneCharacter character;
                    if (CharacterManager.GetLoggedInCharacter(characterID, out character))
                    {
                        character.Guild = null;
                        character.GuildAcademy = null;
                        character.GuildAcademyMember = null;

                        member.Character = null;
                    }
                }
            }
        }

        [InterPacketHandler(InterHeader.ZONE_AcademyBuffUpdate)]
        public static void On_WorldClient_AcademyBuffUpdate(WorldConnector pConnector, InterPacket pPacket)
        {
            int guildID;
            DateTime updateTime;
            double keepTime;
            if (!pPacket.TryReadInt(out guildID)
                || !pPacket.TryReadDateTime(out updateTime)
                || !pPacket.TryReadDouble(out keepTime))
            {
                //Client.Dispose();
                return;
            }


            Guild guild;
            if (GuildManager.GetGuildByID(guildID, out guild))
            {
                guild.Academy.GuildBuffUpdateTime = updateTime;
                guild.Academy.GuildBuffKeepTime = TimeSpan.FromSeconds(keepTime);
            }
        }

        #endregion
    }
}