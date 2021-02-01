using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AltV.Net.Elements.Entities;
using Elasticsearch.Net.Specification.WatcherApi;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Models;

namespace Server.Inventory
{
    public class Inventory
    {
        private InventoryData data;

        private List<InventoryItem> _items;

        public float MaximumWeight => data.InventorySpace;

        public float CurrentWeight
        {
            get
            {
                float count = 0;
                _items.ForEach(i =>
                {
                    count += i.GetTotalWeight(i.Quantity);
                });
                return count;
            }
        }

        //public float MaximumCapacity { get { return data.InventoryCapacity; } }

        //public float CurrentCapacity
        //{
        //    get
        //    {
        //        float count = 0;
        //        _items.ForEach(i => count += i.GetTotalCapacity);
        //        return count;
        //    }
        //}

        public Inventory(InventoryData invData)
        {
            if (invData == null)
            {
                Console.WriteLine("ERROR loading inventory. inventory data is null.");
                return;
            }
            data = invData;
            try
            {
                using Context context = new Context();
                _items = new List<InventoryItem>(JsonConvert.DeserializeObject<List<InventoryItem>>(context.Inventory.Find(data.Id).Items).ToList());
            }
            catch
            {
                _items = new List<InventoryItem>();
                SaveInventory();
            }
        }

        public void ShowInventoryToPlayer(IPlayer player, string title)
        {
            player.SendChatMessage("~g~[======~s~ " + title + " ~g~======]");
            foreach (InventoryItem item in _items)
            {
                player.SendChatMessage("~g~[~y~Item Name:~s~ " + item.GetName(false) + ", quantity: " + item.Quantity + "~g~]");
            }
        }

        private void SaveInventory()
        {
            using Context context = new Context();
            InventoryData? invData = context.Inventory.FirstOrDefault(x => x.Id == data.Id);

            if (invData == null)
            {
                Console.WriteLine($"Inventory ID: {data.Id} is null!");
                return;
            }

            invData.Items = JsonConvert.SerializeObject(_items);

            context.SaveChanges();
        }

        /// <summary>
        /// Transfer items safely between two inventories, returns false if transfer fail
        /// </summary>
        /// <param name="inventoryTarget"></param>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public bool TransferItem(Inventory inventoryTarget, InventoryItem item, double quantity = 1)
        {
            // check if the target inventory has enough space
            if (inventoryTarget.MaximumWeight < inventoryTarget.CurrentWeight + item.GetTotalWeight(quantity))
            {
                return false;
            }
            //if (inventoryTarget.MaximumCapacity < inventoryTarget.CurrentCapacity + item.GetTotalWeight)
            //    return false;

            // check if the current inventory has the item
            if (HasItem(item) == false)
            {
                Console.WriteLine($"Doesnt have item");
                return false;
            }

            if (RemoveItem(item, quantity) == false)
            {
                Console.WriteLine($"Unable to remove");
                return false;
            }

            InventoryItem newItem = new InventoryItem(item.Id, item.CustomName, item.ItemValue, quantity);
            if (inventoryTarget.AddItem(newItem) == false)
            {
                Console.WriteLine($"Unable to add");
                return false;
            }
            SaveInventory();
            return true;
        }

        /// <summary>
        /// Add a single item to the inventory
        /// returns false if not enough space
        /// </summary>
        /// <param name="item"></param>
        public bool AddItem(InventoryItem item)
        {
            if (MaximumWeight < CurrentWeight + item.GetTotalWeight())
                return false;

            //if (MaximumCapacity < CurrentCapacity + item.GetTotalCapacity)
            //    return false;
            //item = new InventoryItem(item.Id, item.CustomName, item.ItemValue, item.Quantity);
            if (item.ItemInfo.Stackable)
            {
                InventoryItem? stackableItem = GetItem(item.Id);
                if (stackableItem != null)
                {
                    stackableItem.Quantity += item.Quantity;
                }
                else
                {
                    // if the inventory doesn't have this item already, add it
                    _items.Add(item);
                }
            }
            else
            {
                for (int i = 1; i <= item.Quantity; i++)
                {
                    InventoryItem newItem = new InventoryItem(item.Id, item.CustomName, item.ItemValue);

                    _items.Add(newItem);
                }
            }
            SaveInventory();
            return true;
        }

        /// <summary>
        ///  Add a bunch of items to the inventory, use this when adding large amount of items.
        ///  returns false if not enough space
        /// </summary>
        /// <param name="items"></param>
        public bool AddItem(List<InventoryItem> items)
        {
            float totalWeight = 0;
            //float totalCapacity = 0;
            //items.ForEach(i => { totalWeight += i.GetTotalWeight; totalCapacity += i.GetTotalCapacity; });

            items.ForEach(i => { totalWeight += i.GetTotalWeight(); });

            if (MaximumWeight < CurrentWeight + totalWeight)
                return false;
            //if (MaximumCapacity < CurrentCapacity + totalCapacity)
            //    return false;

            foreach (InventoryItem item in items)
            {
                InventoryItem i = new InventoryItem(item.Id, item.CustomName, item.ItemValue, item.Quantity);
                if (item.ItemInfo.Stackable)
                {
                    InventoryItem? stackableItem = GetItem(item.Id);
                    if (stackableItem != null)
                    {
                        stackableItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        // if the inventory doesn't have this item already, add it
                        _items.Add(i);
                    }
                }
                else
                {
                    _items.Add(i);
                }
            }
            SaveInventory();
            return true;
        }

        /// <summary>
        /// Removes the first Item with the given Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveItem(string id, double quantity = 1)
        {
            InventoryItem? itemToRemove = _items.FirstOrDefault(i => i.Id == id);
            return itemToRemove != null && RemoveItem(itemToRemove, quantity);
        }

        public bool RemoveItem(InventoryItem item)
        {
            return RemoveItem(item, 1);
        }

        public bool RemoveItem(string id, string value)
        {
            InventoryItem itemToRemove = GetItem(id, value);
            if (itemToRemove == null) return false;

            return RemoveItem(itemToRemove, 1);
        }

        public bool RemoveItem(InventoryItem item, double quantity)
        {
            try
            {
                if (HasItem(item) == false) return false;
                // make sure we are not removing more than the quantity

                double newQuantity = item.Quantity -= quantity;

                if (newQuantity < 0) return false;

                if (item.ItemInfo.Stackable)
                {
                    if (newQuantity == 0)
                    {
                        // if the quantity after removing it by quantity is zero, remove it completely.
                        _items.Remove(item);
                        SaveInventory();
                        return true;
                    }

                    item.Quantity = newQuantity;
                    SaveInventory();
                    return true;
                }
                _items.Remove(item);
                SaveInventory();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public InventoryItem? GetItem(string id)
        {
            InventoryItem? item = _items.FirstOrDefault(i => i.Id == id);
            return item;
        }

        /// <summary>
        /// Returns the first item with the same id and value.
        /// Returns null if item is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public InventoryItem GetItem(string id, string value)
        {
            var foundItems = _items.Where(i => i.Id == id);
            return foundItems.FirstOrDefault(item => item.ItemValue == value);
        }

        public bool HasItem(string id)
        {
            return GetItem(id) != null;
        }

        public bool HasItem(string id, string value)
        {
            return GetItem(id, value) != null;
        }

        public bool HasItem(InventoryItem item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        /// Returns the inventory, use AddItem / RemoveItem to add or remove items from the inventory
        /// </summary>
        /// <returns></returns>
        public List<InventoryItem> GetInventory()
        {
            return _items;
        }

        public List<InventoryItem> GetInventoryItems(string id)
        {
            return _items.Where(i => i.Id == id).ToList();
        }
    }
}