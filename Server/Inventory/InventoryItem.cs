using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Server.Inventory
{
    public class InventoryItem
    {
        [JsonIgnore]
        private GameItem item;

        [JsonIgnore]
        public GameItem ItemInfo => item ??= GameWorld.GetGameItem(Id);

        /// <summary>
        /// Unique Id of the Item
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of Item
        /// </summary>
        public string CustomName { get; set; }

        /// <summary>
        /// string value of Item
        /// </summary>
        public string ItemValue { get; set; }

        /// <summary>
        /// Quantity of Items
        /// </summary>
        public double Quantity { get; set; }

        [JsonIgnore]
        public List<string> QuantityListString
        {
            get
            {
                List<string> stringList = new List<string>();

                for (int i = 1; i <= Quantity; i++)
                {
                    stringList.Add(i.ToString());
                }

                return stringList;
            }
        }

        public InventoryItem(string id, string customName, string itemValue = null, double quantity = 1)
        {
            Id = id;
            CustomName = customName;
            ItemValue = itemValue;
            Quantity = quantity;
        }

        public float GetTotalWeight(double quantity = 1) => (float)Math.Round(ItemInfo.Weight * quantity, 2);

        //public float GetTotalCapacity
        //{
        //    get
        //    {
        //        return ItemInfo.Capacity * Quantity;
        //    }
        //}

        public string GetName(bool showQuantity)
        {
            string name = string.IsNullOrWhiteSpace(CustomName) ? ItemInfo.Name : CustomName;
            if (Quantity > 1 && showQuantity)
            {
                name += " (" + Quantity + ")";
            }

            return name;
        }
    }
}