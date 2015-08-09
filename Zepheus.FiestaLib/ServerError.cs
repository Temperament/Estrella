
namespace Zepheus.FiestaLib
{
/* Error codes
* 0x42 - Unkown Error
* 0x43 - DB Error
* 0x44 - Auth Failed
* 0x45 - Please check ID or password
* 0x46 - DB Error
* 0x47 - The ID has been blocked
* 0x48 - World server maintenance
* 0x49 - Timeout
* 0x4a - Login Failed
* 0x4b - Please accept the agreement. */
    public enum ServerError : ushort
    {
        InvalidCredentials =    0x45,
        DatabaseError =         0x46,
        Exception =             0x42,
        Blocked =               0x47,
        ServerMaintenance =     0x48,
        Timeout =               0x49,
        AgreementMissing =      0x4b,
        WrongRegion =           0x44,
    }
}
