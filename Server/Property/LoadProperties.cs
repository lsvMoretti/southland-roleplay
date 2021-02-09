using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using EntityStreamer;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.Marker;
using Server.Extensions.TextLabel;
using Server.Map;
using Server.Models;
using Blip = Server.Objects.Blip;
using Marker = Server.Extensions.Marker.Marker;

namespace Server.Property
{
    public class LoadProperties
    {
        public static Dictionary<int, Marker> PropertyMarkers = new Dictionary<int, Marker>();
        public static Dictionary<int, TextLabel> PropertyTextLabels = new Dictionary<int, TextLabel>();
        public static Dictionary<int, Blip> PropertyBlips = new Dictionary<int, Blip>();
        public static List<LoadPropertyObject> LoadedPropertyObjects = new List<LoadPropertyObject>();

        /// <summary>
        /// Initializes loading all properties
        /// </summary>
        public static void LoadAllProperties()
        {
            try
            {
                Console.WriteLine($"Loading Properties");

                Interiors.LoadInteriors();

                List<Models.Property> properties = Models.Property.FetchProperties();

                foreach (Models.Property property in properties)
                {
                    LoadProperty(property);
                }

                Console.WriteLine($"Loaded {properties.Count} Properties");
                Console.WriteLine($"Loaded {LoadedPropertyObjects.Count} Property Objects");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        /// <summary>
        /// Loads the property
        /// </summary>
        /// <param name="property"></param>
        public static void LoadProperty(Models.Property property)
        {
            Position propertyPosition = property.FetchExteriorPosition();

            // - new Position(0f, 0f, 1f)
            Marker propertyMarker = new Marker(MarkerType.MarkerTypeHorizontalSplitArrowCircle, property.FetchExteriorPosition() + new Position(0, 0, 0.5f), Vector3.Zero, Rotation.Zero, 1f, Color.DarkOliveGreen);

            PropertyMarkers.Add(property.Id, propertyMarker);

            propertyMarker.Add();

            string propertyType = "";

            switch (property.PropertyType)
            {
                case PropertyType.House:

                    break;

                case PropertyType.GeneralBiz:

                    break;

                case PropertyType.VehicleModShop:
                    break;

                case PropertyType.LowEndClothes:
                    propertyType = "Clothing Store";
                    break;

                case PropertyType.MedClothes:
                    propertyType = "Clothing Store";
                    break;

                case PropertyType.HighClothes:
                    propertyType = "Clothing Store";
                    break;

                case PropertyType.Hair:
                    propertyType = "Hair Salon";
                    break;

                case PropertyType.Tattoo:
                    propertyType = "Tattoo Parlor";
                    break;

                case PropertyType.Surgeon:
                    propertyType = "Surgeon";
                    break;

                case PropertyType.KeySmith:
                    propertyType = "Key Smith";
                    break;

                case PropertyType.GunStore:
                    propertyType = "Gun Store";
                    break;

                default:
                    propertyType = "";
                    break;
            }

            if (property.PropertyType == PropertyType.House)
            {
                if (property.OwnerId == 0 && property.Value > 1)
                {
                    TextLabel propertyTextLabel = new TextLabel($"{property.Address}\nFor Sale: {property.Value:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                    propertyTextLabel.Add();

                    PropertyTextLabels.Add(property.Id, propertyTextLabel);
                }
                else
                {
                    TextLabel propertyTextLabel = new TextLabel(property.Address, propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                    propertyTextLabel.Add();

                    PropertyTextLabels.Add(property.Id, propertyTextLabel);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(propertyType))
                {
                    if (property.OwnerId == 0 && property.Value > 1)
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{property.Address}\nFor Sale: {property.Value:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }

                    if (property.EnterFee > 0)
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{property.Address}\nFee: {property.EnterFee:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }
                    else
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{property.Address}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }
                }
                else
                {
                    if (property.OwnerId == 0 && property.Value > 1)
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{propertyType}\n{property.Address}\nFor Sale: {property.Value:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }

                    if (property.EnterFee > 0)
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{propertyType}\n{property.Address}\nFee: {property.EnterFee:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }
                    else
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{propertyType}\n{property.Address}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }
                }

                if (property.BlipId != 0)
                {
                    Blip propertyBlip = new Blip(property.BusinessName, propertyPosition, property.BlipId, property.BlipColor, 1);
                    propertyBlip.Add();

                    PropertyBlips.Add(property.Id, propertyBlip);
                }
            }

            if (!string.IsNullOrEmpty(property.InteriorName))
            {
                MapHandler.LoadMapForProperty(property);
            }

            if (string.IsNullOrEmpty(property.PropertyObjects))
            {
                using Context context = new Context();

                Models.Property dbProperty = context.Property.Find(property.Id);

                if (dbProperty != null)
                {
                    dbProperty.PropertyObjects = JsonConvert.SerializeObject(new List<PropertyObject>());
                    context.SaveChanges();
                }
            }

            LoadPropertyObjects(property);
        }

        public static void ReloadProperty(Models.Property property)
        {
            Position propertyPosition = property.FetchExteriorPosition();

            if (PropertyMarkers.ContainsKey(property.Id))
            {
                KeyValuePair<int, Marker> oldMarkerInfo = PropertyMarkers.FirstOrDefault(x => x.Key == property.Id);

                oldMarkerInfo.Value.Remove();

                PropertyMarkers.Remove(property.Id);
            }

            if (PropertyTextLabels.ContainsKey(property.Id))
            {
                var oldTextLabel = PropertyTextLabels.FirstOrDefault(x => x.Key == property.Id);

                oldTextLabel.Value.Remove();
                PropertyTextLabels.Remove(property.Id);
            }

            if (PropertyBlips.ContainsKey(property.Id))
            {
                var oldBlipInfo = PropertyBlips.FirstOrDefault(x => x.Key == property.Id);
                oldBlipInfo.Value.Remove();
                PropertyBlips.Remove(property.Id);
            }

            Marker propertyMarker = new Marker(MarkerType.MarkerTypeHorizontalSplitArrowCircle, property.FetchExteriorPosition() + new Position(0, 0, 0.5f), Vector3.Zero, Rotation.Zero, 1f, Color.DarkOliveGreen);

            PropertyMarkers.Add(property.Id, propertyMarker);

            propertyMarker.Add();

            string propertyType = "";

            switch (property.PropertyType)
            {
                case PropertyType.House:

                    break;

                case PropertyType.GeneralBiz:

                    break;

                case PropertyType.VehicleModShop:
                    break;

                case PropertyType.LowEndClothes:
                    propertyType = "Clothing Store";
                    break;

                case PropertyType.MedClothes:
                    propertyType = "Clothing Store";
                    break;

                case PropertyType.HighClothes:
                    propertyType = "Clothing Store";
                    break;

                case PropertyType.Hair:
                    propertyType = "Hair Salon";
                    break;

                case PropertyType.Tattoo:
                    propertyType = "Tattoo Parlor";
                    break;

                case PropertyType.Surgeon:
                    propertyType = "Surgeon";
                    break;

                case PropertyType.KeySmith:
                    propertyType = "Key Smith";
                    break;

                case PropertyType.GunStore:
                    propertyType = "Gun Store";
                    break;

                default:
                    propertyType = "";
                    break;
            }

            if (property.PropertyType == PropertyType.House)
            {
                if (property.OwnerId == 0 && property.Value > 1)
                {
                    TextLabel propertyTextLabel = new TextLabel($"{property.Address}\nFor Sale: {property.Value:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                    propertyTextLabel.Add();

                    PropertyTextLabels.Add(property.Id, propertyTextLabel);
                }
                else
                {
                    TextLabel propertyTextLabel = new TextLabel(property.Address, propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                    propertyTextLabel.Add();

                    PropertyTextLabels.Add(property.Id, propertyTextLabel);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(propertyType))
                {
                    if (property.OwnerId == 0 && property.Value > 1)
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{property.Address}\nFor Sale: {property.Value:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }

                    if (property.EnterFee > 0)
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{property.Address}\nFee: {property.EnterFee:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }
                    else
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{property.Address}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }
                }
                else
                {
                    if (property.OwnerId == 0 && property.Value > 1)
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{propertyType}\n{property.Address}\nFor Sale: {property.Value:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }

                    if (property.EnterFee > 0)
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{propertyType}\n{property.Address}\nFee: {property.EnterFee:C}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }
                    else
                    {
                        TextLabel propertyTextLabel = new TextLabel($"{property.BusinessName}\n{propertyType}\n{property.Address}", propertyPosition, TextFont.FontChaletLondon, new LsvColor(Color.DarkOliveGreen));
                        propertyTextLabel.Add();

                        PropertyTextLabels.Add(property.Id, propertyTextLabel);
                    }
                }

                if (property.BlipId != 0)
                {
                    Blip propertyBlip = new Blip(property.BusinessName, propertyPosition, property.BlipId, property.BlipColor, 1);
                    propertyBlip.Add();

                    PropertyBlips.Add(property.Id, propertyBlip);
                }
            }

            if (!string.IsNullOrEmpty(property.InteriorName))
            {
                MapHandler.LoadMapForProperty(property);
            }

            if (string.IsNullOrEmpty(property.PropertyObjects))
            {
                using Context context = new Context();

                Models.Property dbProperty = context.Property.Find(property.Id);

                if (dbProperty != null)
                {
                    dbProperty.PropertyObjects = JsonConvert.SerializeObject(new List<PropertyObject>());
                    context.SaveChanges();
                }
            }

            LoadPropertyObjects(property);
        }

        public static void UnloadProperty(Models.Property property)
        {
            if (PropertyMarkers.ContainsKey(property.Id))
            {
                KeyValuePair<int, Marker> oldMarkerInfo = PropertyMarkers.FirstOrDefault(x => x.Key == property.Id);

                oldMarkerInfo.Value.Remove();

                PropertyMarkers.Remove(property.Id);
            }

            if (PropertyTextLabels.ContainsKey(property.Id))
            {
                var oldTextLabel = PropertyTextLabels.FirstOrDefault(x => x.Key == property.Id);

                oldTextLabel.Value.Remove();
                PropertyMarkers.Remove(property.Id);
            }

            if (PropertyBlips.ContainsKey(property.Id))
            {
                var oldBlipInfo = PropertyBlips.FirstOrDefault(x => x.Key == property.Id);
                oldBlipInfo.Value.Remove();
                PropertyBlips.Remove(property.Id);
            }

            if (!string.IsNullOrEmpty(property.InteriorName))
            {
                MapHandler.UnloadMapForProperty(property);
            }

            UnloadPropertyObjects(property);
        }

        public static async void LoadPropertyObjects(Models.Property property)
        {
            List<PropertyObject> propertyObjects = null;

            if (string.IsNullOrEmpty(property.PropertyObjects))
            {
                Console.WriteLine($"Property {property.Id} has null property objects.");

                using Context context = new Context();

                propertyObjects = new List<PropertyObject>();

                Models.Property dbProperty = await context.Property.FindAsync(property.Id);

                if (dbProperty == null) return;

                dbProperty.PropertyObjects = JsonConvert.SerializeObject(propertyObjects, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                await context.SaveChangesAsync();
            }

            if (propertyObjects == null)
            {
                propertyObjects = JsonConvert.DeserializeObject<List<PropertyObject>>(property.PropertyObjects);
            }

            if (propertyObjects == null)
            {
                using Context context = new Context();

                propertyObjects = new List<PropertyObject>();

                Models.Property dbProperty = await context.Property.FindAsync(property.Id);

                if (dbProperty == null) return;

                dbProperty.PropertyObjects = JsonConvert.SerializeObject(propertyObjects, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                await context.SaveChangesAsync();
            }

            if (!propertyObjects.Any()) return;

            foreach (LoadPropertyObject loadedPropertyObject in LoadedPropertyObjects.Where(x => x.PropertyId == property.Id).ToList())
            {
                PropStreamer.Delete(loadedPropertyObject.DynamicObject);
                LoadedPropertyObjects.Remove(loadedPropertyObject);
            }

            foreach (PropertyObject propertyObject in propertyObjects)
            {
                LoadPropertyObject(property, propertyObject);
            }
        }

        public static void UnloadPropertyObjects(Models.Property property)
        {
            foreach (LoadPropertyObject loadedPropertyObject in LoadedPropertyObjects.Where(x => x.PropertyId == property.Id).ToList())
            {
                PropStreamer.Delete(loadedPropertyObject.DynamicObject);
                LoadedPropertyObjects.Remove(loadedPropertyObject);
            }
        }

        public static async void LoadPropertyObject(Models.Property property, PropertyObject propertyObject)
        {
            uint objectHash = Alt.Hash(propertyObject.ObjectName);

            Prop dynamicProp = PropStreamer.Create(propertyObject.ObjectName, propertyObject.Position, propertyObject.Rotation,
                propertyObject.Dimension);

            LoadedPropertyObjects.Add(new LoadPropertyObject(property.Id, dynamicProp));

            Console.WriteLine($"Loaded {propertyObject.Name} into {property.Address}");
        }
    }
}