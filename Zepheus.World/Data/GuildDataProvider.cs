using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zepheus.Database;
using Zepheus.Util;
using Zepheus.World.Data.Guilds;
using MySql.Data.MySqlClient;

namespace Zepheus.World.Data
{
    [ServerModule(InitializationStage.GuildProvider)]
    public class GuildDataProvider
    {
        public Dictionary<byte, uint> AcademyLevelUpPoints;
        public static GuildDataProvider Instance { get; set; }

        [InitializerMethod]
        public static bool Init()
        {
            Instance = new GuildDataProvider();
            Log.WriteLine(LogLevel.Info, "GuildDataProvider Initialsize");
            return true;
        }
        public GuildDataProvider()
        {
            LoadAcademyLevelUpPonts();
            LoadGuilds();
        }
        private void LoadGuilds()
        {
            MySqlCommand mysqlCmd = new MySqlCommand("SELECT * FROM Guilds", Program.DatabaseManager.GetClient().GetConnection());
            int GuildCount = 0;
            MySqlDataReader GuildReader = mysqlCmd.ExecuteReader();
            {
                for (int i = 0; i < GuildReader.FieldCount; i++)
                {
                    while (GuildReader.Read())
                    {
                        Guild g = new Guild(Program.DatabaseManager.GetClient().GetConnection(), GuildReader);
                        GuildManager.AddGuildToList(g);
                        GuildCount++;
                    }
                }
            }
            GuildReader.Close();
            Log.WriteLine(LogLevel.Info, "Load {0} Guilds", GuildCount);

        }
        private void LoadAcademyLevelUpPonts()
        {
            AcademyLevelUpPoints = new Dictionary<byte, uint>();
            DataTable AcademyPoints = null;

            using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
            {
                AcademyPoints = dbClient.ReadDataTable("SELECT * FROM `" + Settings.Instance.zoneMysqlDatabase + "`.`AcademyLevelPoints`");
            }

            if (AcademyPoints!= null)
            {
                foreach (DataRow row in AcademyPoints.Rows)
                {
                   AcademyLevelUpPoints.Add(Convert.ToByte(row["Level"]),Convert.ToUInt32(row["Points"]));
                }
            }
            Log.WriteLine(LogLevel.Info, "Load {0 } AcademyLevelUpPoints", this.AcademyLevelUpPoints.Count);
        }
    }
}
