using System.Collections.Generic;
using Zepheus.FiestaLib;
using Zepheus.InterLib;
using Zepheus.World.InterServer;
using Zepheus.FiestaLib.Networking;
using Zepheus.InterLib.Networking;
using Zepheus.Util;
using Zepheus.World.Data;
using Zepheus.FiestaLib.Data;
using Zepheus.World.Networking;
using System;

namespace Zepheus.World
{
    [ServerModule(InitializationStage.Clients)]
    public class MasterManager
    {
        #region .ctor

        public MasterManager()
        {
            pMasterRequests = new List<MasterRequest>();
        }
        [InitializerMethod]
        public static bool Initialize()
        {
            Instance = new MasterManager();
            return true;
        }
        #endregion
        #region Properties
        public static MasterManager Instance { get; private set; }
        private readonly List<MasterRequest> pMasterRequests;
        #endregion
        #region Methods
        public void AddMasterRequest(WorldClient pClient,string target)
        {
            MasterRequest Request = new MasterRequest(target,pClient);
              MasterRequestResponse response = new MasterRequestResponse(Request);
            if(response.responseAnswer)
            {
             response.SendMasterRequest();
             pMasterRequests.Add(Request);
            }
        }
        public void RemoveMasterRequest(WorldClient pClient)
        {
            MasterRequest Request = pMasterRequests.Find(d => d.InvitedClient == pClient);
            pMasterRequests.Remove(Request);

        }
        public void RemoveMasterMember(WorldClient pClient)
        {
            MasterMember pMember = pClient.Character.MasterList.Find(d => d.IsMaster == true);
            if(pMember != null)
            {
             SendApprenticeRemoveMaster(pMember.pMember, pClient.Character.Character.Name);
             MasterMember Memb =  pMember.pMember.Character.MasterList.Find(d => d.pMemberName == pClient.Character.Character.Name);
             pMember.pMember.Character.MasterList.Remove(Memb);
            }
            pMember.RemoveFromDatabase();
            pMember.RemoveFromDatabase(pMember.MasterID, pClient.Character.Character.Name);
            pClient.Character.MasterList.Remove(pMember);
            pClient.Character.UpdateMasterJoin();
            SendMasterRemoveResponse(pClient);
             
        }
        public void ApprenticeLevelUP(WorldCharacter pChar)
        {
            MasterMember pMember = pChar.MasterList.Find(d => d.IsMaster == true);
            if (pMember != null)
            {
                //Todo Add Break if the difference is greater than 5
                AddApprenticeReward(pChar);
                MasterMember.UpdateLevel(pChar.Character.CharLevel, pChar.Character.Name);
                if(pMember.pMember.Character.Client != null)
                SendApprenticeLevelUp(pMember.pMember,pChar.Character.Name,pChar.Character.CharLevel);
            }
        }
        public void RemoveMasterMember(WorldCharacter pChar,string name)
        {
            MasterMember pMember = pChar.MasterList.Find(d => d.pMemberName == name);
            WorldClient pClient = ClientManager.Instance.GetClientByCharname(name);
            if (pClient != null)
            {
                SendApprenticeRemoveMaster(pClient, pMember.pMemberName);
                pClient.Character.MasterList.Remove(pMember);
            }
            pMember.RemoveFromDatabase();
            pMember.RemoveFromDatabase(pChar.Character.ID, pMember.pMemberName);
            pChar.MasterList.Remove(pMember);
            pChar.UpdateMasterJoin();
        
        }
        public void MasterRequestAccept(string requestername, string TargetName)
        {
            WorldClient target = ClientManager.Instance.GetClientByCharname(TargetName);
            WorldClient requester = ClientManager.Instance.GetClientByCharname(requestername);
            MasterRequestResponse Reponse = new MasterRequestResponse(target, requester);
            if (Reponse.responseAnswer)
            {
                MasterMember ReqMember = new MasterMember(requester,target.Character.ID);
                MasterMember TargetM = new MasterMember(target,requester.Character.ID);
                target.Character.MasterList.Add(ReqMember);
                requester.Character.MasterList.Add(TargetM);
                ReqMember.AddToDatabase();
                TargetM.IsMaster = true;
                TargetM.AddToDatabase();
                SendMasterRequestAccept(requester, TargetName);
            }
            else
            {
                MasterRequest rRequest = pMasterRequests.Find(d => d.InvitedClient == requester);
                this.pMasterRequests.Remove(rRequest);
            }
        }
 
        #endregion
        #region private Methods

        private void SendMasterRemoveResponse(WorldClient pClient)
        {
            using (var packet = new Packet(SH37Type.SendMasterResponseRemove))
            {
                packet.WriteByte(0);
                packet.WriteUShort(0x1740);
                pClient.SendPacket(packet);
            }
        }
        
        private void SendApprenticeRemoveMaster(WorldClient pClient,string name)
         {
           using(var packet = new Packet(SH37Type.SendApprenticeRemoveMaster))
           {
               packet.WriteString(name, 16);
               packet.WriteByte(0);//isonline?
               pClient.SendPacket(packet);
           }

         }
        private void SendApprenticeLevelUp(WorldClient pClient,string charname,byte level)
         {
             using (var packet = new Packet(SH37Type.SendApprenticeLevelUp))
             {
                 packet.WriteString(charname, 16);
                 packet.WriteByte(level);
                 pClient.SendPacket(packet);
             }
         }
        private void SendMasterRequestAccept(WorldClient pClient,string TargetName)
        {
            using(var packet = new Packet(SH37Type.SendMasterRequestAccept))
            {
                packet.WriteString(TargetName, 16);
                pClient.SendPacket(packet);
            }
        }
        private void AddApprenticeReward(WorldCharacter pChar)
        {
            List<MasterRewardItem> Rewards = DataProvider.Instance.MasterRewards.FindAll(d => (byte)d.Job == pChar.Character.Job && d.Level == pChar.Character.CharLevel);
            MasterRewardItem rr = new MasterRewardItem
            {
                ItemID = 250,
                Count = 1,
            };
            ZoneConnection Conn = Program.GetZoneByMap(pChar.Character.PositionInfo.Map);
            if (Conn == null)
                return;

            using (var packet = new Packet(SH37Type.SendApprenticeReward))
            {

                packet.WriteByte((byte)Rewards.Count);//count
                foreach (var pReward in Rewards)
                {
                    packet.WriteUShort(pReward.ItemID);
                    packet.WriteByte(pReward.Count);
                    packet.WriteByte(0);//unk
                    InterHandler.SendAddReward(Conn, pReward.ItemID,pReward.Count,pChar.Character.Name);
                }
                pChar.Client.SendPacket(packet);
            }
        }
        public void SendMasterList(WorldClient pClient)
        {
            if(pClient.Character.MasterList.Count== 0)
                return;

            using(var packet = new Packet(SH37Type.SendMasterList))
            {
                MasterMember Master = pClient.Character.MasterList.Find(d => d.IsMaster == true);
                if (Master != null)
                {
                    int nowyear = (Master.RegisterDate.Year - 1920 << 1) | Convert.ToByte(Master.IsOnline);
                    int nowmonth = (Master.RegisterDate.Month << 4) | 0x0F;
                    packet.WriteString(Master.pMemberName, 16);
                    packet.WriteByte((byte)nowyear);
                    packet.WriteByte((byte)nowmonth);
                    packet.WriteByte((byte)DateTime.Now.Day);
                    packet.WriteByte(0x01);//unk
                    packet.WriteByte(Master.Level);
                    packet.WriteByte(0);//unk
                    packet.WriteByte(0x03);//unk
                    int count = pClient.Character.MasterList.Count - 1;
                    packet.WriteUShort((ushort)count);
                }
                else
                {
                    DateTime now = DateTime.Now;
                    int nowyear = (now.Year - 1920 << 1) | 1;
                    int nowmonth = (now.Month << 4) | 0x0F;
                    packet.WriteString("", 16);
                    packet.WriteByte((byte)nowyear);
                    packet.WriteByte((byte)nowmonth);
                    packet.WriteByte((byte)now.Day);
                    packet.WriteByte(0x01);//unk
                    packet.WriteByte(1);
                    packet.WriteByte(0);//unk
                    packet.WriteByte(0x03);//unk
                    packet.WriteUShort((ushort)pClient.Character.MasterList.Count);
                    //tODO when master null
                }
                foreach(var Member in pClient.Character.MasterList)
                {
                        packet.WriteString(Member.pMemberName, 16);
                        int year = (Member.RegisterDate.Year - 1920 << 1) | Convert.ToUInt16(Member.IsOnline);
                        int month = (Member.RegisterDate.Month << 4) | 0x0F;
                        packet.WriteByte((byte)year);
                        packet.WriteByte((byte)month);
                        packet.WriteByte(0xB9);
                        packet.WriteByte(0x11);//unk
                        packet.WriteByte(Member.Level);
                        packet.WriteByte(0);//unk

                }
                pClient.SendPacket(packet);
            }
        }
        #endregion 
        
    }

}