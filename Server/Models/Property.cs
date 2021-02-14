using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace Server.Models
{
    public class Property
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The type of Property
        /// </summary>
        public PropertyType PropertyType { get; set; }

        /// <summary>
        /// The name of the business
        /// </summary>
        public string? BusinessName { get; set; }

        /// <summary>
        /// The address of the Property
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// The Position X of the Property
        /// </summary>
        public float PosX { get; set; }

        /// <summary>
        /// The Position Y of the Property
        /// </summary>
        public float PosY { get; set; }

        /// <summary>
        /// The Position Z if the Property
        /// </summary>
        public float PosZ { get; set; }

        /// <summary>
        /// The name of the interior used for the property
        /// </summary>
        public string? InteriorName { get; set; }

        /// <summary>
        /// Additional Interaction points for the property
        /// </summary>
        public string? InteractionPoints { get; set; }

        /// <summary>
        /// The game items that can be bought
        /// </summary>
        public string? ItemList { get; set; }

        /// <summary>
        /// The Inventory Id of the Property
        /// </summary>
        public int InventoryId { get; set; }

        /// <summary>
        /// The IPL to be loaded in when a player enters
        /// </summary>
        public string? Ipl { get; set; }

        /// <summary>
        /// The Id of the Blip
        /// </summary>
        public int BlipId { get; set; }

        /// <summary>
        /// The blip color
        /// </summary>
        public int BlipColor { get; set; }

        /// <summary>
        /// If the Property is locked
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// The market value of the property
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The owner’s character Id
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// The unique key string? code for the property.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The list of garages associated with a property.
        /// </summary>
        public string? GarageList { get; set; }

        /// <summary>
        /// If the property is Enterable
        /// </summary>
        public bool Enterable { get; set; }

        /// <summary>
        /// If a voucher was used to buy the property
        /// </summary>
        public bool VoucherUsed { get; set; }

        /// <summary>
        /// The Exterior Dimension
        /// </summary>
        public int ExtDimension { get; set; }

        /// <summary>
        /// The Purchase Date of the Property
        /// </summary>
        public DateTime PurchaseDateTime { get; set; }

        /// <summary>
        /// The Buyin value of a house
        /// </summary>
        public int BuyinValue { get; set; }

        /// <summary>
        /// The amount a player has paid of the Buyin.
        /// </summary>
        public int BuyinPaid { get; set; }

        /// <summary>
        /// JSON of List string? for props
        /// </summary>
        public string? PropList { get; set; }

        /// <summary>
        /// List PropertyDoor's
        /// </summary>
        public string? DoorPositions { get; set; }

        /// <summary>
        /// Amount of Products required
        /// </summary>
        public int RequiredProducts { get; set; }

        /// <summary>
        /// Current amount of Products
        /// </summary>
        public int Products { get; set; }

        /// <summary>
        /// The buy price for products
        /// </summary>
        public double ProductBuyPrice { get; set; }

        /// <summary>
        /// The current balance of the business
        /// </summary>
        public double Balance { get; set; }

        public float InvPosX { get; set; }
        public float InvPosY { get; set; }
        public float InvPosZ { get; set; }

        public string? MusicStation { get; set; }

        public bool ClerkActive { get; set; }
        public double MortgageValue { get; set; }
        public DateTime LastMortgagePayment { get; set; }

        public bool Hidden { get; set; }

        public string? PropertyObjects { get; set; }

        public DateTime LastSetActive { get; set; }

        public double EnterFee { get; set; }

        /// <summary>
        /// Fetches total list of properties
        /// </summary>
        /// <returns></returns>
        public static List<Property> FetchProperties()
        {
            using Context context = new Context();
            return context.Property.ToList();
        }

        /// <summary>
        /// Fetches list of character properties
        /// </summary>s
        /// <param name="character"></param>
        /// <returns></returns>
        public static List<Property> FetchCharacterProperties(Character character)
        {
            using Context context = new Context();
            return context.Property.Where(i => i.OwnerId == character.Id).ToList();
        }

        /// <summary>
        /// Fetches property by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Property? FetchProperty(int id)
        {
            using Context context = new Context();
            return context.Property.Find(id);
        }

        /// <summary>
        /// Fetches a Nearby Property in Range
        /// </summary>
        /// <param name="player"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Property? FetchNearbyProperty(IPlayer player, float range)
        {
            Position playerPosition = player.Position;

            List<Property> properties = Models.Property.FetchProperties();

            float lastDistance = range;
            Property lastProperty = null;

            foreach (Property property in properties)
            {
                Position propertyPosition = new Vector3(property.PosX, property.PosY, property.PosZ);

                float distance = propertyPosition.Distance(playerPosition);

                if (!(distance < lastDistance)) continue;
                lastProperty = property;
                lastDistance = distance;
            }

            return lastProperty;
        }

        /// <summary>
        /// Fetches property's garages
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static List<PropertyGarage> FetchGarages(Property property)
        {
            return JsonConvert.DeserializeObject<List<PropertyGarage>>(property.GarageList);
        }

        /// <summary>
        /// Fetches a nearby garage
        /// </summary>
        /// <param name="player"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static PropertyGarage? FetchNearbyGarage(IPlayer player, float range)
        {
            List<Property> propertyList = FetchProperties();

            foreach (Property property in propertyList)
            {
                List<PropertyGarage> propertyGarages = FetchGarages(property);

                if (!propertyGarages.Any()) continue;

                foreach (PropertyGarage propertyGarage in propertyGarages)
                {
                    Position garagePosition = new Position(propertyGarage.PosX, propertyGarage.PosY, propertyGarage.PosZ);

                    if (garagePosition.Distance(player.Position) <= range)
                    {
                        return propertyGarage;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Fetch the PropertyGarage from the Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static PropertyGarage FetchPropertyGarage(string id)
        {
            List<Property> properties = FetchProperties();

            foreach (Property property in properties)
            {
                if (FetchGarages(property).Any())
                {
                    List<PropertyGarage> garages = FetchGarages(property);

                    foreach (PropertyGarage propertyGarage in garages)
                    {
                        if (propertyGarage.Id == id)
                        {
                            return propertyGarage;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Fetches the lifestyle choice of the property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static LifestyleChoice FetchPropertyLifestyle(Property property)
        {
            if (property.Value > 1 && property.Value <= 25000)
            {
                return LifestyleChoice.VeryLow;
            }

            if (property.Value > 25000 && property.Value <= 75000)
            {
                return LifestyleChoice.Low;
            }

            if (property.Value > 75000 && property.Value <= 200000)
            {
                return LifestyleChoice.Medium;
            }

            if (property.Value > 200000 && property.Value <= 400000)
            {
                return LifestyleChoice.High;
            }

            if (property.Value > 400000 && property.Value <= 1000000)
            {
                return LifestyleChoice.Luxury;
            }

            return LifestyleChoice.Homeless;
        }

        public static Inventory.Inventory FetchInventory(Property property)
        {
            if (property == null) return null;

            if (property.InventoryId == 0) return null;

            return new Inventory.Inventory(InventoryData.GetInventoryData(property.InventoryId));
        }
    }

    public enum PropertyType
    {
        House,
        GeneralBiz,
        VehicleModShop,
        LowEndClothes,
        MedClothes,
        HighClothes,
        Hair,
        Tattoo,
        Surgeon,
        KeySmith,
        GunStore
    }

    public class PropertyInteractionPoint
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
    }

    public class PropertyItems
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class PropertyGarage
    {
        public string Id { get; set; }
        public int PropertyId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public GarageTypes GarageType { get; set; }
        public int VehicleCount { get; set; }
    }

    public enum GarageTypes
    {
        None,
        Two,
        Four,
        Ten
    }
}