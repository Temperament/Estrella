using System;
using System.Data;
using MySql.Data.MySqlClient;
using Zepheus.Util;
using System.Collections.Generic;
using Zepheus.Database.Storage;

namespace Zepheus.Database
{
     public sealed class DatabaseClient : IDisposable
     {
        private uint Handle;
 
        private DateTime LastActivity;

        private DatabaseManager Manager;

        private MySqlConnection Connection;
        private MySqlCommand Command;
        public PriorityQueue<MySqlCommand> Commands = new PriorityQueue<MySqlCommand>();
        public int CommandCacheCount;
        public bool IsBussy = false;


        public bool IsAnonymous
        {
            get
            {
                return (Handle == 0);
            }
        }

        public int InactiveTime
        {
            get
            {
                return (int)(DateTime.Now - LastActivity).TotalSeconds;
            }
        }
        public uint mHandle
        {
            get { return Handle; }
        }

       public ConnectionState State
        {
            get
            {
                return (Connection != null) ? Connection.State : ConnectionState.Broken;
            }
        }

       public DatabaseClient(uint mHandle, DatabaseManager pManager)
       {
           if (pManager == null)
               throw new ArgumentNullException("pManager");

           Handle = mHandle;
           Manager = pManager;

           Connection = new MySqlConnection(Manager.CreateConnectionString());
           Command = Connection.CreateCommand();

           UpdateLastActivity();
       }
       public MySqlConnection GetConnection()
       {
           return this.Connection;
       }
           public ConnectionState Connect()
           {
               if (Connection == null && Connection.ConnectionString == null)
               {
                  // Connection.Open();
                   new DatabaseException("Connection instance of database client " + Handle + " holds no value.");
                   return ConnectionState.Broken;
               }
               else if (Connection.State != ConnectionState.Closed)
               {
                   new DatabaseException("Connection instance of database client " + Handle + " requires to be closed before it can open again."); 
                   return ConnectionState.Open;
               }
            try
            {
                Connection.Open();
            }
            catch (MySqlException mex)
            {
               new DatabaseException("Failed to open connection for database client " + Handle + ", exception message: " + mex.Message);
               return ConnectionState.Closed;
            }
            return ConnectionState.Connecting;
           }

        public void Disconnect()
        {
            try
            {
                Connection.Close();
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            if (!this.IsAnonymous) // No disposing for this client yet! Return to the manager!
            {
                IsBussy = false;
                // Reset this!
               // mCommand.CommandText = null;
                Command.Parameters.Clear();

                Manager.ReleaseClient(Handle);
            }
            else // Anonymous client, dispose this right away!
            {
                Destroy();
            }
        }

       public void Destroy()
        {
            Disconnect();

            Connection.Dispose();
            Connection = null;

            Command.Dispose();
            Command = null;

            Manager = null;
        }

        public void UpdateLastActivity()
        {
            LastActivity = DateTime.Now;
        }

        public void AddParamWithValue(string sParam, object val)
        {
            Command.Parameters.AddWithValue(sParam, val);
        }

        private void AddParameters(MySqlCommand command, IEnumerable<MySqlParameter> pParams)
        {
            lock (command)
            {
                foreach (var parameter in pParams)
                {
                    command.Parameters.Add(parameter);
                }
            }
        }
        public void ExecuteQuery(string sQuery)
        {
            try
            {
                

                    if (this.Connection.State == ConnectionState.Closed)
                    {
                        Command.CommandText = sQuery;
                        this.PushCommand(Command);
                    }
                    else
                    {
                        this.IsBussy = true;
                        Command.CommandText = sQuery;
                        Command.Connection = this.Connection;
                        this.PushCommand(Command);
                        for (int i = 0; i < Commands.Count; i++)
                        {
                            MySqlCommand cmd = this.Commands.Dequeue();
                            cmd.Connection = Command.Connection;
                            cmd.ExecuteScalar();
                            this.CommandCacheCount--;
                            Console.WriteLine("Ramm Kacke..");
                        }
                    }
               
                
            }
            catch (Exception e)
            {
                Log.WriteLine(LogLevel.Error,e + "\n (" + sQuery + ")");
            }
        }
        public void PushCommand(MySqlCommand command)
        {
            lock (command)
            {
                CommandCacheCount++;
                Commands.Enqueue(command, CommandCacheCount);
            }
        }
       public void ExecuteQueryWithParameters(MySqlCommand Cmd, params MySqlParameter[] pParams)
        {
            try
            {
                if (this.Connection.State == ConnectionState.Closed)
                {
                    Command = Cmd;
                    this.PushCommand(Command);
                }
                else
                {
                    this.IsBussy = true;
                    Command = Cmd;
                    Command.Connection = this.Connection;
                    this.PushCommand(Command);
                    for (int i = 0; i < Commands.Count; i++)
                    {
                        MySqlCommand cmd = this.Commands.Dequeue();
                        cmd.Connection = Command.Connection;
                        cmd.ExecuteScalar();
                        this.CommandCacheCount--;
                        Console.WriteLine("Ramm Kacke..");
                    }
                }
            }
            catch (Exception e)
            {
               Log.WriteLine(LogLevel.Error,e + "\n (" + Command.CommandText + ")");
            }
         }

        public bool FindsResult(string sQuery)
        {
            bool found = false;
            this.IsBussy = true;
            try
            {
                Command.CommandText = sQuery;
                MySqlDataReader dReader = Command.ExecuteReader();
                found = dReader.HasRows;
                dReader.Close();
            }
            catch (Exception e)
            {
                Log.WriteLine(LogLevel.Error,e + "\n (" + sQuery + ")");
            }
            return found;
        }

        public DataSet ReadDataSet(string query)
        {
            try
            {
                this.IsBussy = true;
                DataSet dataSet = new DataSet();
                Command.CommandText = query;

                using (MySqlDataAdapter adapter = new MySqlDataAdapter(Command))
                {
                    adapter.Fill(dataSet);
                }

               // Command.CommandText = null;
                return dataSet;
            }
            catch (DatabaseException ex)
            {
                Log.WriteLine(LogLevel.Error, ex.ToString());
                return null;
            }
        }

        public DataTable ReadDataTable(string query)
        {
            try
            {
                this.IsBussy = true;
                DataTable dataTable = new DataTable();
                Command.CommandText = query;

                using (MySqlDataAdapter adapter = new MySqlDataAdapter(Command))
                {
                    adapter.Fill(dataTable);
                }

              //  Command.CommandText = null;
                return dataTable;
            }
            catch (DatabaseException ex)
            {
                Log.WriteLine(LogLevel.Error, ex.ToString());
                return null;
            }
        }

        public DataRow ReadDataRow(string query)
        {
            try
            {
                this.IsBussy = true;
                DataTable dataTable = ReadDataTable(query);

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    return dataTable.Rows[0];
                }
            }
            catch (DatabaseException ex)
            {
                Log.WriteLine(LogLevel.Error, ex.ToString());
                return null;
            }
            return null;
        }

        public string ReadString(string query)
        {
            try
            {
                this.IsBussy = true;
                Command.CommandText = query;
                string result = Command.ExecuteScalar().ToString();
               // Command.CommandText = null;
                return result;
            }
            catch (DatabaseException ex)
            {
                Log.WriteLine(LogLevel.Error, ex.ToString());
                return null;
            }
        }

        public uint InsertAndIdentify(string query)
        {
            this.IsBussy = true;
            MySqlCommand command = this.Connection.CreateCommand();
            command.CommandText = query;
            return InsertAndIdentifypublic(command);
        }

        public uint InsertAndIdentifypublic(MySqlCommand pCommand)
        {
            this.IsBussy = true;
            pCommand.Prepare();
            pCommand.ExecuteNonQuery();
            pCommand.CommandText = "SELECT LAST_INSERT_ID()";
            pCommand.Parameters.Clear();
            return (uint)(long)pCommand.ExecuteScalar();
        }
        #region ReadMethods
       public uint ReadUInt(string query)
        {
           this.IsBussy = true;
            Command.CommandText = query;
            uint result = uint.Parse(Command.ExecuteScalar().ToString());
          //  Command.CommandText = null;
            return result;
        }
         public  Int32 ReadInt32(string query)
        {
            this.IsBussy = true;
            Command.CommandText = query;
            Int32 result = Convert.ToInt32(Command.ExecuteScalar());
           // Command.CommandText = null;
            return result;
        }

        public byte[] GetBlob(MySqlCommand pCommand)
        {
            byte[] retvalue;
            try
            {
                this.IsBussy = true;
                retvalue = (byte[])pCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading BLOB: {0} && {1}", ex.Message, ex.StackTrace);
                return null;
            }
            return retvalue;
        }
        #endregion
    }
}