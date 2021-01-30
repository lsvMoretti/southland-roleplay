using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using Server.Chat;
using Server.Extensions;
using Server.Models;

namespace Server.Jobs.Taxi
{
    public class CallHandler
    {
        private static int nextCallId = 1;

        public static List<TaxiCall> TaxiCalls = new List<TaxiCall>();

        public static void StartTaxiCall(IPlayer player, Phones phone)
        {
            player.SetData("taxi:phoneUsed", phone.PhoneNumber);
            player.FetchLocation("taxi:startCall");
        }

        public static void OnLocationReturn(IPlayer player, string streetName, string areaName)
        {
            int callId = nextCallId;
            nextCallId++;

            player.Emit("phone:stopPhoneRinging");
            player.SendPhoneMessage("Operator Says: Thanks for calling Downtown Cab Co. Where would you like to go?");
            player.SetData("taxi:onCall", callId);
            player.GetData("taxi:phoneUsed", out string phoneNumber);

            TaxiCalls.Add(new TaxiCall(player, phoneNumber, streetName, areaName, callId));
        }

        public static void OnDestinationReturn(IPlayer player, string destination)
        {
            player.GetData("taxi:onCall", out int onTaxiCall);

            TaxiCall currentCall = TaxiCalls.FirstOrDefault(x => x.Id == onTaxiCall);

            if (currentCall == null)
            {
                player.SendErrorNotification("An error occurred.");
                player.SetData("taxi:onCall", 0);
                player.Emit("phone:stopPhoneRinging");
                player.SendPhoneMessage("You've hung up.");
                player.SendEmoteMessage("puts their phone away.");
                player.SetData("PHONEANSWERED", 0);
                player.SetData("ONPHONEWITH", 0);
                player.SetData("PHONERINGING", false);
                return;
            }

            currentCall.Destination = destination;

            ChatHandler.SendMessageToNearbyPlayers(player, destination, MessageType.LocalPhone);
            player.SendPhoneMessage($"Operator Says: Thanks for calling. The on duty drivers have been alerted!");

            player.SetData("taxi:onCall", 0);
            player.Emit("phone:stopPhoneRinging");
            player.SendPhoneMessage("You've hung up.");
            player.SendEmoteMessage("puts their phone away.");
            player.SetData("PHONEANSWERED", 0);
            player.SetData("ONPHONEWITH", 0);
            player.SetData("PHONERINGING", false);

            foreach (IPlayer? targetPlayer in Alt.Server.GetPlayers())
            {
                bool hasData = targetPlayer.GetData("taxi:onDuty", out bool onDuty);

                if (hasData && onDuty)
                {
                    targetPlayer.SendInfoNotification($"--Incoming Taxi Call--");
                    targetPlayer.SendInfoNotification($"Number: {currentCall.Number}, Destination: {currentCall.Destination}. Location: {currentCall.Street}, {currentCall.Area}.");
                }
            }
        }
    }
}