using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Jobs.Bus;
using Server.Jobs.Delivery;
using Server.Jobs.Taxi;

namespace Server.Jobs
{
    public class JobCommands
    {
        [Command("join", commandType: CommandType.Job, description: "Joins a job")]
        public static void JobCommandJoin(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                player.SendErrorNotification("Unable to fetch your character info.");
                return;
            }

            if (string.IsNullOrWhiteSpace(playerCharacter.JobList))
            {
                playerCharacter.JobList = JsonConvert.SerializeObject(new List<Models.Jobs>());

                context.SaveChanges();
            }

            List<Models.Jobs> jobList = JsonConvert.DeserializeObject<List<Models.Jobs>>(playerCharacter.JobList);

            if (player.Position.Distance(JobHandler.TaxiJobPosition) < 3f)
            {
                if (jobList.Contains(Models.Jobs.TaxiDriver))
                {
                    player.SendErrorNotification("You already have the Taxi Job!");
                    return;
                }

                jobList.Add(Models.Jobs.TaxiDriver);

                playerCharacter.JobList = JsonConvert.SerializeObject(jobList);

                context.SaveChanges();

                

                player.SendInfoNotification("You have taken on the role of a Taxi Driver!");

                return;
            }

            if (player.Position.Distance(DeliveryHandler.JobPosition) < 3f)
            {
                if (jobList.Contains(Models.Jobs.DeliveryJob))
                {
                    player.SendErrorNotification("You already have the Delivery Job!");
                    return;
                }

                jobList.Add(Models.Jobs.DeliveryJob);

                playerCharacter.JobList = JsonConvert.SerializeObject(jobList);

                context.SaveChanges();
                
                player.SendInfoNotification($"You have taken on the role of a Delivery Driver!");
                return;
            }

            if (player.Position.Distance(BusHandler.CommandPosition) < 3f)
            {
                if (jobList.Contains(Models.Jobs.BusDriver))
                {
                    player.SendErrorNotification("You already have the Bus Driver Job!");
                    return;
                }

                jobList.Add(Models.Jobs.BusDriver);

                playerCharacter.JobList = JsonConvert.SerializeObject(jobList);

                context.SaveChanges();
                
                player.SendInfoNotification($"You have taken on the role of a Bus Driver!");
                return;
            }
        }
    }
}