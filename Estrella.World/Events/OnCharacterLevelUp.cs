using System;
using Estrella.World.Data;


namespace Estrella.World.Events
{
    public class OnCharacterLevelUpArgs : EventArgs
    {
        public delegate void DelegatetType(WorldCharacter pChar);
        public WorldCharacter PCharacter { get; set; }

        public OnCharacterLevelUpArgs(WorldCharacter pChar = null)
        {
            this.PCharacter = pChar;
        }
        public OnCharacterLevelUpArgs()
        {
        }
    }
}
