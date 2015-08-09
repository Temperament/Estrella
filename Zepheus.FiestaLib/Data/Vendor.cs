namespace Zepheus.FiestaLib.Data
{
   public class Vendor
    {
       public string VendorName { get; set; } //latter ids
       public ItemInfo Item { get; set; }
       public ushort ItemID { get; set; }
       public byte InvSlot { get; set; }

    }
}
