namespace MyInventory
{
    public abstract class Item
    {
        public ItemData Data { get; private set; }

        protected Item(ItemData data) => Data = data;
    }
}