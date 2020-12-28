using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AltV.Net.Data;
using Server.Extensions;
using Server.Extensions.TextLabel;
using Server.Models;

namespace Server.Graffiti
{
    public class GraffitiHandler
    {
        public static void LoadGraffitis()
        {
            List<Models.Graffiti> graffitiList = Models.Graffiti.FetchGraffitis();

            Console.WriteLine($"Loading Graffiti's");

            foreach (Models.Graffiti graffiti in graffitiList)
            {
                LoadGraffitiLabel(graffiti);
            }

            Console.WriteLine($"Loaded {TextLabels.Count} Graffiti's");
        }

        public static Dictionary<Models.Graffiti, TextLabel> TextLabels = new Dictionary<Models.Graffiti, TextLabel>();

        public static void LoadGraffitiLabel(Models.Graffiti graffiti)
        {
            Position graffitiPosition = new Position(graffiti.PosX, graffiti.PosY, graffiti.PosZ);

            LsvColor textColor = graffiti.Color switch
            {
                GraffitiColor.Green => new LsvColor(19, 127, 11),
                GraffitiColor.Red => new LsvColor(219, 8, 8),
                GraffitiColor.Yellow => new LsvColor(226, 215, 6),
                GraffitiColor.Purple => new LsvColor(91, 6, 226),
                GraffitiColor.LightBlue => new LsvColor(6, 184, 229),
                _ => new LsvColor(255, 255, 255)
            };

            TextLabel textLabel = new TextLabel(graffiti.Text, graffitiPosition, TextFont.FontChaletComprimeCologne, textColor, 10f);

            textLabel.Add();

            TextLabels.Add(graffiti, textLabel);
        }

        public static void UnloadGraffitiLabel(Models.Graffiti graffiti)
        {
            TextLabel textLabel = TextLabels.FirstOrDefault(x => x.Key == graffiti).Value;

            if (textLabel == null) return;

            textLabel.Remove();

            TextLabels.Remove(graffiti);
        }

        public static Models.Graffiti FetchNearestGraffiti(Position position, float range)
        {
            List<Models.Graffiti> graffitiList = new List<Models.Graffiti>();

            foreach (KeyValuePair<Models.Graffiti, TextLabel> keyValuePair in TextLabels)
            {
                Position graffitiPosition = FetchGraffitiPosition(keyValuePair.Key);

                if (graffitiPosition.Distance(position) <= range) graffitiList.Add(keyValuePair.Key);
            }

            IOrderedEnumerable<Models.Graffiti> orderedList = graffitiList.OrderByDescending(x => FetchGraffitiPosition(x).Distance(position));

            return orderedList.FirstOrDefault();
        }

        public static Position FetchGraffitiPosition(Models.Graffiti graffiti)
        {
            return new Position(graffiti.PosX, graffiti.PosY, graffiti.PosZ);
        }
    }
}