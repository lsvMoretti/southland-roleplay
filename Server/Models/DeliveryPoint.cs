using System.ComponentModel.DataAnnotations;
using AltV.Net.Data;
using Server.Jobs.Delivery;

namespace Server.Models
{
    public class DeliveryPoint
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public DeliveryPointType PointType { get; set; }

        public int WarehouseId { get; set; }

        public float CostPerItem { get; set; }
        /*
        public DeliveryPoint(string name, Position position, DeliveryPointType pointType, int warehouseId)
        {
            Name = name;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;
            PointType = pointType;
            WarehouseId = warehouseId;
            CostPerItem = 0;
        }*/
    }
}