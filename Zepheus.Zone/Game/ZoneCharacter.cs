using System;
using System.Threading;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Zepheus.Database;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Data;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.Zone.Data;
using Zepheus.Zone.Handlers;
using Zepheus.Zone.InterServer;
using Zepheus.Zone.Networking;
using Zepheus.Zone.Networking.Security;
using Zepheus.Database.Storage;
using Zepheus.InterLib.Networking;
using Zepheus.Zone.Managers;
using Zepheus.Zone.Game.Guilds;
using Zepheus.Zone.Game.Guilds.Academy;
using Zepheus.Zone.Game.Buffs;

namespace Zepheus.Zone.Game
{
	public class ZoneCharacter : MapObject
	{
		#region .ctor
		public ZoneCharacter(int CharID, bool loadequips = true)
		{
			try
			{
				Character = Zepheus.Database.DataStore.ReadMethods.ReadCharObjectByIDFromDatabase(CharID, Program.CharDBManager);
				if (Character == null) throw new Exception("Character not found.");
				Buffs = new Buffs.Buffs(this);
                this.Inventory = new Game.Inventory(this);
                this.PremiumInventory = new PremiumInventory();
                this.RewardInventory = new RewardInventory();
                this.RewardInventory.LoadRewardItems(this.ID);
                this.PremiumInventory.LoadPremiumItems(this.ID);
				LastShout = Program.CurrentTime;
				ChatBlocked = DateTime.MinValue;
				NextSPRest = DateTime.MaxValue;
				NextHPRest = DateTime.MaxValue;
				SelectedObject = null;
				House = null;
				HP = (uint)Character.HP; //we copy these to make less stress on Entity
				SP = (uint)Character.SP;
				Exp = Character.Exp;
				StonesHP = Character.HPStones;
				StonesSP = Character.SPStones;
				State = PlayerState.Normal;
				Inventory.LoadFull(this);
				LoadSkills();
				if (IsDead)
				{
					HP = MaxHP / 4;         // uhm no? TODO: fix
					Exp = Exp / 2;          // uhm no? TODO: fix
					MapInfo mi;
					DataProvider.Instance.MapsByID.TryGetValue(MapID, out mi);
					if (mi != null)
					{
						Character.PositionInfo.XPos = mi.RegenX;
						Character.PositionInfo.YPos = mi.RegenY;
					}
				}
				SetMap(MapID);
				
				this.Group = GroupManager.Instance.GetGroupForCharacter(this.ID);
				if (this.Group != null)
				{
					this.GroupMember = this.Group.Members.Single(m => m.Name == this.Name);
					this.GroupMember.IsOnline = true;
					this.GroupMember.Character = this;
				}
                if (Character.MountID != 0xffff)
                {
                    Mount DBMount = DataProvider.Instance.GetMountByHandleid((ushort)Character.MountID);
                    this.Mount = DBMount;
                }
				
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Error reading character from entity: {0}", ex.ToString());
			}
		}
		#endregion
		#region Properties

		public Character Character { get; private set; }
		public Group Group { get; set; }
		public GroupMember GroupMember { get; set; }
        #region Inventory
        public Inventory Inventory { get; set; }
        public RewardInventory RewardInventory { get; set; }
        public PremiumInventory PremiumInventory { get; set; }
        #endregion
        public bool IsAttacking { get { return attackingSequence != null && attackingSequence.State != AttackSequence.AnimationState.Ended; } }
		public bool IsMale { get { return Character.LookInfo.Male; } set { Character.LookInfo.Male = value; } }
        public Trade Trade { get; set; }
		public const byte ChatDelay = 0;
		public const byte ShoutDelay = 10;
		public static readonly TimeSpan HpSpUpdateRate = TimeSpan.FromSeconds(3);
        #region Mount
        public bool IsInCasting { get; set; }
        public DateTime LastUse { get; set; }
        public Mount Mount { get; set; }
        #endregion
        public long RecviveCoper { get; set; }
        public int ID { get { return Character.ID; } }
		public int AccountID { get { return Character.AccountID; } }
		public string Name { get { return Character.Name; } set { Character.Name = value; } }
		public byte Slot { get { return Character.Slot; } }
		public ushort MapID { get { return Character.PositionInfo.Map; } set { Character.PositionInfo.Map = value; } }
		public byte Level { get { return Character.CharLevel; } set { Character.CharLevel = value; } }
		public Job Job { get { return (Job)Character.Job; } set { Character.Job = (byte)value; } }
		// Next values we map locally & save at the end (less stress on entity)
		public long Exp { get; set; }
		public short StonesSP { get; set; }
		public short StonesHP { get; set; }
        
        //Guild & GuildAcademy
        public Guild Guild { get; set; }
        public GuildAcademy GuildAcademy { get; set; }
        public GuildAcademyMember GuildAcademyMember { get; set; }
        public GuildMember GuildMember { get; set; }
        public bool IsInaAcademy { get; set; }
		// End of local variables
		#region Stats
		public int Fame { get { return Character.Fame; } set { Character.Fame = value; } }
		public byte Hair { get { return Character.LookInfo.Hair; } set { Character.LookInfo.Hair = value; } }
		public byte HairColor { get { return Character.LookInfo.HairColor; } set { Character.LookInfo.HairColor = value; } }
		public byte Face { get { return Character.LookInfo.Face; } set { Character.LookInfo.Face = value; } }
        #endregion
        #region States
        public byte StatPoints { get { return Character.StatPoints; } set { Character.StatPoints = value; } }
		public byte Str { get { return Character.CharacterStats.StrStats; } set { Character.CharacterStats.StrStats = value; } }
		public byte Dex { get { return Character.CharacterStats.DexStats; } set { Character.CharacterStats.DexStats = value; } }
		public byte Int { get { return Character.CharacterStats.IntStats; } set { Character.CharacterStats.IntStats = value; } }
		public byte Spr { get { return Character.CharacterStats.SprStats; } set { Character.CharacterStats.SprStats = value; } }
		public byte End { get { return Character.CharacterStats.EndStats; } set { Character.CharacterStats.EndStats = value; } }

        public byte StrBonus { get { return Character.CharacterStats.StrBonus; } set { Character.CharacterStats.StrBonus = value; } }
        public byte DexBonus { get { return Character.CharacterStats.DexBonus; } set { Character.CharacterStats.DexBonus = value; } }
        public byte IntBonus { get { return Character.CharacterStats.IntBonus; } set { Character.CharacterStats.IntBonus = value; } }
        public byte SprBonus { get { return Character.CharacterStats.SprBonus; } set { Character.CharacterStats.SprBonus = value; } }
        public byte EndBonus { get { return Character.CharacterStats.EndBonus; } set { Character.CharacterStats.EndBonus = value; } }

		public override uint MaxHP { get { return (uint)(BaseStats.MaxHP + GetMaxHPBuff()); } set { return; } }
		public override uint MaxSP { get { return (uint)(BaseStats.MaxSP + GetMaxSPBuff()); } set { return; } }

        public ushort MinDamage { get { return Character.CharacterStats.MinDamage; } set { Character.CharacterStats.MinDamage = value; } }
        public ushort MaxDamage { get { return Character.CharacterStats.MaxDamage; } set { Character.CharacterStats.MaxDamage = value; } }
        public ushort MinMagic { get { return Character.CharacterStats.MinMagic; } set { Character.CharacterStats.MinMagic = value; } }
        public ushort MaxMagic { get { return Character.CharacterStats.MaxMagic; } set { Character.CharacterStats.MaxMagic = value; } }
        public ushort WeaponDef { get { return Character.CharacterStats.WeaponDef; } set { Character.CharacterStats.WeaponDef = value; } }
        public ushort MagicDef { get { return Character.CharacterStats.MagicDef; } set { Character.CharacterStats.MagicDef = value; } }
        #endregion
        //Parrty Shit
        #region Party Variabels
        public Dictionary<string, ZoneClient> Party = new Dictionary<string, ZoneClient>();
		public bool IsInParty { get; set; } //check variabel for heath update
		public bool HealthThreadState { get; set; }
		public bool SendGrpInsector { get; set; }
        #endregion
        //local shit
        #region ZoneCharacter Variabels
        public ZoneClient Client { get; set; }
		public Dictionary<ushort, Skill> SkillsActive { get; private set; }
		public Dictionary<ushort, Skill> SkillsPassive { get; private set; }
		public PlayerState State { get; set; }
		public MapObject SelectedObject { get; set; }
		public FiestaBaseStat BaseStats { get { return DataProvider.Instance.GetBaseStats(Job, Level); } }
		private Buffs.Buffs Buffs { get; set; }
		public House House { get; set; }
		public MapObject CharacterInTarget { get; set; }
		public Question Question { get; set; }
     
		private AttackSequence attackingSequence;

		public DateTime LastShout { get; set; }
		public DateTime LastChat { get; set; }
		public DateTime ChatBlocked { get; set; }
		public DateTime NextHPRest { get; set; }
		public DateTime NextSPRest { get; set; }
        #endregion
        //lazy loading cheattracker
		private CheatTracker tracker;
		public CheatTracker CheatTracker { get { return tracker ?? (tracker = new CheatTracker(this)); } }
		#endregion
		#region Methods
		
		public bool Save()
		{
			Character.HP = (int)this.HP;
			Character.SP = (int)this.SP;
			Character.Exp = this.Exp;
			Character.HPStones = this.StonesHP;
			Character.SPStones = this.StonesSP;
			Character.PositionInfo.XPos = this.Position.X;
			Character.PositionInfo.YPos = this.Position.Y;
			
			if (Map != null)
			{
				Character.PositionInfo.Map = (byte)Map.MapID;
			}

			DateTime start = DateTime.Now;
            ushort Mountfood = 0;
            ushort MountID = 0xffff;
			try
			{
                if(this.Mount != null)
                {
                    Mountfood = this.Mount.Food;
                    MountID = this.Mount.Handle;
                }
				Program.CharDBManager.GetClient().
					ExecuteQuery(
						"UPDATE Characters SET XPos=" + Character.PositionInfo.XPos 
						+ ", YPos=" + Character.PositionInfo.YPos 
						+ ", Map=" + Character.PositionInfo.Map 
						+ ", Level=" + Character.CharLevel 
						+ ", Job=" + Character.Job 
						+ ", CurHP=" + Character.HP 
						+ " , CurSP=" + Character.SP 
						+ ", Exp=" + Character.Exp 
						+ " , Money=" + Character.Money 
						+ ", Hair=" + Character.LookInfo.Hair 
						+ " , HairColor=" + Character.LookInfo.HairColor 
						+ " , Face=" + Character.LookInfo.Face 
						+ " , StatPoints=" + Character.StatPoints 
						+ " , Str=" + Character.CharacterStats.StrStats 
						+ " , End=" + Character.CharacterStats.EndStats 
						+ " , Dex=" + Character.CharacterStats.DexStats 
						+ " , StrInt=" + Character.CharacterStats.IntStats 
						+ " , Spr=" + Character.CharacterStats.StrStats 
						+ " , GuildID=" + Character.GuildID 
						+ " , UsablePoints=" + Character.UsablePoints
                        + " , MountID=" + MountID
                        + " , MountFood=" + Mountfood
						+ " WHERE CharID=" + Character.ID + "");

				TimeSpan savetime = DateTime.Now - start;
				Log.WriteLine(LogLevel.Debug, "Saved character in {0}", savetime.TotalMilliseconds);
			}
			catch // Note - Try to prevent any general and empty catch-blocks!
			{
			}
			return true;
		}

		public void GiveExp(uint amount, ushort mobid = (ushort) 0xFFFF)
		{
			if (Level == DataProvider.Instance.ExpTable.Count) return; // No overleveling
			if (Exp + amount < 0)
			{
				Exp = long.MaxValue;
			}
			else
			{
				Exp += amount;
			}

			Handler9.SendGainExp(this, amount, mobid);

			while (true)
			{
				if ((ulong)this.Exp >= DataProvider.Instance.GetMaxExpForLevel(Level))
				{
					LevelUP(mobid); // Auto levels
				}
				else
				{
					break;
				}
			}
		}
		public void DropMessage(string text, params object[] param)
		{
			Handler8.SendAdminNotice(Client, String.Format(text, param));
		}		 
		public void Broadcast(Packet packet, bool toself = false)
		{
			Broadcast(packet, MapSector.SurroundingSectors, toself);
		}
		public void Broadcast(Packet packet, List<Sector> sectors, bool toself = false)
		{
			foreach (var character in Map.GetCharactersBySectors(sectors))
			{
				if ((!toself && character == this) || character.Client == null) continue;
				character.Client.SendPacket(packet);
			}
		}
		public override Packet Spawn()
		{
			return Handler7.SpawnSinglePlayer(this);
		}
		public void Ban()
		{
			Save();
			// Program.worldService.DisconnectClient(this.Name, true); // TODO: Inter server packet.
			using (var p = new InterPacket(InterHeader.BanAccount))
			{
				p.WriteString(this.Character.Name, 16);
				WorldConnector.Instance.SendPacket(p);
			}
			Client.Disconnect();
		}
		public void SendGetIngameChunk()
		{
            CharacterManager.InvokeCharacterLogin(this);
			Handler4.SendCharacterInfo(this);
			Handler4.SendCharacterLook(this);
			Handler4.SendQuestListBusy(this);
			Handler4.SendQuestListDone(this);
			Handler4.SendActiveSkillList(this);
			Handler4.SendPassiveSkillList(this);
			Handler4.SendEquippedList(this);
			Handler4.SendInventoryList(this);
			Handler4.SendHouseList(this);
			Handler4.SendPremiumEmotions(this);
			Handler4.SendPremiumItemList(this);
			Handler4.SendTitleList(this);
			Handler4.SendCharacterChunkEnd(this);
			Handler6.SendDetailedCharacterInfo(this);
            this.WritePremiumList(0);
            this.WriteRewardList(0);
          
		}
		public void SwapEquips(Item sourceEquip, Item destEquip)
		{
			try
			{
				this.Inventory.Enter();
				sbyte sourceSlot = sourceEquip.Slot;
				sbyte destSlot = destEquip.Slot;
				this.Inventory.EquippedItems.Remove(sourceEquip);
				this.Inventory.InventoryItems.Remove((byte)destEquip.Slot);
				sourceEquip.Slot = destSlot;
				sourceEquip.IsEquipped = false;
				destEquip.Slot = sourceSlot;
				destEquip.IsEquipped = true;
				this.Inventory.AddToEquipped(destEquip);
				this.Inventory.AddToInventory(sourceEquip);
				sourceEquip.Save();
				destEquip.Save();
				Handler12.UpdateEquipSlot(this, (byte)destSlot, 0x24, (byte)destEquip.ItemInfo.Slot, destEquip);
                Handler12.UpdateInventorySlot(this, (byte)sourceSlot, 0x20, (byte)destSlot, sourceEquip);
                this.UpdateStats();
			}
			finally
			{
			 this.Inventory.Release();
			}
		}
		public void EquipItem(Item pEquip)
		{
			try
			{
				if (pEquip == null) new ArgumentNullException();
				if (!pEquip.IsEquipped || Level > pEquip.ItemInfo.Level) //:Todo Get race
				{
					Item equip = this.Inventory.EquippedItems.Find(d => d.Slot == pEquip.Slot && d.IsEquipped);
					if (equip != null)
					{
						SwapEquips(pEquip, equip);
					}
					else
						this.Inventory.Enter();

					byte sourceSlot = (byte)pEquip.Slot;
					this.Inventory.InventoryItems.Remove(sourceSlot);
					pEquip.IsEquipped = true;
					pEquip.Slot = (sbyte)pEquip.Slot;
					this.Inventory.AddToEquipped(pEquip);
					pEquip.Save();

					Handler12.UpdateEquipSlot(this, sourceSlot, 0x24, (byte)pEquip.ItemInfo.Slot, pEquip);
					Handler12.UpdateInventorySlot(this, (byte)pEquip.ItemInfo.Slot, 0x20, sourceSlot, null);
                    this.UpdateStats();
				}
			}
			finally
			{
			   this.Inventory.Release();
			}
		}
        private void MountCasting(object Cast)
        {
            bool Casting = Convert.ToBoolean(Cast.ToString());
            if (!Casting)
            {
                using (var packet = new Packet(SH8Type.CastItem))
                {
                    packet.WriteUShort((ushort)this.Mount.CastTime);
                    this.Client.SendPacket(packet);
                }
                Thread.Sleep(this.Mount.CastTime - 500);
                this.IsInCasting = false;
                this.State = PlayerState.Mount;
                this.Mount.Tick = System.DateTime.Now;
                using (var packet = new Packet(SH8Type.MapMount))
                {
                    packet.WriteUShort(this.MapObjectID);
                    packet.WriteUShort(this.Mount.Handle);
                    this.Map.Broadcast(packet);
                }

            }
            using (var packet = new Packet(SH8Type.Mounting))
            {
                packet.WriteUShort(this.Mount.Handle);
                this.Client.SendPacket(packet);
            }
            this.UpdateMountFood();
        }
        public void Mounting(ushort pHandle,bool Casting)
        {
            Mount pMount = null;
            DataProvider.Instance.MountyByHandleID.TryGetValue(pHandle, out pMount);
            if (pMount != null && this.Mount != null)
            {
                if (!this.IsInCasting)
                {
                   
                    this.IsInCasting = true;
                    ParameterizedThreadStart pts = new ParameterizedThreadStart(this.MountCasting);
                    Thread CastingThread = new Thread(pts);
                    CastingThread.Start(Casting);
                }
            }
            else
            {
                this.Mount = null;
                //TODO Mounting Failed
            }
        }
  
        public void UpdateMountFood()
        {
            if (this.Mount != null)
            {

                if (this.Mount.Food >= 0)
                {
                    if (!this.Mount.permanent)
                    {
                        using (var packet = new Packet(SH8Type.UpdateMountFood))
                        {
                            packet.WriteUShort(this.Mount.Food);
                            this.Client.SendPacket(packet);
                        }
                        this.Mount.Food--;
                    }
                }
                else
                {
                    this.UnMount();
                }
            }

        }
        public void UnMount()
        {
            using (var packet = new Packet(SH8Type.Unmount))
            {
              this.Client.SendPacket(packet);
            }
            Program.CharDBManager.GetClient().ExecuteQuery("UPDATE Items SET fuelcount="+this.Mount.Food+" WHERE Owner="+this.ID+" AND ItemID="+this.Mount.ItemID+" AND Slot="+this.Mount.ItemSlot+";");
            this.State = PlayerState.Normal;
            this.Mount = null;
            using (var packet = new Packet(SH8Type.MapUnmount))
            {

                packet.WriteUShort(this.MapObjectID);
                this.Map.Broadcast(packet);
            }
        }

		public void UnequipItem(Item pEquip, byte destSlot)
		{
			try
			{
                if (pEquip == null)
                {
                    Log.WriteLine(LogLevel.Error, "Unequip Failed by Slot {0}", destSlot);
                    return;
                }
				  this.Inventory.Enter();
				  byte sourceSlot = (byte)pEquip.Slot;
				this.Inventory.EquippedItems.Remove(pEquip);
				pEquip.Slot = (sbyte)destSlot;
				pEquip.IsEquipped = false;
				this.Inventory.AddToInventory(pEquip);
				pEquip.Save();
				Handler12.UpdateEquipSlot(this, destSlot, 0x24, (byte)pEquip.ItemInfo.Slot, null);
				Handler12.UpdateInventorySlot(this, sourceSlot, 0x20, destSlot, pEquip);
				this.UpdateStats();
			}
			finally
			{
			  this.Inventory.Release();
			}
		}


		public void UseItem(byte slot)
		{
			Item item;
			if (!this.Inventory.InventoryItems.TryGetValue(slot, out item)) //TODO: not sure about return scrolls
			{
				//TODO: send item not found / error occured packet
				return;
			}

			if (item.ItemInfo.Level > Level)
			{
				Handler12.SendItemUsed(this, item, 1800);
				return;
			}

			if (((uint)item.ItemInfo.Jobs & (uint)Job) == 0)
			{
				Handler12.SendItemUsed(this, item, 1801);
				return;
			}

			if (item.ItemInfo.Type == ItemType.Useable) //potion
			{
				if (item.ItemInfo.Class == ItemClass.Scroll) //return scroll
				{
					RecallCoordinate coord;
					MapInfo map;
					if (DataProvider.Instance.RecallCoordinates.TryGetValue(item.ItemInfo.InxName, out coord)
						&& (map = DataProvider.Instance.MapsByID.Values.First(m => m.ShortName == coord.MapName)) != null)
					{
						Handler12.SendItemUsed(this, item); //No idea what this does, but normally it's sent.
						UseOneItemStack(item);
						ChangeMap(map.ID, coord.LinkX, coord.LinkY); //TODO: do this properly via world later.

					}
					else
					{
						Handler12.SendItemUsed(this, item, 1811);
					}
				}
                else if (item.ItemInfo.Class == ItemClass.Mount)
                {
                    Handler12.SendItemUsed(this, item);
                  /*  if (this.Mount == null)
                    {
     
                        if (item.Mount != null)
                        {
                            this.Mount = item.Mount;
                            this.Mount.Food = item.Mount.Food;
                            this.Mounting(item.Mount.Handle,false);
                        }
                        else
                        {
                            //no mount data found
                        }
                    }
                    else
                    {
                        if (this.Mount != null)
                        {
                            UnMount();
                        }
                        else if (this.LastUse.Subtract(DateTime.Now).TotalSeconds >= this.Mount.Cooldown)
                        {
                            Zepheus.FiestaLib.Data.Mount mMount = null;
                            if (DataProvider.Instance.MountyByItemID.TryGetValue(item.ID, out mMount))
                            {
                                
                                this.UnMount();
                                this.Mount = mMount;

                                this.Mounting(Mount.Handle,false);
                            }
                        }
                    }*/
                }
                else if (item.ItemInfo.Class == ItemClass.MountFood)
                {
                    if (this.Mount != null)
                    {
                        Mount cMount = null;
                        if (DataProvider.Instance.MountyByItemID.TryGetValue(item.ID, out cMount))
                        {
                            int newfoodcount = this.Mount.Food += 250;
                            if (cMount.Food >= newfoodcount)
                            {
                                this.Mount.Food = (ushort)newfoodcount;
                            }
                            else
                            {
                                this.Mount.Food = cMount.Food;
                            }
                            this.UpdateMountFood();
                        }
                    }
                }
                else if (item.ItemInfo.Class == ItemClass.SkillBook)
                {
                    //TODO: passive skills!
                    ActiveSkillInfo info;
                    if (DataProvider.Instance.ActiveSkillsByName.TryGetValue(item.ItemInfo.InxName, out info))
                    {
                        if (SkillsActive.ContainsKey(info.ID))
                        {
                            Handler12.SendItemUsed(this, item, 1811);
                            //character has this skill already
                        }
                        else
                        {
                            Handler12.SendItemUsed(this, item);
                            UseOneItemStack(item);
                            DatabaseSkill dskill = new DatabaseSkill();
                            dskill.Character = Character;
                            dskill.SkillID = (short)info.ID;
                            dskill.IsPassive = false;
                            dskill.Upgrades = 0;
                            Character.SkillList.Add(dskill);
                            Program.CharDBManager.GetClient().ExecuteQuery("INSERT INTO Skillist (ID,Owner,SkillID,Upgrades,IsPassive) VALUES ('" + dskill.ID + "','" + dskill.Character.ID + "','" + dskill.SkillID + "','" + dskill.Upgrades + "','" + Convert.ToInt32(dskill.IsPassive) + "')");
                            Save();
                            Skill skill = new Skill(dskill);
                            SkillsActive.Add(skill.ID, skill);
                            Handler18.SendSkillLearnt(this, skill.ID);
                            //TODO: broadcast the animation of learning to others
                        }
                    }
                    else
                    {
                        Log.WriteLine(LogLevel.Error, "Character tried to use skillbook but ActiveSkill does not exist.");
                        Handler12.SendItemUsed(this, item, 1811);
                    }
                }
                else
                {
                    ItemUseEffectInfo effects;
                    if (!DataProvider.Instance.ItemUseEffects.TryGetValue(item.ID, out effects))
                    {
                        Log.WriteLine(LogLevel.Warn, "Missing ItemUseEffect for ID {0}", item.ID);
                        Handler12.SendItemUsed(this, item, 1811);
                        return;
                    }

                    Handler12.SendItemUsed(this, item); //No idea what this does, but normally it's sent.
                    UseOneItemStack(item);
                    foreach (ItemEffect effect in effects.Effects)
                    {
                        switch (effect.Type)
                        {
                            case ItemUseEffectType.AbState: //TOOD: add buffs for itemuse
                                continue;

                            case ItemUseEffectType.HP:
                                HealHP(effect.Value);
                                break;

                            case ItemUseEffectType.SP:
                                HealSP(effect.Value);
                                break;
                            case ItemUseEffectType.ScrollTier:

                                break;

                            default:
                                Log.WriteLine(LogLevel.Warn, "Invalid item effect for ID {0}: {1}", item.ID, effect.Type.ToString());
                                break;
                        }
                    }
                }
			}
			else
			{
				Log.WriteLine(LogLevel.Warn, "Invalid item use.");
			}
		}

		private void UseOneItemStack(Item item)
		{
			byte sendslot = (byte)item.Slot;
			if (item.Ammount > 1)
			{
				--item.Ammount;
				Handler12.ModifyInventorySlot(this, 0x24, sendslot, sendslot, item);
			}
			else
			{
				if (this.Inventory.InventoryItems.Remove((byte)item.Slot))
				{
					item.Delete();
					Handler12.ModifyInventorySlot(this, 0x24, sendslot, sendslot, null);
				}
				else Log.WriteLine(LogLevel.Warn, "Error deleting item from slot {0}.", item.Slot);
			}
			Save();
		}

		public override void Update(DateTime date)
		{
			if (attackingSequence != null)
			{
				attackingSequence.Update(date);
				if (attackingSequence.State == AttackSequence.AnimationState.Ended)
				{
					attackingSequence = null;
				}
			}

			if (SelectedObject != null)
			{
				if (SelectedObject is Mob)
				{
					if ((SelectedObject as Mob).IsDead) SelectedObject = null; // Stop the reference ffs
				}
			}

			if (State == PlayerState.Resting)
			{
				if (date >= NextHPRest)
				{
					HealHP((MaxHP / 1000 * House.Info.HPRecovery));
					//TODO: also show this to people who have me selected.
					NextHPRest = date.AddMilliseconds(House.Info.HPTick);
				}
				if (date >= NextSPRest)
				{
					HealSP((MaxSP / 1000 * House.Info.SPRecovery));
					//TODO: also show this to people who have me selected.
					NextSPRest = date.AddMilliseconds(House.Info.SPTick);
				}
			}
		}

		public void Damage(uint value)
		{
			Damage(null, value);
		}

		public bool RemoveFromMap()
		{
			if (Map != null)
			{
				return Map.RemoveObject(this.MapObjectID);
			}
			else return false;
		}


		public void ChangeMoney(long newMoney)
		{
			this.Character.Money = newMoney;
            InterHandler.UpdateMoneyWorld(newMoney, this.Name);//update in world
			using (var packet = new Packet(SH4Type.Money))
			{
				packet.WriteLong(this.Character.Money);// money
				this.Client.SendPacket(packet);
			}

		}
        private void GiveMoney(long money)
        {
           this.ChangeMoney(this.Inventory.Money += money);
        }

		public void AttackStop()
		{
			if (IsAttacking)
			{
				attackingSequence = null;
			}
		}
		public void Rest(bool pStart)
		{
			if (IsDead)
			{
				Log.WriteLine(LogLevel.Warn, "Zombie tried to rest while being dead. {0}", this);
				CheatTracker.AddCheat(CheatTypes.DeadRest, 100);
				return;
			}
			if (pStart && (this.State == PlayerState.Resting || this.State == PlayerState.Vendor))
			{
				Log.WriteLine(LogLevel.Warn, "Tried to go in home twice {0}", this);
				return;
			}
			else if (!pStart && this.House == null)
			{
				Log.WriteLine(LogLevel.Warn, "Tried to exit house while not in one {0}", this);
			}

			if (pStart)
			{
				this.State = PlayerState.Resting;
				this.House = new House(this, House.HouseType.Resting);
				this.NextHPRest = Program.CurrentTime.AddMilliseconds((uint)this.House.Info.HPTick);
				this.NextSPRest = Program.CurrentTime.AddMilliseconds((uint)this.House.Info.SPTick);
				Handler8.SendBeginRestResponse(this.Client, 0x0a81);

				using (var broad = Handler8.BeginDisplayRest(this))
				{
					this.Broadcast(broad);
				}
			}
			else
			{
				this.State = PlayerState.Normal;
				this.House = null;

				Handler8.SendEndRestResponse(this.Client);
				this.NextHPRest = DateTime.MaxValue;
				this.NextSPRest = DateTime.MaxValue;
				using (var broad = Handler8.EndDisplayRest(this))
				{
					this.Broadcast(broad);
				}
			}
		}
		public void Store(bool pStart, bool pSells = true, ushort pItemID = (ushort) 0, string pName = "")
		{
			if (pStart && (this.State == PlayerState.Resting || this.State == PlayerState.Vendor))
			{
				Log.WriteLine(LogLevel.Warn, "Tried to go in home twice {0}", this);
				return;
			}
			else if (!pStart && this.House == null)
			{
				Log.WriteLine(LogLevel.Warn, "Tried to exit house while not in one {0}", this);
			}

			if (pStart)
			{
				this.State = PlayerState.Vendor;
				this.House = new House(this, pSells ? House.HouseType.SellingVendor : Game.House.HouseType.BuyingVendor, pItemID, pName);
			}
			else
			{
				this.State = PlayerState.Normal;
				this.House = null;
			}
		}
        public void CalculateMasterCopper(long buyprice)
        {
            long recvcoper = buyprice / 100 * 10;
            RecviveCoper =+ recvcoper;
            if (RecviveCoper > 20)//this is not offical like
            {
                InterHandler.SendReciveCoper(this.Name, RecviveCoper,false);
            }
        }
         public bool GiveMasterRewardItem(ushort ItemID,byte count)
        {
            MasterRewardState States;
            ushort PageID;
            byte pSlot;
            if (this.RewardInventory.GetEmptySlot(out pSlot, out PageID))
            {
              if (!Data.DataProvider.Instance.MasterRewardStates.TryGetValue(ItemID, out States))
                 return false;

                RewardItem Reward = new RewardItem
                  {
                      ID = ItemID,
                      Slot = (sbyte)pSlot,
                      PageID = PageID,
                      Ammount = count,

                  };
                Reward.UpgradeStats = new UpgradeStats
                {
                    Str = States.Str,
                    Int = States.Int,
                    Spr = States.Spr,
                    Dex = States.Dex,
                    End = States.End,
                };
                this.RewardInventory.AddRewardItem(Reward);
                return true;
            }
            else
            {
                //Todo Send FULL
                return false;
            }
        }
        public void WritePremiumList(byte PageID)
        {
            List<PremiumItem> Items;
            if(this.PremiumInventory.PremiumItems.TryGetValue(PageID, out Items))
            {
            using (var packet = new Packet(SH12Type.SendPremiumItemList))
            {
                packet.WriteUShort(0x1041);
                packet.WriteByte(1);//unk
                packet.WriteUShort((ushort)this.PremiumInventory.PremiumItems.Count);
                foreach (PremiumItem pItem in Items)
                {
                    pItem.WritePremiumInfo(packet);
                }
                this.Client.SendPacket(packet);
            }
            }
        }
        public void WriteRewardList(ushort PageID)
        {
            List<RewardItem> Items;
            if (this.RewardInventory.RewardItems.TryGetValue(PageID, out Items))
            {
                using (var packet = new Packet(SH12Type.SendRewardList))
                {
                    packet.WriteByte((byte)Items.Count);
                    foreach (RewardItem pItem in Items)
                    {
                        pItem.WriteInfo(packet);
                    }
                    packet.WriteByte(90);//unk
                    Client.SendPacket(packet);
                }
            }
        }

		private void LoadSkills()
		{
			DataTable skilllistdata = null;
			using (DatabaseClient dbClient = Program.CharDBManager.GetClient())
			{
				skilllistdata = dbClient.ReadDataTable("SELECT *FROM Skillist WHERE Owner='" + Character.ID + "'");
			}
			SkillsActive = new Dictionary<ushort, Skill>();
			SkillsPassive = new Dictionary<ushort, Skill>();
			if (skilllistdata != null)
			{
				LoadSkillsFromDataTable(skilllistdata);
			}
		}
		private void LoadSkillsFromDataTable(DataTable skilllistdata)
		{
			foreach (DataRow row in skilllistdata.Rows)
			{
				DatabaseSkill skill = new DatabaseSkill();
				skill.ID = long.Parse(row["ID"].ToString());
				skill.Upgrades = short.Parse(row["Upgrades"].ToString());
				skill.Character = Character;
				skill.SkillID = short.Parse(row["SkillID"].ToString());
				skill.IsPassive = (bool)row["IsPassive"];
				Skill s = new Skill(skill);
				if (s.IsPassive)
				{
					SkillsPassive.Add(s.ID, s);
				}
				else
				{
					SkillsActive.Add(s.ID, s);
				}
			}
		}
		public void Heal()
		{
			HP = MaxHP;
			SP = MaxSP;

			if (State == PlayerState.Dead)
			{
				State = PlayerState.Normal;
			}

			Handler9.SendUpdateHP(this);
			Handler9.SendUpdateSP(this);
		}
		public void SetHP(uint value)
		{
			if (value > MaxHP) value = MaxHP;
			if (value < 0) value = 0;
			HP = value;
			Handler9.SendUpdateHP(this);
		}
		public void SetSP(uint value)
		{
			if (value > MaxSP) value = MaxSP;
			if (value < 0) value = 0;
			SP = value;
			Handler9.SendUpdateSP(this);
		}
		public void HealHP(uint value)
		{
			if (HP == MaxHP) return;

			if (HP + value > MaxHP)
				HP = MaxHP;
			else
				HP += value;

			Handler9.SendUpdateHP(this);
		}
		public void HealSP(uint value)
		{
			if (SP == MaxSP) return;

			if (SP + value > MaxSP)
				SP = MaxSP;
			else
				SP += value;

			Handler9.SendUpdateSP(this);
		}

		public void WriteCharacterDisplay(Packet packet)
		{
			packet.WriteUShort(MapObjectID);
			packet.WriteString(Name, 16);
			packet.WriteInt(Position.X);
			packet.WriteInt(Position.Y);
			packet.WriteByte(Rotation);                // Rotation
			packet.WriteByte((byte)State);          // Player State (1,2 - Player, 3 - Dead, 4 - Resting, 5 - Vendor, 6 - On Mount)
			packet.WriteByte((byte)Job);
			if (State != PlayerState.Resting && State != PlayerState.Vendor && this.House == null)
			{
				WriteLook(packet);
				WriteEquipment(packet);
			}
			else
			{
				this.House.WritePacket(packet);
			}
			WriteRefinement(packet);
            //(IsMale ? 1 : 0)
            int mount = (this.Mount != null) ? (int) this.Mount.Handle : (int)0xffff;
			packet.WriteUShort((ushort)mount);  // Mount Handle
			packet.WriteUShort(0xffff);
			packet.WriteByte(0xff);          // Emote (0xff = nothing)
			packet.WriteUShort(0xffff);
			packet.WriteShort(0);
			packet.WriteUShort(0);             // Mob ID (title = 10)
        
			packet.Fill(55, 0);                // Buff Bits? Something like that
            if (this.Character.GuildID > 1)
            {
                packet.WriteInt(this.Guild.ID);
            }
            else if (this.Character.AcademyID > 0)
            {
                packet.WriteInt(this.Character.AcademyID);
            }
            else
            {
                packet.WriteInt(0);
            }
			packet.WriteByte(0x02);            // UNK (0x02)

			packet.WriteBool(this.IsInaAcademy);            // In Guild Academy (0 - No, 1 - Yes)
			packet.WriteBool(true);            // Pet AutoPickup   (0 - Off, 1 - On)
			packet.WriteByte(this.Level);
		}
            public bool IsInAcademy()
            {
                if (this.Character.AcademyID > 0)
                {
                    return true;
                }
                return false;
            }
		public void WriteRefinement(Packet packet)
		{
			packet.WriteByte(Convert.ToByte(GetUpgradesBySlot(ItemSlot.Weapon) << 4 | GetUpgradesBySlot(ItemSlot.Weapon2)));
			packet.WriteByte(0);    		// UNK
			packet.WriteByte(0);    		// UNK
		}
		public void WriteEquipment(Packet packet)
		{
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Helm));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Weapon));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Armor));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Weapon2));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Pants));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Boots));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.CostumeBoots));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.CostumePants));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.CostumeArmor));
			packet.Fill(6, 0xff);              // UNK
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Glasses));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.CostumeHelm));
			packet.Fill(2, 0xff);              // UNK
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.CostumeWeapon));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Wing));
			packet.Fill(2, 0xff);              // UNK
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Tail));
			packet.WriteUShort(GetEquippedBySlot(ItemSlot.Pet));
		}
		public void WriteDetailedInfo(Packet pPacket)
		{
			pPacket.WriteInt(ID);
			pPacket.WriteString(this.Name, 16);
			pPacket.WriteByte(this.Slot);
			pPacket.WriteByte(this.Level);
			pPacket.WriteLong(this.Exp);
			pPacket.WriteInt(12345678);                // UNK
			pPacket.WriteShort(this.StonesHP);
			pPacket.WriteShort(this.StonesSP);
			pPacket.WriteUInt(this.HP);
			pPacket.WriteUInt(this.SP);
            pPacket.WriteInt(this.Fame);
			pPacket.WriteLong(this.Inventory.Money);
			pPacket.WriteString(this.Map.MapInfo.ShortName, 12);
			pPacket.WriteInt(this.Position.X);
			pPacket.WriteInt(this.Position.Y);
			pPacket.WriteByte(this.Rotation);
			pPacket.WriteByte(this.Str);//  -.  
			pPacket.WriteByte(this.End);//   |   
			pPacket.WriteByte(this.Dex);//   |  Boni
			pPacket.WriteByte(this.Int);//   |   
			pPacket.WriteByte(this.Spr);//  -'   
			pPacket.WriteShort(0);               // UNK
			pPacket.WriteUInt(0);               // Killpoints (TODO)
			pPacket.Fill(7, 0);                 // UNK
		}
		public void WriteLook(Packet packet)
		{
			packet.WriteByte(Convert.ToByte(0x01 | ((byte)Job << 2) | (IsMale ? 1 : 0) << 7));
			packet.WriteByte(this.Hair);
			packet.WriteByte(this.HairColor);
			packet.WriteByte(this.Face);
		}
		public void WriteDetailedInfoExtra(Packet packet, bool levelUP = false)
		{
			if (!levelUP)
			{
				packet.WriteUShort(this.MapObjectID);
			}

			packet.WriteLong(this.Exp);
			packet.WriteULong(DataProvider.Instance.GetMaxExpForLevel(this.Level));

			packet.WriteInt(BaseStats.Strength);
			packet.WriteInt(BaseStats.Strength + GetExtraStr());
			packet.WriteInt(BaseStats.Endurance);
			packet.WriteInt(BaseStats.Endurance + GetExtraEnd());
			packet.WriteInt(BaseStats.Dexterity);
			packet.WriteInt(BaseStats.Dexterity + GetExtraDex());
			packet.WriteInt(BaseStats.Intelligence);
			packet.WriteInt(BaseStats.Intelligence + GetExtraInt());
			packet.WriteInt(0); // Wizdom. It isn't set in the server so it can contain shit from old buffers... :D
			packet.WriteInt(0); // I once had a name here :P
			packet.WriteInt(BaseStats.Spirit);
			packet.WriteInt(BaseStats.Spirit + GetExtraSpr());

			packet.WriteInt(GetWeaponDamage()); //base damage
			packet.WriteInt(GetWeaponDamage(true)); //increased damage (e.g. buffs)
			packet.WriteInt(GetMagicDamage()); //magic dmg
			packet.WriteInt(GetMagicDamage(true)); //inc magic dmg

			packet.WriteInt(GetWeaponDefense()); //todo equip stats loading (weapondef)
			packet.WriteInt(GetWeaponDefense(true)); //weapondef inc

			packet.WriteInt(GetAim()); //TODO: basestats aim + dex?
			packet.WriteInt(GetAim(true)); //aim inc (calcuate later based on dex)

			packet.WriteInt(GetEvasion()); //evasion
			packet.WriteInt(GetEvasion(true)); //evasion inc

			packet.WriteInt(GetWeaponDamage()); //damage block again
			packet.WriteInt(GetWeaponDamage(true));

			packet.WriteInt(GetMagicDamage()); //magic damage
			packet.WriteInt(GetMagicDamage(true));

			packet.WriteInt(GetMagicDefense()); //magic def 
			packet.WriteInt(GetMagicDefense(true)); //magic def inc

			packet.WriteInt(1);
			packet.WriteInt(20);
			packet.WriteInt(2);
			packet.WriteInt(40);

			packet.WriteUInt(BaseStats.MaxHP); //max HP
			packet.WriteUInt(BaseStats.MaxSP); //max SP

			packet.WriteInt(0);                   // UNK
			packet.WriteInt(BaseStats.MaxSoulHP);   // Max HP Stones
			packet.WriteInt(BaseStats.MaxSoulSP);   // Max SP Stones
			packet.Fill(64, 0);
			if (!levelUP)
			{
				packet.WriteInt(this.Position.X);
				packet.WriteInt(this.Position.Y);
			}
		}
        public void UpdateStats()
        {
            CalculateDefense();
            CalculateDamage();
            using (var packet = new Packet(SH4Type.UpdateStats))
            {
                UpdateStatsPacket(packet);
                this.Client.SendPacket(packet);
            }
            // :TODO calcutelate basestates
        }
        private void CalculateDamage()
        {

            ushort id = this.Inventory.GetEquippedBySlot(ItemSlot.Weapon);
            if (id == 0)
            {
                id = this.Inventory.GetEquippedBySlot(ItemSlot.Weapon2); //shield or bow
 
            }

            if (id > 0 && id != ushort.MaxValue)
            {
                ItemInfo weapon;
                if (DataProvider.GetItemInfo(id, out weapon))
                {

                    //dex increases your min damage
                    this.MinDamage =
                        (ushort)(weapon.MinMelee * (1 + this.Str * 0.02) * (1 + this.Dex * 0.01));
                    this.MaxDamage =
                        (ushort)(weapon.MaxMelee * (1 + this.Str * 0.03) * (1 + this.Dex * 0.02));

                    this.MinMagic =
                        (ushort)(weapon.MinMagic * (1 + this.Int * 0.02) * (1 + this.Spr * 0.01));
                    this.MaxMagic =
                        (ushort)(weapon.MaxMagic * (1 + this.Int * 0.03) * (1 + this.Spr * 0.02));
                }
            }
            else
            {
                //:TODO make buffs in defaults

                //make defaul damage
                int dmg = this.BaseStats.Strength / 10; 
                int magicdmg = this.BaseStats.Intelligence / 10;
                this.MinDamage = (ushort)dmg;
                this.MaxDamage = (ushort)dmg;
                this.MinMagic = (ushort)magicdmg;
                this.MaxMagic = (ushort)magicdmg;
            }
        }
        private StatsByte[] statsToUpdate = new[] {
            StatsByte.MinMelee, 
            StatsByte.MaxMelee, 
            StatsByte.MDef, 
            StatsByte.WDef, 
            StatsByte.MinMagic, 
            StatsByte.MaxMagic
        };
        private void CalculateDefense()
        {
            try
            {
                ushort wdef = 0;
                ushort mdef = 0; //TODO magic
                foreach (ItemInfo itemInfo in Inventory.EquippedItems.Select(e => e.ItemInfo))
                {
                    wdef += itemInfo.WeaponDef;
                    mdef += itemInfo.MagicDef;
                }
                this.WeaponDef = wdef;
                this.MagicDef = mdef;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Warn, "Error calculating defense. {0}", ex.Message);
                this.WeaponDef = 1;
                this.MagicDef = 1;
            }
        }
   
  
		public void WriteUpdateStats(Packet packet)
		{
			packet.WriteUInt(this.HP);
			packet.WriteUInt(BaseStats.MaxHP);
			packet.WriteUInt(this.SP);
			packet.WriteUInt(BaseStats.MaxSP);
			packet.WriteByte(this.Level);
			packet.WriteUShort(this.UpdateCounter);
		}
        public  Packet UpdateStatsPacket(Packet packet)
        {
            StatsByte[] pUpdate = this.statsToUpdate;
         
            int lange = pUpdate.Length;
            packet.WriteByte((byte)lange);
            for (int i = 0; i < pUpdate.Length; ++i)
            {
                packet.WriteByte((byte)pUpdate[i]);
				// THIS BUGS.
				packet.WriteInt(this.GetStatValue(pUpdate[i]));
            }
            return packet;
        }
		public int GetStatValue(StatsByte pStat)
		{
			switch(pStat)
			{
				case StatsByte.MinMelee:
					return this.MinDamage;
				case StatsByte.MaxMelee:
					return this.MaxDamage;
				case StatsByte.MinMagic:
					return this.MinMagic;
				case StatsByte.MaxMagic:
					return this.MaxMagic;
				case StatsByte.Aim:
					return this.GetAim();
				case StatsByte.EndBonus:
					return this.EndBonus;
				case StatsByte.StrBonus:
					return this.StrBonus;
				case StatsByte.Evasion:
					return this.GetEvasion();
				case StatsByte.WDef:
					return this.WeaponDef;
				case StatsByte.MDef:
					return this.MagicDef;
				default:
					return 0;
			}
		}
		public int GetMaxHPBuff()
		{
			return Buffs.MaxHP;
		}
		public int GetMaxSPBuff()
		{
			return Buffs.MaxSP;
		}
		public int GetExtraStr()
		{
			return this.Str + Buffs.Str;
		}
		public int GetExtraEnd()
		{
			return this.End + Buffs.End;
		}
		public int GetExtraDex()
		{
			return this.Dex + Buffs.Dex;
		}
		public int GetExtraInt()
		{
			return this.Int + Buffs.Int;
		}
		public int GetExtraSpr()
		{
			return this.Spr + Buffs.Spr;
		}
		public int GetWeaponDamage(bool buffed = false)
		{
			return this.Str + (this.Str % 10) + (buffed ? Buffs.WeaponDamage : 0);
		}
		public int GetMagicDamage(bool buffed = false)
		{
			return 10 + (buffed ? Buffs.MagicDamage : 0);
		}
		public int GetWeaponDefense(bool buffed = false)
		{
			return 10 + (buffed ? Buffs.WeaponDefense : 0);
		}
		public int GetMagicDefense(bool buffed = false)
		{
			return 10 + (buffed ? Buffs.MagicDefense : 0);
		}
		public int GetEvasion(bool buffed = false)
		{
			return 6 + (buffed ? Buffs.Evasion : 0);
		}
		public int GetAim(bool buffed = false)
		{
			return 15;
			//TODO: basestats aim + dex?
			//aim inc (calculate later based on dex)
		}
		public ushort GetEquippedBySlot(ItemSlot pType)
		{
			//double check if found
			Item equip = this.Inventory.EquippedItems.Find(d => d.Slot == (sbyte)pType && d.IsEquipped);
			if (equip == null)
			{
				return 0xffff;
			}
			else
			{
				return (ushort)equip.ID;
			}
		}

		public byte GetUpgradesBySlot(ItemSlot pType)
		{
			//double check if found

			Item equip = this.Inventory.EquippedItems.Find(d => d.Slot == (sbyte)pType && d.IsEquipped);
			if (equip == null)
			{
				return 0;
			}
			else
			{
				return equip.UpgradeStats.Upgrades;
			}
		} 
	 
		public bool GiveItem(Item pItem)
		{
			byte newslot;
			if (Inventory.GetEmptySlot(out newslot))
			{
               /* if (pItem.ItemInfo.Class == ItemClass.Mount)
                {
                    pItem.Mount = DataProvider.Instance.GetMountByItemID(pItem.ID);
                    pItem.Mount.ItemSlot = newslot;
                }*/
				pItem.Slot = (sbyte)newslot;
				pItem.Owner = (uint)this.ID;
				Inventory.AddToInventory(pItem);Handler12.ModifyInventorySlot(this, (byte)pItem.Slot, 0x24, (byte)pItem.Slot, pItem);
				return true;
			}
			else return false;
		}
		public InventoryStatus GiveItem(ushort pID, ushort pCount = (ushort) 1)
		{  // 0 = ok, 1 = inv full, 2 = not found
			ItemInfo inf;
			if (DataProvider.GetItemInfo(pID, out inf))
			{
				byte targetSlot;
				if (!Inventory.GetEmptySlot(out targetSlot))
				{
					return InventoryStatus.Full; //inventory is full
				}

					Item equip = new Item(0,(uint)this.ID, inf.ItemID, (sbyte)targetSlot);
                    equip.UpgradeStats = new UpgradeStats();
					equip.Save();
					Inventory.AddToInventory(equip);
					Handler12.ModifyInventorySlot(this, targetSlot, 0x24, targetSlot, equip);
				return InventoryStatus.Added;
			}
			else
			{
				return InventoryStatus.NotFound;
			}
		}
		public void LootItem(ushort id)
		{
			sbyte freeslot;
			bool gotslot = GetFreeInventorySlot(out freeslot);

			Drop drop;
			if (Map.Drops.TryGetValue(id, out drop))
			{
				if (!drop.CanTake || Vector2.Distance(this.Position, drop.Position) >= 500)
				{
					Handler12.ObtainedItem(this, drop.Item, ObtainedItemStatus.Failed);
					return;
				}
				else if (!gotslot)
				{
					Handler12.ObtainedItem(this, drop.Item, ObtainedItemStatus.InvFull);
					return;
				}
				drop.CanTake = false; //just to be sure
				Map.RemoveDrop(drop);
				Item item = null;
                //fix later
				/*if (drop.Item is DroppedEquip)
				{
                    item = new Item((uint)this.ID, drop.ID, freeslot);
					//item.UniqueID = this.AccountID;
					this.Inventory.EquippedItems.Add((Item)item);
				}
				else
				{
					item = new Item((uint)this.ID, drop.ID, (ushort)drop.Item.Amount);
					this.Inventory.InventoryItems.Add((byte)freeslot, item);
				}*/
				Handler12.ObtainedItem(this, drop.Item, ObtainedItemStatus.Obtained);
				Handler12.ModifyInventorySlot(this, 0x24, (byte)freeslot, 0, item);
			}
		}

		public void DropItemRequest(byte slot)
		{
			Item item;
			if (!this.Inventory.InventoryItems.TryGetValue(slot, out item))
			{
				//TODO: send client 'item not found'
				Log.WriteLine(LogLevel.Warn, "Client tried to drop non-existing object.");
				return;
			}

			if (Question != null)
			{
				Log.WriteLine(LogLevel.Debug, "Client is answering another question. Cannot proceed drop.");
				return;
			}

			Question = new Question("Do you want to discard the item?", OnDropResponse, item);
			Question.Add("Yes", "No");
			Question.Send(this, 500);
		}
		public void UpgradeItem(byte eqpslot, byte stoneslot)
		{
			Item eqpitem, stone;
			if (!this.Inventory.InventoryItems.TryGetValue(eqpslot, out eqpitem) ||
				!this.Inventory.InventoryItems.TryGetValue(stoneslot, out stone))
			{
				Log.WriteLine(LogLevel.Warn, "Invalid item enhancement: item slot does not exist.");
				return;
			}

			Item eqp;
			if ((eqp = eqpitem as Item) == null)
			{
				Log.WriteLine(LogLevel.Warn, "Character tried to upgrade non-equip item.");
				return;
			}

			if (stone.ItemInfo.UpResource == 0)
			{
				Log.WriteLine(LogLevel.Warn, "Character tried to upgrade with non-upgrade item.");
				return;
			}

			byte required = 0;
			if (eqp.UpgradeStats.Upgrades <= 2) required = 2;
			else if (eqp.UpgradeStats.Upgrades <= 5) required = 5;
			else if (eqp.UpgradeStats.Upgrades <= 8) required = 8;
			else required = 10;

			if (stone.ItemInfo.UpResource != required)
			{
				Log.WriteLine(LogLevel.Warn, "Character is using a wrong upgrade stone.");
				return;
			}

			UseOneItemStack(stone);
			int rand = Program.Randomizer.Next(0, 200);
			bool success = rand <= stone.ItemInfo.UpSucRation;

			if (success)
			{
				eqp.UpgradeStats.Upgrades++;
				Handler12.SendUpgradeResult(this, true);
			}
			else
			{
				//TODO: destroy item rate?
				if (eqp.UpgradeStats.Upgrades > 0) --eqp.UpgradeStats.Upgrades;
				Handler12.SendUpgradeResult(this, false);
			}
			Handler12.ModifyInventorySlot(this, 0x24, (byte)eqpslot, (byte)eqpslot, eqp);
		}
		public bool GetFreeInventorySlot(out sbyte value)
		{
			value = -1;
			for (sbyte i = 0; i < 96; i++)
			{
				if (!this.Inventory.InventoryItems.ContainsKey((byte)i))
				{
					value = i;
					return true;
				}
			}
			return false;
		}
		public void DropItem(Item item)
		{
			Drop drop = new Drop(item, this, Position.X, Position.Y, 120);
            if(drop == null) 
            return;
			this.Inventory.InventoryItems.Remove((byte)item.Slot);
			item.Delete();
			Handler12.ModifyInventorySlot(this, 0x24, (byte)item.Slot, 0, null);
			Map.AddDrop(drop);
		}

		private void OnDropResponse(ZoneCharacter character, byte answer)
		{
			Item item = (Item)character.Question.Object;
			switch (answer)
			{
				case 0:
					DropItem(item);
					break;
				case 1:

					break;

				default:
					Log.WriteLine(LogLevel.Warn, "Invalid dropitem response.");
					break;
			}
		}

		public int ChatCheck()
		{
			int currentblock = CheckSpamBlock();
			if (currentblock > 0) return currentblock;
			if (Program.CurrentTime.Subtract(LastChat).TotalSeconds <= ChatDelay)
			{
				ChatBlocked = Program.CurrentTime.AddSeconds(10);
				return 10;
			}
			else
			{
				LastChat = Program.CurrentTime;
				return -1;
			}
		}
		public int ShoutCheck()
		{
			int currentblock = CheckSpamBlock();
			if (currentblock > 0) return currentblock;
			if (Program.CurrentTime.Subtract(LastShout).TotalSeconds <= ShoutDelay)
			{
				ChatBlocked = Program.CurrentTime.AddSeconds(10);
				return 10;
			}
			else
			{
				LastShout = Program.CurrentTime;
				return -1;
			}
		}
		public int CheckSpamBlock()
		{
			DateTime now = Program.CurrentTime;
			if (now < ChatBlocked)
			{
				return (int)ChatBlocked.Subtract(Program.CurrentTime).TotalSeconds;
			}
			else return -1;
		}

		public void LevelUP(ushort mobid = (ushort) 0xFFFF, byte levels = (byte) 1)
		{
			int prevLevel = this.Level;
			byte maxlvl = (byte)DataProvider.Instance.ExpTable.Count;
			if (Level + levels > maxlvl)
			{
				levels = (byte)(maxlvl - Level);
			}
			Level += levels;
			int newLevel = this.Level;

			OnLevelUp(prevLevel, newLevel, mobid);
		}
		private void LevelUpHandleUsablePoints(byte levels)
		{
			Character.UsablePoints += levels;
			Handler4.SendUsablePoints(Client);
		}
		private void SendLevelUpAnimation(ushort pMobId)
		{
			Handler9.SendLevelUPAnim(this, pMobId);
			Handler9.SendLevelUPData(this, pMobId);
		}

		public void SetMap(ushort pMapId, short instance = (short) -1)
		{
			MapInfo info;
			if (DataProvider.Instance.MapsByID.TryGetValue(pMapId, out info))
			{
				Map = MapManager.Instance.GetMap(info);
				if (Map.Block != null)
				{
					if (!Map.Block.CanWalk(Character.PositionInfo.XPos, Character.PositionInfo.YPos))
					{
						Character.PositionInfo.XPos = Map.MapInfo.RegenX;
						Character.PositionInfo.YPos = Map.MapInfo.RegenY;
					}
				}
				Position = new Vector2(Character.PositionInfo.XPos, Character.PositionInfo.YPos);
				Rotation = 0x55; //degrees / 2
				Map.AssignObjectID(this);
			}
			else
			{
				Log.WriteLine(LogLevel.Warn, "Character joined the wrong zone. Map {0} doesn't belong here.", pMapId);
			}
		}
		public void ChangeMap(ushort id, int x = -1, int y = -1, short instance = (short) -1)
		{
			if (id > 120)
			{
				Log.WriteLine(LogLevel.Warn, "Character trying to warp to unexisting map: {0}", id);
				DropMessage("Unable to transfer to this map. Error code 10");
				return;
			}
			ZoneData zci = Program.GetZoneForMap(id);

			if (zci != null)
			{
				var v = zci.MapsToLoad.Find(m => m.ID == id);
				int tox = 0;
				int toy = 0;
				if (x < 0 || y < 0)
				{
					tox = v.RegenX;
					toy = v.RegenY;
				}
				else
				{
					tox = x;
					toy = y;
				}

				// Try setting up transfer
				ushort randomID = (ushort)Program.Randomizer.Next(0, ushort.MaxValue);

				InterHandler.TransferClient(zci.ID, id, this.Client.AccountID, this.Client.Username,this.Character.ID, this.Name, randomID, this.Client.Admin, this.Client.Host);
                ClientTransfer Zonetran = new ClientTransfer(this.Client.AccountID, this.Client.Username,Client.Character.Name, this.Client.Character.ID,randomID,this.Client.Admin,this.Client.Host);
                ClientManager.Instance.AddTransfer(Zonetran);
				Map.RemoveObject(MapObjectID);
				Position.X = tox;
				Position.Y = toy;
				Character.PositionInfo.Map = (byte)id;
				Save();
                InterHandler.SendChangeZoneToWorld(this, id, tox, toy, zci.IP, zci.Port, randomID);
				Handler6.SendChangeZone(this, id, tox, toy, zci.IP, zci.Port, randomID);
			}
			else
			{
				DropMessage("Unable to transfer to this map. Error code 1");
			}
		}
		public void Teleport(int newx, int newy)
		{
			Position.X = newx;
			Position.Y = newy;
			Sector movedin = Map.GetSectorByPos(Position);
			if (movedin != MapSector)
			{
				MapSector.Transfer(this, movedin);
			}
		}
		public void Move(int oldx, int oldy, int newx, int newy, bool walk, bool stop)
		{
			Teleport(newx, newy);

			if (stop)
			{
				using (var packet = Handler8.StopObject(this))
				{
					Broadcast(packet);
				}
			}
			else
			{
				ushort speed = 0;
				if (walk) speed = 60;
				else if(Mount != null) speed = this.Mount.speed;
                else speed = 115;
				foreach (var member in this.Party)
				{
					if (member.Value.Character.Name != this.Character.Name)
					{
						using (var ppacket = new Packet(14, 73))
						{
							ppacket.WriteByte(1);//unk
							ppacket.WriteString(member.Key, 16);
							ppacket.WriteInt(member.Value.Character.Character.PositionInfo.XPos);
							ppacket.WriteInt(member.Value.Character.Character.PositionInfo.XPos);
							member.Value.SendPacket(ppacket);
						}
					}
				}
				using (var packet = Handler8.MoveObject(this, oldx, oldy, walk, speed))
				{
					Broadcast(packet);
				}

				if (this.Group != null)
				{
					this.Group.CharacterMoved(this.GroupMember, oldx, oldy, newx, newy);
				}
			}
		}

		public override void Attack(MapObject victim)
		{
			if (victim == null)
			{
				victim = SelectedObject;
			}

			if (IsAttacking || victim == null || !victim.IsAttackable) return;
			ushort attackspeed = 1200;
			Item weapon;
			this.Inventory.GetEquiptBySlot((byte)ItemSlot.Weapon, out weapon);
			uint dmgmin = (uint)GetWeaponDamage(true);
			uint dmgmax = (uint)(GetWeaponDamage(true) + (GetWeaponDamage(true) % 3));
			if (weapon != null)
			{
				attackspeed = weapon.ItemInfo.AttackSpeed;
				dmgmin += weapon.ItemInfo.MinMelee;
				dmgmax += weapon.ItemInfo.MaxMelee;
			}

			base.Attack(victim);
			attackingSequence = new AttackSequence(this, victim, dmgmin, dmgmax, attackspeed);
		}
		public override void AttackSkill(ushort skillid, MapObject victim)
		{
			if (victim == null)
			{
				victim = SelectedObject;
			}

			if (IsAttacking || victim == null || !victim.IsAttackable) return;

			Item weapon;
			this.Inventory.GetEquiptBySlot((byte)ItemSlot.Weapon, out weapon);
			uint dmgmin = (uint)GetWeaponDamage(true);
			uint dmgmax = (uint)(GetWeaponDamage(true) + (GetWeaponDamage(true) % 3));
			if (weapon != null)
			{
				dmgmin += weapon.ItemInfo.MinMelee;
				dmgmax += weapon.ItemInfo.MaxMelee;
			}

			attackingSequence = new AttackSequence(this, victim, dmgmin, dmgmax, skillid, true);
		}
		public override void AttackSkillAoE(ushort skillid, uint x, uint y)
		{
			if (IsAttacking) return;

			Item weapon;
			this.Inventory.GetEquiptBySlot((byte)ItemSlot.Weapon, out weapon);
			uint dmgmin = (uint)GetExtraStr();
			uint dmgmax = (uint)(GetExtraStr() + (GetExtraStr() % 3));
			if (weapon != null)
			{
				dmgmin += weapon.ItemInfo.MinMelee;
				dmgmax += weapon.ItemInfo.MaxMelee;
			}

			attackingSequence = new AttackSequence(this, dmgmin, dmgmax, skillid, x, y);
		}

		public override void Damage(MapObject bully, uint amount, bool isSP = false)
		{
			base.Damage(bully, amount, isSP);
			if (IsDead)
			{
				State = PlayerState.Dead;
				Handler4.SendReviveWindow(this.Client, 3);
			}
		}
		public override string ToString()
		{
			return "ZoneCharacter(" + this.Name + " | " + this.ID + ")";
		}

		#region Event-Invoker

		protected virtual void OnLevelUp(int pOldLevel, int pNewLevel, ushort pMobId)
		{
			SendLevelUpAnimation(pMobId);
			Heal();
            InterHandler.SendLevelUpToWorld((byte)pNewLevel, this.Character.Name);
			LevelUpHandleUsablePoints((byte) (pNewLevel - pOldLevel));
			if(LevelUp != null)
				LevelUp(this, new LevelUpEventArgs(pOldLevel, pNewLevel, pMobId));
			if (this.Group != null)
				this.Group.UpdateCharacterLevel(this);
		}
		protected override void OnHpSpChanged()
		{
			base.OnHpSpChanged();
			if (this.Group != null)
				this.Group.UpdateCharacterHpSp(this);
		}

		#endregion
		#endregion
		#region Events
		public event EventHandler<LevelUpEventArgs> LevelUp;
		#endregion
	}
}