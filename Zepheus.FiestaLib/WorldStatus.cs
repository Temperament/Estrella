
namespace Zepheus.FiestaLib
{
    public enum WorldStatus : byte
    {
        // With messages:
        Offline = 0,        // Server is closed. Please try login later.
        Maintenance = 1,    // Server is under maintenance. Please try login later.
        Emptyserver = 2,    // You cannot connect to an empty server.
        Reserved = 3,       // The server has been reserved for a special use.
        Offlineunkerror = 4, // Login failed due to an unknown error.
        Full = 5,           // Server is full. Please try again later.
        Low = 6,
        // Low = 7,
        // Low = 8,
        Medium = 9,
        High = 10,
    }
}
