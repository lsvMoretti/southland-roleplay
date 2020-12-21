using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Serilog;
using Server.Apartments;
using Server.Bank.Payment;
using Server.Character;
using Server.Character.Clothing;
using Server.Chat;
using Server.Commands;
using Server.Doors;
using Server.Extensions;
using Server.Inventory;
using Server.Models;
using Server.Motel;
using Server.Property.Stores;

namespace Server.Property
{
    public class PropertyCommands
    {
        [Command("buy", commandType: CommandType.Property, description: "Used to buy items from properties")]
        public static void BuyCommand(IPlayer player)
        {
            if (player.FetchCharacter() == null) return;

            bool hasCurrentWeaponData = player.GetData("CurrentWeaponHash", out uint weaponHash);

            if (hasCurrentWeaponData || weaponHash != 0)
            {
                player.SendErrorNotification("You must holster your weapon before the clerk can serve you.");
                return;
            }

            Models.Property property = Models.Property.FetchNearbyProperty(player, 5f);

            if (property == null)
            {
                List<Models.Property> properties = Models.Property.FetchProperties();

                foreach (var lProperty in properties)
                {
                    List<PropertyInteractionPoint> interactionPoints =
                        JsonConvert.DeserializeObject<List<PropertyInteractionPoint>>(lProperty.InteractionPoints);

                    if (!interactionPoints.Any()) continue;

                    foreach (var propertyInteractionPoint in interactionPoints)
                    {
                        Vector3 interactionPointPos = new Vector3(propertyInteractionPoint.PosX,
                            propertyInteractionPoint.PosY, propertyInteractionPoint.PosZ);

                        if (player.Position.Distance(interactionPointPos) <= 5f)
                        {
                            property = lProperty;
                            break;
                        }
                    }
                }

                if (property == null)
                {
                    player.SendErrorNotification($"You're not near a property.");
                    return;
                }
            }

            if (property.Products <= 0)
            {
                player.SendErrorNotification("This business is out of stock.");
                return;
            }

            player.SetData("INSTOREID", property.Id);

            bool hasWelcomeData = player.GetData(WelcomePlayer.WelcomeData, out int welcomeStage);

            if (property.PropertyType == PropertyType.LowEndClothes ||
                property.PropertyType == PropertyType.MedClothes || property.PropertyType == PropertyType.HighClothes)
            {
                if (hasWelcomeData)
                {
                    WelcomePlayer.OnBuyCommand(player);
                }
                ClothingStore.ShowClothesStoreMenu(player);
                return;
            }

            if (property.PropertyType == PropertyType.Hair)
            {
                HairStore.LoadHairStoreMenu(player);
                return;
            }

            if (property.PropertyType == PropertyType.Tattoo)
            {
                TattooStore.ShowTattooMenu(player);
                return;
            }

            if (property.PropertyType == PropertyType.Surgeon)
            {
                if (player.FetchCharacter()?.Money < CharacterCreator.SurgeonCost)
                {
                    player.SendErrorNotification($"You need {CharacterCreator.SurgeonCost:C} to carry out Surgery.");
                    return;
                }

                CharacterCreator.SendToCreator(player);
                return;
            }

            if (property.PropertyType == PropertyType.KeySmith)
            {
                KeySmith.ShowKeySmithMenu(player, property);
                return;
            }

            if (property.PropertyType == PropertyType.GunStore)
            {
                GunStore.ShowGunStoreMainMenu(player);
                return;
            }

            List<GameItem> itemList = JsonConvert.DeserializeObject<List<GameItem>>(property.ItemList);

            if (!itemList.Any())
            {
                player.SendErrorNotification($"No items available.");
                return;
            }

            NativeMenu shopMenu = new NativeMenu("PropertyBuyItem", "Purchase", property.BusinessName);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            List<GameItem> gameItems = new List<GameItem>();

            foreach (var item in itemList)
            {
                gameItems.Add(GameWorld.GetGameItem(item.ID));
            }

            if (gameItems.Any(x => x.ID.Contains("PHONE")))
            {
                if (hasWelcomeData)
                {
                    WelcomePlayer.OnBuyCommand(player);
                }
            }

            foreach (var gameItem in gameItems)
            {
                menuItems.Add(new NativeMenuItem(gameItem.Name, $"${gameItem.Price}"));
            }

            shopMenu.MenuItems = menuItems;

            player.SetData("BUYITEMLIST", JsonConvert.SerializeObject(gameItems));

            NativeUi.ShowNativeMenu(player, shopMenu, true);
        }

        public static void Remote_PropertyBuyItem(IPlayer player, string item)
        {
            if (item == "Close")
            {
                player.DeleteData("BUYITEMLIST");
                return;
            }

            player.GetData("BUYITEMLIST", out string itemList);

            player.DeleteData("BUYITEMLIST");

            player.GetData("INSTOREID", out int storeId);

            Models.Property property = Models.Property.FetchProperty(storeId);

            if (itemList == null) return;

            GameItem selectedItem = JsonConvert.DeserializeObject<List<GameItem>>(itemList)
                .FirstOrDefault(x => x.Name == item);

            Inventory.Inventory playerInventory = player.FetchInventory();

            float playerMoney = player.FetchCharacter().Money;

            if (playerMoney < selectedItem.Price)
            {
                player.SendErrorNotification($"You don't have enough money for this. {selectedItem.Price:C}.");
                return;
            }

            double price = selectedItem.Price;

            if (property.ClerkActive)
            {
                double reduction = price * 0.1;
                price -= reduction;
            }

            if (selectedItem.ID == "ITEM_BACKPACK" || selectedItem.ID == "ITEM_DUFFELBAG")
            {
                // Backpacks
                if (player.FetchCharacter().BackpackId > 0)
                {
                    player.SendErrorNotification("You already have a backpack equipped!");
                    return;
                }

                player.RemoveCash(price);

                Models.Backpack newBackpack = null;

                if (selectedItem.ID == "ITEM_BACKPACK")
                {
                    newBackpack = Models.Backpack.CreateBackpack(31);
                }

                if (selectedItem.ID == "ITEM_DUFFELBAG")
                {
                    newBackpack = Models.Backpack.CreateBackpack(44);
                }

                if (newBackpack == null)
                {
                    player.SendErrorNotification("There was an error creating the backpack!");
                    return;
                }

                bool backPackAdded = playerInventory.AddItem(new InventoryItem(selectedItem.ID, selectedItem.Name, newBackpack.Id.ToString()));

                if (!backPackAdded)
                {
                    using Context context = new Context();

                    context.Backpacks.Remove(newBackpack);

                    context.SaveChanges();

                    player.SendErrorNotification("There was an error adding the backpack to your inventory!");
                    return;
                }

                using Context playerContext = new Context();

                Models.Character playerCharacter = playerContext.Character.Find(player.GetClass().CharacterId);

                playerCharacter.BackpackId = newBackpack.Id;

                playerContext.SaveChanges();

                player.LoadCharacterCustomization();

                player.SendInfoNotification($"You've bought {selectedItem.Name} for {price:C}");
                return;
            }

            if (selectedItem.ID == "ITEM_SPRAYCAN")
            {
                player.RemoveCash(price);

                bool sprayCanAdded =
                    playerInventory.AddItem(new InventoryItem(selectedItem.ID, selectedItem.Name, 5.ToString()));

                if (!sprayCanAdded)
                {
                    player.SendErrorNotification($"Your inventory is full");
                    return;
                }

                player.SendInfoNotification($"You've bought {selectedItem.Name} for {price:C}");
                return;
            }

            if (selectedItem.ID == "ITEM_GRAFFITISTRIPPER")
            {
                player.RemoveCash(price);

                bool graffitiStripper =
                    playerInventory.AddItem(new InventoryItem(selectedItem.ID, selectedItem.Name, 5.ToString()));

                if (!graffitiStripper)
                {
                    player.SendErrorNotification($"Your inventory is full");
                    return;
                }

                player.SendInfoNotification($"You've bought {selectedItem.Name} for {price:C}");
                return;
            }

            if (selectedItem.ID == "ITEM_SPRAYCAN")
            {
                player.RemoveCash(price);

                bool sprayCanAdded =
                    playerInventory.AddItem(new InventoryItem(selectedItem.ID, selectedItem.Name, 5.ToString()));

                if (!sprayCanAdded)
                {
                    player.SendErrorNotification($"Your inventory is full");
                    return;
                }

                player.SendInfoNotification($"You've bought {selectedItem.Name} for {price:C}");
                return;
            }

            if (selectedItem.ID == "ITEM_GRAFFITISTRIPPER")
            {
                player.RemoveCash(price);

                bool graffitiStripper =
                    playerInventory.AddItem(new InventoryItem(selectedItem.ID, selectedItem.Name, 5.ToString()));

                if (!graffitiStripper)
                {
                    player.SendErrorNotification($"Your inventory is full");
                    return;
                }

                player.SendInfoNotification($"You've bought {selectedItem.Name} for {price:C}");
                return;
            }

            if (selectedItem.ID == $"ITEM_PHONE" || selectedItem.ID == $"ITEM_EXPENSIVEPHONE")
            {
                Phones newPhone = Phones.CreatePhone(player.FetchCharacterId());

                if (newPhone == null)
                {
                    newPhone = Phones.CreatePhone(player.FetchCharacterId());
                }

                if (newPhone == null)
                {
                    player.SendErrorNotification($"There was an error creating your phone.");
                    return;
                }

                bool itemAdded =
                    playerInventory.AddItem(new InventoryItem(selectedItem.ID, selectedItem.Name,
                        newPhone.PhoneNumber));

                if (!itemAdded)
                {
                    player.SendErrorNotification($"There was an issue adding this to your Inventory!");
                    using Context context = new Context();
                    var phoneDb = context.Phones.Find(newPhone.Id);

                    context.Phones.Remove(phoneDb);
                    context.SaveChanges();

                    return;
                }

                player.RemoveCash(price);

                player.SendInfoNotification($"You’ve bought {selectedItem.Name} for {price:C}.");
                player.SendInfoNotification($"Your new phone number is: {newPhone.PhoneNumber}");
                return;
            }

            if (selectedItem.ID.Contains("MELEE"))
            {
                WeaponInfo newWeaponInfo = new WeaponInfo(1, true, player.GetClass().Name);

                bool itemAdded = playerInventory.AddItem(new InventoryItem(selectedItem.ID, selectedItem.Name,
                    JsonConvert.SerializeObject(newWeaponInfo)));

                if (!itemAdded)
                {
                    player.SendErrorNotification("You don't have space!");
                    return;
                }

                player.RemoveCash(price);

                player.SendInfoNotification($"You’ve bought {selectedItem.Name} for {price:C}.");

                return;
            }

            bool added = playerInventory.AddItem(new InventoryItem(selectedItem.ID, selectedItem.Name));

            if (!added)
            {
                player.SendErrorNotification($"Your inventory is full");
                return;
            }

            property.AddToBalance(selectedItem.Price);
            player.RemoveCash(price);

            player.SendInfoNotification($"You've bought {selectedItem.Name} for {price:C}");
        }

        [Command("enter", commandType: CommandType.Property, description: "Used to enter properties")]
        public static void CommandEnterProperty(IPlayer player)
        {
            if (player.GetClass().Downed)
            {
                player.SendErrorNotification("You can't do that when downed.");
                return;
            }

            MotelRoom nearRoom = MotelHandler.FetchNearestMotelRoom(player.Position);

            if (nearRoom != null)
            {
                if (nearRoom.Locked)
                {
                    player.SendErrorNotification("Room locked.");
                    return;
                }

                MotelHandler.SetPlayerIntoMotelRoom(player, nearRoom);
                return;
            }

            ApartmentComplexes nearestComplex = ApartmentComplexes.FetchNearestApartmentComplex(player, 5f);

            if (nearestComplex != null)
            {
                ApartmentHandler.LoadApartmentList(player, nearestComplex);
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 4f);

            if (nearestProperty == null) return;

            if (nearestProperty.Locked)
            {
                player.SendNotification($"~r~Property is locked.");
                return;
            }

            if (!nearestProperty.Enterable)
            {
                player.SendErrorNotification("You can't enter here.");
                return;
            }

            Interiors interior = Interiors.InteriorList.FirstOrDefault(x =>
                x.InteriorName == nearestProperty.InteriorName && x.Ipl == nearestProperty.Ipl);

            if (interior == null)
            {
                player.SendErrorNotification("An error occurred fetching the interior.");
                return;
            }

            if (!string.IsNullOrEmpty(interior.Ipl))
            {
                if (interior.Ipl == "ch_DLC_Arcade")
                {
                    player.RequestIpl("ch_DLC_Plan");
                }

                player.RequestIpl(interior.Ipl);
            }

            if (interior.IsMapped == true)
            {
                player.SetPosition(interior.Position, player.Rotation, 5000, unfreezeTime: 5000);
            }
            else
            {
                player.Position = interior.Position;
            }

            int dimension = nearestProperty.Id;

            player.Dimension = dimension;
            player.SetSyncedMetaData("PlayerDimension", dimension);

            DoorHandler.UpdateDoorsForPlayer(player);

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.FetchCharacterId());

            if (playerCharacter == null) return;

            playerCharacter.InsideProperty = nearestProperty.Id;
            playerCharacter.Dimension = dimension;

            context.SaveChanges();

            if (!string.IsNullOrEmpty(nearestProperty.MusicStation))
            {
                player.PlayMusicFromUrl(nearestProperty.MusicStation);
            }

            List<string> propList = JsonConvert.DeserializeObject<List<string>>(nearestProperty.PropList);

            if (propList.Any())
            {
                foreach (string prop in propList)
                {
                    player.LoadInteriorProp(prop);
                }
            }
        }

        [Command("exit", commandType: CommandType.Property, description: "Used to exit properties")]
        public static void CommandExitProperty(IPlayer player)
        {
            if (player.GetClass().Downed)
            {
                player.SendErrorNotification("You can't do that when downed.");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            if (playerCharacter.InMotelRoom > 0)
            {
                MotelHandler.SetPlayerOutOfMotelRoom(player);
                return;
            }

            if (playerCharacter.InsideApartmentComplex > 0)
            {
                ApartmentHandler.LeaveApartment(player);
                return;
            }

            using Context context = new Context();

            Models.Property insideProperty = Models.Property.FetchProperty(playerCharacter.InsideProperty);

            if (insideProperty == null) return;

            if (insideProperty.Locked)
            {
                player.SendNotification($"~r~Property is locked.");
                return;
            }

            Models.Character playerCharacterDb = context.Character.Find(playerCharacter.Id);

            if (playerCharacterDb == null) return;

            playerCharacterDb.InsideProperty = insideProperty.ExtDimension;

            playerCharacterDb.Dimension = insideProperty.ExtDimension;

            context.SaveChanges();

            player.Position = insideProperty.FetchExteriorPosition();

            player.Dimension = (short)insideProperty.ExtDimension;
            player.SetSyncedMetaData("PlayerDimension", player.Dimension);

            DoorHandler.UpdateDoorsForPlayer(player);

            player.UnloadIpl(insideProperty.Ipl);

            if (!string.IsNullOrEmpty(insideProperty.MusicStation))
            {
                player.StopMusic();
            }

            List<string> propList = JsonConvert.DeserializeObject<List<string>>(insideProperty.PropList);

            if (propList.Any())
            {
                foreach (string prop in propList)
                {
                    player.UnloadInteriorProp(prop);
                }
            }
        }

        [Command("setrequiredproducts", onlyOne: true, commandType: CommandType.Property,
            description: "Business: Used to set the amount of required products. /setrequiredproducts [Amount]")]
        public static void PropertyCommandSetRequiredProducts(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setrequiredproducts [Amount]");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You must be near a property.");
                return;
            }

            bool tryAmountParse = int.TryParse(args, out int amount);

            if (!tryAmountParse)
            {
                player.SendErrorNotification("Parameter must be numeric.");
                return;
            }

            if (nearestProperty.OwnerId != playerCharacter.Id)
            {
                player.SendErrorNotification("You must be the owner of the business.");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearestProperty.Id);

            property.RequiredProducts = amount;

            context.SaveChanges();

            player.SendInfoNotification($"You've set {nearestProperty.BusinessName} required products to {amount}.");

            Logging.AddToCharacterLog(player,
                $"Has set required products for {nearestProperty.BusinessName} (ID: {nearestProperty.Id}) to {amount}.");
        }

        [Command("setproductprice", onlyOne: true, commandType: CommandType.Property,
            description: "Business: Used to set product buy price per product. /setproductprice [Price]")]
        public static void CommandSetProductPrice(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setproductprice [Price]");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You must be near a property.");
                return;
            }

            bool tryAmountParse = double.TryParse(args, out double amount);

            if (!tryAmountParse)
            {
                player.SendErrorNotification("Parameter must be numeric.");
                return;
            }

            if (nearestProperty.OwnerId != playerCharacter.Id)
            {
                player.SendErrorNotification("You must be the owner of the business.");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearestProperty.Id);

            property.ProductBuyPrice = amount;

            context.SaveChanges();

            player.SendInfoNotification($"You've set {nearestProperty.BusinessName} product buy price to {amount:C}.");

            Logging.AddToCharacterLog(player,
                $"Has set product buy price for {nearestProperty.BusinessName} (ID: {nearestProperty.Id}) to {amount:C}.");
        }

        [Command("pwithdraw", onlyOne: true, commandType: CommandType.Property,
            description: "Used to withdraw money from a property. /pwithdraw [Amount]")]
        public static void PropertyCommandWithdraw(IPlayer player, string args = "")
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/pwithdraw [Amount]");
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f) ?? Models.Property.FetchProperty(player.Dimension);

            if (nearestProperty == null || nearestProperty.PropertyType == PropertyType.House)
            {
                player.SendErrorNotification("You're not near a business.");
                return;
            }

            if (nearestProperty.OwnerId != playerCharacter.Id)
            {
                player.SendPermissionError();
                return;
            }

            bool amountParse = double.TryParse(args, out double amount);

            if (!amountParse)
            {
                player.SendErrorNotification("Parameter must be generic.");
                return;
            }

            if (amount > nearestProperty.Balance || amount <= 0)
            {
                player.SendErrorNotification(
                    $"You can't withdraw this much. You only have {nearestProperty.Balance:C} in your business.");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearestProperty.Id);

            property.Balance -= amount;
            player.AddCash(amount);

            context.SaveChanges();

            player.SendInfoNotification($"You've withdraw {amount:C} from your business.");

            Logging.AddToCharacterLog(player,
                $"has withdrawn {amount:C} from the business {nearestProperty.BusinessName} ID: {nearestProperty.Id}.");
        }

        [Command("pdeposit", onlyOne: true, commandType: CommandType.Property,
            description: "Used to withdraw money from a property. /pdeposit [Amount]")]
        public static void PropertyCommandDeposit(IPlayer player, string args = "")
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/pdeposit [Amount]");
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f) ?? Models.Property.FetchProperty(player.Dimension);

            if (nearestProperty == null || nearestProperty.PropertyType == PropertyType.House)
            {
                player.SendErrorNotification("You're not near a business.");
                return;
            }

            if (nearestProperty.OwnerId != playerCharacter.Id)
            {
                player.SendPermissionError();
                return;
            }

            bool amountParse = double.TryParse(args, out double amount);

            if (!amountParse)
            {
                player.SendErrorNotification("Parameter must be generic.");
                return;
            }

            if (amount > playerCharacter.Money || amount <= 0)
            {
                player.SendErrorNotification($"You can't deposit this much. You only have {playerCharacter.Money:C}.");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearestProperty.Id);

            property.Balance += amount;
            player.RemoveCash(amount);

            context.SaveChanges();

            player.SendInfoNotification($"You've deposited {amount:C} into your business.");

            Logging.AddToCharacterLog(player,
                $"has deposited {amount:C} into the business {nearestProperty.BusinessName} ID: {nearestProperty.Id}.");
        }

        [Command("mortgageproperty", commandType: CommandType.Property,
            description: "Mortgage: Used to mortgage a property")]
        public static void PropertyCommandMortgage(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You must be near a property.");
                return;
            }

            if (nearestProperty.OwnerId > 0 || nearestProperty.Value <= 0)
            {
                player.SendErrorNotification("This property is already owned.");
                return;
            }

            bool isMortgaging = Models.Property.FetchCharacterProperties(player.FetchCharacter())
                .Any(x => x.MortgageValue > 0);

            if (isMortgaging)
            {
                player.SendErrorNotification("You are already mortgaging a property!");
                return;
            }

            double depositAmount = nearestProperty.Value * 0.2;

            if (player.GetClass().Cash < depositAmount)
            {
                player.SendErrorNotification($"You need {depositAmount:C} in cash to mortgage this property.");
                return;
            }

            using Context context = new Context();

            Models.Property propertyDb = context.Property.Find(nearestProperty.Id);

            if (propertyDb == null)
            {
                player.SendErrorNotification("An error occurred fetching the property.");
                return;
            }

            double mortgageValue = propertyDb.Value * 1.05;

            Console.WriteLine($"Mortgage Value: {mortgageValue:C}, Deposit Amount: {depositAmount:C}, Property Value: {propertyDb.Value:C}.");

            mortgageValue -= depositAmount;

            Console.WriteLine($"Mortgage Left: {mortgageValue:C}");

            propertyDb.MortgageValue = mortgageValue;

            propertyDb.LastMortgagePayment = DateTime.Now;

            Inventory.Inventory playerInventory = player.FetchInventory();

            string newKey = Utility.GenerateRandomString(8);

            propertyDb.OwnerId = player.GetClass().CharacterId;

            player.RemoveCash(depositAmount);

            propertyDb.PurchaseDateTime = DateTime.Now;

            propertyDb.Key = newKey;

            playerInventory.AddItem(new InventoryItem("ITEM_PROPERTY_KEY", propertyDb.Address, newKey));

            context.SaveChanges();

            player.SendInfoNotification($"You've started to mortgage {propertyDb.Address}. Deposited {depositAmount:C}. Left to pay {mortgageValue:C}.");
            player.SendInfoNotification($"Next mortgage payment should be made by {propertyDb.LastMortgagePayment.AddMonths(2)}.");

            LoadProperties.ReloadProperty(propertyDb);
        }

        [Command("buyproperty", commandType: CommandType.Property, description: "Used to purchase properties.")]
        public static void PropertyCommandBuyProperty(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You must be near a property.");
                return;
            }

            if (nearestProperty.OwnerId > 0 || nearestProperty.Value <= 0)
            {
                player.SendErrorNotification("This property is already owned.");
                return;
            }

            player.SetData("property:PurchasingProperty", nearestProperty.Id);

            PaymentHandler.ShowPaymentSelection(player, "property:PurchasePropertyHandler");
        }

        public static void OnPurchasePropertySelectPayment(IPlayer player, string option)
        {
            player.FreezePlayer(false);
            player.FreezeCam(false);
            player.FreezeInput(false);
            player.FreezePlayer(false);
            player.ChatInput(true);
            player.HideChat(false);
            player.ShowHud(true);
            if (option == "IncorrectPin")
            {
                player.SendErrorNotification("Incorrect PIN.");
                return;
            }

            if (option == "close") return;

            player.GetData("property:PurchasingProperty", out int propertyId);

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            Models.Property property = context.Property.Find(propertyId);

            Inventory.Inventory playerInventory = player.FetchInventory();

            string newKey = Utility.GenerateRandomString(8);

            if (option == "cash")
            {
                if (playerCharacter.Money < property.Value)
                {
                    player.SendErrorNotification($"You don't have the required amount of {property.Value:C} on you.");

                    return;
                }

                property.OwnerId = playerCharacter.Id;

                playerCharacter.Money -= property.Value;

                property.PurchaseDateTime = DateTime.Now;

                property.Key = newKey;

                playerInventory.AddItem(new InventoryItem("ITEM_PROPERTY_KEY", property.Address, newKey));

                player.SendInfoNotification($"You've bought {property.Address} for {property.Value:C}.");

                context.SaveChanges();

                LoadProperties.ReloadProperty(property);

                return;
            }

            bool accountParse = long.TryParse(option, out long accNo);

            if (!accountParse)
            {
                player.SendErrorNotification("An error occurred passing the account information across.");
                return;
            }

            BankAccount bankAccount = context.BankAccount.FirstOrDefault(x => x.AccountNumber == accNo);

            if (bankAccount == null)
            {
                player.SendErrorNotification("This bank account doesn't exist.");
                return;
            }

            if (bankAccount.OwnerId != playerCharacter.Id)
            {
                player.SendErrorNotification("You must be the owner of the account to purchase this.");
                return;
            }

            if (bankAccount.Balance < property.Value)
            {
                player.SendErrorNotification(
                    $"You require {property.Value:C} and only have {bankAccount.Balance:C} in your account.");
                return;
            }

            bankAccount.Balance -= property.Value;

            property.OwnerId = playerCharacter.Id;
            property.PurchaseDateTime = DateTime.Now;

            property.Key = newKey;

            playerInventory.AddItem(new InventoryItem("ITEM_PROPERTY_KEY", property.Address, newKey));

            player.SendInfoNotification($"You've bought {property.Address} for {property.Value:C}.");

            context.SaveChanges();

            LoadProperties.ReloadProperty(property);
        }

        [Command("sellproperty", commandType: CommandType.Property, description: "Used to sell a property.")]
        public static void PropertyCommandSellProperty(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            List<Models.Property> properties = Models.Property.FetchCharacterProperties(player.FetchCharacter());

            if (!properties.Any())
            {
                player.SendErrorNotification("You don't own any properties.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Models.Property property in properties)
            {
                menuItems.Add(new NativeMenuItem(property.Address, $"{property.Value / 2:C}"));
            }

            NativeMenu menu = new NativeMenu("property:SellPropertySelectProperty", "Properties",
                "Select a Property to sell", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSellPropertySelectProperty(IPlayer player, string option)
        {
            if (option == "Close") return;

            Models.Property property = Models.Property.FetchCharacterProperties(player.FetchCharacter())
                .FirstOrDefault(x => x.Address == option);

            if (property == null)
            {
                player.SendErrorNotification($"Unable to find a property by the address of {option}.");
                return;
            }

            if (property.MortgageValue > 0)
            {
                HandleSellMortgageProperty(player, property);
                return;
            }

            player.SetData("SELLINGPROPERTY", property.Id);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Player"),
                new NativeMenuItem("State")
            };

            NativeMenu menu = new NativeMenu("property:SelectPropertySellType", "Properties",
                "Select the method you wish to sell", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void HandleSellMortgageProperty(IPlayer player, Models.Property property)
        {
            double depositAmount = property.Value * 0.2;
            double mortgageValue = property.Value * 1.05;
            double afterPayment = mortgageValue - depositAmount;
            double totalPayments = afterPayment - property.MortgageValue;
            double refundAmount = (totalPayments * 0.8) + depositAmount;

            Console.WriteLine($"Deposit: {depositAmount:C}, Mortgage Value: {mortgageValue:C}, After Init Deposit Value: {afterPayment:C}, Mortgage Paid Off: {totalPayments:C}. Total Refund: {refundAmount:C}.");

            Logging.AddToCharacterLog(player, $"Is looking to sell their mortgage property. Deposit: {depositAmount:C}, Mortgage Value: {mortgageValue:C}, After Init Deposit Value: {afterPayment:C}, Mortgage Paid Off: {totalPayments:C}. Total Refund: {refundAmount:C}.");

            player.SetData("SellMortgageProperty", property.Id);

            NativeUi.ShowYesNoMenu(player, "Property:ConfirmSellMortgageProperty", "Confirm Sale", $"{refundAmount:C}");
        }

        public static void OnSelectMortgageSale(IPlayer player, string option)
        {
            player.GetData("SellMortgageProperty", out int propertyId);

            if (option == "No")
            {
                player.DeleteData("SellMortgageProperty");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(propertyId);

            if (property == null)
            {
                player.SendErrorNotification("There was an error fetching the property.");
                return;
            }

            double depositAmount = property.Value * 0.2;
            double mortgageValue = property.Value * 1.05;
            double afterPayment = mortgageValue - depositAmount;
            double totalPayments = afterPayment - property.MortgageValue;
            double refundAmount = (totalPayments * 0.8) + depositAmount;

            BankAccount bankAccount = BankAccount.FetchBankAccount(player.FetchCharacter().PaydayAccount);

            if (bankAccount == null)
            {
                player.SendErrorNotification("Unable to find your active bank account!");
                return;
            }

            float newBalance = bankAccount.Balance += (float)refundAmount;

            BankAccount.UpdateBalance(bankAccount, newBalance);

            property.OwnerId = 0;
            property.Key = Utility.GenerateRandomString(8);
            property.MortgageValue = 0;

            context.SaveChanges();

            LoadProperties.LoadProperty(property);
            player.SendInfoNotification($"You've sold {property.Address} for {refundAmount:C} (after fee's).");
        }

        public static void OnSelectPropertySellType(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("SELLINGPROPERTY", out int propertyId);

            if (option == "State")
            {
                using Context context = new Context();

                Models.Property selectedProperty = context.Property.Find(propertyId);

                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                if (selectedProperty == null)
                {
                    player.SendErrorNotification("There was an error fetching the property.");
                    return;
                }

                if (selectedProperty.Value <= 0)
                {
                    player.SendErrorNotification("You're unable to sell this property.");
                    return;
                }

                if (selectedProperty.VoucherUsed)
                {
                    int compareBack = DateTime.Compare(DateTime.Now, selectedProperty.PurchaseDateTime.AddDays(3));

                    LifestyleChoice propertyVoucher = Models.Property.FetchPropertyLifestyle(selectedProperty);

                    int voucherValue = PropertyHandler.FetchVoucherValue(propertyVoucher);

                    if (compareBack < 0)
                    {
                        if (playerCharacter.HouseVoucherPurchases == 1)
                        {
                            // Voucher used once. Option to choose to want voucher value or return it.

                            List<NativeMenuItem> voucherMenuItems = new List<NativeMenuItem>
                            {
                                new NativeMenuItem("Voucher", "The voucher will be returned to you"),
                                new NativeMenuItem("Voucher Value", $"Voucher Value: {voucherValue:C}")
                            };

                            NativeMenu menu = new NativeMenu("property:SellVoucherOptions", "Properties",
                                "Select an option", voucherMenuItems);

                            NativeUi.ShowNativeMenu(player, menu, true);

                            return;
                        }

                        if (playerCharacter.HouseVoucherPurchases == 2)
                        {
                            player.AddCash(voucherValue);

                            player.SendInfoNotification(
                                $"You've used the voucher twice. The value {voucherValue:C} has been given to you.");

                            selectedProperty.OwnerId = 0;
                            selectedProperty.Key = Utility.GenerateRandomString(8);
                            selectedProperty.BuyinPaid = 0;
                            selectedProperty.BuyinValue = 0;

                            context.SaveChanges();

                            LoadProperties.ReloadProperty(selectedProperty);
                            return;
                        }
                    }

                    if (compareBack > 0)
                    {
                        int moneyBack = voucherValue + selectedProperty.BuyinPaid;

                        player.AddCash(moneyBack);

                        player.SendInfoNotification(
                            $"You've sold the property and received the voucher value and what you've put in.");
                        player.SendInfoNotification($"Amount refunded: {moneyBack:C}");

                        selectedProperty.OwnerId = 0;
                        selectedProperty.Key = Utility.GenerateRandomString(8);
                        selectedProperty.BuyinPaid = 0;
                        selectedProperty.BuyinValue = 0;

                        context.SaveChanges();

                        LoadProperties.ReloadProperty(selectedProperty);

                        return;
                    }
                }

                int paybackPrice = selectedProperty.Value / 2;

                player.SendInfoNotification(
                    $"You've bought the property for {selectedProperty.Value:C} and will receive 50% of this.");
                player.SendInfoNotification($"You've received {paybackPrice:C}.");

                player.AddCash(paybackPrice);

                selectedProperty.OwnerId = 0;
                selectedProperty.Key = Utility.GenerateRandomString(8);
                context.SaveChanges();

                LoadProperties.ReloadProperty(selectedProperty);
                return;
            }

            if (option == "Player")
            {
                List<IPlayer> playerList = Alt.Server.GetPlayers().Where(x =>
                    x.FetchCharacter() != null && x.Position.Distance(player.Position) <= 5f).ToList();

                if (!playerList.Any())
                {
                    player.SendErrorNotification("You don't have any players around you.");
                    return;
                }

                var orderedList = playerList.OrderByDescending(x => x.Position.Distance(player.Position));

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                foreach (IPlayer target in orderedList)
                {
                    menuItems.Add(new NativeMenuItem(target.GetClass().Name));
                }

                NativeMenu menu = new NativeMenu("property:sell:selectPlayer", "Properties", "Select a targetPlayer",
                    menuItems);

                NativeUi.ShowNativeMenu(player, menu, true);
            }
        }

        public static void OnPropertySellSelectPlayer(IPlayer player, string option)
        {
            if (option == "Close") return;

            IPlayer targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetClass().Name == option);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find this targetPlayer.");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("This targetPlayer isn't logged in.");
                return;
            }

            if (player.Position.Distance(targetPlayer.Position) > 5)
            {
                player.SendErrorNotification("The targetPlayer isn't nearby.");
                return;
            }

            player.GetData("SELLINGPROPERTY", out int propertyId);

            Models.Property property = Models.Property.FetchProperty(propertyId);

            targetPlayer.SetData("BUYINGPROPERTY", propertyId);

            targetPlayer.SendInfoNotification($"{player.GetClass().Name} is offering you {property.Address}.");

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Yes"),
                new NativeMenuItem("No")
            };

            NativeMenu menu = new NativeMenu("property:sell:offerToPlayer", "Properties", property.Address, menuItems);

            NativeUi.ShowNativeMenu(targetPlayer, menu, true);
        }

        public static void PropertySellOfferToPlayer(IPlayer targetPlayer, string option)
        {
            if (option == "Close" || option == "No")
            {
                targetPlayer.SendInfoNotification($"Deal declined.");
                return;
            }

            targetPlayer.GetData("BUYINGPROPERTY", out int propertyId);

            Models.Property property = Models.Property.FetchProperty(propertyId);

            IPlayer player = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetClass().CharacterId == property.OwnerId);

            if (player == null)
            {
                targetPlayer.SendErrorNotification("Buyer not found.");
                return;
            }

            using Context context = new Context();

            Models.Property propertyDb = context.Property.Find(property.Id);

            string oldKey = propertyDb.Key;

            propertyDb.OwnerId = targetPlayer.GetClass().CharacterId;

            string newKey = Utility.GenerateRandomString(8);

            propertyDb.Key = newKey;

            propertyDb.PurchaseDateTime = DateTime.Now;

            context.SaveChanges();

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> oldKeys = playerInventory.GetInventoryItems("ITEM_PROPERTY_KEY")
                .Where(x => x.ItemValue == oldKey).ToList();

            foreach (InventoryItem oldInventoryKey in oldKeys)
            {
                playerInventory.RemoveItem(oldInventoryKey);
            }

            Inventory.Inventory targetInventory = targetPlayer.FetchInventory();

            targetInventory.AddItem(new InventoryItem("ITEM_PROPERTY_KEY", property.Address, newKey));

            player.SendInfoNotification(
                $"You've transferred ownership of {property.Address} to {targetPlayer.GetClass().Name}.");

            targetPlayer.SendInfoNotification($"You've received {property.Address} from {player.GetClass().Name}.");

            Logging.AddToCharacterLog(player,
                $"has given {property.Address} (ID: {property.Id}) to {targetPlayer.GetClass().Name} (ID: {targetPlayer.GetClass().CharacterId}).");

            Logging.AddToCharacterLog(targetPlayer,
                $"has received {property.Address} (ID: {property.Id}) from {player.GetClass().Name} (ID: {player.GetClass().CharacterId}).");
            LoadProperties.ReloadProperty(property);
        }

        public static void SellVoucherOption(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("SELLINGPROPERTY", out int propertyId);

            using Context context = new Context();

            Models.Property selectedProperty = context.Property.Find(propertyId);

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            selectedProperty.OwnerId = 0;
            selectedProperty.Key = Utility.GenerateRandomString(8);
            selectedProperty.BuyinPaid = 0;
            selectedProperty.BuyinValue = 0;

            if (option == "Voucher")
            {
                playerCharacter.HouseVoucher = true;

                context.SaveChanges();

                Logging.AddToCharacterLog(player,
                    $"has sold {selectedProperty.Address} and the voucher has been return to them.");

                player.SendInfoNotification(
                    $"You've sold {selectedProperty.Address} and the voucher has been returned to you.");
                return;
            }

            context.SaveChanges();

            LifestyleChoice propertyVoucher = Models.Property.FetchPropertyLifestyle(selectedProperty);

            int voucherValue = PropertyHandler.FetchVoucherValue(propertyVoucher);

            Logging.AddToCharacterLog(player,
                $"has sold {selectedProperty.Address} and chosen voucher value of {voucherValue:C}");

            player.AddCash(voucherValue);

            player.SendInfoNotification(
                $"You've sold {selectedProperty.Address} and the voucher value of {voucherValue:C} has been given to you.");
            return;
        }

        [Command("plock")]
        public static bool CommandPropertyLock(IPlayer player)
        {
            if (player == null) return false;

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                if (player.Dimension > 0 && player.FetchCharacter().InsideGarage == 0)
                {
                    // Inside Property or Apartment
                    if (string.IsNullOrEmpty(player.FetchCharacter().InsideApartment))
                    {
                        nearestProperty = Models.Property.FetchProperty(player.Dimension);
                        if (nearestProperty == null) return false;
                    }
                    else return false;
                }
                else
                {
                    List<Models.Property> properties = Models.Property.FetchProperties();

                    Position playerPosition = player.Position;

                    foreach (Models.Property property in properties)
                    {
                        List<PropertyDoor> propertyDoors =
                            JsonConvert.DeserializeObject<List<PropertyDoor>>(property.DoorPositions);

                        if (!propertyDoors.Any()) continue;

                        foreach (PropertyDoor propertyDoor in propertyDoors)
                        {
                            Position doorPosition = new Position(propertyDoor.EnterPosX, propertyDoor.EnterPosY, propertyDoor.EnterPosZ);

                            if (doorPosition.Distance(playerPosition) < 5 &&
                                player.Dimension == propertyDoor.EnterDimension)
                            {
                                nearestProperty = property;
                                break;
                            }
                        }
                    }
                }
                if (nearestProperty == null) return false;
            }

            using Context context = new Context();

            Models.Property propertyDb = context.Property.Find(nearestProperty.Id);

            Inventory.Inventory playerInventory = player.FetchInventory();

            bool hasKey = playerInventory.GetInventoryItems("ITEM_PROPERTY_KEY")
                .FirstOrDefault(x => x.ItemValue == propertyDb.Key) != null;

            if (!hasKey)
            {
                hasKey = player.GetClass().AdminDuty;
            }

            if (!hasKey)
            {
                player.SendErrorNotification("You don't have the keys!");
                return true;
            }

            propertyDb.Locked = !propertyDb.Locked;

            context.SaveChanges();

            player.SendInfoNotification(propertyDb.Locked
                ? "You have locked the property."
                : "You have unlocked the property.");

            Logging.AddToCharacterLog(player,
                $"Has set property Id {propertyDb.Id} lock status to {propertyDb.Locked}");

            return true;
        }

        [Command("setstorage", commandType: CommandType.Property, description: "Storage: Used to set storage location")]
        public static void PropertyCommandSetStorage(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (player.Dimension > 0 && !string.IsNullOrEmpty(playerCharacter.InsideApartment))
            {
                player.SendErrorNotification("You can't do this in an apartment right now!");
                return;
            }

            Models.Property playerProperty = Models.Property.FetchProperty(player.Dimension);

            if (playerProperty == null)
            {
                player.SendErrorNotification("You're not inside a property.");
                return;
            }

            if (playerCharacter.Id != playerProperty.OwnerId)
            {
                player.SendErrorNotification("You must be the owner.");
                return;
            }

            if (playerProperty.InventoryId > 0)
            {
                player.SendErrorNotification("You already have a location set.");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(playerProperty.Id);

            InventoryData newData = InventoryData.CreateDefaultInventory(60f, 50f);

            property.InvPosX = player.Position.X;
            property.InvPosY = player.Position.Y;
            property.InvPosZ = player.Position.Z;
            property.InventoryId = newData.ID;

            context.SaveChanges();

            player.SendInfoNotification($"You've set the storage location.");
        }

        [Command("pinv", commandType: CommandType.Property, description: "Storage: Used to view your property storage")]
        public static void PropertyCommandViewPInventory(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (player.Dimension > 0 && !string.IsNullOrEmpty(playerCharacter.InsideApartment))
            {
                player.SendErrorNotification("You can't do this in an apartment right now!");
                return;
            }

            Models.Property playerProperty = Models.Property.FetchProperty(player.Dimension);

            if (playerProperty == null)
            {
                player.SendErrorNotification("You're not inside a property.");
                return;
            }

            if (playerProperty.InventoryId == 0) return;

            Position inventoryPosition =
                new Position(playerProperty.InvPosX, playerProperty.InvPosY, playerProperty.InvPosZ);

            if (player.Position.Distance(inventoryPosition) > 3) return;

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Store"),
                new NativeMenuItem("Take")
            };

            NativeMenu menu = new NativeMenu("property:storage:SelectMainOption", "Inventory", "Select an option",
                menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);

            player.SetData("InsidePropertyInventory", playerProperty.Id);
        }

        public static void OnPropertyInventorySelectMainMenu(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Store")
            {
                ShowPropertyStoreMenu(player);
                return;
            }

            if (option == "Take")
            {
                ShowPropertyTakeMenu(player);
                return;
            }
        }

        private static void ShowPropertyStoreMenu(IPlayer player)
        {
            Inventory.Inventory playerInventory = player.FetchInventory();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            List<InventoryItem> inventoryItems = playerInventory.GetInventory();

            player.GetData("InsidePropertyInventory", out int currentPropertyId);

            Models.Property insideProperty = Models.Property.FetchProperty(currentPropertyId);

            Inventory.Inventory propertyInventory = Models.Property.FetchInventory(insideProperty);

            if (propertyInventory == null)
            {
                player.SendErrorNotification("An error occurred.");
                player.DeleteData("InsidePropertyInventory");
                return;
            }

            int backPackCount = propertyInventory.GetInventoryItems("ITEM_BACKPACK").Count;

            backPackCount += propertyInventory.GetInventoryItems("ITEM_DUFFELBAG").Count;

            foreach (InventoryItem inventoryItem in inventoryItems)
            {
                if (inventoryItem.Id == "ITEM_BACKPACK" || inventoryItem.Id == "ITEM_DUFFELBAG")
                {
                    if (backPackCount >= 2) continue;
                }

                if (inventoryItem.Id.Contains("WEAPON") && !inventoryItem.Id.Contains("AMMO"))
                {
                    WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(inventoryItem.ItemValue);

                    if (weaponInfo != null)
                    {
                        if (weaponInfo.AmmoCount > 0)
                        {
                            NativeMenuItem menuItem = new NativeMenuItem(inventoryItem.CustomName, $"{inventoryItem.ItemInfo.Description} - Bullets: {weaponInfo.AmmoCount}");
                            menuItems.Add(menuItem);
                            continue;
                        }
                    }
                }

                menuItems.Add(inventoryItem.Quantity > 1
                    ? new NativeMenuItem(inventoryItem.CustomName, $"Quantity: {inventoryItem.Quantity}")
                    : new NativeMenuItem(inventoryItem.CustomName));
            }

            NativeMenu menu =
                new NativeMenu("property:storage:OnSelectStoreItem", "Inventory", "Select an option", menuItems)
                {
                    PassIndex = true
                };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectStoreItem(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> inventoryItems = playerInventory.GetInventory();

            InventoryItem selectedInventoryItem = inventoryItems[index];

            player.GetData("InsidePropertyInventory", out int currentPropertyId);

            Models.Property insideProperty = Models.Property.FetchProperty(currentPropertyId);

            Inventory.Inventory propertyInventory = Models.Property.FetchInventory(insideProperty);

            if (propertyInventory == null)
            {
                player.SendErrorNotification("An error occurred.");
                player.DeleteData("InsidePropertyInventory");
                return;
            }

            bool itemTransferred = playerInventory.TransferItem(propertyInventory, selectedInventoryItem);

            if (!itemTransferred)
            {
                player.SendErrorNotification("An error occurred adding the item!");
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_BACKPACK" || selectedInventoryItem.Id == "ITEM_DUFFELBAG")
            {
                using Context context = new Context();

                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                playerCharacter.BackpackId = 0;

                context.SaveChanges();

                player.LoadCharacterCustomization();
            }

            player.SendInfoNotification($"You've stored {selectedInventoryItem.CustomName} into the property.");
        }

        private static void ShowPropertyTakeMenu(IPlayer player)
        {
            player.GetData("InsidePropertyInventory", out int currentPropertyId);

            Models.Property insideProperty = Models.Property.FetchProperty(currentPropertyId);

            Inventory.Inventory propertyInventory = Models.Property.FetchInventory(insideProperty);

            List<InventoryItem> inventoryItems = propertyInventory.GetInventory();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            int backPackId = player.FetchCharacter().BackpackId;
            foreach (InventoryItem inventoryItem in inventoryItems)
            {
                if (backPackId > 0)
                {
                    if (inventoryItem.Id == "ITEM_BACKPACK" || inventoryItem.Id == "ITEM_DUFFELBAG") continue;
                }

                if (inventoryItem.Id.Contains("WEAPON") && !inventoryItem.Id.Contains("AMMO"))
                {
                    WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(inventoryItem.ItemValue);

                    if (weaponInfo != null)
                    {
                        if (weaponInfo.AmmoCount > 0)
                        {
                            NativeMenuItem menuItem = new NativeMenuItem(inventoryItem.CustomName, $"{inventoryItem.ItemInfo.Description} - Bullets: {weaponInfo.AmmoCount}");
                            menuItems.Add(menuItem);
                            continue;
                        }
                    }
                }

                menuItems.Add(inventoryItem.Quantity > 1
                    ? new NativeMenuItem(inventoryItem.CustomName, $"Quantity: {inventoryItem.Quantity}")
                    : new NativeMenuItem(inventoryItem.CustomName));
            }

            NativeMenu menu =
                new NativeMenu("property:storage:OnSelectTakeItem", "Inventory", "Select an item", menuItems)
                {
                    PassIndex = true
                };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectTakeItem(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            player.GetData("InsidePropertyInventory", out int currentPropertyId);

            Models.Property insideProperty = Models.Property.FetchProperty(currentPropertyId);

            Inventory.Inventory propertyInventory = Models.Property.FetchInventory(insideProperty);

            if (propertyInventory == null)
            {
                player.SendErrorNotification("An error occurred.");
                player.DeleteData("InsidePropertyInventory");
                return;
            }

            List<InventoryItem> inventoryItems = propertyInventory.GetInventory();

            InventoryItem selectedInventoryItem = inventoryItems[index];

            bool itemTransferred = propertyInventory.TransferItem(playerInventory, selectedInventoryItem,
                selectedInventoryItem.Quantity);

            if (!itemTransferred)
            {
                player.SendErrorNotification("An error occurred adding the item!");
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_BACKPACK" || selectedInventoryItem.Id == "ITEM_DUFFELBAG")
            {
                bool tryParse = int.TryParse(selectedInventoryItem.ItemValue, out int backPackId);

                if (!tryParse)
                {
                    player.SendErrorNotification("An error occurred getting the backpack data.");
                    return;
                }

                using Context context = new Context();

                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                playerCharacter.BackpackId = backPackId;

                context.SaveChanges();

                player.LoadCharacterCustomization();
            }

            player.SendInfoNotification($"You've taken {selectedInventoryItem.CustomName} from the property.");
        }

        [Command("pradio", onlyOne: true, commandType: CommandType.Property,
            description: "Music: Used to set a music stream")]
        public static void PropertyCommandSetRadio(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (player.FetchCharacter() == null)
            {
                player.SendPermissionError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/pradio [URL/off]");
                return;
            }

            Models.Property nearProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearProperty == null)
            {
                nearProperty = Models.Property.FetchProperty(player.Dimension);

                if (nearProperty == null)
                {
                    player.SendErrorNotification("You are not near a property.");
                    return;
                }
            }

            if (nearProperty.OwnerId != player.FetchCharacter().Id)
            {
                player.SendPermissionError();
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearProperty.Id);

            if (property == null)
            {
                player.SendErrorNotification("You are not near a property.");
                return;
            }

            if (args.ToLower().Trim() == "off")
            {
                property.MusicStation = string.Empty;
                player.SendInfoNotification($"You've stopped the music.");
            }
            else
            {
                property.MusicStation = args;
                player.SendInfoNotification($"You've set the music to URL {args}.");
            }

            context.SaveChanges();

            PropertyHandler.ReloadPropertyRadio(property);
        }

        [Command("bahama")]
        public static void PropertyCommandBahama(IPlayer player)
        {
            Position byBarPosition = new Position(-1390.8923f, -597.95605f, 30.30774f);
            Position behindBarPosition = new Position(-1391.4725f, -601.17365f, 30.30774f);

            if (player.Position.Distance(byBarPosition) > 10) return;

            bool hasData = player.GetData("BehindBahama", out bool behind);

            if (!hasData || !behind)
            {
                // Not beind no data
                player.Position = behindBarPosition;
                player.SetData("BehindBahama", true);
                return;
            }

            player.Position = byBarPosition;
            player.SetData("BehindBahama", false);
        }

        [Command("setactive", commandType: CommandType.Property,
            description: "Business: Used to set the business as active")]
        public static void PropertyCommandSetActive(IPlayer player)
        {
            Models.Property nearProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearProperty == null)
            {
                player.SendErrorNotification("You are not near a property.");
                return;
            }

            if (nearProperty.PropertyType == PropertyType.House)
            {
                player.SendErrorNotification("You can't do this!");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            bool hasKey = playerInventory.GetInventoryItems("ITEM_PROPERTY_KEY")
                .FirstOrDefault(x => x.ItemValue == nearProperty.Key) != null;

            if (!hasKey)
            {
                player.SendErrorNotification("You don't have the keys!");
                return;
            }

            if (ActiveBusiness.ActiveBusinessBlips.ContainsKey(nearProperty.Id))
            {
                ActiveBusiness.RemoveActiveBusiness(nearProperty);
                player.SendNotification("~g~You've removed this being from being active!");
                return;
            }

            if (ActiveBusiness.ActiveBusinessBlips.Count > 3)
            {
                player.SendErrorNotification("There are already 3 businesses active.");
                return;
            }

            bool setActive = ActiveBusiness.MakeBusinessActive(nearProperty);

            if (!setActive)
            {
                player.SendErrorNotification("Unable to set this business to active. Is it already active?");
                return;
            }

            player.SendNotification("~g~You've set this business to active!");

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearProperty.Id);

            property.LastSetActive = DateTime.Now;

            context.SaveChanges();
        }

        [Command("penter", commandType: CommandType.Property, description: "Entrances: Used to enter exit different entrances")]
        public static void CommandPEnter(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            foreach (var property in Models.Property.FetchProperties().Where(x => !string.IsNullOrEmpty(x.DoorPositions)).ToList())
            {
                List<PropertyDoor> propertyDoors =
                    JsonConvert.DeserializeObject<List<PropertyDoor>>(property.DoorPositions);

                foreach (var propertyDoor in propertyDoors)
                {
                    if (player.Dimension != propertyDoor.EnterDimension) continue;

                    Vector3 enterPosition = new Vector3(propertyDoor.EnterPosX, propertyDoor.EnterPosY, propertyDoor.EnterPosZ);

                    if (player.Position.Distance(enterPosition) <= 4)
                    {
                        if (property.Locked)
                        {
                            player.SendErrorNotification("Property Locked!");
                            return;
                        }

                        Interiors interior =
                            Interiors.InteriorList.FirstOrDefault(x => x.InteriorName == property.InteriorName);

                        if (!string.IsNullOrEmpty(interior.Ipl))
                        {
                            player.RequestIpl(interior.Ipl);
                        }

                        player.Position = new Vector3(propertyDoor.ExitPosX, propertyDoor.ExitPosY, propertyDoor.ExitPosZ);
                        player.Dimension = property.Id;

                        List<string> propList = JsonConvert.DeserializeObject<List<string>>(property.PropList);

                        if (propList.Any())
                        {
                            foreach (var prop in propList)
                            {
                                player.LoadInteriorProp(prop);
                            }
                        }

                        player.Position = new Vector3(propertyDoor.ExitPosX, propertyDoor.ExitPosY, propertyDoor.ExitPosZ);
                        player.Dimension = (int)propertyDoor.ExitDimension;

                        DoorHandler.UpdateDoorsForPlayer(player);
                    }
                }
            }
        }

        [Command("pexit", commandType: CommandType.Property, description: "Entrances: Used to exit additional property entrances")]
        public static void CommandPExit(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Models.Property insideProperty = Models.Property.FetchProperty((int)player.Dimension);

            if (insideProperty == null)
            {
                foreach (var property in Models.Property.FetchProperties())
                {
                    List<PropertyDoor> externalPropertyDoors =
                        JsonConvert.DeserializeObject<List<PropertyDoor>>(property.DoorPositions);

                    foreach (var propertyDoor in externalPropertyDoors)
                    {
                        if (player.Dimension != propertyDoor.EnterDimension) continue;

                        Vector3 enterPosition = new Vector3(propertyDoor.ExitPosX, propertyDoor.ExitPosY, propertyDoor.ExitPosZ);

                        if (player.Position.Distance(enterPosition) <= 4)
                        {
                            if (property.Locked)
                            {
                                player.SendErrorNotification("Property Locked!");
                                return;
                            }
                            player.Position = new Vector3(propertyDoor.EnterPosX, propertyDoor.EnterPosY, propertyDoor.EnterPosZ);
                            player.Dimension = (int)propertyDoor.ExitDimension;

                            DoorHandler.UpdateDoorsForPlayer(player);
                            return;
                        }
                    }
                }
                player.SendErrorNotification("Your not in a property.");
                return;
            }

            List<PropertyDoor> propertyDoors =
                JsonConvert.DeserializeObject<List<PropertyDoor>>(insideProperty.DoorPositions);

            if (!propertyDoors.Any())
            {
                player.SendErrorNotification("This property doesn't have any doors.");
                return;
            }

            foreach (var propertyDoor in propertyDoors)
            {
                if (player.Dimension != propertyDoor.ExitDimension) continue;

                Vector3 exitPosition = new Vector3(propertyDoor.ExitPosX, propertyDoor.ExitPosY, propertyDoor.ExitPosZ);

                if (player.Position.Distance(exitPosition) <= 3)
                {
                    player.Position = new Vector3(propertyDoor.EnterPosX, propertyDoor.EnterPosY, propertyDoor.EnterPosZ);
                    player.Dimension = (int)propertyDoor.EnterDimension;
                    DoorHandler.UpdateDoorsForPlayer(player);
                }
            }
        }
    }
}