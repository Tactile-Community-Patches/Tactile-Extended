using System.IO;

namespace TactileLibrary
{
    public class ShopItemData : Item_Data
    {
        public ShopItemData(Item_Data_Type type, int id, int uses)
            : base(type, id, uses) { }
        public ShopItemData(int type, int id, int uses)
            : base(type, id, uses) { }
        public ShopItemData()
            : base() { }

        public override void consume_use()
        {
            Uses--;
        }
        public void add_stock()
        {
            Uses++;
        }
    }
}
