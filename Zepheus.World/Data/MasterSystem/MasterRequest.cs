using System;
using Zepheus.World.Networking;

namespace Zepheus.World.Data
{
    public class MasterRequest
    {
        #region .ctor
        public MasterRequest(string target,WorldClient pClient)
        {
            this.InvitedClient = ClientManager.Instance.GetClientByCharname(target);
            if (this.InvitedClient == null)
                return;

            this.InviterClient = pClient;
            this.CrationTimeStamp = DateTime.Now;
        }
        #endregion
        #region Properties
        public DateTime CrationTimeStamp { get; private set; }
        public WorldClient InvitedClient { get; private set; }
        public WorldClient InviterClient { get; private set; }
        #endregion
    }
}
