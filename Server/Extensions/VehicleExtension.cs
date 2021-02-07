using System;
using System.Collections.Generic;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Models;

namespace Server.Extensions
{
    public static class VehicleExtension
    {
        /// <summary>
        /// Sets the vehicle database Id
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="Id"></param>
        public static void SetVehicleId(this IVehicle vehicle, int Id)
        {
            vehicle.SetData("VEHICLEID", Id);
        }

        /// <summary>
        /// Returns the vehicle's database Id
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public static int GetVehicleId(this IVehicle vehicle)
        {
            vehicle.GetData("VEHICLEID", out int result);

            return result;
        }

        /// <summary>
        /// Returns the vehicle's database information
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public static Models.Vehicle? FetchVehicleData(this IVehicle vehicle)
        {
            vehicle.GetData("VEHICLEID", out int result);
            using Context context = new Context();
            return context.Vehicle.Find(result);
        }

        /// <summary>
        /// Set the vehicle’s distance in Meters
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="distance">Meters</param>
        [Obsolete("Use .GetClass instead", true)]
        public static void SetVehicleDistance(this IVehicle vehicle, float distance)
        {
            vehicle.SetSyncedMetaData("ODOREADING", distance);
        }

        /// <summary>
        /// Returns the vehicle distance in Meters
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        [Obsolete("Use .GetClass instead", true)]
        public static float GetVehicleDistance(this IVehicle vehicle)
        {
            vehicle.GetSyncedMetaData("ODOREADING", out float reading);

            return reading;
        }

        /// <summary>
        /// Returns a list of player IDs in the vehicle
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public static Dictionary<byte, int> Occupants(this IVehicle vehicle)
        {
            vehicle.GetData("OCCUPANTLIST", out string json);

            if (string.IsNullOrEmpty(json))
            {
                return new Dictionary<byte, int>();
            }

            return JsonConvert.DeserializeObject<Dictionary<byte, int>>(json);
        }

        public static VehicleEntity GetClass(this IVehicle vehicle)
        {
            try
            {
                if (vehicle.GetData("vehicle-class", out VehicleEntity data))
                {
                    return data;
                }

                var vehicleEntity = new VehicleEntity(vehicle);
                vehicle.SetData("vehicle-class", vehicleEntity);

                return vehicleEntity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static Inventory.Inventory FetchInventory(this IVehicle vehicle)
        {
            if (vehicle.FetchVehicleData() == null) return null;

            return vehicle.FetchVehicleData() == null ? null : new Inventory.Inventory(InventoryData.GetInventoryData(vehicle.FetchVehicleData().InventoryId));
        }

        //public static Position GetTrunkPosition(this IVehicle vehicle, float distance = 3f)
        //{
        //    Position trunkPosition = vehicle.Position;
        //    Rotation trunkRotation = vehicle.Rotation;

        //    trunkPosition.X += -distance * (float)Math.Sin(trunkRotation.Pitch * Math.PI / 180.0);
        //    trunkPosition.Y += -distance * (float)Math.Cos(trunkRotation.Roll * Math.PI / 180.0);

        //    Console.WriteLine($"Vehicle Pos: {vehicle.Position.ToString()}");
        //    Console.WriteLine($"Trunk Pos: {trunkPosition}");
        //    Console.WriteLine($"Distance: {vehicle.Position.Distance(trunkPosition)}");

        //    return trunkPosition;
        //}

        /// <summary>
        /// Will totally delete a vehicle
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns>True if successful</returns>
        public static bool Delete(this IVehicle vehicle)
        {
            try
            {
                Models.Vehicle vehicleData = FetchVehicleData(vehicle);

                if (vehicleData == null)
                {
                    vehicle.Remove();
                    return true;
                }

                using Context context = new Context();

                Models.Vehicle vehicleContext = context.Vehicle.Find(vehicleData.Id);

                context.Vehicle.Remove(vehicleContext);

                InventoryData vehicleInventory = context.Inventory.Find(vehicleData.InventoryId);

                if (vehicleInventory != null)
                {
                    context.Inventory.Remove(vehicleInventory);
                }

                context.SaveChanges();

                vehicle.Remove();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }

    public class VehicleEntity
    {
        private readonly IVehicle _vehicle;

        public VehicleEntity(IVehicle vehicle)
        {
            _vehicle = vehicle;
        }

        /// <summary>
        /// The Vehicle's distance
        /// </summary>
        public float Distance
        {
            get
            {
                _vehicle.GetSyncedMetaData("ODOREADING", out float reading);

                return reading;
            }
            set
            {
                _vehicle.SetSyncedMetaData("ODOREADING", value);
            }
        }

        /// <summary>
        /// The vehicle Database Id
        /// </summary>
        public int Id
        {
            get
            {
                _vehicle.GetData("VEHICLEID", out int result);

                return result;
            }
            set => _vehicle.SetData("VEHICLEID", value);
        }

        /// <summary>
        /// Occupants (Seat & Player Id)
        /// </summary>
        public Dictionary<byte, int> Occupants
        {
            get
            {
                _vehicle.GetData("OCCUPANTLIST", out string json);

                if (string.IsNullOrEmpty(json))
                {
                    return new Dictionary<byte, int>();
                }

                return JsonConvert.DeserializeObject<Dictionary<byte, int>>(json);
            }
        }

        public int FuelLevel
        {
            get
            {
                _vehicle.GetData("FUELLEVEL", out int fuelLevel);
                return fuelLevel;
            }
            set
            {
                _vehicle.SetData("FUELLEVEL", value);
                _vehicle.SetSyncedMetaData("FUELLEVEL", value);
            }
        }
    }
}