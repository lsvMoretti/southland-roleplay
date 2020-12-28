using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Jobs.Fishing
{
    public class FishingCommands
    {
        private static List<string> fishingItems = new List<string>
        {
            "Yellowtail",
            "Used Condom", //
            "White Sea Bass",
            "Calico Bass",
            "Tire", //
            "Barracuda",
            "Tuna",
            "Plastic Bottle", //
            "Rockfish",
            "Mackerel",
            "Dead Fish", //
            "Bluefish",
            "Redfish",
            "Pacific Halibut",
        };

        [Command("fish", commandType: CommandType.Job, description: "Fishing: Used to catch fish.")]
        public static void FishingCommandFish(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendPermissionError();
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendNotification("~r~You are in a vehicle");
                return;
            }

            if (player.GetClass().Fishing)
            {
                player.SendNotification("~r~You are currently fishing.");
                return;
            }

            FishingPoint fishPoint = FishingHandler.FetchNearestPosition(player.Position, 15f);

            if (fishPoint == null)
            {
                player.SendNotification("~r~You are not at a fishing point.");
                return;
            }

            if (fishPoint.PointType == FishingPointType.SellPoint)
            {
                player.SendNotification("~r~You can't fish here!");
                return;
            }

            if (fishPoint.FishCount == 0)
            {
                player.SendNotification("~r~No fish can be seen.");
                return;
            }

            player.SendEmoteMessage("casts their fishing rod out.");
            player.Emit("PlayScenario", "WORLD_HUMAN_STAND_FISHING");
            player.GetClass().Fishing = true;

            player.SetData("Fishing:StopFishing", false);

            Random rand = new Random();
            Timer fishingTimer = new Timer();
            int timerInterval = rand.Next(45000, 120000);
            fishingTimer.Interval = timerInterval;
            fishingTimer.Enabled = true;
            fishingTimer.Start();

            fishingTimer.Elapsed += (sender, args) =>
            {
                fishingTimer.Stop();

                player.GetClass().Fishing = false;
                player.Emit("StopScenario");
                bool hasStoppedData = player.GetData("Fishing:StopFishing", out bool stopFishing);
                if (hasStoppedData && stopFishing)
                {
                    player.SetData("Fishing:StopFishing", false);
                    return;
                }

                using Context context = new Context();

                FishingPoint atFishingPoint = context.FishingPoints.Find(fishPoint.Id);

                if (atFishingPoint.FishCount <= 0)
                {
                    player.SendEmoteMessage("reels their line back in and catches nothing.");
                    player.SendNotification("~r~No fish are here!");
                    return;
                }

                Random fishRandom = new Random();
                int randomIndex = fishRandom.Next(0, fishingItems.Count);

                string caughtItem = fishingItems[randomIndex];

                var isFish = caughtItem switch
                {
                    "Used Condom" => false,
                    "Tire" => false,
                    "Plastic Bottle" => false,
                    "Dead Fish" => false,
                    _ => true
                };

                if (!isFish)
                {
                    player.SendEmoteMessage($"reels their line back in and catches a {caughtItem}, throwing it back.");
                    return;
                }

                Inventory.Inventory playerInventory = player.FetchInventory();

                InventoryItem newFishingItem = new InventoryItem("ITEM_RESOURCE_FISH", caughtItem, DateTime.Now.ToString());

                bool itemAdded = playerInventory.AddItem(newFishingItem);

                if (!itemAdded)
                {
                    player.SendEmoteMessage("reels their line back in and catches nothing.");
                    player.SendNotification("~r~Your inventory is full!");
                    return;
                }

                atFishingPoint.FishCount -= 1;
                context.SaveChanges();
                

                player.SendEmoteMessage($"reels their line back in and catches a {caughtItem}.");
            };
        }

        [Command("sellfish", commandType: CommandType.Job, description: "Used to sell your fish.")]
        public static void FishingCommandSellFish(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendPermissionError();
                return;
            }

            if (player.GetClass().Fishing)
            {
                player.SendNotification("~r~You are currently fishing.");
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendNotification("~r~You are in a vehicle");
                return;
            }

            FishingPoint fishPoint = FishingHandler.FetchNearestPosition(player.Position, 15f);

            if (fishPoint == null)
            {
                player.SendNotification("~r~You are not at a fishing point.");
                return;
            }

            if (fishPoint.PointType == FishingPointType.FishSpot)
            {
                player.SendNotification("~r~You are not able to sell here.");
                return;
            }

            if (fishPoint.FishCount >= fishPoint.MaxFish)
            {
                player.SendNotification("~r~This point is full!");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> fishItems = playerInventory.GetInventoryItems("ITEM_RESOURCE_FISH");

            if (!fishItems.Any())
            {
                player.SendNotification("~r~You have no fish on you!");
                return;
            }

            int fishSpaceLeft = fishPoint.MaxFish - fishPoint.FishCount;

            if (fishSpaceLeft < fishItems.Count)
            {
                var removeFishList = fishItems.Take(fishSpaceLeft).ToList();

                List<DateTime> dates = new List<DateTime>();

                foreach (InventoryItem removeFish in removeFishList)
                {
                    dates.Add(DateTime.Parse(removeFish.ItemValue));

                    playerInventory.RemoveItem(removeFish);
                }

                var count = dates.Count;
                double temp = 0D;
                for (int i = 0; i < count; i++)
                {
                    temp += dates[i].Ticks / (double)count;
                }
                var averageDate = new DateTime((long)temp);

                TimeSpan diffTimeSpan = DateTime.Now - averageDate;

                // double costPerItem = Utility.Rescale(nearestWarehouse.Products, 1, nearestWarehouse.MaxProducts, nearestWarehouse.MaxPrice, nearestWarehouse.MinPrice);

                double costPerFish = Utility.Rescale(fishSpaceLeft, 1, fishPoint.MaxFish, fishPoint.MaxPrice,
                    fishPoint.MinPrice);

                if (diffTimeSpan.Hours > 3)
                {
                    double oldCost = costPerFish;
                    costPerFish /= 1.5;
                    double totalAmount = costPerFish * fishSpaceLeft;
                    player.SendInfoNotification($"Your average fish age is older than 3 hours.");
                    player.SendInfoNotification($"Your cost per fish has gone down to {costPerFish:C} from {oldCost:C}. Total Earnings: {totalAmount:C}. You have sold {fishSpaceLeft}/{fishItems.Count} fish.");
                    player.AddCash(totalAmount);
                }
                else
                {
                    double totalAmount = costPerFish * fishSpaceLeft;
                    player.SendInfoNotification($"Your cost per fish is {costPerFish:C}. Total Earnings: {totalAmount:C}. You have sold {fishSpaceLeft}/{fishItems.Count} fish.");
                    player.AddCash(totalAmount);
                }

                using Context context = new Context();

                var fishPointDb = context.FishingPoints.Find(fishPoint.Id);
                fishPointDb.FishCount += fishSpaceLeft;
                context.SaveChanges();
                
                return;
            }

            List<DateTime> allDates = new List<DateTime>();

            foreach (InventoryItem fishItem in fishItems)
            {
                allDates.Add(DateTime.Parse(fishItem.ItemValue));
                playerInventory.RemoveItem(fishItem);
            }

            var dateCount = allDates.Count;
            double dateTemp = 0D;
            for (int i = 0; i < dateCount; i++)
            {
                dateTemp += allDates[i].Ticks / (double)dateCount;
            }
            var fishAverageDate = new DateTime((long)dateTemp);

            TimeSpan fishAverageTimeSpan = DateTime.Now - fishAverageDate;

            double fishCost = Utility.Rescale(fishItems.Count, 1, fishPoint.MaxFish, fishPoint.MaxPrice,
                fishPoint.MinPrice);

            if (fishAverageTimeSpan.Hours > 3)
            {
                double oldCost = fishCost;
                fishCost /= 1.5;
                double totalAmount = fishCost * fishItems.Count;
                player.SendInfoNotification($"Your average fish age is older than 3 hours.");
                player.SendInfoNotification($"Your cost per fish has gone down to {fishCost:C} from {oldCost:C}. Total Earnings: {totalAmount:C}. You have sold {fishItems.Count} fish.");
                player.AddCash(totalAmount);
            }
            else
            {
                double totalAmount = fishCost * fishItems.Count;
                player.SendInfoNotification($"Your cost per fish is {fishCost:C}. Total Earnings: {totalAmount:C}. You have sold {fishItems.Count} fish.");
                player.AddCash(totalAmount);
            }

            using Context fishContext = new Context();

            var pointDb = fishContext.FishingPoints.Find(fishPoint.Id);
            pointDb.FishCount += fishItems.Count;
            fishContext.SaveChanges();
            
            return;
        }

        [Command("stopfishing", commandType: CommandType.Job, description: "Fishing: Used to stop fishing")]
        public static void FishingCommandStop(IPlayer player)
        {
            if (!player.GetClass().Fishing)
            {
                player.SendErrorNotification("You are not fishing.");
                return;
            }

            player.GetClass().Fishing = false;
            player.Emit("StopScenario");
            player.SetData("Fishing:StopFishing", true);
        }
    }
}