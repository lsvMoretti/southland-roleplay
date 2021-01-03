using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Server.Inventory;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

            List<Outfit>? playerOutfits = JsonSerializer.Deserialize<List<Outfit>>(playerCharacter.Outfits);

            using Context context = new Context();

            Models.Character character = context.Character.First(x => x.Id == playerCharacter.Id);

            if (playerOutfits == null)
            {
                playerOutfits = new List<Outfit>();
                character.Outfits = JsonSerializer.Serialize(playerOutfits);
                context.SaveChangesAsync();
            }

            if (playerOutfits.Any(x => x.Name?.ToLower() == outfitName))
            {
                player.SendErrorMessage("This outfit name has already been used!");
                return;
            }

            Outfit newOutfit = new Outfit(outfitName, playerClothes, playerAccessories);

            playerOutfits.Add(newOutfit);

            character.Outfits = JsonSerializer.Serialize(playerOutfits);

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

            List<Outfit>? playerOutfits = JsonSerializer.Deserialize<List<Outfit>>(playerCharacter.Outfits);

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

            List<Outfit>? playerOutfits = JsonSerializer.Deserialize<List<Outfit>>(playerCharacter.Outfits);

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

            List<Outfit>? playerOutfits = JsonSerializer.Deserialize<List<Outfit>>(playerCharacter.Outfits);

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

                character.Outfits = JsonSerializer.Serialize(playerOutfits);

                context.SaveChanges();

                player.SendInfoNotification($"You've removed the {selectedOutfit.Name} from your outfits.");
            }
        }

        private static void PlaceOutfitOn(IPlayer player, Outfit outfit)
        {
            try
            {
                Models.Character playerCharacter = player.FetchCharacter();

                List<ClothesData>? outfitList = JsonSerializer.Deserialize<List<ClothesData>>(outfit.Clothes);

                List<ClothesData>? CurrentClothingList =
                    JsonSerializer.Deserialize<List<ClothesData>>(playerCharacter.ClothesJson);

                if (outfitList is null)
                {
                    player.SendErrorNotification("An error occurred.");
                    return;
                }

                Inventory.Inventory playerInventory = player.FetchInventory();

                List<ClothesData> newClothingList = CurrentClothingList;

                foreach (ClothesData clothesData in outfitList)
                {
                    Console.WriteLine($"Outfit Clothing Item Slot: {clothesData.slot}");

                    // Already has clothing item on
                    if (CurrentClothingList.Contains(clothesData)) continue;

                    InventoryItem inventoryItem = Clothes.ConvertClothesToInventoryItem(clothesData, player.GetClass().IsMale);

                    // Player doesn't have item in inventory & not wearing
                    if (!playerInventory.HasItem(inventoryItem))
                    {
                        player.SendErrorMessage($"You don't have a clothing item in your inventory!");
                        return;
                    }

                    // Remove item from inventory
                    playerInventory.RemoveItem(inventoryItem);

                    ClothesData? currentClothingData = CurrentClothingList.FirstOrDefault(x => x.slot == clothesData.slot);

                    if (currentClothingData != null)
                    {
                        InventoryItem currentClothingItem =
                            Clothes.ConvertClothesToInventoryItem(currentClothingData, player.GetClass().IsMale);

                        playerInventory.AddItem(currentClothingItem);

                        newClothingList.Remove(currentClothingData);
                        newClothingList.Add(clothesData);
                    }

                    Clothes.SetClothes(player, clothesData);
                    Clothes.SaveClothes(player, clothesData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}