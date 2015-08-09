/*File for this file Basic Copyright 2012 no0dl */
using System;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Zepheus.World.Data.Guilds.Academy;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.World.Networking;
using Zepheus.World;
using Zepheus.World.Managers;
using Zepheus.InterLib;
using Zepheus.InterLib.Networking;
using Zepheus.Database;
using Zepheus.Database.DataStore;

namespace Zepheus.World.Data.Guilds
{
    public sealed class Guild
    {
        public int ID { get; private set; }

        public string Name { get; set; }
        public string Password
        {
            get
            {
                var data = _Password;
                //InterCrypto.Decrypt(ref data, 0, data.Length);

                return Encoding.UTF8.GetString(data);
            }
            set
            {
                var data = Encoding.UTF8.GetBytes(value);
               // InterCrypto.Encrypt(ref data, 0, data.Length);

                _Password = data;
            }
        }
        private byte[] _Password;


        public bool AllowGuildWar { get; set; }
        public string Message { get; set; }
        public DateTime MessageCreateTime { get; set; }
        public WorldCharacter MessageCreater { get; set; }

        public DateTime CreateTime { get; private set; }


        public List<GuildMember> Members { get; private set; }
        public GuildMember Master { get { return Members.Find(m => m.Rank == GuildRank.Master); } }

        public GuildAcademy Academy { get; private set; }




        public object ThreadLocker { get; private set; }
        public const int Price = 1000000;





        public Guild(MySqlConnection con, int ID, string Name, byte[] Password, bool AllowGuildWar, WorldCharacter Creater, DateTime CreateTime)
            : this()
        {
            this.ID = ID;
            this.Name = Name;
            _Password = Password;

            this.AllowGuildWar = AllowGuildWar;
            this.CreateTime = CreateTime;

            Message = "";
            MessageCreateTime = Program.CurrentTime;
            MessageCreater = Creater;


            Load();
        }
        public Guild(MySqlConnection con, MySqlDataReader reader)
            : this()
        {
            ID = reader.GetInt32("ID");
            Name = reader.GetString("GuildName");
            // _Password = (byte[])reader.GetValue("Password");
            _Password = new byte[12];
            AllowGuildWar = reader.GetBoolean("AllowGuildWar");
         
            Message = reader.GetString("GuildMessage");
            MessageCreateTime = reader.GetDateTime(8);
            CreateTime = DateTime.Now;//read later

            WorldCharacter creater;
            if (!CharacterManager.Instance.GetCharacterByID(reader.GetInt32("GuildMessageCreater"), out creater))
                throw new InvalidOperationException("Can't find character which created guild message. Character ID: " + reader.GetInt32("GuildMessageCreater"));

            MessageCreater = creater;
            
            Load();
        }
        private Guild()
        {
            ThreadLocker = new object();

            Members = new List<GuildMember>();
        }
        public void Dispose()
        {
            Name = null;
            _Password = null;
            Message = null;
            MessageCreater = null;

            ThreadLocker = null;


            Members.ForEach(m => m.Dispose());
            Members.Clear();
            Members = null;


            Academy.Dispose();
            Academy = null;
        }




        private void Load()
        {
            //members
            DataTable MemberData = null;
           using(DatabaseClient DBClient = Program.DatabaseManager.GetClient())
           {
              MemberData = DBClient.ReadDataTable("SELECT * FROM GuildMembers WHERE GuildID = "+this.ID+"");

           }

           foreach (DataRow row in MemberData.Rows)
           {
                        //get character
                        WorldCharacter character;
                        if (!CharacterManager.Instance.GetCharacterByID(Convert.ToInt32(row["CharID"]), out character))
                            continue;

                        var member = new GuildMember(this,

                                                     character,
                                                     (GuildRank)GetDataTypes.GetByte(row["Rank"]),
                                                     GetDataTypes.GetUshort(row["Korp"]));

                        Members.Add(member);
               }


            //academy
            Academy = new GuildAcademy(this);
        }
        public void Save(MySqlConnection con = null)
        {
            lock (ThreadLocker)
            {
                var conCreated = (con == null);
                if (conCreated)
                {
                    con = Program.DatabaseManager.GetClient().GetConnection();
                }

                //save the guild itself
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "Guild_Save";

                    cmd.Parameters.Add(new MySqlParameter("@pID", ID));
                    cmd.Parameters.Add(new MySqlParameter("@pName", Name));
                    cmd.Parameters.Add(new MySqlParameter("@pPPassword", _Password));
                    cmd.Parameters.Add(new MySqlParameter("@pAllowGuildWar", AllowGuildWar));
                    cmd.Parameters.Add(new MySqlParameter("@pMessage", Message));
                    cmd.Parameters.Add(new MySqlParameter("@pMessageCreateTime", MessageCreateTime));
                    cmd.Parameters.Add(new MySqlParameter("@pMessageCreaterID", MessageCreater.ID));



                    cmd.ExecuteNonQuery();
                }

                //save members
                foreach (var member in Members)
                {
                    member.Save(con);
                }


                //save aka
                Academy.Save(con);



                if (conCreated)
                {
                    con.Dispose();
                }
            }
        }


        public void Broadcast(Packet Packet, GuildMember Exclude = null)
        {
            lock (ThreadLocker)
            {
                foreach (var member in Members)
                {
                    if (Exclude != null
                        && member == Exclude)
                        continue;


                    if (member.Character.IsOnline)
                    {
                        try
                        {
                            member.Character.Client.SendPacket(Packet);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
        }
        public void WriteGuildInfo(Packet Packet)
        {
            Packet.WriteInt(ID);
            Packet.WriteInt(ID); // academy id?
            Packet.WriteString(Name, 16);

            Packet.Fill(24, 0x00); //unk
            Packet.WriteUShort(38);
            Packet.WriteInt(100);
            Packet.Fill(233, 0x00);//unk
            Packet.WriteUShort(11779);
            Packet.WriteShort(20082);
            Packet.WriteInt(31);
            Packet.WriteInt(55);
            Packet.WriteInt(18);//unk
            Packet.WriteInt(15);
            Packet.WriteInt(8);//unk
            Packet.WriteInt(111);//unk
            Packet.WriteInt(4);
            Packet.Fill(136, 0);//buff or string
            Packet.WriteUShort(1824);
            Packet.WriteUShort(20152);
            Packet.WriteInt(16);
            Packet.WriteInt(28);
            Packet.WriteInt(MessageCreateTime.Minute);//createDetails Guild Minutes Date
            Packet.WriteInt(MessageCreateTime.Hour); //create Details Guild Hours Date
            Packet.WriteInt(MessageCreateTime.Day);//create details Guild Day Date
            Packet.WriteInt(MessageCreateTime.Month);//create details Month
            Packet.WriteInt(MessageCreateTime.Year - 1900);//creae details year 1900- 2012
            Packet.WriteInt(10);//unk
            Packet.WriteUShort(2);
            Packet.Fill(6, 0);//unk
            if(MessageCreater.Character.Name == null)
            {
            Packet.WriteString("", 16);
            }
            else
            {
                Packet.WriteString(MessageCreater.Character.Name, 16);
            }
            Packet.WriteString(Message, 512);//details message
        }
        public void SendMemberList(WorldClient Client)
        {
            lock (ThreadLocker)
            {
                for (int i = 0; i < Members.Count; i += 20)
                {
                    using (var packet = GetMemberListPacket(i, i + Math.Min(20, Members.Count - i)))
                    {
                        Client.SendPacket(packet);
                    }
                }
            }
        }
        private Packet GetMemberListPacket(int Start, int End)
        {
            var left = (Members.Count - End);


            var packet = new Packet(SH29Type.GuildMemberList);

            packet.WriteUShort((ushort)Members.Count);
            packet.WriteUShort((ushort)left);
            packet.WriteUShort((ushort)End);
            for (int i = Start; i < End; i++)
            {
                Members[i].WriteInfo(packet);
            }

            return packet;
        }


        public bool GetMember(string Name, out GuildMember Member)
        {
            lock (ThreadLocker)
            {
                Member = Members.Find(m => m.Character.Character.Name.Equals(Name));
            }

            return (Member != null);
        }
        public void AddMember(WorldCharacter Character, GuildRank Rank, MySqlConnection con = null, bool BroadcastAdd = true, bool SendGuildInfoToClient = true)
        {
            lock (ThreadLocker)
            {
                var conCreated = (con == null);
                if (conCreated)
                {
                    con = Program.DatabaseManager.GetClient().GetConnection();
                }

                //add to db
                int result;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GuildMember_Create";

                    cmd.Parameters.Add(new MySqlParameter("@pGuildID", ID));
                    cmd.Parameters.Add(new MySqlParameter("@pCharacterID", Character.ID));
                    cmd.Parameters.Add(new MySqlParameter("@pRank", (byte)Rank));
                    cmd.Parameters.Add(new MySqlParameter("@pCorp", Convert.ToInt16("0")));



                   result  = Convert.ToInt32(cmd.ExecuteScalar());
                }
                if (result == -1)
                    return;
                //create object
                var newMember = new GuildMember(this, Character, Rank, 0);

                //update character
                Character.Guild = this;
                Character.GuildMember = newMember;
                Character.GuildAcademy = Academy;

                //add to list
                Members.Add(newMember);


                if (BroadcastAdd)
                {
                    newMember.BroadcastGuildName();

                    //broadcast that guild member joined
                    using (var packet = new Packet(SH29Type.GuildMemberJoined))
                    {
                        newMember.WriteInfo(packet);


                        Broadcast(packet, newMember);
                    }
                    using (var packet = new Packet(SH29Type.GuildMemberLoggedIn))
                    {
                        packet.WriteString(newMember.Character.Character.Name, 16);


                        Broadcast(packet, newMember);
                    }


                    //let zone know that a new member has been added to guild
                    using (var packet = new InterPacket(InterHeader.ZONE_GuildMemberAdd))
                    {
                        packet.WriteInt(ID);
                        packet.WriteInt(Character.ID);
                        packet.WriteByte((byte)newMember.Rank);
                        packet.WriteUShort(newMember.Corp);


                     
                       Managers.ZoneManager.Instance.Broadcast(packet);
                    }
                }

                //send guild info to new member
                if (SendGuildInfoToClient)
                {
                    SendMemberList(newMember.Character.Client);

                    using (var packet = new Packet(SH4Type.CharacterGuildinfo))
                    {
                        WriteGuildInfo(packet);
                        newMember.Character.Client.SendPacket(packet);
                    }
                }



                if (conCreated)
                {
                    con.Dispose();
                }
            }
        }
        public void RemoveMember(GuildMember Member, MySqlConnection con = null, bool BroadcastRemove = true)
        {
            lock (ThreadLocker)
            {
                var conCreated = (con == null);
                if (conCreated)
                {
                    con = Program.DatabaseManager.GetClient().GetConnection();
                }


                //remove from db
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "GuildMember_Remove";

                    cmd.Parameters.Add(new MySqlParameter("@pGuildID", ID));
                    cmd.Parameters.Add(new MySqlParameter("@pCharacterID", Member.Character.ID));


                    
                    cmd.ExecuteNonQuery();
                }


                //remove from list
                Members.Remove(Member);

                //update character
                Member.Character.Guild = null;
                Member.Character.GuildMember = null;
                Member.Character.GuildAcademy = null;


                //broadcast member left packet
                if (BroadcastRemove)
                {
                    using (var packet = new Packet(SH29Type.GuildMemberLeft))
                    {
                        packet.WriteString(Member.Character.Character.Name);



                        Broadcast(packet);
                    }

                    //send packet to zones that a member has been removed
                    using (var packet = new InterPacket(InterHeader.ZONE_GuildMemberRemove))
                    {
                        packet.WriteInt(ID);
                        packet.WriteInt(Member.Character.ID);


                        ZoneManager.Instance.Broadcast(packet);
                    }
                }


                //clean up
                Member.Dispose();



                if (conCreated)
                {
                    con.Dispose();
                }
            }
        }
        public void UpdateMemberRank(GuildMember Member, GuildRank NewRank)
        {
            Member.Rank = NewRank;
            Member.Save();


            //broadcast to members
            using (var packet = new Packet(SH29Type.UpdateGuildMemberRank))
            {
                packet.WriteString(Member.Character.Character.Name, 16);
                packet.WriteByte((byte)NewRank);


                Broadcast(packet);
            }


            //broadcast to zones
            using (var packet = new InterPacket(InterHeader.ZONE_GuildMemberRankUpdate))
            {
                packet.WriteInt(ID);
                packet.WriteInt(Member.Character.ID);
                packet.WriteByte((byte)NewRank);


                ZoneManager.Instance.Broadcast(packet);
            }
        }
    }
}