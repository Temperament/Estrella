using System;
using System.Collections.Generic;
using System.Linq;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;
using Zepheus.Zone.Data;
using Zepheus.Zone.Game;
using Zepheus.Zone.Handlers;
using Zepheus.Zone.InterServer;
using Zepheus.Zone.Networking;
using Zepheus.FiestaLib;

namespace Zepheus.Zone
{
    [ServerModule(Util.InitializationStage.Metadata)]
    public sealed class CommandHandler
    {
        public static CommandHandler Instance { get; private set; }
        public delegate void Command(ZoneCharacter character, params string[] param);
        private readonly Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();

        public CommandHandler()
        {
            LoadCommands();
            Log.WriteLine(LogLevel.Info, "{0} command(s) registered.", commands.Count);
        }

        public void LoadCommands()
        {
            RegisterCommand("&Inv", Inv, 1);
            RegisterCommand("&GiveMoney",GiveMoney,1,"<long>");
            RegisterCommand("&ChangeMoney", ChangeMoney, 1,"NewMoney");
            RegisterCommand("&adminlevel", AdminLevel, 1);
            RegisterCommand("&map", ChangeMap, 1, "mapid", "x", "y");
            RegisterCommand("&pos", Pos, 1);
            RegisterCommand("&p", WritePacket, 1, "header", "type");
            RegisterCommand("&command", CommandParam, 1, "command");
            RegisterCommand("&item", ItemCommand, 1, "ID", "amount");
            RegisterCommand("&resetinv", ResetInv, 1, "slot");
            RegisterCommand("&isg@y", IsGay, 1, "charname");
            RegisterCommand("&spawnmob", SpawnMob, 1, "id");
            RegisterCommand("&spawnmobt", SpawnMobT, 1, "id", "amount");
            RegisterCommand("&createmobspawn", CreateMobSpawn, 1, "id");
            RegisterCommand("&savemobspawns", SaveMobSpawns, 1, "mapname/all");
            RegisterCommand("&heal", Heal, 1);
            RegisterCommand("&gg", Testg, 1);
            RegisterCommand("&clone", Clone, 1);
            RegisterCommand("&test99", Test99, 1);
            RegisterCommand("&damageme", DamageMe, 1, "HP");
            RegisterCommand("&test", Test, 1, "SkillID");
            RegisterCommand("&list", List, 1);
            RegisterCommand("&killallmobs", KillAllMobs, 1);
            RegisterCommand("&levelup", LevelUP, 1, "{levels}");
            RegisterCommand("&expget", ExpGet, 1, "amount");
            RegisterCommand("&anim", Anim, 1, "animid");
            RegisterCommand("&animall", AnimAll, 1, "animid");
            RegisterCommand("&perf", Performance, 1);
            RegisterCommand("&allm", Allm, 1);
            RegisterCommand("&Reward", TestReward, 1, "ItemID", "count");
            RegisterCommand("&movetome", Movetome, 1,"playername");
            RegisterCommand("&movetoplayer", Movetoplayer, 1,"playername");
            RegisterCommand("&NpcInfo", NpcInfo, 1);
            RegisterCommand("&Ban", Ban, 1, "Charname");
        }
        private void TestReward(ZoneCharacter chr, params string[] param)
        {
             ushort ItemID = ushort.Parse(param[1]);
             byte count = byte.Parse(param[2]);
             chr.GiveMasterRewardItem(ItemID, count);
         
        }
        private void NpcInfo(ZoneCharacter chr, params string[] param)
        {
            if (chr.CharacterInTarget is Npc)
            {
                Npc Targetnpc = chr.CharacterInTarget as Npc;

                SendMessageChat(chr, "NpcID :" + Targetnpc.ID);
                SendMessageChat(chr, "NpcHP :" + Targetnpc.HP);
                SendMessageChat(chr, "NpcSP :" + Targetnpc.SP);
                SendMessageChat(chr, "NpcMaxHP :" + Targetnpc.MaxHP);
                SendMessageChat(chr, "NpcMaxSP :" + Targetnpc.MaxSP);
                SendMessageChat(chr, "NpCPositionx :" + Targetnpc.Position.X);
                SendMessageChat(chr, "NpCPositionY :" + Targetnpc.Position.Y);
                SendMessageChat(chr, "NpRot :" + Targetnpc.Rotation);
                SendMessageChat(chr, "NpCFlags :" + Targetnpc.Point.Flags);
            }
        }
        private void SendMessageChat(ZoneCharacter character, string chat)
        {
            lock (chat)
                using (var packet = new Packet(SH8Type.ChatNormal))
                {
                    Npc ne = character.CharacterInTarget as Npc;
                    packet.WriteUShort(ne.MapObjectID);
                    packet.WriteByte((byte)chat.Length);
                    packet.WriteByte(0x03);
                    packet.WriteString(chat, chat.Length);
                    character.Client.SendPacket(packet);
                }
        }
        private void Ban(ZoneCharacter Char, params string[] param)
        {
            string player = param[1];
            ZoneClient playerc = ClientManager.Instance.GetClientByName(player);
            if (playerc != null)
            {
                playerc.Character.Ban();
            }
        }
        private void Movetoplayer(ZoneCharacter character, params string[] param)
        {
            string player = param[1];
            ZoneClient playerc = ClientManager.Instance.GetClientByName(player);
            character.ChangeMap(playerc.Character.MapID, playerc.Character.Position.X, character.Position.Y);
        }
        private void GiveMoney(ZoneCharacter character, params string[] param)
        {
            long givedm = long.Parse(param[1].ToString());
            character.ChangeMoney(character.Inventory.Money+= givedm);
        }
        private void ChangeMoney(ZoneCharacter character, params string[] param)
        {
            long newMoney = long.Parse(param[1].ToString());
            character.ChangeMoney(newMoney);
        }
        private void Movetome(ZoneCharacter character, params string[] param)
        {
            string player = param[1];
          ZoneClient playerc =  ClientManager.Instance.GetClientByName(player);
          playerc.Character.ChangeMap(character.MapID, character.Position.X, character.Position.Y);
        }
        private void Allm(ZoneCharacter character, params string[] param)
        {
            Console.WriteLine(character.IsInParty);
            Console.WriteLine(character.HealthThreadState);
            foreach (var chaj in character.Party)
            {
                Console.WriteLine("key:"+chaj.Key+"char"+chaj.Value.Character.Name+"");
                foreach (var iw in chaj.Value.Character.Party)
                {
                    Console.WriteLine("key:" + iw.Key + "char" + iw.Value.Character.Name + "");
                }
            }
            //character.Party.Clear();
        }
        public void CanWald(ZoneCharacter @char, params string[] param)
        {
        }
        private void Inv(ZoneCharacter character, params string[] param)
        {
            using (var packet = new Packet(8,67))
            {
               
                packet.WriteUShort(23);
                character.Client.SendPacket(packet);
            }
            using (var packet = new Packet(8, 64))
            {

                packet.WriteUShort(23);
                packet.WriteUShort(0);
                character.Client.SendPacket(packet);
            }
        }
        private void Test(ZoneCharacter character, params string[] param)
        {
            ushort skillid = ushort.Parse(param[1]);
            if (!DataProvider.Instance.ActiveSkillsByID.ContainsKey(skillid))
            {
                character.DropMessage("Unknown skill.");
            }
            else if (character.SkillsActive.ContainsKey(skillid))
            {
                character.DropMessage("You already have that skill");
            }
            else
            {
                character.SkillsActive.Add(skillid, new Skill(character, skillid));
                Handler18.SendSkillLearnt(character, skillid);
            }
        }

        private void Anim(ZoneCharacter character, params string[] param)
        {
            byte animid = param.Length >= 2 ? byte.Parse(param[1]) : (byte)50;
            using (var broad = Handler8.Animation(character, animid))
            {
                character.Broadcast(broad, true);
            }
        }
        private void Test99(ZoneCharacter character, params string[] param)
        {
            using (var packet = new Packet(SH14Type.UpdatePartyMemberStats))
            {
                packet.WriteByte(1);//unk
                packet.WriteString("Stan", 16);
                packet.WriteUInt(character.HP);
                packet.WriteUInt(character.SP);
                character.Client.SendPacket(packet);
            }
            using (var ppacket = new Packet(SH14Type.SetMemberStats))//when character has levelup in group
            {
                ppacket.WriteByte(1);
                ppacket.WriteString("Stan", 16);
                ppacket.WriteByte((byte)character.Job);
                ppacket.WriteByte(character.Level);
                ppacket.WriteUInt(character.MaxHP);//maxhp
                ppacket.WriteUInt(character.MaxSP);//MaxSP
                ppacket.WriteByte(1);
                character.Client.SendPacket(ppacket);

            }
        }
        private void Performance(ZoneCharacter character, params string[] param)
        {
            character.DropMessage("Ticks per second on this zone: {0}", Worker.Instance.TicksPerSecond);
        }

        private void AnimAll(ZoneCharacter character, params string[] param)
        {
            byte animid = param.Length >= 2 ? byte.Parse(param[1]) : (byte)50;
            foreach (var ch in character.Map.Objects.Values.Where(o => o is ZoneCharacter))
            {
                var obj = ch as ZoneCharacter;
                using (var broad = Handler8.Animation(obj, animid))
                {
                    obj.Broadcast(broad, true);
                }
            }
        }

        private void LevelUP(ZoneCharacter character, params string[] param)
        {
        
            byte lvls = param.Length >= 2 ? byte.Parse(param[1]) : (byte)1;
            for (byte i = 0; i < lvls; i++)
            {
                character.LevelUP();
            }
        }

        private void ExpGet(ZoneCharacter character, params string[] param)
        {
            uint exp = param.Length >= 2 ? uint.Parse(param[1]) : 1234567;
            character.GiveExp(exp);
        }

        private void KillAllMobs(ZoneCharacter character, params string[] param)
        {
            ushort dmg = (ushort)(param.Length >= 2 ? ushort.Parse(param[1]) : 900001);
            foreach (MapObject obj in character.Map.Objects.Values.Where(o => o is Mob))
            {
                var mob = obj as Mob;
                Handler9.SendAttackDamage(character, mob.MapObjectID, dmg, true, 0, obj.UpdateCounter);
                mob.Damage(null, dmg);
            }
        }

        private void List(ZoneCharacter character, params string[] param)
        {
            character.DropMessage("Your l33t commands:");
            foreach (var kvp in commands)
            {
                if (kvp.Value.GmLevel <= character.Client.Admin)
                {
                    character.DropMessage("{0} {1}", kvp.Key, string.Join(" ", kvp.Value.Parameters));
                }
            }
        }

        private void DamageMe(ZoneCharacter character, params string[] param)
        {
            uint value = uint.Parse(param[1]);
            character.Damage(value);
        }

        private void Heal(ZoneCharacter character, params string[] param)
        {
            character.Heal();
        }

        private void IsGay(ZoneCharacter character, params string[] param)
        {
            ZoneClient otherclient = ClientManager.Instance.GetClientByName(param[1]);
            if (otherclient == null || otherclient.Character == null)
            {
                character.DropMessage("Character not found.");
            }
            ZoneCharacter other = otherclient.Character;

            Question question = new Question("Are you gay?", new QuestionCallback(AnswerGay), character);
            question.Add("Yes", "No", "Boobs!");
            other.Question = question;
            question.Send(other);
        }

        private void AnswerGay(ZoneCharacter character, byte answer)
        {
            ZoneCharacter other = (ZoneCharacter)character.Question.Object;
            switch (answer)
            {
                case 0:
                    other.DropMessage("{0} admitted he's gay.", character.Name);
                    break;

                case 1:
                    other.DropMessage("{0} says he isn't gay, wouldn't trust him though...", character.Name);
                    break;

                case 2:
                    other.DropMessage("{0} watches too much porn, better ban him...", character.Name);
                    break;
                default:
                    Log.WriteLine(LogLevel.Warn, "Invalid gay question response.");
                    break;
            }
        }

        private void ResetInv(ZoneCharacter character, params string[] param)
        {
            byte slot = byte.Parse(param[1]);
            Handler12.ResetInventorySlot(character, slot);
        }

        private void ItemCommand(ZoneCharacter character, params string[] param)
        {
            ushort id = ushort.Parse(param[1]);
            byte amount = 1;
            if (param.Length > 2)
            {
                amount = byte.Parse(param[2]);
            }
          // character.GiveItem(id,amount);
            switch (character.GiveItem(id, amount))
            {
                case FiestaLib.InventoryStatus.Full:
                    Handler12.InventoryFull(character);
                    return;
                case FiestaLib.InventoryStatus.NotFound:
                    character.DropMessage("Item not found.");
                    return;
            }
        }

        private void Pos(ZoneCharacter character, params string[] param)
        {
            character.DropMessage("Map= {0} ({1}); Pos= {2}:{3};",
                character.Map.MapID,
                character.Map.MapInfo.FullName,
                character.Position.X,
                character.Position.Y);
        }

        private void CreateMobSpawn(ZoneCharacter character, params string[] param)
        {
            if (param.Length == 2)
            {
                character.Map.MobBreeds.Add(MobBreedLocation.CreateLocationFromPlayer(character, ushort.Parse(param[1])));
            }
        }

        private void SaveMobSpawns(ZoneCharacter character, params string[] param)
        {
            if (param.Length == 2)
            {
                if (param[1] == "current")
                {
                    Program.DatabaseManager.GetClient().ExecuteQuery("DELETE FROM `" + Settings.Instance.zoneMysqlDatabase + "`.`Mobspawn` WHERE MapID='" + character.MapID + "'");
                    character.Map.SaveMobBreeds();
                }
                else if (param[1] == "all")
                {
                    Program.DatabaseManager.GetClient().ExecuteQuery("DELETE FROM `" + Settings.Instance.zoneMysqlDatabase + "`.`Mobspawn`");
                    foreach (var val in MapManager.Instance.Maps)
                    {
                        if (val.Value.Count > 0)
                        { 

                            val.Value[0].SaveMobBreeds();
                        }
                    }
                }
                else
                {
                    param[1] = param[1].ToLower();
                    foreach (var val in MapManager.Instance.Maps)
                    {
                        if (val.Key.ShortName.ToLower() == param[1] && val.Value.Count > 0)
                        {
                            val.Value[0].SaveMobBreeds();
                        }
                    }
                }
            }
        }

        private void SpawnMob(ZoneCharacter character, params string[] param)
        {
            ushort id = ushort.Parse(param[1]);
            if (DataProvider.Instance.MobsByID.ContainsKey(id))
            {
                Mob mob = new Mob((ushort)(param.Length == 2 ? id : (ushort) 1045), new Vector2(character.Position));
                character.Map.FullAddObject(mob);
            }
            else character.DropMessage("Monster ID not found.");
        }

        private void SpawnMobT(ZoneCharacter character, params string[] param)
        {
            ushort id = ushort.Parse(param[1]);
            int count = 1;
            if (param.Length >= 3)
            {
                count = int.Parse(param[2]);
            }
            if (DataProvider.Instance.MobsByID.ContainsKey(id))
            {
                for (int i = 0; i < count; i++)
                {
                    Mob mob = new Mob(id, new Vector2(character.Position));
                    character.Map.FullAddObject(mob);
                }
            }
            else character.DropMessage("Monster ID not found.");
        }

        private void CommandParam(ZoneCharacter character, params string[] param)
        {
            string input = param[1];
            string request;
            if (!input.StartsWith("&"))
            {
                request = "&" + input;
            }
            else request = input;
            CommandInfo info;
            if (commands.TryGetValue(request, out info))
            {
                string output = request + ": ";
                if (info.Parameters.Length > 0)
                {
                    foreach (var par in info.Parameters)
                    {
                        output += "[" + par + "] ";
                    }
                }
                else output += "None";
                character.DropMessage(output);
            }
            else character.DropMessage("Command not found.");
        }
        private void Clone(ZoneCharacter character, params string[] param)
        {
        }
        private void Testg(ZoneCharacter character, params string[] param)
        {
            //05 58 00 04 0A 7A 0F 00 90 2B
            using (var pack = new Packet(SH38Type.AcademyMemberJoined))
            {
                pack.WriteString("invider", 16);
                pack.WriteString("Create", 16);
                pack.WriteByte(6);//rank
                pack.WriteInt(0);//unk
                pack.WriteUShort(9000);//korp
                pack.Fill(64, 0x00);//unk
                pack.WriteByte(95);// (this.isOnline ? (byte)0x95 : (byte)0x00);
                pack.Fill(3, 0x00);//unk
                pack.WriteByte(21);
                pack.WriteByte(255);
                pack.Fill(13, 0x00);//unk
                character.Client.SendPacket(pack);
            }

        }
        private void ChangeMap(ZoneCharacter character, params string[] param)
        {
            ushort mapid = 0;
            if (!ushort.TryParse(param[1], out mapid))
            {
                param[1] = param[1].ToLower();
                var map = DataProvider.Instance.MapsByID.Values.ToList().Find(m => m.ShortName.ToLower() == param[1]);
                if (map != null)
                {
                    mapid = map.ID;
                }
                else
                {
                    character.DropMessage("Map not found");
                    return;
                }
            }
            else
            {
                if (!DataProvider.Instance.MapsByID.ContainsKey(mapid))
                {
                    character.DropMessage("Map not found");
                    return;
                }
            }

            if (param.Length > 2)
            {
                int x = int.Parse(param[2]);
                int y = int.Parse(param[3]);
                character.ChangeMap(mapid, x, y);
            }
            character.ChangeMap(mapid);

        }

        private void WritePacket(ZoneCharacter character, params string[] param)
        {
            if (param.Length >= 3)
            {
                byte header = byte.Parse(param[1]);
                byte type = byte.Parse(param[2]);

                using (var packet = new Packet(header, type))
                {
                    if (param.Length > 3)
                    {
                        packet.WriteHexAsBytes(string.Join("", param, 3, param.Length - 3));
                    }
                    character.Client.SendPacket(packet);
                }
            }
        }

        private void AdminLevel(ZoneCharacter character, params string[] param)
        {
            Log.WriteLine(LogLevel.Debug, "GM {0} authenticated ingame.", character.Name);
            character.DropMessage("Admin level is {0}", character.Client.Admin);
            InterHandler.SendWorldMessage(FiestaLib.WorldMessageTypes.Level20, "Fun!");
        }

        public void RegisterCommand(string command, Command function, byte gmlevel, params string[] param)
        {
            if (commands.ContainsKey(command))
            {
                Log.WriteLine(LogLevel.Warn, "{0} already registered as a command.", command);
                return;
            }
            CommandInfo info = new CommandInfo(command.ToLower(), function, gmlevel, param);
            commands.Add(command.ToLower(), info);
        }

        public string[] GetCommandParams(string command)
        {
            CommandInfo info;
            if (commands.TryGetValue(command, out info))
            {
                return info.Parameters;
            }
            else return null;
        }

        public CommandStatus ExecuteCommand(ZoneCharacter character, string[] command)
        {
            if (character == null) return CommandStatus.Error;
            CommandInfo info;
            if (commands.TryGetValue(command[0].ToLower(), out info))
            {
                if (info.GmLevel > character.Client.Admin)
                {
                    return CommandStatus.GMLevelTooLow;
                }
                else
                {
                    try
                    {
                        info.Function(character, command);
                        return CommandStatus.Done;
                    }
                    catch (Exception ex)
                    {
                        string wholeCommand = string.Join(" ", command);
                        Log.WriteLine(LogLevel.Exception, "Exception while handling command '{0}': {1}", wholeCommand, ex.ToString());
                        return CommandStatus.Error;
                    }
                }
            }
            else return CommandStatus.NotFound;
        }

        [InitializerMethod]
        public static bool Load()
        {
            Instance = new CommandHandler();
            return true;
        }
    }
}
