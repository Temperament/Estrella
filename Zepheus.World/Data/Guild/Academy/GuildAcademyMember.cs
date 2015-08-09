/*File for this file Basic Copyright 2012 no0dl */
using System;
using System.Data;
using MySql.Data.MySqlClient;
using Zepheus.InterLib.Networking;
using Zepheus.FiestaLib.Networking;
using Zepheus.FiestaLib;
using Zepheus.World.Data;

namespace Zepheus.World.Data.Guilds.Academy
{
    public sealed class GuildAcademyMember
    {
        public GuildAcademy Academy { get; private set; }

        public WorldCharacter Character { get; private set; }
        public GuildAcademyRank Rank { get; set; }

        public DateTime RegisterDate { get; private set; }
        public bool IsChatBlocked { get; set; }




        public GuildAcademyMember(GuildAcademy Academy, WorldCharacter Character, DataRow Row)
        {
            this.Academy = Academy;
            this.Character = Character;

            Load(Row);
        }
        public GuildAcademyMember(GuildAcademy Academy, WorldCharacter Character, DateTime RegisterDate, GuildAcademyRank Rank)
        {
            this.Academy = Academy;
            this.Character = Character;
            this.RegisterDate = RegisterDate;
            this.Rank = Rank;
        }
        public void Dispose()
        {
            Academy = null;
            Character = null;
        }



        private void Load(DataRow Row)
        {
            RegisterDate = Convert.ToDateTime(Row["RegisterDate"]);
            IsChatBlocked = Convert.ToBoolean(Row["IsChatBlocked"]);
            Rank = (GuildAcademyRank)Convert.ToByte(Row["Rank"]);
        }
        public void Save(MySqlConnection con)
        {
            var conCreated = (con == null);
            if (conCreated)
            {
                con = Program.DatabaseManager.GetClient().GetConnection();
            }


            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "GuildAcademyMember_Save";
                cmd.CommandType = CommandType.StoredProcedure;


                cmd.Parameters.Add(new MySqlParameter("@pGuildID", Academy.Guild.ID));
                cmd.Parameters.Add(new MySqlParameter("@pCharacterID", Character.ID));
                cmd.Parameters.Add(new MySqlParameter("@pIsChatBlocked", IsChatBlocked));
                cmd.Parameters.Add(new MySqlParameter("@pRank", (byte)Rank));


                
                cmd.ExecuteNonQuery();
            }


            if (conCreated)
            {
                con.Dispose();
            }
        }









        public void WriteInfo(Packet packet)
        { 
            packet.WriteString(Character.Character.Name, 16);
            packet.Fill(65, 0x00);//unk
            packet.WriteBool(Character.IsIngame);
            packet.Fill(3, 0x00);//unk
            packet.WriteByte(Character.Character.Job);//job 
            packet.WriteByte(Character.Character.CharLevel);//level
            packet.WriteByte(0);// unk
            packet.WriteString(DataProvider.GetMapname(Character.Character.PositionInfo.Map), 12);//mapName
            packet.WriteByte((byte)RegisterDate.Month);//month
            packet.WriteByte(184);//year fortmat unkown
            packet.WriteByte((byte)RegisterDate.Day);//day
            packet.WriteByte(0);//unk
            packet.WriteByte(0);  //unk
        }
        public void BroadcastGuildName()
        {
            var packet = new Packet(SH29Type.GuildNameResult);
            packet.WriteInt(Academy.Guild.ID);
            packet.WriteString(Academy.Guild.Name, 16);

            Character.BroucastPacket(packet);
        }
    }
}