using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;

namespace Server.Character.Clothing
{
    public class OutfitCommands
    {
        [Command("saveoutfit", commandType: CommandType.Character, onlyOne: true, description: "Used to save an outfit to your outfits.")]
        public static void OutfitCommandSaveOutfit(IPlayer player, string outfitName = "")
        {
            if (!player.IsSpawned()) return;

            if (player.GetClass().CreatorRoom) return;

            if (string.IsNullOrEmpty(outfitName) || outfitName.Length < 2)
            {
                player.SendSyntaxMessage("/saveoutfit [Outfit Name]");
                return;
            }

            Models.Character? playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendErrorNotification("Unable to find your character.");
                return;
            }

            string? playerClothes = playerCharacter.ClothesJson;
            string? playerAccessories = playerCharacter.AccessoryJson;

            List<Outfit>? playerOutfits = JsonConvert.DeserializeObject<List<Outfit>>(playerCharacter.Outfits);

            using Context context = new Context();

            Models.Character character = context.Character.First(x => x.Id == playerCharacter.Id);

            if (playerOutfits == null)
            {
                playerOutfits = new List<Outfit>();
                character.Outfits = JsonConvert.SerializeObject(playerOutfits);
                context.SaveChangesAsync();
            }

            if (playerOutfits.Any(x => x.Name?.ToLower() == outfitName))
            {
                player.SendErrorMessage("This outfit name has already been used!");
                return;
            }

            Outfit newOutfit = new Outfit(outfitName, playerClothes, playerAccessories);

            playerOutfits.Add(newOutfit);

            character.Outfits = JsonConvert.SerializeObject(playerOutfits);

            context.SaveChanges();

            player.SendInfoNotification($"You've created a new outfit named {outfitName}.");
        }

        [Command("outfits", commandType: CommandType.Character, description: "Used to switch between outfits.")]
        public static void OutfitCommandOutfits(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (player.GetClass().CreatorRoom) return;

            Models.Character? playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendErrorNotification("Unable to find your character.");
                return;
            }

            List<Outfit>? playerOutfits = JsonConvert.DeserializeObject<List<Outfit>>(playerCharacter.Outfits);

            if (playerOutfits == null || !playerOutfits.Any())
            {
                player.SendErrorNotification("You don't have any outfits.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Outfit outfit in playerOutfits)
            {
                menuItems.Add(new NativeMenuItem(outfit.Name));
            }

            NativeMenu menu = new NativeMenu("OutfitSystem:OutfitMainMenu", "Outfits", "Select an outfit to change into", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnOutfitMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            Models.Character? playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendErrorNotification("Unable to find your character.");
                return;
            }

            List<Outfit>? playerOutfits = JsonConvert.DeserializeObject<List<Outfit>>(playerCharacter.Outfits);

            if (playerOutfits == null || !playerOutfits.Any())
            {
                player.SendErrorNotification("You don't have any outfits.");
                return;
            }

            Outfit selectedOutfit = playerOutfits.First(x => x.Name == option);

            player.SetData("OutfitSystem:SelectedOutfit", selectedOutfit.Name);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Place On", "Change into the outfit"),
                new NativeMenuItem("Remove Outfit", "Removes it from your outfit system")
            };

            NativeMenu menu = new NativeMenu("OutfitSystem:SubItemSelect", "Outfits", "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnOutfitSubMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            bool hasData = player.GetData("OutfitSystem:SelectedOutfit", out string selectedOutfitName);

            if (!hasData)
            {
                player.SendErrorNotification("An error has occurred.");
                return;
            }

            Models.Character? playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendErrorNotification("Unable to find your character.");
                return;
            }

            List<Outfit>? playerOutfits = JsonConvert.DeserializeObject<List<Outfit>>(playerCharacter.Outfits);

            if (playerOutfits == null || !playerOutfits.Any())
            {
                player.SendErrorNotification("You don't have any outfits.");
                return;
            }

            Outfit selectedOutfit = playerOutfits.First(x => x.Name == selectedOutfitName);

            if (option == "Place On")
            {
                PlaceOutfitOn(player, selectedOutfit);
                return;
            }
            if (option == "Remove Outfit")
            {
                playerOutfits.Remove(selectedOutfit);

                using Context context = new Context();

                Models.Character character = context.Character.First(x => x.Id == playerCharacter.Id);

                character.Outfits = JsonConvert.SerializeObject(playerOutfits);

                context.SaveChanges();

                player.SendInfoNotification($"You've removed the {selectedOutfit.Name} from your outfits.");
            }
        }

        private static void PlaceOutfitOn(IPlayer player, Outfit outfit)
        {
            try
            {
                Models.Character playerCharacter = player.FetchCharacter();

                List<ClothesData>? outfitClothingList = JsonConvert.DeserializeObject<List<ClothesData>>(outfit.Clothes);
                List<AccessoryData>? outfitAccessoryList = JsonConvert.DeserializeObject<List<AccessoryData>>(outfit.Accessories);

                List<ClothesData>? CurrentClothingList =
                    JsonConvert.DeserializeObject<List<ClothesData>>(playerCharacter.ClothesJson);
                List<AccessoryData>? CurrentAccessoryList =
                    JsonConvert.DeserializeObject<List<AccessoryData>>(playerCharacter.AccessoryJson);

                if (outfitClothingList is null || outfitAccessoryList is null)
                {
                    player.SendErrorNotification("An error occurred.");
                    return;
                }

                Inventory.Inventory playerInventory = player.FetchInventory();

                #region Clothing

                List<InventoryItem> clothingItems = playerInventory.GetInventoryItems("ITEM_CLOTHES");

                foreach (ClothesData clothesData in outfitClothingList)
                {
                    Console.WriteLine($"Outfit Clothing Item Slot: {clothesData.slot}");

                    // Already has clothing item on
                    if (CurrentClothingList.FirstOrDefault(x => x.slot == clothesData.slot && x.drawable == clothesData.drawable && x.texture == clothesData.texture) != null) continue;

                    clothesData.male = player.GetClass().IsMale;

                    InventoryItem? clothingItem = null;

                    foreach (InventoryItem inventoryItem in clothingItems)
                    {
                        ClothesData itemData = JsonConvert.DeserializeObject<ClothesData>(inventoryItem.ItemValue);

                        if (itemData.slot != clothesData.slot) continue;
                        if (itemData.drawable != clothesData.drawable) continue;
                        if (itemData.texture != clothesData.texture) continue;
                        if (itemData.male != player.GetClass().IsMale) continue;

                        clothingItem = inventoryItem;
                        break;
                    }

                    if (clothingItem == null)
                    {
                        player.SendErrorNotification("Unable to find an item in your inventory.");
                        return;
                    }

                    // Player doesn't have item in inventory & not wearing
                    if (!playerInventory.HasItem(clothingItem))
                    {
                        player.SendErrorMessage($"You don't have a clothing item in your inventory!");
                        return;
                    }

                    // Remove item from inventory
                    playerInventory.RemoveItem(clothingItem);

                    ClothesData? currentClothingData = CurrentClothingList.FirstOrDefault(x => x.slot == clothesData.slot);

                    if (currentClothingData != null)
                    {
                        InventoryItem currentClothingItem =
                            Clothes.ConvertClothesToInventoryItem(currentClothingData, player.GetClass().IsMale);

                        playerInventory.AddItem(currentClothingItem);
                    }

                    Clothes.SetClothes(player, clothesData);
                    Clothes.SaveClothes(player, clothesData);
                }

                #endregion Clothing

                #region Accessories

                List<InventoryItem> accessoryItems = playerInventory.GetInventoryItems("ITEM_CLOTHES_ACCESSORY");

                foreach (AccessoryData accessoryData in outfitAccessoryList)
                {
                    Console.WriteLine($"Outfit Accessory Item Slot: {accessoryData.slot}");

                    // Already has clothing item on
                    if (CurrentAccessoryList.FirstOrDefault(x => x.slot == accessoryData.slot && x.drawable == accessoryData.drawable && x.texture == accessoryData.texture) != null) continue;

                    accessoryData.male = player.GetClass().IsMale;

                    InventoryItem? accessoryItem = null;

                    foreach (InventoryItem inventoryItem in accessoryItems)
                    {
                        AccessoryData itemData = JsonConvert.DeserializeObject<AccessoryData>(inventoryItem.ItemValue);

                        if (itemData.slot != accessoryData.slot) continue;
                        if (itemData.drawable != accessoryData.drawable) continue;
                        if (itemData.texture != accessoryData.texture) continue;
                        if (itemData.male != player.GetClass().IsMale) continue;

                        accessoryItem = inventoryItem;
                        break;
                    }

                    if (accessoryItem == null)
                    {
                        player.SendErrorNotification("Unable to find an item in your inventory.");
                        return;
                    }

                    // Player doesn't have item in inventory & not wearing
                    if (!playerInventory.HasItem(accessoryItem))
                    {
                        player.SendErrorMessage($"You don't have a clothing item in your inventory!");
                        return;
                    }

                    // Remove item from inventory
                    playerInventory.RemoveItem(accessoryItem);

                    AccessoryData? currentAccessoryData = CurrentAccessoryList.FirstOrDefault(x => x.slot == accessoryData.slot);

                    if (currentAccessoryData != null)
                    {
                        InventoryItem currentClothingItem =
                            Clothes.ConvertAccessoryToInventoryItem(currentAccessoryData, player.GetClass().IsMale);

                        playerInventory.AddItem(currentClothingItem);
                    }

                    Clothes.SetAccessories(player, accessoryData);
                    Clothes.SaveAccessories(player, accessoryData);
                }

                #endregion Accessories
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}