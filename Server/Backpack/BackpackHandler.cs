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
using Server.Models;

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

        public static void StoreBackpackInStorage(IPlayer player, InventoryItem backpackItem,
            Storage storage)
        {
            Inventory.Inventory inventory = Storage.FetchInventory(storage);

            if (!inventory.AddItem(backpackItem))
            {
                player.SendErrorNotification("An error occurred adding this to the storage");
                return;
            }

            player.SetClothes(5, 0, 0);

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);
            playerCharacter.BackpackId = 0;

            context.SaveChanges();

            player.SendInfoNotification($"You've stored a backpack in the bin. Bin Id: {storage.Id}.");

            player.SendEmoteMessage($"reaches into the bin placing an item.");
        }

        public static void TakeBackpackFromStorage(IPlayer player, InventoryItem backpackItem, Storage storage)
        {
            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter.BackpackId > 0)
            {
                player.SendErrorNotification("You already have a backpack.");
                return;
            }

            Inventory.Inventory inventory = Storage.FetchInventory(storage);
            Inventory.Inventory playerInventory = player.FetchInventory();

            int backpackId = int.Parse(backpackItem.ItemValue);

            Models.Backpack backpackDb = context.Backpacks.Find(backpackId);

            bool itemAdded = false;
            InventoryItem newItem = null;
            if (backpackDb.Drawable == 31)
            {
                // Backpack

                newItem = new InventoryItem("ITEM_BACKPACK", "Backpack", backpackDb.Id.ToString());
                itemAdded = playerInventory.AddItem(newItem);
            }

            if (backpackDb.Drawable == 44)
            {
                // Duffel

                newItem = new InventoryItem("ITEM_DUFFELBAG", "Duffelbag", backpackDb.Id.ToString());
                itemAdded = playerInventory.AddItem(newItem);
            }

            if (!itemAdded)
            {
                player.SendErrorMessage("Unable to add this item to your inventory");
                return;
            }

            if (!inventory.RemoveItem(backpackItem))
            {
                player.SendErrorNotification("An error occurred taking this. Check you got space!");
                playerInventory.RemoveItem(newItem);
                return;
            }

            player.SetClothes(5, backpackDb.Drawable, 0);

            playerCharacter.BackpackId = backpackDb.Id;
            context.SaveChanges();

            player.SendInfoNotification("You've taken a backpack from the storage.");

            player.SendEmoteMessage("reaches into the bin, taking an item from it.");
        }
    }
}