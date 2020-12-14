using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Extensions;
using Server.Extensions.TextLabel;

namespace Server.Inventory
{
    public class DroppedItems
    {
        private static List<DroppedItem> droppedGameItems = new List<DroppedItem>();

        public static void LoadDroppedItemsForPlayer(IPlayer player)
        {
            if (!droppedGameItems.Any()) return;

        }

        public static void CreateDroppedItem(InventoryItem item, Position position)
        {
            string labelText = $"{item.CustomName}\nUse /pickupitem";

            if (item.Quantity > 1)
            {
                labelText = $"{item.CustomName} x{item.Quantity}\nUse /pickupitem";
            }

            TextLabel textLabel = new TextLabel(labelText, position - new Position(0, 0, 0.5f), TextFont.FontChaletComprimeCologne, new LsvColor(36, 114, 18), 2f);
            textLabel.Add();

            DroppedItem newDroppedItem = new DroppedItem(item, position, textLabel);

            droppedGameItems.Add(newDroppedItem);
        }

        public static DroppedItem FetchNearestDroppedItem(Position position, float range)
        {
            return droppedGameItems.FirstOrDefault(x => x.Position.Distance(position) <= range);
        }

        public static void RemoveDroppedItem(DroppedItem droppedItem)
        {
            droppedItem.TextLabel.Remove();

            droppedItem.Delete();
            droppedGameItems.Remove(droppedItem);
        }
    }

    public class DroppedItem
    {
        public InventoryItem Item { get; set; }
        public Position Position { get; set; }
        public TextLabel TextLabel { get; set; }

        public DroppedItem(InventoryItem item, Vector3 position, TextLabel textLabel)
        {
            Item = item;
            Position = position;
            TextLabel = textLabel;
        }
    }

    public static class DroppedItemExtension
    {
        public static void Delete(this DroppedItem droppedItem)
        {
            droppedItem.TextLabel.Remove();
        }
    }
}