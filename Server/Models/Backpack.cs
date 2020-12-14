using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Backpack
    {
        [Key]
        public int Id { get; set; }

        public int InventoryId { get; set; }

        public int Drawable { get; set; }

        public float DropPosX { get; set; }
        public float DropPosY { get; set; }
        public float DropPosZ { get; set; }

        public static Backpack CreateBackpack(int drawable)
        {
            if (drawable == 31)
            {
                // Black rucksack
                Backpack newBackpack = new Backpack
                {
                    InventoryId = InventoryData.CreateDefaultInventory(10, 8).ID,
                    Drawable = drawable,
                    DropPosX = 0,
                    DropPosY = 0,
                    DropPosZ = 0
                };
                using Context context = new Context();

                context.Backpacks.Add(newBackpack);

                context.SaveChanges();
                

                return newBackpack;
            }

            if (drawable == 44)
            {
                // Back gym bag
                Backpack newBackpack = new Backpack
                {
                    InventoryId = InventoryData.CreateDefaultInventory(14, 8).ID,
                    Drawable = drawable,
                    DropPosX = 0,
                    DropPosY = 0,
                    DropPosZ = 0
                };
                using Context context = new Context();

                context.Backpacks.Add(newBackpack);

                context.SaveChanges();
                

                return newBackpack;
            }

            return null;
        }

        public static Backpack FetchBackpack(int id)
        {
            using Context context = new Context();

            return context.Backpacks.Find(id);
        }

        public static Inventory.Inventory FetchBackpackInventory(Backpack backpack)
        {
            return backpack == null ? null : new Inventory.Inventory(InventoryData.GetInventoryData(backpack.InventoryId));
        }
    }
}