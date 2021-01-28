using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Groups.EUP
{
    public class EupMenu
    {
        #region LSPD Male

        public static List<EupOutfit> MalePoliceOutfits = new List<EupOutfit>
        {
            new EupOutfit("Class A - Long", new []{0, 0}, new []{35, 0}, new []{25, 0},  new []{378, 0}),
            new EupOutfit("Class A Traffic - Long", new []{0, 0}, new []{35, 0}, new []{25, 0},  new []{378, 1}),
            new EupOutfit("Class C - Short", new []{0, 0}, new []{35, 0}, new []{25, 0},  new []{375, 0}),
            new EupOutfit("Class C - Long", new []{1, 0}, new []{35, 0}, new []{25, 0},  new []{377, 0}),
            new EupOutfit("Traffic - Short", new []{0, 0}, new []{35, 0}, new []{25, 0},  new []{375, 1}),
            new EupOutfit("Traffic - Long", new []{1, 0}, new []{35, 0}, new []{25, 0},  new []{377, 1}),
            new EupOutfit("Police Jacket", new []{0, 0}, new []{35, 0}, new []{25, 0},  new []{374, 0}),
            new EupOutfit("Traffic Jacket", new []{0, 0}, new []{35, 0}, new []{25, 0},  new []{374, 1}),
            new EupOutfit("SWAT", new []{1, 0}, new []{130, 0}, new []{25, 0},  new []{369, 0}),
            new EupOutfit("Police Tee", new []{0, 0}, new []{35, 0}, new []{25, 0},  new []{380, 0}),
            new EupOutfit("SWAT Tee", new []{1, 0}, new []{130, 0}, new []{25, 0},  new []{380, 1}),
        };

        public static List<EupProp> MalePoliceProps = new List<EupProp>
        {
            new EupProp("No Badge", EupPropType.Clothing, 5, new []{85, -1}),
            new EupProp("No Hat", EupPropType.Prop, 0, new []{8, 0}),
            new EupProp("No Vest", EupPropType.Clothing, 9, new []{56, -1}),
            new EupProp("No Belt", EupPropType.Clothing, 8, new []{169, -1}),
            new EupProp("LSPD Badge", EupPropType.Clothing,5, new []{89, 0}),
            new EupProp("Officer Hat", EupPropType.Prop,0, new []{153, 0}),
            new EupProp("LSPD ASU Helmet", EupPropType.Prop,0, new []{160, 1}),
            new EupProp("LSPD ASU Helmet Visor Down", EupPropType.Prop,0, new []{161, 1}),
            new EupProp("Motorcycle Helmet", EupPropType.Prop,0, new []{162, 1}),
            new EupProp("LSPD Hi-Vis", EupPropType.Clothing,9, new []{56, 0}),
            new EupProp("LSPD Traffic Hi-Vis", EupPropType.Clothing,9, new []{56, 4}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{178, 0}),
            new EupProp("Duty Belt 2", EupPropType.Clothing,8, new []{179, 0}),
            new EupProp("Duty Belt 3", EupPropType.Clothing,8, new []{181, 0}),
            new EupProp("Duty Belt 4", EupPropType.Clothing,8, new []{182, 0}),
            new EupProp("Duty Belt 5", EupPropType.Clothing,8, new []{183, 0}),
            new EupProp("Duty Belt 6", EupPropType.Clothing,8, new []{185, 0}),
            new EupProp("Duty Belt 7", EupPropType.Clothing,8, new []{186, 0}),
            new EupProp("Duty Belt 8", EupPropType.Clothing,8, new []{187, 0}),
            new EupProp("Duty Belt 9", EupPropType.Clothing,8, new []{188, 0}),
            new EupProp("Duty Belt 10", EupPropType.Clothing,8, new []{189, 0}),
            new EupProp("Duty Belt 11", EupPropType.Clothing,8, new []{190, 0}),
            new EupProp("Duty Belt 12", EupPropType.Clothing,8, new []{191, 0}),
            new EupProp("Duty Belt 13", EupPropType.Clothing,8, new []{192, 0}),
            new EupProp("Duty Belt 14", EupPropType.Clothing,8, new []{193, 0}),
            new EupProp("Radio", EupPropType.Clothing,8, new []{194, 0}),
            new EupProp("Duty Belt 15", EupPropType.Clothing,8, new []{195, 0}),
            new EupProp("Duty Belt 16", EupPropType.Clothing,8, new []{196, 0}),
            new EupProp("LSPD Armor Vest", EupPropType.Clothing,9, new []{57, 0}),
            new EupProp("SWAT Helmet", EupPropType.Prop,0, new []{154, 0}),
            new EupProp("SWAT Helmet With Mic", EupPropType.Prop,0, new []{157, 0}),
            new EupProp("SWAT Vest", EupPropType.Clothing,9, new []{58, 0}),
            new EupProp("LSPD Neck Id", EupPropType.Clothing,9, new []{60, 0}),
            new EupProp("LSPD Belt Badge & Cuffs", EupPropType.Clothing,9, new []{61, 0}),
            new EupProp("LSPD Belt Badge", EupPropType.Clothing,5, new []{91, 0}),
            new EupProp("LSPD Neck Badge", EupPropType.Clothing,5, new []{92, 0}),
            new EupProp("Two Mags On Belt", EupPropType.Clothing,5, new []{90, 0}),
            new EupProp("Empty Holster", EupPropType.Clothing,7, new []{152, 0}),
            new EupProp("Leg Holster", EupPropType.Clothing,7, new []{153, 0}),
            new EupProp("Holster w/ Mags", EupPropType.Clothing,7, new []{154, 0}),
            new EupProp("Holster w/ Gun & Mags", EupPropType.Clothing,7, new []{155, 0}),
            new EupProp("Holster w/ Gun", EupPropType.Clothing,7, new []{156, 0}),
            new EupProp("Tie & US Badge", EupPropType.Clothing,7, new []{157, 0}),
            new EupProp("Loose Tie", EupPropType.Clothing,7, new []{158, 0}),
            new EupProp("Gang Unit Vest", EupPropType.Clothing,9, new []{63, 0}),
        };

        public static List<EupProp> MalePoliceRanks = new List<EupProp>
        {
            new EupProp("No Rank", EupPropType.Clothing,10, new []{81, -1}),
            new EupProp("Police Officer III", EupPropType.Clothing,10, new []{82, 0}),
            new EupProp("Police Officer III+I", EupPropType.Clothing,10, new []{82, 1}),
            new EupProp("Sergeant I", EupPropType.Clothing,10, new []{82, 2}),
            new EupProp("Sergeant II", EupPropType.Clothing,10, new []{82, 3}),
            new EupProp("Detective I", EupPropType.Clothing,10, new []{82, 4}),
            new EupProp("Detective II", EupPropType.Clothing,10, new []{82, 5}),
            new EupProp("Detective III", EupPropType.Clothing,10, new []{82, 6}),
            new EupProp("Lieutenant", EupPropType.Clothing,10, new []{85, 0}),
            new EupProp("Captain", EupPropType.Clothing,10, new []{ 85, 1}),
            new EupProp("Commander", EupPropType.Clothing,10, new []{ 85, 2}),
            new EupProp("Deputy Chief", EupPropType.Clothing,10, new []{ 85, 3}),
            new EupProp("Assistant Chief", EupPropType.Clothing,10, new []{ 85, 4}),
            new EupProp("Chief of Police", EupPropType.Clothing,10, new []{85, 5}),
        };

        #endregion LSPD Male

        #region LSPD Female

        public static List<EupOutfit> FemalePoliceOutfits = new List<EupOutfit>
        {
            new EupOutfit("Class A - Long", new []{3, 0}, new []{34, 0}, new []{25, 0},  new []{385, 0}),
            new EupOutfit("Class A - Long Traffic", new []{3, 0}, new []{34, 0}, new []{25, 0},  new []{385, 1}),
            new EupOutfit("Class C - Short", new []{14, 0}, new []{34, 0}, new []{25, 0},  new []{383, 0}),
            new EupOutfit("Class C - Short Traffic", new []{14, 0}, new []{34, 0}, new []{25, 0},  new []{383, 1}),
            new EupOutfit("Class C - Long", new []{3, 0}, new []{34, 0}, new []{25, 0},  new []{384, 0}),
            new EupOutfit("Class C - Long Traffic", new []{3, 0}, new []{34, 0}, new []{25, 0},  new []{384, 1}),
            new EupOutfit("LSPD Tee", new []{3, 0}, new []{34, 0}, new []{25, 0},  new []{382, 0}),
            new EupOutfit("SWAT", new []{14, 0}, new []{132, 0}, new []{25, 0},  new []{381, 0}),
            new EupOutfit("SWAT Tee", new []{14, 0}, new []{127, 0}, new []{25, 0},  new []{382, 1}),
        };

        public static List<EupProp> FemalePoliceProps = new List<EupProp>
        {
            new EupProp("No Badge", EupPropType.Clothing, 5, new []{85, -1}),
            new EupProp("No Hat", EupPropType.Prop, 0, new []{-1, 0}),
            new EupProp("No Vest", EupPropType.Clothing, 9, new []{56, -1}),
            new EupProp("No Belt", EupPropType.Clothing, 8, new []{169, -1}),
            new EupProp("LSPD Badge", EupPropType.Clothing,5, new []{89, 0}),
            new EupProp("Belt & Badge", EupPropType.Clothing,5, new []{90, 0}),
            new EupProp("Officer Hat", EupPropType.Prop,0, new []{152, 0}),
            new EupProp("LSPD ASU Helmet", EupPropType.Prop,0, new []{156, 1}),
            new EupProp("LSPD ASU Helmet Visor", EupPropType.Prop,0, new []{157, 1}),
            new EupProp("Motorcycle Helmet", EupPropType.Prop,0, new []{158, 1}),
            new EupProp("LSPD Hi-Vis", EupPropType.Clothing,9, new []{56, 0}),
            new EupProp("Traffic Hi-Vis", EupPropType.Clothing,9, new []{56, 8}),
            new EupProp("Shoulder Mic", EupPropType.Clothing,9, new []{57, 0}),
            new EupProp("Belt & Mic", EupPropType.Clothing,9, new []{58, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{216, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{217, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{218, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{220, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{221, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{223, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{224, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{225, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{226, 0}),
            new EupProp("Duty Belt 1", EupPropType.Clothing,8, new []{227, 0}),
            new EupProp("Radio", EupPropType.Clothing,8, new []{228, 0}),
            new EupProp("Underarm Holster", EupPropType.Clothing,8, new []{229, 0}),
            new EupProp("Underarm w/o Gun Holster", EupPropType.Clothing,8, new []{231, 0}),
            new EupProp("Police Vest", EupPropType.Clothing,8, new []{230, 0}),
            new EupProp("LSPD Armor Vest", EupPropType.Clothing,9, new []{58, 4}),
            new EupProp("SWAT Helmet", EupPropType.Prop,0, new []{150, 0}),
            new EupProp("SWAT Vest", EupPropType.Clothing,9, new []{59, 0}),
        };

        public static List<EupProp> FemalePoliceRanks = new List<EupProp>
        {
            new EupProp("No Rank", EupPropType.Clothing,10, new []{81, -1}),
            new EupProp("Police Officer III", EupPropType.Clothing,10, new []{94, 0}),
            new EupProp("Police Officer III+I", EupPropType.Clothing,10, new []{ 94, 1}),
            new EupProp("Sergeant I", EupPropType.Clothing,10, new []{ 94, 2}),
            new EupProp("Sergeant II", EupPropType.Clothing,10, new []{ 94, 3}),
            new EupProp("Detective I", EupPropType.Clothing,10, new []{ 94, 4}),
            new EupProp("Detective II", EupPropType.Clothing,10, new []{ 94, 5}),
            new EupProp("Detective III", EupPropType.Clothing,10, new []{ 94, 6}),
            new EupProp("Lieutenant", EupPropType.Clothing,10, new []{93, 0}),
            new EupProp("Captain", EupPropType.Clothing,10, new []{ 93, 1}),
            new EupProp("Commander", EupPropType.Clothing,10, new []{ 93, 2}),
            new EupProp("Deputy Chief", EupPropType.Clothing,10, new []{ 93, 3}),
            new EupProp("Assistant Chief", EupPropType.Clothing,10, new []{ 93, 4}),
            new EupProp("Chief of Police", EupPropType.Clothing,10, new []{ 93, 5}),
        };

        #endregion LSPD Female

        [Command("eup", commandType: CommandType.Faction, description: "Display EUP Outfit Menu")]
        public static void CommandEup(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (player.IsLeo(true))
            {
                ShowPoliceEupMenu(player);
                return;
            }

            player.SendPermissionError();
        }

        private static void ShowPoliceEupMenu(IPlayer player)
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (player.FetchCharacter().Sex == 0)
            {
                // Show Male Outfit

                menuItems.Add(new NativeMenuItem("Props"));
                menuItems.Add(new NativeMenuItem("Ranks"));

                foreach (EupOutfit malePoliceOutfit in MalePoliceOutfits)
                {
                    menuItems.Add(new NativeMenuItem(malePoliceOutfit.Name));
                }

                NativeMenu menu = new NativeMenu("EupMenu:Police:Male", "EUP", "Select an Outfit", menuItems);

                NativeUi.ShowNativeMenu(player, menu, true);

                return;
            }

            if (player.FetchCharacter().Sex == 1)
            {
                // Female
                menuItems.Add(new NativeMenuItem("Props"));
                menuItems.Add(new NativeMenuItem("Ranks"));

                foreach (EupOutfit femaleOutfit in FemalePoliceOutfits)
                {
                    menuItems.Add(new NativeMenuItem(femaleOutfit.Name));
                }

                NativeMenu menu = new NativeMenu("EupMenu:Police:Female", "EUP", "Select an Outfit", menuItems);

                NativeUi.ShowNativeMenu(player, menu, true);

                return;
            }
        }

        public static void OnMalePoliceSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Props")
            {
                ShowMalePoliceProps(player);
                return;
            }

            if (option == "Ranks")
            {
                ShowMalePoliceRanks(player);
                return;
            }

            EupOutfit selectedOutfit = MalePoliceOutfits.FirstOrDefault(s => s.Name == option);

            if (selectedOutfit == null)
            {
                player.SendErrorNotification("An error occurred");
                return;
            }

            player.SetClothes(3, selectedOutfit.Torso[0], selectedOutfit.Torso[1]);
            player.SetClothes(4, selectedOutfit.Legs[0], selectedOutfit.Legs[1]);
            player.SetClothes(6, selectedOutfit.Shoes[0], selectedOutfit.Shoes[1]);
            player.SetClothes(11, selectedOutfit.Top[0], selectedOutfit.Top[1]);

            if (selectedOutfit.Name == "SWAT")
            {
                //player.SetClothes(10, 78, 0);
            }

            if (selectedOutfit.Name == "Police Jacket")
            {
                player.SetClothes(8, 180, 0);
            }

            player.SendNotification($"~y~You've changed outfits to {selectedOutfit.Name}.");

            Logging.AddToCharacterLog(player, $"has switched EUP outfit to {selectedOutfit.Name}.");
        }

        public static void OnFemalePoliceSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Props")
            {
                ShowFemalePoliceProps(player);
                return;
            }

            if (option == "Ranks")
            {
                ShowFemalePoliceRanks(player);
                return;
            }

            EupOutfit selectedOutfit = FemalePoliceOutfits.FirstOrDefault(s => s.Name == option);

            if (selectedOutfit == null)
            {
                player.SendErrorNotification("An error occurred");
                return;
            }

            player.SetClothes(3, selectedOutfit.Torso[0], selectedOutfit.Torso[1]);
            player.SetClothes(4, selectedOutfit.Legs[0], selectedOutfit.Legs[1]);
            player.SetClothes(6, selectedOutfit.Shoes[0], selectedOutfit.Shoes[1]);
            player.SetClothes(11, selectedOutfit.Top[0], selectedOutfit.Top[1]);

            if (selectedOutfit.Name == "SWAT")
            {
                // 80?
                //player.SetClothes(10, 78, 0);
            }

            if (selectedOutfit.Name == "Police Jacket")
            {
                player.SetClothes(8, 201, 0);
            }

            player.SendNotification($"~y~You've changed outfits to {selectedOutfit.Name}.");

            Logging.AddToCharacterLog(player, $"has switched EUP outfit to {selectedOutfit.Name}.");
        }

        #region Male Police Props

        private static void ShowMalePoliceProps(IPlayer player)
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (EupProp malePoliceProp in MalePoliceProps)
            {
                menuItems.Add(new NativeMenuItem(malePoliceProp.Name));
            }

            if (player.GetClass().AccountId == 1)
            {
                menuItems.Add(new NativeMenuItem("Chiefs Hat"));
            }

            NativeMenu menu = new NativeMenu("EupMenu:Police:PropMale", "EUP", "Select a prop", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnMalePolicePropSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Chiefs Hat")
            {
                player.SetAccessory(0, 155, 0);
                return;
            }

            EupProp selectedProp = MalePoliceProps.FirstOrDefault(x => x.Name == option);

            if (selectedProp == null)
            {
                player.SendErrorNotification("Unable to find that prop!");
                return;
            }

            if (selectedProp.PropType == EupPropType.Clothing)
            {
                player.SetClothes(selectedProp.Slot, selectedProp.Data[0], selectedProp.Data[1]);
            }

            if (selectedProp.PropType == EupPropType.Prop)
            {
                player.SetAccessory(selectedProp.Slot, selectedProp.Data[0], selectedProp.Data[1]);
            }

            player.SendNotification($"~y~You've selected the {selectedProp.Name}.");

            Logging.AddToCharacterLog(player, $"has equipped prop {selectedProp.Name}.");
        }

        #endregion Male Police Props

        #region Male Police Ranks

        private static void ShowMalePoliceRanks(IPlayer player)
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (EupProp malePoliceRank in MalePoliceRanks)
            {
                menuItems.Add(new NativeMenuItem(malePoliceRank.Name));
            }

            NativeMenu menu = new NativeMenu("EupMenu:Police:MaleRanks", "EUP", "Select a Rank", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnPoliceMaleRankSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            EupProp selectedRank = MalePoliceRanks.FirstOrDefault(x => x.Name == option);

            if (selectedRank == null)
            {
                player.SendErrorNotification("Unable to find the rank.");
                return;
            }

            if (selectedRank.PropType == EupPropType.Clothing)
            {
                player.SetClothes(selectedRank.Slot, selectedRank.Data[0], selectedRank.Data[1]);
            }

            if (selectedRank.PropType == EupPropType.Prop)
            {
                player.SetAccessory(selectedRank.Slot, selectedRank.Data[0], selectedRank.Data[1]);
            }

            player.SendNotification($"~y~You've selected the rank {option}.");

            Logging.AddToCharacterLog(player, $"has set their rank prop to {option}.");
        }

        #endregion Male Police Ranks

        #region Female Police Props

        private static void ShowFemalePoliceProps(IPlayer player)
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (EupProp femalePoliceProp in FemalePoliceProps)
            {
                menuItems.Add(new NativeMenuItem(femalePoliceProp.Name));
            }

            if (player.GetClass().AccountId == 1)
            {
                menuItems.Add(new NativeMenuItem("Chiefs Hat"));
            }

            NativeMenu menu = new NativeMenu("EupMenu:Police:PropFemale", "EUP", "Select a prop", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnFemalePolicePropSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Chiefs Hat")
            {
                player.SetAccessory(0, 150, 0);
                return;
            }

            EupProp selectedProp = FemalePoliceProps.FirstOrDefault(x => x.Name == option);

            if (selectedProp == null)
            {
                player.SendErrorNotification("Unable to find that prop!");
                return;
            }

            if (selectedProp.PropType == EupPropType.Clothing)
            {
                player.SetClothes(selectedProp.Slot, selectedProp.Data[0], selectedProp.Data[1]);
            }

            if (selectedProp.PropType == EupPropType.Prop)
            {
                player.SetAccessory(selectedProp.Slot, selectedProp.Data[0], selectedProp.Data[1]);
            }

            player.SendNotification($"~y~You've selected the {selectedProp.Name}.");

            Logging.AddToCharacterLog(player, $"has equipped prop {selectedProp.Name}.");
        }

        #endregion Female Police Props

        #region Female Police Ranks

        private static void ShowFemalePoliceRanks(IPlayer player)
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (EupProp malePoliceRank in MalePoliceRanks)
            {
                menuItems.Add(new NativeMenuItem(malePoliceRank.Name));
            }

            NativeMenu menu = new NativeMenu("EupMenu:Police:FemaleRanks", "EUP", "Select a Rank", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnPoliceFemaleRankSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            EupProp selectedRank = MalePoliceRanks.FirstOrDefault(x => x.Name == option);

            if (selectedRank == null)
            {
                player.SendErrorNotification("Unable to find the rank.");
                return;
            }

            if (selectedRank.PropType == EupPropType.Clothing)
            {
                player.SetClothes(selectedRank.Slot, selectedRank.Data[0], selectedRank.Data[1]);
            }

            if (selectedRank.PropType == EupPropType.Prop)
            {
                player.SetAccessory(selectedRank.Slot, selectedRank.Data[0], selectedRank.Data[1]);
            }

            player.SendNotification($"~y~You've selected the rank {option}.");

            Logging.AddToCharacterLog(player, $"has set their rank prop to {option}.");
        }

        #endregion Female Police Ranks
    }
}