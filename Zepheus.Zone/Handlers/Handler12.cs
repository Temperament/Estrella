using Zepheus.FiestaLib.Data;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.Zone.Game;
using Zepheus.Zone.Networking;

namespace Zepheus.Zone.Handlers
{
    public sealed class Handler12
    {
        [PacketHandler(CH12Type.GetPremiumItemList)]
        public static void GetPremiumItemList(ZoneClient pClient, Packet pPacket)
        {
            byte PageID;
            if (!pPacket.TryReadByte(out PageID))
                return;

            pClient.Character.WritePremiumList(PageID);
        }
        [PacketHandler(CH12Type.GetRewardItemList)]
        public static void GetRewardItemList(ZoneClient pClient, Packet pPacket)
        {
            ushort PageID;
            if (!pPacket.TryReadUShort(out PageID))
                return;

            pClient.Character.WriteRewardList(PageID);
        }
        [PacketHandler(CH12Type.Unequip)]
        public static void ClientUnequippedItem(ZoneClient pClient, Packet pPacket)
        {
            byte sourceSlot, destinationSlot;
            if (!pPacket.TryReadByte(out sourceSlot) ||
                !pPacket.TryReadByte(out destinationSlot))
            {
                Log.WriteLine(LogLevel.Warn, "Could not read unequip values.");
                return;
            }

            Item sourceEquip = pClient.Character.Inventory.EquippedItems.Find(e => (byte)e.ItemInfo.Slot == sourceSlot);
            //Item destinationItem = pClient.Character.Inventory.EquippedItems.Find(i => i.Slot == destinationSlot);// Item was searched from wrong place
            Item destinationItem;
            pClient.Character.Inventory.InventoryItems.TryGetValue(destinationSlot, out destinationItem);       // check if something there
            if (destinationItem != null && (destinationItem.ItemInfo.Slot == ItemSlot.None))
            {
                Log.WriteLine(LogLevel.Warn, "Equipping an item, not possible.");
                // Failed to unequip message here, no need to log it
                return;
            }

            // TODO: If source and destination types are different return.
            // Except rings and costumes (But that can be done later).
            /*
            if( sourceEquip.Type != destinationItem.Type ) {
                Log.WriteLine(LogLevel.Warn, "SourceType != DestinationType, just debugging message, not important");
                // Failed to unequip message here, no need to log it
                return;
            }
            */

            if (destinationItem != null)
            {
                Item destinationEquip = (Item)destinationItem;
                pClient.Character.SwapEquips(sourceEquip, destinationEquip);
            }
            else
            {
                if (sourceEquip == null)
                {
                    Handler12.UpdateEquipSlot(pClient.Character, destinationSlot, 0x24, 0, null);
                    return;
                }
                pClient.Character.UnequipItem(sourceEquip, destinationSlot);
            }
        }
        public static void SendMoveIteminContaInComplet(ZoneClient pClient)
        {
            using (var packet = new Packet(SH12Type.MoveIteminContaInComplet))
            {
                packet.WriteUShort(577);
                pClient.SendPacket(packet);
            }
        }
        [PacketHandler(CH12Type.TakeGuildMoney)]
        public static void TakeGuildMoney(ZoneClient client, Packet packet)
        {
            long TakeMoney;
            if (!packet.TryReadLong(out TakeMoney))
                return;
            if (client.Character.Guild == null)
                return;
            client.Character.Guild.GuildMoney -= TakeMoney;
            client.Character.Guild.GuildStore.SendRemoveFromGuildStore(Data.GuildStoreAddFlags.Gold, client.Character.Character.Name, TakeMoney, client.Character.Guild.GuildMoney);
        }
        [PacketHandler(CH12Type.GiveGuildMoney)]
        public static void GiveGuildMoney(ZoneClient client, Packet packet)
        {
            long GiveMoney;
            if (!packet.TryReadLong(out GiveMoney))
                return;
            if (client.Character.Guild == null)
                return;
            if (client.Character.Character.Money < GiveMoney)
            {
                //todo response you have money to low
                return;
            }
            client.Character.Character.Money -= GiveMoney;
            client.Character.ChangeMoney(client.Character.Character.Money);

            client.Character.Guild.GuildMoney += GiveMoney;
            client.Character.Guild.GuildMoneySave();
            client.Character.Guild.GuildStore.SendAddGuildStore(Data.GuildStoreAddFlags.Gold, client.Character.Character.Name, GiveMoney, client.Character.Guild.GuildMoney);
        }
        [PacketHandler(CH12Type.BuyItem)]
        public static void BuyItem(ZoneClient client, Packet packet)
        {
            ZoneCharacter character = client.Character;
            ushort buyItemID;
            int amount;
            if (packet.TryReadUShort(out buyItemID) && packet.TryReadInt(out amount))
            {
                FiestaLib.Data.ItemInfo buyItem;
                Data.DataProvider.Instance.ItemsByID.TryGetValue(buyItemID, out buyItem);
                if (amount < 255)
                {
                    if (character.GiveItem(buyItemID, (byte)amount) != InventoryStatus.Full)
                    {
                        character.Inventory.Money -= amount * buyItem.BuyPrice;
                        character.ChangeMoney(character.Inventory.Money);
                    }
                }
                else
                {
                    while (amount > 0)
                    {
                        if (character.GiveItem(buyItemID, 255) != InventoryStatus.Full)
                        {
                            character.Inventory.Money -= amount * buyItem.BuyPrice;
                            character.ChangeMoney(character.Inventory.Money);
                            character.CalculateMasterCopper(buyItem.BuyPrice);
                        }
                        if (amount < 255)
                        {
                            if (character.GiveItem(buyItemID, (byte)amount) != InventoryStatus.Full)
                            {
                                character.Inventory.Money -= amount * buyItem.BuyPrice;
                                character.ChangeMoney(character.Inventory.Money);
                                character.CalculateMasterCopper(buyItem.BuyPrice);
                            }
                            break;
                        }
                        amount -= 255;
                    }
                }
            }
        }
        [PacketHandler(CH12Type.SellItem)]
        public static void SellItem(ZoneClient client, Packet packet)
        {
            byte slot;
            int sellcount;
            ZoneCharacter character = client.Character;
            if (packet.TryReadByte(out slot) && packet.TryReadInt(out sellcount))
            {

                Item item;
                character.Inventory.InventoryItems.TryGetValue(slot, out item);
                if (item != null)
                {

                    long fullSellPrice = sellcount * item.ItemInfo.SellPrice;
                    if (item.Ammount > 1)
                    {
                        item.Ammount -= (ushort)sellcount;
                        byte Slot = (byte)item.Slot;
                        Handler12.ModifyInventorySlot(character, 0x24, Slot, Slot, item);
                        character.Inventory.Money += fullSellPrice;
                        character.ChangeMoney(character.Inventory.Money);
                    }
                    else
                    {
                        character.Inventory.Money += fullSellPrice;
                        character.ChangeMoney(character.Inventory.Money);
                        character.Inventory.InventoryItems.Remove(slot);
                        ResetInventorySlot(character, slot);
                    }
                    System.Console.WriteLine(item.ItemInfo.Type);
                }
            }
        }
        [PacketHandler(CH12Type.LootItem)]
        public static void LootHandler(ZoneClient client, Packet packet)
        {
            ushort id;
            if (!packet.TryReadUShort(out id))
            {
                Log.WriteLine(LogLevel.Warn, "Invalid loot request.");
                return;
            }
            client.Character.LootItem(id);
        }

        [PacketHandler(CH12Type.UseItem)]
        public static void UseHandler(ZoneClient client, Packet packet)
        {
            byte slot;
            if (!packet.TryReadByte(out slot))
            {
                Log.WriteLine(LogLevel.Warn, "Error reading used item slot.");
                return;
            }
            client.Character.UseItem(slot);
        }

        public static void SendItemUseOK(ZoneCharacter character)
        {
            using (var packet = new Packet(SH12Type.ItemUsedOk))
            {
                character.Client.SendPacket(packet);
            }
        }

        public static void SendItemUsed(ZoneCharacter character, Item item, ushort error = (ushort) 1792)
        {
            if (error == 1792)
            {
                SendItemUseOK(character);
            }

            using (var packet = new Packet(SH12Type.ItemUseEffect))
            {
                packet.WriteUShort(error); //when not ok, it'll tell you there will be no effect
                packet.WriteUShort(item.ID);
                character.Client.SendPacket(packet);
            }
        }


        public static void ObtainedItem(ZoneCharacter character, DroppedItem item, ObtainedItemStatus status)
        {
            using (var packet = new Packet(SH12Type.ObtainedItem))
            {
                packet.WriteUShort(item.ItemID);
                packet.WriteInt(item.Amount);
                packet.WriteUShort((ushort)status);
                packet.WriteUShort(0xffff);
                character.Client.SendPacket(packet);
            }
        }
        public static Packet InventoryMessage(ushort pMessage, ushort pID = ushort.MaxValue, ushort pCount = (ushort) 1)
        {
            /*  0x0341 	Item Obtained
                0x0342 	Failed to obtain
                0x0346 	Inventory Full  */
            Packet pack = new Packet();
            pack.WriteUShort(0x300a);
            pack.WriteUShort(pID);
            pack.WriteInt(pCount);
            pack.WriteUShort(pMessage);
            pack.Fill(2, 0xff);
            return pack;
        }
        public static Packet EquipUnEquipMessage(ushort pMessage)
        {
            /*  0x0285 	Item cannot be equipped
                0x0281  Item (un)equipped */
            Packet pack = new Packet();
            pack.WriteUShort(3011);
            pack.WriteUShort(pMessage);
            return pack;
        }

        [PacketHandler(CH12Type.Equip)]
        public static void ClientEquippedItem(ZoneClient pClient, Packet pPacket)
        {
            byte fromSlot;
            if (!pPacket.TryReadByte(out fromSlot))
            {
                Log.WriteLine(LogLevel.Warn, "Could not read equip slot.");
                return;
            }

            Item fromItem;
            if (!pClient.Character.Inventory.InventoryItems.TryGetValue(fromSlot, out fromItem))
            {
                Log.WriteLine(LogLevel.Warn, "Equipping empty inventory slot.");
                return;
            }

            if (fromItem == null)
            {
                Log.WriteLine(LogLevel.Warn, "Client tries to equip an ITEM, not EQUIP!");
                return;
            }
            byte toSlot = (byte)fromItem.Slot;
            Item toEquip = pClient.Character.Inventory.EquippedItems.Find(e => e.ItemInfo.Slot == fromItem.ItemInfo.Slot);

            // TODO: Check, does user equip item to correct slot. Right now client only does it.

            ZoneClient client = pClient;
            if (fromItem.ItemInfo.Level > pClient.Character.Level)
            {
                FailedEquip(client.Character, 645); // 85 02
            }
            else
            {
                if (toEquip == null)
                {
                    pClient.Character.EquipItem(fromItem);
                }
                else
                {
                    pClient.Character.SwapEquips(toEquip, fromItem);
                }
            }
        }
        [PacketHandler(CH12Type.MoveItem)]
        public static void MoveItemHandler(ZoneClient pClient, Packet pPacket)
        {
            byte oldslot, oldstate, newslot, newstate;
            if (!pPacket.TryReadByte(out oldslot) ||
                !pPacket.TryReadByte(out oldstate) ||
                !pPacket.TryReadByte(out newslot) ||
                !pPacket.TryReadByte(out newstate))
            {
                Log.WriteLine(LogLevel.Warn, "Could not read item move.");
                return;
            }

            if (oldslot == newslot)
            {
                Log.WriteLine(LogLevel.Warn, "Client tried to dupe an item.");
                return;
            }

            Item source;
            if (!pClient.Character.Inventory.InventoryItems.TryGetValue(oldslot, out source) && newstate != 0x00 && oldstate != 0x00 || newstate == 0x00)
            {
                if (pClient.Character.Guild != null)
                {
                   if(newstate == 0x00 && oldstate == 0x24)
                    {
                        source.Flags = Data.ItemFlags.GuildItem;
                    }
                   else if(newstate == 0x24 && oldstate == 0x00)
                   {
                       source.Flags = Data.ItemFlags.Normal;
                   }
                    else if (source == null || newstate != 0x24)
                    {
                        if (!pClient.Character.Guild.GuildStore.GuildStorageItems.TryGetValue(oldslot, out source))
                        {
                            return;
                        }
                    }
                }
                if (source == null)
                {
                    Log.WriteLine(LogLevel.Warn, "Client tried to move empty slot.");
                    return;
                }
            }
            if (newslot == 0xff || newstate == 0xff)
            {
                pClient.Character.Inventory.InventoryItems.Remove(oldslot);
                source.Delete(); //TODO: make a drop
                Handler12.ModifyInventorySlot(pClient.Character, oldslot, oldstate, (byte)source.Slot, null);
            }
            else if(newstate == 0x00 && oldstate == 0x24 && pClient.Character.Guild != null)
            {

                if (!pClient.Character.Guild.GuildStore.GetHasFreeGuildStoreSlot())
                    //todo GuildStorefuell
                    return;
                pClient.Character.Inventory.RemoveInventory(source);
                pClient.Character.Guild.GuildStore.GuildStorageItems.Add(newslot, source);
                pClient.Character.Guild.GuildStore.SendAddGuildStore(Data.GuildStoreAddFlags.Item, pClient.Character.Character.Name, source.Ammount, 0, source.ItemInfo.ItemID);
                pClient.Character.Guild.GuildStore.SaveStoreItem(pClient.Character.Guild.ID, source.ItemInfo.ItemID, newslot);
                Handler12.ModifyInventorySlot(pClient.Character, oldstate, newstate, oldslot, null);
                Handler12.ModifyInventorySlot(pClient.Character, oldstate, newstate, oldslot, newslot, source);
                return;
            }
            else if(oldstate == 0x00 && newstate == 0x24 && pClient.Character.Guild != null)
            {
                if (!pClient.Character.Guild.GuildStore.GuildStorageItems.TryGetValue(oldslot, out source))
                    return;
                source.Slot = (sbyte)newslot;
                pClient.Character.Guild.GuildStore.GuildStorageItems.Remove(oldslot);
                pClient.Character.Inventory.AddToInventory(source);
                pClient.Character.Guild.GuildStore.SendRemoveFromGuildStore (Data.GuildStoreAddFlags.Item, pClient.Character.Character.Name, source.Ammount, 0, source.ItemInfo.ItemID);
                pClient.Character.Guild.GuildStore.RemoveStoreItem(pClient.Character.Guild.ID, source.ItemInfo.ItemID);
                Handler12.ModifyInventorySlot(pClient.Character, newstate,oldstate, newslot, oldslot, null);
                Handler12.ModifyInventorySlot(pClient.Character, newstate, newstate, newstate, newslot, source);
                return;
            }
            if (source.Flags == Data.ItemFlags.Normal)
            {
                Item destination;
                if (pClient.Character.Inventory.InventoryItems.TryGetValue(newslot, out destination))
                {
                    //item swap
                    pClient.Character.Inventory.InventoryItems.Remove(oldslot);
                    pClient.Character.Inventory.InventoryItems.Remove(newslot);
                    source.Slot = (sbyte)newslot;
                    destination.Slot = (sbyte)oldslot;
                    pClient.Character.Inventory.InventoryItems.Add(newslot, source);
                    pClient.Character.Inventory.InventoryItems.Add(oldslot, destination);
                    source.Save();
                    destination.Save();
                    Handler12.ModifyInventorySlot(pClient.Character, newslot, 0x24, oldslot, destination);
                    Handler12.ModifyInventorySlot(pClient.Character, oldslot, 0x24, newslot, source);
                }
                else
                {
                    //item moved to empty slot
                    pClient.Character.Inventory.InventoryItems.Remove(oldslot);
                    pClient.Character.Inventory.InventoryItems.Add(newslot, source);
                    source.Slot = (sbyte)newslot;
                    source.Save();
                    Handler12.ModifyInventorySlot(pClient.Character, newslot, 0x24, oldslot, null);
                    Handler12.ModifyInventorySlot(pClient.Character, oldslot, 0x24, newslot, source);
                }
            }
            else if (source.Flags == Data.ItemFlags.GuildItem)
            {
                Item destination;
                if (pClient.Character.Guild.GuildStore.GuildStorageItems.TryGetValue(newslot, out destination))
                {
                    //item swap
                    pClient.Character.Guild.GuildStore.GuildStorageItems.Remove(oldslot);
                    pClient.Character.Guild.GuildStore.GuildStorageItems.Remove(newslot);
                    source.Slot = (sbyte)newslot;
                    destination.Slot = (sbyte)oldslot;
                    pClient.Character.Guild.GuildStore.GuildStorageItems.Add(newslot, source);
                    pClient.Character.Guild.GuildStore.GuildStorageItems.Add(oldslot, destination);
                    Handler12.ModifyInventorySlot(pClient.Character, oldstate, newstate, newslot, oldslot, destination);
                    Handler12.ModifyInventorySlot(pClient.Character, oldstate, oldstate, newstate, newslot, source);
                }
                else
                {
                    //item moved to empty slot
                    pClient.Character.Guild.GuildStore.GuildStorageItems.Remove(oldslot);
                    pClient.Character.Guild.GuildStore.GuildStorageItems.Add(newslot, source);
                    source.Slot = (sbyte)newslot;

                    Handler12.ModifyInventorySlot(pClient.Character, oldstate, newstate, newslot, oldslot,null);
                    Handler12.ModifyInventorySlot(pClient.Character, newstate, oldstate, newstate, newslot, source);
                }
            }
        }
        [PacketHandler(CH12Type.DropItem)]
        public static void DropItemHandler(ZoneClient client, Packet packet)
        {
            byte slot;
            if (!packet.TryReadByte(out slot))
            {
                Log.WriteLine(LogLevel.Warn, "Invalid drop request.");
                return;
            }
            client.Character.DropItemRequest(slot);
        }

        [PacketHandler(CH12Type.ItemEnhance)]
        public static void EnhancementHandler(ZoneClient client, Packet packet)
        {
            byte weapslot, stoneslot;
            if (!packet.TryReadByte(out weapslot) ||
                !packet.TryReadByte(out stoneslot))
            {
                Log.WriteLine(LogLevel.Warn, "Invalid item enhance request.");
                return;
            }
            client.Character.UpgradeItem(weapslot, stoneslot);
        }

        public static void SendUpgradeResult(ZoneCharacter character, bool success)
        {
            using (var packet = new Packet(SH12Type.ItemUpgrade))
            {
                packet.WriteUShort(success ? (ushort)2243 : (ushort)2245);
                character.Client.SendPacket(packet);
            }
        }

        public static void InventoryFull(ZoneCharacter character)
        {
            using (var packet = new Packet(SH12Type.InventoryFull))
            {
                packet.WriteUShort(522);
                character.Client.SendPacket(packet);
            }
        }

        public static void FailedUnequip(ZoneCharacter character)
        {
            using (var packet = new Packet(SH12Type.FailedUnequip))
            {
                packet.WriteUShort(706);
                character.Client.SendPacket(packet);
            }
        }

        public static void FailedEquip(ZoneCharacter character, ushort val = (ushort) 0)
        {
            using (var packet = new Packet(SH12Type.FailedEquip))
            {
                packet.WriteUShort(val);
                character.Client.SendPacket(packet);
            }
        }

        public static void UpdateInventorySlot(ZoneCharacter pChar, byte pFromSlot, byte pFromInv, byte pToSlot, Item pItem)
        {
            using (var packet = new Packet(SH12Type.ModifyItemSlot))
            {
                packet.WriteByte(pFromSlot);
                packet.WriteByte(pFromInv);
                packet.WriteByte(pToSlot);
                packet.WriteByte(0x24);         // pToInv
                if (pItem == null)
                {
                    packet.WriteUShort(0xffff);
                }
                else
                {
                    if (pItem.ItemInfo.Slot == ItemSlot.None)
                    {
                        pItem.WriteStats(packet);
                    }
                    else
                    {
                        pItem.WriteEquipStats(packet);
                    }
                }
                pChar.Client.SendPacket(packet);
            }
        }

        public static void UpdateEquipSlot(ZoneCharacter pClient, byte pFromSlot, byte pFromInv, byte pToSlot, Item pItem)
        {
            using (var packet = new Packet(SH12Type.ModifyEquipSlot))
            {
                packet.WriteByte(pFromSlot);
                packet.WriteByte(pFromInv);
                packet.WriteByte(pToSlot);
                if (pItem == null)
                {
                    packet.WriteUShort(0xffff);
                }
                else
                {
                    if (pItem.ItemInfo.Slot == ItemSlot.None)
                        pItem.WriteStats(packet);
                    else
                        pItem.WriteEquipStats(packet);
                }
                pClient.Client.SendPacket(packet);
            }
        }
        public static void ModifyInventorySlot(ZoneCharacter character, byte inventory, byte oldslot, byte newslot, Item item)
        {
            ModifyInventorySlot(character, inventory, (byte)0x24, oldslot, newslot, item);
        }

        public static void ModifyInventorySlot(ZoneCharacter character, byte sourcestate, byte deststate, byte oldslot, byte newslot, Item item)
        {
            using (var packet = new Packet(SH12Type.ModifyItemSlot))
            {
                packet.WriteByte(oldslot);
                packet.WriteByte(sourcestate); //aka 'unequipped' bool
                packet.WriteByte(newslot);
                packet.WriteByte(deststate);
                if (item == null)
                {
                    packet.WriteUShort(0xffff);
                }
                else
                {
                    if (item.ItemInfo.Slot == ItemSlot.None)
                    {
                        item.WriteStats(packet);
                    }
                    else
                    {
                        item.WriteEquipStats(packet);
                    }
                }
                character.Client.SendPacket(packet);
            }
        }

        public static void ResetInventorySlot(ZoneCharacter character, byte slot)
        {


            using (var packet = new Packet(SH12Type.ModifyItemSlot))
            {
                packet.WriteByte(0);
                packet.WriteByte(0x20);
                packet.WriteByte(slot);
                packet.WriteByte(0x24);
                packet.WriteUShort(0xffff);
                character.Client.SendPacket(packet);
            }
            character.Inventory.InventoryItems.Remove(slot);
        }
    }
}