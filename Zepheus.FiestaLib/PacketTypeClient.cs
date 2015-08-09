
namespace Zepheus.FiestaLib
{
    public enum CH2Type : byte
    {
        Pong = 5,
        Unk1 = 13,
    }

    public enum CH3Type : byte
    {
        Version = 1,
        Login = 56,
        WorldReRequest = 27,
        FileHash = 4,
        WorldSelect = 11,
        //Actually used in World
        WorldClientKey = 15,
        BackToCharSelect = 24,
    }

    public enum CH4Type : byte
    {
        CharSelect = 1,

        ReviveToTown = 78,
        SetPointOnStat = 92,
    }

    public enum CH5Type : byte
    {
        CreateCharacter = 1,
        ChangeCharacterName = 5,
        DeleteCharacter = 7,
    }

    public enum CH6Type : byte
    {
        TransferKey = 1,
        ClientReady = 3,
        Teleporter = 26,
    }

    public enum CH7Type : byte
    {
        UnknownSomethingWithMobs = 1,
    }
    public enum CH14Type : byte
    {
        PartyAccept = 4,
        PartyDecline= 5, 
        PartyReqest = 2,
        PartyLeave = 10,
        PartyMaster = 84,
        PartyInviteGame = 72,   // no data
        ChangePartyMaster = 40,
        ChangePartyDrop = 75,
        KickPartyMember = 20,

    }
    public enum CH8Type : byte
    {
        By = 29,
        ByCancel = 11,
        WisperTo = 12,
        ChatNormal = 1,
        ChatParty = 20,
        BeginInteraction = 10,
        Stop = 18,
        Walk = 23,
        Run = 25,
        Shout = 30,
        Emote = 32,
        Jump = 36,
        BeginRest = 39,
        EndRest = 42,
    }

    public enum CH9Type : byte
    {
        SelectObject = 1,
        DeselectObject = 8,

        AttackEntityMelee = 43,

        StopAttackingMelee = 50,

        AttackEntitySkill = 61,
        UseSkillWithTarget = 64,
        UseSkillWithPosition = 65,
    }

    //items
    public enum CH12Type : byte
    {
        BuyItem = 3,
        SellItem = 6,
        DropItem = 7,
        LootItem = 9,
        MoveItem = 11,
        Equip = 15,
        Unequip = 18,
        UseItem = 21,
        ItemEnhance = 23,
        GetPremiumItemList = 32,
        GetRewardItemList = 44,
        TakeGuildMoney = 47,
        GiveGuildMoney = 49,
    }

    public enum CH15Type : byte
    {
        AnswerQuestion = 2,
    }
    public enum CH19Type : byte
    {
        TradeReqest = 1,
        TradeReqestDecline = 3,
        TradeAccept = 6,
        TradeBreak = 10,
        TradeAddItem = 13,
        TradeRemoveItem = 17,
        TradeChangeMoney = 21,
        TradeLock = 25,
        TradeAgree = 31,
    }
    public enum CH20Type : byte
    {
        ByHPStone = 1,
        BySPStone = 2,
        UseHPStone = 7,
        UseSPStone = 9,
    }
    public enum CH21Type : byte
    {
        FriendInvite = 1,
        FriendInviteResponse = 4,
        FriendListDelete = 5,
    }
    public enum CH22Type : byte
    {
        GotIngame = 27,
    }
    public enum CH28Type : byte
    {
        GetQuickBar = 2,
        GetQuickBarState = 4,
        GetGameSettings = 10,
        GetClientSettings = 12,
        GetShortCuts = 14,

        SaveQuickBar = 16,
        SaveQuickBarState = 17,
        SaveGameSettings = 20,
        SaveClientSettings = 21,
        SaveShortCuts = 22,
    }

    public enum CH29Type : byte
    {
        GetGuildList = 3,
        CreateGuild = 5,
        GuildInviteRequest = 9,
        GuildInviteResponse = 12,
        UpdateGuildMessage = 16,
        UpdateGuildMemberRank = 22,
        LeaveGuild = 28,
        GuildChat = 115,
        GuildNameRequest = 118,
        GuildMemberListRequest = 190,

    }

    public enum CH31Type : byte
    {
        GetUnknown = 6,
    }
    public enum CH37Type : byte
    {
        MasterRequest = 1,
        MasterRequestResponse = 5,
        RemoveMasterByApprentice = 6,
        MasterRemove = 10,
        MasterRewardCoperRquest= 60,
        SendReciveCoperAccept = 64,
    }

    public enum CH38Type : byte
    {
        GetAcademyMemberList = 7,
        GetAcademyList = 11,
        JoinAcademy = 17,
        LeaveAcademy = 27,
        JumpToMember = 31,
        BlockAcademyChat = 33,
        UpdateDetails = 36,
        ChangeRequestToGuild = 44,
        GetAcademyGoldRewardList = 49,
        AcademyChat = 104,
        GuildExtraRequest = 109,
    }
    public enum CH42Type : byte
    {
        AddToBlockList = 3,
        RemoveFromBlockList = 7,
        ClearBlockList = 11,
    }
}
