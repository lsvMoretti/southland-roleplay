using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;
using AltV.Net.Data;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Server.Models;
using Server.Objects;

namespace Server.Jobs.Fishing
{
    public class FishingHandler
    {
        public static void InitFishing()
        {
            Console.WriteLine($"Loading Fishing Points");
            using Context context = new Context();

            List<FishingPoint> fishingPoints = context.FishingPoints.ToList();

            foreach (FishingPoint fishingPoint in fishingPoints)
            {
                LoadFishingPoint(fishingPoint);
            }

            Console.WriteLine($"Loaded {fishingPoints.Count} Fishing Points");
            
            Timer timer = new Timer(300000)
            {
                AutoReset = true
            };
            timer.Start();
            timer.Elapsed += (sender, args) =>
            {
                timer.Stop();

                using Context fishContext = new Context();
                List<FishingPoint> fishPoints = fishContext.FishingPoints.ToList();

                foreach (FishingPoint fishingPoint in fishPoints)
                {
                    if (fishingPoint.PointType == FishingPointType.FishSpot)
                    {
                        int fishLeft = 120 - fishingPoint.FishCount;
                        if (fishLeft > 5)
                        {
                            fishingPoint.FishCount += 5;

                            Console.WriteLine($"Added {5} fish to point {fishingPoint.Id}.");
                        }
                        else
                        {
                            fishingPoint.FishCount += fishLeft;
                            Console.WriteLine($"Added {fishLeft} fish to point {fishingPoint.Id}.");
                        }
                    }
                    else
                    {
                        double removeCount = Math.Round(fishingPoint.FishCount * 0.03, MidpointRounding.ToZero);

                        fishingPoint.FishCount -= (int)removeCount;

                        Console.WriteLine($"Removed {(int)removeCount} fish from point {fishingPoint.Id}.");
                    }
                }
                fishContext.SaveChanges();
                timer.Start();
            };
        }

        public static void LoadFishingPoint(FishingPoint fishingPoint)
        {
            Position position = new Position(fishingPoint.PosX, fishingPoint.PosY, fishingPoint.PosZ);

            string labelText = "";
            string blipText = "Fishing";
            switch (fishingPoint.PointType)
            {
                case FishingPointType.FishSpot:
                    labelText = "Fishing Spot\nUse /fish";
                    blipText = "Fishing";
                    break;

                case FishingPointType.SellPoint:
                    labelText = "Fishing Sell Point\nUse /sellfish";
                    blipText = "Fishmonger";
                    break;

                default:
                    labelText = "";
                    blipText = "Fishing";
                    break;
            }

            Blip fishingBlip = new Blip(blipText, position, 540, 57, 0.5f);

            fishingBlip.Add();

            TextLabel text = new TextLabel(labelText, position, TextFont.FontChaletComprimeCologne, new LsvColor(Color.CadetBlue));

            text.Add();
        }

        public static FishingPoint FetchNearestPosition(Position position, float distance = 5f)
        {
            using Context context = new Context();

            List<FishingPoint> fishingPoints = context.FishingPoints.ToList();

            

            FishingPoint closestPoint = null;
            float lastDistance = distance;

            foreach (FishingPoint fishingPoint in fishingPoints)
            {
                Position fishingPosition = new Position(fishingPoint.PosX, fishingPoint.PosY, fishingPoint.PosZ);

                if (fishingPosition.Distance(position) > lastDistance) continue;

                lastDistance = fishingPosition.Distance(position);
                closestPoint = fishingPoint;
            }

            return closestPoint;
        }
    }
}