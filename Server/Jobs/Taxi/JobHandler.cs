using System.Drawing;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using Server.Chat;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.Marker;
using Server.Extensions.TextLabel;
using Blip = Server.Objects.Blip;

namespace Server.Jobs.Taxi
{
    public class JobHandler
    {
        public static Position TaxiJobPosition = new Position(896.4264f, -144.11868f, 76.81323f);

        private static Timer _updateTimer = null;


        public static void InitTaxiJobLocation()
        {
            Blip taxiBlip = new Blip("Downtown Cab Co.", TaxiJobPosition, 198, 5, 0.75f);

            taxiBlip.Add();

            TextLabel taxiLabel = new TextLabel("Downtown Cab Co.\nUse /join to get the job!", TaxiJobPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.DarkOrange));

            taxiLabel.Add();

            _updateTimer = new Timer(600000)
            {
                AutoReset = true
            };
            _updateTimer.Start();

            _updateTimer.Elapsed += _updateTimer_Elapsed;

        }

        private static void _updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool onDuty = false;

            foreach (IPlayer player in Alt.GetAllPlayers())
            {
                player.GetData("taxi:onDuty", out bool taxiDuty);

                if (!taxiDuty) continue;
                onDuty = true;
                break;
            }

            if (onDuty)
            {
                foreach (IPlayer target in Alt.GetAllPlayers().Where(x => x.FetchCharacter() != null).ToList())
                {
                    target.SendAdvertMessage($"We have taxi's driving about! Call 5555 today for a cab!");
                }
            }

        }
    }
}