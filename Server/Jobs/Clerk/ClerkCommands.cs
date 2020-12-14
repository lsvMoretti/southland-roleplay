using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Jobs.Clerk
{
    public class ClerkCommands
    {
        [Command("startclerk", commandType: CommandType.Job, description: "Clerk: Used to start the Clerk job")]
        public static void ClerkCommandStart(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (player.GetClass().ClerkJob > 0)
            {
                player.SendErrorNotification("You are currently doing the clerk job.");
                return;
            }

            List<Models.Clerk> clerkList = Models.Clerk.FetchClerks();

            Models.Clerk lastClerk = null;
            float lastDistance = 5;
            Position playerPosition = player.Position;

            foreach (Models.Clerk clerk in clerkList)
            {
                Position clerkPosition = Models.Clerk.FetchPosition(clerk);

                float distance = clerkPosition.Distance(playerPosition);

                if (distance < lastDistance)
                {
                    lastClerk = clerk;
                    lastDistance = distance;
                }
            }

            if (lastClerk == null)
            {
                player.SendErrorNotification("Your not near a clerk position.");
                return;
            }

            if (!Models.Clerk.FetchPositions(lastClerk).Any())
            {
                player.SendErrorNotification("This job needs it's positions setting!");
                return;
            }

            if (ClerkHandler.OccupiedJobs.Contains(lastClerk.Id))
            {
                player.SendErrorNotification("This job is already occupied!");
                return;
            }

            player.GetClass().ClerkJob = lastClerk.Id;

            player.SendInfoNotification($"You've started the clerk job. You can use /stopclerk when you wish to stop.");

            player.Emit("Clerk:StartJob");

            ClerkHandler.OccupiedJobs.Add(lastClerk.Id);
        }

        [Command("stopclerk", commandType: CommandType.Job, description: "Clerk Job: Used to stop the clerk job")]
        public static void StopClerkJob(IPlayer player)
        {
            ClerkHandler.StopClerkJob(player);
        }
    }
}