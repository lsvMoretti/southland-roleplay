using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using AltV.Net.Data;
using EntityStreamer;
using Server.Extensions;
using Server.Extensions.TextLabel;
using Server.Models;

namespace Server.Drug
{
    public class GrowingHandler
    {
        private static Timer _minuteTimer = null;

        public static Dictionary<int, TextLabel> MarijuanaLabels = new Dictionary<int, TextLabel>();
        public static Dictionary<int, Prop> MarijuanaObjects = new Dictionary<int, Prop>();

        public static void InitDrugGrowing()
        {
            _minuteTimer = new Timer(60000)
            {
                AutoReset = true
            };

            _minuteTimer.Start();
            _minuteTimer.Elapsed += MinuteTimerOnElapsed;

            using Context context = new Context();

            List<Marijuana> marijuanaList = context.Marijuana.ToList();

            foreach (Marijuana marijuana in marijuanaList)
            {
                LoadMarijuana(marijuana);
            }
        }

        private static void MinuteTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _minuteTimer.Stop();

            using Context context = new Context();

            List<Marijuana> marijuanaList = context.Marijuana.ToList();

            DateTime timeNow = DateTime.Now;

            foreach (Marijuana marijuana in marijuanaList)
            {
                if (marijuana.Status == MarijuanaStatus.Seed)
                {
                    // First Stage = Seed

                    // Has been 1 days since planted

                    DateTime seedlingTime = marijuana.PlantTime.AddDays(1);

                    if (marijuana.Test)
                    {
                        seedlingTime = marijuana.PlantTime.AddMinutes(1);
                    }

                    if (DateTime.Compare(timeNow, seedlingTime) < 0) continue;

                    marijuana.Status = MarijuanaStatus.Seedling;

                    context.SaveChanges();
                    LoadMarijuana(marijuana);
                    continue;
                }

                if (marijuana.Status == MarijuanaStatus.Seedling)
                {
                    // Second State = Seedling

                    // Has been 2 days since being planted

                    DateTime vegetativeTime = marijuana.PlantTime.AddDays(2);

                    if (marijuana.Test)
                    {
                        vegetativeTime = marijuana.PlantTime.AddMinutes(1);
                    }

                    if (DateTime.Compare(timeNow, vegetativeTime) < 0) continue;

                    marijuana.Status = MarijuanaStatus.Vegetative;

                    context.SaveChanges();

                    LoadMarijuana(marijuana);
                    continue;
                }

                if (marijuana.Status == MarijuanaStatus.Vegetative)
                {
                    // Third State = Vegetative

                    // Has been 3 days since planted

                    DateTime floweringTime = marijuana.PlantTime.AddDays(3);

                    if (marijuana.Test)
                    {
                        floweringTime = marijuana.PlantTime.AddMinutes(1);
                    }

                    if (DateTime.Compare(timeNow, floweringTime) < 0) continue;

                    marijuana.Status = MarijuanaStatus.Flowering;

                    context.SaveChanges();

                    LoadMarijuana(marijuana);
                    continue;
                }

                if (marijuana.Status == MarijuanaStatus.Flowering)
                {
                    // Fourth State = Flowering

                    // Has been 7 days since planted

                    DateTime harvestTime = marijuana.PlantTime.AddDays(7);

                    if (marijuana.Test)
                    {
                        harvestTime = marijuana.PlantTime.AddMinutes(1);
                    }

                    if (DateTime.Compare(timeNow, harvestTime) < 0) continue;

                    marijuana.Status = MarijuanaStatus.Harvest;

                    context.SaveChanges();

                    LoadMarijuana(marijuana);
                    continue;
                }

                if (marijuana.Status == MarijuanaStatus.Harvest)
                {
                    // Fifth State = Harvest

                    // Has been 9 days since planted

                    DateTime witheredTime = marijuana.PlantTime.AddDays(9);

                    if (marijuana.Test)
                    {
                        witheredTime = marijuana.PlantTime.AddMinutes(1);
                    }

                    if (DateTime.Compare(timeNow, witheredTime) < 0) continue;

                    marijuana.Status = MarijuanaStatus.Withered;
                    context.SaveChanges();

                    LoadMarijuana(marijuana);
                    continue;
                }
            }

            _minuteTimer.Start();
        }

        public static void LoadMarijuana(Marijuana marijuana)
        {
            bool hasLabel = MarijuanaLabels.TryGetValue(marijuana.Id, out TextLabel label);

            if (hasLabel)
            {
                label.Remove();
                MarijuanaLabels.Remove(marijuana.Id);
            }

            Position plantPosition = Marijuana.Position(marijuana);

            TextLabel newLabel = new TextLabel($"Marijuana Plant\nState - {marijuana.Status.ToString()}", plantPosition, TextFont.FontChaletComprimeCologne, new LsvColor(26, 158, 9), dimension: marijuana.Dimension);

            newLabel.Add();

            MarijuanaLabels.Add(marijuana.Id, newLabel);

            bool hasObject = MarijuanaObjects.TryGetValue(marijuana.Id, out Prop oldObject);

            if (hasObject)
            {
                oldObject.Destroy();
                MarijuanaObjects.Remove(marijuana.Id);
            }

            Prop marijuanaObject = null;

            if (marijuana.Status == MarijuanaStatus.Seed)
            {
                // Seed Object

                marijuanaObject = PropStreamer.Create("bkr_prop_weed_01_small_01c",
                    Marijuana.Position(marijuana) - new Position(0, 0, 1.0f),
                    new Vector3(0, 0, 0), dimension: marijuana.Dimension, visible: true, frozen: true);
            }

            if (marijuana.Status == MarijuanaStatus.Seedling)
            {
                // Seedling Object
                marijuanaObject = PropStreamer.Create("bkr_prop_weed_01_small_01b", Marijuana.Position(marijuana) - new Position(0, 0, 1.0f),
                    new Vector3(0, 0, 0), dimension: marijuana.Dimension, visible: true, frozen: true);
            }

            if (marijuana.Status == MarijuanaStatus.Vegetative)
            {
                // Vegetative Object
                marijuanaObject = PropStreamer.Create("bkr_prop_weed_01_small_01a", Marijuana.Position(marijuana) - new Position(0, 0, 1.0f),
                    new Vector3(0, 0, 0), dimension: marijuana.Dimension, visible: true, frozen: true);
            }

            if (marijuana.Status == MarijuanaStatus.Flowering)
            {
                // Flowering Object
                marijuanaObject = PropStreamer.Create("bkr_prop_weed_med_01b", Marijuana.Position(marijuana) - new Position(0, 0, 3.5f),
                    new Vector3(0, 0, 0), dimension: marijuana.Dimension, visible: true, frozen: true);
            }

            if (marijuana.Status == MarijuanaStatus.Harvest)
            {
                // Harvest Object
                marijuanaObject = PropStreamer.Create("bkr_prop_weed_lrg_01b", Marijuana.Position(marijuana) - new Position(0, 0, 3.5f),
                    new Vector3(0, 0, 0), dimension: marijuana.Dimension, visible: true, frozen: true);
            }

            if (marijuana.Status == MarijuanaStatus.Withered)
            {
                // Withered

                marijuanaObject = PropStreamer.Create("bkr_prop_weed_01_small_01c", Marijuana.Position(marijuana) - new Position(0, 0, 1.0f),
                    new Vector3(0, 0, 0), dimension: marijuana.Dimension, visible: true, frozen: true);
            }

            if (marijuanaObject != null)
            {
                MarijuanaObjects.Add(marijuana.Id, marijuanaObject);
            }
        }
    }
}