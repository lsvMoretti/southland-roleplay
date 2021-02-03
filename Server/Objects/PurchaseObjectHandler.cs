using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using EntityStreamer;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;
using Server.Property;

namespace Server.Objects
{
    public class PurchaseObjectHandler
    {
        public static List<PurchaseObject> BuyableObjects = new List<PurchaseObject>();

        public static void LoadBuyableObjects()
        {
            Console.WriteLine($"Loading Buyable Objects");

            string filePath = "data/buyableobjects.json";

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Unable to find {filePath}");

                BuyableObjects = new List<PurchaseObject>
                {
                    new PurchaseObject("Drink Tray 1", "apa_mp_h_acc_drink_tray_02", 5.00)
                };

                File.WriteAllText(filePath, JsonConvert.SerializeObject(BuyableObjects, Formatting.Indented));

                Console.WriteLine($"New file created at {filePath}.");

                return;
            }

            BuyableObjects = new List<PurchaseObject>();

            string fileContents = File.ReadAllText(filePath);

            BuyableObjects = JsonConvert.DeserializeObject<List<PurchaseObject>>(fileContents);

            Console.WriteLine($"Loaded {BuyableObjects.Count} Buyable Objects");
        }

        /*
        [Command("buyobject", commandType: CommandType.Property,
            description: "Interior Mapping: Used to buy an object")]*/

        public static void BuyObjectCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (PurchaseObject purchaseObject in BuyableObjects)
            {
                menuItems.Add(new NativeMenuItem(purchaseObject.Name, $"Cost: ~g~{purchaseObject.Cost:C}"));
            }

            NativeMenu menu = new NativeMenu("BuyObject:SelectItem", "Objects", "Select an Object to preview", menuItems)
            {
                PassIndex = true,
                ItemChangeTrigger = "BuyObject:ChangeItem"
            };

            player.SetData("ObjectPreview:OldDimension", player.Dimension);
            player.SetData("ObjectPreview:OldPosition", player.Position);
            player.SetData("ObjectPreview:ObjectIndex", 0);

            player.Dimension = player.GetPlayerId();
            player.Emit("StartObjectPreview", JsonConvert.SerializeObject(BuyableObjects));
            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnBuyObjectSelectItem(IPlayer player, string option, int index)
        {
            player.GetData("ObjectPreview:OldDimension", out int oldDimension);
            player.GetData("ObjectPreview:OldPosition", out Position oldPosition);

            player.Dimension = oldDimension;
            player.Position = oldPosition;

            if (option == "Close")
            {
                player.Emit("ObjectPreview:Close");
                return;
            }

            PurchaseObject selectedObject = BuyableObjects[index];

            if (selectedObject == null)
            {
                NotificationExtension.SendErrorNotification(player, "Unable to find this object.");
                return;
            }

            NotificationExtension.SendInfoNotification(player, $"You've selected {selectedObject.Name}.");

            player.Emit("ObjectPreview:Close");

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                NotificationExtension.SendErrorNotification(player, "Unable to fetch your inventory.");
                return;
            }

            if (player.GetClass().Cash < selectedObject.Cost)
            {
                NotificationExtension.SendErrorNotification(player, $"You don't have enough. {selectedObject.Cost:C}.");
                return;
            }

            InventoryItem newItem = new InventoryItem("ITEM_PLACEABLEOBJECT", selectedObject.Name, selectedObject.ObjectName);

            bool itemAdded = playerInventory.AddItem(newItem);

            if (!itemAdded)
            {
                NotificationExtension.SendErrorNotification(player, "Not enough space!");
                return;
            }

            player.RemoveCash(selectedObject.Cost);

            NotificationExtension.SendInfoNotification(player, $"You've bought {selectedObject.Name} for {selectedObject.Cost:C}.");
        }

        public static void OnBuyObjectChangeItem(IPlayer player, int newIndex, string itemText)
        {
            if (itemText == "Close") return;

            player.Emit("ObjectPreview:ChangeIndex", newIndex);
            player.SetData("ObjectPreview:ObjectIndex", newIndex);
        }

        [Command("placeobject", commandType: CommandType.Property,
            description: "Interior Mapping: Used to place an object")]
        public static void ObjectCommandPlaceObject(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 30f);

            if (nearbyProperty == null)
            {
                nearbyProperty = Models.Property.FetchProperty(player.FetchCharacter().InsideProperty);

                if (nearbyProperty == null)
                {
                    NotificationExtension.SendErrorNotification(player, "Your not near a property.");
                    return;
                }
            }

            if (nearbyProperty.OwnerId != player.GetClass().CharacterId)
            {
                NotificationExtension.SendErrorNotification(player, "You must be the owner of the property.");
                return;
            }

            Models.Account? playerAccount = player.FetchAccount();

            if (!string.IsNullOrEmpty(nearbyProperty.PropertyObjects))
            {
                List<PropertyObject> propertyObjects =
                    JsonConvert.DeserializeObject<List<PropertyObject>>(nearbyProperty.PropertyObjects);

                int count = propertyObjects.Count;

                if (playerAccount.DonatorLevel == DonationLevel.None && count >= 50)
                {
                    player.SendErrorNotification("You've reached your limit.");
                    return;
                }

                if (playerAccount.DonatorLevel == DonationLevel.Bronze && count >= 100)
                {
                    player.SendErrorNotification("You've reached your limit.");
                    return;
                }

                if (playerAccount.DonatorLevel == DonationLevel.Silver && count >= 250)
                {
                    player.SendErrorNotification("You've reached your limit.");
                    return;
                }

                if (playerAccount.DonatorLevel == DonationLevel.Gold && count >= 700)
                {
                    player.SendErrorNotification("You've reached your limit.");
                    return;
                }
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                NotificationExtension.SendErrorNotification(player, "Unable to find your inventory.");
                return;
            }

            List<InventoryItem> placeableObjects = playerInventory.GetInventoryItems("ITEM_PLACEABLEOBJECT");

            if (!placeableObjects.Any())
            {
                NotificationExtension.SendErrorNotification(player, "You don't have any objects you can place.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem placeableObject in placeableObjects)
            {
                menuItems.Add(new NativeMenuItem(placeableObject.CustomName));
            }

            NativeMenu menu = new NativeMenu("InteriorMapping:SelectObject", "Objects", "Select an object to place", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);

            player.SetData("InteriorMapping:EditProperty", nearbyProperty.Id);
        }

        public static void OnInteriorMappingSelectObject(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                NotificationExtension.SendErrorNotification(player, "Unable to find your inventory.");
                return;
            }

            List<InventoryItem> placeableObjects = playerInventory.GetInventoryItems("ITEM_PLACEABLEOBJECT");

            if (!placeableObjects.Any())
            {
                NotificationExtension.SendErrorNotification(player, "You don't have any objects you can place.");
                return;
            }

            InventoryItem selectedItem = placeableObjects[index];

            if (selectedItem == null)
            {
                NotificationExtension.SendErrorNotification(player, "Unable to fetch this object.");
                return;
            }

            player.SetData("InteriorMapping:SelectedItemIndex", index);

            player.Emit("InteriorMapping:StartPlacement", selectedItem.ItemValue);

            player.SendInfoMessage($"Arrows to move about, left control & right control to move. Tab to change between rotation and position.");
            player.SendInfoMessage($"F1 to cancel, F2 to save.");
        }

        public static void OnInteriorMappingSaveObject(IPlayer player, Vector3 position, Vector3 rotation)
        {
            player.GetData("InteriorMapping:EditProperty", out int propertyId);
            player.GetData("InteriorMapping:SelectedItemIndex", out int itemIndex);

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                NotificationExtension.SendErrorNotification(player, "Unable to find your inventory.");
                return;
            }

            List<InventoryItem> placeableObjects = playerInventory.GetInventoryItems("ITEM_PLACEABLEOBJECT");

            if (!placeableObjects.Any())
            {
                NotificationExtension.SendErrorNotification(player, "You don't have any objects you can place.");
                return;
            }

            InventoryItem selectedItem = placeableObjects[itemIndex];

            if (selectedItem == null)
            {
                NotificationExtension.SendErrorNotification(player, "Unable to fetch this object.");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(propertyId);

            if (property == null)
            {
                player.SendErrorNotification("An error occurred loading your property.");
                return;
            }

            List<PropertyObject> propertyObjects = null;

            if (string.IsNullOrEmpty(property.PropertyObjects))
            {
                propertyObjects = new List<PropertyObject>();
            }
            else
            {
                propertyObjects =
                    JsonConvert.DeserializeObject<List<PropertyObject>>(property.PropertyObjects);
            }

            PropertyObject newObject = new PropertyObject
            {
                Name = selectedItem.CustomName,
                ObjectName = selectedItem.ItemValue,
                Dimension = player.Dimension,
                Position = position,
                Rotation = rotation
            };

            bool removeItem = playerInventory.RemoveItem(selectedItem);

            if (!removeItem)
            {
                player.SendErrorNotification("Unable to remove this item from your inventory.");
                return;
            }

            propertyObjects.Add(newObject);

            string json = JsonConvert.SerializeObject(propertyObjects, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            property.PropertyObjects = json;
            context.SaveChanges();

            LoadProperties.LoadPropertyObject(property, newObject);

            player.SendInfoNotification($"You've placed down a {selectedItem.CustomName}.");
        }

        [Command("moveobject", commandType: CommandType.Property,
            description: "Interior Mapping: Used to move an object")]
        public static void OnMoveObjectCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 30f);

            if (nearbyProperty == null)
            {
                nearbyProperty = Models.Property.FetchProperty(player.FetchCharacter().InsideProperty);

                if (nearbyProperty == null)
                {
                    NotificationExtension.SendErrorNotification(player, "Your not near a property.");
                    return;
                }
            }

            if (nearbyProperty.OwnerId != player.GetClass().CharacterId)
            {
                NotificationExtension.SendErrorNotification(player, "You must be the owner of the property.");
                return;
            }

            if (string.IsNullOrEmpty(nearbyProperty.PropertyObjects))
            {
                player.SendErrorNotification("There are no objects here.");
                return;
            }

            List<PropertyObject> propertyObjects =
                JsonConvert.DeserializeObject<List<PropertyObject>>(nearbyProperty.PropertyObjects);

            if (!propertyObjects.Any())
            {
                player.SendErrorNotification("There are no objects.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (PropertyObject propertyObject in propertyObjects)
            {
                if (propertyObject.Dimension != player.Dimension) continue;
                menuItems.Add(new NativeMenuItem(propertyObject.Name));
            }

            NativeMenu menu = new NativeMenu("InteriorMapping:ShowMoveObjectList", "Objects", "Select an object to move", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnShowObjectMoveListSelected(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 30f);

            if (nearbyProperty == null)
            {
                nearbyProperty = Models.Property.FetchProperty(player.FetchCharacter().InsideProperty);

                if (nearbyProperty == null)
                {
                    NotificationExtension.SendErrorNotification(player, "Your not near a property.");
                    return;
                }
            }

            List<PropertyObject> propertyObjects =
                JsonConvert.DeserializeObject<List<PropertyObject>>(nearbyProperty.PropertyObjects);

            if (!propertyObjects.Any())
            {
                player.SendErrorNotification("There are no objects.");
                return;
            }

            PropertyObject selectedObject = propertyObjects[index];

            if (selectedObject == null)
            {
                player.SendErrorNotification("You've selected an invalid object.");
                return;
            }

            List<LoadPropertyObject> loadPropertyObjects = LoadProperties.LoadedPropertyObjects
                .Where(x => x.PropertyId == nearbyProperty.Id).ToList();

            LoadPropertyObject selectedPropertyObject =
                loadPropertyObjects.FirstOrDefault(x => x.DynamicObject.Position == (Vector3)selectedObject.Position && x.DynamicObject.Rotation == (Vector3)selectedObject.Rotation);

            if (selectedPropertyObject == null)
            {
                player.SendErrorNotification("Unable to fetch your selected item.");
                return;
            }

            player.Emit("InteriorMapping:MoveCurrentObject", selectedObject.ObjectName, selectedPropertyObject.DynamicObject.Position);

            player.SendInfoMessage($"Arrows to move about, left control & right control to move. Tab to change between rotation and position.");
            player.SendInfoMessage($"F1 to cancel, F2 to save.");

            player.SetData("InteriorMapping:CurrentlyMoving", propertyObjects.IndexOf(selectedObject));
        }

        public static void OnFinishMovingObject(IPlayer player, Vector3 newPosition, Vector3 newRotation)
        {
            player.GetData("InteriorMapping:CurrentlyMoving", out int currentIndex);

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 30f);

            if (nearbyProperty == null)
            {
                nearbyProperty = Models.Property.FetchProperty(player.FetchCharacter().InsideProperty);

                if (nearbyProperty == null)
                {
                    NotificationExtension.SendErrorNotification(player, "Your not near a property.");
                    return;
                }
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearbyProperty.Id);

            if (property == null)
            {
                player.SendErrorNotification("Unable to fetch your objects.");
                LoadProperties.LoadPropertyObjects(nearbyProperty);
                return;
            }

            List<PropertyObject> propertyObjects =
                JsonConvert.DeserializeObject<List<PropertyObject>>(property.PropertyObjects);

            if (!propertyObjects.Any())
            {
                player.SendErrorNotification("There are no objects.");
                return;
            }

            PropertyObject selectedObject = propertyObjects[currentIndex];

            if (selectedObject == null)
            {
                player.SendErrorNotification("You've selected an invalid object.");
                return;
            }

            List<LoadPropertyObject> loadPropertyObjects = LoadProperties.LoadedPropertyObjects
                .Where(x => x.PropertyId == nearbyProperty.Id).ToList();

            LoadPropertyObject selectedPropertyObject =
                loadPropertyObjects.FirstOrDefault(x => x.DynamicObject.Position == (Vector3)selectedObject.Position && x.DynamicObject.Rotation == (Vector3)selectedObject.Rotation);

            if (selectedPropertyObject == null)
            {
                player.SendErrorNotification("Unable to fetch your selected item.");
                return;
            }

            PropStreamer.Delete(selectedPropertyObject.DynamicObject);

            LoadProperties.LoadedPropertyObjects.Remove(selectedPropertyObject);

            propertyObjects.Remove(selectedObject);

            PropertyObject newObject = new PropertyObject
            {
                Name = selectedObject.Name,
                ObjectName = selectedObject.ObjectName,
                Position = newPosition,
                Rotation = newRotation,
                Dimension = selectedObject.Dimension
            };

            propertyObjects.Add(newObject);

            property.PropertyObjects = JsonConvert.SerializeObject(propertyObjects);

            context.SaveChanges();

            LoadProperties.LoadPropertyObject(property, newObject);

            player.SendInfoNotification($"You've moved {newObject.Name}.");
        }

        [Command("pickupobject", commandType: CommandType.Property,
            description: "Interior Mapping: Used to pickup an object")]
        public static void PickUpObjectCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 30f);

            if (nearbyProperty == null)
            {
                nearbyProperty = Models.Property.FetchProperty(player.FetchCharacter().InsideProperty);

                if (nearbyProperty == null)
                {
                    NotificationExtension.SendErrorNotification(player, "Your not near a property.");
                    return;
                }
            }

            if (nearbyProperty.OwnerId != player.GetClass().CharacterId)
            {
                NotificationExtension.SendErrorNotification(player, "You must be the owner of the property.");
                return;
            }

            if (string.IsNullOrEmpty(nearbyProperty.PropertyObjects))
            {
                player.SendErrorNotification("There are no objects here.");
                return;
            }

            List<PropertyObject> propertyObjects =
                JsonConvert.DeserializeObject<List<PropertyObject>>(nearbyProperty.PropertyObjects);

            if (!propertyObjects.Any())
            {
                player.SendErrorNotification("There are no objects.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (PropertyObject propertyObject in propertyObjects)
            {
                if (propertyObject.Dimension != player.Dimension) continue;
                menuItems.Add(new NativeMenuItem(propertyObject.Name));
            }

            NativeMenu menu = new NativeMenu("InteriorMapping:ShowPickupObjectList", "Objects", "Select an object to pickup", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnItemPickupSelect(IPlayer player, string option, int index)
        {
            if (!player.IsSpawned()) return;

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 30f);

            if (nearbyProperty == null)
            {
                nearbyProperty = Models.Property.FetchProperty(player.FetchCharacter().InsideProperty);

                if (nearbyProperty == null)
                {
                    NotificationExtension.SendErrorNotification(player, "Your not near a property.");
                    return;
                }
            }

            if (nearbyProperty.OwnerId != player.GetClass().CharacterId)
            {
                NotificationExtension.SendErrorNotification(player, "You must be the owner of the property.");
                return;
            }

            if (string.IsNullOrEmpty(nearbyProperty.PropertyObjects))
            {
                player.SendErrorNotification("There are no objects here.");
                return;
            }

            List<PropertyObject> propertyObjects =
                JsonConvert.DeserializeObject<List<PropertyObject>>(nearbyProperty.PropertyObjects);

            if (!propertyObjects.Any())
            {
                player.SendErrorNotification("There are no objects.");
                return;
            }

            PropertyObject selectedObject = propertyObjects[index];

            if (selectedObject == null)
            {
                player.SendErrorNotification("Unable to find the selected object");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            bool itemAdded = playerInventory.AddItem(new InventoryItem("ITEM_PLACEABLEOBJECT", selectedObject.Name, selectedObject.ObjectName));

            if (!itemAdded)
            {
                player.SendErrorNotification("Unable to add the item to your inventory.");
                return;
            }

            propertyObjects.Remove(selectedObject);

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearbyProperty.Id);

            property.PropertyObjects = JsonConvert.SerializeObject(propertyObjects);

            context.SaveChanges();

            LoadProperties.LoadPropertyObjects(property);

            player.SendInfoNotification($"You've picked up {selectedObject.Name} and been placed into your inventory.");

            Logging.AddToCharacterLog(player, $"has picked up {selectedObject.Name} from {property.Address}.");
        }
    }
}