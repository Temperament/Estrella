using Zepheus.Zone.Game;

namespace Zepheus.Zone.Game
{
    public class GroupMember
    {
        #region .ctor

        public GroupMember()
        {
            IsMaster = false;
            IsOnline = false;
            Group = null;
            Name = "";
            Character = null;
            IsReadyForUpdates = false;
        }
        public GroupMember(string pName, bool pIsMaster, bool pIsOnline)
        {
            this.Name = pName;
            this.IsMaster = pIsMaster;
            this.IsOnline = pIsOnline;
        }

        #endregion
        #region Properties

        public bool IsMaster { get; internal set; }
        public bool IsOnline { get; set; }
        public Group Group { get; internal set; }
        public string Name { get; internal set; }
        public ZoneCharacter Character { get; internal set; }

        public bool IsReadyForUpdates { get; set; }

        #endregion
    }
}
