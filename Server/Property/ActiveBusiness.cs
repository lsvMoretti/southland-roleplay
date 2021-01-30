using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Discord;
using Server.Extensions;
using Server.Extensions.Blip;
using Blip = Server.Objects.Blip;

namespace Server.Property
{
    public class ActiveBusiness
    {
        public static Dictionary<int, Blip> ActiveBusinessBlips = new Dictionary<int, Blip>();
        private static Dictionary<int, DateTime> _blipExpiry = new Dictionary<int, DateTime>();

        private static Timer _minuteTimer;

        public static void InitActiveBusinessSystem()
        {
            _minuteTimer = new Timer(60000) { AutoReset = true };
            _minuteTimer.Start();

            _minuteTimer.Elapsed += MinuteTimer_Elapsed;
        }

        private static void MinuteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _minuteTimer.Stop();
            if (!ActiveBusinessBlips.Any())
            {
                _minuteTimer.Start();
                return;
            }

            DateTime now = DateTime.Now;

            foreach (KeyValuePair<int, DateTime> keyValuePair in _blipExpiry)
            {
                if (DateTime.Compare(now, keyValuePair.Value) > 0)
                {
                    Models.Property activeProperty = Models.Property.FetchProperty(keyValuePair.Key);

                    if (activeProperty != null)
                    {
                        RemoveActiveBusiness(activeProperty);
                    }
                }
            }

            _minuteTimer.Start();
        }

        /// <summary>
        /// Sets business to active
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="property"></param>
        /// <returns>True = Success</returns>
        public static bool MakeBusinessActive(Models.Property property)
        {
            if (property == null) return false;

            if (ActiveBusinessBlips.ContainsKey(property.Id)) return false;

            Blip activeBlip = new Blip(property.BusinessName, property.FetchExteriorPosition(), 1, 5, 1, false);

            activeBlip.Add();

            ActiveBusinessBlips.Add(property.Id, activeBlip);
            _blipExpiry.Add(property.Id, DateTime.Now.AddHours(2));

            string message = $"It looks like the {property.BusinessName} is open for business! [Yellow Blip Icon]";

            foreach (IPlayer target in Alt.GetAllPlayers().Where(x => x.FetchCharacter() != null).ToList())
            {
                target.SendAdvertMessage(message);
            }

            SignalR.SendDiscordMessage(795084275090587748, message);

            return true;
        }

        public static void RemoveActiveBusiness(Models.Property property)
        {
            if (!ActiveBusinessBlips.ContainsKey(property.Id)) return;

            var activeBusiness = ActiveBusinessBlips.FirstOrDefault(x => x.Key == property.Id);

            activeBusiness.Value.Remove();

            ActiveBusinessBlips.Remove(property.Id);
            _blipExpiry.Remove(property.Id);
        }
    }
}