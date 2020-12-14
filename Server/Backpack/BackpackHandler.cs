using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Extensions;
using Server.Extensions.TextLabel;
using Server.Inventory;

namespace Server.Backpack
{
    public class BackpackHandler
    {
        public static Dictionary<int, TextLabel> DroppedLabels = new Dictionary<int, TextLabel>();

        public static void LoadDroppedBackpacks()
        {
            using Context context = new Context();

            List<Models.Backpack> backpacks = context.Backpacks.ToList();

            int dropCount = 0;

            foreach (Models.Backpack backpack in backpacks)
            {
                Position backpackPosition = new Position(backpack.DropPosX, backpack.DropPosY, backpack.DropPosZ);

                if (backpackPosition == Position.Zero) continue;

                TextLabel droppedLabel = new TextLabel("Backpack\nUse /pickupbackpack", backpackPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.DarkGreen));
                droppedLabel.Add();
                DroppedLabels.Add(backpack.Id, droppedLabel);
                dropCount++;
            }

            Console.WriteLine($"Loaded {dropCount} dropped backpacks.");
        }

        public static void DropBackpack(IPlayer player, InventoryItem backPackItem)
        {
            Inventory.Inventory playerInventory = player.FetchInventory();

            InventoryItem item = playerInventory.GetItem(backPackItem.Id);

            bool backPackRemoved = playerInventory.RemoveItem(item);

            if (!backPackRemoved)
            {
                player.SendErrorNotification("There was an error dropping the item.");
                return;
            }

            bool tryParse = int.TryParse(backPackItem.ItemValue, out int backPackId);

            if (!tryParse)
            {
                player.SendErrorNotification("There was an error getting the backpack id.");
                return;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            Models.Backpack backpack = context.Backpacks.Find(backPackId);

            Position playerPosition = player.Position;

            backpack.DropPosX = playerPosition.X;
            backpack.DropPosY = playerPosition.Y;
            backpack.DropPosZ = playerPosition.Z;

            playerCharacter.BackpackId = 0;

            context.SaveChanges();
            

            TextLabel droppedLabel = new TextLabel("Backpack\nUse /pickupbackpack", playerPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.DarkGreen));
            droppedLabel.Add();
            DroppedLabels.Add(backPackId, droppedLabel);

            player.SetClothes(5, 0, 0);

            player.SendInfoNotification($"You've dropped your backpack.");
        }
    }
}