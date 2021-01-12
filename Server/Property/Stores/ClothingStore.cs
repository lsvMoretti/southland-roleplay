using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Character;
using Server.Character.Clothing;
using Server.Chat;
using Server.Extensions;

namespace Server.Property.Stores
{
    public class ClothingStore
    {
        private const string CLOTHESDATA = "CLOTHINGDATA";
        private const string SUBCLOTHESDATA = "SUBCLOTHINGDATA";

        public static void ShowClothesStoreMenu(IPlayer player)
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            player.GetData("INSTOREID", out int inStoreId);

            Models.Property property = Models.Property.FetchProperty(inStoreId);

            NativeMenu clothesStoreMainMenu = new NativeMenu("ClothingStoreMainMenu", property.BusinessName, "Clothing Store");

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                {new NativeMenuItem("Accessories") },
                {new NativeMenuItem("Clothing") }
            };

            clothesStoreMainMenu.MenuItems = menuItems;

            NativeUi.ShowNativeMenu(player, clothesStoreMainMenu, true);
        }

        public static void EventClothingStoreMainMenu(IPlayer player, string option)
        {
            if (option == "Close") return;

            var clothingStore = new ClothingStore();

            if (option == "Clothing")
            {
                clothingStore.ShowClothingOptions(player);
                return;
            }

            if (option == "Accessories")
            {
                clothingStore.ShowAccessoryTypesMenu(player);
            }
        }

        #region Clothing System

        /// <summary>
        /// Show clothing categories
        /// </summary>
        /// <param name="player"></param>
        private void ShowClothingOptions(IPlayer player)
        {
            player.GetData("INSTOREID", out int inStoreId);

            Models.Property property = Models.Property.FetchProperty(inStoreId);

            NativeMenu nativeMenu = new NativeMenu("ClothingStoreShowClothingTypesMenu", property.BusinessName, "Clothing Store");

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Masks"),
                new NativeMenuItem("Tops"),
                new NativeMenuItem("Undershirts"),
                new NativeMenuItem("Trousers"),
                new NativeMenuItem("Shoes"),
                new NativeMenuItem("Accessories")
            };

            nativeMenu.MenuItems = menuItems;

            NativeUi.ShowNativeMenu(player, nativeMenu, true);
        }

        public static void EventClothingStoreShowClothingTypesMenu(IPlayer player, string option)
        {
            if (option == "Close")
            {
                Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                return;
            }

            int selectedSlot = option switch
            {
                "Masks" => (int)ClothesType.Mask,
                "Tops" => (int)ClothesType.Top,
                "Undershirts" => (int)ClothesType.Undershirt,
                "Trousers" => (int)ClothesType.Legs,
                "Shoes" => (int)ClothesType.Feet,
                "Accessories" => (int)ClothesType.Accessories,
                _ => -1
            };

            if (selectedSlot == -1)
            {
                player.SendErrorNotification("The item you selected was invalid.");
                return;
            }

            // Place clothes into list
            List<KeyValuePair<ClothesData, ClothesInfo>> clothesList = Clothes.DictClothesInfo.Where(x => x.Key.slot == selectedSlot && x.Key.texture == 0).OrderBy(x => x.Value.DisplayNameMale).ToList();

            List<KeyValuePair<ClothesData, ClothesInfo>> updatedClothesList = new List<KeyValuePair<ClothesData, ClothesInfo>>();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (var keyValuePair in clothesList.Where(x => x.Key.slot == selectedSlot))
            {
                if (player.IsMale())
                {
                    if (keyValuePair.Value.DisplayNameMale.Contains("undefined") || string.IsNullOrEmpty(keyValuePair.Value.DisplayNameMale))
                    {
                        continue;
                    }
                    /*if (updatedClothesList.FirstOrDefault(x =>
                            x.Value.DisplayNameMale == keyValuePair.Value.DisplayNameMale).Key != null) continue;*/

                    menuItems.Add(new NativeMenuItem(keyValuePair.Value.DisplayNameMale, $"~g~${keyValuePair.Value.Price}"));
                    updatedClothesList.Add(keyValuePair);
                }
                else
                {
                    if (keyValuePair.Value.DisplayNameFemale.Contains("undefined") || string.IsNullOrEmpty(keyValuePair.Value.DisplayNameFemale))
                    {
                        continue;
                    }

                    /*if (updatedClothesList.FirstOrDefault(x =>
                            x.Value.DisplayNameFemale == keyValuePair.Value.DisplayNameFemale).Key != null) continue;*/

                    menuItems.Add(new NativeMenuItem(keyValuePair.Value.DisplayNameFemale, $"~g~${keyValuePair.Value.Price}"));
                    updatedClothesList.Add(keyValuePair);
                }
            }

            player.SetData(CLOTHESDATA, JsonConvert.SerializeObject(updatedClothesList));

            player.GetData("INSTOREID", out int inStoreId);

            Models.Property property = Models.Property.FetchProperty(inStoreId);

            NativeMenu menu = new NativeMenu("ClothingListSelect", property.BusinessName, "Clothing Store", menuItems)
            {
                PassIndex = true,
                ItemChangeTrigger = "ClothingListItemChange"
            };

            //menu.DisableFPP = true;

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void EventClothingListSelect(IPlayer player, string option, int index)
        {
            try
            {
                if (option == "Close")
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                    return;
                }

                player.GetData(CLOTHESDATA, out string clothesJson);

                List<KeyValuePair<ClothesData, ClothesInfo>> clothesList =
                    JsonConvert.DeserializeObject<List<KeyValuePair<ClothesData, ClothesInfo>>>(
                        clothesJson);

                if (index >= clothesList.Count)
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                    return;
                }

                var selectedItem = clothesList[index];

                List<KeyValuePair<ClothesData, ClothesInfo>> updatedClothesList = new List<KeyValuePair<ClothesData, ClothesInfo>>();

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                Console.WriteLine($"Index: {index}, Selected key drawable: {selectedItem.Key.drawable}");
                Console.WriteLine($"Index+1: {index + 1}, Selected key drawable: {clothesList[index + 1].Key.drawable}");

                foreach (var clothesInfo in Clothes.DictClothesInfo)
                {
                    if (clothesInfo.Key.slot != selectedItem.Key.slot) continue;
                    if (clothesInfo.Key.drawable != selectedItem.Key.drawable) continue;

                    if (player.IsMale())
                    {
                        if (clothesInfo.Value.DisplayNameMale.Contains("undefined") || string.IsNullOrEmpty(clothesInfo.Value.DisplayNameMale))
                        {
                            continue;
                        }

                        var item = new NativeMenuItem($"{clothesInfo.Value.DisplayNameMale}", $"~g~${clothesInfo.Value.Price}");
                        menuItems.Add(item);
                        updatedClothesList.Add(clothesInfo);

                        Console.WriteLine($"Added: {clothesInfo.Value.DisplayNameMale}, Slot: {clothesInfo.Key.slot}, Drawable: {clothesInfo.Key.drawable}");
                    }
                    else
                    {
                        if (clothesInfo.Value.DisplayNameFemale.Contains("undefined") || string.IsNullOrEmpty(clothesInfo.Value.DisplayNameFemale))
                        {
                            continue;
                        }

                        var item = new NativeMenuItem($"{clothesInfo.Value.DisplayNameFemale}", $"~g~${clothesInfo.Value.Price}");
                        menuItems.Add(item);
                        updatedClothesList.Add(clothesInfo);
                    }
                }

                player.SetData(SUBCLOTHESDATA, JsonConvert.SerializeObject(updatedClothesList));

                player.GetData("INSTOREID", out int inStoreId);

                Models.Property property = Models.Property.FetchProperty(inStoreId);

                NativeMenu menu = new NativeMenu("ClothingListSubSelect", property.BusinessName, "Clothing Store", menuItems)
                {
                    PassIndex = true,
                    ItemChangeTrigger = "ClothingListSubItemChange",
                    //DisableFPP = true,
                };

                NativeUi.ShowNativeMenu(player, menu, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                player.LoadCharacterCustomization();
                player.Position = player.Position;
                return;
            }
        }

        public static void EventClothingListItemChange(IPlayer player, int index, string itemText)
        {
            try
            {
                if (itemText == "Close") return;

                player.GetData(CLOTHESDATA, out string clothesJson);

                List<KeyValuePair<ClothesData, ClothesInfo>> updatedClothesList =
                    JsonConvert.DeserializeObject<List<KeyValuePair<ClothesData, ClothesInfo>>>(
                        clothesJson);

                if (index >= updatedClothesList.Count)
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));

                    return;
                }

                var selectedItem = updatedClothesList[index];

                player.SetClothes(selectedItem.Key.slot, selectedItem.Key.drawable, selectedItem.Key.texture);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                player.Position = player.Position;
                return;
            }
        }

        public static void EventClothingListSubItemChange(IPlayer player, int index, string itemText)
        {
            try
            {
                if (itemText == "Close") return;
                player.GetData(SUBCLOTHESDATA, out string clothesJson);

                List<KeyValuePair<ClothesData, ClothesInfo>> updatedClothesList =
                    JsonConvert.DeserializeObject<List<KeyValuePair<ClothesData, ClothesInfo>>>(
                        clothesJson);

                if (index >= updatedClothesList.Count)
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));

                    return;
                }

                var selectedItem = updatedClothesList[index];

                player.SetClothes(selectedItem.Key.slot, selectedItem.Key.drawable, selectedItem.Key.texture);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                player.Position = player.Position;
                return;
            }
        }

        public static void EventClothingListSubSelect(IPlayer player, string option, int index)
        {
            try
            {
                if (option == "Close")
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                    return;
                }

                player.GetData(SUBCLOTHESDATA, out string clothesJson);

                List<KeyValuePair<ClothesData, ClothesInfo>> clothesList =
                    JsonConvert.DeserializeObject<List<KeyValuePair<ClothesData, ClothesInfo>>>(
                        clothesJson);

                if (index >= clothesList.Count)
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                    return;
                }

                KeyValuePair<ClothesData, ClothesInfo> selectedItem = clothesList[index];

                player.SetData("CLOTHINGITEMSELECTED", JsonConvert.SerializeObject(selectedItem));

                player.GetData("INSTOREID", out int inStoreId);

                Models.Property property = Models.Property.FetchProperty(inStoreId);

                NativeMenu menu = new NativeMenu("PurchaseClothingItemMenu", property.BusinessName, "Choose an Option", new List<NativeMenuItem> { new NativeMenuItem("Purchase") });

                NativeUi.ShowNativeMenu(player, menu, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                player.LoadCharacterCustomization();
                player.Position = player.Position;
                return;
            }
        }

        public static void EventPurchaseClothingItemMenu(IPlayer player, string option)
        {
            try
            {
                CustomCharacter customCharacter =
                    JsonConvert.DeserializeObject<CustomCharacter>(player.FetchCharacter().CustomCharacter);

                HairInfo hairInfo = JsonConvert.DeserializeObject<HairInfo>(customCharacter.Hair);

                Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));

                if (option == "Close")
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                    return;
                }

                player.GetData("CLOTHINGITEMSELECTED", out string clothingItemSelected);

                KeyValuePair<ClothesData, ClothesInfo> selectedItem =
                    JsonConvert.DeserializeObject<KeyValuePair<ClothesData, ClothesInfo>>(
                        clothingItemSelected);

                float playerMoney = player.FetchCharacter().Money;

                if (playerMoney < selectedItem.Value.Price)
                {
                    player.SendInfoNotification($"You don't have the funds for this!");
                    return;
                }

                using Context context = new Context();
                Models.Character playerCharacter = context.Character.Find(player.FetchCharacterId());

                Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));

                var clothesItem = selectedItem.Key;

                clothesItem.male = player.IsMale();

                if (clothesItem.slot != 2)
                {
                    Inventory.Inventory playerInventory = player.FetchInventory();

                    bool itemAdded = playerInventory.AddItem(Clothes.ConvertClothesToInventoryItem(clothesItem, player.IsMale()));

                    if (itemAdded)
                    {
                        player.GetData("INSTOREID", out int extraId);

                        player.RemoveCash(selectedItem.Value.Price);

                        Models.Property extraProperty = Models.Property.FetchProperty(extraId);

                        extraProperty?.AddToBalance(selectedItem.Value.Price);

                        if (playerCharacter.Sex == 0)
                        {
                            player.SendInfoNotification($"You've bought {selectedItem.Value.DisplayNameMale} for {selectedItem.Value.Price:C}.");
                            return;
                        }

                        player.SendInfoNotification($"You've bought {selectedItem.Value.DisplayNameFemale} for {selectedItem.Value.Price:C}.");
                        return;
                    }

                    player.SendErrorNotification("You don't have enough space for this.");
                    return;
                }

                hairInfo.Hair = clothesItem.drawable;

                customCharacter.Hair = JsonConvert.SerializeObject(hairInfo);

                playerCharacter.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

                player.SetClothes(2, clothesItem.drawable, hairInfo.Color);

                context.SaveChanges();

                player.SendInfoNotification($"You've bought {selectedItem.Value.DisplayNameMale} for {selectedItem.Value.Price:C}.");

                player.GetData("INSTOREID", out int storeId);

                player.RemoveCash(selectedItem.Value.Price);

                Models.Property property = Models.Property.FetchProperty(storeId);

                property?.AddToBalance(selectedItem.Value.Price);
                player.Position = player.Position;
            }
            catch (Exception e)
            {
                player.LoadCharacterCustomization();
                Console.WriteLine(e);
                player.Position = player.Position;
                return;
            }

            return;
        }

        #endregion Clothing System

        #region Accessory System

        private void ShowAccessoryTypesMenu(IPlayer player)
        {
            player.GetData("INSTOREID", out int inStoreId);

            Models.Property property = Models.Property.FetchProperty(inStoreId);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Hats"),
                new NativeMenuItem("Glasses"),
                new NativeMenuItem("Ears"),
                new NativeMenuItem("Watches"),
                new NativeMenuItem("Bracelets")
            };

            NativeMenu nativeMenu = new NativeMenu("ClothingStoreShowAccessoryTypesMenu", property.BusinessName, "Clothing Store", menuItems);

            NativeUi.ShowNativeMenu(player, nativeMenu, true);
        }

        public static void EventClothingStoreShowAccessoryTypesMenu(IPlayer player, string option)
        {
            if (option == "Back" || option == "Close") return;

            int propSlot = option switch
            {
                "Hats" => 0,
                "Glasses" => 1,
                "Ears" => 2,
                "Watches" => 6,
                "Bracelets" => 7,
                _ => -1
            };

            player.GetData("INSTOREID", out int inStoreId);

            Models.Property property = Models.Property.FetchProperty(inStoreId);

            if (propSlot == -1)
            {
                player.SendErrorNotification("The item you selected was invalid.");
                return;
            }

            List<KeyValuePair<ClothesData, ClothesInfo>> accessoryList = Clothes.DictAccessoriesInfo.Where(x => x.Key.slot == propSlot && x.Key.texture == 0)
                .ToList();

            List<KeyValuePair<ClothesData, ClothesInfo>> updatedAccessoryList = new List<KeyValuePair<ClothesData, ClothesInfo>>();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (var keyValuePair in accessoryList)
            {
                if (player.IsMale())
                {
                    if (keyValuePair.Value.DisplayNameMale.Contains("undefined") || string.IsNullOrEmpty(keyValuePair.Value.DisplayNameMale))
                    {
                        continue;
                    }

                    menuItems.Add(new NativeMenuItem($"{keyValuePair.Value.DisplayNameMale}", $"~g~${keyValuePair.Value.Price}"));
                    updatedAccessoryList.Add(keyValuePair);
                }
                else
                {
                    if (keyValuePair.Value.DisplayNameFemale.Contains("undefined") || string.IsNullOrEmpty(keyValuePair.Value.DisplayNameFemale))
                    {
                        continue;
                    }

                    menuItems.Add(new NativeMenuItem($"{keyValuePair.Value.DisplayNameFemale}", $"~g~${keyValuePair.Value.Price}"));
                    updatedAccessoryList.Add(keyValuePair);
                }
            }

            player.SetData(CLOTHESDATA, JsonConvert.SerializeObject(updatedAccessoryList));

            NativeMenu menu = new NativeMenu("AccessoryListSelect", property.BusinessName, "Clothing Store", menuItems)
            {
                PassIndex = true,
                ItemChangeTrigger = "AccessoryListItemChange",
                //DisableFPP = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void EventAccessoryListSelect(IPlayer player, string option, int index)
        {
            try
            {
                if (option == "Close")
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                    player.Position = player.Position;
                    return;
                }

                player.GetData(CLOTHESDATA, out string clothesData);

                List<KeyValuePair<ClothesData, ClothesInfo>> accessoryList =
                    JsonConvert.DeserializeObject<List<KeyValuePair<ClothesData, ClothesInfo>>>(
                        clothesData);

                if (index >= accessoryList.Count)
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                    return;
                }

                var selectedItem = accessoryList[index];

                List<KeyValuePair<ClothesData, ClothesInfo>> updatedAccessoryList = new List<KeyValuePair<ClothesData, ClothesInfo>>();

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                foreach (var clothesInfo in Clothes.DictAccessoriesInfo.Where(x => x.Key.slot == selectedItem.Key.slot && x.Key.drawable == selectedItem.Key.drawable))
                {
                    if (player.IsMale())
                    {
                        if (clothesInfo.Value.DisplayNameMale.Contains("undefined") || string.IsNullOrEmpty(clothesInfo.Value.DisplayNameMale))
                        {
                            continue;
                        }

                        menuItems.Add(new NativeMenuItem($"{clothesInfo.Value.DisplayNameMale}", $"~g~${clothesInfo.Value.Price}"));
                        updatedAccessoryList.Add(clothesInfo);
                    }
                    else
                    {
                        if (clothesInfo.Value.DisplayNameFemale.Contains("undefined") || string.IsNullOrEmpty(clothesInfo.Value.DisplayNameFemale))
                        {
                            continue;
                        }

                        menuItems.Add(new NativeMenuItem($"{clothesInfo.Value.DisplayNameFemale}", $"~g~${clothesInfo.Value.Price}"));
                        updatedAccessoryList.Add(clothesInfo);
                    }
                }

                player.SetData(SUBCLOTHESDATA, JsonConvert.SerializeObject(updatedAccessoryList));

                player.GetData("INSTOREID", out int inStoreId);

                Models.Property property = Models.Property.FetchProperty(inStoreId);

                NativeMenu menu = new NativeMenu("AccessoryListSubSelect", property.BusinessName, "Clothing Store", menuItems)
                {
                    PassIndex = true,
                    ItemChangeTrigger = "AccessoryListSubItemChange",
                    //DisableFPP = true
                };

                NativeUi.ShowNativeMenu(player, menu, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                player.LoadCharacterCustomization();
                player.Position = player.Position;
                return;
            }
        }

        public static void EventAccessoryListItemChange(IPlayer player, int index, string itemText)
        {
            try
            {
                if (itemText == "Close") return;
                player.GetData(CLOTHESDATA, out string clothesData);

                List<KeyValuePair<ClothesData, ClothesInfo>> updatedAccessoryList =
                    JsonConvert.DeserializeObject<List<KeyValuePair<ClothesData, ClothesInfo>>>(
                        clothesData);

                if (index >= updatedAccessoryList.Count)
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));

                    return;
                }

                var selectedItem = updatedAccessoryList[index];

                player.SetAccessory(selectedItem.Key.slot, selectedItem.Key.drawable, selectedItem.Key.texture);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                player.LoadCharacterCustomization();
                player.Position = player.Position;
                return;
            }
        }

        public static void EventAccessoryListSubItemChange(IPlayer player, int index, string itemText)
        {
            try
            {
                if (itemText == "Close") return;
                player.GetData(SUBCLOTHESDATA, out string clothesData);

                List<KeyValuePair<ClothesData, ClothesInfo>> updatedAccessoryList =
                    JsonConvert.DeserializeObject<List<KeyValuePair<ClothesData, ClothesInfo>>>(
                        clothesData);

                if (index >= updatedAccessoryList.Count)
                {
                    Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));

                    return;
                }

                var selectedItem = updatedAccessoryList[index];

                player.SetAccessory(selectedItem.Key.slot, selectedItem.Key.drawable, selectedItem.Key.texture);
            }
            catch (Exception e)
            {
                Clothes.LoadClothes(player,
                    JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson),
                    JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                player.LoadCharacterCustomization();
                player.Position = player.Position;
                Console.WriteLine(e);
                return;
            }
        }

        public static void EventAccessoryListSubSelect(IPlayer player, string option, int index)
        {
            try
            {
                if (option == "Close")
                {
                    Clothes.LoadClothes(player,
                        JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson),
                        JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                    return;
                }

                player.GetData(SUBCLOTHESDATA, out string clothesData);

                List<KeyValuePair<ClothesData, ClothesInfo>> accessoryList =
                    JsonConvert.DeserializeObject<List<KeyValuePair<ClothesData, ClothesInfo>>>(
                        clothesData);

                if (index >= accessoryList.Count)
                {
                    Clothes.LoadClothes(player,
                        JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson),
                        JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                    return;
                }

                KeyValuePair<ClothesData, ClothesInfo> selectedItem = accessoryList[index];

                player.SetData("CLOTHINGITEMSELECTED", JsonConvert.SerializeObject(selectedItem));

                player.GetData("INSTOREID", out int inStoreId);

                Models.Property property = Models.Property.FetchProperty(inStoreId);

                NativeMenu menu = new NativeMenu("PurchaseAccessoryMenu", property.BusinessName, "Choose an option", new List<NativeMenuItem>
                {
                    new NativeMenuItem("Purchase")
                });

                NativeUi.ShowNativeMenu(player, menu, true);
            }
            catch (Exception e)
            {
                Clothes.LoadClothes(player,
                    JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson),
                    JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                player.LoadCharacterCustomization();
                player.Position = player.Position;
                Console.WriteLine(e);
                return;
            }
        }

        public static void EventPurchaseAccessoryMenu(IPlayer player, string option)
        {
            try
            {
                CustomCharacter customCharacter =
                    JsonConvert.DeserializeObject<CustomCharacter>(player.FetchCharacter().CustomCharacter);

                Clothes.LoadClothes(player, JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson), JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));

                if (option == "Close")
                {
                    return;
                }

                player.GetData("CLOTHINGITEMSELECTED", out string clothingItemJson);

                KeyValuePair<ClothesData, ClothesInfo> selectedItem =
                    JsonConvert.DeserializeObject<KeyValuePair<ClothesData, ClothesInfo>>(
                        clothingItemJson);

                float playerMoney = player.FetchCharacter().Money;

                if (playerMoney < selectedItem.Value.Price)
                {
                    player.SendInfoNotification($"You don't have the funds for this!");
                    return;
                }

                using Context context = new Context();
                Models.Character playerCharacter = context.Character.Find(player.FetchCharacterId());

                Clothes.LoadClothes(player,
                    JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson),
                    JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                player.LoadCharacterCustomization();
                player.Position = player.Position;

                var clothesItem = selectedItem.Key;

                clothesItem.male = player.IsMale();

                //if (clothesItem.slot != 2)
                //{
                //    Inventory.Inventory playerInventory = player.FetchInventory();

                //    bool itemAdded = playerInventory.AddItem(Clothes.ConvertAccessoryToInventoryItem(clothesItem, player.IsMale()));

                //    if (itemAdded)
                //    {
                //        player.GetData("INSTOREID", out int extraId);

                //        player.RemoveCash(selectedItem.Value.Price);

                //        Models.Property extraProperty = Models.Property.FetchProperty(extraId);

                //        extraProperty?.AddToBalance(selectedItem.Value.Price);

                //        if (playerCharacter.Sex == 0)
                //        {
                //            player.SendInfoMessage($"You've bought {selectedItem.Value.DisplayNameMale} for {selectedItem.Value.Price:C}.");
                //            return;
                //        }

                //        player.SendInfoMessage($"You've bought {selectedItem.Value.DisplayNameFemale} for {selectedItem.Value.Price:C}.");
                //        return;
                //    }

                //    player.SendErrorMessage("You don't have enough space for this.");
                //    return;
                //}

                Inventory.Inventory playerInventory = player.FetchInventory();

                bool itemAdded = playerInventory.AddItem(Clothes.ConvertAccessoryToInventoryItem(clothesItem, player.IsMale()));

                if (itemAdded)
                {
                    player.GetData("INSTOREID", out int extraId);

                    player.RemoveCash(selectedItem.Value.Price);

                    Models.Property extraProperty = Models.Property.FetchProperty(extraId);

                    extraProperty?.AddToBalance(selectedItem.Value.Price);

                    if (playerCharacter.Sex == 0)
                    {
                        player.SendInfoNotification($"You've bought {selectedItem.Value.DisplayNameMale} for {selectedItem.Value.Price:C}.");
                        return;
                    }

                    player.SendInfoNotification($"You've bought {selectedItem.Value.DisplayNameFemale} for {selectedItem.Value.Price:C}.");
                    return;
                }

                player.SendErrorNotification("You don't have enough space for this.");
                return;

                //HairInfo hair = JsonConvert.DeserializeObject<HairInfo>(customCharacter.Hair);

                //hair.Hair = clothesItem.drawable;

                //customCharacter.Hair = JsonConvert.SerializeObject(hair);

                //playerCharacter.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

                //player.SetClothes(2, clothesItem.drawable, hair.Color);

                //context.SaveChanges();

                //

                //player.SendInfoMessage($"You've bought {selectedItem.Value.DisplayNameMale} for ~g~${selectedItem.Value.Price}~w~.");

                //player.GetData("INSTOREID", out int storeId);

                //player.RemoveCash(selectedItem.Value.Price);

                //Models.Property property = Models.Property.FetchProperty(storeId);

                //property?.AddToBalance(selectedItem.Value.Price);

                //player.Position = player.Position;
            }
            catch (Exception e)
            {
                Clothes.LoadClothes(player,
                    JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson),
                    JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson));
                player.LoadCharacterCustomization();
                player.Position = player.Position;
                Console.WriteLine(e);
                return;
            }
        }

        #endregion Accessory System
    }
}