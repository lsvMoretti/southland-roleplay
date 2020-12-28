using Newtonsoft.Json;
using Server.Inventory;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Server.Models
{
    public class InventoryData
    {
        [Key]
        public int ID { get; set; }

        public string? invetoryItems { get; set; }
        public float InventorySpace { get; set; }
        public float InventoryCapacity { get; set; }

        public static InventoryData CreateDefaultInventory(float space, float capacity)
        {
            InventoryData newInv = new InventoryData
            {
                invetoryItems = JsonConvert.SerializeObject(new List<InventoryItem>()),
                InventorySpace = space,
                InventoryCapacity = capacity
            };

            using (Context context = new Context())
            {
                context.Inventory.Add(newInv);
                context.SaveChanges();
            }

            return newInv;
        }

        public static InventoryData GetInventoryData(int id)
        {
            using Context context = new Context();
            return context.Inventory.FirstOrDefault(i => i.ID == id);
        }
    }
}