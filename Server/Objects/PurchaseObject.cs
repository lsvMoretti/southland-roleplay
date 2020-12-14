namespace Server.Objects
{
    public class PurchaseObject
    {
        // Friendly name of object
        public string Name { get; set; }
        // Object Name
        public string ObjectName { get; set; }
        // Price
        public double Cost { get; set; }

        public PurchaseObject()
        {
            
        }

        public PurchaseObject(string name, string objectName, double cost)
        {
            Name = name;
            ObjectName = objectName;
            Cost = cost;
        }
    }
}