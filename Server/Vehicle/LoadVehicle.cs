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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Character;
using Server.Extensions;

namespace Server.Vehicle
{
    public class LoadVehicle
    {
        public static void ResetAllVehiclesSpawnStatus()
        {
            try
            {
                using Context context = new Context();
                var vehicles = context.Vehicle.Where(x => x.Spawned).ToList();

                foreach (Models.Vehicle vehicle in vehicles)
                {
                    vehicle.Spawned = false;

                    if (string.IsNullOrEmpty(vehicle.PartDamages))
                    {
                        vehicle.PartDamages = JsonConvert.SerializeObject(new List<byte>());
                    }

                    if (string.IsNullOrEmpty(vehicle.PartBulletHoles))
                    {
                        vehicle.PartBulletHoles = JsonConvert.SerializeObject(new List<byte>());
                    }
                }

                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static async Task LoadFactionVehicles()
        {
            Stopwatch sw = Stopwatch.StartNew();

            Console.WriteLine($"Loading Faction Vehicles");

            await using Context context = new Context();

            List<Models.Vehicle> factionVehicles = context.Vehicle.Where(x => x.FactionId != 0).ToList();

            foreach (Models.Vehicle factionVehicle in factionVehicles)
            {
                if (!string.IsNullOrEmpty(factionVehicle.GarageId)) continue;

                Position vehiclePosition = new Position(factionVehicle.PosX, factionVehicle.PosY, factionVehicle.PosZ);
                await LoadDatabaseVehicleAsync(factionVehicle, vehiclePosition, true);
            }

            sw.Stop();

            Console.WriteLine($"Loaded {factionVehicles.Count} Faction Vehicles. This took {sw.Elapsed}");
        }

        /// <summary>
        ///  Loads all characters vehicles
        /// </summary>
        public static async Task LoadCharacterVehicles()
        {
            Console.WriteLine($"Loading Character Vehicles");

            using Context context = new Context();

            List<Models.Character> characters = context.Character.ToList();

            int count = 0;

            foreach (Models.Character character in characters)
            {
                List<Models.Vehicle> vehicles = Models.Vehicle.FetchCharacterVehicles(character.Id);

                foreach (Models.Vehicle vehicle in vehicles)
                {
                    if (vehicle.Spawned) continue;
                    if (!string.IsNullOrEmpty(vehicle.GarageId)) continue;

                    Position vehiclePosition = new Position(vehicle.PosX, vehicle.PosY, vehicle.PosZ);

                    await LoadDatabaseVehicleAsync(vehicle, vehiclePosition);
                    count++;
                }
            }

            Console.WriteLine($"Loaded {count} character vehicles.");
        }

        public static async void CharacterLoaded(Models.Character playerCharacter)
        {
            List<Models.Vehicle> playerVehicles = Models.Vehicle.FetchCharacterVehicles(playerCharacter.Id)
                .Where(x => !x.Spawned).ToList();

            foreach (Models.Vehicle playerVehicle in playerVehicles)
            {
                if (playerVehicle.Impounded) continue;
                if (!string.IsNullOrEmpty(playerVehicle.GarageId)) continue;

                Position vehiclePosition = new Position(playerVehicle.PosX, playerVehicle.PosY, playerVehicle.PosZ);
                await LoadDatabaseVehicleAsync(playerVehicle, vehiclePosition);
            }
        }

        public static async Task<IVehicle> LoadDatabaseVehicleAsync(Models.Vehicle vehicleData, Position spawnPosition, bool ignoreDamage = false)
        {
            IVehicle? vehicle = null;

            bool modelParse = int.TryParse(vehicleData.Model, out int vModelResult);

            if (!modelParse)
            {
                vModelResult = (int)Alt.Hash(vehicleData.Model);
            }

            vehicle = Alt.Server.CreateVehicle((uint)vModelResult, spawnPosition, new Rotation(0, 0, vehicleData.RotZ));

            if (vehicle == null)
                return null;

            vehicle.Rotation = new DegreeRotation(0, 0, vehicleData.RotZ);

            if (vehicleData.Class != 13)
            {
                vehicle.ManualEngineControl = true;
            }

            vehicle.NumberplateText = !vehicleData.HasPlateBeenStolen ? vehicleData.Plate : "__";

            if (!string.IsNullOrWhiteSpace(vehicleData.StolenPlate))
            {
                vehicle.NumberplateText = vehicleData.StolenPlate;
            }

            if (vehicleData.FactionId == 0)
            {
                vehicle.EngineOn = vehicleData.FuelLevel > 0 && vehicleData.Engine;
            }
            else
            {
                vehicle.EngineOn = false;
            }

            if (vehicleData.Locked)
            {
                vehicle.LockState = VehicleLockState.Locked;
            }

            if (!vehicleData.Locked)
            {
                vehicle.LockState = VehicleLockState.Unlocked;
            }

            if (vehicleData.FactionId != 0)
            {
                vehicle.LockState = VehicleLockState.Locked;
                vehicleData.FuelLevel = 100;
            }

            if (vehicleData.Class == 13)
            {
                // Cycle
                vehicle.EngineOn = true;
                vehicle.ManualEngineControl = false;
            }

            if (!ignoreDamage)
            {
                if (!string.IsNullOrEmpty(vehicleData.DamageData))
                {
                    vehicle.DamageData = vehicleData.DamageData;
                }

                if (!string.IsNullOrEmpty(vehicleData.HealthData))
                {
                    vehicle.HealthData = vehicleData.HealthData;
                }

                if (!string.IsNullOrEmpty(vehicleData.AppearanceData))
                {
                    vehicle.AppearanceData = vehicleData.AppearanceData;
                }

                vehicle.BodyHealth = vehicleData.BodyHealth;
                vehicle.BodyAdditionalHealth = vehicleData.BodyAdditionalHealth;
                vehicle.DirtLevel = vehicleData.DirtLevel;

                if (!string.IsNullOrEmpty(vehicleData.PartDamages))
                {
                    List<byte> partDamages = JsonConvert.DeserializeObject<List<byte>>(vehicleData.PartDamages);

                    foreach (byte partDamage in partDamages)
                    {
                        int index = partDamages.IndexOf(partDamage);

                        vehicle.SetPartDamageLevel((byte)index, partDamage);
                    }
                }

                if (!string.IsNullOrEmpty(vehicleData.PartBulletHoles))
                {
                    List<byte> partHoles = JsonConvert.DeserializeObject<List<byte>>(vehicleData.PartBulletHoles);

                    foreach (byte partDamage in partHoles)
                    {
                        int index = partHoles.IndexOf(partDamage);

                        vehicle.SetPartBulletHoles((byte)index, partDamage);
                    }
                }
            }

            vehicle.SetVehicleId(vehicleData.Id);

            //vehicle.SetSyncedMetaData("FUELLEVEL", vehicleData.FuelLevel);

            vehicle.GetClass().FuelLevel = vehicleData.FuelLevel;

            vehicle.GetClass().Distance = (float)Decimal.Round((Decimal)vehicleData.Odometer, 2);

            LoadVehicleMods(vehicle);

            vehicle.SetSyncedMetaData("VehicleAnchorStatus", vehicleData.Anchor);

            await using Context context = new Context();

            Models.Vehicle vehicleDb = await context.Vehicle.FindAsync(vehicleData.Id);

            if (vehicleDb == null) return null;

            vehicleDb.Spawned = true;

            if (!string.IsNullOrEmpty(vehicleDb.GarageId))
            {
                vehicleDb.GarageId = string.Empty;
            }

            await context.SaveChangesAsync();

            return vehicle;
        }

        public static IVehicle LoadDatabaseVehicle(Models.Vehicle vehicleData, Position spawnPosition, bool ignoreDamage = false)
        {
            IVehicle vehicle = null;

            bool modelParse = int.TryParse(vehicleData.Model, out int vModelResult);

            if (!modelParse)
            {
                vModelResult = (int)Alt.Hash(vehicleData.Model);
            }

            vehicle = Alt.Server.CreateVehicle((uint)vModelResult, spawnPosition,
                new DegreeRotation(0, 0, vehicleData.RotZ));

            if (vehicle == null)
            {
                return null;
            }

            //vehicle.Rotation = new DegreeRotation(0, 0, vehicleData.RotZ);

            if (vehicleData.Class != 13)
            {
                vehicle.ManualEngineControl = true;
            }

            vehicle.NumberplateText = !vehicleData.HasPlateBeenStolen ? vehicleData.Plate : "__";

            if (!string.IsNullOrWhiteSpace(vehicleData.StolenPlate))
            {
                vehicle.NumberplateText = vehicleData.StolenPlate;
            }

            if (vehicleData.FactionId == 0)
            {
                vehicle.EngineOn = vehicleData.FuelLevel > 0 && vehicleData.Engine;
            }
            else
            {
                vehicle.EngineOn = false;
            }

            if (vehicleData.Locked)
            {
                vehicle.LockState = VehicleLockState.Locked;
            }

            if (!vehicleData.Locked)
            {
                vehicle.LockState = VehicleLockState.Unlocked;
            }

            if (vehicleData.FactionId != 0)
            {
                vehicle.LockState = VehicleLockState.Locked;
                vehicle.GetClass().FuelLevel = 100;
            }

            if (vehicleData.Class == 13)
            {
                // Cycle
                vehicle.EngineOn = true;
                vehicle.ManualEngineControl = false;
            }

            if (!ignoreDamage)
            {
                if (!string.IsNullOrEmpty(vehicleData.DamageData))
                {
                    vehicle.DamageData = vehicleData.DamageData;
                }

                if (!string.IsNullOrEmpty(vehicleData.HealthData))
                {
                    vehicle.HealthData = vehicleData.HealthData;
                }

                if (!string.IsNullOrEmpty(vehicleData.AppearanceData))
                {
                    vehicle.AppearanceData = vehicleData.AppearanceData;
                }

                vehicle.BodyHealth = vehicleData.BodyHealth;
                vehicle.BodyAdditionalHealth = vehicleData.BodyAdditionalHealth;
                vehicle.DirtLevel = vehicleData.DirtLevel;

                if (!string.IsNullOrEmpty(vehicleData.PartDamages))
                {
                    List<byte> partDamages = JsonConvert.DeserializeObject<List<byte>>(vehicleData.PartDamages);

                    foreach (byte partDamage in partDamages)
                    {
                        int index = partDamages.IndexOf(partDamage);

                        vehicle.SetPartDamageLevel((byte)index, partDamage);
                    }
                }

                if (!string.IsNullOrEmpty(vehicleData.PartBulletHoles))
                {
                    List<byte> partHoles = JsonConvert.DeserializeObject<List<byte>>(vehicleData.PartBulletHoles);

                    foreach (byte partDamage in partHoles)
                    {
                        int index = partHoles.IndexOf(partDamage);

                        vehicle.SetPartBulletHoles((byte)index, partDamage);
                    }
                }
            }

            vehicle.SetVehicleId(vehicleData.Id);

            //vehicle.SetSyncedMetaData("FUELLEVEL", vehicleData.FuelLevel);

            vehicle.GetClass().FuelLevel = vehicleData.FuelLevel;

            vehicle.GetClass().Distance = (float)Decimal.Round((Decimal)vehicleData.Odometer, 2);

            LoadVehicleMods(vehicle);

            vehicle.SetSyncedMetaData("VehicleAnchorStatus", vehicleData.Anchor);

            using Context context = new Context();

            Models.Vehicle vehicleDb = context.Vehicle.Find(vehicleData.Id);

            if (vehicleDb == null) return null;

            vehicleDb.Spawned = true;

            if (!string.IsNullOrEmpty(vehicleDb.GarageId))
            {
                vehicleDb.GarageId = string.Empty;
            }

            context.SaveChanges();

            return vehicle;
        }

        public static void LoadVehicleMods(IVehicle vehicle)
        {
            Models.Vehicle vehicleData = vehicle.FetchVehicleData();

            if (vehicleData == null) return;

            vehicle.ModKit = 1;

            for (byte i = 0; i < 75; i++)
            {
                vehicle.SetMod(i, 0);
            }

            Dictionary<int, int> modList = JsonConvert.DeserializeObject<Dictionary<int, int>>(vehicleData.VehicleMods);

            if (vehicleData.FrontWheelType != -1)
            {
                vehicle.SetWheels((byte)vehicleData.FrontWheelType, (byte)vehicleData.FrontWheel);
            }

            foreach (KeyValuePair<int, int> mod in modList)
            {
                if (mod.Key == 69)
                {
                    vehicle.WindowTint = Convert.ToByte(mod.Value);
                    continue;
                }

                // Front wheels
                if (mod.Key == (int)VehicleModType.FrontWheels) continue;

                vehicle.SetMod(Convert.ToByte(mod.Key), Convert.ToByte(mod.Value));
            }

            byte[] color1 = vehicleData.Color1.Split(',').Select(byte.Parse).ToArray();
            byte[] color2 = vehicleData.Color2.Split(',').Select(byte.Parse).ToArray();

            vehicle.PrimaryColorRgb = new Rgba(color1[0], color1[1], color1[2], 255);
            vehicle.SecondaryColorRgb = new Rgba(color2[0], color2[1], color2[2], 255);
            vehicle.PearlColor = 0;

            vehicle.Livery = (byte)vehicleData.Livery;

            vehicle.WheelColor = (byte)vehicleData.WheelColor;
        }

        public static void UnloadVehicle(IVehicle vehicle, bool updatePosition = false)
        {
            if (vehicle.FetchVehicleData() == null) return;

            using Context context = new Context();

            Models.Vehicle vehicleData = context.Vehicle.Find(vehicle.GetClass().Id);

            if (vehicleData == null) return;

            vehicleData.Spawned = false;

            if (updatePosition)
            {
                vehicleData.PosX = vehicle.Position.X;
                vehicleData.PosY = vehicle.Position.Y;
                vehicleData.PosZ = vehicle.Position.Z;
                vehicleData.RotZ = vehicle.Rotation.Yaw;
            }

            context.SaveChanges();

            vehicle.Remove();
        }

        public static void StartCharacterVehicleDespawnTimer()
        {
            Timer timer = new Timer(900000)
            {
                AutoReset = true
            };
            timer.Start();

            timer.Elapsed += (sender, args) =>
            {
                ICollection<IVehicle> vehicles = Alt.GetAllVehicles();

                int startCount = vehicles.Count;
                int removeCount = 0;

                foreach (IVehicle vehicle in vehicles)
                {
                    lock (vehicle)
                    {
                        if (!vehicle.Exists) continue;
                        var vehicleData = vehicle.FetchVehicleData();
                        if (vehicleData is null) continue;

                        if (vehicle.Driver != null) continue;

                        if (vehicleData.FactionId > 0) continue;

                        var vehicleCharacter = Models.Character.GetCharacter(vehicleData.OwnerId);

                        if (vehicleCharacter is null) continue;

                        var characterAccount = Models.Account.FindAccountById(vehicleCharacter.OwnerId);

                        if (characterAccount is null) continue;

                        if (characterAccount.IsOnline) continue;

                        if (DateTime.Compare(vehicleCharacter.LastTimeCheck.AddMinutes(5), DateTime.Now) > 0)
                        {
                            UnloadVehicle(vehicle, true);

                            removeCount += 1;
                        }
                    }
                }

                Console.WriteLine($"Cleared un-used character vehicles. {removeCount}/{startCount} removed.");
            };
        }
    }
}