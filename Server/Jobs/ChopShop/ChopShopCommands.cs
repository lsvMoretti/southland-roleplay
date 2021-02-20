using System;
using System.Linq;
using System.Timers;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Vehicle;

namespace Server.Jobs.ChopShop
{
    public class ChopShopCommands
    {
        private static readonly string ToolboxItem = "ITEM_TOOLBOX";
        private static readonly string PlayerLastPartRemoved = "ChopShop:LastPartRemoveTime";
        private static readonly int MaxPartsFromVehicle = 5;
        private static readonly string ChopShopPart = "ITEM_CHOPSHOP_PART";
        private static readonly string ChopShopCommandInUse = "ChopShop:CommandInUse";
        private static readonly double ChopWaitTime = 60000;

        [Command("chop", commandType: CommandType.Vehicle, description: "Used to start chopping parts of a vehicle")]
        public static void ChopShopCommandChop(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (player.HasData(ChopShopCommandInUse))
            {
                player.SendErrorNotification("Please wait!");
                return;
            }

            var playerInventory = player.FetchInventory();

            if (playerInventory is null)
            {
                player.SendErrorNotification("Unable to fetch your inventory.");
                return;
            }

            if (!playerInventory.HasItem(ToolboxItem))
            {
                player.SendErrorNotification("You don't have the tools!");
                return;
            }

            var nearestVehicle = VehicleHandler.FetchNearestVehicle(player);

            if (nearestVehicle is null)
            {
                player.SendErrorNotification("You're not near a vehicle.");
                return;
            }

            if (nearestVehicle.Occupants().Any())
            {
                player.SendErrorNotification("There is someone in the vehicle!");
                return;
            }

            var vehicleData = nearestVehicle.FetchVehicleData();

            if (vehicleData is null)
            {
                player.SendErrorNotification("You can't do that on this vehicle");
                return;
            }

            if (vehicleData.FactionId is 1 || vehicleData.FactionId is 2)
            {
                // PD / EMS Vehicles

                player.SendErrorNotification("You can't do that on this vehicle");
                return;
            }

            using Context context = new Context();

            var vehicleDb = context.Vehicle.FirstOrDefault(x => x.Id == vehicleData.Id);

            if (vehicleDb is null)
            {
                player.SendErrorNotification("You can't do that on this vehicle");
                return;
            }

            if (vehicleData.RemovedParts >= MaxPartsFromVehicle)
            {
                vehicleDb.RespawnDelay = 59;
                context.SaveChanges();
                player.SendErrorNotification("You've removed the max parts from the vehicle. Vehicle is being despawned.");
                LoadVehicle.UnloadVehicle(nearestVehicle, true);
                return;
            }

            if (player.GetData(PlayerLastPartRemoved, out DateTime lastTime))
            {
                if (DateTime.Compare(lastTime.AddMinutes(5), DateTime.Now) > 0)
                {
                    player.SendErrorNotification("You must wait before reusing the command.");
                    return;
                }
            }

            var chopShopPart = new InventoryItem(ChopShopPart, "Vehicle Part", vehicleDb.Id.ToString());

            var frontBumperDamage = nearestVehicle.GetBumperDamageLevelExt(VehicleBumper.Front);
            if (frontBumperDamage != VehicleBumperDamage.None)
            {
                Timer timer = new Timer(ChopWaitTime)
                {
                    AutoReset = false
                };
                timer.Start();
                player.SetData(ChopShopCommandInUse, true);
                player.SendInfoNotification("Starting to remove the front bumper from the vehicle");
                timer.Elapsed += (sender, args) =>
                {
                    if (!player.Exists) return;
                    if (!nearestVehicle.Exists)
                    {
                        player.SendErrorNotification("Unable to find the vehicle");
                        return;
                    }

                    if (player.Position.Distance(nearestVehicle.Position) > 5)
                    {
                        player.SendErrorNotification("You've moved away from the vehicle.");
                        return;
                    }

                    if (!playerInventory.AddItem(chopShopPart))
                    {
                        player.SendErrorNotification("Your inventory is full!");
                        return;
                    }
                    Context removeContext = new Context();
                    var removeVehicleDb = removeContext.Vehicle.FirstOrDefault(x => x.Id == vehicleDb.Id);
                    if (removeVehicleDb is null)
                    {
                        player.SendErrorNotification("Unable to get the vehicle data.");
                        return;
                    }
                    removeContext.SaveChanges();
                    nearestVehicle.SetBumperDamageLevelExt(VehicleBumper.Front, VehicleBumperDamage.None);
                    removeVehicleDb.RemovedParts += 1;
                    Models.Vehicle.UpdateVehicle(nearestVehicle);
                    player.SendInfoNotification("You've removed the front bumper from the vehicle.");
                    player.DeleteData(ChopShopCommandInUse);
                };
                return;
            }

            var rearBumperDamage = nearestVehicle.GetBumperDamageLevelExt(VehicleBumper.Rear);
            if (rearBumperDamage != VehicleBumperDamage.None)
            {
                Timer timer = new Timer(ChopWaitTime)
                {
                    AutoReset = false
                };
                timer.Start();
                player.SetData(ChopShopCommandInUse, true);
                player.SendInfoNotification("Starting to remove the rear bumper from the vehicle");
                timer.Elapsed += (sender, args) =>
                {
                    if (!player.Exists) return;
                    if (!nearestVehicle.Exists)
                    {
                        player.SendErrorNotification("Unable to find the vehicle");
                        return;
                    }

                    if (player.Position.Distance(nearestVehicle.Position) > 5)
                    {
                        player.SendErrorNotification("You've moved away from the vehicle.");
                        return;
                    }

                    if (!playerInventory.AddItem(chopShopPart))
                    {
                        player.SendErrorNotification("Your inventory is full!");
                        return;
                    }
                    Context removeContext = new Context();
                    var removeVehicleDb = removeContext.Vehicle.FirstOrDefault(x => x.Id == vehicleDb.Id);
                    if (removeVehicleDb is null)
                    {
                        player.SendErrorNotification("Unable to get the vehicle data.");
                        return;
                    }
                    removeContext.SaveChanges();
                    nearestVehicle.SetBumperDamageLevelExt(VehicleBumper.Rear, VehicleBumperDamage.None);
                    Models.Vehicle.UpdateVehicle(nearestVehicle);
                    player.SendInfoNotification("You've removed the rear bumper from the vehicle.");
                    player.DeleteData(ChopShopCommandInUse);
                };
                return;
            }

            var frontLeftPartDamage = nearestVehicle.GetPartDamageLevelExt(VehiclePart.FrontLeft);
            if (frontLeftPartDamage != VehiclePartDamage.DamagedLevel3)
            {
                Timer timer = new Timer(ChopWaitTime)
                {
                    AutoReset = false
                };
                timer.Start();
                player.SetData(ChopShopCommandInUse, true);
                player.SendInfoNotification("Starting to remove the front left from the vehicle");
                timer.Elapsed += (sender, args) =>
                {
                    if (!player.Exists) return;
                    if (!nearestVehicle.Exists)
                    {
                        player.SendErrorNotification("Unable to find the vehicle");
                        return;
                    }

                    if (player.Position.Distance(nearestVehicle.Position) > 5)
                    {
                        player.SendErrorNotification("You've moved away from the vehicle.");
                        return;
                    }

                    if (!playerInventory.AddItem(chopShopPart))
                    {
                        player.SendErrorNotification("Your inventory is full!");
                        return;
                    }
                    Context removeContext = new Context();
                    var removeVehicleDb = removeContext.Vehicle.FirstOrDefault(x => x.Id == vehicleDb.Id);
                    if (removeVehicleDb is null)
                    {
                        player.SendErrorNotification("Unable to get the vehicle data.");
                        return;
                    }
                    removeContext.SaveChanges();
                    nearestVehicle.SetPartDamageLevelExt(VehiclePart.FrontLeft, VehiclePartDamage.DamagedLevel3);
                    Models.Vehicle.UpdateVehicle(nearestVehicle);
                    player.SendInfoNotification("You've removed the front left from the vehicle.");
                    player.DeleteData(ChopShopCommandInUse);
                };
                return;
            }

            var frontRightPartDamage = nearestVehicle.GetPartDamageLevelExt(VehiclePart.FrontRight);
            if (frontRightPartDamage != VehiclePartDamage.DamagedLevel3)
            {
                Timer timer = new Timer(ChopWaitTime)
                {
                    AutoReset = false
                };
                timer.Start();
                player.SetData(ChopShopCommandInUse, true);
                player.SendInfoNotification("Starting to remove the front right from the vehicle");
                timer.Elapsed += (sender, args) =>
                {
                    if (!player.Exists) return;
                    if (!nearestVehicle.Exists)
                    {
                        player.SendErrorNotification("Unable to find the vehicle");
                        return;
                    }

                    if (player.Position.Distance(nearestVehicle.Position) > 5)
                    {
                        player.SendErrorNotification("You've moved away from the vehicle.");
                        return;
                    }

                    if (!playerInventory.AddItem(chopShopPart))
                    {
                        player.SendErrorNotification("Your inventory is full!");
                        return;
                    }
                    Context removeContext = new Context();
                    var removeVehicleDb = removeContext.Vehicle.FirstOrDefault(x => x.Id == vehicleDb.Id);
                    if (removeVehicleDb is null)
                    {
                        player.SendErrorNotification("Unable to get the vehicle data.");
                        return;
                    }
                    removeContext.SaveChanges();
                    nearestVehicle.SetPartDamageLevelExt(VehiclePart.FrontRight, VehiclePartDamage.DamagedLevel3);
                    Models.Vehicle.UpdateVehicle(nearestVehicle);
                    player.SendInfoNotification("You've removed the front right from the vehicle.");
                    player.DeleteData(ChopShopCommandInUse);
                };
                return;
            }
            var rearLeftPartDamage = nearestVehicle.GetPartDamageLevelExt(VehiclePart.RearLeft);
            if (rearLeftPartDamage != VehiclePartDamage.DamagedLevel3)
            {
                Timer timer = new Timer(ChopWaitTime)
                {
                    AutoReset = false
                };
                timer.Start();
                player.SetData(ChopShopCommandInUse, true);
                player.SendInfoNotification("Starting to remove the rear left from the vehicle");
                timer.Elapsed += (sender, args) =>
                {
                    if (!player.Exists) return;
                    if (!nearestVehicle.Exists)
                    {
                        player.SendErrorNotification("Unable to find the vehicle");
                        return;
                    }

                    if (player.Position.Distance(nearestVehicle.Position) > 5)
                    {
                        player.SendErrorNotification("You've moved away from the vehicle.");
                        return;
                    }

                    if (!playerInventory.AddItem(chopShopPart))
                    {
                        player.SendErrorNotification("Your inventory is full!");
                        return;
                    }
                    Context removeContext = new Context();
                    var removeVehicleDb = removeContext.Vehicle.FirstOrDefault(x => x.Id == vehicleDb.Id);
                    if (removeVehicleDb is null)
                    {
                        player.SendErrorNotification("Unable to get the vehicle data.");
                        return;
                    }
                    removeContext.SaveChanges();
                    nearestVehicle.SetPartDamageLevelExt(VehiclePart.RearLeft, VehiclePartDamage.DamagedLevel3);
                    Models.Vehicle.UpdateVehicle(nearestVehicle);
                    player.SendInfoNotification("You've removed the rear left from the vehicle.");
                    player.DeleteData(ChopShopCommandInUse);
                };
                return;
            }
            var rearRightDamage = nearestVehicle.GetPartDamageLevelExt(VehiclePart.RearRight);
            if (rearRightDamage != VehiclePartDamage.DamagedLevel3)
            {
                Timer timer = new Timer(ChopWaitTime)
                {
                    AutoReset = false
                };
                timer.Start();
                player.SetData(ChopShopCommandInUse, true);
                player.SendInfoNotification("Starting to remove the rear right from the vehicle");
                timer.Elapsed += (sender, args) =>
                {
                    if (!player.Exists) return;
                    if (!nearestVehicle.Exists)
                    {
                        player.SendErrorNotification("Unable to find the vehicle");
                        return;
                    }

                    if (player.Position.Distance(nearestVehicle.Position) > 5)
                    {
                        player.SendErrorNotification("You've moved away from the vehicle.");
                        return;
                    }

                    if (!playerInventory.AddItem(chopShopPart))
                    {
                        player.SendErrorNotification("Your inventory is full!");
                        return;
                    }
                    Context removeContext = new Context();
                    var removeVehicleDb = removeContext.Vehicle.FirstOrDefault(x => x.Id == vehicleDb.Id);
                    if (removeVehicleDb is null)
                    {
                        player.SendErrorNotification("Unable to get the vehicle data.");
                        return;
                    }
                    removeContext.SaveChanges();
                    nearestVehicle.SetPartDamageLevelExt(VehiclePart.RearRight, VehiclePartDamage.DamagedLevel3);
                    Models.Vehicle.UpdateVehicle(nearestVehicle);
                    player.SendInfoNotification("You've removed the rear right from the vehicle.");
                    player.DeleteData(ChopShopCommandInUse);
                };
                return;
            }
            var middleLeftDamage = nearestVehicle.GetPartDamageLevelExt(VehiclePart.MiddleLeft);
            if (middleLeftDamage != VehiclePartDamage.DamagedLevel3)
            {
                Timer timer = new Timer(ChopWaitTime)
                {
                    AutoReset = false
                };
                timer.Start();
                player.SetData(ChopShopCommandInUse, true);
                player.SendInfoNotification("Starting to remove the middle left from the vehicle");
                timer.Elapsed += (sender, args) =>
                {
                    if (!player.Exists) return;
                    if (!nearestVehicle.Exists)
                    {
                        player.SendErrorNotification("Unable to find the vehicle");
                        return;
                    }

                    if (player.Position.Distance(nearestVehicle.Position) > 5)
                    {
                        player.SendErrorNotification("You've moved away from the vehicle.");
                        return;
                    }

                    if (!playerInventory.AddItem(chopShopPart))
                    {
                        player.SendErrorNotification("Your inventory is full!");
                        return;
                    }
                    Context removeContext = new Context();
                    var removeVehicleDb = removeContext.Vehicle.FirstOrDefault(x => x.Id == vehicleDb.Id);
                    if (removeVehicleDb is null)
                    {
                        player.SendErrorNotification("Unable to get the vehicle data.");
                        return;
                    }
                    removeContext.SaveChanges();
                    nearestVehicle.SetPartDamageLevelExt(VehiclePart.MiddleLeft, VehiclePartDamage.DamagedLevel3);
                    Models.Vehicle.UpdateVehicle(nearestVehicle);
                    player.SendInfoNotification("You've removed the middle left from the vehicle.");
                    player.DeleteData(ChopShopCommandInUse);
                };
                return;
            }
            var middleRightDamage = nearestVehicle.GetPartDamageLevelExt(VehiclePart.MiddleRight);
            if (middleRightDamage != VehiclePartDamage.DamagedLevel3)
            {
                Timer timer = new Timer(ChopWaitTime)
                {
                    AutoReset = false
                };
                timer.Start();
                player.SetData(ChopShopCommandInUse, true);
                player.SendInfoNotification("Starting to remove the middle right from the vehicle");
                timer.Elapsed += (sender, args) =>
                {
                    if (!player.Exists) return;
                    if (!nearestVehicle.Exists)
                    {
                        player.SendErrorNotification("Unable to find the vehicle");
                        return;
                    }

                    if (player.Position.Distance(nearestVehicle.Position) > 5)
                    {
                        player.SendErrorNotification("You've moved away from the vehicle.");
                        return;
                    }

                    if (!playerInventory.AddItem(chopShopPart))
                    {
                        player.SendErrorNotification("Your inventory is full!");
                        return;
                    }
                    Context removeContext = new Context();
                    var removeVehicleDb = removeContext.Vehicle.FirstOrDefault(x => x.Id == vehicleDb.Id);
                    if (removeVehicleDb is null)
                    {
                        player.SendErrorNotification("Unable to get the vehicle data.");
                        return;
                    }
                    removeContext.SaveChanges();
                    nearestVehicle.SetPartDamageLevelExt(VehiclePart.MiddleRight, VehiclePartDamage.DamagedLevel3);
                    Models.Vehicle.UpdateVehicle(nearestVehicle);
                    player.SendInfoNotification("You've removed the middle right from the vehicle.");
                    player.DeleteData(ChopShopCommandInUse);
                };
                return;
            }

            player.SendErrorNotification("Unable to remove any more parts from the vehicle!");
            return;
        }
    }
}