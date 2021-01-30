using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;

namespace Server.Vehicle
{
    public class VehicleHandler
    {
        private static Timer oneSecondTimer = null;
        private static Timer oneMinuteTimer = null;
        private static Timer fuelTimer = null;

        public static void StartFiveSecondTimer()
        {
            if (oneSecondTimer != null)
            {
                oneSecondTimer.Stop();
            }

            if (oneMinuteTimer != null)
            {
                oneMinuteTimer.Stop();
            }

            if (fuelTimer != null)
            {
                fuelTimer.Stop();
            }

            oneSecondTimer = new Timer(5000) { AutoReset = true };

            oneSecondTimer.Elapsed += OneSecondTimerOnElapsed;
            oneSecondTimer.Start();

            oneMinuteTimer = new Timer(6000) { AutoReset = true };

            oneMinuteTimer.Elapsed += OneMinuteTimerOnElapsed;
            oneMinuteTimer.Start();

            fuelTimer = new Timer(132000) { AutoReset = true };
            fuelTimer.Elapsed += FuelTimerOnElapsed;
            fuelTimer.Start();
        }

        private static void FuelTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            fuelTimer.Stop();

            List<IVehicle> vehicleList = Alt.Server.GetVehicles().Where(x => x.FetchVehicleData() != null).ToList();

            if (!vehicleList.Any())
            {
                fuelTimer.Start();
                return;
            }

            foreach (IVehicle vehicle in vehicleList)
            {
                using Context context = new Context();

                Models.Vehicle vehicleData = context.Vehicle.Find(vehicle.GetClass().Id);

                if (vehicleData.Class == 13) continue;

                if (vehicleData.Engine)
                {
                    // Engine On
                    int currentFuel = vehicle.GetClass().FuelLevel;

                    if (currentFuel <= 0)
                    {
                        vehicleData.Engine = false;
                        vehicle.EngineOn = false;
                        if (vehicle.GetClass().Occupants.Any())
                        {
                            var driverInfo = vehicle.GetClass().Occupants.FirstOrDefault(x => x.Key == 1);

                            IPlayer driver = Alt.GetAllPlayers()
                                .FirstOrDefault(x => x.GetPlayerId() == driverInfo.Value);

                            driver?.Emit("Vehicle:SetEngineStatus", vehicle, vehicle.EngineOn, true);
                        }
                    }

                    int newFuel = currentFuel - 1;
                    vehicle.GetClass().FuelLevel = newFuel;

                    if (newFuel <= 0)
                    {
                        // No fuel!
                        vehicleData.Engine = false;
                        vehicle.EngineOn = false;
                    }

                    vehicleData.FuelLevel = newFuel;

                    context.SaveChanges();
                }
            }

            fuelTimer.Start();
        }

        private static async void OneMinuteTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                oneMinuteTimer.Stop();

                List<IVehicle> vehicleList = Alt.Server.GetVehicles().Where(x => x.FetchVehicleData() != null).ToList();

                if (!vehicleList.Any())
                {
                    oneMinuteTimer.Start();
                    return;
                }

                using Context context = new Context();
                lock (vehicleList)
                {
                    foreach (IVehicle vehicle in vehicleList)
                    {
                        if (!vehicle.Exists) continue;
                        Models.Vehicle vehicleData = context.Vehicle.Find(vehicle.GetClass().Id);

                        //vehicleData.Odometer = vehicle.GetVehicleDistance();

                        if (vehicle.LockState == VehicleLockState.Locked)
                        {
                            vehicleData.Locked = true;
                        }

                        if (vehicle.LockState == VehicleLockState.Unlocked)
                        {
                            vehicleData.Locked = false;
                        }

                        if (vehicleData.FactionId != 0)
                        {
                            Models.Vehicle.UpdateVehicle(vehicle, false, null, false);
                        }
                        else
                        {
                            Models.Vehicle.UpdateVehicle(vehicle);
                        }
                    }
                }
                context.SaveChanges();
                oneMinuteTimer.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return;
            }
        }

        private static void OneSecondTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            oneSecondTimer.Stop();

            List<IVehicle> vehicleList = Alt.Server.GetVehicles().Where(x => x.FetchVehicleData() != null).ToList();

            if (!vehicleList.Any())
            {
                oneSecondTimer.Start();
                return;
            }

            foreach (IVehicle vehicle in vehicleList)
            {
                Position currentPosition = vehicle.Position;

                if (vehicle.GetData("LASTVEHICLEPOSITION", out Position vehiclePosition))
                {
                    if (currentPosition == vehiclePosition) continue;

                    float currentDistance = vehicle.GetClass().Distance;

                    float distance = currentPosition.Distance(vehiclePosition);

                    float newDistance = currentDistance + distance;

                    vehicle.GetClass().Distance = (float)decimal.Round((decimal)newDistance, 2);

                    vehicle.SetData("LASTVEHICLEPOSITION", currentPosition);

                    continue;
                }

                vehicle.SetData("LASTVEHICLEPOSITION", currentPosition);
            }
            oneSecondTimer.Start();
        }

        public static void AltOnOnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            try
            {
                player.Emit("DisableSeatSwitch", vehicle);

                if (vehicle.Driver == player)
                {
                    Models.Vehicle? data = vehicle.FetchVehicleData();

                    if (data != null)
                    {
                        if (data.FactionId == 1)
                        {
                            if (!player.IsLeo(true))
                            {
                                player.SendErrorNotification("You can't enter this vehicle.");
                                player.Emit("Vehicle:RemoveFromVehicle");
                                return;
                            }
                        }
                    }
                }

                Dictionary<byte, int> occupants = vehicle.Occupants();

                if (occupants.ContainsKey(seat))
                {
                    occupants.Remove(seat);
                }

                player.GetClass().LastVehicle = vehicle;

                occupants.Add(seat, player.GetPlayerId());

                vehicle.SetData("OCCUPANTLIST", JsonConvert.SerializeObject(occupants, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                vehicle.GetData("CURRENTMUSICSTREAM", out string musicUrl);

                if (!string.IsNullOrEmpty(musicUrl))
                {
                    player.PlayMusicFromUrl(musicUrl);
                }

                if (vehicle.GetClass().Id < 0)
                {
                    vehicle.EngineOn = true;
                }

                var vehicleData = vehicle.FetchVehicleData();

                if (vehicleData != null)
                {
                    if (vehicleData.Engine)
                    {
                        vehicle.EngineOn = true;
                        vehicle.EngineOn = true;
                        vehicle.EngineOn = true;
                    }

                    if (!vehicleData.Engine)
                    {
                        vehicle.EngineOn = false;
                        vehicle.EngineOn = false;
                        vehicle.EngineOn = false;
                        vehicle.EngineOn = false;
                    }

                    int vehicleClass = vehicleData.Class;

                    if (vehicleClass == -1 && seat == 1 || vehicleClass == 0 && seat == 1)
                    {
                        // Class not retrieved & in driver
                        player.Emit("FetchVehicleClass", vehicle);
                    }

                    if (vehicleClass >= 10 && vehicleClass <= 12 && seat == 1 || vehicleClass == 20 && seat == 1)
                    {
                        // Industrial, Utility, Vans, Commercial & driver

                        using Context context = new Context();

                        var inventoryData = context.Inventory.Find(vehicleData.InventoryId);

                        if (inventoryData == null) return;

                        if (inventoryData.InventorySpace != 40)
                        {
                            inventoryData.InventorySpace = 40;
                        }

                        context.SaveChanges();
                    }

                    if (vehicleClass == 13 && seat == 0)
                    {
                        // Cycles
                        vehicle.EngineOn = true;

                        using Context context = new Context();

                        var inventoryData = context.Inventory.Find(vehicleData.InventoryId);

                        if (inventoryData == null) return;

                        if (inventoryData.InventorySpace != 0)
                        {
                            inventoryData.InventorySpace = 0;

                            context.SaveChanges();
                        }
                    }

                    if (vehicleClass == 8 && seat == 0)
                    {
                        using Context context = new Context();

                        var inventoryData = context.Inventory.Find(vehicleData.InventoryId);

                        if (inventoryData == null) return;

                        if (inventoryData.InventorySpace != 5)
                        {
                            inventoryData.InventorySpace = 5;

                            context.SaveChanges();
                        }
                    }

                    if (vehicleData.SalePrice > 0)
                    {
                        double vehicleMileage = vehicleData.Odometer / 1609;
                        Dictionary<int, int> vehicleMods =
                            JsonConvert.DeserializeObject<Dictionary<int, int>>(vehicleData.VehicleMods);

                        player.SendInfoNotification($"This {vehicleData.Name} is for sale for {vehicleData.SalePrice:C}.");
                        player.SendInfoNotification($"Vehicle Mileage: {vehicleMileage:F1} miles, Fuel Level: {vehicleData.FuelLevel:##'%'}. Previous Owners: {vehicleData.OwnerCount}, Mod Count: {vehicleMods.Count}.");

                        player.SendInfoNotification($"You can purchase this with /buyvehicle");
                    }
                }

                player.Emit("Vehicle:SetEngineStatus", vehicle, vehicle.EngineOn, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static void OnVehicleClassReturn(IPlayer player, int classId)
        {
            if (player.Vehicle.FetchVehicleData() == null) return;

            using Context context = new Context();

            var vehicleDb = context.Vehicle.Find(player.Vehicle.GetClass().Id);

            if (vehicleDb == null) return;

            vehicleDb.Class = classId;

            context.SaveChanges();
        }

        public static void AltOnOnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            if (player.HasData("Hotwire:Vehicle"))
            {
                player.DeleteSyncedMetaData("Hotwire:Decrypted");
                player.DeleteSyncedMetaData("Hotwire:Shuffled");
                player.DeleteData("Hotwire:Vehicle");
                player.Emit("VehicleScramble:ClosePage");
                player.SendErrorNotification("You've left the vehicle whilst hot wiring.");
            }

            Dictionary<byte, int> occupants = vehicle.Occupants();

            if (occupants.ContainsKey(seat))
            {
                occupants.Remove(seat);

                vehicle.SetData("OCCUPANTLIST", JsonConvert.SerializeObject(occupants, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }

            if (occupants.ContainsValue(player.GetPlayerId()))
            {
                occupants.Remove(occupants.FirstOrDefault(x => x.Value == player.GetPlayerId()).Key);

                vehicle.SetData("OCCUPANTLIST", JsonConvert.SerializeObject(occupants, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }

            vehicle.GetData("CURRENTMUSICSTREAM", out string musicUrl);

            if (!string.IsNullOrEmpty(musicUrl))
            {
                player.StopMusic();
            }

            bool hasTaxiDriverData = player.GetData("taxi:driverId", out int taxiDriverId);
            bool hasTaxiFareData = player.GetData("taxi:CurrentCost", out double currentCost);

            player.Emit("Vehicle:SetEngineStatus", vehicle, vehicle.EngineOn, true);

            if (hasTaxiDriverData && taxiDriverId > 0 && hasTaxiFareData)
            {
                player.SendInfoNotification($"You owe the Cab Driver {currentCost:C}.");
                IPlayer cabDriver = Alt.GetAllPlayers().FirstOrDefault(x => x.GetPlayerId() == taxiDriverId);

                if (cabDriver != null)
                {
                    cabDriver.SetData("taxi:payingCustomer", 0);
                    cabDriver.SendInfoNotification($"The passenger owes you {currentCost:C}.");
                }

                player.SetData("taxi:driverId", 0);
                player.SetData("taxi:CurrentCost", 0);
                return;
            }
        }

        /// <summary>
        /// Sets the Vehicle Door to open if Open = True
        /// </summary>
        /// <param name="player"></param>
        /// <param name="vehicle"></param>
        /// <param name="door"></param>
        /// <param name="open"></param>
        public static void SetVehicleDoorState(IPlayer player, IVehicle vehicle, int door, bool open)
        {
            player.Emit("setDoorState", vehicle, door, open);
        }

        public static void SetVehicleWindowState(IPlayer player, IVehicle vehicle, int window, bool open)
        {
            player.Emit("setWindowState", vehicle, window, open);
        }

        public static IVehicle? FetchNearestVehicle(IPlayer player, float radius = 5f)
        {
            try
            {
                IEnumerable<IVehicle> vehicles = Alt.Server.GetVehicles().Where(x => x.Dimension == player.Dimension);

                float lastDistance = radius + 1;

                IVehicle nearestVehicle = null;

                foreach (IVehicle vehicle in vehicles)
                {
                    Position vehiclePosition = Position.Zero;

                    lock (vehicle)
                    {
                        if (vehicle.Exists)
                        {
                            vehiclePosition = vehicle.Position;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    Position playerPosition = Position.Zero;

                    lock (player)
                    {
                        if (player.Exists)
                        {
                            playerPosition = player.Position;
                        }
                        else
                        {
                            return null;
                        }
                    }

                    float currentDistance = vehiclePosition.Distance(playerPosition);

                    if (currentDistance < radius && currentDistance < lastDistance)
                    {
                        lastDistance = currentDistance;
                        nearestVehicle = vehicle;
                    }
                }

                return nearestVehicle;

                //return !NearVehicles.Any() ? null : NearVehicles.OrderBy(x => x.Position.Distance(player.Position)).FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static bool HandleDriverEnterExit(IPlayer player)
        {
            if (player.IsInVehicle)
            {
                player.Emit("ExitVehicle", player.Vehicle);
                return true;
            }

            IVehicle vehicle = FetchNearestVehicle(player, 3);

            if (vehicle == null) return false;

            player.Emit("EnterVehicle", vehicle, true);

            return true;
        }

        public static bool HandPassengerEnterExit(IPlayer player)
        {
            if (player.IsInVehicle)
            {
                player.Emit("ExitVehicle", player.Vehicle);
                return true;
            }

            IVehicle vehicle = FetchNearestVehicle(player, 3);

            if (vehicle == null) return false;

            player.Emit("EnterVehicle", vehicle, false);

            return true;
        }

        public static void OnIndicatorStateChange(IPlayer player, int indicator, bool state)
        {
            if (indicator == 0)
            {
                // Left
                player.Vehicle.SetSyncedMetaData("LeftIndicatorState", state);
            }
            else
            {
                player.Vehicle.SetSyncedMetaData("RightIndicatorState", state);
            }
        }

        public static void ToggleVehicleExtra(IPlayer player, IVehicle vehicle, int intSlot, bool state)
        {
            byte slot = (byte)intSlot;

            //vehicle?.ToggleExtra(slot, state);

#if DEBUG

            Console.WriteLine($"Vehicle Extra: {intSlot} set to {state}");
#endif

            Alt.EmitAllClients("SetVehicleExtra", vehicle, intSlot, state);
        }

        public static void SetBombBayDoorState(IPlayer player, IVehicle vehicle, bool state)
        {
#if DEBUG

            Console.WriteLine($"Bomby Door State Set to: {state}");
#endif
            Alt.EmitAllClients("SetBombBayDoorState", vehicle, state);
        }
    }
}