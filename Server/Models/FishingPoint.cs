using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net.Data;

namespace Server.Models
{
    [Table("fishingpoints")]
    public class FishingPoint
    {
        [Key]
        public int Id { get; set; }

        public FishingPointType PointType { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public int FishCount { get; set; }

        public int MaxFish { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }

        public FishingPoint()
        {
        }

        public FishingPoint(FishingPointType pointType, Position position, int fishCount, int maxFish, int minPrice, int maxPrice)
        {
            PointType = pointType;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;

            FishCount = fishCount;

            MaxFish = maxFish;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
        }
    }

    public enum FishingPointType
    {
        FishSpot,
        SellPoint
    }
}