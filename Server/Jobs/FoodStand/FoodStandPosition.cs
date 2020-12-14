using Newtonsoft.Json;

namespace Server.Jobs.FoodStand
{
    public class FoodStandPosition
    {
        public string Name { get; set; }
        public object Hash { get; set; }
        public Position Position { get; set; }
        public Rotation Rotation { get; set; }
        
        public bool IsHotDogStand { get; set; }
        public bool IsBurgerStand { get; set; }
        
        public FoodStandPosition(bool isHotDogStand = false, bool isBurgerStand = false)
        {
            IsHotDogStand = isHotDogStand;
            IsBurgerStand = isBurgerStand;
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

    public static class FoodStandExtension
    {
        public static AltV.Net.Data.Position FetchPosition(this FoodStandPosition stand)
        {
            return new AltV.Net.Data.Position(stand.Position.X, stand.Position.Y, stand.Position.Z);
        }
    }
}