using System;
using System.Data;
using Zepheus.World.Networking;
using Zepheus.Database.DataStore;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace Zepheus.World.Data
{
    public class MasterMember
    {  
        #region .ctor
        #endregion
        #region Properties
        public WorldClient pMember { get; private set; }
        public string pMemberName { get; private set; }
        public DateTime RegisterDate { get; private set; }

        public bool IsOnline { get; private set; }
        public bool IsMaster { get; set; }
        public int CharID { get; set; }
        public int MasterID { get; set; }
        public byte Level { get; private set; }

        public MasterMember()
        {
        }
        public MasterMember(WorldClient pClient,int MasterCharID)
        {
            this.MasterID = MasterCharID;
            this.IsOnline = true;
            this.CharID = pClient.Character.ID;
            this.Level = pClient.Character.Character.CharLevel;
            this.RegisterDate = DateTime.Now;
            this.pMemberName = pClient.Character.Character.Name;
            this.pMember = pClient;
        }
        #endregion
        #region Methods
        public static MasterMember LoadFromDatabase(DataRow row)
        {
            MasterMember Member = new MasterMember
            {
                pMemberName = row["MemberName"].ToString(),
                CharID = GetDataTypes.GetInt(row["CharID"]),
                Level = GetDataTypes.GetByte(row["Level"]),
                IsMaster = GetDataTypes.GetBool(row["isMaster"]),
                MasterID = GetDataTypes.GetInt(row["MasterID"]),
                RegisterDate = DateTime.ParseExact(row["RegisterDate"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
            };
                Member.pMember = ClientManager.Instance.GetClientByCharname(Member.pMemberName);
                Member.IsOnline = ClientManager.Instance.IsOnline(Member.pMemberName);
            return Member;
        }
        public void AddToDatabase()
        {
            Program.DatabaseManager.GetClient().ExecuteQuery("INSERT INTO Masters (CharID,MasterID,MemberName,Level,RegisterDate,isMaster) VALUES ('" + this.MasterID+ "','"+this.CharID+"','" + this.pMemberName + "','" + this.Level + "','" + this.RegisterDate.ToString("yyyy-MM-dd hh:mm") + "','"+Convert.ToByte(this.IsMaster)+"')");
        }
        public void RemoveFromDatabase()
        {
            Program.DatabaseManager.GetClient().ExecuteQuery("DELETE FROM Masters WHERE CharID ='" + this.CharID + "' AND MasterID ='"+this.MasterID+"'");
        }
        public void RemoveFromDatabase(int MasterID,string Charname)
        {
            Program.DatabaseManager.GetClient().ExecuteQuery("DELETE FROM Masters WHERE CharID ='" + MasterID + "' AND MasterID ='" +this.CharID + "'");
        }
        public  static void UpdateLevel(byte level,string charame)
        {
            Program.DatabaseManager.GetClient().ExecuteQuery("UPDATE  Masters SET Level='"+level+"'WHERE binary `MemberName` ='" + charame+ "'");
        }
        public  void SetMemberStatus(bool Status,string name)
        {
            if(Status)
            {
                SetOnline(name);
            }
            else
            {
                SetOffline(name);
            }
        }
        #endregion
        #region Packets
        private void SetOffline(string name)
        {
            this.IsOnline = false;

                using (var packet = new Packet(SH37Type.SendMasterMemberOffline))
                {
                  packet.WriteString(name, 16);
                  this.pMember.SendPacket(packet);
                }

        }
        private void SetOnline(string name)
        {
            this.IsOnline = true;

            using (var packet = new Packet(SH37Type.SendMasterMemberOnline))
            {
                packet.WriteString(name, 16);
               this.pMember.SendPacket(packet);
            }
        }
        #endregion
    }
}
