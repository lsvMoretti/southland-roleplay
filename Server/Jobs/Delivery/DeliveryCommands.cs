using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Jobs.Delivery
{
    public class DeliveryCommands
    {
        [Command("buyshipment", onlyOne: true, commandType: CommandType.Job,
            description: "Delivery: Buys from a point. /buyshipment [Amount]")]
        public static void DeliveryCommandBuyShipment(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/buyshipment [Amount]");
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendErrorNotification("You must be on foot.");
                return;
            }

            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            bool tryParse = int.TryParse(args, out int amount);

            if (!tryParse)
            {
                player.SendErrorNotification("Amount must be a number!");
                return;
            }

            if (amount <= 0)
            {
                player.SendErrorNotification("Amount must be greater than 0.");
                return;
            }

            DeliveryPoint nearestPoint = DeliveryHandler.FetchNearestPoint(player.Position);

            if (nearestPoint == null)
            {
                player.SendErrorNotification("You aren't near a delivery point.");
                return;
            }

            if (nearestPoint.PointType != DeliveryPointType.Pickup)
            {
                player.SendErrorNotification("You must be at a pickup point.");
                return;
            }

            float requiredCash = amount * nearestPoint.CostPerItem;

            if (player.GetClass().Cash < requiredCash)
            {
                player.SendErrorNotification($"You don't have enough money. You need at least {requiredCash:C}.");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            bool itemsAdded = playerInventory.AddItem(new InventoryItem("ITEM_DELIVERYPRODUCT", "Delivery Product", "", amount));

            if (!itemsAdded)
            {
                player.SendErrorNotification("There was an error adding these items to your inventory.");
                return;
            }

            player.RemoveCash(requiredCash);

            player.SendInfoNotification($"You've bought {amount} items for {requiredCash:C}. These have been added to your inventory.");
        }

        [Command("sellshipment", onlyOne: true, commandType: CommandType.Job,
            description: "Delivery: Used to sell to Warehouses. /sellshipment [Amount]")]
        public static void DeliveryCommandSellShipment(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/sellshipment [Amount]");
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendErrorNotification("You must be on foot.");
                return;
            }

            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            bool tryParse = int.TryParse(args, out int amount);

            if (!tryParse)
            {
                player.SendErrorNotification("Amount must be a number!");
                return;
            }

            if (amount <= 0)
            {
                player.SendErrorNotification("Amount must be greater than 0.");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> productList = playerInventory.GetInventoryItems("ITEM_DELIVERYPRODUCT");

            if (!productList.Any())
            {
                player.SendErrorNotification("You have no products to sell here.");
                return;
            }

            double productCount = 0;

            foreach (InventoryItem inventoryItem in productList)
            {
                productCount += inventoryItem.Quantity;
            }

            if (productCount < amount)
            {
                player.SendErrorNotification($"You only have {productCount} products on you.");
                return;
            }

            DeliveryPoint nearestPoint = DeliveryHandler.FetchNearestPoint(player.Position);

            if (nearestPoint == null)
            {
                player.SendErrorNotification("You aren't near a delivery point.");
                return;
            }

            if (nearestPoint.PointType != DeliveryPointType.DropOff)
            {
                Console.WriteLine($"Point Type: {nearestPoint.PointType.ToString()}. Point Id: {nearestPoint.Id}");
                player.SendErrorNotification("You must be at a drop-off point.");
                return;
            }

            Warehouse warehouse = WarehouseHandler.FetchWarehouse(nearestPoint.WarehouseId);

            if (warehouse == null)
            {
                player.SendErrorNotification("There was an error fetching the warehouse for this point.");
                return;
            }

            int amountLeft = (int)warehouse.MaxProducts - warehouse.Products;

            if (amount > amountLeft)
            {
                player.SendErrorNotification($"This warehouse can only take {amountLeft:## 'products'}.");
                return;
            }

            bool removeProducts = playerInventory.RemoveItem("ITEM_DELIVERYPRODUCT", amount);

            if (!removeProducts)
            {
                player.SendErrorNotification("There was an error removing the products from your inventory.");
                return;
            }

            using Context context = new Context();

            Warehouse warehouseDb = context.Warehouse.Find(warehouse.Id);

            warehouseDb.Products += amount;

            context.SaveChanges();

            double pay = amount * (warehouse.MinPrice * 1.2);

            player.SendInfoNotification($"You have sold {amount:## 'products'} for {pay:C}");

            player.AddCash(pay);
        }

        [Command("buyproducts", onlyOne: true, commandType: CommandType.Job,
            description: "Business: Used to buy products from a warehouse. /buyproducts [Amount]")]
        public static void CommandBuyProducts(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/buyproducts [Amount]");
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendErrorNotification("You must be on foot.");
                return;
            }

            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            bool tryParse = int.TryParse(args, out int amount);

            if (!tryParse)
            {
                player.SendErrorNotification("Amount must be a number!");
                return;
            }

            if (amount <= 0)
            {
                player.SendErrorNotification("Amount must be greater than 0.");
                return;
            }

            Warehouse nearestWarehouse = WarehouseHandler.FetchNearestPoint(player.Position);

            if (nearestWarehouse == null)
            {
                player.SendErrorNotification("You're not near a warehouse.");
                return;
            }

            if (nearestWarehouse.Products == 0)
            {
                player.SendErrorNotification("This warehouse has no products left.");
                return;
            }

            if (nearestWarehouse.Products < amount)
            {
                player.SendErrorNotification($"This warehouse only has {nearestWarehouse.Products:## 'products'} left.");
                return;
            }

            double costPerItem = Utility.Rescale(nearestWarehouse.Products, 1, nearestWarehouse.MaxProducts,
                nearestWarehouse.MaxPrice, nearestWarehouse.MinPrice);

            double totalCost = costPerItem * amount;

            if (player.GetClass().Cash < totalCost)
            {
                player.SendErrorNotification($"You don't have enough money! You need {totalCost:C}. Cost per item currently is {costPerItem:C}.");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            bool productsAdded = playerInventory.AddItem(new InventoryItem("ITEM_WAREHOUSEPRODUCT", "Warehouse Product", costPerItem.ToString(), amount));

            if (!productsAdded)
            {
                player.SendErrorNotification("You don't have enough space for these items.");
                return;
            }

            player.RemoveCash(totalCost);

            using Context context = new Context();

            Warehouse warehouse = context.Warehouse.Find(nearestWarehouse.Id);

            warehouse.Products -= amount;

            context.SaveChanges();

            if (amount > 1)
            {
                player.SendInfoNotification($"Cost per product was {costPerItem:C}.");
            }

            player.SendInfoNotification($"You have bought {amount:## 'products'} for {totalCost:C} from the {warehouse.Name} warehouse.");
        }

        [Command("sellproducts", onlyOne: true, commandType: CommandType.Job,
            description: "Used to sell products to a business. /sellproducts [Amount]")]
        public static void DeliveryCommandSellProducts(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/sellproducts [Amount]");
                return;
            }

            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            bool amountParse = double.TryParse(args, out double amount);

            if (!amountParse)
            {
                player.SendErrorNotification("Parameter must be number only.");
                return;
            }

            if (amount <= 0)
            {
                player.SendErrorNotification("Amount can't be less than 0.");
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendErrorNotification("You must be on foot.");
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null || nearestProperty.PropertyType == PropertyType.House)
            {
                player.SendErrorNotification("You must be near a business for this.");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> productItems = playerInventory.GetInventoryItems("ITEM_WAREHOUSEPRODUCT");

            double productCount = 0;

            foreach (InventoryItem inventoryItem in productItems)
            {
                productCount += inventoryItem.Quantity;
            }

            if (productCount < amount)
            {
                player.SendErrorNotification($"You only have {productCount:## 'products'} on you.");
                return;
            }

            if (nearestProperty.OwnerId == 0)
            {
                // Property Not Owned
                if (nearestProperty.Products >= 100)
                {
                    player.SendErrorNotification("This product doesn't require any products.");
                    return;
                }

                int requiredAmount = 100 - nearestProperty.Products;

                if (amount > requiredAmount)
                {
                    player.SendErrorNotification($"This property isn't owned. It only requires {requiredAmount:## 'products'}.");
                    return;
                }

                double totalCost = 0.0;

                double count = 0;

                List<InventoryItem> RemoveItems = new List<InventoryItem>();
                List<double> RemoveCount = new List<double>();

                foreach (var inventoryItem in productItems)
                {
                    if (count >= amount) break;

                    // Amount of products required
                    double productsRequired = amount -= count;

                    if (inventoryItem.Quantity >= productsRequired)
                    {
                        // Has more items in this than required.

                        // Increase Cost
                        if (double.TryParse(inventoryItem.ItemValue, out double buyPrice))
                        {
                            totalCost += buyPrice * productsRequired;
                        }
                        else
                        {
                            totalCost += 3 * productsRequired;
                        }

                        count += productsRequired;

                        RemoveItems.Add(inventoryItem);
                        RemoveCount.Add(productsRequired);
                    }
                    else
                    {
                        // Has less items in this than required.
                        // Increase Cost
                        if (double.TryParse(inventoryItem.ItemValue, out double buyPrice))
                        {
                            totalCost += buyPrice * inventoryItem.Quantity;
                        }
                        else
                        {
                            totalCost += 3 * inventoryItem.Quantity;
                        }

                        count += inventoryItem.Quantity;

                        RemoveItems.Add(inventoryItem);
                        RemoveCount.Add(inventoryItem.Quantity);
                    }
                }

                bool itemRemoved = true;

                foreach (InventoryItem inventoryItem in RemoveItems)
                {
                    // Remove Item & Quantity of that item
                    bool removed = playerInventory.RemoveItem(inventoryItem, RemoveCount[RemoveItems.IndexOf(inventoryItem)]);
                    if (!removed)
                    {
                        itemRemoved = false;
                    }
                }

                if (!itemRemoved)
                {
                    player.SendErrorNotification("There was an error removing the products from your inventory.");
                    return;
                }

                using Context context = new Context();

                Models.Property propertyDb = context.Property.Find(nearestProperty.Id);

                propertyDb.Products += (int)amount;

                context.SaveChanges();

                double earnings = totalCost * 1.15;

                player.AddCash(earnings);

                player.SendInfoNotification($"You have sold {(int)amount} items to {nearestProperty.BusinessName} for {earnings:C}.");

                return;
            }

            if (nearestProperty.RequiredProducts < amount)
            {
                player.SendErrorNotification($"This business is only requesting {nearestProperty.RequiredProducts:## 'products'}.");
                return;
            }

            bool ownedItemRemoved = playerInventory.RemoveItem("ITEM_WAREHOUSEPRODUCT", amount);

            if (!ownedItemRemoved)
            {
                player.SendErrorNotification("There was an error removing the products from your inventory.");
                return;
            }

            using Context ownedContext = new Context();

            Models.Property property = ownedContext.Property.Find(nearestProperty.Id);

            property.RequiredProducts -= (int)amount;

            property.Products += (int)amount;

            double totalCash = property.ProductBuyPrice * amount;

            ownedContext.SaveChanges();

            player.SendInfoNotification($"You've sold {(int)amount:## 'products'} to {nearestProperty.BusinessName} for a total of {totalCash:C}");

            player.AddCash(totalCash);
        }

        [Command("products", commandType: CommandType.Job,
            description: "Delivery: Used to check required products.")]
        public static void DeliveryCommandViewProducts(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (player.IsInVehicle)
            {
                player.SendErrorNotification("You must be on foot.");
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null || nearestProperty.PropertyType == PropertyType.House)
            {
                player.SendErrorNotification("You must be near a business.");
                return;
            }

            if (nearestProperty.OwnerId == 0)
            {
                int requiredAmount = 100 - nearestProperty.Products;
                player.SendInfoNotification($"Required Products: {requiredAmount:D1} products.");
                return;
            }

            player.SendInfoNotification($"Required Products: {nearestProperty.RequiredProducts}. Price per product: {nearestProperty.ProductBuyPrice:C}.");
        }

        [Command("shipments", commandType: CommandType.Job,
            description: "Delivery: Used to look at the shipment areas.")]
        public static void DeliveryCommandViewShipments(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>()
            {
                new NativeMenuItem("Pickup Locations", "Locations to buy shipments"),
                new NativeMenuItem("Drop Off Locations", "Locations to sell shipments")
            };

            NativeMenu menu = new NativeMenu("job:delivery:ShowShipmentType", "Shipment Areas", "Select the type of place you want.", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectShipmentType(IPlayer player, string option)
        {
            if (option == "Close") return;

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            DeliveryPointType pointType = option switch
            {
                "Pickup Locations" => DeliveryPointType.Pickup,
                "Drop Off Locations" => DeliveryPointType.DropOff,
                _ => DeliveryPointType.Pickup
            };

            List<DeliveryPoint> pointList = DeliveryHandler.FetchAllDeliveryPoints().Where(x => x.PointType == pointType).ToList();

            if (!pointList.Any())
            {
                player.SendErrorNotification("No points found.");
                return;
            }

            foreach (DeliveryPoint deliveryPoint in pointList.OrderBy(x => x.Name))
            {
                if (deliveryPoint.PointType == DeliveryPointType.Pickup)
                {
                    menuItems.Add(new NativeMenuItem(deliveryPoint.Name, $"Current Price: {deliveryPoint.CostPerItem:C}."));
                }
                else
                {
                    Warehouse warehouse = WarehouseHandler.FetchWarehouse(deliveryPoint.WarehouseId);

                    menuItems.Add(new NativeMenuItem(deliveryPoint.Name, $"Current Price: {warehouse.MinPrice * 1.2:C}."));
                }
            }

            NativeMenu menu = new NativeMenu("job:delivery:OnSelectShipmentPoint", "Shipments", "Select a place you wish to head to.", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectShipmentLocation(IPlayer player, string option)
        {
            if (option == "Close") return;

            DeliveryPoint deliveryPoint =
                DeliveryHandler.FetchAllDeliveryPoints().FirstOrDefault(x => x.Name == option);

            if (deliveryPoint == null)
            {
                player.SendErrorNotification("Unable to find this location.");
                return;
            }

            player.SetWaypoint(deliveryPoint.Position());

            player.SendInfoNotification($"Waypoint set the location!");
        }

        [Command("warehouses", commandType: CommandType.Job, description: "Delivery: Used to view warehouses")]
        public static void DeliveryCommandWarehouses(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            List<Warehouse> warehouses = WarehouseHandler.FetchWarehouses();

            if (!warehouses.Any())
            {
                player.SendErrorNotification("No warehouses found.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Warehouse warehouse in warehouses)
            {
                double costPerItem = Utility.Rescale(warehouse.Products, 1, warehouse.MaxProducts,
                    warehouse.MaxPrice, warehouse.MinPrice);
                menuItems.Add(new NativeMenuItem(warehouse.Name, $"Price: {costPerItem:C}. {warehouse.Products:## 'Products'}"));
            }

            NativeMenu menu = new NativeMenu("job:delivery:OnSelectWarehouse", "Warehouses", "Select a Warehouse you want to goto", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectWarehouse(IPlayer player, string option)
        {
            if (option == "Close") return;

            Warehouse warehouse = WarehouseHandler.FetchWarehouses().FirstOrDefault(x => x.Name == option);

            if (warehouse == null)
            {
                player.SendErrorNotification("Warehouse no longer exists!");
                return;
            }
            double costPerItem = Utility.Rescale(warehouse.Products, 1, warehouse.MaxProducts,
                warehouse.MaxPrice, warehouse.MinPrice);

            player.SendInfoNotification($"Navigating you to {warehouse.Name}. Current Price: {costPerItem:C}, {warehouse.Products:## 'Remaining'}.");
            player.SetWaypoint(warehouse.Position());
        }
    }
}