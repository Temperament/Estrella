using System;
using Zepheus.World.Data;


namespace Zepheus.World.Events
{
    public class OnCharacterLogoutArgs : EventArgs
    {
        public WorldCharacter PCharacter { get; set; }

        public OnCharacterLogoutArgs(WorldCharacter pChar)
        {
            this.PCharacter = pChar;
        }
    }
}
