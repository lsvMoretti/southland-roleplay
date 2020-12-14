using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Character.Clothing;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Groups.Fire
{
    public class FireCommands
    {
        [Command("breathing", commandType: CommandType.Faction, description: "FD: Used to toggle breathing apparatus")]
        public static void FireBreathingCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendPermissionError();
                return;
            }

            if (Faction.FetchFaction(playerCharacter.ActiveFaction).SubFactionType != SubFactionTypes.Medical || !playerCharacter.FactionDuty)
            {
                player.SendPermissionError();
                return;
            }

            if (playerCharacter.DutyStatus != 2)
            {
                player.SendErrorNotification("You must be wearing a fire persons outfit!");
                return;
            }

            bool hasDutyClothing = player.GetData("FACTION:DUTYCLOTHING", out string dutyClothingJson);
            bool hasAccessoryClothing = player.GetData("FACTION:DUTYACCESSORY", out string dutyAccessoryJson);

            List<ClothesData> clothesData = JsonConvert.DeserializeObject<List<ClothesData>>(dutyClothingJson);

            List<AccessoryData> accessoryData = JsonConvert.DeserializeObject<List<AccessoryData>>(dutyAccessoryJson);

            if (!hasDutyClothing || !hasAccessoryClothing)
            {
                player.SendErrorNotification("An error occurred fetching your duty clothing.");
                return;
            }

            ClothesData underShirt = clothesData.FirstOrDefault(x => x.slot == 8);
            AccessoryData hatData = accessoryData.FirstOrDefault(x => x.slot == 0);

            if (underShirt == null || hatData == null)
            {
                player.SendErrorNotification("An error occurred fetching your clothing data.");
                return;
            }

            if (playerCharacter.Sex == 0)
            {
                // Male
                if (underShirt.drawable == 151)
                {
                    underShirt.drawable = 0;
                    underShirt.texture = 0;
                    hatData.drawable = 138;
                    hatData.texture = 0;
                }
                else
                {
                    underShirt.drawable = 151;
                    underShirt.texture = 0;
                    hatData.drawable = 137;
                    hatData.texture = 0;
                }
            }
            else
            {
                // Female
                if (underShirt.drawable == 187)
                {
                    underShirt.drawable = 0;
                    underShirt.texture = 0;
                    hatData.drawable = 137;
                    hatData.texture = 0;
                }
                else
                {
                    underShirt.drawable = 187;
                    underShirt.texture = 0;
                    hatData.drawable = 136;
                    hatData.texture = 0;
                }
            }

            player.SetData("FACTION:DUTYCLOTHING", JsonConvert.SerializeObject(clothesData));
            player.SetData("FACTION:DUTYACCESSORY", JsonConvert.SerializeObject(accessoryData));
            Clothes.LoadClothes(player, clothesData, accessoryData);

            player.SendInfoNotification($"You've toggled the breathing apparatus!");
        }

        [Command("heal", onlyOne: true, commandType: CommandType.Faction, description: "FD: Used to heal a player")]
        public static void FireCommandHealPlayer(IPlayer player, string args = "")
        {
            
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendPermissionError();
                return;
            }

            if (Faction.FetchFaction(playerCharacter.ActiveFaction).SubFactionType != SubFactionTypes.Medical || !playerCharacter.FactionDuty)
            {
                player.SendPermissionError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/heal [NameOrId]");
                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Target not found.");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Target not spawned.");
                return;
            }

            if (player.Position.Distance(targetPlayer.Position) > 5f && player.Dimension != targetPlayer.Dimension)
            {
                player.SendErrorNotification("Your not near the player.");
                return;
            }

            targetPlayer.Health = 200;

            player.SendNotification($"~g~You've healed {targetPlayer.GetClass().Name}.");
        }
    }
}