using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Server.Jobs.FoodStand
{
    public class FoodStandHandler
    {
        public static List<FoodStandPosition> FoodStands = new List<FoodStandPosition>();

        public static void FetchFoodStandPositions()
        {
            Console.WriteLine($"Fetching Food Stand Positions");

#if RELEASE

            string directory = "D:/servers/Paradigm/data/foodstands";
#endif

#if DEBUG

            string directory = "D:/servers/Paradigm-Dev/data/foodstands";
#endif

            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Food Stand Directory not found!");
                return;
            }

            int foodStandCount = 0;
            int hotdogCount = 0;
            int burgerCount = 0;

            FoodStands = new List<FoodStandPosition>();

            foreach (string filePath in Directory.GetFiles(directory))
            {
                string fileContents = File.ReadAllText(filePath);

                List<FoodStandPosition> foodStandPositions =
                    JsonConvert.DeserializeObject<List<FoodStandPosition>>(fileContents);

                foreach (FoodStandPosition foodStandPosition in foodStandPositions)
                {
                    foodStandCount++;

                    if (foodStandPosition.Name == "prop_burgerstand_01")
                    {
                        burgerCount++;
                        foodStandPosition.IsBurgerStand = true;
                    }

                    if (foodStandPosition.Name == "prop_hotdogstand_01")
                    {
                        hotdogCount++;
                        foodStandPosition.IsHotDogStand = true;
                    }
                }

                FoodStands.AddRange(foodStandPositions);
            }

            Console.WriteLine($"Found {foodStandCount} food stands. {hotdogCount} hot dog stands and {burgerCount} burger stands");
        }

        public static FoodStandPosition FetchNearestPosition(AltV.Net.Data.Position position, float distance = 5f)
        {
            float lastDistance = distance;
            FoodStandPosition foodStand = null;

            foreach (FoodStandPosition foodStandPosition in FoodStands)
            {
                AltV.Net.Data.Position standPosition = foodStandPosition.FetchPosition();

                float standDistance = position.Distance(standPosition);

                if (standDistance < lastDistance)
                {
                    lastDistance = standDistance;
                    foodStand = foodStandPosition;
                }
            }

            return foodStand;
        }
    }
}