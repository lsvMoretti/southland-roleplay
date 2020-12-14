using AltV.Net.Elements.Entities;
using Server.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AltV.Net.Data;
using Newtonsoft.Json;
using Server.Chat;

namespace Server.Property
{
    public class PropertyEntrances
    {
        public static void ExitPropertyContextMenu(IPlayer player, string selectedItem)
        {
            if (player == null) return;

            if (selectedItem == "Exit Menu") return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            Models.Property insideProperty = Models.Property.FetchProperty((int)playerCharacter.Dimension);

            if (insideProperty == null) return;

            using Context context = new Context();

            Models.Property propertyDatabase = context.Property.Find(insideProperty.Id);

            if (propertyDatabase == null) return;

            if (selectedItem == "Unlock" || selectedItem == "Lock")
            {
                propertyDatabase.Locked = !propertyDatabase.Locked;

                context.SaveChanges();

                player.SendInfoNotification(propertyDatabase.Locked
                    ? "You've locked the property."
                    : "You've unlocked the property.");
                return;
            }

            if (selectedItem == "Leave")
            {
                Models.Character playerCharacterDb = context.Character.Find(playerCharacter.Id);

                if (playerCharacterDb == null) return;

                playerCharacterDb.Dimension = insideProperty.ExtDimension;

                context.SaveChanges();

                player.Position = insideProperty.FetchExteriorPosition();

                player.Dimension = (short)insideProperty.ExtDimension;

                player.UnloadIpl(insideProperty.Ipl);

                List<string> propList = JsonConvert.DeserializeObject<List<string>>(insideProperty.PropList);

                if (propList.Any())
                {
                    foreach (string prop in propList)
                    {
                        player.UnloadInteriorProp(prop);
                    }
                }
            }

            
        }

        public static void EnterPropertyContextMenu(IPlayer player, string selectedItem)
        {
            if (player == null) return;

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 4f);

            if (nearestProperty == null) return;

            if (selectedItem == "Exit Menu") return;

            using Context context = new Context();
            Models.Property propertyDatabase = context.Property.Find(nearestProperty.Id);

            if (propertyDatabase == null) return;

            if (selectedItem == "Unlock" || selectedItem == "Lock")
            {
                propertyDatabase.Locked = !propertyDatabase.Locked;

                context.SaveChanges();

                player.SendInfoNotification(propertyDatabase.Locked
                    ? "You've locked the property."
                    : "You've unlocked the property.");
                return;
            }

            if (selectedItem == "Knock")
            {
                player.SendErrorNotification("Coming soon!");
                return;
            }

            if (selectedItem == "Enter")
            {
                Interiors interior = Interiors.InteriorList.FirstOrDefault(x =>
                    x.InteriorName == nearestProperty.InteriorName && x.Ipl == nearestProperty.Ipl);

                if (interior == null)
                {
                    player.SendErrorNotification("An error occurred fetching the interior.");
                    return;
                }

                if (!string.IsNullOrEmpty(interior.Ipl))
                {
                    player.RequestIpl(interior.Ipl);
                }

                player.Position = interior.Position;

                player.Dimension = (short)nearestProperty.Id;

                Models.Character playerCharacter = context.Character.Find(player.FetchCharacterId());

                if (playerCharacter == null) return;

                playerCharacter.Dimension = nearestProperty.Id;

                context.SaveChanges();

                List<string> propList = JsonConvert.DeserializeObject<List<string>>(nearestProperty.PropList);

                if (propList.Any())
                {
                    foreach (string prop in propList)
                    {
                        player.LoadInteriorProp(prop);
                    }
                }
            }

            
        }

        public static bool EnterProperty(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return false;

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 3f);

            if (nearestProperty == null) return false;

            List<string> menuItems = new List<string>();

            if (playerCharacter.Id == nearestProperty.OwnerId)
            {
                menuItems.Add(nearestProperty.Locked ? "Unlock" : "Lock");
            }

            menuItems.Add("Knock");

            if (!nearestProperty.Locked)
            {
                menuItems.Add("Enter");
            }

            menuItems.Add("Exit Menu");

            ContextMenu contextMenu = new ContextMenu("ShowPlayerPropertyEnterMenu", new Vector3(nearestProperty.PosX, nearestProperty.PosY, nearestProperty.PosZ + 0.5f), menuItems);

            ContextMenu.ShowContextMenu(player, contextMenu);

            return true;
        }

        public static bool ExitProperty(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return false;

            Models.Property insideProperty = Models.Property.FetchProperty((int)playerCharacter.Dimension);

            if (insideProperty == null) return false;

            List<string> menuItems = new List<string>();

            if (playerCharacter.Id == insideProperty.OwnerId)
            {
                menuItems.Add(insideProperty.Locked ? "Unlock" : "Lock");
            }

            if (!insideProperty.Locked)
            {
                menuItems.Add("Leave");
            }

            menuItems.Add("Exit Menu");

            ContextMenu contextMenu = new ContextMenu("ShowPlayerPropertyLeaveMenu", new Position(insideProperty.PosX, insideProperty.PosY, insideProperty.PosZ + 0.5f), menuItems);

            ContextMenu.ShowContextMenu(player, contextMenu);

            return true;
        }
    }
}