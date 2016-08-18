using System;
using System.Collections.Generic;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.InterLib.Networking;
using Estrella.Zone.Game;
using Estrella.Zone.Data;
using Estrella.Zone.Networking;
using Estrella.Util;
using System.Threading;

namespace Estrella.Zone.Managers
{
   [ServerModule(InitializationStage.Clients)]
   public class TradeManager
   {
       #region .ctor
       public TradeManager()
       {
           TradeReqests = new List<TradeReqest>();
       }
        [InitializerMethod]
        public static bool Initialize()
        {
            Instance = new TradeManager();
            
            return true;
        }
       #endregion
       #region Properties
       public static TradeManager Instance { get; private set; }

       private readonly List<TradeReqest> TradeReqests;
   
       #endregion 
       #region Methods

       private void SendTradeRequest(TradeReqest pRequest)
       {
           using (var pPacket = new Packet(SH19Type.SendTradeReqest))
           {
               pPacket.WriteUShort(pRequest.pFromTradeClient.MapObjectID);
               pRequest.pToTradeClient.Client.SendPacket(pPacket);
           }
       }
       private TradeReqest GetTradeRquestByChar(ZoneCharacter pChar)
       {
           TradeReqest Request = TradeReqests.Find(r => r.MapID == pChar.MapID && r.pToTradeClient.MapObjectID == pChar.MapObjectID);
           return Request;
       }
       public void AddTradeRequest(ZoneClient pClient,ushort  MapObjectIDto)
       {
           Log.WriteLine(LogLevel.Debug, "{0} AddTradeReqest {1}", pClient.Character.Character.Name, MapObjectIDto);
           TradeReqest pRequest = new TradeReqest(pClient.Character, MapObjectIDto);
           this.TradeReqests.Add(pRequest);
           SendTradeRequest(pRequest);
       }
       public void RemoveReqest(ZoneClient pClient)
       {
           TradeReqest Request = TradeReqests.Find(r => r.MapID == pClient.Character.MapID && r.pToTradeClient.MapObjectID== pClient.Character.MapObjectID);
           if (TradeReqests.Contains(Request))
           {
               TradeReqests.Remove(Request);
           }
       }
       public void AcceptTrade(ZoneClient pClient)
       {
        TradeReqest Request = GetTradeRquestByChar(pClient.Character);
        if (Request != null)
        {
            TradeReqests.Remove(Request);
            Trade pTrade = new Trade(Request.pFromTradeClient, pClient.Character);
        }
       }
       #endregion
   }
}
