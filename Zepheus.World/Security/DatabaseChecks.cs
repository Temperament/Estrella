using Zepheus.Database;
using System.Data;

namespace Zepheus.World.Security
{
    public sealed class DatabaseChecks
    {
        public static bool IsCharNameUsed(string name)
        {
            DataTable data = null;
            using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
            {
                data = dbClient.ReadDataTable("Select CharID from characters  WHERE binary Name='" + name + "'");
            }
            if (data != null)
            {
                if (data.Rows.Count == 1)
                {
                    return true;
                }
                else
                {
                    if (data.Rows.Count == 0)
                        return false;
                }
                return true;
            }
            else
            {
                return true;
            }
        }
    }
}

