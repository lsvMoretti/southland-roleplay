using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using EntityStreamer;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

namespace Server.Map
{
    public class MapHandler
    {
        public static List<Map> LoadedMaps = new List<Map>();

        /// <summary>
        /// Loads maps dynamically
        /// </summary>
        public static async Task LoadMaps()
        {
            if (LoadedMaps.Count > 0)
            {
                foreach (Map loadedMap in LoadedMaps)
                {
                    foreach (Prop loadedObject in loadedMap.LoadedObjects)
                    {
                        loadedObject.Destroy();
                    }
                }

                LoadedMaps = new List<Map>();
            }

            string directory = "data/maps/";

            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Unable to find the Mapping Directory!");
                return;
            }

            int objectCount = 0;

            foreach (string file in Directory.GetFiles(directory))
            {
                if (!file.EndsWith(".json")) continue;

                Console.WriteLine($"Opening {file}.");

                string fileContents = File.ReadAllText(file);

                Map newMap = JsonConvert.DeserializeObject<Map>(fileContents);

                Console.WriteLine($"Found map: {newMap.MapName}. IsInterior: {newMap.IsInterior}. Objects: {newMap.MapObjects.Count}");

                if (!newMap.IsInterior)
                {
                    foreach (MapObject mapObject in newMap.MapObjects)
                    {
                        Prop newObject = PropStreamer.Create(mapObject.Model.ToString(), mapObject.Position, mapObject.Rotation,
                            mapObject.Dimension, mapObject.Dynamic, false, mapObject.Frozen, mapObject.LodDistance,
                            mapObject.LightColor, mapObject.OnFire, mapObject.TextureVariation, mapObject.Visible,
                            mapObject.StreamRange);
                        newMap.LoadedObjects.Add(newObject);
                        objectCount++;
                    }
                }
                LoadedMaps.Add(newMap);
                Console.WriteLine($"Loaded map: {newMap.MapName}.");
            }

            Console.WriteLine($"Loaded {LoadedMaps.Count} maps with {objectCount} total objects.");
        }

        public static void LoadMapForProperty(Models.Property property)
        {
            if (string.IsNullOrEmpty(property.InteriorName)) return;

            Map map = LoadedMaps.FirstOrDefault(x => x.IsInterior && x.Interior == property.InteriorName);

            if (map == null) return;

            List<Prop> loadedObjects = map.LoadedObjects.Where(x => x.Dimension == property.Id).ToList();

            if (loadedObjects.Any())
            {
                foreach (Prop loadedObject in loadedObjects)
                {
                    loadedObject.Destroy();
                    map.LoadedObjects.Remove(loadedObject);
                }
            }

            int dimension = property.Id;
            int objectCount = 0;

            foreach (MapObject mapObject in map.MapObjects)
            {
                Prop newObject = PropStreamer.Create(mapObject.Model.ToString(), mapObject.Position, mapObject.Rotation,
                    mapObject.Dimension, mapObject.Dynamic, false, mapObject.Frozen, mapObject.LodDistance,
                    mapObject.LightColor, mapObject.OnFire, mapObject.TextureVariation, mapObject.Visible,
                    mapObject.StreamRange);
                map.LoadedObjects.Add(newObject);
                objectCount++;
            }

            Console.WriteLine($"Loaded {objectCount} objects for {property.Address}.");
        }

        public static void UnloadMapForProperty(Models.Property property)
        {
            if (string.IsNullOrEmpty(property.InteriorName)) return;

            Map map = LoadedMaps.FirstOrDefault(x => x.IsInterior && x.Interior == property.InteriorName);

            if (map == null) return;

            List<Prop> loadedObjects = map.LoadedObjects.Where(x => x.Dimension == property.Id).ToList();

            if (loadedObjects.Any())
            {
                foreach (Prop loadedObject in loadedObjects)
                {
                    loadedObject.Destroy();
                    map.LoadedObjects.Remove(loadedObject);
                }
            }
        }

        public static async void ReloadMaps()
        {
            using Context context = new Context();

            List<Models.Property> properties =
                context.Property.Where(x => !string.IsNullOrEmpty(x.InteriorName)).ToList();

            foreach (Models.Property property in properties)
            {
                UnloadMapForProperty(property);
            }

            await LoadMaps();

            foreach (Models.Property property in properties)
            {
                LoadMapForProperty(property);
            }
        }
    }

    public class MapObject
    {
        public uint Model { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public int Dimension { get; set; }
        public bool? Dynamic { get; set; }
        public bool? Frozen { get; set; }
        public uint? LodDistance { get; set; }
        public Rgb LightColor { get; set; }
        public bool? OnFire { get; set; }
        public TextureVariation? TextureVariation { get; set; }
        public bool? Visible { get; set; }
        public uint StreamRange { get; set; }
    }
}