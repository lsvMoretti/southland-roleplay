using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using AltV.Net.Data;
using EntityStreamer;
using Server.Drug;
using Server.Extensions.TextLabel;

namespace Server.Models
{
    public class Marijuana
    {
        [Key]
        public int Id { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public int Dimension { get; set; }
        public MarijuanaStatus Status { get; set; }
        public DateTime PlantTime { get; set; }
        public bool Boosted { get; set; }
        public bool Test { get; set; }

        public Marijuana()
        {
        }

        public static Marijuana PlantMarijuana(Position position, int dimension)
        {
            Marijuana newMarijuana = new Marijuana
            {
                PosX = position.X,
                PosY = position.Y,
                PosZ = position.Z,
                Dimension = dimension,
                Status = MarijuanaStatus.Seed,
                PlantTime = DateTime.Now,
                Boosted = false,
                Test = false
            };

            using Context context = new Context();

            context.Marijuana.Add(newMarijuana);
            context.SaveChanges();

            GrowingHandler.LoadMarijuana(newMarijuana);

            return newMarijuana;
        }

        public static Position Position(Marijuana marijuana)
        {
            return new Position(marijuana.PosX, marijuana.PosY, marijuana.PosZ);
        }

        public static Marijuana FetchNearest(Position position, int dimension, float range = 3f)
        {
            using Context context = new Context();

            Marijuana marijuana = null;
            float lastRange = range;

            foreach (Marijuana item in context.Marijuana.ToList())
            {
                if (item.Dimension != dimension) continue;

                AltV.Net.Data.Position itemPosition = Position(item);

                float distance = position.Distance(itemPosition);

                if (distance < lastRange)
                {
                    lastRange = distance;
                    marijuana = item;
                }
            }

            return marijuana;
        }

        public static void RemoveMarijuana(Marijuana marijuana)
        {
            using Context context = new Context();

            bool hasLabel = GrowingHandler.MarijuanaLabels.TryGetValue(marijuana.Id, out TextLabel label);

            if (hasLabel)
            {
                label.Remove();
                GrowingHandler.MarijuanaLabels.Remove(marijuana.Id);
            }

            bool hasObject = GrowingHandler.MarijuanaObjects.TryGetValue(marijuana.Id, out Prop dynamicObject);

            if (hasObject)
            {
                dynamicObject.Destroy();
                GrowingHandler.MarijuanaObjects.Remove(marijuana.Id);
            }

            context.Marijuana.Remove(marijuana);

            context.SaveChanges();
        }
    }

    public enum MarijuanaStatus
    {
        [Description("Seed")]
        Seed,

        [Description("Seedling")]
        Seedling,

        [Description("Vegetative")]
        Vegetative,

        [Description("Flowering")]
        Flowering,

        [Description("Harvest")]
        Harvest,

        [Description("Withered")]
        Withered
    }
}