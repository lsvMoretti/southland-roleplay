using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection.PortableExecutable;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Vehicle.ModShop
{
    public class ModHandler
    {
        #region Vehicle Prices

        private const int DefaultPrice = 100;
        private const int DigitailRadioPrice = 250;

        #endregion Vehicle Prices

        [Command("vmod")]
        public static void ShowVehicleModMenu(IPlayer player)
        {
            if (player?.FetchCharacter() == null) return;

            if (!player.IsInVehicle)
            {
                player.SendNotification("~r~You must be in a vehicle!", true);
                return;
            }

            if (player.Seat != 1)
            {
                player.SendNotification("~r~You must be in the drivers seat!", true);
                return;
            }

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 7f);

            if (nearbyProperty == null)
            {
                List<Models.Property> properties = Models.Property.FetchProperties();

                foreach (Models.Property property in properties)
                {
                    List<PropertyInteractionPoint> points =
                        JsonConvert.DeserializeObject<List<PropertyInteractionPoint>>(property.InteractionPoints);

                    if (points.Any())
                    {
                        foreach (PropertyInteractionPoint propertyInteractionPoint in points)
                        {
                            Position position = new Position(propertyInteractionPoint.PosX, propertyInteractionPoint.PosY, propertyInteractionPoint.PosZ);

                            if (player.Position.Distance(position) <= 7f)
                            {
                                nearbyProperty = property;
                                break;
                            }
                        }
                    }
                }

                if (nearbyProperty == null)
                {
                    player.SendErrorNotification("You're not near a property.");
                    return;
                }
            }

            if (nearbyProperty.PropertyType != PropertyType.VehicleModShop)
            {
                player.SendErrorNotification("You must be near a modshop.");
                return;
            }

            if (nearbyProperty.Products < 1)
            {
                player.SendErrorNotification("There is no stock here!");
                return;
            }

            if (player.Vehicle.FetchVehicleData() == null)
            {
                player.SendNotification("~r~You must be in a player owned vehicle!", true);
                return;
            }

            player.SetData("AtModProperty", nearbyProperty.Id);

            player.Emit("vehicleMod:FetchVehicleMods", JsonConvert.SerializeObject(new VehicleNumMods()));
        }

        public static void OnVehicleNumModsReturn(IPlayer player, string json)
        {
            VehicleNumMods vMods = JsonConvert.DeserializeObject<VehicleNumMods>(json);

            double cpp = 10000;

            if (player.Vehicle.FetchVehicleData().VehiclePrice > 0)
            {
                cpp = (double)player.Vehicle.FetchVehicleData().VehiclePrice;
            }

            int vehicleClass = player.Vehicle.FetchVehicleData().Class;

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (vMods.Spoilers > 0)
            {
                menuItems.Add(new NativeMenuItem("Spoilers", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.FBumper > 0)
            {
                menuItems.Add(new NativeMenuItem("Front Bumper", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.RBumper > 0)
            {
                menuItems.Add(new NativeMenuItem("Rear Bumper", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.SSkirt > 0)
            {
                menuItems.Add(new NativeMenuItem("Side Skirt", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Exhaust > 0)
            {
                menuItems.Add(new NativeMenuItem("Exhaust", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Frame > 0)
            {
                menuItems.Add(new NativeMenuItem("Frame", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Grille > 0)
            {
                menuItems.Add(new NativeMenuItem("Grille", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Hood > 0)
            {
                menuItems.Add(new NativeMenuItem("Hood", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.RFender > 0)
            {
                menuItems.Add(new NativeMenuItem("Rear Fender", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Roof > 0)
            {
                menuItems.Add(new NativeMenuItem("Roof", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Engine > 0)
            {
                int price = Convert.ToInt32(0 * 1 * (cpp * 0.0075) + (cpp * 0.0075));
                int higherPrice = Convert.ToInt32(3 * 4 * (cpp * 0.0075) + (cpp * 0.0075));
                menuItems.Add(new NativeMenuItem("Engine", $"~g~{price:C0} - {higherPrice:C0}"));
            }
            if (vMods.Brakes > 0)
            {
                int price = Convert.ToInt32(0 * 1 * (cpp * 0.0075) + (cpp * 0.0025));
                int higherPrice = Convert.ToInt32(3 * 4 * (cpp * 0.0075) + (cpp * 0.0025));
                menuItems.Add(new NativeMenuItem("Brakes", $"~g~{price:C} - {higherPrice:C}"));
            }
            if (vMods.Transmission > 0)
            {
                int price = Convert.ToInt32(0 * 1 * (cpp * 0.0075) + (cpp * 0.0050));
                int higherPrice = Convert.ToInt32(3 * 4 * (cpp * 0.0075) + (cpp * 0.0050));
                menuItems.Add(new NativeMenuItem("Transmission", $"~g~{price:C} - {higherPrice:C}"));
            }
            if (vMods.Suspension > 0)
            {
                int price = Convert.ToInt32(0 * 1 * (cpp * 0.0075) + (cpp * 0.0025));
                int higherPrice = Convert.ToInt32(3 * 4 * (cpp * 0.0075) + (cpp * 0.0025));
                menuItems.Add(new NativeMenuItem("Suspension", $"~g~{price:C} - {higherPrice:C}"));
            }
            if (vMods.Horns > 0)
            {
                menuItems.Add(new NativeMenuItem("Horns", $"~g~{DefaultPrice:C0}"));
            }

            if (vehicleClass != 8 || vehicleClass != -1)
            {
                menuItems.Add(new NativeMenuItem("Turbo", $"~g~{Convert.ToInt32(2 * 3 * (cpp * 0.0025) + cpp * 0.0025):C0}"));

                menuItems.Add(new NativeMenuItem("Xenon", $"~g~{DefaultPrice:C0}"));

                menuItems.Add(new NativeMenuItem("Window Tint", $"~g~{DefaultPrice:C0}"));

                if (!player.Vehicle.FetchVehicleData().DigitalRadio)
                {
                    menuItems.Add(new NativeMenuItem("Digital Radio", $"~g~{DigitailRadioPrice:C0}"));
                }
            }

            menuItems.Add(new NativeMenuItem("Front Wheels", $"~g~{DefaultPrice:C0}"));

            if (vMods.BWheels > 0)
            {
                menuItems.Add(new NativeMenuItem("Rear Wheels", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.PlateHolders > 0)
            {
                menuItems.Add(new NativeMenuItem("Plate Holders", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.TrimDesign > 0)
            {
                menuItems.Add(new NativeMenuItem("Trim", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Ornaments > 0)
            {
                menuItems.Add(new NativeMenuItem("Ornaments", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.DialDesign > 0)
            {
                menuItems.Add(new NativeMenuItem("Dials", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.SteeringWheel > 0)
            {
                menuItems.Add(new NativeMenuItem("Steering Wheel", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.ShiftLever > 0)
            {
                menuItems.Add(new NativeMenuItem("Shift Lever", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Plaques > 0)
            {
                menuItems.Add(new NativeMenuItem("Plaques", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Hydraulics > 0)
            {
                menuItems.Add(new NativeMenuItem("Hydraulics", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.Livery > 0)
            {
                menuItems.Add(new NativeMenuItem("Livery", $"~g~{DefaultPrice:C0}"));
            }

            if (vMods.DashColor > 0)
            {
                menuItems.Add(new NativeMenuItem("Dashboard Color", $"~g~{DefaultPrice:C0}"));
            }
            if (vMods.TrimColor > 0)
            {
                menuItems.Add(new NativeMenuItem("Trim Color", $"~g~{DefaultPrice:C0}"));
            }

            NativeMenu menu = new NativeMenu("vehicleMod:ShowMainMenu", "Mod Shop", "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.SetData("vehicleMod:TypeSelected", option);

            if (option == "Spoilers")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Spoilers);
            }

            if (option == "Front Bumper")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.FrontBumper);
            }

            if (option == "Rear Bumper")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.RearBumper);
            }

            if (option == "Side Skirt")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.SideSkirt);
            }

            if (option == "Exhaust")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Exhaust);
            }

            if (option == "Exhaust")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Exhaust);
            }

            if (option == "Frame")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Frame);
            }

            if (option == "Grille")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Grille);
            }

            if (option == "Hood")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Hood);
            }

            if (option == "Right Fender")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.RightFender);
            }

            if (option == "Roof")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Roof);
            }

            if (option == "Engine")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Engine);
            }

            if (option == "Brakes")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Brakes);
            }

            if (option == "Transmission")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Transmission);
            }

            if (option == "Horns")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Horns);
            }

            if (option == "Suspension")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Suspension);
            }

            if (option == "Armor")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Armor);
            }

            if (option == "Turbo")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Turbo);
            }

            if (option == "Xenon")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Xenon);
            }

            if (option == "Front Wheels")
            {
                ShowWheelMenu(player);
                return;
            }

            if (option == "Back Wheels")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.BackWheels);
            }

            if (option == "Plate Holders")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Suspension);
            }

            if (option == "Trim")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.TrimDesign);
            }

            if (option == "Ornaments")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Ornaments);
            }

            if (option == "Dials")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.DialDesign);
            }

            if (option == "Steering Wheels")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.SteeringWheel);
            }

            if (option == "Shift Lever")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.ShiftLever);
            }

            if (option == "Plaques")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Plaques);
            }

            if (option == "Hydraulics")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Hydraulics);
            }

            if (option == "Boost")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Boost);
            }

            if (option == "Livery")
            {
                player.Emit("vehicleMod:FetchModNames", (int)VehicleModType.Livery);
            }

            if (option == "Window Tint")
            {
                player.Emit("vehicleMod:FetchModNames", 69);
            }

            if (option == "Dashboard Color")
            {
                player.Emit("vehicleMod:FetchModNames", 74);
            }

            if (option == "Trim Color")
            {
                player.Emit("vehicleMod:FetchModNames", 75);
            }

            if (option == "Digital Radio")
            {
                NativeUi.ShowYesNoMenu(player, "vehicleMod:purchaseDigitalRadio", "Mod Shop", $"Purchase a Digital Radio for {DigitailRadioPrice:C0}?");
            }
        }

        public static void OnReturnModNames(IPlayer player, string json, int modSlot)
        {
            List<string> modNames = new List<string> { "None" };

            modNames.AddRange(JsonConvert.DeserializeObject<List<string>>(json));

            player.SetData("vehicleMod:modSlotSelected", modSlot);
            player.SetData("vehicleMod:modNamesJson", JsonConvert.SerializeObject(modNames));

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            double cpp = 10000;

            int price = DefaultPrice;

            if (player.Vehicle.FetchVehicleData().VehiclePrice > 0)
            {
                cpp = (double)player.Vehicle.FetchVehicleData().VehiclePrice;
            }

            foreach (string modName in modNames)
            {
                #region Engine Mods

                if (modName == "EMS-Improvement 1")
                {
                    menuItems.Add(new NativeMenuItem(modName, $"{Convert.ToInt32(0 * 1 * (cpp * 0.0075) + (cpp * 0.0075)):C}"));
                }
                if (modName == "EMS-Improvement 2")
                {
                    menuItems.Add(new NativeMenuItem(modName, $"{Convert.ToInt32(1 * 2 * (cpp * 0.0075) + (cpp * 0.0075)):C}"));
                }
                if (modName == "EMS-Improvement 3")
                {
                    menuItems.Add(new NativeMenuItem(modName, $"{Convert.ToInt32(2 * 3 * (cpp * 0.0075) + (cpp * 0.0075)):C}"));
                }
                if (modName == "EMS-Improvement 4")
                {
                    menuItems.Add(new NativeMenuItem(modName, $"{Convert.ToInt32(2 * 3 * (cpp * 0.0075) + (cpp * 0.0075)):C}"));
                }

                #endregion Engine Mods

                #region Brakes

                if (modName == "Street Brakes")
                {
                    menuItems.Add(new NativeMenuItem(modName, $"{Convert.ToInt32(0 * 1 * (cpp * 0.0025) + (cpp * 0.0025)):C}"));
                }
                if (modName == "Sport Brakes")
                {
                    menuItems.Add(new NativeMenuItem(modName, $"{Convert.ToInt32(1 * 2 * (cpp * 0.0025) + (cpp * 0.0025)):C}"));
                }
                if (modName == "Race Brakes")
                {
                    menuItems.Add(new NativeMenuItem(modName, $"{Convert.ToInt32(2 * 2 * (cpp * 0.0025) + (cpp * 0.0025)):C}"));
                }

                #endregion Brakes

                else
                {
                    menuItems.Add(new NativeMenuItem(modName));
                }
            }

            menuItems.Add(new NativeMenuItem("Back", "Go back a menu"));

            NativeMenu menu = new NativeMenu("vehicleMod:showModNameMenu", "Vehicle Mod Shop", "Select a mod to preview", menuItems)
            {
                ItemChangeTrigger = "vehicleMod:OnModNameIndexChange",
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnModNameChangeIndex(IPlayer player, int index, string itemText)
        {
            if (itemText == "Close" || itemText == "Back") return;

            player.GetData("vehicleMod:modSlotSelected", out int modSlot);
            player.GetData("vehicleMod:modNamesJson", out string json);

            List<string> modNames = JsonConvert.DeserializeObject<List<string>>(json);

            player.Vehicle?.SetMod((byte)modSlot, (byte)index);
        }

        public static void OnModNameMenuSelect(IPlayer player, string option)
        {
            player.GetData("vehicleMod:modNamesJson", out string json);
            player.GetData("vehicleMod:TypeSelected", out string modTypeString);
            player.GetData("vehicleMod:modSlotSelected", out int modSlot);

            if (option == "Close")
            {
                LoadVehicle.LoadVehicleMods(player.Vehicle);
                return;
            }

            if (option == "Back")
            {
                LoadVehicle.LoadVehicleMods(player.Vehicle);
                ShowVehicleModMenu(player);
                return;
            }

            List<string> modNames = JsonConvert.DeserializeObject<List<string>>(json);

            string selectedMod = modNames.FirstOrDefault(x => x == option);

            if (selectedMod == null)
            {
                player.SendErrorNotification("An error occurred fetching the mod name.");
                LoadVehicle.LoadVehicleMods(player.Vehicle);
                return;
            }

            int modIndex = modNames.IndexOf(selectedMod);

            int modType = modIndex;

            player.SetData("vehicleMod:modSelectedIndex", modIndex);

            if (option == "Pure Black")
            {
                modType = 1;
            }

            if (option == "Dark Smoke")
            {
                modType = 2;
            }

            if (option == "Light Smoke")
            {
                modType = 3;
            }

            if (option == "Stock")
            {
                modType = 4;
            }

            if (option == "Limo")
            {
                modType = 5;
            }

            if (option == "Green")
            {
                modType = 6;
            }

            if (option == "Turbo")
            {
                modType = 1;
            }

            if (option == "EMS-Improvement 1")
            {
                modType = 1;
            }

            if (option == "EMS-Improvement 2")
            {
                modType = 2;
            }

            if (option == "EMS-Improvement 3")
            {
                modType = 3;
            }

            if (option == "EMS-Improvement 4")
            {
                modType = 4;
            }

            if (option == "Street Brakes")
            {
                modType = 1;
            }

            if (option == "Sport Brakes")
            {
                modType = 2;
            }

            if (option == "Race Brakes")
            {
                modType = 3;
            }

            if (option == "Street Transmission")
            {
                modType = 1;
            }

            if (option == "Sport Transmission")
            {
                modType = 2;
            }

            if (option == "Race Transmission")
            {
                modType = 3;
            }

            if (option == "Lower Suspension")
            {
                modType = 1;
            }

            if (option == "Street Suspension")
            {
                modType = 2;
            }

            if (option == "Sport Suspension")
            {
                modType = 3;
            }

            if (option == "Race Suspension")
            {
                modType = 4;
            }

            if (option == "Standard Headlights")
            {
                modType = -1;
            }

            if (option == "Xenon Headlights")
            {
                modType = 1;
            }

            VehicleModType vModType = VehicleModType.Frame;

            if (modSlot != 74 || modSlot != 75)
            {
                vModType = (VehicleModType)modSlot;
            }

            int higherPrice = 0;

            double cpp = 10000;

            int price = DefaultPrice;

            if (player.Vehicle.FetchVehicleData().VehiclePrice > 0)
            {
                cpp = (double)player.Vehicle.FetchVehicleData().VehiclePrice;
            }

            switch (vModType)
            {
                case VehicleModType.Engine:

                    price = Convert.ToInt32(0 * 1 * (cpp * 0.0075) + (cpp * 0.0075));
                    higherPrice = Convert.ToInt32(3 * 4 * (cpp * 0.0075) + (cpp * 0.0075));
                    break;

                case VehicleModType.Brakes:

                    price = Convert.ToInt32(0 * 1 * (cpp * 0.0025) + (cpp * 0.0025));
                    higherPrice = Convert.ToInt32(3 * 4 * (cpp * 0.0025) + (cpp * 0.0025));
                    break;

                case VehicleModType.Transmission:

                    price = Convert.ToInt32(0 * 1 * (cpp * 0.0025) + (cpp * 0.0050));
                    higherPrice = Convert.ToInt32(3 * 4 * (cpp * 0.0025) + (cpp * 0.0050));

                    break;

                case VehicleModType.Suspension:
                    price = Convert.ToInt32(0 * 1 * (cpp * 0.0025) + (cpp * 0.0050));
                    higherPrice = Convert.ToInt32(3 * 4 * (cpp * 0.0025) + (cpp * 0.0050));
                    break;

                case VehicleModType.Turbo:
                    price =
                    Convert.ToInt32(2 * 3 * (cpp * 0.0025) + cpp * 0.0025);
                    break;

                default:
                    price = 100;
                    break;
            }

            Models.Vehicle vehicleData = player.Vehicle.FetchVehicleData();

            Dictionary<int, int> modList = JsonConvert.DeserializeObject<Dictionary<int, int>>(vehicleData.VehicleMods);

            if (modSlot == 11)
            {
                // modTypes
                // None 0
                // Level 1 = 1
                // Level 2 = 2
                // Level 3 = 3
                // Level 4 = 4
                // Engine Upgrade
                if (modList.ContainsKey(modSlot))
                {
                    var modInfo = modList.FirstOrDefault(x => x.Key == modSlot);

                    if (modType == 1 && modInfo.Value < 0)
                    {
                        // Wants to buy Level 2 - Has none
                        player.SendErrorNotification("You need to select Level 1 first!");
                        return;
                    }

                    if (modType == 2 && modInfo.Value < 1)
                    {
                        // Want to buy Level 3 - Has level 1 or below
                        player.SendErrorNotification("You need to select Level 2 first!");
                        return;
                    }
                    if (modType == 3 && modInfo.Value < 2)
                    {
                        // Want to buy Level 4 - Has level 2 or below
                        player.SendErrorNotification("You need to select Level 2 first!");
                        return;
                    }
                    if (modType == 4 && modInfo.Value < 3)
                    {
                        // Want to buy Level 4 - Has level 2 or below
                        player.SendErrorNotification("You need to select Level 3 first!");
                        return;
                    }
                }

                if (!modList.ContainsKey(modSlot))
                {
                    // Has no mods
                    if (modType > 1)
                    {
                        player.SendErrorNotification("You need to select Level 1 first!");
                        return;
                    }
                }
            }
            if (modSlot == 12)
            {
                if (modList.ContainsKey(modSlot))
                {
                    var modInfo = modList.FirstOrDefault(x => x.Key == modSlot);

                    if (modType == 1 && modInfo.Value < 0)
                    {
                        // Wants to buy Level 2 - Has none
                        player.SendErrorNotification("You need to select Level 1 first!");
                        return;
                    }

                    if (modType == 2 && modInfo.Value < 1)
                    {
                        // Want to buy Level 3 - Has level 1 or below
                        player.SendErrorNotification("You need to select Level 2 first!");
                        return;
                    }
                }

                if (!modList.ContainsKey(modSlot))
                {
                    // Has no mods
                    if (modType > 1)
                    {
                        player.SendErrorNotification("You need to select Level 1 first!");
                        return;
                    }
                }
            }

            if (modSlot == 13)
            {
                if (modList.ContainsKey(modSlot))
                {
                    var modInfo = modList.FirstOrDefault(x => x.Key == modSlot);

                    if (modType == 1 && modInfo.Value < 0)
                    {
                        // Wants to buy Level 2 - Has none
                        player.SendErrorNotification("You need to select Level 1 first!");
                        return;
                    }

                    if (modType == 2 && modInfo.Value < 1)
                    {
                        // Want to buy Level 3 - Has level 1 or below
                        player.SendErrorNotification("You need to select Level 2 first!");
                        return;
                    }
                }

                if (!modList.ContainsKey(modSlot))
                {
                    // Has no mods
                    if (modType > 1)
                    {
                        player.SendErrorNotification("You need to select Level 1 first!");
                        return;
                    }
                }
            }

            KeyValuePair<int, int> modData = new KeyValuePair<int, int>(modSlot, modType);

            player.SetData("vehicleMod:modData", JsonConvert.SerializeObject(modData));

            string priceRange = $"~g~{price:C0}";

            if (higherPrice > 0)
            {
                priceRange = $"~g~${price} - ${higherPrice}";
            }

            if (modSlot == (int)VehicleModType.WindowTint)
            {
                player.Vehicle.WindowTint = (byte)modType;
            }

            player.Vehicle.SetMod((byte)modSlot, (byte)modType);

            /*
             *
            if (player.HasData("SELECTEDWHEELTYPE"))
            {
                if (modSlot == (int)VehicleModTypes.FrontWheels)
                {
                    player.Vehicle.WheelType = player.GetData("SELECTEDWHEELTYPE");
                }
            }
             */

            NativeUi.ShowYesNoMenu(player, "vehicleMod:PurchaseMenuSelect", "Vehicle Mod Shop", $"Cost: {priceRange}");
        }

        public static void OnPurchaseMenuSelect(IPlayer player, string option)
        {
            LoadVehicle.LoadVehicleMods(player.Vehicle);

            if (option == "No")
            {
                ShowVehicleModMenu(player);
                return;
            }

            player.GetData("vehicleMod:modData", out string jsonModData);

            KeyValuePair<int, int> modData =
                JsonConvert.DeserializeObject<KeyValuePair<int, int>>(jsonModData);

            VehicleModType vModType = VehicleModType.Frame;

            if (modData.Key != 74 || modData.Key != 75)
            {
                vModType = (VehicleModType)modData.Key;
            }

            double cpp = 10000;

            if (player.Vehicle.FetchVehicleData().VehiclePrice > 0)
            {
                cpp = (double)player.Vehicle.FetchVehicleData().VehiclePrice;
            }

            int price = DefaultPrice;

            switch (vModType)
            {
                case VehicleModType.Engine:
                    if (modData.Value == 0)
                    {
                        // No Engine Mod, needed level 1
                        price = Convert.ToInt32(0 * 1 * (cpp * 0.0075) + (cpp * 0.0075));
                    }

                    if (modData.Value == 1)
                    {
                        price = Convert.ToInt32(1 * 2 * (cpp * 0.0075) + (cpp * 0.0075));
                    }

                    if (modData.Value == 2)
                    {
                        price = Convert.ToInt32(2 * 3 * (cpp * 0.0075) + (cpp * 0.0075));
                    }

                    if (modData.Value == 3)
                    {
                        price = Convert.ToInt32(3 * 4 * (cpp * 0.0075) + (cpp * 0.0075));
                    }
                    if (modData.Value == 4)
                    {
                        price = Convert.ToInt32(3 * 4 * (cpp * 0.0075) + (cpp * 0.0075));
                    }

                    break;

                case VehicleModType.Brakes:
                    if (modData.Value == 0)
                    {
                        price = Convert.ToInt32(0 * 1 * (cpp * 0.0025) + (cpp * 0.0025));
                    }

                    if (modData.Value == 1)
                    {
                        price = Convert.ToInt32(1 * 2 * (cpp * 0.0025) + (cpp * 0.0025));
                    }

                    if (modData.Value == 2)
                    {
                        price = Convert.ToInt32(3 * 4 * (cpp * 0.0025) + (cpp * 0.0025));
                    }
                    break;

                case VehicleModType.Transmission:
                    if (modData.Value == 0)
                    {
                        price = Convert.ToInt32(0 * 1 * (cpp * 0.0025) + (cpp * 0.0050));
                    }

                    if (modData.Value == 1)
                    {
                        price = Convert.ToInt32(1 * 2 * (cpp * 0.0025) + (cpp * 0.0050));
                    }

                    if (modData.Value == 2)
                    {
                        price = Convert.ToInt32(3 * 4 * (cpp * 0.0025) + (cpp * 0.0050));
                    }

                    break;

                case VehicleModType.Suspension:
                    if (modData.Value == 0)
                    {
                        price = Convert.ToInt32(0 * 1 * (cpp * 0.0025) + (cpp * 0.0025));
                    }

                    if (modData.Value == 1)
                    {
                        price = Convert.ToInt32(1 * 2 * (cpp * 0.0025) + (cpp * 0.0025));
                    }

                    if (modData.Value == 2)
                    {
                        price = Convert.ToInt32(3 * 4 * (cpp * 0.0025) + (cpp * 0.0025));
                    }
                    break;

                case VehicleModType.Turbo:
                    price = Convert.ToInt32(2 * 3 * (cpp * 0.0025) + cpp * 0.0025);
                    break;

                default:
                    price = 100;
                    break;
            }

            bool adminMod = player.HasData("AdminModVehicle");

            if (!adminMod && player.FetchCharacter().Money < price)
            {
                player.SendErrorNotification("You don't have enough money for this.");
                return;
            }

            player.GetData("vehicleMod:modSelectedIndex", out int modSelectedIndex);

            player.GetData("vehicleMod:modNamesJson", out string json);

            player.GetData("vehicleMod:TypeSelected", out string modTypeString);

            List<string> modNames = JsonConvert.DeserializeObject<List<string>>(json);

            string modName = modNames[modSelectedIndex];

            var vehicleInventory = player.Vehicle.FetchInventory();

            if (vehicleInventory == null)
            {
                player.SendErrorNotification("You can't do this in this vehicle.");
                return;
            }

            VehicleInventoryModData modInfo = new VehicleInventoryModData
            {
                ModClassName = modTypeString,
                ModName = modName,
                VehicleName = player.Vehicle.FetchVehicleData().Name,
                ModData = modData
            };

            bool itemAdded = vehicleInventory.AddItem(new InventoryItem("ITEM_VEHICLE_MOD", modName,
                JsonConvert.SerializeObject(modInfo)));

            if (!itemAdded)
            {
                player.SendInfoNotification("Couldn't add the item into the Inventory.");
                return;
            }

            if (!adminMod)
            {
                player.RemoveCash(price);
            }

            player.DeleteData("AdminModVehicle");

            player.SendInfoNotification($"You've selected the new {modName} for your vehicle's {modTypeString}. This has cost you {price:C0}.");

            player.SendInfoNotification($"Head to a mechanic to get this fitted.");

            Logging.AddToCharacterLog(player, $"has bought a new VMod for vehicle Id: {player.Vehicle.FetchVehicleData().Id}. Mod slot: {modTypeString}, Mod Name: {modName}, Index: {modSelectedIndex}.");

            bool hasPropertyData = player.GetData("AtModProperty", out int propertyId);

            if (hasPropertyData)
            {
                using Context context = new Context();

                Models.Property atProperty = context.Property.Find(propertyId);

                if (atProperty == null) return;

                atProperty.Products -= 1;

                // 10% of mod value
                double modValuePercentage = price * 0.5;

                atProperty.Balance += modValuePercentage;

                context.SaveChanges();
            }
        }

        public static void OnDigitalRadioPurchaseSelect(IPlayer player, string option)
        {
            if (option == "No" || option == "Close")
            {
                LoadVehicle.LoadVehicleMods(player.Vehicle);
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            bool adminMod = player.HasData("AdminModVehicle");

            if (!adminMod && playerCharacter.Money < DigitailRadioPrice)
            {
                player.SendErrorNotification($"You don't have enough money for this. You need {DigitailRadioPrice:CO}");
                return;
            }

            var vehicleInventory = player.Vehicle.FetchInventory();

            if (vehicleInventory == null)
            {
                player.SendErrorNotification("You can't do this in this vehicle.");
                return;
            }

            bool itemAdded = vehicleInventory.AddItem(new InventoryItem("ITEM_VEHICLE_MOD", "Digital Radio", ""));

            if (!itemAdded)
            {
                player.SendInfoNotification("Couldn't add the item into the Inventory.");
                return;
            }

            if (!adminMod)
            {
                player.RemoveCash(DigitailRadioPrice);
            }

            player.DeleteData("AdminModVehicle");

            player.SendInfoNotification($"You've purchased a new digital radio for your vehicle. Head to a mechanic to get this fitted!");

            Logging.AddToCharacterLog(player, $"has bought a digital radio for their vehicle Id: {player.Vehicle.FetchVehicleData()}.");

            bool hasPropertyData = player.GetData("AtModProperty", out int propertyId);

            if (hasPropertyData)
            {
                using Context context = new Context();

                Models.Property atProperty = context.Property.Find(propertyId);

                if (atProperty == null) return;

                atProperty.Products -= 1;

                // 10% of mod value
                double modValuePercentage = DigitailRadioPrice * 0.1;

                atProperty.Balance += modValuePercentage;

                context.SaveChanges();
            }
        }

        public static void ShowWheelMenu(IPlayer player)
        {
            if (player == null) return;

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Sport"),
                new NativeMenuItem("Muscle"),
                new NativeMenuItem("Lowrider"),
                new NativeMenuItem("SUV"),
                new NativeMenuItem("Offroad"),
                new NativeMenuItem("Tuner"),
                new NativeMenuItem("Bike Wheels"),
                new NativeMenuItem("High End"),
                new NativeMenuItem("Benny's Original"),
                new NativeMenuItem("Benny's Bespoke")
            };

            NativeMenu wheelTypeMenu = new NativeMenu("modShop:WheelTypeSelectionMainMenu", "Mod Shop", "Select a wheel type", menuItems);

            NativeUi.ShowNativeMenu(player, wheelTypeMenu, true);
        }

        public static void OnWheelTypeSelected(IPlayer player, string option)
        {
            if (option == "Close")
            {
                LoadVehicle.LoadVehicleMods(player.Vehicle);
                return;
            }

            switch (option)
            {
                case "Sport":
                    player.SetData("modShop:SelectedWheelType", 0);
                    player.Emit("modShop:fetchWheelTypes", 0);
                    player.Vehicle.SetWheels(0, 0);
                    break;

                case "Muscle":
                    player.SetData("modShop:SelectedWheelType", 1);
                    player.Emit("modShop:fetchWheelTypes", 1);
                    player.Vehicle.SetWheels(1, 0);
                    break;

                case "Lowrider":
                    player.SetData("modShop:SelectedWheelType", 2);
                    player.Emit("modShop:fetchWheelTypes", 2);
                    player.Vehicle.SetWheels(2, 0);
                    break;

                case "SUV":
                    player.SetData("modShop:SelectedWheelType", 3);
                    player.Emit("modShop:fetchWheelTypes", 3);
                    player.Vehicle.SetWheels(3, 0);
                    break;

                case "Offroad":
                    player.SetData("modShop:SelectedWheelType", 4);
                    player.Emit("modShop:fetchWheelTypes", 4);
                    player.Vehicle.SetWheels(4, 0);
                    break;

                case "Tuner":
                    player.SetData("modShop:SelectedWheelType", 5);
                    player.Emit("modShop:fetchWheelTypes", 5);
                    player.Vehicle.SetWheels(5, 0);
                    break;

                case "Bike Wheels":
                    player.SetData("modShop:SelectedWheelType", 6);
                    player.Emit("modShop:fetchWheelTypes", 6);
                    player.Vehicle.SetWheels(6, 0);
                    break;

                case "High End":
                    player.SetData("modShop:SelectedWheelType", 7);
                    player.Emit("modShop:fetchWheelTypes", 7);
                    player.Vehicle.SetWheels(7, 0);
                    break;

                case "Benny's Original":
                    player.SetData("modShop:SelectedWheelType", 8);
                    player.Emit("modShop:fetchWheelTypes", 8);
                    player.Vehicle.SetWheels(8, 0);
                    break;

                case "Benny's Bespoke":
                    player.SetData("modShop:SelectedWheelType", 9);
                    player.Emit("modShop:fetchWheelTypes", 9);
                    player.Vehicle.SetWheels(9, 0);
                    break;

                default:
                    return;
            }
        }

        public static void OnWheelNamesFetched(IPlayer player, string json)
        {
            List<string> wheelNames = JsonConvert.DeserializeObject<List<string>>(json);

            wheelNames.Insert(0, "Stock");

            player.SetData("modShop:wheelNames", JsonConvert.SerializeObject(wheelNames));

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (string wheelName in wheelNames)
            {
                menuItems.Add(new NativeMenuItem(wheelName));
            }

            NativeMenu wheelMenu = new NativeMenu("modShop:selectWheelName", "Mod Shop", "Select a wheel", menuItems)
            {
                ItemChangeTrigger = "modshop:OnWheelNameChangeIndex",
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, wheelMenu, true);
        }

        public static void OnWheelNameChangeIndex(IPlayer player, int newIndex, string itemText)
        {
            if (itemText == "Close") return;

            player.GetData("modShop:SelectedWheelType", out int wheelType);

            player.Vehicle?.SetWheels((byte)wheelType, (byte)newIndex);
        }

        public static void OnWheelNameSelected(IPlayer player, string option, int index)
        {
            if (option == "Close")
            {
                LoadVehicle.LoadVehicleMods(player.Vehicle);
                return;
            }

            player.GetData("modShop:wheelNames", out string json);
            player.GetData("modShop:SelectedWheelType", out int wheelType);

            List<string> wheelNames = JsonConvert.DeserializeObject<List<string>>(json);

            string selectedWheel = wheelNames.FirstOrDefault(x => x == option);

            player.SetData("modShop:selectedWheelIndex", index);

            player.Vehicle.SetWheels((byte)wheelType, (byte)index);

            NativeUi.ShowYesNoMenu(player, "modShop:purchaseSelectedWheels", "Mod Shop", "Would you like to purchase these wheels?");
        }

        public static void OnWheelPurchaseSelect(IPlayer player, string option)
        {
            IVehicle playerVehicle = player.Vehicle;

            LoadVehicle.LoadVehicleMods(playerVehicle);

            if (option == "Close" || option == "No") return;

            bool adminMod = player.HasData("AdminModVehicle");

            if (!adminMod && player.FetchCharacter().Money < DefaultPrice)
            {
                player.SendErrorNotification("You don't have enough money for this.");
                return;
            }

            player.GetData("modShop:selectedWheelIndex", out int selectedWheelIndex);

            player.GetData("modShop:wheelNames", out string json);

            player.GetData("modShop:SelectedWheelType", out int wheelType);

            List<string> modNames = JsonConvert.DeserializeObject<List<string>>(json);

            string modName = modNames[selectedWheelIndex];

            var vehicleInventory = player.Vehicle.FetchInventory();

            if (vehicleInventory == null)
            {
                player.SendErrorNotification("You can't do this in this vehicle.");
                return;
            }

            VehicleInventoryModData modInfo = new VehicleInventoryModData
            {
                ModClassName = "Front Wheels",
                ModName = modName,
                VehicleName = player.Vehicle.FetchVehicleData().Name,
                ModData = new KeyValuePair<int, int>(23, selectedWheelIndex),
                WheelType = wheelType
            };

            bool itemAdded = vehicleInventory.AddItem(new InventoryItem("ITEM_VEHICLE_MOD", modName,
                JsonConvert.SerializeObject(modInfo)));

            if (!itemAdded)
            {
                player.SendInfoNotification("Couldn't add the item into the Inventory.");
                return;
            }

            if (!adminMod)
            {
                player.RemoveCash(DefaultPrice);
            }

            player.DeleteData("AdminModVehicle");

            player.SendInfoNotification($"You've selected the new {modName} for your vehicle's Front Wheels. This has cost you {DefaultPrice:C0}.");

            player.SendInfoNotification($"Head to a mechanic to get this fitted.");

            Logging.AddToCharacterLog(player, $"has bought a new VMod for vehicle Id: {player.Vehicle.FetchVehicleData().Id}. Mod slot: Front Wheels, Mod Name: {modName}, Index: {selectedWheelIndex}.");

            bool hasPropertyData = player.GetData("AtModProperty", out int propertyId);

            if (hasPropertyData)
            {
                using Context context = new Context();

                Models.Property atProperty = context.Property.Find(propertyId);

                if (atProperty == null) return;

                atProperty.Products -= 1;

                // 10% of mod value
                double modValuePercentage = DefaultPrice * 0.1;

                atProperty.Balance += modValuePercentage;

                context.SaveChanges();
            }
        }

        public static string FetchColorName(byte colorId)
        {
            return colorId switch
            {
                0 => "Metallic Black",
                1 => "Metallic Graphite Black",
                2 => "Metallic Black Steal",
                3 => "Metallic Dark Silver",
                4 => "Metallic Silver",
                5 => "Metallic Blue Silver",
                6 => "Metallic Steel Gray",
                7 => "Metallic Shadow Silver",
                8 => "Metallic Stone Silver",
                9 => "Metallic Midnight Silver",
                10 => "Metallic Gun Metal",
                11 => "Metallic Anthracite Grey",
                12 => "Matte Black",
                13 => "Matte Gray",
                14 => "Matte Light Grey",
                15 => "Util Black",
                16 => "Util Black Poly",
                17 => "Util Dark silver",
                18 => "Util Silver",
                19 => "Util Gun Metal",
                20 => "Util Shadow Silver",
                21 => "Worn Black",
                22 => "Worn Graphite",
                23 => "Worn Silver Grey",
                24 => "Worn Silver",
                25 => "Worn Blue Silver",
                26 => "Worn Shadow Silver",
                27 => "Metallic Red",
                28 => "Metallic Torino Red",
                29 => "Metallic Formula Red",
                30 => "Metallic Blaze Red",
                31 => "Metallic Graceful Red",
                32 => "Metallic Garnet Red",
                33 => "Metallic Desert Red",
                34 => "Metallic Cabernet Red",
                35 => "Metallic Candy Red",
                36 => "Metallic Sunrise Orange",
                37 => "Metallic Classic Gold",
                38 => "Metallic Orange",
                39 => "Matte Red",
                40 => "Matte Dark Red",
                41 => "Matte Orange",
                42 => "Matte Yellow",
                43 => "Util Red",
                44 => "Util Bright Red",
                45 => "Util Garnet Red",
                46 => "Worn Red",
                47 => "Worn Golden Red",
                48 => "Worn Dark Red",
                49 => "Metallic Dark Green",
                50 => "Metallic Racing Green",
                51 => "Metallic Sea Green",
                52 => "Metallic Olive Green",
                53 => "Metallic Green",
                54 => "Metallic Gasoline Blue Green",
                55 => "Matte Lime Green",
                56 => "Util Dark Green",
                57 => "Util Green",
                58 => "Worn Dark Green",
                59 => "Worn Green",
                60 => "Worn Sea Wash",
                61 => "Metallic Midnight Blue",
                62 => "Metallic Dark Blue",
                63 => "Metallic Saxony Blue",
                64 => "Metallic Blue",
                65 => "Metallic Mariner Blue",
                66 => "Metallic Harbor Blue",
                67 => "Metallic Diamond Blue",
                68 => "Metallic Surf Blue",
                69 => "Metallic Nautical Blue",
                70 => "Metallic Bright Blue",
                71 => "Metallic Purple Blue",
                72 => "Metallic Spinnaker Blue",
                73 => "Metallic Ultra Blue",
                74 => "Metallic Bright Blue",
                75 => "Util Dark Blue",
                76 => "Util Midnight Blue",
                77 => "Util Blue",
                78 => "Util Sea Foam Blue",
                79 => "Util Lightning Blue",
                80 => "Util Maui Blue Poly",
                81 => "Util Bright Blue",
                82 => "Matte Dark Blue",
                83 => "Matte Blue",
                84 => "Matte Midnight Blue",
                85 => "Worn Dark blue",
                86 => "Worn Blue",
                87 => "Worn Light blue",
                88 => "Metallic Taxi Yellow",
                89 => "Metallic Race Yellow",
                90 => "Metallic Bronze",
                91 => "Metallic Yellow Bird",
                92 => "Metallic Lime",
                93 => "Metallic Champagne",
                94 => "Metallic Pueblo Beige",
                95 => "Metallic Dark Ivory",
                96 => "Metallic Choco Brown",
                97 => "Metallic Golden Brown",
                98 => "Metallic Light Brown",
                99 => "Metallic Straw Beige",
                100 => "Metallic Moss Brown",
                101 => "Metallic Biston Brown",
                102 => "Metallic Beechwood",
                103 => "Metallic Dark Beechwood",
                104 => "Metallic Choco Orange",
                105 => "Metallic Beach Sand",
                106 => "Metallic Sun Bleeched Sand",
                107 => "Metallic Cream",
                108 => "Util Brown",
                109 => "Util Medium Brown",
                110 => "Util Light Brown",
                111 => "Metallic White",
                112 => "Metallic Frost White",
                113 => "Worn Honey Beige",
                114 => "Worn Brown",
                115 => "Worn Dark Brown",
                116 => "Worn straw beige",
                117 => "Brushed Steel",
                118 => "Brushed Black steel",
                119 => "Brushed Aluminum",
                120 => "Chrome",
                121 => "Worn Off White",
                122 => "Util Off White",
                123 => "Worn Orange",
                124 => "Worn Light Orange",
                125 => "Metallic Securicor Green",
                126 => "Worn Taxi Yellow",
                127 => "Police Car Blue",
                128 => "Matte Green",
                129 => "Matte Brown",
                130 => "Worn Orange",
                131 => "Matte White",
                132 => "Worn White",
                133 => "Worn Olive Army Green",
                134 => "Pure White",
                135 => "Hot Pink",
                136 => "Salmon pink",
                137 => "Metallic Vermillion Pink",
                138 => "Orange",
                139 => "Green",
                140 => "Blue",
                141 => "Mettalic Black Blue",
                142 => "Metallic Black Purple",
                143 => "Metallic Black Red",
                144 => "Hunter Green",
                145 => "Metallic Purple",
                146 => "Metallic V Dark Blue",
                147 => "Modshop Black",
                148 => "Matte Purple",
                149 => "Matte Dark Purple",
                150 => "Metallic Lava Red",
                151 => "Matte Forest Green",
                152 => "Matte Olive Drab",
                153 => "Matte Desert Brown",
                154 => "Matte Desert Tan",
                155 => "Matte Foilage Green",
                156 => "Alloy",
                157 => "Epsilon Blue",
                158 => "Pure Gold",
                159 => "Brushed Gold",
                _ => "Metallic Black"
            };
        }

        public static List<string> FetchVehicleColorList()
        {
            List<string> colorList = new List<string>();

            for (byte i = 0; i <= 159; i++)
            {
                colorList.Add(FetchColorName(i));
            }

            return colorList;
        }
    }
}