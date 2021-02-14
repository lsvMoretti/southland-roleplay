using AltV.Net.Data;

namespace Server.Extensions
{
    public static class PropertyExtension
    {
        /// <summary>
        /// Loads the exterior door position of the property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Position FetchExteriorPosition(this Models.Property property)
        {
            return new Position(property.PosX, property.PosY, property.PosZ);
        }

        public static void AddToBalance(this Models.Property property, double amount)
        {
            using Context context = new Context();

            Models.Property propertyDb = context.Property.Find(property.Id);

            if (propertyDb == null) return;

            propertyDb.Balance += amount;

            context.SaveChanges();
        }

        public static void RemoveProduct(this Models.Property property, int amount = 1)
        {
            using Context context = new Context();

            Models.Property propertyDb = context.Property.Find(property.Id);

            if (propertyDb == null) return;

            propertyDb.Products -= amount;
            context.SaveChanges();
        }
    }
}