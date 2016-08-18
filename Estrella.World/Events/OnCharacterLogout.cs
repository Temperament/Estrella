using System;
using Estrella.World.Data;


namespace Estrella.World.Events
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
