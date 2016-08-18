using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Estrella.World.Data;
namespace Estrella.World.Events
{
    public class OnCharacterLoginArgs : EventArgs
    {
        public OnCharacterLoginArgs(WorldCharacter pChar,OnCharacterLoginArgs args)
        {
        }
    }
}
