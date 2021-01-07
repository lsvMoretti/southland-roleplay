using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net.Native;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.DMV;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Vehicle
{
    public class Commands
    {
        [Command("takeplate", commandType: CommandType.Vehicle, description: "Used to take a plate from a vehicle")]
        public static void VehicleCommandTakePlate(IPlayer player)
        {
            if (!player.GetClass().Spawned)
            {
                player.SendLoginError();
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendErrorMessage("You must not be in a vehicle!");
                return;
            }

            IVehicle? nearestVehicle = VehicleHandler.FetchNearestVehicle(player);

            if (nearestVehicle is null || nearestVehicle.Dimension != player.Dimension)
            {
                player.SendErrorMessage("You must be near a vehicle.");
                return;
            }

            Models.Vehicle? vehicleData = nearestVehicle.FetchVehicleData();

            if (vehicleData is null)
            {
                player.SendErrorMessage("You can't do that on this vehicle!");
                return;
            }

            if (vehicleData.FactionId > 0)
            {
                player.SendErrorMessage("You can't do that on this vehicle!");
                return;
            }

            if (vehicleData.HasPlateBeenStolen)
            {
                player.SendErrorMessage("There is no plate to steal!");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (!playerInventory.HasItem("ITEM_SCREWDRIVER"))
            {
                player.SendErrorNotification("You don't have the required tools.");
                return;
            }

            if (!string.IsNullOrEmpty(vehicleData.StolenPlate))
            {
                // Vehicle has a stolen plate fitted

                Models.Vehicle? stolenPlateVehicle = Models.Vehicle.FetchVehicle(vehicleData.StolenPlate);

                if (stolenPlateVehicle is null)
                {
                    player.SendErrorMessage("An error occurred getting the vehicle information.");
                    return;
                }

                StolenPlate stolenPlate = new StolenPlate(stolenPlateVehicle);

                InventoryItem stolenPlateItem = new InventoryItem("ITEM_VEHICLE_PLATE",
                    $"Plate: {stolenPlate.Plate}", JsonConvert.SerializeObject(stolenPlate));

                using Context context = new Context();

                Models.Vehicle stolenPlateVehicleData = context.Vehicle.First(x => x.Id == stolenPlateVehicle.Id);
                Models.Vehicle nearVehicleData = context.Vehicle.First(x => x.Id == vehicleData.Id);

                if (stolenPlateVehicleData is null)
                {
                    player.SendErrorMessage("An error occurred getting the vehicle information.");
                    return;
                }

                if (!playerInventory.AddItem(stolenPlateItem))
                {
                    player.SendErrorNotification("There was an error adding the plate to your inventory.");
                    return;
                }

                nearVehicleData.HasPlateBeenStolen = true;

                if (!string.IsNullOrEmpty(nearVehicleData.StolenPlate))
                {
                    nearVehicleData.StolenPlate = null;
                }

                context.SaveChanges();

                nearestVehicle.NumberplateText = "__";

                player.SendEmoteMessage("reaches down and takes a plate from the vehicle.");
            }
            else
            {
                // Vehicle doesn't have stolen plate

                StolenPlate stolenPlate = new StolenPlate(vehicleData);

                InventoryItem stolenPlateItem = new InventoryItem("ITEM_VEHICLE_PLATE",
                    $"Plate: {stolenPlate.Plate}", JsonConvert.SerializeObject(stolenPlate));

                using Context context = new Context();

                Models.Vehicle nearVehicleData = context.Vehicle.First(x => x.Id == vehicleData.Id);

                if (nearVehicleData is null)
                {
                    player.SendErrorMessage("An error occurred getting the vehicle information.");
                    return;
                }

                if (!playerInventory.AddItem(stolenPlateItem))
                {
                    player.SendErrorNotification("There was an error adding the plate to your inventory.");
                    return;
                }

                nearVehicleData.HasPlateBeenStolen = true;

                if (!string.IsNullOrEmpty(nearVehicleData.StolenPlate))
                {
                    nearVehicleData.StolenPlate = null;
                }

                context.SaveChanges();

                nearestVehicle.NumberplateText = "__";

                player.SendEmoteMessage("reaches down and takes a plate from the vehicle.");
            }
        }

        [Command("placeplate", commandType: CommandType.Vehicle, description: "Used to place a plate onto a vehicle")]
        public static void VehicleCommandPlacePlate(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            IVehicle? nearestVehicle = VehicleHandler.FetchNearestVehicle(player);

            if (nearestVehicle == null || nearestVehicle.Dimension != player.Dimension)
            {
                player.SendErrorNotification("You must be near a vehicle!");
                return;
            }

            Models.Vehicle? vehicleData = nearestVehicle.FetchVehicleData();

            if (vehicleData is null || vehicleData.FactionId > 0)
            {
                player.SendErrorNotification("You can't do that on this vehicle!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (!playerInventory.HasItem("ITEM_SCREWDRIVER"))
            {
                player.SendErrorNotification("You don't have the required tools.");
                return;
            }

            List<InventoryItem> plateItems = playerInventory.GetInventoryItems("ITEM_VEHICLE_PLATE");

            if (!plateItems.Any())
            {
                player.SendErrorNotification("You don't have any plates on you!");
                return;
            }

            foreach (InventoryItem plateItem in plateItems)
            {
                StolenPlate stolenPlate = JsonConvert.DeserializeObject<StolenPlate>(plateItem.ItemValue);

                menuItems.Add(new NativeMenuItem(stolenPlate.Plate, stolenPlate.Model));
            }

            NativeMenu menu = new NativeMenu("VehicleCommand:PlaceStolePlate", "Plates", "Select a plate", menuItems);

            player.SetData("PLACINGSTOLENPLATE", vehicleData.Id);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnPlaceStolenPlateSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> plateItems = playerInventory.GetInventoryItems("ITEM_VEHICLE_PLATE");

            bool hasIdData = player.GetData("PLACINGSTOLENPLATE", out int vehicleId);

            if (!hasIdData)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            StolenPlate? stolenPlate = null;
            InventoryItem? stolenItem = null;

            foreach (InventoryItem inventoryItem in plateItems)
            {
                StolenPlate plateInfo = JsonConvert.DeserializeObject<StolenPlate>(inventoryItem.ItemValue);

                if (plateInfo.Plate != option) continue;
                stolenItem = inventoryItem;
                stolenPlate = plateInfo;
                break;
            }

            if (stolenPlate is null || stolenItem is null)
            {
                player.SendErrorNotification("Unable to find this plate in your inventory!");
                return;
            }

            IVehicle? nearVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == vehicleId);

            if (nearVehicle is null || nearVehicle.Position.Distance(player.Position) > 5)
            {
                player.SendErrorNotification("You must be near the vehicle!");
                return;
            }

            using Context context = new Context();

            Models.Vehicle? nearVehicleDb = context.Vehicle.FirstOrDefault(x => x.Id == vehicleId);

            if (nearVehicleDb is null)
            {
                player.SendErrorNotification("You must be near the vehicle!");
                return;
            }

            if (!playerInventory.RemoveItem(stolenItem))
            {
                player.SendErrorNotification("There was an error removing this plate from your inventory.");
                return;
            }

            if (!string.IsNullOrEmpty(nearVehicleDb.StolenPlate))
            {
                // A stolen plate has been placed on the vehicle

                // Vehicle has a stolen plate fitted

                Models.Vehicle? stolenPlateVehicle = Models.Vehicle.FetchVehicle(nearVehicleDb.StolenPlate);

                if (stolenPlateVehicle is null)
                {
                    player.SendErrorMessage("An error occurred getting the vehicle information.");
                    return;
                }

                StolenPlate oldPlate = new StolenPlate(stolenPlateVehicle);

                InventoryItem stolenPlateItem = new InventoryItem("ITEM_VEHICLE_PLATE",
                    $"Plate: {oldPlate.Plate}", JsonConvert.SerializeObject(oldPlate));

                Models.Vehicle stolenPlateVehicleData = context.Vehicle.First(x => x.Id == stolenPlateVehicle.Id);
                Models.Vehicle nearVehicleData = context.Vehicle.First(x => x.Id == nearVehicleDb.Id);

                if (stolenPlateVehicleData is null)
                {
                    player.SendErrorMessage("An error occurred getting the vehicle information.");
                    return;
                }

                if (!playerInventory.AddItem(stolenPlateItem))
                {
                    player.SendErrorNotification("There was an error adding the plate to your inventory.");
                    return;
                }

                nearVehicleData.HasPlateBeenStolen = true;

                nearVehicleData.StolenPlate = stolenPlate.Plate;

                context.SaveChanges();

                nearVehicle.NumberplateText = stolenPlate.Plate;

                player.SendEmoteMessage("reaches down and changes a plate on the vehicle.");
            }
            else
            {
                StolenPlate oldPlate = new StolenPlate(nearVehicleDb);

                InventoryItem stolenPlateItem = new InventoryItem("ITEM_VEHICLE_PLATE",
                    $"Plate: {oldPlate.Plate}", JsonConvert.SerializeObject(oldPlate));

                Models.Vehicle nearVehicleData = context.Vehicle.First(x => x.Id == nearVehicleDb.Id);

                if (nearVehicleData is null)
                {
                    player.SendErrorMessage("An error occurred getting the vehicle information.");
                    return;
                }

                if (!nearVehicleDb.HasPlateBeenStolen)
                {
                    if (!playerInventory.AddItem(stolenPlateItem))
                    {
                        player.SendErrorNotification("There was an error adding the plate to your inventory.");
                        return;
                    }
                }

                nearVehicleData.HasPlateBeenStolen = true;

                nearVehicleData.StolenPlate = stolenPlate.Plate;

                context.SaveChanges();

                nearVehicle.NumberplateText = stolenPlate.Plate;

                if (stolenPlate.Plate == nearVehicleData.Plate)
                {
                    nearVehicleData.StolenPlate = null;
                    nearVehicleData.HasPlateBeenStolen = false;
                    nearVehicle.NumberplateText = nearVehicleData.Plate;
                    context.SaveChanges();

                    playerInventory.RemoveItem(stolenPlateItem);
                }

                player.SendEmoteMessage("reaches down and changes a plate on the vehicle.");
            }
        }

        [Command("vattributes", onlyOne: true, commandType: CommandType.Vehicle,
            description: "Used to set a vehicle description")]
        public static void VehicleCommandAttributes(IPlayer player, string description = "")
        {
            if (player?.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            if (description == "" || description.Length < 2)
            {
                player.SendSyntaxMessage("/vattributes [Description/Clear]");
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            if (player.Seat != 1)
            {
                player.SendErrorNotification("You must be the driver.");
                return;
            }

            Models.Vehicle playerVehicle = player.Vehicle.FetchVehicleData();

            if (playerVehicle == null)
            {
                player.SendErrorNotification("You must be in an ownable vehicle!");
                return;
            }

            if (playerVehicle.OwnerId != player.GetClass().CharacterId)
            {
                player.SendErrorNotification("You must be the vehicle owner!");
                return;
            }

            using Context context = new Context();

            Models.Vehicle vehicle = context.Vehicle.Find(playerVehicle.Id);

            if (description.ToLower() == "clear")
            {
                vehicle.Description = null;
                player.SendInfoNotification($"You've cleared the attribute for the vehicle.");
                Logging.AddToCharacterLog(player, $"has removed the description from vehicle id {playerVehicle.Id}");
            }
            else
            {
                player.SendInfoNotification($"You've set the attribute to {description}");
                vehicle.Description = description;
                Logging.AddToCharacterLog(player,
                    $"has set vehicle id {playerVehicle.Id} description to {description}");
            }

            context.SaveChanges();
        }

        [Command("vexamine", onlyOne: true, commandType: CommandType.Vehicle,
            description: "Used to examine a vehicles attributes")]
        public static void VehicleCommandExamineAttribute(IPlayer player, string plate = "")
        {
            if (player?.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            if (plate == "")
            {
                player.SendSyntaxMessage("/vexamine [Plate/Near]");
                return;
            }

            IVehicle vehicle = null;

            if (plate.ToLower().Contains("near"))
            {
                vehicle = VehicleHandler.FetchNearestVehicle(player);
            }
            else
            {
                vehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.NumberplateText.ToLower() == plate.ToLower());
            }

            if (vehicle == null || vehicle.FetchVehicleData() == null)
            {
                player.SendErrorNotification("Unable to find the vehicle.");
                return;
            }

            if (vehicle.Position.Distance(player.Position) > 5)
            {
                player.SendErrorNotification("You must be closer!");
                return;
            }

            string description = vehicle.FetchVehicleData().Description;

            if (string.IsNullOrEmpty(description))
            {
                player.SendInfoNotification($"Vehicle Attribute is not set for this vehicle.");
                return;
            }

            player.SendInfoNotification($"Vehicle Attribute for {vehicle.NumberplateText}: {description}");

            player.SendEmoteMessage($"looks at the {vehicle.FetchVehicleData().Name}.");
        }

        [Command("refuel", commandType: CommandType.Vehicle, description: "Used to refuel vehicles")]
        public static void VehicleCommandRefuel(IPlayer player)
        {
            if (player?.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (!player.IsInVehicle)
            {
                if (playerInventory.HasItem("ITEM_FUELCAN_EMPTY"))
                {
                    player.Emit("vehicle:fuel:isNearPump");
                    return;
                }

                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            if (player.Seat != 1)
            {
                player.SendErrorNotification("You must be the driver.");
                return;
            }

            if (playerInventory.HasItem("ITEM_FUELCAN"))
            {
                InventoryItem fuelCan = playerInventory.GetItem("ITEM_FUELCAN");

                playerInventory.RemoveItem(fuelCan);

                player.Vehicle.GetClass().FuelLevel += 10;

                player.SendInfoNotification($"You've filled the vehicle up with 10 more litres of fuel!");

                playerInventory.AddItem(new InventoryItem("ITEM_FUELCAN_EMPTY", "Fuelcan (Empty)"));

                return;
            }

            player.Emit("vehicle:fuel:isNearPump");
        }

        public static void IsPlayerNearFuelPump(IPlayer player, bool nearPump)
        {
            if (!player.IsInVehicle && nearPump)
            {
                Inventory.Inventory playerInventory = player.FetchInventory();

                double fuelCanCost = 10 * 0.33;

                if (!playerInventory.HasItem("ITEM_FUELCAN_EMPTY"))
                {
                    player.SendErrorNotification("You must be in a vehicle or have a empty fuel can!");
                    return;
                }

                if (player.GetClass().Cash < fuelCanCost)
                {
                    player.SendErrorNotification($"You require {fuelCanCost:C} to fill this fuel can.");
                    return;
                }

                InventoryItem emptyFuelCan = playerInventory.GetItem("ITEM_FUELCAN_EMPTY");

                playerInventory.RemoveItem(emptyFuelCan);

                playerInventory.AddItem(new InventoryItem("ITEM_FUELCAN_EMPTY", "Fuelcan (10 Litres)"));

                player.RemoveCash(fuelCanCost);

                player.SendInfoNotification($"You've filled the fuel can up.");
                return;
            }

            IVehicle playerVehicle = player.Vehicle;

            if (!nearPump)
            {
                player.SendErrorNotification("You're not near a pump!");
                return;
            }

            int fuelRequired = 100 - playerVehicle.GetClass().FuelLevel;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter.FactionDuty)
            {
                Models.Vehicle vehicleData = playerVehicle.FetchVehicleData();

                if (vehicleData != null)
                {
                    if (vehicleData.FactionId > 0)
                    {
                        player.SendInfoNotification(
                            $"You filled up with {fuelRequired} gallons. This has cost you {0:C}.");
                        playerVehicle.GetClass().FuelLevel = 100;
                        return;
                    }
                }
            }

            double cost = fuelRequired * 0.33;

            if (player.GetClass().Cash < cost)
            {
                player.SendErrorNotification($"You don't have enough. You require {cost:C}.");
                return;
            }

            player.RemoveCash(cost);

            player.SendInfoNotification(
                $"You filled up with {fuelRequired} gallons. This has cost you {cost:C}.");

            playerVehicle.GetClass().FuelLevel = 100;
        }

        [Command("vlock", commandType: CommandType.Vehicle, description: "Locks / Unlocks your vehicle")]
        public static bool ToggleNearestVehicleLock(IPlayer player)
        {
            try
            {
                IVehicle nearestVehicle = VehicleHandler.FetchNearestVehicle(player, 4f);

                if (nearestVehicle == null)
                {
                    return false;
                }

                lock (nearestVehicle)
                {
                    bool canUnlock = false;

                    Inventory.Inventory playerInventory = player.FetchInventory();

                    var keyList = playerInventory.GetInventoryItems("ITEM_VEHICLE_KEY");

                    if (nearestVehicle.GetData("RentalCar:KeyCode", out string keyCode))
                    {
                        canUnlock = keyList.Any(x => x.ItemValue == keyCode);
                    }

                    if (nearestVehicle.GetData("Trucking:Owner", out int truckOwner))
                    {
                        if (truckOwner != player.GetClass().CharacterId)
                        {
                            player.SendPermissionError();
                            return false;
                        }

                        canUnlock = true;
                    }

                    if (!canUnlock)
                    {
                        canUnlock = player.GetClass().AdminDuty;
                    }

                    if (nearestVehicle.FetchVehicleData() != null && !canUnlock)
                    {
                        if (nearestVehicle.FetchVehicleData().Impounded)
                        {
                            if (player.IsLeo(true))
                            {
                                canUnlock = true;
                            }
                            else
                            {
                                player.SendErrorNotification("The vehicle is impounded.");
                                return true;
                            }
                        }

                        if (keyList.FirstOrDefault(x => x.ItemValue == nearestVehicle.FetchVehicleData().Keycode) !=
                            null)
                        {
                            canUnlock = true;
                        }

                        if (!canUnlock)
                        {
                            if (player.FetchCharacter().ActiveFaction > 0 &&
                                nearestVehicle.FetchVehicleData().FactionId > 0)
                            {
                                canUnlock = player.FetchCharacter().ActiveFaction ==
                                            nearestVehicle.FetchVehicleData().FactionId;

                                if (!canUnlock)
                                {
                                    player.SendErrorNotification("You don't have the keys");
                                    return true;
                                }
                            }
                        }

                        if (!canUnlock)
                        {
                            player.SendErrorNotification("You don't have the keys");
                            return true;
                        }

                        using Context context = new Context();

                        Models.Vehicle vehicleData = context.Vehicle.Find(nearestVehicle.FetchVehicleData().Id);

                        VehicleLockState currentLockState = nearestVehicle.LockState;
                        if (currentLockState != VehicleLockState.Unlocked)
                        {
                            vehicleData.Locked = false;
                            context.SaveChanges();

                            nearestVehicle.LockState = VehicleLockState.Unlocked;
                            player.SendEmoteMessage($"unlocks the {nearestVehicle.FetchVehicleData()?.Name}.");
                            Logging.AddToCharacterLog(player,
                                $"Has unlocked vehicle Id: {nearestVehicle.FetchVehicleData().Id}");

                            return true;
                        }

                        vehicleData.Locked = true;
                        context.SaveChanges();

                        nearestVehicle.LockState = VehicleLockState.Locked;

                        player.SendEmoteMessage($"locks the {nearestVehicle.FetchVehicleData()?.Name}.");
                        Logging.AddToCharacterLog(player,
                            $"Has locked vehicle Id: {nearestVehicle.FetchVehicleData().Id}");

                        player.Emit("PlayVehicleLockSound", nearestVehicle);

                        return true;
                    }

                    if (canUnlock)
                    {
                        VehicleLockState currentLockState = nearestVehicle.LockState;
                        if (currentLockState != VehicleLockState.Unlocked)
                        {
                            nearestVehicle.LockState = VehicleLockState.Unlocked;
                            player.SendEmoteMessage($"unlocks the vehicle.");
                        }
                        else
                        {
                            nearestVehicle.LockState = VehicleLockState.Locked;

                            player.SendEmoteMessage($"locks the vehicle.");
                        }

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        [Command("engine", commandType: CommandType.Vehicle, description: "Allows you to start / stop the vehicle")]
        public static bool EngineCommand(IPlayer player)
        {
            if (player.FetchCharacter() == null) return false;

            if (!player.IsInVehicle)
            {
                player.SendNotification($"~r~You must be in a vehicle.");
                return false;
            }

            if (player.Vehicle.Driver != player)
            {
                player.SendNotification($"~r~Your not in the drivers seat!");
                return false;
            }

            if (player.Vehicle.GetClass().FuelLevel == 0)
            {
                player.SendNotification("~r~This vehicle has no fuel!");
                return false;
            }

            if (player.Vehicle.FetchVehicleData()?.Class == 13)
            {
                // Cycle
                player.SendNotification("~r~You can't do that on a bicycle!");
                return false;
            }

            if (player.Vehicle.GetData("ActiveRepair", out bool isRepairing))
            {
                if (isRepairing)
                {
                    player.SendErrorNotification("The vehicle is currently being fixed.");
                    return false;
                }
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            var keyList = playerInventory.GetInventoryItems("ITEM_VEHICLE_KEY");

            bool canEngine = false;

            if (player.Vehicle.GetData("Trucking:Owner", out int truckOwner))
            {
                bool hasLoadData = player.Vehicle.HasData("Trucking:Loading");
                if (hasLoadData)
                {
                    player.SendErrorNotification("The truck is being loaded.");
                    return false;
                }

                if (truckOwner != player.GetClass().CharacterId)
                {
                    player.SendPermissionError();
                    return false;
                }

                canEngine = true;
            }

            bool isDmvVehicle = player.Vehicle.GetData("DMV:OwnerCharacter", out int characterId);

            if (isDmvVehicle)
            {
                canEngine = characterId == player.GetClass().CharacterId;
            }

            if (player.Vehicle.GetData("RentalCar:KeyCode", out string keyCode))
            {
                canEngine = keyList.Any(x => x.ItemValue == keyCode);
            }

            if (player.Vehicle.FetchVehicleData() != null)
            {
                if (player.Vehicle.FetchVehicleData().Impounded)
                {
                    player.SendErrorNotification("The vehicle is impounded.");
                    return false;
                }

                // Database vehicle
                if (keyList.FirstOrDefault(x => x.ItemValue == player.Vehicle.FetchVehicleData().Keycode) != null)
                {
                    canEngine = true;
                }

                if (!canEngine)
                {
                    canEngine = player.GetClass().AdminDuty;
                }

                if (!canEngine)
                {
                    if (player.FetchCharacter().ActiveFaction != 0)
                    {
                        canEngine = player.FetchCharacter().ActiveFaction ==
                                    player.Vehicle.FetchVehicleData().FactionId;

                        if (!canEngine)
                        {
                            player.SendErrorNotification("You don't have the keys");
                            return false;
                        }
                    }
                    else if (player.FetchCharacter().ActiveFaction == 0)
                    {
                        player.SendErrorNotification("You don't have the keys");
                        return false;
                    }
                }
            }

            using Context context = new Context();
            var vehicleDb = context.Vehicle.Find(player.Vehicle.GetVehicleId());
            if (vehicleDb == null)
            {
                player.Vehicle.EngineOn = !player.Vehicle.EngineOn;
                player.SendEmoteMessage(player.Vehicle.EngineOn ? "turns the vehicle on." : "turns the vehicle off.");
                return true;
            }

            vehicleDb.Engine = !vehicleDb.Engine;

            context.SaveChanges();

            player.Vehicle.EngineOn = vehicleDb.Engine;

            player.Emit("Vehicle:SetEngineStatus", player.Vehicle, player.Vehicle.EngineOn, false);

            player.SendEmoteMessage(vehicleDb.Engine
                ? $"turns the {vehicleDb.Name} engine on."
                : $"turns the {vehicleDb.Name} engine off.");
            Logging.AddToCharacterLog(player, $"Has set vehicle Id {vehicleDb.Id} engine status to {vehicleDb.Engine}");

            return true;
        }

        public static IVehicle SpawnVehicleById(int vehicleId, Position vehiclePosition = default)
        {
            Models.Vehicle vehicleData = Models.Vehicle.FetchVehicle(vehicleId);

            if (vehiclePosition.Equals(default))
            {
                vehiclePosition = new Position(vehicleData.PosX, vehicleData.PosY, vehicleData.PosZ);
            }

            if (vehicleData == null) return null;

            IVehicle vehicle = LoadVehicle.LoadDatabaseVehicleAsync(vehicleData, vehiclePosition, true).Result;

            return vehicle;
        }

        [Command("windows", commandType: CommandType.Vehicle, description: "Allows you to roll down / up your windows")]
        public static void CommandVehicleWindows(IPlayer player)
        {
            /* windowIndex

            0 = Front Right Window
            1 = Front Left Window
            2 = Back Right Window
            3 = Back Left Window

             */

            if (!player.IsSpawned()) return;

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null)
            {
                player.SendErrorNotification("You must be in a vehicle!");
                return;
            }

            if (player.Vehicle.FetchVehicleData()?.Class == 13)
            {
                // Cycle
                player.SendNotification("~r~You can't do that on a bicycle!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            bool driverWindowStatus = playerVehicle.IsWindowOpened(0);

            if (driverWindowStatus && player.Seat == 1)
            {
                // Window Open & In Driver Seat
                menuItems.Add(new NativeMenuItem("Close Drivers Window"));
            }

            if (!driverWindowStatus && player.Seat == 1)
            {
                // Window Closed & In Driver Seat
                menuItems.Add(new NativeMenuItem("Open Drivers Window"));
            }

            bool frontPassWindowStatus = playerVehicle.IsWindowOpened(1);

            if (frontPassWindowStatus && player.Seat == 1 || frontPassWindowStatus && player.Seat == 2)
            {
                // Window Open & In Driver Seat or Front Pass
                menuItems.Add(new NativeMenuItem("Close Front Passenger Window"));
            }

            if (!frontPassWindowStatus && player.Seat == 1 || !frontPassWindowStatus && player.Seat == 2)
            {
                // Window Closed & In Driver Seat or Front Pass
                menuItems.Add(new NativeMenuItem("Open Front Passenger Window"));
            }

            bool driverRearWindowStatus = playerVehicle.IsWindowOpened(2);

            if (driverRearWindowStatus && player.Seat == 1 || driverRearWindowStatus && player.Seat == 3)
            {
                // Window Open & Driver Seat or Behind Driver
                menuItems.Add(new NativeMenuItem("Close Behind Driver Window"));
            }

            if (!driverRearWindowStatus && player.Seat == 1 || !driverRearWindowStatus && player.Seat == 3)
            {
                // Window Closed & Driver Seat or Behind Driver
                menuItems.Add(new NativeMenuItem("Open Behind Driver Window"));
            }

            bool passengerRearWindowStatus = playerVehicle.IsWindowOpened(3);

            if (passengerRearWindowStatus && player.Seat == 1 || passengerRearWindowStatus && player.Seat == 4)
            {
                // Window Open & Driver Seat or Behind Front Pass
                menuItems.Add(new NativeMenuItem("Close Behind Passenger Window"));
            }

            if (!passengerRearWindowStatus && player.Seat == 1 || !passengerRearWindowStatus && player.Seat == 4)
            {
                // Window Closed & Driver Seat or Behind Front Pass
                menuItems.Add(new NativeMenuItem("Open Behind Passenger Window"));
            }

            if (player.Seat == 1)
            {
                // In Drivers Seat

                menuItems.Add(new NativeMenuItem("Close All Windows"));
                menuItems.Add(new NativeMenuItem("Open All Windows"));
            }

            NativeMenu menu = new NativeMenu("vehicle:windowControl", "Windows", "Vehicle Window Control", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnWindowControlSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (player.Vehicle == null)
            {
                player.SendErrorNotification("You're not in a vehicle!");
                return;
            }

            IVehicle playerVehicle = player.Vehicle;

            if (option == "Close All Windows")
            {
                for (byte i = 0; i < 4; i++)
                {
                    VehicleHandler.SetVehicleWindowState(player, playerVehicle, i, false);
                    //playerVehicle.SetWindowOpened(i, false);
                }

                player.SendInfoNotification("You have closed all the windows!");
                player.SendEmoteMessage("closes the vehicle windows.");
                return;
            }

            if (option == "Open All Windows")
            {
                for (byte i = 0; i < 4; i++)
                {
                    VehicleHandler.SetVehicleWindowState(player, playerVehicle, i, true);
                    //playerVehicle.SetWindowOpened(i, true);
                }

                player.SendInfoNotification("You have opened all the windows!");
                player.SendEmoteMessage("opens the vehicle windows.");
                return;
            }

            if (option.Contains("Drivers Window"))
            {
                if (playerVehicle.IsWindowOpened(0))
                {
                    VehicleHandler.SetVehicleWindowState(player, playerVehicle, 0, false);
                    //playerVehicle.SetWindowOpened(0, false);
                    player.SendInfoNotification($"You've closed the Drivers Window");
                    player.SendEmoteMessage("closes the driver side window.");
                    return;
                }

                VehicleHandler.SetVehicleWindowState(player, playerVehicle, 0, true);
                //playerVehicle.SetWindowOpened(0, true);
                player.SendInfoNotification($"You've opened the Drivers Window");
                player.SendEmoteMessage("opens the driver side window.");
                return;
            }

            if (option.Contains("Front Passenger Window"))
            {
                if (playerVehicle.IsWindowOpened(1))
                {
                    VehicleHandler.SetVehicleWindowState(player, playerVehicle, 1, false);
                    //playerVehicle.SetWindowOpened(1, false);
                    player.SendInfoNotification($"You've closed the Front Passenger Window");
                    player.SendEmoteMessage("closes the front passenger side window.");
                    return;
                }

                VehicleHandler.SetVehicleWindowState(player, playerVehicle, 1, true);
                //playerVehicle.SetWindowOpened(1, true);
                player.SendInfoNotification($"You've opened the Front Passenger Window");
                player.SendEmoteMessage("opens the front passenger side window.");
                return;
            }

            if (option.Contains("Behind Driver Window"))
            {
                if (playerVehicle.IsWindowOpened(2))
                {
                    VehicleHandler.SetVehicleWindowState(player, playerVehicle, 2, false);
                    //playerVehicle.SetWindowOpened(2, false);
                    player.SendInfoNotification($"You've closed the Rear Driver Window");
                    player.SendEmoteMessage("closes the rear driver side window.");
                    return;
                }

                VehicleHandler.SetVehicleWindowState(player, playerVehicle, 2, true);

                //playerVehicle.SetWindowOpened(2, true);
                player.SendInfoNotification($"You've opened the Rear Driver Window");
                player.SendEmoteMessage("opens the rear driver side window..");
                return;
            }

            if (option.Contains("Behind Passenger"))
            {
                if (playerVehicle.IsWindowOpened(3))
                {
                    VehicleHandler.SetVehicleWindowState(player, playerVehicle, 3, false);
                    //playerVehicle.SetWindowOpened(3, false);
                    player.SendInfoNotification($"You've closed the Rear Passenger Window");
                    player.SendEmoteMessage("closes the rear passenger side window.");
                    return;
                }

                VehicleHandler.SetVehicleWindowState(player, playerVehicle, 3, true);
                //playerVehicle.SetWindowOpened(3, true);
                player.SendInfoNotification($"You've opened the Rear Passenger Window");
                player.SendEmoteMessage("opens the rear passenger side window.");
                return;
            }
        }

        [Command("doors", commandType: CommandType.Vehicle, description: "Allows control of your vehicle doors")]
        public static void VehicleCommandDoors(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null)
            {
                playerVehicle = VehicleHandler.FetchNearestVehicle(player);

                if (playerVehicle == null)
                {
                    player.SendErrorNotification("You're not near a vehicle.");
                    return;
                }

                if (playerVehicle.FetchVehicleData()?.Class == 13)
                {
                    // Cycle
                    player.SendNotification("~r~You can't do that on a bicycle!");
                    return;
                }

                if (playerVehicle.LockState == VehicleLockState.Locked)
                {
                    // Near Vehicle, Not inside, vehicle locked
                    player.SendErrorNotification("The vehicle is locked!");
                    return;
                }
            }

            if (playerVehicle.FetchVehicleData()?.Class == 13)
            {
                // Cycle
                player.SendNotification("~r~You can't do that on a bicycle!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            VehicleDoorState driverFrontDoorState = (VehicleDoorState)playerVehicle.GetDoorState((byte)VehicleDoor.DriverFront);
            VehicleDoorState passFrontDoorState = (VehicleDoorState)playerVehicle.GetDoorState((byte)VehicleDoor.PassengerFront);
            VehicleDoorState driverRearDoorState = (VehicleDoorState)playerVehicle.GetDoorState((byte)VehicleDoor.DriverRear);
            VehicleDoorState passRearDoorState = (VehicleDoorState)playerVehicle.GetDoorState((byte)VehicleDoor.PassengerRear);
            VehicleDoorState trunkDoorState = (VehicleDoorState)playerVehicle.GetDoorState((byte)VehicleDoor.Trunk);
            VehicleDoorState hoodDoorState = (VehicleDoorState)playerVehicle.GetDoorState((byte)VehicleDoor.Hood);

            if (player.Seat == 1 || !player.IsInVehicle)
            {
                // Drivers
                if (driverFrontDoorState != VehicleDoorState.Closed &&
                    driverFrontDoorState != VehicleDoorState.DoesNotExists)
                {
                    // Door Exists & Is Open
                    menuItems.Add(new NativeMenuItem("Close Drivers Door"));
                }

                if (driverFrontDoorState == VehicleDoorState.Closed)
                {
                    menuItems.Add(new NativeMenuItem("Open Drivers Door"));
                }

                // Trunk

                if (trunkDoorState != VehicleDoorState.Closed &&
                    trunkDoorState != VehicleDoorState.DoesNotExists)
                {
                    // Door Exists & Is Open
                    menuItems.Add(new NativeMenuItem("Close Trunk"));
                }

                if (trunkDoorState == VehicleDoorState.Closed)
                {
                    menuItems.Add(new NativeMenuItem("Open Trunk"));
                }
                // Hood

                if (hoodDoorState != VehicleDoorState.Closed &&
                    hoodDoorState != VehicleDoorState.DoesNotExists)
                {
                    // Door Exists & Is Open
                    menuItems.Add(new NativeMenuItem("Close Hood"));
                }

                if (hoodDoorState == VehicleDoorState.Closed)
                {
                    menuItems.Add(new NativeMenuItem("Open Hood"));
                }
            }

            if (player.Seat == 2 || !player.IsInVehicle)
            {
                // Front Pass
                if (passFrontDoorState != VehicleDoorState.Closed &&
                    passFrontDoorState != VehicleDoorState.DoesNotExists)
                {
                    // Door Exists & Is Open
                    menuItems.Add(new NativeMenuItem("Close Front Passenger Door"));
                }

                if (passFrontDoorState == VehicleDoorState.Closed)
                {
                    menuItems.Add(new NativeMenuItem("Open Front Passenger Door"));
                }
            }

            if (player.Seat == 3 || !player.IsInVehicle)
            {
                // Rear Driver
                if (driverRearDoorState != VehicleDoorState.Closed &&
                    driverRearDoorState != VehicleDoorState.DoesNotExists)
                {
                    // Door Exists & Is Open
                    menuItems.Add(new NativeMenuItem("Close Rear Driver Door"));
                }

                if (driverRearDoorState == VehicleDoorState.Closed)
                {
                    menuItems.Add(new NativeMenuItem("Open Rear Driver Door"));
                }
            }

            if (player.Seat == 4 || !player.IsInVehicle)
            {
                // Rear Driver
                if (passRearDoorState != VehicleDoorState.Closed &&
                    passRearDoorState != VehicleDoorState.DoesNotExists)
                {
                    // Door Exists & Is Open
                    menuItems.Add(new NativeMenuItem("Close Rear Passenger Door"));
                }

                if (passRearDoorState == VehicleDoorState.Closed)
                {
                    menuItems.Add(new NativeMenuItem("Open Rear Passenger Door"));
                }
            }

            NativeMenu menu = new NativeMenu("vehicle:doorControl", "Door Control", "Vehicle Door Control", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnVehicleDoorControl(IPlayer player, string option)
        {
            if (option == "Close") return;

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null)
            {
                playerVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.Position.Distance(player.Position) < 5f);

                if (playerVehicle == null)
                {
                    player.SendErrorNotification("You're not near a vehicle.");
                    return;
                }

                if (playerVehicle.LockState == VehicleLockState.Locked)
                {
                    // Near Vehicle, Not inside, vehicle locked
                    player.SendErrorNotification("The vehicle is locked!");
                    return;
                }
            }

            if (option == "Close Drivers Door")
            {
                //playerVehicle.SetDoorState((int)VehicleDoor.DriverFront, VehicleDoorState.Closed);
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.DriverFront, false);
                player.SendInfoNotification($"You have closed the Drivers Door.");
                player.SendEmoteMessage("closes the drivers door.");
                return;
            }

            if (option == "Open Drivers Door")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.DriverFront, true);
                //playerVehicle.SetDoorState((int)VehicleDoor.DriverFront, VehicleDoorState.OpenedLevel1);
                player.SendInfoNotification($"You have opened the Drivers Door.");
                player.SendEmoteMessage("open the drivers door.");
                return;
            }

            if (option == "Close Trunk")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.Trunk, false);
                //playerVehicle.SetDoorState((int)VehicleDoor.Trunk, VehicleDoorState.Closed);
                player.SendInfoNotification($"You have closed the Trunk.");
                player.SendEmoteMessage("closes the trunk.");
                return;
            }

            if (option == "Open Trunk")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.Trunk, true);
                //playerVehicle.SetDoorState((int)VehicleDoor.Trunk, VehicleDoorState.OpenedLevel1);
                //playerVehicle.SetDoorState((int)VehicleDoor.Trunk, 45);
                player.SendInfoNotification($"You have opened the Trunk.");
                player.SendEmoteMessage("opens the trunk.");
                return;
            }

            if (option == "Close Hood")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.Hood, false);
                //playerVehicle.SetDoorState((int)VehicleDoor.Hood, VehicleDoorState.Closed);
                player.SendInfoNotification($"You have closed the Hood.");
                player.SendEmoteMessage("closes the hood.");
                return;
            }

            if (option == "Open Hood")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.Hood, true);
                //playerVehicle.SetDoorState((int)VehicleDoor.Hood, VehicleDoorState.OpenedLevel1);
                player.SendInfoNotification($"You have opened the Hood.");
                player.SendEmoteMessage("opens the hood.");
                return;
            }

            if (option == "Close Front Passenger Door")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.PassengerFront, false);
                //playerVehicle.SetDoorState((int)VehicleDoor.PassengerFront, VehicleDoorState.Closed);
                player.SendInfoNotification($"You have closed the Front Passenger Door.");
                player.SendEmoteMessage("closes the front passenger door.");
                return;
            }

            if (option == "Open Front Passenger Door")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.PassengerFront, true);
                //playerVehicle.SetDoorState((int)VehicleDoor.PassengerFront, VehicleDoorState.OpenedLevel1);
                player.SendInfoNotification($"You have opened the Front Passenger Door.");
                player.SendEmoteMessage("opens the front passenger door.");
                return;
            }

            if (option == "Close Rear Driver Door")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.DriverRear, false);
                //playerVehicle.SetDoorState((int)VehicleDoor.DriverRear, VehicleDoorState.Closed);
                player.SendInfoNotification($"You have closed the Rear Driver Door.");
                player.SendEmoteMessage("closes the rear driver door.");
                return;
            }

            if (option == "Open Rear Driver Door")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.DriverRear, true);
                //playerVehicle.SetDoorState((int)VehicleDoor.DriverRear, VehicleDoorState.OpenedLevel1);
                player.SendInfoNotification($"You have opened the Rear Driver Door.");
                player.SendEmoteMessage("opens the rear driver door.");
                return;
            }

            if (option == "Close Rear Passenger Door")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.PassengerRear, false);
                //playerVehicle.SetDoorState((int)VehicleDoor.PassengerRear, VehicleDoorState.Closed);
                player.SendInfoNotification($"You have closed the Rear Passenger Door.");
                player.SendEmoteMessage("closes the rear passenger door.");
                return;
            }

            if (option == "Open Rear Passenger Door")
            {
                VehicleHandler.SetVehicleDoorState(player, playerVehicle, (int)VehicleDoor.PassengerRear, true);
                //playerVehicle.SetDoorState((int)VehicleDoor.PassengerRear, VehicleDoorState.OpenedLevel1);
                player.SendInfoNotification($"You have opened the Rear Passenger Door.");
                player.SendEmoteMessage("opens the rear passenger door.");
                return;
            }
        }

        [Command("vfaction", commandType: CommandType.Vehicle,
            description: "Sets / Removes the vehicle from your active faction")]
        public static void VehicleCommandVFaction(IPlayer player)
        {
            if (player?.FetchCharacter() == null)
            {
                player.SendPermissionError();
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            IVehicle playerVehicle = player.Vehicle;

            Models.Vehicle vehicleData = playerVehicle.FetchVehicleData();

            if (vehicleData?.OwnerId != player.GetClass().CharacterId)
            {
                player.SendErrorNotification("You don't own this vehicle.");
                return;
            }

            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You're not in a faction or have it set as active.");
                return;
            }

            using Context context = new Context();

            Models.Vehicle vehicleDb = context.Vehicle.Find(vehicleData.Id);

            bool currentFactionVehicle = vehicleDb.FactionId == 0;

            vehicleDb.FactionId = vehicleDb.FactionId != 0 ? 0 : activeFaction.Id;

            context.SaveChanges();

            if (currentFactionVehicle)
            {
                player.SendInfoNotification($"You've set the vehicle to your active faction. {activeFaction.Name}.");

                return;
            }

            player.SendInfoNotification($"You've removed this vehicle from the faction.");
            return;
        }

        [Command("vid", commandType: CommandType.Vehicle, description: "Used to display the vehicle id.")]
        public static void VehicleCommandViewId(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            IVehicle vehicle = VehicleHandler.FetchNearestVehicle(player);

            if (vehicle == null)
            {
                player.SendErrorNotification("You're not in a vehicle.");
                return;
            }

            player.SendInfoNotification($"The current vehicle id is {vehicle.GetClass().Id}.");
        }

        [Command("vget", commandType: CommandType.Vehicle, description: "Used to spawn in vehicles")]
        public static void VehicleCommandVGet(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            PropertyGarage propertyGarage = Models.Property.FetchNearbyGarage(player, 5f);

            if (propertyGarage == null)
            {
                List<Models.Vehicle> playerVehicles = Models.Vehicle.FetchCharacterVehicles(player.GetClass().CharacterId)
                    .Where(x => !x.Spawned && !x.IsStored).ToList();

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                foreach (Models.Vehicle playerVehicle in playerVehicles)
                {
                    menuItems.Add(new NativeMenuItem(playerVehicle.Name, playerVehicle.Plate));
                }

                NativeMenu menu =
                    new NativeMenu("vehicle:vget:mainmenu", "Vehicles", "Select a vehicle to spawn", menuItems)
                    {
                        PassIndex = true
                    };

                NativeUi.ShowNativeMenu(player, menu, true);
                return;
            }

            Models.Property property = Models.Property.FetchProperty(propertyGarage.PropertyId);

            if (property == null)
            {
                NotificationExtension.SendErrorNotification(player, "An error occurred fetching the property.");
                return;
            }

            if (property.Locked)
            {
                NotificationExtension.SendErrorNotification(player, "The property is locked!");
                return;
            }

            List<NativeMenuItem> garageItems = new List<NativeMenuItem>();

            List<Models.Vehicle> garageVehicles = Models.Vehicle.FetchPropertyGarageVehicles(propertyGarage.Id);

            foreach (Models.Vehicle garageVehicle in garageVehicles)
            {
                if (garageVehicle.FactionId > 0 && playerCharacter.ActiveFaction == garageVehicle.FactionId)
                {
                    garageItems.Add(new NativeMenuItem(garageVehicle.Name, garageVehicle.Plate));
                }

                if (garageVehicle.FactionId == 0 && garageVehicle.OwnerId == playerCharacter.Id)
                {
                    garageItems.Add(new NativeMenuItem(garageVehicle.Name, garageVehicle.Plate));
                }
            }

            NativeMenu garageMenu = new NativeMenu("vehicle:vget:garageMenu", "Vehicles", "Select a vehicle to spawn", garageItems)
            {
                PassIndex = true
            };

            player.SetData("VGetGarage", propertyGarage.Id);

            NativeUi.ShowNativeMenu(player, garageMenu, true);
        }

        public static async void OnGarageVGetSelect(IPlayer player, string option, int index)
        {
            try
            {
                if (option == "Close") return;

                bool hasData = player.GetData("VGetGarage", out string garageId);

                if (!hasData) return;

                player.DeleteData("VGetGarage");

                Models.Character playerCharacter = player.FetchCharacter();

                if (playerCharacter == null)
                {
                    player.SendLoginError();
                    return;
                }

                PropertyGarage pGarage = Models.Property.FetchNearbyGarage(player, 5f);

                List<Models.Vehicle> garageVehicles = Models.Vehicle.FetchPropertyGarageVehicles(pGarage.Id);

                List<Models.Vehicle> vehicleList = new List<Models.Vehicle>();

                foreach (Models.Vehicle garageVehicle in garageVehicles)
                {
                    if (garageVehicle.FactionId > 0 && playerCharacter.ActiveFaction == garageVehicle.FactionId)
                    {
                        vehicleList.Add(garageVehicle);
                    }
                    else if (garageVehicle.OwnerId == playerCharacter.OwnerId)
                    {
                        vehicleList.Add(garageVehicle);
                    }
                }

                Models.Vehicle selectedVehicle = vehicleList[index];

                if (selectedVehicle == null)
                {
                    player.SendErrorNotification("An error occurred fetching the vehicle.");
                    return;
                }

                using Context context = new Context();

                Models.Property property = await context.Property.FindAsync(pGarage.PropertyId);

                if (property == null)
                {
                    player.SendErrorNotification("Unable to find this garage's property.");
                    return;
                }

                List<PropertyGarage> propertyGarages =
                    JsonConvert.DeserializeObject<List<PropertyGarage>>(property.GarageList);

                PropertyGarage propertyGarage = propertyGarages.FirstOrDefault(x => x.Id == garageId);

                if (propertyGarage == null)
                {
                    player.SendErrorNotification("Unable to find this garage.");
                    return;
                }

                await context.SaveChangesAsync();

                Position spawnPosition = new Position(propertyGarage.PosX, propertyGarage.PosY, propertyGarage.PosZ);

                await LoadVehicle.LoadDatabaseVehicleAsync(selectedVehicle, spawnPosition);

                player.SendInfoNotification($"You have spawned {selectedVehicle.Name}, plate: {selectedVehicle.Plate}.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static async void OnVGetSelect(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            List<Models.Vehicle> playerVehicles = Models.Vehicle.FetchCharacterVehicles(player.GetClass().CharacterId)
                .Where(x => !x.Spawned).ToList();

            Models.Vehicle selectedVehicle = playerVehicles[index];

            if (selectedVehicle == null)
            {
                player.SendErrorNotification("An error occurred fetching the data.");
                return;
            }

            if (!string.IsNullOrEmpty(selectedVehicle.GarageId))
            {
                PropertyGarage garage = Models.Property.FetchPropertyGarage(selectedVehicle.GarageId);

                if (garage == null)
                {
                    player.SendErrorNotification("Unable to find this garage.");
                    return;
                }

                using Context context = new Context();

                Models.Property property = await context.Property.FindAsync(garage.PropertyId);

                if (property == null)
                {
                    player.SendErrorNotification("Unable to find this garage's property.");
                    return;
                }

                List<PropertyGarage> propertyGarages =
                    JsonConvert.DeserializeObject<List<PropertyGarage>>(property.GarageList);

                PropertyGarage propertyGarage = propertyGarages.FirstOrDefault(x => x.Id == garage.Id);

                if (propertyGarage == null)
                {
                    player.SendErrorNotification("Unable to find this garage.");
                    return;
                }
                Position spawnPosition = new Position(garage.PosX, garage.PosY, garage.PosZ);

                await LoadVehicle.LoadDatabaseVehicleAsync(selectedVehicle, spawnPosition);

                player.SendInfoNotification($"You have spawned {selectedVehicle.Name}, plate: {selectedVehicle.Plate}.");

                return;
            }

            Position vehiclePosition = new Position(selectedVehicle.PosX, selectedVehicle.PosY, selectedVehicle.PosZ);

            await LoadVehicle.LoadDatabaseVehicleAsync(selectedVehicle, vehiclePosition);

            player.SendInfoNotification($"You have spawned {selectedVehicle.Name}, plate: {selectedVehicle.Plate}.");
        }

        [Command("vpark", commandType: CommandType.Vehicle,
            description: "Garage: Used to store a vehicle in a property garage")]
        public static void VehicleCommandVPark(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (player.Vehicle?.FetchVehicleData() == null)
            {
                player.SendPermissionError();
                return;
            }

            PropertyGarage propertyGarage = Models.Property.FetchNearbyGarage(player, 5f);

            if (propertyGarage == null)
            {
                player.SendErrorNotification("Your not near a garage.");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(propertyGarage.PropertyId);

            if (property == null)
            {
                player.SendErrorNotification("Unable to find this property.");
                return;
            }

            List<PropertyGarage> propertyGarages =
                JsonConvert.DeserializeObject<List<PropertyGarage>>(property.GarageList);

            PropertyGarage pGarage = propertyGarages.FirstOrDefault(x => x.Id == propertyGarage.Id);

            if (pGarage == null)
            {
                player.SendErrorNotification("Unable to find this property garage.");
                return;
            }

            // Fetch amount of vehicles in garage
            int garageVehicles = Models.Vehicle.FetchPropertyGarageVehicles(pGarage.Id).Count;

            // If it has space remaining
            int remaining = pGarage.VehicleCount - garageVehicles;

            if (remaining <= 0)
            {
                player.SendErrorNotification("This garage is full.");
                return;
            }

            AltAsync.Do(() => { Models.Vehicle.UpdateVehicle(player.Vehicle, true, pGarage.Id, true); });

            player.SendInfoNotification($"You've parked the vehicle.");
        }

        [Command("storecycle", onlyOne: true, commandType: CommandType.Vehicle,
            description: "Other: Stores a cycle in your vehicle")]
        public static void VehicleCommandStoreCycle(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/storecycle [VehicleId]");
                return;
            }

            bool tryParse = int.TryParse(args, out int targetId);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/storecycle [VehicleId]");
                player.SendErrorNotification("Parameter must be a number.");
                return;
            }

            Models.Vehicle vehicleData = player.Vehicle.FetchVehicleData();

            if (vehicleData == null)
            {
                player.SendErrorNotification("You need to be in a server vehicle.");
                return;
            }

            if (vehicleData.Class != 2)
            {
                if (vehicleData.Class != 9)
                {
                    if (vehicleData.Class != 12)
                    {
                        player.SendErrorNotification("You're not in the correct vehicle type.");
                        return;
                    }
                }
            }

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == targetId);

            if (targetVehicle == null || targetVehicle.FetchVehicleData() == null)
            {
                player.SendErrorNotification("Invalid Id");
                return;
            }

            if (targetVehicle.FetchVehicleData().Class != 13)
            {
                player.SendErrorNotification("The target vehicle is not a cycle.");
                return;
            }

            player.SetData("StoreCycleId", targetId);

            player.GetTrunkPosition(player.Vehicle, "vehicle:cycle:storeReturnPosition");
        }

        public static void OnCycleStoreReturnTrunkPosition(IPlayer player, float position)
        {
            player.GetData("StoreCycleId", out int targetId);

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == targetId);

            IVehicle playerVehicle = player.Vehicle;
            Position trunkPosition = new Position(playerVehicle.Position.X, position, playerVehicle.Position.Z);

            float distance = trunkPosition.Distance(targetVehicle.Position);

            Console.WriteLine($"Cycle Distance {distance}");

            if (distance > 6)
            {
                player.SendErrorNotification("The cycle is not near your trunk.");
                return;
            }

            using Context context = new Context();

            Models.Vehicle playerVehicleData = context.Vehicle.Find(playerVehicle.GetClass().Id);
            Models.Vehicle targetVehicleData = context.Vehicle.Find(targetId);

            List<int> storedVehicles = JsonConvert.DeserializeObject<List<int>>(playerVehicleData.StoredVehicles);

            if (storedVehicles.Count > 2)
            {
                player.SendErrorNotification("You can only store two cycle in this vehicle.");
                return;
            }

            storedVehicles.Add(targetId);

            playerVehicleData.StoredVehicles = JsonConvert.SerializeObject(storedVehicles);

            targetVehicleData.IsStored = true;

            context.SaveChanges();

            AltAsync.Do(() => { LoadVehicle.UnloadVehicle(targetVehicle); });

            player.SendInfoNotification($"You have put {targetVehicleData.Name} into your trunk.");
            Logging.AddToCharacterLog(player, $"has stored vehicle id {targetVehicleData.Id}");
        }

        [Command("getcycle", commandType: CommandType.Vehicle, description: "Other: Fetches a cycle from the vehicle")]
        public static void VehicleCommandGetCycle(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            Models.Vehicle vehicleData = player.Vehicle.FetchVehicleData();

            List<int> storedIds = JsonConvert.DeserializeObject<List<int>>(vehicleData.StoredVehicles);

            if (!storedIds.Any())
            {
                player.SendErrorNotification("There are no cycles store.");
                return;
            }

            List<Models.Vehicle> storedVehicles = new List<Models.Vehicle>();

            foreach (int storedId in storedIds)
            {
                storedVehicles.Add(Models.Vehicle.FetchVehicle(storedId));
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Models.Vehicle storedVehicle in storedVehicles)
            {
                menuItems.Add(new NativeMenuItem(storedVehicle.Name, $"ID: {storedVehicle.Id}"));
            }

            NativeMenu menu = new NativeMenu("vehicle:cycle:GetCycleSelect", "Cycles", "Select a Cycle", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnGetCycleSelect(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            player.SetData("FetchCycle", index);

            player.GetTrunkPosition(player.Vehicle, "vehicle:cycle:getReturnPosition");
        }

        public static void OnGetReturnTrunkPosition(IPlayer player, float position)
        {
            //position += 1.0f;

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            IVehicle playerVehicle = player.Vehicle;
            Position trunkPosition = new Position(playerVehicle.Position.X, position, playerVehicle.Position.Z);

            player.GetData("FetchCycle", out int vehicleIndex);

            Models.Vehicle vehicleData = player.Vehicle.FetchVehicleData();

            List<int> storedIds = JsonConvert.DeserializeObject<List<int>>(vehicleData.StoredVehicles);

            if (!storedIds.Any())
            {
                player.SendErrorNotification("There are no cycles store.");
                return;
            }

            List<Models.Vehicle> storedVehicles = new List<Models.Vehicle>();

            foreach (int storedId in storedIds)
            {
                storedVehicles.Add(Models.Vehicle.FetchVehicle(storedId));
            }

            Models.Vehicle storedVehicle = storedVehicles[vehicleIndex];

            if (storedVehicle == null)
            {
                player.SendErrorNotification("An error occurred fetching the vehicle data.");
                return;
            }

            using Context context = new Context();

            Models.Vehicle vehicleDb = context.Vehicle.Find(vehicleData.Id);

            storedIds.Remove(storedVehicle.Id);

            vehicleDb.StoredVehicles = JsonConvert.SerializeObject(storedIds);

            Models.Vehicle targetVehicleDb = context.Vehicle.Find(storedVehicle.Id);

            targetVehicleDb.IsStored = false;

            context.SaveChanges();

            LoadVehicle.LoadDatabaseVehicleAsync(storedVehicle, trunkPosition);

            player.SendInfoNotification($"You have removed {storedVehicle.Name} from your vehicle trunk.");

            Logging.AddToCharacterLog(player, $"has removed vehicle id {storedVehicle.Id} from their trunk.");
        }

        [Command("vscrap", commandType: CommandType.Vehicle, description: "Used to scrap your vehicle.")]
        public static void VehicleCommandScrap(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null)
            {
                player.SendErrorNotification("You must be in a vehicle!");
                return;
            }

            Models.Vehicle vehicleData = playerVehicle.FetchVehicleData();

            if (vehicleData == null)
            {
                player.SendErrorNotification("You must be in a ownable vehicle!");
                return;
            }

            if (vehicleData.OwnerId != player.GetClass().CharacterId)
            {
                player.SendErrorNotification("You must be the vehicle owner!");
                return;
            }

            if (vehicleData.Impounded)
            {
                player.SendErrorNotification("You can't scrap an impounded car!");
                return;
            }

            double vehicleMileage = vehicleData.Odometer / 1609.344;

            int vehicleBoughtPrice = vehicleData.VehiclePrice;

            double vehicleValue;

            if (vehicleMileage < 20)
            {
                // 85% of Price with less than 10 miles
                vehicleValue = vehicleBoughtPrice * 0.85;
            }
            else if (vehicleMileage < 800)
            {
                // 30% of Price with less than 200 miles
                vehicleValue = vehicleBoughtPrice * 0.3;
            }
            else if (vehicleMileage < 1500)
            {
                // 15% if less than 500 miles
                vehicleValue = vehicleBoughtPrice * 0.15;
            }
            else
            {
                // Else you get 5%
                vehicleValue = vehicleBoughtPrice * 0.05;
            }

            Console.WriteLine($"Mileage: {vehicleMileage}. Scrap Value: {vehicleValue:C}");

            NativeUi.ShowYesNoMenu(player, "vehicle:ConfirmScrap", "Scrap",
                $"Scrap vehicle for ~g~{vehicleValue:C}~w~?");
        }

        public static void OnConfirmVehicleScrap(IPlayer player, string option)
        {
            if (option == "No") return;

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null)
            {
                player.SendErrorNotification("You must be in a vehicle!");
                return;
            }

            Models.Vehicle vehicleData = playerVehicle.FetchVehicleData();

            if (vehicleData == null)
            {
                player.SendErrorNotification("You must be in a ownable vehicle!");
                return;
            }

            if (vehicleData.OwnerId != player.GetClass().CharacterId)
            {
                player.SendErrorNotification("You must be the vehicle owner!");
                return;
            }

            double vehicleMileage = vehicleData.Odometer / 1609.344;

            int vehicleBoughtPrice = vehicleData.VehiclePrice;

            double vehicleValue;

            if (vehicleMileage < 20)
            {
                // 85% of Price with less than 10 miles
                vehicleValue = vehicleBoughtPrice * 0.85;
            }
            else if (vehicleMileage < 800)
            {
                // 30% of Price with less than 200 miles
                vehicleValue = vehicleBoughtPrice * 0.3;
            }
            else if (vehicleMileage < 1500)
            {
                // 15% if less than 500 miles
                vehicleValue = vehicleBoughtPrice * 0.15;
            }
            else
            {
                // Else you get 5%
                vehicleValue = vehicleBoughtPrice * 0.05;
            }

            player.AddCash(vehicleValue);

            playerVehicle.Delete();

            player.SendInfoNotification($"You've sold your {vehicleData.Name} for {vehicleValue:C}.");

            Logging.AddToCharacterLog(player,
                $"has sold vehicle ID {vehicleData.Id}, Name: {vehicleData.Name} for {vehicleValue:C}.");
        }

        [Command("vsetforsale", onlyOne: true, commandType: CommandType.Vehicle,
            description: "Used to set a sale price for your vehicle")]
        public static void VehicleCommandSetForSale(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/vsetforsale [0/Price]");
                return;
            }

            bool tryParse = int.TryParse(args, out int salePrice);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/vsetforsale [0/Price]");
                return;
            }

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null)
            {
                player.SendErrorNotification("You must be in a vehicle!");
                return;
            }

            Models.Vehicle vehicleData = playerVehicle.FetchVehicleData();

            if (vehicleData == null)
            {
                player.SendErrorNotification("You must be in a ownable vehicle!");
                return;
            }

            if (vehicleData.OwnerId != player.GetClass().CharacterId)
            {
                player.SendErrorNotification("You must be the vehicle owner!");
                return;
            }

            if (vehicleData.FactionId > 0)
            {
                player.SendErrorNotification("You must unset this from the faction first!");
                return;
            }

            using Context context = new Context();

            Models.Vehicle vehicleContext = context.Vehicle.Find(vehicleData.Id);

            vehicleContext.SalePrice = salePrice;

            context.SaveChanges();

            if (salePrice == 0)
            {
                // Set to 0 - Stop for sale

                player.SendInfoNotification($"You've removed your vehicle from being for sale.");
                return;
            }

            player.SendInfoNotification($"You've set your vehicle to be sold for {salePrice:C}.");

            Logging.AddToCharacterLog(player, $"has set vehicle id {vehicleData.Id} sale price to: {salePrice:C}.");
        }

        [Command("buyvehicle", commandType: CommandType.Vehicle,
            description: "Used to purchase a vehicle when inside")]
        public static void VehicleCommandBuyVehicle(IPlayer player)
        {
            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null)
            {
                player.SendErrorNotification("You must be in a vehicle!");
                return;
            }

            Models.Vehicle vehicleData = playerVehicle.FetchVehicleData();

            if (vehicleData == null)
            {
                player.SendErrorNotification("You must be in a ownable vehicle!");
                return;
            }

            if (vehicleData.SalePrice == 0)
            {
                player.SendErrorNotification("This vehicle isn't for sale.");
                return;
            }

            if (player.GetClass().Cash < vehicleData.SalePrice)
            {
                player.SendErrorNotification($"You don't have enough. You require {vehicleData.SalePrice:C}.");
                return;
            }

            int oldOwnerId = vehicleData.OwnerId;

            using Context context = new Context();

            Models.Vehicle vehicle = context.Vehicle.Find(vehicleData.Id);

            player.RemoveCash(vehicleData.SalePrice);

            vehicle.SalePrice = 0;
            vehicle.OwnerCount++;
            vehicle.OwnerId = player.GetClass().CharacterId;

            string newKey = Utility.GenerateRandomString(8);

            vehicle.Keycode = newKey;

            Inventory.Inventory playerInventory = player.FetchInventory();

            playerInventory.AddItem(new InventoryItem("ITEM_VEHICLE_KEY", vehicle.Name, newKey));

            Models.Character oldOwnerCharacter = context.Character.Find(oldOwnerId);

            if (oldOwnerCharacter != null)
            {
                oldOwnerCharacter.Money += vehicleData.SalePrice;
            }

            context.SaveChanges();

            IPlayer oldOwner = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetClass().CharacterId == oldOwnerId);

            oldOwner?.SendInfoNotification($"Someone has bought your {vehicleData.Name} for {vehicleData.SalePrice:C}.");

            player.SendInfoNotification($"You've bought this {vehicleData.Name} for {vehicleData.SalePrice:C}.");

            Logging.AddToCharacterLog(player, $"has bought vehicle id {vehicleData.Id} for {vehicleData.SalePrice:C}.");
        }

        [Command("carwash", commandType: CommandType.Vehicle, description: "Used to clean your car!")]
        public static void VehicleCommandCarWash(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (!player.IsInVehicle || player.Seat != 1)
            {
                player.SendErrorNotification("You must be in the drivers seat of a vehicle.");
                return;
            }

            Position playerPosition = player.Position;

            bool atProperty = false;
            bool atCarWash = false;

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 7f);

            if (nearbyProperty == null)
            {
                List<Models.Property> properties = Models.Property.FetchProperties();

                foreach (Models.Property property in properties)
                {
                    List<PropertyInteractionPoint> points =
                        JsonConvert.DeserializeObject<List<PropertyInteractionPoint>>(property.InteractionPoints);

                    if (points.Any())
                    {
                        foreach (PropertyInteractionPoint propertyInteractionPoint in points)
                        {
                            Position position = new Position(propertyInteractionPoint.PosX, propertyInteractionPoint.PosY, propertyInteractionPoint.PosZ);

                            if (player.Position.Distance(position) <= 7f)
                            {
                                nearbyProperty = property;
                                atProperty = true;
                                break;
                            }
                        }
                    }
                }

                if (nearbyProperty == null)
                {
                    if (playerPosition.Distance(new Position(22.786814f, -1392.0264f, 29.313599f)) < 5f)
                    {
                        atCarWash = true;
                    }
                }
            }

            if (atProperty)
            {
                if (nearbyProperty.PropertyType != PropertyType.VehicleModShop)
                {
                    player.SendErrorNotification("You must be at a mechanic garage!");
                }
            }

            if (!atProperty && !atCarWash)
            {
                player.SendErrorNotification("You must be at a car wash location!");
                return;
            }

            if (player.GetClass().Cash < 5)
            {
                player.SendErrorNotification("You must have $5 to do this!");
                return;
            }

            player.SendInfoNotification($"You've washed the vehicle.");

            player.RemoveCash(5);

            Alt.EmitAllClients("CleanVehicle", player.Vehicle);
        }

        [Command("anchor", commandType: CommandType.Vehicle, description: "Used to anchor a boat")]
        public static void VehicleCommandAnchor(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (!player.IsInVehicle)
            {
                NotificationExtension.SendErrorNotification(player, "You must be in a vehicle.");
                return;
            }

            if (player.Seat != 1)
            {
                NotificationExtension.SendErrorNotification(player, "You must be in the drivers seat.");
                return;
            }

            if (player.Vehicle.FetchVehicleData() == null)
            {
                NotificationExtension.SendErrorNotification(player, "You must be in an ownable vehicle.");
                return;
            }

            using Context context = new Context();

            Models.Vehicle vehicleDb = context.Vehicle.Find(player.Vehicle.GetClass().Id);

            if (vehicleDb == null)
            {
                NotificationExtension.SendErrorNotification(player, "You must be in an ownable vehicle.");
                return;
            }

            vehicleDb.Anchor = !vehicleDb.Anchor;

            string status = vehicleDb.Anchor ? "lowered the anchor." : "raised the anchor.";

            player.SendEmoteMessage($"has {status}.");

            context.SaveChanges();

            player.Vehicle.SetSyncedMetaData("VehicleAnchorStatus", vehicleDb.Anchor);
        }
    }
}