/*File for this file Basic Copyright 2012 no0dl */
using System;
using System.Text;
using System.Linq;
using System.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Zepheus.FiestaLib.Networking;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.World.Data.Guilds.Academy;
using Zepheus.InterLib;
using Zepheus.InterLib.Networking;
using Zepheus.World.Networking;
using Zepheus.World.Managers;
using Zepheus.Util;

namespace Zepheus.World.Data.Guilds
{
    [ServerModule(InitializationStage.Clients)]
    public static class GuildManager
    {
        public static object ThreadLocker { get; private set; }


        private static List<Guild> LoadedGuilds;

        [InitializerMethod]
        public static bool OnAppStart()
        {
            ThreadLocker = new object();

            LoadedGuilds = new List<Guild>();
            CharacterManager.CharacterLogin += CharacterManager.OneLoadGuildInCharacter;//were load Guild to char
            CharacterManager.CharacterLogin += On_CharacterManager_CharacterLogin;
            CharacterManager.OnCharacterLogout += On_CharacterManager_CharacterLogout;
            return true;
        }
        public static void AddGuildToList(Guild pGuild)
        {
         LoadedGuilds.Add(pGuild);
        }
        private static void On_CharacterManager_CharacterLogin(WorldCharacter Character)
        {
            if (Character.IsInGuild)
            {
                var guild = Character.Guild;

                //send guild info to client
                using (var packet = new Packet(SH4Type.CharacterGuildinfo))
                {
                    guild.WriteGuildInfo(packet);


                    Character.Client.SendPacket(packet);
                }


                //send member list to client
                guild.SendMemberList(Character.Client);



                GuildMember member;
                if (guild.GetMember(Character.Character.Name, out member))
                {
                    //send guild member logged in to other guild members
                    using (var packet = new Packet(SH29Type.GuildMemberLoggedIn))
                    {
                        packet.WriteString(Character.Character.Name, 16);


                        Character.Guild.Broadcast(packet, member);
                    }
                }


                //send packet to zone that guild member logged in
                using (var packet = new InterPacket(InterHeader.ZONE_GuildMemberLogin))
                {
                    packet.WriteInt(guild.ID);
                    packet.WriteInt(Character.ID);


                    ZoneManager.Instance.Broadcast(packet);
                }
            }
            else
            {
                using (var packet = new Packet(SH4Type.CharacterGuildinfo))
                {
                    packet.WriteInt(0);


                    Character.Client.SendPacket(packet);
                }
            }



            //academy
            var academy = Character.GuildAcademy;
            if (academy != null)
            {
                if (Character.IsInGuildAcademy)
                {
                    using (var packet = new Packet(SH4Type.CharacterGuildacademyinfo))
                    {
                        academy.WriteInfo(packet);


                        Character.Client.SendPacket(packet);
                    }

                    
                    academy.SendMemberList(Character.Client);
                }
                else
                {
                    using (var packet = new Packet(SH4Type.CharacterGuildacademyinfo))
                    {
                        packet.Fill(5, 0);

                        
                        Character.Client.SendPacket(packet);
                    }
                }
            }
            else
            {
                using (var packet = new Packet(SH4Type.CharacterGuildacademyinfo))
                {
                    packet.Fill(5, 0);


                    Character.Client.SendPacket(packet);
                }
            }
        }
        private static void On_CharacterManager_CharacterLogout(WorldCharacter Character)
        {
            GuildMember member;
            if (Character.Guild != null
                && Character.Guild.GetMember(Character.Character.Name, out member))
            {
                //send guild member logged out to other guild members
                using (var packet = new Packet(SH29Type.GuildMemberLoggedOut))
                {
                    packet.WriteString(Character.Character.Name, 16);


                    Character.Guild.Broadcast(packet, member);
                }


                //send packet to zone that guild member logged out
                using (var packet = new InterPacket(InterHeader.ZONE_GuildMemberLogout))
                {
                    packet.WriteInt(Character.Guild.ID);
                    packet.WriteInt(Character.ID);


                    ZoneManager.Instance.Broadcast(packet);
                }
            }
        }





        public static bool GetGuildByID(int ID, out Guild Guild)
        {
            lock (ThreadLocker)
            {
                Guild = LoadedGuilds.Find(g => g.ID.Equals(ID));


                if (Guild == null)
                {
                    //try to load from db
                    using (var con = Program.DatabaseManager.GetClient().GetConnection())
                    {
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT * FROM Guilds WHERE ID = @pID";

                            cmd.Parameters.Add(new MySqlParameter("@pID", ID));


                            using (var reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                    return false;

                                //create new guild
                                Guild = new Guild(con, reader);

                                //add to list
                                LoadedGuilds.Add(Guild);
                            }
                        }
                    }
                }
            }

            return (Guild != null);
        }
        public static bool GetGuildByName(string Name, out Guild Guild)
        {
            lock (ThreadLocker)
            {
                Guild = LoadedGuilds.Find(g => g.Name.Equals(Name));


                if (Guild == null)
                {
                    //try to load from db
                    using (var con = Program.DatabaseManager.GetClient().GetConnection())
                    {
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT * FROM Guilds WHERE GuildName = @pName";

                            cmd.Parameters.Add(new MySqlParameter("@pName", Name));


                            using (var reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                    return false;

                                //create new guild
                                Guild = new Guild(con, reader);

                                //add to list
                                LoadedGuilds.Add(Guild);
                            }
                        }
                    }
                }
            }

            return (Guild != null);
        }










        #region Game Client Handlers

        
        [PacketHandler(CH29Type.GetGuildList)]
        public static void On_GameClient_GetGuildList(WorldClient Client, Packet Packet)
        {
            if (Client.Character == null)
            {
                return;
            }


            var now = Program.CurrentTime;
            if (now.Subtract(Client.Character.LastGuildListRefresh).TotalSeconds >= 60)
            {
                Client.Character.LastGuildListRefresh = now;



                const int GuildsPerPacket = 100;
                lock (ThreadLocker)
                {
                    using (var con = Program.DatabaseManager.GetClient().GetConnection())
                    {
                        //get guild count
                        int guildCount;
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT COUNT(*) FROM Guilds";


                            guildCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }


                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT ID FROM Guilds";


                            Packet listPacket = null;
                            var count = 0;
                            var globalCount = 0;

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (listPacket == null)
                                    {
                                        listPacket = new Packet(SH29Type.SendGuildList);
                                        listPacket.WriteUShort(3137);
                                        listPacket.WriteByte(1);
                                        listPacket.WriteUShort((ushort)guildCount);
                                        listPacket.WriteUShort((ushort)Math.Min(GuildsPerPacket, guildCount - globalCount));
                                    }



                                    Guild guild;
                                    if (GuildManager.GetGuildByID(reader.GetInt32(0), out guild))
                                    {
                                        //write packet
                                        listPacket.WriteInt(guild.ID);
                                        listPacket.WriteString(guild.Name, 16);
                                        listPacket.WriteString(guild.Master.Character.Character.Name, 16);
                                        listPacket.WriteBool(guild.AllowGuildWar);
                                        listPacket.WriteByte(1);     // unk
                                        listPacket.WriteUShort((ushort)guild.Members.Count);
                                        listPacket.WriteUShort(100); // unk
                                    }
                                    else
                                    {
                                        Packet.Fill(42, 0); // guild get error
                                    }



                                    globalCount++;
                                    count++;
                                    if (count >= Math.Min(GuildsPerPacket, guildCount - globalCount))
                                    {
                                        //send packet
                                        Client.SendPacket(listPacket);

                                        listPacket.Dispose();
                                        listPacket = null;


                                        //reset
                                        count = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [PacketHandler(CH29Type.CreateGuild)]
        public static void On_GameClient_CreateGuild(WorldClient Client, Packet Packet)
        {
            string name, password;
            bool allowGuildWar;
            if (!Packet.TryReadString(out name, 16)
                || !Packet.TryReadString(out password, 8)
                || !Packet.ReadSkip(4) // unk ?
                || !Packet.TryReadBool(out allowGuildWar))
            {
                return;
            }


            GuildCreateResponse response;

            if (Client.Character.Character.CharLevel < 20)
            {
                response = GuildCreateResponse.LevelTooLow;
            }
            else if (Client.Character.Character.Money < Guild.Price)
            {
                response = GuildCreateResponse.MoneyTooLow;
            }
            else
            {
                //encrypt guild pw
               var pwData = Encoding.UTF8.GetBytes(password);
//                InterCrypto.Encrypt(ref pwData, 0, pwData.Length);
              

                Guild guild;

                //try to create guild
                lock (ThreadLocker)
                {
                    int result;
                    int guildID;
                    var createTime = Program.CurrentTime;

                    using (var con = Program.DatabaseManager.GetClient().GetConnection())
                    {
                        //insert guild in db
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "Guild_Create";

                            cmd.Parameters.Add(new MySqlParameter("@pName", name));
                            cmd.Parameters.Add(new MySqlParameter("@pPassword", pwData));
                            cmd.Parameters.Add(new MySqlParameter("@pAllowGuildWar", allowGuildWar));
                            cmd.Parameters.Add(new MySqlParameter("@pCreaterID", Client.Character.ID));
                            cmd.Parameters.Add(new MySqlParameter("@pCreateTime", createTime));

                            var idParam = cmd.Parameters.Add(new MySqlParameter("@pID", SqlDbType.Int)
                                {
                                    Direction = ParameterDirection.Output
                                });
                            result = Convert.ToInt32(cmd.ExecuteScalar());
                            guildID = (int)idParam.Value;
                        }

                        switch (result)
                        {
                            case -1:
                                //guild name already exists (ToDo: get response code)
                              
                                SendGuildCreateResponse(Client, name, password, allowGuildWar, GuildCreateResponse.AlredyExist);
                                return;

                            case -2: //database error @ insert guild (ToDo: get response code)
                                SendGuildCreateResponse(Client, name, password, allowGuildWar, GuildCreateResponse.Failed);
                                return;
                            case -3: //database error @ insert guild academy (ToDo: get response code)
                                SendGuildCreateResponse(Client, name, password, allowGuildWar, GuildCreateResponse.Failed);
                                return;

                            case 0:

                                //create guild
                                guild = new Guild(con, guildID, name, pwData, allowGuildWar, Client.Character, createTime);
                            //insert guild master (character will get updated)
                                guild.AddMember(Client.Character, GuildRank.Master, con, false, false);
                                //add to loaded guilds
                                LoadedGuilds.Add(guild);

                                break;


                            default:
                                return;
                        }
                    }
                }


                Client.Character.
                //revoke money
               Client.Character.ChangeMoney(Client.Character.Character.Money - Guild.Price);

                //let character broadcast guild name packet
                using (var packet = new Packet(SH29Type.GuildNameResult))
                {
                    packet.WriteInt(guild.ID);
                    packet.WriteString(guild.Name, 16);


                    BroadcastManager.Instance.BroadcastInRange(Client.Character, packet, true);
                }

                //let zone know that a guild has been loaded
                using (var packet = new InterPacket(InterHeader.ZONE_GuildCreated))
                {
                    packet.WriteInt(guild.ID);
                    packet.WriteInt(Client.Character.ID);


                    ZoneManager.Instance.Broadcast(packet);
                }

                
                //set response to success
                response = GuildCreateResponse.Success;
            }

            SendGuildCreateResponse(Client, name, password, allowGuildWar, response);
        }
        private static void SendGuildCreateResponse(WorldClient Client, string Name, string Password, bool AllowGuildWar, GuildCreateResponse Response)
        {
            using (var packet = new Packet(SH29Type.CreateGuildResponse))
            {

                packet.WriteUShort((ushort)Response);
                packet.WriteInt((Response == GuildCreateResponse.Success ? 32 : 0));

                packet.WriteString(Name, 16);
                packet.WriteString(Password, 8);
                packet.WriteBool(AllowGuildWar);
                Client.SendPacket(packet);
            }
        }


        [PacketHandler(CH29Type.GuildNameRequest)]
        public static void On_GameClient_GuildNameRequest(WorldClient Client, Packet Packet)
        {
            int guildID;
            if (!Packet.TryReadInt(out guildID))
            {
                return;
            }


            Guild guild;
            if (GetGuildByID(guildID, out guild))
            {
                using (var packet = new Packet(SH29Type.GuildNameResult))
                {
                    packet.WriteInt(guildID);
                    packet.WriteString(guild.Name, 16);



                    Client.SendPacket(packet);
                }
            }
        }


        [PacketHandler(CH29Type.GuildMemberListRequest)]
        public static void On_GameClient_GuildMemberListRequest(WorldClient Client, Packet Packet)
        {
            if (Client.Character == null)
            {
                return;
            }


            if (Client.Character.Guild != null)
            {
                Client.Character.Guild.SendMemberList(Client);
            }
        }

        [PacketHandler(CH29Type.UpdateGuildMessage)]
        public static void On_GameClient_UpdateGuildMessage(WorldClient Client, Packet Packet)
        {
            ushort length;
            string message;
            if (Client.Character.Guild == null
                || !Packet.TryReadUShort(out length)
                || !Packet.TryReadString(out message, length))
            {
                return;
            }

            //response packets
            using (var packet = new Packet(SH29Type.UnkMessageChange))
            {
                packet.WriteHexAsBytes("68 1B 00 92 AD F8 4F 2E 00 00 00 2B 00 00 00 17 00 00 00 07 00 00 00 06 00 00 00 70 00 00 00 06 00 00 00 BC 00 00 00 01 00 00 00 00 00");

                Client.SendPacket(packet);
            }
            using (var packet = new Packet(SH29Type.ClearGuildDetailsMessage))
            {
                packet.WriteUShort(3137);
                packet.WriteLong(0);


                Client.SendPacket(packet);
            }
            using (var packet = new Packet(SH29Type.UpdateGuildMessageResponse))
            {
                packet.WriteUShort(3137);
                Client.SendPacket(packet);
            }



            //update guild
            lock (Client.Character.Guild.ThreadLocker)
            {
                Client.Character.Guild.Message = message;
                Client.Character.Guild.MessageCreater = Client.Character;
                Client.Character.Guild.MessageCreateTime = Program.CurrentTime;

                Client.Character.Guild.Save();



                //broadcast packet to all guild members
                using (var packet = new Packet(SH29Type.SendUpdateGuildDetails))
                {
                    packet.Fill(4, 0x00);
                    packet.WriteInt(Client.Character.Guild.MessageCreateTime.Second);
                    packet.WriteInt(Client.Character.Guild.MessageCreateTime.Minute);
                    packet.WriteInt(Client.Character.Guild.MessageCreateTime.Hour);
                    packet.WriteInt(Client.Character.Guild.MessageCreateTime.Day);
                    packet.WriteInt(Client.Character.Guild.MessageCreateTime.Month - 1);
                    packet.WriteInt(Client.Character.Guild.MessageCreateTime.Year - 1900);
                    packet.WriteInt(0);
                    packet.WriteLong(0);
                    packet.WriteString(Client.Character.Character.Name, 16);
                    packet.WriteUShort(length);
                    packet.WriteString(message, length);


                    
                    Client.Character.Guild.Broadcast(packet);
                }


                //send packet to zone that guild message changed
                using (var packet = new InterPacket(InterHeader.ZONE_GuildMessageUpdate))
                {
                    packet.WriteInt(Client.Character.Guild.ID);
                    packet.WriteInt(Client.Character.ID);
                    packet.WriteDateTime(Client.Character.Guild.MessageCreateTime);

                    packet.WriteUShort(length);
                    packet.WriteString(message, length);
                    ZoneManager.Instance.Broadcast(packet);
                }
            }
        }

        [PacketHandler(CH29Type.LeaveGuild)]
        public static void On_GameClient_LeaveGuild(WorldClient Client, Packet pPacket)
        {
            if (Client.Character.Guild == null)
            {
                return;
            }


            GuildMember member;
            if (Client.Character.Guild.GetMember(Client.Character.Character.Name, out member))
            {
                Client.Character.Guild.RemoveMember(member, null, true);

                using (var packet = new Packet(SH29Type.LeaveGuildResponse))
                {
                    packet.WriteShort(3137);

                    Client.SendPacket(packet);
                }
            }
        }

        [PacketHandler(CH29Type.GuildInviteRequest)]
        public static void On_GameClient_GuildInviteRequest(WorldClient Client, Packet Packet)
        {
            string targetName;
            if (Client.Character == null
                || Client.Character.Guild == null // cheating ?
                || !Packet.TryReadString(out targetName, 16))
            {
                return;
            }


            //get target
            WorldCharacter target;
            if (!CharacterManager.GetLoggedInCharacter(targetName, out target)
                || !target.IsOnline)
            {
                return;
            }


            //todo: check for academy, too
            if (target.Guild != null)
            {
                SendGuildInviteError(Client, targetName, GuildInviteError.TargetHasAlreadyGuild);
                return;
            }


            //send invite to target
            using (var packet = new Packet(SH29Type.GuildInviteRequest))
            {
                packet.WriteString(Client.Character.Guild.Name, 16);
                packet.WriteString(Client.Character.Character.Name, 16);


                
                target.Client.SendPacket(packet);
            }
        }
        private static void SendGuildInviteError(Client Client, string TargetName, GuildInviteError Error)
        {
            using (var packet = new Packet(SH29Type.GuildInviteError))
            {
                packet.WriteString(TargetName, 16);
                packet.WriteUShort((ushort)Error);


                Client.SendPacket(packet);
            }
        }

        [PacketHandler(CH29Type.GuildInviteResponse)]
        public static void On_GameClient_GuildInviteResponse(WorldClient Client, Packet pPacket)
        {
            string guildName;
            bool joinGuild;
            if (!pPacket.TryReadString(out guildName, 16)
                || !pPacket.TryReadBool(out joinGuild))
            {
                return;
            }



            //get guild
            Guild guild;
            if (GetGuildByName(guildName, out guild))
            {
                guild.AddMember(Client.Character, GuildRank.Member, null, true, true);
            }
        }

        [PacketHandler(CH29Type.GuildChat)]
        public static void On_GameClient_GuildChat(WorldClient Client, Packet pPacket)
        {
            byte len;
            string msg;
            if (Client.Character == null
                || !pPacket.TryReadByte(out len)
                || !pPacket.TryReadString(out msg, len))
            {
                return;
            }

            len = (byte)(len + 2);


            if (Client.Character.Guild != null)
            {
                using (var packet = new Packet(SH29Type.GuildChat))
                {
                    packet.WriteInt(Client.Character.Guild.ID);
                    packet.WriteString(Client.Character.Character.Name, 16);
                    
                    packet.WriteByte(len);
                    packet.WriteString(msg, len);


                    Client.Character.Guild.Broadcast(packet);
                }
            }
        }

        [PacketHandler(CH29Type.UpdateGuildMemberRank)]
        public static void On_GameClient_UpdateGuildMemberRank(WorldClient Client, Packet Packet)
        {
            string targetName;
            byte newRankByte;
            if (!Packet.TryReadString(out targetName, 16)
                || !Packet.TryReadByte(out newRankByte))
            {
                return;
            }


            var newRank = (GuildRank)newRankByte;
            GuildMember member;
            GuildMember target;
            if (Client.Character.Guild != null
                && Client.Character.Guild.GetMember(Client.Character.Character.Name, out member)
                && Client.Character.Guild.GetMember(targetName, out target))
            {
                switch (member.Rank)
                {
                    case GuildRank.Master:

                        if (newRank == GuildRank.Master)
                        {
                            Client.Character.Guild.UpdateMemberRank(member, GuildRank.Member);
                        }

                        Client.Character.Guild.UpdateMemberRank(target, newRank);

                        using (var packet = new Packet(SH29Type.UpdateGuildMemberRankResponse))
                        {
                            packet.WriteString(targetName, 16);
                            packet.WriteByte(newRankByte);
                            packet.WriteUShort(3137); // ok response


                            Client.SendPacket(packet);
                        }

                        break;

                    case GuildRank.Admin:
                    case GuildRank.Advice:
                    case GuildRank.Commander:
                    case GuildRank.Default:
                    case GuildRank.Guard:
                    case GuildRank.Member:
                        return;
                }
            }
        }

        #endregion
    }
}