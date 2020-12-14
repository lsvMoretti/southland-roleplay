using System;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Character.Scenes
{
    public class SceneCommands
    {
        [Command("createscene", onlyOne: true, commandType: CommandType.Character,
            description: "Scenes: Used to create a scene at your location")]
        public static void CommandCreateScene(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/createscene [Text]");
                return;
            }

            if (!player.IsSpawned()) return;

            bool hasCooldownData = player.GetData("Scenes:NextSceneTime", out DateTime nextTime);

            if (hasCooldownData && DateTime.Compare(DateTime.Now, nextTime) <= 0)
            {
                player.SendErrorNotification("You have a active cool down.");
                return;
            }

            if (args.Length < 3)
            {
                player.SendErrorNotification("Input a longer text.");
                return;
            }

            LsvColor color = new LsvColor(194, 162, 218);

            SceneHandler.CreateScene(args, player.Position, color, player.GetClass().CharacterId, player.Dimension);

            player.SendInfoNotification($"You've created a new scene at this location.");

            player.SetData("Scenes:NextSceneTime", DateTime.Now.AddMinutes(5));

            Logging.AddToCharacterLog(player, $"has created a new scene with the text: {args}");
        }

        [Command("removescene", commandType: CommandType.Character,
            description: "Scenes: Used to remove a scene label from nearby.")]
        public static void CommandRemoveScene(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Position playerPosition = player.Position;

            Scene nearestScene = SceneHandler.FetchNearestScene(playerPosition);

            if (nearestScene == null)
            {
                player.SendErrorNotification("There is no scene near you.");
                return;
            }

            if (nearestScene.Position.Distance(playerPosition) > 5)
            {
                player.SendErrorNotification("There is no scene near you.");
                return;
            }

            if (player.GetClass().CharacterId != nearestScene.CharacterId)
            {
                if (player.FetchAccount().AdminLevel < AdminLevel.Administrator)
                {
                    if (!player.FetchAccount().Developer)
                    {
                        player.SendErrorNotification("You can't remove this scene.");
                        return;
                    }
                }
            }

            bool sceneRemoved = SceneHandler.RemoveScene(nearestScene);

            if (!sceneRemoved)
            {
                player.SendErrorNotification("An error occurred removing the scene.");
                return;
            }

            player.SendInfoNotification($"You've removed the nearest scene to you.");
        }
    }
}