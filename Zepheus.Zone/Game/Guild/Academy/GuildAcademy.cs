/*File for this file Basic Copyright 2012 no0dl */

using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using Zepheus.Zone.Managers;

namespace Zepheus.Zone.Game.Guilds.Academy
{
    public sealed class GuildAcademy
    {
        public Guild Guild { get; private set; }
        public string Message { get; set; }


        public DateTime GuildBuffUpdateTime { get; set; }
        public TimeSpan GuildBuffKeepTime { get; set; }



        public List<GuildAcademyMember> Members { get; private set; }
        public const ushort MaxMembers = 50; // Yes, its up to the server. Max is: 65535




        public GuildAcademy(Guild Guild)
        {
            this.Guild = Guild;

            Members = new List<GuildAcademyMember>();
        }
        public void Dispose()
        {
            Guild = null;

            Message = null;

            Members.ForEach(m => m.Dispose());
            Members.Clear();
            Members = null;
        }
        private void Load()
        {
            //load academy info
            MySqlConnection con = Program.CharDBManager.GetClient().GetConnection();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM GuildAcademy WHERE GuildID = @pGuildID";

                cmd.Parameters.Add(new MySqlParameter("@pGuildID", Guild.ID));


                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new InvalidOperationException("Error getting guild academy info from database for guild: " + Guild.Name);

                    Message = reader.GetString(1);
                }
            }



            //members
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM GuildAcademyMembers WHERE GuildID = @pGuildID";

                cmd.Parameters.Add(new MySqlParameter("@pGuildID", Guild.ID));


                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var member = new GuildAcademyMember(this, reader);

                        //try to get character
                        ZoneCharacter character;
                        if (CharacterManager.GetLoggedInCharacter(reader.GetInt32(1), out character))
                        {
                            member.Character = character;
                            member.IsOnline = true;
                        }


                        Members.Add(member);
                    }
                }
            }
        }





        public bool GetMember(int CharacterID, out GuildAcademyMember Member)
        {
            lock (Guild.ThreadLocker)
            {
                Member = Members.Find(m => m.CharacterID.Equals(CharacterID));
            }

            return (Member != null);
        }
    }
}