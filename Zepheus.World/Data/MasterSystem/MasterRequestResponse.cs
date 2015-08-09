using System.Collections.Generic;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.InterLib.Networking;
using Zepheus.Util;
using Zepheus.World.Data;
using Zepheus.World.Networking;
using System;

namespace Zepheus.World.Data
{
    public class MasterRequestResponse
    {
        #region Properties
        public  MasterRequest  pRequest { get; private set; }
        public bool responseAnswer { get; private set; }
        #endregion
        public MasterRequestResponse(MasterRequest pRequest)
        {
            this.pRequest = pRequest;
            this.responseAnswer = this.CheckRequestBeforSendRequest(pRequest);
        }
        public MasterRequestResponse(WorldClient Target,WorldClient Reqeuster)
        {
            this.responseAnswer = this.CheckRequest(Target, Reqeuster);
        }
        #region Methods
        private bool CheckRequestBeforSendRequest(MasterRequest pRequest)
        {
            if (pRequest.InvitedClient.Character.MasterList.Find(m => m.pMemberName == pRequest.InviterClient.Character.Character.Name) != null)
            {
                RequestResponse(pRequest.InviterClient, 0x174E, pRequest.InviterClient.Character.Character.MasterJoin);
                return false;
            }
            if (pRequest.InvitedClient.Character.MasterList.Find(d => d.IsMaster == true) != null)
            {
                RequestResponse(pRequest.InviterClient, 0x1749, pRequest.InviterClient.Character.Character.MasterJoin);
                return false;
            }
            if (pRequest.InviterClient.Character.Character.CharLevel + 5 >= pRequest.InvitedClient.Character.Character.CharLevel)
            {
                RequestResponse(pRequest.InviterClient, 0x174C, pRequest.InviterClient.Character.Character.MasterJoin);
                return false;
            }
           
            if (DateTime.Now.Subtract(pRequest.InviterClient.Character.Character.MasterJoin).TotalHours < 24)
            {
 
                RequestResponse(pRequest.InviterClient, 0x174A, pRequest.InviterClient.Character.Character.MasterJoin);//24 hours must pass before a master can receive a new apprentice.
                return false;
            }
            if (pRequest.InviterClient.Character.MasterList.Count >= 20)
            {
                RequestResponse(pRequest.InviterClient, 0x174D, pRequest.InviterClient.Character.Character.MasterJoin);
                return false;
            }
            return true;
        }
           private bool CheckRequest(WorldClient Target,WorldClient Reqeuster)
            {
            if (Reqeuster.Character.MasterList.Count >= 20)
            {
                SendMasterApprentice(0x0174D, Reqeuster, Target);//The master is unable to accept additional apprentices.
                return false;
            }
            this.SendMasterApprentice(0x1740,Reqeuster,Target);//${Target} has been registered as your apprentice.
            this.InvideResponse(Reqeuster, Target.Character.Character.Name,Reqeuster.Character.Character.CharLevel);
            return true;
        }
        #endregion
        #region Packets
          public void SendMasterRequest()
           {
               using (var packet = new Packet(SH37Type.SendMasterRequest))
               {
                   packet.WriteString(pRequest.InviterClient.Character.Character.Name, 16);
                   packet.WriteString(pRequest.InvitedClient.Character.Character.Name, 16);
                  this.pRequest.InvitedClient.SendPacket(packet);
               }
           }
        private void SendMasterApprentice(ushort pCode,WorldClient Target,WorldClient Requester)
        {
            DateTime now = DateTime.Now;
            int year = (now.Year - 1920 << 1) | 1;
            int month = (now.Month << 4) | 0x0F;

            using (var packet = new Packet(SH37Type.SendRegisterApprentice))
            {
                packet.WriteUShort(pCode);
                packet.WriteString(Target.Character.Character.Name,16);
                packet.WriteByte((byte)year);
                packet.WriteByte((byte)month);
                packet.WriteByte((byte)now.Day);
                packet.WriteByte(0);
                packet.WriteByte(Target.Character.Character.CharLevel);
                packet.WriteByte(0);
                Requester.SendPacket(packet);
            }
        }
        private void InvideResponse(WorldClient pClient,string name,byte level)
        {
            using (var packet = new Packet(SH37Type.SendMasterRequestReponse))
            {
                DateTime now = DateTime.Now;
                packet.WriteUShort(0x1740);//pcode
                packet.WriteString(name, 16);
                packet.WriteByte(0x01);//IsOnline (now.Year - 1900 << 1) | isonline
                packet.WriteByte((byte)(now.Month << 4));
                packet.WriteByte((byte)now.Day);//day
                packet.WriteByte(0);//year
                packet.WriteByte(level);//level
                packet.WriteByte(0);//unk
                packet.WriteByte(2);//unk
                packet.WriteString("KüssMirDieFüße",14);//WTF?
                packet.Fill(22, 0x00);
                packet.WriteByte(112);//unk
                packet.WriteByte(112);//unk
                packet.WriteByte(0);//unk
                pClient.SendPacket(packet);

            }
        }
        private void RequestResponse(WorldClient pclient,ushort pCode,DateTime pTime)
        {
            using (var packet = new Packet(SH37Type.SendMasterRequestReponse))
            {
                packet.WriteUShort(pCode);

                packet.Fill(30, 0x00);
                packet.WriteInt(pTime.Minute);
                packet.WriteInt(pTime.Hour);
                packet.WriteInt(pTime.Month);
                packet.WriteInt(pTime.Day);
                packet.WriteInt(pTime.Year - 1900);

                packet.WriteInt(2);
                packet.WriteInt(pTime.DayOfYear);
                packet.WriteInt(1);
                pclient.SendPacket(packet);
            }
          
        }
       #endregion
    }
}
