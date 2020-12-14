using System;

namespace Server.Inventory
{
    public class GameItem
    {
        public string ID;
        public string Name;
        public string Description;

        public float Weight;

        [Obsolete("Don't use this, use GetTotalCapacity instead. InventoryItem.GetTotalCapacity")]
        public float Capacity;

        public int Price;
        public bool Stackable;

        public GameItem(string iD, string name, string description, float weight, float capacity, int price, bool stackable)
        {
            ID = iD;
            Name = name;
            Description = description;
            Stackable = stackable;
            Price = price;
            Weight = weight;
        }
    }
}