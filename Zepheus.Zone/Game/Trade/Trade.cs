using System;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.Zone.Game;
using Zepheus.Zone.Networking;
using Zepheus.Zone.Managers;
using System.Collections.Generic;

namespace Zepheus.Zone.Data
{
    public sealed class Trade
    {
        #region .ctor
        public Trade(ZoneCharacter pFrom,ZoneCharacter pTo)
        {
            this.pCharFrom = pFrom;
            this.pCharTo = pTo;
            this.pCharFrom.Trade = this;
            this.pCharTo.Trade = this;
            SendTradeBeginn();
        }
        #endregion
        #region Properties

        public ZoneCharacter pCharTo { get; private set; }
        public List<TradeItem> pToHandelItemList = new List<TradeItem>();

        private long pToHandelMoney { get;  set; }
        private bool pToLocket { get;  set; }
        private bool pToAgree { get; set; }
        public byte pToItemCounter { get; private set; }

        private long pFromHandelMoney { get;  set; }
        private bool pFromLocket { get; set; }
        private bool pFromAgree { get; set; }

        public List<TradeItem> pFromHandelItemList = new List<TradeItem>();
        public ZoneCharacter pCharFrom { get; private set; }
        public byte pFromItemCounter { get; private set; }
        
        #endregion
        #region Methods
        #region public
        public void ChangeMoneyToTrade(ZoneCharacter pChar, long money)
        {
            if (this.pCharFrom == pChar)
            {
                this.pFromHandelMoney = money;
                SendChangeMoney(this.pCharTo.Client,money);
            }
            else if (this.pCharTo == pCharTo)
            {
                this.pToHandelMoney = money;
                SendChangeMoney(this.pCharFrom.Client, money);
            }

        }
        public void RemoveItemToHandel(ZoneCharacter pChar,byte pSlot)
        {
            if (this.pCharFrom == pChar)
            {
                TradeItem item = pFromHandelItemList.Find(d => d.TradeSlot == pSlot);
               
                this.pFromHandelItemList.Remove(item);
                SendItemRemovFromHandel(this.pCharTo.Client, pSlot);
                SendItemRemoveMe(this.pCharFrom.Client, pSlot);
                pFromItemCounter--;
            }
            else if (this.pCharTo == pCharTo)
            {
                TradeItem item = pToHandelItemList.Find(d => d.TradeSlot == pSlot);
                this.pToHandelItemList.Remove(item);
                SendItemRemovFromHandel(this.pCharFrom.Client, pSlot);
                SendItemRemoveMe(this.pCharTo.Client, pSlot);
                pToItemCounter--;
            }
        }
        public void AddItemToHandel(ZoneCharacter pChar,byte pSlot)
        {
            Item pItem;
            if (!pChar.Inventory.InventoryItems.TryGetValue(pSlot, out pItem))
                return;
            if (this.pCharFrom == pChar)
            {
    
                TradeItem Item = new TradeItem(pChar, pSlot, pFromItemCounter,pItem);
                this.pFromHandelItemList.Add(Item);
                this.SendTradeAddItemTo(this.pCharTo.Client, pItem,pFromItemCounter);
  
                this.SendTradeAddItemMe(this.pCharFrom.Client, pSlot, pFromItemCounter);
                pFromItemCounter++;

            }
            else if(this.pCharTo == pChar)
            {
              
                TradeItem Item = new TradeItem(pChar, pSlot, pToItemCounter,pItem);
                this.pFromHandelItemList.Add(Item);
                this.SendTradeAddItemTo(this.pCharFrom.Client, pItem, pToItemCounter);
                this.SendTradeAddItemMe(this.pCharTo.Client, pSlot, pToItemCounter);
                pToItemCounter++;
     
            }
               
        }
        public void TradeLock(ZoneCharacter pChar)
        {
            if (this.pCharFrom == pChar)
            {
                this.pFromLocket = true;
                if (this.pFromLocket && this.pToLocket)
                {
                    SendTradeLock(this.pCharFrom.Client);
                    SendTradeRdy();
                }
                else
                {
                   SendTradeLock(this.pCharTo.Client);
                }

            }
            else if (this.pCharTo == pCharTo)
            {
                this.pToLocket = true;
                if (this.pFromLocket && this.pToLocket)
                {
                    SendTradeLock(this.pCharFrom.Client);
                    SendTradeRdy();   
                }
                else
                {
             
                    SendTradeLock(this.pCharTo.Client);
                }
            }

        }
        public void TradeBreak(ZoneCharacter pChar)
        {

            if (this.pCharFrom == pChar)
            {
                this.SendTradeBreak(this.pCharTo.Client);
                this.pCharTo.Trade = null;

            }
            else if (this.pCharTo == pCharTo)
            {
                this.SendTradeBreak(this.pCharFrom.Client);
                this.pCharFrom = null;
            }
        }
        public void AcceptTrade(ZoneCharacter pChar)
        {
            if (this.pCharTo == pChar)
            {
                this.pToAgree = true;
                SendTradeAgreeMe(this.pCharTo.Client);
                SendTradeAgreepTo(this.pCharFrom.Client);
            }
            else if(this.pCharFrom == pChar)
            {
                this.pFromAgree = true;
                SendTradeAgreeMe(this.pCharFrom.Client);
                SendTradeAgreepTo(pCharTo.Client);
            }
            if(this.pFromAgree && this.pToAgree && this.pFromLocket && this.pToLocket)
            {
                TradeComplett();
            }
        }
        #endregion
        #region privat
        private void SendPacketToAllTradeVendors(Packet packet)
        {
            pCharFrom.Client.SendPacket(packet);
            pCharTo.Client.SendPacket(packet);
        }
        private void TradeComplett()
        {
            foreach (var Item in pFromHandelItemList)
            {

                pCharFrom.Inventory.RemoveInventory(Item.Item);
                Item.Item.Owner = (uint)this.pCharTo.ID;
                sbyte pSlot;
                pCharTo.GetFreeInventorySlot(out pSlot);
                Item.Item.Slot = pSlot;
                pCharTo.GiveItem(Item.Item);
            }
            foreach (var Item in pToHandelItemList)
            {
                pCharTo.Inventory.RemoveInventory(Item.Item);
                Item.Item.Owner = (uint)this.pCharTo.ID;
                sbyte pSlot;
                pCharFrom.GetFreeInventorySlot(out pSlot);
                Item.Item.Slot = pSlot;
                pCharFrom.GiveItem(Item.Item);
            }
            SendTradeComplett();
            pCharFrom.Trade = null;
            pCharTo.Trade = null;
            long pToMoney = pCharTo.Character.Money + this.pFromHandelMoney - this.pToHandelMoney;
            long pFromMoney = pCharFrom.Character.Money + this.pToHandelMoney - this.pFromHandelMoney;
            pCharFrom.ChangeMoney(pFromMoney);
            pCharTo.ChangeMoney(pToMoney);
        }
        #endregion 
        #region Packets
        private void SendTradeLock(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendTradeLock))
            {
                pClient.SendPacket(packet);
            }

        }
        private void SendTradeAddItemMe(ZoneClient pClient,byte pSlot,byte TradeSlot)
        {
            using (var packet = new Packet(SH19Type.SendAddItemSuccefull))
            {
                packet.WriteByte(pSlot);
                packet.WriteByte(TradeSlot);
                pClient.SendPacket(packet);
            }

        }
        private void SendTradeAddItemTo(ZoneClient pClient,Item pItem,byte TradepSlot)
        {
            using (var packet = new Packet(SH19Type.SendAddItem))
            {
                packet.WriteByte(TradepSlot);
                if (pItem.ItemInfo.Slot == ItemSlot.None)

                    pItem.WriteStats(packet);

                else

                   pItem.WriteEquipStats(packet);

              pClient.SendPacket(packet);
            }

        }
        private void SendTradeBeginn()
        {
            using (var packet = new Packet(SH19Type.SendTradeAccept))
            {
                packet.WriteUShort(pCharFrom.MapObjectID);
                this.pCharTo.Client.SendPacket(packet);
            }
            using (var packet = new Packet(SH19Type.SendTradeAccept))
            {
                packet.WriteUShort(pCharTo.MapObjectID);
                this.pCharFrom.Client.SendPacket(packet);
            }
        }
        private void SendTradeRdy()
        {
            using (var packet = new Packet(SH19Type.SendTradeRdy))
            {
                SendPacketToAllTradeVendors(packet);
            }
        }
        private void SendTradeAgreeMe(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendTradeAgreeMe))
            {
                pClient.SendPacket(packet);
            }
        }
        private void SendTradeAgreepTo(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendTradeAgreeTo))
            {
                pClient.SendPacket(packet);
            }
        }
        private void SendTradeComplett()
        {
            using (var packet = new Packet(SH19Type.SendTradeComplett))
            {
                SendPacketToAllTradeVendors(packet);
            }
        }
        private void SendChangeMoney(ZoneClient pClient, long money)
        {
            using (var packet = new Packet(SH19Type.SendChangeMoney))
            {
                packet.WriteLong(money);
                pClient.SendPacket(packet);
            }
        }
        private void SendTradeBreak(ZoneClient pClient)
        {
            using (var packet = new Packet(SH19Type.SendTradeBreak))
            {
                pClient.SendPacket(packet);
            }
        }
        private void SendItemRemovFromHandel(ZoneClient pClient, byte Slot)
        {
            using (var packet = new Packet(SH19Type.SendRemoveItemFromHandel))
            {
                packet.WriteByte(Slot);
                pClient.SendPacket(packet);
            }
        }
        private void SendItemRemoveMe(ZoneClient pClient,byte pTradeSlot)
        {
            using (var packet = new Packet(SH19Type.SendItemRemove))
            {
                packet.WriteByte(pTradeSlot);
                pClient.SendPacket(packet);
            }
        }
        #endregion
        #endregion
    }
}
