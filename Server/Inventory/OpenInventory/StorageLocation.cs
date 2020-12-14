namespace Server.Inventory.OpenInventory
{
    
    public class StorageLocation
    {
        public string Name { get; set; }
        public object Hash { get; set; }
        public Position Position { get; set; }
        public Rotation Rotation { get; set; }
        
        public bool IsDumpster { get; set; }

        public StorageLocation(bool isDumpster = false)
        {
            IsDumpster = isDumpster;
        }
    }

    public class Position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class Rotation
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
    }
}