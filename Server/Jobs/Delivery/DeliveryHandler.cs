using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AltV.Net.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.Marker;
using Server.Extensions.TextLabel;
using Server.Models;
using Server.Objects;

namespace Server.Jobs.Delivery
{
    public class DeliveryHandler
    {
        private static Dictionary<int, TextLabel> pointLabels = new Dictionary<int, TextLabel>();
        private static Dictionary<int, Marker> pointMarkers = new Dictionary<int, Marker>();

        public static Position JobPosition = new Position(-424.06152f, -2789.3933f, 6.2460938f);

        public static void LoadJobPoint()
        {
            Blip jobBlip = new Blip("Delivery Driver", JobPosition, 67, 54, 0.75f);
            jobBlip.Add();

            TextLabel textLabel = new TextLabel("Delivery Job\nUsage: /join", JobPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.DarkCyan));
            textLabel.Add();
        }

        public static void LoadDeliveryPoints()
        {
            LoadJobPoint();
            using Context context = new Context();

            Console.WriteLine($"Loading Delivery Points");

            List<DeliveryPoint> deliveryPoints = context.DeliveryPoint.ToList();

            foreach (DeliveryPoint deliveryPoint in deliveryPoints)
            {
                LoadDeliveryPoint(deliveryPoint);
            }

            Console.WriteLine($"Loaded {deliveryPoints.Count} Delivery Points");
        }

        public static void LoadDeliveryPoint(DeliveryPoint deliveryPoint)
        {
            Position position = FetchPosition(deliveryPoint);

            switch (deliveryPoint.PointType)
            {
                case DeliveryPointType.Pickup:
                    TextLabel newLabel = new TextLabel($"{deliveryPoint.Name}\nUsage: /buyshipment [Amount]\n{deliveryPoint.CostPerItem:C} per item", position, TextFont.FontChaletComprimeCologne, new LsvColor(Color.Coral));

                    newLabel.Add();

                    pointLabels.Add(deliveryPoint.Id, newLabel);
                    break;

                case DeliveryPointType.DropOff:

                    TextLabel dropoffLabel = new TextLabel($"{deliveryPoint.Name}\nUsage: /sellshipment [Amount]\n{WarehouseHandler.FetchWarehouse(deliveryPoint.WarehouseId)?.MinPrice * 1.2:C} per item", position, TextFont.FontChaletComprimeCologne, new LsvColor(Color.Coral));

                    dropoffLabel.Add();

                    pointLabels.Add(deliveryPoint.Id, dropoffLabel);
                    break;
            }

            Marker newMarker = new Marker(MarkerType.MarkerTypeVerticalCylinder, position, Vector3.Zero, Rotation.Zero, 1f, Color.Coral);

            newMarker.Add();

            pointMarkers.Add(deliveryPoint.Id, newMarker);
        }

        public static Position FetchPosition(DeliveryPoint deliveryPoint)
        {
            return new Position(deliveryPoint.PosX, deliveryPoint.PosY, deliveryPoint.PosZ);
        }

        public static DeliveryPoint FetchNearestPoint(Position position, float range = 5f)
        {
            using Context context = new Context();

            List<DeliveryPoint> deliveryPoints =
                context.DeliveryPoint.ToList();

            
            List<DeliveryPoint> inRangeList = new List<DeliveryPoint>();

            foreach (DeliveryPoint deliveryPoint in deliveryPoints)
            {
                Position deliveryPosition = FetchPosition(deliveryPoint);

                if (deliveryPosition.Distance(position) <= range)
                {
                    inRangeList.Add(deliveryPoint);
                }
            }

            var ordered = inRangeList.OrderBy(x => FetchPosition(x).Distance(position)).ToList();

            foreach (DeliveryPoint deliveryPoint in ordered)
            {
                Console.WriteLine(deliveryPoint.Name);
            }

            return ordered.Any() ? ordered.FirstOrDefault() : null;
        }

        public static List<DeliveryPoint> FetchAllDeliveryPoints()
        {
            using Context context = new Context();

            return context.DeliveryPoint.ToList();
        }
    }

    public static class DeliveryPointExtension
    {
        public static Position Position(this DeliveryPoint deliveryPoint)
        {
            return new Position(deliveryPoint.PosX, deliveryPoint.PosY, deliveryPoint.PosY);
        }

        public static Position Position(this Warehouse warehouse)
        {
            return new Position(warehouse.PosX, warehouse.PosY, warehouse.PosZ);
        }
    }

    public enum DeliveryPointType
    {
        Pickup,
        DropOff
    }
}