using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Discord;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Graffiti
{
    public class GraffitiCommands
    {
        [Command("graffiti", onlyOne: true, commandType: CommandType.Character,
            description: "Graffiti: Used to create un-sightly graffiti's. /graffiti [Text]")]
        public static void GraffitiCommand(IPlayer player, string text = "")
        {
            if (text == "")
            {
                player.SendSyntaxMessage("/graffiti [Text]");
                return;
            }

            if (!player.IsSpawned())
            {
                player.SendLoginError();
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (!playerInventory.HasItem("ITEM_SPRAYCAN"))
            {
                player.SendErrorNotification("You must have a spray can for this!");
                return;
            }

            if (DateTime.Compare(DateTime.Now, player.FetchCharacter().NextGraffitiTime) <= 0)
            {
                player.SendErrorNotification("You have an active cooldown.");
                return;
            }

            if (text.Length < 3)
            {
                player.SendErrorNotification("Text length too short.");
                return;
            }

            string[] splitString = text.Split(' ');

            bool containsIllegal = false;

            foreach (string s in splitString)
            {
                bool isMatch = Regex.IsMatch(s, @"^[a-zA-Z0-9_!/]+$");

                if (!isMatch)
                {
                    containsIllegal = true;
                }
            }

            if (containsIllegal)
            {
                player.SendErrorNotification("Must be letters or numbers.");
                return;
            }

            player.SetData("GRAFFITITEXT", text);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Green"),
                new NativeMenuItem("Red"),
                new NativeMenuItem("Yellow"),
                new NativeMenuItem("Purple"),
                new NativeMenuItem("Light Blue")
            };

            NativeMenu menu = new NativeMenu("graffiti:SelectColor", "Graffiti", "Select a color", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnGraffitiColorSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("GRAFFITITEXT", out string graffitiText);

            GraffitiColor graffitiColor = option switch
            {
                "Green" => GraffitiColor.Green,
                "Red" => GraffitiColor.Red,
                "Yellow" => GraffitiColor.Yellow,
                "Purple" => GraffitiColor.Purple,
                "Light Blue" => GraffitiColor.LightBlue,
                _ => GraffitiColor.Green
            };

            player.SetData("graffiti:selectedColor", graffitiColor);

            player.FetchForwardPosition("graffiti:FetchForwardPosition", 0.5);
        }

        public static void ReturnGraffitiPosition(IPlayer player, float x, float y, float z)
        {
            player.GetData("graffiti:selectedColor", out GraffitiColor graffitiColor);

            player.GetData("GRAFFITITEXT", out string graffitiText);

            Inventory.Inventory playerInventory = player.FetchInventory();

            InventoryItem sprayCanItem = playerInventory.GetItem("ITEM_SPRAYCAN");

            int sprays = Convert.ToInt32(sprayCanItem.ItemValue);

            playerInventory.RemoveItem(sprayCanItem);

            if (sprays > 1)
            {
                sprayCanItem.ItemValue = (sprays - 1).ToString();
                playerInventory.AddItem(sprayCanItem);
            }

            Models.Graffiti newGraffiti = Models.Graffiti.CreateGraffiti(player, graffitiText, graffitiColor, new Position(x, y, z));

            GraffitiHandler.LoadGraffitiLabel(newGraffiti);

            player.SendInfoNotification($"You've created a graffiti saying {graffitiText}.");

            Logging.AddToCharacterLog(player, $"has created a new graffiti with the text {newGraffiti.Text}. Id: {newGraffiti.Id}.");
        }

        [Command("cleantag", commandType: CommandType.Character,
            description: "Graffiti: Used to clean up the messy tags.")]
        public static void GraffitiCommandCleanTag(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (!playerInventory.HasItem("ITEM_GRAFFITISTRIPPER"))
            {
                player.SendErrorNotification("You don't have any graffiti stripper.");
                return;
            }

            Models.Graffiti nearestGraffiti = GraffitiHandler.FetchNearestGraffiti(player.Position, 5f);

            if (nearestGraffiti == null)
            {
                player.SendErrorNotification("You're not near a graffiti!");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();
            Models.Character graffitiCharacter = Models.Character.GetCharacter(nearestGraffiti.CharacterId);

            if (playerCharacter.GraffitiCleanCount > 4)
            {
                player.SendErrorNotification("You can only remove 4 graffiti's per hour.");
                return;
            }

            GraffitiHandler.UnloadGraffitiLabel(nearestGraffiti);
            Models.Graffiti.DeleteGraffiti(nearestGraffiti);

            InventoryItem stripperItem = playerInventory.GetItem("ITEM_GRAFFITISTRIPPER");

            int spraysLeft = Convert.ToInt32(stripperItem.ItemValue);

            playerInventory.RemoveItem(stripperItem);

            if (spraysLeft > 1)
            {
                stripperItem.ItemValue = (spraysLeft - 1).ToString();
                playerInventory.AddItem(stripperItem);
            }

            int moneyEarned = 10;

            using Context context = new Context();

            var characterDatabase = context.Character.Find(player.GetClass().CharacterId);

            if (characterDatabase == null)
            {
                player.SendErrorNotification("There was an error fetching your character information.");
                return;
            }

            characterDatabase.GraffitiCleanCount += 1;

            if (playerCharacter.OwnerId != graffitiCharacter.OwnerId)
            {
                characterDatabase.PaydayAmount += moneyEarned;

                player.SendInfoNotification($"You've cleaned up the tag and earned {moneyEarned:C}. This has been added to your paycheck.");

                context.SaveChanges();
                return;
            }

            player.SendInfoNotification($"You've cleaned up the tag. As you have done this, it's free!");

            context.SaveChanges();
        }
    }
}