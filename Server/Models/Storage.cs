using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AltV.Net.Data;

namespace Server.Models
{
    public class Storage
    {
        [Key]
        public int Id { get; set; }

        public int InventoryId { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public double Balance { get; set; }

        public static Inventory.Inventory FetchInventory(Storage storage)
        {
            return new Inventory.Inventory(InventoryData.GetInventoryData(storage.InventoryId));
        }

        public static Storage FetchNearestStorage(Position position, float range = 2f)
        {
            Storage nearStorage = null;
            float lastRange = range;

            using Context context = new Context();

            List<Storage> storages = context.Storages.ToList();

            foreach (Storage storage in storages)
            {
                Position storagePosition = new Position(storage.PosX, storage.PosY, storage.PosZ);

                float distance = storagePosition.Distance(position);

                if (distance < lastRange)
                {
                    nearStorage = storage;
                    lastRange = distance;
                }
            }

            return nearStorage;
        }

        public static Storage FetchStorage(int id)
        {
            using Context context = new Context();

            return context.Storages.Find(id);
        }
    }
}