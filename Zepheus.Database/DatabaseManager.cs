using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Text;
using Zepheus.Util;

namespace Zepheus.Database
{
    /// <summary>
    /// DatabaseManager acts as a proxy towards an encapsulated Database at a DatabaseServer.
    /// </summary>
    public class DatabaseManager
    {
        #region Fields
        private DatabaseServer mServer;
        private Database mDatabase;
        private int MaxCacheQuerysPerClient;
        private DatabaseClient[] mClients = new DatabaseClient[0];
        private bool[] mClientAvailable = new bool[0];
        private int mClientStarvationCounter;
        private object mLockObject;
        private int overloadflags;
        private Task mClientMonitor;
        #endregion

        #region Properties

        #endregion

        #region Constructor
        /// <summary>
        /// Constructs a DatabaseManager for a given DatabaseServer and Database.
        /// </summary>
        /// <param name="pServer">The DatabaseServer for this database proxy.</param>
        /// <param name="pDatabase">The Database for this database proxy.</param>
        public DatabaseManager(DatabaseServer pServer, Database pDatabase, int MaxCacheQuerysPerClientCount, int Overloadflags)
        {
            this.MaxCacheQuerysPerClient = MaxCacheQuerysPerClientCount;
            this.overloadflags = Overloadflags;
            mServer = pServer;
            mDatabase = pDatabase;
            mLockObject = new object();
        }
        /// <summary>
        /// Constructs a DatabaseManager for given database server and database details.
        /// </summary>
        /// <param name="sServer">The network host of the database server, eg 'localhost' or '85.214.55.189'.</param>
        /// <param name="Port">The network port of the database server as an unsigned 32 bit integer.</param>
        /// <param name="sUser">The username to use when connecting to the database.</param>
        /// <param name="sPassword">The password to use in combination with the username when connecting to the database.</param>
        /// <param name="sDatabase">The name of the database to connect to.</param>
        /// <param name="minPoolSize">The minimum connection pool size for the database.</param>
        /// <param name="maxPoolSize">The maximum connection pool size for the database.</param>
        public DatabaseManager(string sServer, uint Port, string sUser, string sPassword, string sDatabase, uint minPoolSize, uint maxPoolSize, int MaxQueryCountPerClient,int OverloadFlags)
        {
            mServer = new DatabaseServer(sServer, Port, sUser, sPassword);
            mDatabase = new Database(sDatabase, minPoolSize, maxPoolSize);
            this.MaxCacheQuerysPerClient = MaxQueryCountPerClient;
            overloadflags = OverloadFlags;
            mClientMonitor = new Task(MonitorClientsLoop);
            //mClientMonitor.Priority = ThreadPriority.Lowest;
            mLockObject = new object();
            mClientMonitor.Start();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starts the client monitor thread. The client monitor disconnects inactive clients etc.
        /// </summary>
        //internal void StartMonitor()
        //{
        //    mClientMonitor = new Task(MonitorClientsLoop);
        //    //mClientMonitor.Priority = ThreadPriority.Lowest;

        //    mClientMonitor.Start();
        //}
        /// <summary>
        /// Stops the client monitor thread.
        /// </summary>
        internal void StopMonitor()
        {
            if (mClientMonitor != null)
            {
                mClientMonitor.Dispose();
            }
        }

        /// <summary>
        /// Disconnects and destroys all database clients.
        /// </summary>
        internal void DestroyClients()
        {
            lock (this)
            {
                for (int i = 0; i < mClients.Length; i++)
                {
                    DatabaseClient tClient = mClients[i];
                    if (tClient != null)
                    {
                        tClient.Destroy();
                        mClients[i] = null;
                    }
                }
            }
        }
        /// <summary>
        /// Nulls all instance fields of the database manager.
        /// </summary>
        internal void DestroyManager()
        {
            //mServer = null;
            //mDatabase = null;
            //mClients = null;
            //mClientAvailable = null;

            //mClientMonitor = null;
        }

        /// <summary>
        /// Closes the connections of database clients that have been inactive for too long. Connections can be opened again when needed.
        /// </summary>
        private void MonitorClientsLoop()
        {
            while (true)
            {
                try
                {
                    lock (mLockObject)
                    {
                        DateTime dtNow = DateTime.Now;
                        for (int i = 0; i < mClients.Length; i++)
                        {
                            if (mClients[i].State != ConnectionState.Closed)
                            {
                                if (mClients[i].InactiveTime >= 60) // Not used in the last %x% seconds
                                {
                                    mClients[i].Disconnect(); // Temporarily close connection

                                    Log.WriteLine(LogLevel.Debug,"Disconnected database client #" + mClients[i].mHandle);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine(LogLevel.Error,"" + ex.ToString() + "DatabaseManager task");
                }
                Thread.Sleep(10000); // 10 seconds
            }
        }
        /// <summary>
        /// Creates the connection string for this database proxy.
        /// </summary>
        internal string CreateConnectionString()
        {
            MySqlConnectionStringBuilder pCSB = new MySqlConnectionStringBuilder();

            // Server
            pCSB.Server = mServer.Host;
            pCSB.Port = mServer.Port;
            pCSB.UserID = mServer.User;
            pCSB.Password = mServer.Password;

            // Database
            pCSB.Database = mDatabase.Name;
            pCSB.MinimumPoolSize = mDatabase.minPoolSize;
            pCSB.MaximumPoolSize = mDatabase.maxPoolSize;

            return pCSB.ToString();
        }

        public DatabaseClient GetClient()
        {
            lock (this)
            {
                if(mClients.Length > 2)
                for (uint i = 0; i < mClients.Length; i++)
                {
                    if (mClientAvailable[i] == true)
                    {
                        mClientAvailable[i] = false;
                        mClientStarvationCounter = 0;

                        if (mClients[i].State == ConnectionState.Closed)
                        {
                            try
                            {
                                ConnectionState StateConn = mClients[i].Connect();
                                if (StateConn == ConnectionState.Connecting)
                                {
                                    Log.WriteLine(LogLevel.Debug,"Opening connection for database client #" + mClients[i].mHandle);
                                }
                                else if (StateConn == ConnectionState.Open)
                                {
                                    mClients[i].Destroy();
                                    mClients[i] = new DatabaseClient(i, this);
                                    mClients[i].Connect();
                                }
                                else if (StateConn == ConnectionState.Closed)
                                {
                                   //TODO Caching
                                    Console.WriteLine("Caching client Message");
                                    return mClients[i];
                                }
                                else if(StateConn == ConnectionState.Broken)
                                {
                                    mClients[i].Destroy();
                                    mClients[i] = new DatabaseClient(i, this);
                                    mClients[i].Connect();
                                }
                            }
                            catch(Exception ex)
                            {

                                Log.WriteLine(LogLevel.Exception,"{0}", ex.Message);
                            }
                        }

                        if (mClients[i].State == ConnectionState.Open)
                        {
                            mClients[i].UpdateLastActivity();
                            if (!mClients[i].IsBussy)
                            {
                                mClients[i].IsBussy = true;
                                return mClients[i];
                            }
                        }
                    }
                }

                mClientStarvationCounter++;

                if (mClientStarvationCounter >= ((mClients.Length + 1) / 2))
                {
                    mClientStarvationCounter = 0;
                    SetClientAmount((uint)(mClients.Length + 1 * 1.3f));
                   
                    return GetClient();
                }

                DatabaseClient pAnonymous = new DatabaseClient(0, this);
                ConnectionState StateConns = pAnonymous.Connect();
                if (StateConns == ConnectionState.Connecting)
                {
                    pAnonymous.IsBussy = true;
                    Log.WriteLine(LogLevel.Debug, "Opening connection for database clientanon");
                }
                else if (StateConns == ConnectionState.Open)
                {
                    pAnonymous.Destroy();
                    pAnonymous = new DatabaseClient(0, this);
                    pAnonymous.Connect();
                }
                else if (StateConns == ConnectionState.Closed)
                {
                    //TODO Caching
                    Console.WriteLine("Caching client Message");
                    return pAnonymous;
                }
                else if (StateConns == ConnectionState.Broken)
                {
                    pAnonymous.Destroy();
                    pAnonymous = new DatabaseClient(0, this);
                    pAnonymous.Connect();
                }
//                pAnonymous.Connect();

                 Log.WriteLine(LogLevel.Debug,"Handed out anonymous client.");
                return pAnonymous;
            }
        }
        internal void ReleaseClient(uint Handle)
        {
            if (mClients.Length >= (Handle - 1)) // Ensure client exists
            {
                mClientAvailable[Handle - 1] = true;
                Log.WriteLine(LogLevel.Debug,"Released client #" + Handle);
            }
        }

        /// <summary>
        /// Sets the amount of clients that will be available to requesting methods. If the new amount is lower than the current amount, the 'excluded' connections are destroyed. If the new connection amount is higher than the current amount, new clients are prepared. Already existing clients and their state will be maintained.
        /// </summary>
        /// <param name="Amount">The new amount of clients.</param>
        internal void SetClientAmount(uint Amount)
        {
            lock (this)
            {
                if (mClients.Length == Amount)
                    return;

                if (Amount < mClients.Length) // Client amount shrinks, dispose clients that will die
                {
                    for (uint i = Amount; i < mClients.Length; i++)
                    {
                        mClients[i].Destroy();
                        mClients[i] = null;
                    }
                }

                DatabaseClient[] pClients = new DatabaseClient[Amount];
                bool[] pClientAvailable = new bool[Amount];
                for (uint i = 0; i < Amount; i++)
                {
                    if (i < mClients.Length) // Keep the existing client and it's available state
                    {
                        pClients[i] = mClients[i];
                        pClientAvailable[i] = mClientAvailable[i];
                    }
                    else // We are in need of more clients, so make another one
                    {
                        pClients[i] = new DatabaseClient((i + 1), this);
                        pClientAvailable[i] = true; // Elegant?
                    }
                }

                // Update the instance fields
                mClients = pClients;
                mClientAvailable = pClientAvailable;
            }
        }

        //internal bool INSERT(IDataObject obj)
        //{
        //    using (DatabaseClient dbClient = GetClient())
        //    {
        //        return obj.INSERT(dbClient);
        //    }
        //}

        //internal bool DELETE(IDataObject obj)
        //{
        //    using (DatabaseClient dbClient = GetClient())
        //    {
        //        return obj.DELETE(dbClient);
        //    }
        //}

        //internal bool UPDATE(IDataObject obj)
        //{
        //    using (DatabaseClient dbClient = GetClient())
        //    {
        //        return obj.UPDATE(dbClient);
        //    }
        //}

        public override string ToString()
        {
            return mServer.ToString() + ":" + mDatabase.Name;
        }

        internal int ConnectionCount
        {
            get
            {
                int Count = 0;
                for (int i = 0; i < mClients.Length; i++)
                {
                    DatabaseClient Client = mClients[i];
                    if (Client == null)
                        continue;
                    if (Client.State != ConnectionState.Closed)
                        Count++;
                }

                return Count;
            }
        }
        #endregion
    }
}

