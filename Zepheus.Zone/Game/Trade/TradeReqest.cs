using System;
using Zepheus.Zone.Game;
using Zepheus.Zone.Networking;
namespace Zepheus.Zone.Data
{
    public class TradeReqest
    {
        #region .ctor
        public TradeReqest(ZoneCharacter pFrom, ushort ToMapObjectID)
        {
            if (pFrom.SelectedObject.MapObjectID == ToMapObjectID)
            {
                this.CrationTimeStamp = DateTime.Now;
                this.pToTradeClient = pFrom.SelectedObject as ZoneCharacter;
                this.pFromTradeClient = pFrom;
                this.MapID = pFrom.MapID;
            }
        }
		#endregion
        #region Properties
		public DateTime CrationTimeStamp { get; private set; }
		public ZoneCharacter pToTradeClient { get; private set; }
		public ZoneCharacter pFromTradeClient { get; private set; }
        public ushort MapID { get; private set; }
		#endregion

    }
}
