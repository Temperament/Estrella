using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zepheus.World.Data;
namespace Zepheus.World.Events
{
    public class OnCharacterLoginArgs : EventArgs
    {
        public OnCharacterLoginArgs(WorldCharacter pChar,OnCharacterLoginArgs args)
        {
        }
    }
}
