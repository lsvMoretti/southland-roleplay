using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net.Data;

namespace Server.Models
{
    public class Warehouse
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of Warehouse
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// PosX of Warehouse
        /// </summary>
        public float PosX { get; set; }

        /// <summary>
        /// PosY of Warehouse
        /// </summary>
        public float PosY { get; set; }

        /// <summary>
        /// PosZ of Warehouse
        /// </summary>
        public float PosZ { get; set; }

        /// <summary>
        /// WarehouseType
        /// </summary>
        public WarehouseType Type { get; set; }

        /// <summary>
        /// Current amount of Products
        /// </summary>
        public int Products { get; set; }

        /// <summary>
        /// Maximum amount of Products
        /// </summary>
        public double MaxProducts { get; set; }

        /// <summary>
        /// Minimum price for product when Products = 0 / 1
        /// </summary>
        public double MinPrice { get; set; }

        /// <summary>
        /// Maximum of Price when Products = MaxProducts
        /// </summary>
        public double MaxPrice { get; set; }

        /*
        public Warehouse(string name, Position position, int type, double maxProducts, double minPrice, double maxPrice)
        {
            Name = name;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;
            Type = (WarehouseType)type;
            Products = 0;
            MaxProducts = maxProducts;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
        }*/
    }

    public enum WarehouseType
    {
        None
    }
}