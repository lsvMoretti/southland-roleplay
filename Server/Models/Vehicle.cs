using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Extensions;

namespace Server.Models
{
    public class Vehicle
    {
        /// <summary>
        /// Gets vehicle information. Dimension, colour etc.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Vehicle model
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Vehicle name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The vehicle class
        /// </summary>
        public int Class { get; set; }

        /// <summary>
        /// vehicle plate
        /// </summary>
        public string? Plate { get; set; }

        /// <summary>
        /// Vehicle position X
        /// </summary>
        public float PosX { get; set; }

        /// <summary>
        /// Vehicle Position Y
        /// </summary>
        public float PosY { get; set; }

        /// <summary>
        /// Vehicle Position Z
        /// </summary>
        public float PosZ { get; set; }

        /// <summary>
        /// Vehicle Rotation Z
        /// </summary>
        public float RotZ { get; set; }

        /// <summary>
        /// Vehicle dimension
        /// </summary>
        public uint Dimension { get; set; }

        /// <summary>
        /// Vehicle health
        /// </summary>
        public float Health { get; set; }

        /// <summary>
        /// Vehicle owner
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Lock Status
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// Engine Status
        /// </summary>
        public bool Engine { get; set; }

        /// <summary>
        /// Keys
        /// </summary>
        public string? Keycode { get; set; }

        /// <summary>
        /// Is the vehicle spawned?
        /// </summary>
        public bool Spawned { get; set; }

        /// <summary>
        /// Garage ID of parked vehicle
        /// </summary>
        public string? GarageId { get; set; }

        /// <summary>
        /// List of Serialized Mods
        /// </summary>
        public string? VehicleMods { get; set; }

        /// <summary>
        /// Custom Color 1
        /// </summary>
        public string? Color1 { get; set; }

        /// <summary>
        /// Custom Color 2
        /// </summary>
        public string? Color2 { get; set; }

        /// <summary>
        /// Vehicle Front Wheel Type
        /// </summary>
        public int FrontWheelType { get; set; }

        /// <summary>
        /// Vehicle Front Wheel
        /// </summary>
        public int FrontWheel { get; set; }

        /// <summary>
        /// FuelLevel
        /// </summary>
        public int FuelLevel { get; set; }

        /// <summary>
        /// Mile-O-Meter
        /// </summary>
        public float Odometer { get; set; }

        /// <summary>
        /// Inventory ID for the vehicle
        /// </summary>
        public int InventoryId { get; set; }

        /// <summary>
        /// Vehicle set to faction
        /// </summary>
        public int FactionId { get; set; }

        /// <summary>
        /// Vehicle Livery
        /// </summary>
        public int Livery { get; set; }

        /// <summary>
        /// Vehicle Price
        /// </summary>
        public int VehiclePrice { get; set; }

        /// <summary>
        /// Stored Vehicles Json (List<int>)
        /// </summary>
        public string? StoredVehicles { get; set; }

        /// <summary>
        /// Is the vehicle stored
        /// </summary>
        public bool IsStored { get; set; }

        /// <summary>
        /// Has purchased an XMR radio
        /// </summary>
        public bool DigitalRadio { get; set; }

        /// <summary>
        /// The wheel color
        /// </summary>
        public int WheelColor { get; set; }

        /// <summary>
        /// Is Vehicle Impounded
        /// </summary>
        public bool Impounded { get; set; }

        /// <summary>
        /// Description of vehicle
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Sale Price of a Vehicle. 0 = not for sale
        /// </summary>
        public int SalePrice { get; set; }

        /// <summary>
        /// Amount of Owners the vehicle has had
        /// </summary>
        public int OwnerCount { get; set; }

        /// <summary>
        /// Vehicles Damage Data
        /// </summary>
        public string? DamageData { get; set; }

        /// <summary>
        /// Vehicles Health Data
        /// </summary>
        public string? HealthData { get; set; }

        public string? AppearanceData { get; set; }

        public uint BodyHealth { get; set; }

        public uint BodyAdditionalHealth { get; set; }

        public byte DirtLevel { get; set; }

        /// <summary>
        /// List of Byte
        /// </summary>
        public string? PartDamages { get; set; }

        /// <summary>
        /// List of byte
        /// </summary>
        public string? PartBulletHoles { get; set; }

        /// <summary>
        /// Status of the Anchor
        /// </summary>
        public bool Anchor { get; set; }

        public bool HasPlateBeenStolen { get; set; }

        public string? StolenPlate { get; set; }

        /// <summary>
        /// Find vehicle
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Models.Vehicle FetchVehicle(int id)
        {
            using Context context = new Context();
            return context.Vehicle.Find(id);
        }

        /// <summary>
        /// Adds a Vehicle to the DataBase
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public static int AddVehicle(Models.Vehicle vehicle)
        {
            using Context context = new Context();
            context.Vehicle.Add(vehicle);
            context.SaveChanges();

            return vehicle.Id;
        }

        /// <summary>
        /// Find plate
        /// </summary>
        /// <param name="plate"></param>
        /// <returns></returns>
        public static Models.Vehicle FetchVehicle(string? plate)
        {
            using Context context = new Context();
            return context.Vehicle.FirstOrDefault(i => i.Plate == plate);
        }

        /// <summary>
        /// Find owner
        /// </summary>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        public static List<Vehicle> FetchCharacterVehicles(int ownerId)
        {
            using Context context = new Context();
            return context.Vehicle.Where(i => i.OwnerId == ownerId).ToList();
        }

        public static List<Vehicle> FetchPropertyGarageVehicles(string? garageId)
        {
            using Context context = new Context();
            return context.Vehicle.Where(i => i.GarageId == garageId).ToList();
        }

        public static async void UpdateVehicle(IVehicle vehicle, bool despawn = false, string? garage = null, bool updatePos = true)
        {
            if (vehicle.FetchVehicleData() == null) return;

            using (Context context = new Context())
            {
                Vehicle veh = context.Vehicle.FirstOrDefault(i => i.Id == vehicle.FetchVehicleData().Id);

                if (veh == null) return;

                if (vehicle.LockState == VehicleLockState.LockedCanBeDamaged ||
                    vehicle.LockState == VehicleLockState.Locked)
                {
                    veh.Locked = true;
                }
                else if (!veh.Locked)
                {
                    vehicle.LockState = VehicleLockState.Unlocked;
                }

                veh.DamageData = vehicle.DamageData;
                veh.HealthData = vehicle.HealthData;
                veh.AppearanceData = vehicle.AppearanceData;
                veh.BodyHealth = vehicle.BodyHealth;
                veh.BodyAdditionalHealth = vehicle.BodyAdditionalHealth;
                veh.DirtLevel = vehicle.DirtLevel;

                List<byte> newDamages = new List<byte>();

                for (byte i = 0; i < 6; i++)
                {
                    newDamages.Add(vehicle.GetPartDamageLevel(i));
                }

                veh.PartDamages = JsonConvert.SerializeObject(newDamages);

                List<byte> bulletHoles = new List<byte>();

                for (byte i = 0; i < 6; i++)
                {
                    bulletHoles.Add(vehicle.GetPartBulletHoles(i));
                }

                veh.PartBulletHoles = JsonConvert.SerializeObject(bulletHoles);

                if (updatePos)
                {
                    DegreeRotation rotation = vehicle.Rotation;

                    veh.PosX = vehicle.Position.X;
                    veh.PosY = vehicle.Position.Y;
                    veh.PosZ = vehicle.Position.Z;
                    veh.RotZ = rotation.Yaw;
                    veh.Dimension = (uint)vehicle.Dimension;
                }

                if (despawn)
                {
                    veh.Spawned = false;
                }

                if (garage != null)
                {
                    veh.GarageId = garage;
                }

                veh.FuelLevel = vehicle.GetClass().FuelLevel;

                veh.Odometer = vehicle.GetClass().Distance;

                context.SaveChanges();
            }

            if (despawn)
            {
                await AltAsync.Do(() => { Alt.RemoveVehicle(vehicle); });
            }
        }
    }

    public class VehicleMods
    {
        public int slot { get; set; }
        public int modType { get; set; }
    }
}