using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Character.Tattoo;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Property.Stores
{
    public class TattooStore
    {
        /*private const int headPrice = 200;
        private const int torsoPrice = 800;
        private const int armPrice = 500;
        private const int legPrice = 500;*/

        private const int headPrice = 100;
        private const int torsoPrice = 100;
        private const int armPrice = 100;
        private const int legPrice = 100;

        public static void ShowTattooMenu(IPlayer player)
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>()
            {
                new NativeMenuItem("Head", $"~g~{headPrice:C0}"),
                new NativeMenuItem("Torso", $"~g~{torsoPrice:C0}"),
                new NativeMenuItem("Left Arm", $"~g~{armPrice:C0}"),
                new NativeMenuItem("Right Arm", $"~g~{armPrice:C0}"),
                new NativeMenuItem("Left Leg", $"~g~{legPrice:C0}"),
                new NativeMenuItem("Right Leg", $"~g~{legPrice:C0}"),
            };

            NativeMenu menu = new NativeMenu("store:tattoo:MainMenuSelect", "Tattoos", "Select the body area.", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            int selectedZone = option switch
            {
                "Torso" => 0,
                "Head" => 1,
                "Left Arm" => 2,
                "Right Arm" => 3,
                "Left Leg" => 4,
                "Right Leg" => 5,
                _ => 0
            };

            player.SetData("tattoo:SelectedZone", selectedZone);

            Dictionary<TattooData, string> tattooList = TattooHandler.TattooList;

            List<KeyValuePair<TattooData, string>> selectedTattoos = tattooList.Where(x => x.Key.ZoneID == selectedZone).ToList();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (KeyValuePair<TattooData, string> selectedTattoo in selectedTattoos)
            {
                menuItems.Add(new NativeMenuItem(selectedTattoo.Key.LocalizedName));
            }

            NativeMenu menu = new NativeMenu("store:tattoo:TattooItemSelect", "Tattoos", "Select the Tattoo to preview", menuItems)
            {
                ItemChangeTrigger =  "store:tattoo:TattooItemChange",
                PassIndex = true,
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnTattooItemChange(IPlayer player, int index, string itemText)
        {
            if (itemText == "Close") return;

            player.Emit("ClearTattoos");

            player.GetData("tattoo:SelectedZone", out int selectedZone);

            Dictionary<TattooData, string> tattooList = TattooHandler.TattooList;

            List<KeyValuePair<TattooData, string>> selectedTattoos = tattooList.Where(x => x.Key.ZoneID == selectedZone).ToList();

            KeyValuePair<TattooData, string> selectedTattoo = selectedTattoos[index];
            
            string[] collection = selectedTattoo.Value.Split('.');
            uint collectionHash = Alt.Hash(collection[0]);

            string overlay = player.GetClass().IsMale ? selectedTattoo.Key.HashNameMale : selectedTattoo.Key.HashNameFemale;

            player.Emit("loadTattooData", collection[0], overlay);
        }

        public static void OnTattooSelect(IPlayer player, string option)
        {
            if (option == "Close")
            {
                player.LoadCharacterCustomization();
                return;
            }

            player.GetData("tattoo:SelectedZone", out int selectedZone);

            float playerMoney = player.FetchCharacter().Money;

            var tattooCost = selectedZone switch
            {
                1 =>
                // Head
                headPrice,
                0 =>
                // Torso
                torsoPrice,
                2 =>
                // Left Arm
                armPrice,
                3 =>
                // Right Arm
                armPrice,
                4 =>
                // Left Leg
                legPrice,
                5 =>
                // Right Leg
                legPrice,
                _ => 500
            };

            if (playerMoney < tattooCost)
            {
                player.SendErrorNotification($"You don't have the funds required for this. {tattooCost:C0}.");
                return;
            }

            Dictionary<TattooData, string> tattooList = TattooHandler.TattooList;

            KeyValuePair<TattooData, string> selectedTattoo =
                tattooList.FirstOrDefault(x => x.Key.ZoneID == selectedZone && x.Key.LocalizedName == option);

            string[] collection = selectedTattoo.Value.Split('.');
            uint collectionHash = Alt.Hash(collection[0]);

            string overlay = player.GetClass().IsMale ? selectedTattoo.Key.HashNameMale : selectedTattoo.Key.HashNameFemale;

            player.Emit("loadTattooData", collectionHash, overlay);

            player.SetData("tattoo:localName", selectedTattoo.Key.LocalizedName);
            player.SetData("tattoo:collectionHash", collectionHash);
            player.SetData("tattoo:overlay", overlay);
            player.Emit("loadTattooData", collectionHash, overlay);

            NativeUi.ShowYesNoMenu(player, "store:tattoo:ConfirmPurchase", "Tattoos", "Would you like to purchase?");
        }

        public static void OnConfirmPurchase(IPlayer player, string option)
        {
            TattooHandler.LoadTattoos(player);

            if (option == "No")
            {
                player.LoadCharacterCustomization();
                return;
            }

            player.GetData("tattoo:SelectedZone", out int selectedZone);
            player.GetData("tattoo:localName", out string localizedName);
            player.GetData("tattoo:collectionHash", out uint collectionHash);
            player.GetData("tattoo:overlay", out uint overlay);

            var tattooCost = selectedZone switch
            {
                1 =>
                // Head
                headPrice,
                0 =>
                // Torso
                torsoPrice,
                2 =>
                // Left Arm
                armPrice,
                3 =>
                // Right Arm
                armPrice,
                4 =>
                // Left Leg
                legPrice,
                5 =>
                // Right Leg
                legPrice,
                _ => 500
            };

            player.RemoveCash(tattooCost);

            KeyValuePair<TattooData, string> selectedTattoo = TattooHandler.TattooList.FirstOrDefault(x =>
                x.Key.LocalizedName == localizedName && x.Key.ZoneID == selectedZone);

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            List<TattooData> tattooList = JsonConvert.DeserializeObject<List<TattooData>>(playerCharacter.TattooJson);

            tattooList.Add(selectedTattoo.Key);

            playerCharacter.TattooJson = JsonConvert.SerializeObject(tattooList);

            context.SaveChanges();

            

            player.SendInfoNotification($"You've added the {localizedName} to your body for {tattooCost:C0}.");

            player.GetData("INSTOREID", out int storeId);

            Models.Property property = Models.Property.FetchProperty(storeId);

            property?.AddToBalance(tattooCost);

            TattooHandler.LoadTattoos(player);
        }

        [Command("removetattoo")]
        public static void CommandRemoveTattoo(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Models.Property property = Models.Property.FetchNearbyProperty(player, 3f);

            if (property == null)
            {
                List<Models.Property> properties = Models.Property.FetchProperties();

                foreach (Models.Property iProperty in properties)
                {
                    List<PropertyInteractionPoint> interactionPoints =
                        JsonConvert.DeserializeObject<List<PropertyInteractionPoint>>(iProperty.InteractionPoints);

                    if (!interactionPoints.Any()) continue;

                    foreach (var propertyInteractionPoint in interactionPoints)
                    {
                        Position interactionPointPos = new Position(propertyInteractionPoint.PosX, propertyInteractionPoint.PosY, propertyInteractionPoint.PosZ);

                        if (player.Position.Distance(interactionPointPos) <= 3f)
                        {
                            property = iProperty;
                            break;
                        }
                    }
                }

                if (property == null)
                {
                    player.SendErrorNotification($"You're not near a property.");
                    return;
                }
            }

            if (property.PropertyType != PropertyType.Tattoo)
            {
                player.SendErrorNotification("You're not at a Tattoo Parlor!");
                return;
            }

            player.SetData("INSTOREID", property.Id);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>()
            {
                new NativeMenuItem("Head"),
                new NativeMenuItem("Torso"),
                new NativeMenuItem("Left Arm"),
                new NativeMenuItem("Right Arm"),
                new NativeMenuItem("Left Leg"),
                new NativeMenuItem("Right Leg"),
            };

            NativeMenu menu = new NativeMenu("tattoo:store:RemoveTattooMainMenu", "Tattoos", "Select a Zone to show tattoos.", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnRemoveMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            var selectedZone = option switch
            {
                "Head" => 1,
                "Torso" => 0,
                "Left Arm" => 2,
                "Right Arm" => 3,
                "Left Leg" => 4,
                "Right Leg" => 5,
                _ => 0
            };

            List<TattooData> tattooList =
                JsonConvert.DeserializeObject<List<TattooData>>(player.FetchCharacter().TattooJson).Where(x => x.ZoneID == selectedZone).ToList();

            if (!tattooList.Any())
            {
                player.SendErrorNotification("You don't have any tattoo's in this zone.");
                return;
            }

            player.SetData("tattoo:SelectedRemoveZone", selectedZone);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (TattooData tattooData in tattooList)
            {
                menuItems.Add(new NativeMenuItem(tattooData.LocalizedName));
            }

            NativeMenu menu = new NativeMenu("store:tattoo:OnRemoveTattooSelect", "Tattoos", "Select a tattoo to remove", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnRemoveTattooSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            List<TattooData> tattooList = JsonConvert.DeserializeObject<List<TattooData>>(playerCharacter.TattooJson);

            player.GetData("tattoo:SelectedRemoveZone", out int selectedZone);

            TattooData selectedTattoo = tattooList.FirstOrDefault(x => x.ZoneID == selectedZone && x.LocalizedName == option);

            tattooList.Remove(selectedTattoo);

            playerCharacter.TattooJson = JsonConvert.SerializeObject(tattooList);

            context.SaveChanges();

            

            TattooHandler.LoadTattoos(player);

            player.SendInfoNotification($"You have removed {option} from your body! This was free.");
        }
    }
}