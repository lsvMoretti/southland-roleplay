using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AltV.Net.Data;
using Remotion.Linq.Utilities;
using Server.Extensions;
using Server.Extensions.Marker;
using Server.Extensions.TextLabel;
using Server.Models;

namespace Server.Jobs.Delivery
{
    public class WarehouseHandler
    {
        private static Dictionary<int, TextLabel> pointLabels = new Dictionary<int, TextLabel>();
        private static Dictionary<int, Marker> pointMarkers = new Dictionary<int, Marker>();

        public static void LoadWarehouses()
        {
            using Context context = new Context();

            Console.WriteLine($"Loading Warehouse Points");

            List<Warehouse> warehouses = context.Warehouse.ToList();

            foreach (Warehouse warehouse in warehouses)
            {
                LoadWarehouse(warehouse);
            }

            Console.WriteLine($"Loaded {warehouses.Count} Warehouse Points");
        }

        public static void LoadWarehouse(Warehouse warehouse)
        {
            Position position = FetchPosition(warehouse);

            TextLabel newLabel = new TextLabel($"{warehouse.Name}\nUsage: /buyproducts [Amount]", position, TextFont.FontChaletComprimeCologne, new LsvColor(Color.Coral));

            newLabel.Add();

            pointLabels.Add(warehouse.Id, newLabel);

            Marker newMarker = new Marker(MarkerType.MarkerTypeVerticalCylinder, position, Vector3.Zero, Rotation.Zero, 1f, Color.Coral);

            newMarker.Add();

            pointMarkers.Add(warehouse.Id, newMarker);
        }

        public static Position FetchPosition(Warehouse warehouse)
        {
            return new Position(warehouse.PosX, warehouse.PosY, warehouse.PosZ);
        }

        public static Warehouse FetchNearestPoint(Position position, float range = 5f)
        {
            using Context context = new Context();

            List<Warehouse> warehousePoints =
                context.Warehouse.ToList();

            
            List<Warehouse> inRangeList = new List<Warehouse>();

            foreach (Warehouse warehousePoint in warehousePoints)
            {
                Position deliveryPosition = FetchPosition(warehousePoint);

                if (deliveryPosition.Distance(position) <= range)
                {
                    inRangeList.Add(warehousePoint);
                }
            }

            var ordered = inRangeList.OrderBy(x => FetchPosition(x).Distance(position)).ToList();

            foreach (Warehouse deliveryPoint in ordered)
            {
                Console.WriteLine(deliveryPoint.Name);
            }

            return ordered.Any() ? ordered.FirstOrDefault() : null;
        }

        public static Warehouse FetchWarehouse(int id)
        {
            using Context context = new Context();

            return context.Warehouse.Find(id);
        }

        public static List<Warehouse> FetchWarehouses()
        {
            using Context context = new Context();

            return context.Warehouse.ToList();
        }
    }
}