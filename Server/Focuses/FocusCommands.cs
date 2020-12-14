using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Vehicle;
using Server.Vehicle.ModShop;

namespace Server.Focuses
{
    public class FocusCommands
    {
        [Command("joinfocus", commandType: CommandType.Focus, description: "Joins a focus /joinfocus [Focus]")]
        public static void CommandJoinFocus(IPlayer player, string args = "")
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/joinfocus [Focus Name]");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            List<FocusTypes> playerFocuses = JsonConvert.DeserializeObject<List<FocusTypes>>(playerCharacter.FocusJson);

            if (args.ToLower() == "mechanic")
            {
                if (player.Position.Distance(FocusHandler.MechanicPosition) > 5f)
                {
                    player.SendErrorNotification("You're not near the location.");
                    return;
                }

                if (playerFocuses.Contains(FocusTypes.Mechanic))
                {
                    player.SendErrorNotification("You already have the mechanic focus!");
                    return;
                }

                using Context context = new Context();
                var playerCharacterDb = context.Character.Find(playerCharacter.Id);

                playerFocuses.Add(FocusTypes.Mechanic);

                playerCharacterDb.FocusJson = JsonConvert.SerializeObject(playerFocuses);

                context.SaveChanges();

                player.SendInfoNotification($"You've joined the Mechanic Focus.");
            }
        }

        [Command("leavefocus", commandType: CommandType.Focus, description: "Leaves a focus /leavefocus [Focus]")]
        public static void CommandLeaveFocus(IPlayer player, string args = "")
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/leavefocus [Mechanic]");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            List<FocusTypes> playerFocuses = JsonConvert.DeserializeObject<List<FocusTypes>>(playerCharacter.FocusJson);

            if (args.ToLower() == "mechanic")
            {
                if (player.Position.Distance(FocusHandler.MechanicPosition) > 5f)
                {
                    player.SendErrorNotification("You're not near the location.");
                    return;
                }

                if (!playerFocuses.Contains(FocusTypes.Mechanic))
                {
                    player.SendErrorNotification("You don't have the mechanic focus!");
                    return;
                }

                using Context context = new Context();
                var playerCharacterDb = context.Character.Find(playerCharacter.Id);

                playerFocuses.Remove(FocusTypes.Mechanic);

                playerCharacterDb.FocusJson = JsonConvert.SerializeObject(playerFocuses);

                context.SaveChanges();

                player.SendInfoNotification($"You've left the Mechanic Focus.");
            }
        }

        #region Mechanic Focus Commands

        [Command("repair", commandType: CommandType.Focus, description: "Mechanic: Used to fix vehicles")]
        public static void CommandRepairVehicle(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            if (player.Seat != 1)
            {
                player.SendErrorNotification("Your not in the drivers seat.");
                return;
            }

            if (player.GetClass().Cash < 15)
            {
                player.SendErrorNotification("You don't have enough. Required $15.");
                return;
            }

            if (player.Vehicle.EngineOn)
            {
                player.SendErrorNotification("The engine must be off.");
                return;
            }

            player.RemoveCash(15);

            player.SendInfoNotification($"The vehicle is now being repaired. Please wait.");

            Timer fixTimer = new Timer(15000) {AutoReset = false};
            fixTimer.Start();

            player.Vehicle.SetData("ActiveRepair", true);

            IVehicle vehicle = player.Vehicle;

            fixTimer.Elapsed += (sender, args) =>
            {
                fixTimer.Stop();
                
                vehicle.DeleteData("ActiveRepair");

                Alt.EmitAllClients("FixVehicle", vehicle);
                player.SendInfoNotification($"The vehicle has been repaired.");

            };

        }

        [Command("addmod", commandType: CommandType.Focus, description: "Mechanic: Adds a mod to a vehicle")]
        public static void CommandAddMod(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle!");
                return;
            }

            var vehicleData = player.Vehicle.FetchVehicleData();

            if (vehicleData == null)
            {
                player.SendErrorNotification("You can't do that in this vehicle!");
                return;
            }

            List<FocusTypes> playerFocuses =
                JsonConvert.DeserializeObject<List<FocusTypes>>(player.FetchCharacter().FocusJson);

            if (!playerFocuses.Contains(FocusTypes.Mechanic))
            {
                player.SendErrorNotification("You don't have the mechanic focus!");
                return;
            }

            List<InventoryItem> modItems = player.Vehicle.FetchInventory().GetInventory()
                .Where(x => x.Id == "ITEM_VEHICLE_MOD").ToList();

            if (!modItems.Any())
            {
                player.SendErrorNotification("This vehicle doesn't have any mods in the Inventory.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (var inventoryItem in modItems)
            {
                menuItems.Add(new NativeMenuItem(inventoryItem.CustomName));
            }

            NativeMenu menu = new NativeMenu("focus:mechanic:addModSelected", "Mods", "Select a mod to install", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnAddModSelected(IPlayer player, string option)
        {
            if (option == "Close") return;

            var vehicleInventory = player.Vehicle.FetchInventory();

            List<InventoryItem> modItems = vehicleInventory.GetInventory()
                .Where(x => x.Id == "ITEM_VEHICLE_MOD").ToList();

            InventoryItem modItem = modItems.FirstOrDefault(x => x.CustomName == option);

            if (modItem?.CustomName == "Digital Radio")
            {
                using Context context = new Context();

                Models.Vehicle vehicleDb = context.Vehicle.Find(player.Vehicle.GetClass().Id);

                if (vehicleDb == null)
                {
                    player.SendErrorNotification("An error occurred fetching the vehicle data.");
                    return;
                }

                bool radioItemRemoved = vehicleInventory.RemoveItem(modItem);

                if (!radioItemRemoved)
                {
                    player.SendErrorNotification("There was an error removing the item from the vehicle inventory.");
                    return;
                }

                vehicleDb.DigitalRadio = true;

                context.SaveChanges();

                

                player.SendInfoNotification($"You've installed a new digital radio onto the vehicle.");
                return;
            }

            VehicleInventoryModData modData = JsonConvert.DeserializeObject<VehicleInventoryModData>(modItem.ItemValue);

            if (modData == null)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            if (player.Vehicle.FetchVehicleData().Name != modData.VehicleName)
            {
                player.SendInfoNotification($"You can only install this mod on a {modData.VehicleName}.");
                return;
            }

            int modSlot = modData.ModData.Key;
            int modType = modData.ModData.Value;

            if (modSlot == (int)VehicleModType.FrontWheels)
            {
                player.Vehicle.SetWheels((byte)modData.WheelType, (byte)modType);
            }

            bool itemRemoved = vehicleInventory.RemoveItem(modItem);

            if (!itemRemoved)
            {
                player.SendErrorNotification("There was an error removing the item from the vehicle inventory.");
                return;
            }

            using (Context context = new Context())
            {
                Models.Vehicle vDb = context.Vehicle.Find(player.Vehicle.FetchVehicleData().Id);

                if (vDb == null) return;

                Dictionary<int, int> modList = JsonConvert.DeserializeObject<Dictionary<int, int>>(vDb.VehicleMods);

                if (modList.ContainsKey(modSlot))
                {
                    modList.Remove(modSlot);
                }

                modList.Add(modSlot, modType);

                vDb.VehicleMods = JsonConvert.SerializeObject(modList);

                if (modSlot == (int)VehicleModType.FrontWheels)
                {
                    vDb.FrontWheelType = modData.WheelType;
                    vDb.FrontWheel = modType;
                }

                context.SaveChanges();
            }
            LoadVehicle.LoadVehicleMods(player.Vehicle);

            player.SendInfoNotification($"You've installed the {modData.ModName} onto the vehicle.");
        }

        [Command("removemod", commandType: CommandType.Focus, description: "Mechanic: Removes a mod from a vehicle")]
        public static void CommandRemoveMod(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle!");
                return;
            }

            var vehicleData = player.Vehicle.FetchVehicleData();

            if (vehicleData == null)
            {
                player.SendErrorNotification("You can't do that in this vehicle!");
                return;
            }

            List<FocusTypes> playerFocuses =
                JsonConvert.DeserializeObject<List<FocusTypes>>(player.FetchCharacter().FocusJson);

            if (!playerFocuses.Contains(FocusTypes.Mechanic))
            {
                player.SendErrorNotification("You don't have the mechanic focus!");
                return;
            }

            Dictionary<int, int> modList = JsonConvert.DeserializeObject<Dictionary<int, int>>(vehicleData.VehicleMods);

            if (!modList.Any())
            {
                player.SendInfoNotification($"This vehicle doesn't have any mods.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (var modData in modList)
            {
                if (modData.Key == 0)
                {
                    menuItems.Add(new NativeMenuItem("Spoiler"));
                }

                if (modData.Key == 1)
                {
                    menuItems.Add(new NativeMenuItem("Front Bumper"));
                }

                if (modData.Key == 2)
                {
                    menuItems.Add(new NativeMenuItem("Rear Bumper"));
                }

                if (modData.Key == 3)
                {
                    menuItems.Add(new NativeMenuItem("Side Skirt"));
                }

                if (modData.Key == 4)
                {
                    menuItems.Add(new NativeMenuItem("Exhaust"));
                }

                if (modData.Key == 5)
                {
                    menuItems.Add(new NativeMenuItem("Frame"));
                }

                if (modData.Key == 6)
                {
                    menuItems.Add(new NativeMenuItem("Grille"));
                }

                if (modData.Key == 7)
                {
                    menuItems.Add(new NativeMenuItem("Hood"));
                }

                if (modData.Key == 8)
                {
                    menuItems.Add(new NativeMenuItem("Fender"));
                }

                if (modData.Key == 9)
                {
                    menuItems.Add(new NativeMenuItem("Right Fender"));
                }

                if (modData.Key == 10)
                {
                    menuItems.Add(new NativeMenuItem("Roof"));
                }

                if (modData.Key == 11)
                {
                    menuItems.Add(new NativeMenuItem("Engine"));
                }

                if (modData.Key == 12)
                {
                    menuItems.Add(new NativeMenuItem("Brakes"));
                }

                if (modData.Key == 13)
                {
                    menuItems.Add(new NativeMenuItem("Transmission"));
                }

                if (modData.Key == 14)
                {
                    menuItems.Add(new NativeMenuItem("Horn"));
                }

                if (modData.Key == 15)
                {
                    menuItems.Add(new NativeMenuItem("Suspension"));
                }

                if (modData.Key == 16)
                {
                    menuItems.Add(new NativeMenuItem("Armor"));
                }

                if (modData.Key == 18)
                {
                    menuItems.Add(new NativeMenuItem("Turbo"));
                }

                if (modData.Key == 22)
                {
                    menuItems.Add(new NativeMenuItem("Xenon"));
                }

                if (modData.Key == 23)
                {
                    menuItems.Add(new NativeMenuItem("Front Wheels"));
                }

                if (modData.Key == 24)
                {
                    menuItems.Add(new NativeMenuItem("Back Wheels"));
                }

                if (modData.Key == 25)
                {
                    menuItems.Add(new NativeMenuItem("Plate Holder"));
                }

                if (modData.Key == 27)
                {
                    menuItems.Add(new NativeMenuItem("Trim Design"));
                }

                if (modData.Key == 28)
                {
                    menuItems.Add(new NativeMenuItem("Ornaments"));
                }

                if (modData.Key == 30)
                {
                    menuItems.Add(new NativeMenuItem("Dial Design"));
                }

                if (modData.Key == 33)
                {
                    menuItems.Add(new NativeMenuItem("Steering Wheel"));
                }

                if (modData.Key == 34)
                {
                    menuItems.Add(new NativeMenuItem("Shift Lever"));
                }

                if (modData.Key == 35)
                {
                    menuItems.Add(new NativeMenuItem("Plaques"));
                }

                if (modData.Key == 38)
                {
                    menuItems.Add(new NativeMenuItem("Hydraulics"));
                }

                if (modData.Key == 40)
                {
                    menuItems.Add(new NativeMenuItem("Boost"));
                }

                if (modData.Key == 48)
                {
                    menuItems.Add(new NativeMenuItem("Livery"));
                }

                if (modData.Key == 62)
                {
                    menuItems.Add(new NativeMenuItem("Plate"));
                }

                if (modData.Key == 69)
                {
                    menuItems.Add(new NativeMenuItem("Window Tint"));
                }

                if (modData.Key == 74)
                {
                    menuItems.Add(new NativeMenuItem("Dashboard Color"));
                }

                if (modData.Key == 75)
                {
                    menuItems.Add(new NativeMenuItem("Trim Color"));
                }
            }

            NativeMenu menu = new NativeMenu("focus:mechanic:onRemoveModSelect", "Mods", "Select a slot to remove",
                menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnRemoveModSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            int modSlot = -1;

            switch (option)
            {
                case "Spoiler":
                    modSlot = 0;
                    break;

                case "Front Bumper":
                    modSlot = 1;
                    break;

                case "Rear Bumper":
                    modSlot = 2;
                    break;

                case "Side Skirt":
                    modSlot = 3;
                    break;

                case "Exhaust":
                    modSlot = 4;
                    break;

                case "Frame":
                    modSlot = 5;
                    break;

                case "Grille":
                    modSlot = 6;
                    break;

                case "Hood":
                    modSlot = 7;
                    break;

                case "Fender":
                    modSlot = 8;
                    break;

                case "Right Fender":
                    modSlot = 9;
                    break;

                case "Roof":
                    modSlot = 10;
                    break;

                case "Engine":
                    modSlot = 11;
                    break;

                case "Brakes":
                    modSlot = 12;
                    break;

                case "Transmission":
                    modSlot = 13;
                    break;

                case "Horn":
                    modSlot = 14;
                    break;

                case "Suspension":
                    modSlot = 15;
                    break;

                case "Armor":
                    modSlot = 16;
                    break;

                case "Turbo":
                    modSlot = 18;
                    break;

                case "Xenon":
                    modSlot = 22;
                    break;

                case "Front Wheels":
                    modSlot = 23;
                    break;

                case "Back Wheels":
                    modSlot = 24;
                    break;

                case "Plate Holder":
                    modSlot = 25;
                    break;

                case "Trim Design":
                    modSlot = 27;
                    break;

                case "Ornaments":
                    modSlot = 28;
                    break;

                case "Dial Design":
                    modSlot = 30;
                    break;

                case "Steering Wheel":
                    modSlot = 33;
                    break;

                case "Shift Lever":
                    modSlot = 34;
                    break;

                case "Plaques":
                    modSlot = 35;
                    break;

                case "Hydraulics":
                    modSlot = 38;
                    break;

                case "Boost":
                    modSlot = 40;
                    break;

                case "Livery":
                    modSlot = 48;
                    break;

                case "Plate":
                    modSlot = 62;
                    break;

                case "Window Tint":
                    modSlot = 69;
                    break;

                case "Dashboard Color":
                    modSlot = 74;
                    break;

                case "Trim Color":
                    modSlot = 75;
                    break;
            }

            using Context context = new Context();
            var vDb = context.Vehicle.Find(player.Vehicle.FetchVehicleData().Id);

            if (vDb == null) return;

            Dictionary<int, int> modList = JsonConvert.DeserializeObject<Dictionary<int, int>>(vDb.VehicleMods);

            if (modList.ContainsKey(modSlot))
            {
                modList.Remove(modSlot);
            }

            vDb.VehicleMods = JsonConvert.SerializeObject(modList);

            context.SaveChanges();

            LoadVehicle.LoadVehicleMods(player.Vehicle);

            player.SendInfoNotification($"You've remove the {option} from the vehicle.");
        }

        [Command("respray", alternatives: "paint", commandType: CommandType.Focus, description: "Mechanic: Resprays a vehicle. /respray [1/2]")]
        public static void MechanicCommandRespray(IPlayer player, string args = "")
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (!player.IsInVehicle || player.Seat != 1)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/respray [Slot 1/2].");
                return;
            }

            bool tryParse = int.TryParse(args, out int slot);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/respray [Slot 1/2].");
                return;
            }

            List<FocusTypes> playerFocuses =
                JsonConvert.DeserializeObject<List<FocusTypes>>(player.FetchCharacter().FocusJson);

            if (!playerFocuses.Contains(FocusTypes.Mechanic))
            {
                player.SendErrorNotification("You don't have the mechanic focus type.");
                return;
            }

            ShowColorSelection(player, slot);
        }

        private static void ShowColorSelection(IPlayer player, int slot)
        {
            List<NativeListItem> listItems = new List<NativeListItem>();

            List<int> colorList = Enumerable.Range(0, 256).ToList();

            List<string> cList = colorList.ConvertAll<string>(x => x.ToString());

            NativeListItem R = new NativeListItem { Title = "Red", StringList = cList };
            NativeListItem G = new NativeListItem { Title = "Green", StringList = cList };
            NativeListItem B = new NativeListItem { Title = "Blue", StringList = cList };

            listItems.Add(R);
            listItems.Add(G);
            listItems.Add(B);

            NativeMenu menu = new NativeMenu("focus:mechanic:colorSelection", "Mod Shop", "Select the color value.")
            {
                ListTrigger = "focus:mechanic:resprayListChange",
                ListMenuItems = listItems,
                MenuItems = new List<NativeMenuItem> { new NativeMenuItem("Submit") }
            };

            player.SetData("focus:mechanic:colorChangeR", 0);
            player.SetData("focus:mechanic:colorChangeG", 0);
            player.SetData("focus:mechanic:colorChangeB", 0);
            player.SetData("focus:mechanic:colorChangeSlot", slot);

            if (slot == 1)
            {
                player.Vehicle.PrimaryColorRgb = new Rgba(0, 0, 0, 255);
            }
            else
            {
                player.Vehicle.SecondaryColorRgb = new Rgba(0, 0, 0, 255);
            }

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnResprayListChange(IPlayer player, string itemTitle, string colorValue)
        {
            player.GetData("focus:mechanic:colorChangeR", out int R);
            player.GetData("focus:mechanic:colorChangeG", out int G);
            player.GetData("focus:mechanic:colorChangeB", out int B);
            player.GetData("focus:mechanic:colorChangeSlot", out int slot);

            int newColor = int.Parse(colorValue);

            if (slot == 1)
            {
                if (itemTitle == "Red")
                {
                    player.SetData("focus:mechanic:colorChangeR", newColor);
                    player.Vehicle.PrimaryColorRgb = new Rgba((byte)newColor, (byte)G, (byte)B, 255);
                }
                if (itemTitle == "Green")
                {
                    player.SetData("focus:mechanic:colorChangeG", newColor);
                    player.Vehicle.PrimaryColorRgb = new Rgba((byte)R, (byte)newColor, (byte)B, 255);
                }
                if (itemTitle == "Blue")
                {
                    player.SetData("focus:mechanic:colorChangeB", newColor);
                    player.Vehicle.PrimaryColorRgb = new Rgba((byte)R, (byte)G, (byte)newColor, 255);
                }
            }

            if (slot == 2)
            {
                if (itemTitle == "Red")
                {
                    player.SetData("focus:mechanic:colorChangeR", newColor);
                    player.Vehicle.SecondaryColorRgb = new Rgba((byte)newColor, (byte)G, (byte)B, 255);
                }
                if (itemTitle == "Green")
                {
                    player.SetData("focus:mechanic:colorChangeG", newColor);
                    player.Vehicle.SecondaryColorRgb = new Rgba((byte)R, (byte)newColor, (byte)B, 255);
                }
                if (itemTitle == "Blue")
                {
                    player.SetData("focus:mechanic:colorChangeB", newColor);
                    player.Vehicle.SecondaryColorRgb = new Rgba((byte)R, (byte)G, (byte)newColor, 255);
                }
            }
        }

        public static void OnResprayMenuSelect(IPlayer player, string option)
        {
            try
            {
                if (option == "Close")
                {
                    LoadVehicle.LoadVehicleMods(player.Vehicle);
                    return;
                }

                player.GetData("focus:mechanic:colorChangeR", out int R);
                player.GetData("focus:mechanic:colorChangeG", out int G);
                player.GetData("focus:mechanic:colorChangeB", out int B);
                player.GetData("focus:mechanic:colorChangeSlot", out int slot);

                using Context context = new Context();

                Models.Vehicle vehicleDb = context.Vehicle.Find(player.Vehicle.FetchVehicleData().Id);

                if (vehicleDb == null) return;

                if (slot == 1)
                {
                    vehicleDb.Color1 = $"{R},{G},{B}";
                }

                if (slot == 2)
                {
                    vehicleDb.Color2 = $"{R},{G},{B}";
                }

                context.SaveChanges();

                

                LoadVehicle.LoadVehicleMods(player.Vehicle);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        private static double _basicWheelColor = 100;
        private static double _luxuryWheelColor = 600;

        [Command("respraywheels", commandType: CommandType.Focus, description: "Mechanic: Resprays wheels")]
        public static void MechanicCommandResprayWheels(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (!player.IsInVehicle || player.Seat != 1)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            List<FocusTypes> playerFocuses =
                JsonConvert.DeserializeObject<List<FocusTypes>>(player.FetchCharacter().FocusJson);

            if (!playerFocuses.Contains(FocusTypes.Mechanic))
            {
                player.SendErrorNotification("You don't have the mechanic focus type.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (string color in ModHandler.FetchVehicleColorList())
            {
                menuItems.Add(color.Contains("Gold")
                    ? new NativeMenuItem(color, $"Cost: {_luxuryWheelColor:C}")
                    : new NativeMenuItem(color, $"Cost: {_basicWheelColor:C}"));
            }

            NativeMenu menu = new NativeMenu("focus:mechanic:selectWheelColor", "Wheel Respray", "Select a color.", menuItems)
            {
                ItemChangeTrigger = "focus:mechanic:OnWheelResprayChange",
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnWheelResprayChange(IPlayer player, int index, string option)
        {
            if (option == "Close") return;

            player.Vehicle.WheelColor = (byte)index;
        }

        public static void OnWheelRespraySelect(IPlayer player, string option, int index)
        {
            if (option == "Close")
            {
                LoadVehicle.LoadVehicleMods(player.Vehicle);
                return;
            }

            double cost = _basicWheelColor;

            if (option.Contains("Gold"))
            {
                cost = _luxuryWheelColor;
            }

            if (player.GetClass().Cash < cost)
            {
                player.SendErrorNotification($"You don't have enough. You require {cost:C}.");
                return;
            }

            using Context context = new Context();

            var vehicleDb = context.Vehicle.Find(player.Vehicle.GetClass().Id);

            if (vehicleDb == null)
            {
                player.SendErrorNotification("You can't do that in this vehicle.");
                return;
            }

            vehicleDb.WheelColor = index;

            context.SaveChanges();

            

            player.SendInfoNotification($"You've resprayed the wheels to {ModHandler.FetchColorName((byte)index)}. This has cost you {cost:C}.");

            LoadVehicle.LoadVehicleMods(player.Vehicle);
        }

        #endregion Mechanic Focus Commands
    }
}