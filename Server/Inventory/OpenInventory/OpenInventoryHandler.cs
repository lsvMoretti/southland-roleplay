using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Models;

namespace Server.Inventory.OpenInventory
{
    public class OpenInventoryHandler
    {
        public static void CheckOpenInventoryLocations()
        {
            List<StorageLocation> storageLocations = new List<StorageLocation>();

            string directory = "data/storages/";

            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Unable to find inventory storage directory. /data/storages");
                return;
            }

            foreach (string filePath in Directory.GetFiles(directory))
            {
                string fileContents = File.ReadAllText(filePath);

                List<StorageLocation> storage = JsonConvert.DeserializeObject<List<StorageLocation>>(fileContents);

                bool isDumpster = filePath.Contains("Dumpsters");

                if (isDumpster)
                {
                    foreach (StorageLocation storageLocation in storage)
                    {
                        storageLocation.IsDumpster = true;
                    }
                }

                storageLocations.AddRange(storage);
            }

            Console.WriteLine($"Found {storageLocations.Count} storage places!");

            using Context context = new Context();

            List<Models.Storage> storages = context.Storages.ToList();

            int dumpsterCount = 0;
            int binCount = 0;

            if (storages.Count >= storageLocations.Count) return;

            foreach (StorageLocation storageLocation in storageLocations)
            {
                Storage storage = storages.FirstOrDefault(x =>
                    Math.Abs(x.PosX - storageLocation.Position.X) < 1 && Math.Abs(x.PosY - storageLocation.Position.Y) < 1 &&
                    Math.Abs(x.PosZ - storageLocation.Position.Z) < 1);

                if (storage != null) continue;

                Storage newStorage = new Storage
                {
                    PosX = storageLocation.Position.X,
                    PosY = storageLocation.Position.Y,
                    PosZ = storageLocation.Position.Z,
                    RotX = storageLocation.Rotation.X,
                    RotY = storageLocation.Rotation.Y,
                    RotZ = storageLocation.Rotation.Z
                };

                newStorage.InventoryId = storageLocation.IsDumpster ? InventoryData.CreateDefaultInventory(15f, 10f).ID : InventoryData.CreateDefaultInventory(7f, 10f).ID;

                if (storageLocation.IsDumpster)
                {
                    dumpsterCount++;
                }
                else
                {
                    binCount++;
                }

                context.Storages.Add(newStorage);
            }

            context.SaveChanges();

            Console.WriteLine($"Added {storageLocations.Count - context.Storages.Count()} new storages.");
        }
    }
}