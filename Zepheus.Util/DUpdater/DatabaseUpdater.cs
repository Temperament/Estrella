using System;
using System.IO;
using MySql.Data.MySqlClient;

namespace Zepheus.Util.DUpdater
{
	public sealed class DatabaseUpdater
	{
		public enum DatabaseTypes
		{
			Login,
			World,
		}

		private DatabaseTypes Type { get; set; }


		public void Update()
		{
			Log.WriteLine(LogLevel.Info, "Looking for database updates...");
		}

		   /* using (MySqlConnection connection = new MySqlConnection(ConnectionStringbuilder.CreateConnectionString(this._setting)))
			{
				connection.Open();
				// Check DB version
				try
				{
					using (var reader = new MySqlCommand("SELECT [Version] FROM [ZepheusVersion]", connection).ExecuteReader())
					{
						if (reader.Read())
						{
							version = reader.GetInt32(0);
						}
					}
				}
				catch { } // default is set to 0 :D

				Log.WriteLine(LogLevel.Debug, "Current Database version is {0}", version);

				// Now we have the function, lets see which files there are...
			
				if (Directory.Exists("SQL"))
				{
					string type = _type == DatabaseTypes.Login ? "login" : "world";
					foreach (string filename in Directory.GetFiles("SQL", type + "_*.sql"))
					{
						try
						{
							string[] pieces = filename.Split('_'); // login_XX_dat-a-lawl.sql
							int p = int.Parse(pieces[1]);

							if (p <= version) continue; // Already ran this one!

							if (p < patch)
							{
								Log.WriteLine(LogLevel.Warn, "Patch ID out of order O.o. Using last patch ID instead: {0}", patch);
							}
							else
							{
								patch = p;
							}
							string message = pieces[2].Replace(".sql", "");

							Log.WriteLine(LogLevel.Info, "Trying to update {0} database with patch {1}. Message:", type, patch);
							Log.WriteLine(LogLevel.Info, message);
							RunFile(filename, connection);
						}
						catch (Exception ex)
						{
							Log.WriteLine(LogLevel.Exception, "Could not parse file {0}: {1}", filename, ex.ToString());
							Console.ReadLine();
							Environment.Exit(400);
						}
					}

					if (version < patch)
					{
						Log.WriteLine(LogLevel.Info, "Database updated!");
						version = patch;
						// Try to update table to new version
						using (MySqlCommand cmd = new MySqlCommand("DELETE FROM [ZepheusVersion];", connection))
							cmd.ExecuteNonQuery();
						using (MySqlCommand cmd = new MySqlCommand("INSERT INTO [ZepheusVersion] VALUES (" + patch.ToString() + ");", connection))
							cmd.ExecuteNonQuery();
					}
					else
					{
						Log.WriteLine(LogLevel.Info, "Database up-to-date!");
					}
				}
				else
				{
					Log.WriteLine(LogLevel.Error, "Couldn't find SQL dir. Cannot update db.");
				}

				connection.Close();
			}
		}
	*/
		private void RunFile(string pFilename, MySqlConnection pConnection)
		{
			using (StreamReader sr = new StreamReader(pFilename))
			{
				string line;
				string query = "";
				bool commit = false;
				bool add = true;

				while (!sr.EndOfStream)
				{
					line = sr.ReadLine();
					if (line.StartsWith("/*") || line.StartsWith("#") || line.Trim() == "") continue;
					if (!commit && line.ToUpper().StartsWith("BEGIN TRANSACTION"))
					{
						commit = true;
						add = false;
					}
					if (commit && line.ToUpper().StartsWith("COMMIT"))
					{
						commit = false;
						// Small override.
						line = "GO";
						add = false;
					}
					if (line.ToUpper().StartsWith("GO"))
					{
						if (!commit)
						{
							RunQuery(query, pConnection);
							query = "";
						}
						add = false;

					}
					if (add)
					{
						query += line + Environment.NewLine;
					}
					add = true;
				}
				if (query != "")
				{
					// Try to run last one too.
					RunQuery(query, pConnection);
				}
			}
		}

		private void RunQuery(string pQuery, MySqlConnection pConnection)
		{
			using (MySqlCommand cmd = new MySqlCommand(pQuery, pConnection))
			{
				int changed = cmd.ExecuteNonQuery();
				if (changed != -1)
				{
					Log.WriteLine(LogLevel.Info, "DB query changed {0} rows.", changed);
				}
			}
		}
	}
}
