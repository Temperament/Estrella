using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.Zone.Game;
using Zepheus.Zone.Networking;
using Zepheus.Zone.Managers;

namespace Zepheus.Zone.Handlers
{
    public sealed class Handler19
    {
        [PacketHandler(CH19Type.TradeReqest)]
        public static void TradeReqest(ZoneClient pClient, Packet pPacket)
        {
            ushort MapObjectID;
            if (!pPacket.TryReadUShort(out MapObjectID))
                return;
            TradeManager.Instance.AddTradeRequest(pClient, MapObjectID);
        }
        [PacketHandler(CH19Type.TradeReqestDecline)]
        public static void TradeReqestDecline(ZoneClient pClient, Packet pPacket)
        {
            TradeManager.Instance.RemoveReqest(pClient);
        }
        [PacketHandler(CH19Type.TradeRemoveItem)]
        public static void TradeRemovitem(ZoneClient pClient, Packet pPacket)
        {
            byte pSlot;
            if (!pPacket.TryReadByte(out pSlot))
                return;
            if (pClient.Character.Trade == null)
                return;
            pClient.Character.Trade.RemoveItemToHandel(pClient.Character, pSlot);
        }
        [PacketHandler(CH19Type.TradeAccept)]
        public static void TradeAccept(ZoneClient pClient, Packet pPacket)
        {
            Managers.TradeManager.Instance.AcceptTrade(pClient);
        }
        [PacketHandler(CH19Type.TradeChangeMoney)]
        public static void TradeChangeMoney(ZoneClient pClient, Packet pPacket)
        {
            long money;
            if(!pPacket.TryReadLong(out money))
                return;
            if (pClient.Character.Trade != null)
            {
                pClient.Character.Trade.ChangeMoneyToTrade(pClient.Character, money);
            }
        }
        [PacketHandler(CH19Type.TradeLock)]
        public static void TradeLock(ZoneClient pClient, Packet pPacket)
        {
            if (pClient.Character.Trade != null)
            {
                pClient.Character.Trade.TradeLock(pClient.Character);
            }
        }
        [PacketHandler(CH19Type.TradeAddItem)]
        public static void TradeAddItem(ZoneClient pClient, Packet pPacket)
        {
            byte pSlot;
            if(!pPacket.TryReadByte(out pSlot))
            return;

            if(pClient.Character.Trade == null)
                return;
            pClient.Character.Trade.AddItemToHandel(pClient.Character, pSlot);
        }
        [PacketHandler(CH19Type.TradeAgree)]
        public static void TradeAgree(ZoneClient pClient, Packet pPacket)
        {
            if (pClient.Character.Trade == null)
                return;

            pClient.Character.Trade.AcceptTrade(pClient.Character);
        }
        [PacketHandler(CH19Type.TradeBreak)]
        public static void TradeBreak(ZoneClient pClient, Packet pPacket)
        {
            if (pClient.Character.Trade == null)
                return;

            pClient.Character.Trade.TradeBreak(pClient.Character);
            pClient.Character.Trade = null;

        }
    }
}
