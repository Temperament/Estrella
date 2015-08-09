using System;
using Zepheus.Zone.Game;

namespace Zepheus.Zone.Managers
{
   public  delegate void CharacterEvent(ZoneCharacter Character);
   public class CharacterManager
    {
       public static event CharacterEvent OnCharacterLogin;

       public static void InvokeCharacterLogin(ZoneCharacter pChar)
       {
           OnCharacterLogin.Invoke(pChar);
       }
       public static bool GetLoggedInCharacter(int ID, out ZoneCharacter pChar)
       {
           pChar = ClientManager.Instance.GetClientByCharID(ID).Character;
           if (pChar != null)
           {
               return true;
           }
           return false;
       }
    }
}
