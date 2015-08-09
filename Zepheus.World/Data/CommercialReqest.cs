using System;
using Zepheus.World.Networking;
namespace Zepheus.World.Data
{
    public class TradeReqest
    {
        #region .ctor
        public TradeReqest(WorldClient pFrom, string pToClient)
		{

			this.CrationTimeStamp = DateTime.Now;
			this.pToTradeClient = ClientManager.Instance.GetClientByCharname(pToClient);
			this.pFromTradeClient = pFrom;

		}
		#endregion
        #region Properties
		public DateTime CrationTimeStamp { get; private set; }
		public WorldClient pToTradeClient { get; private set; }
		public WorldClient pFromTradeClient { get; private set; }
		#endregion

    }
}
