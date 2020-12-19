using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Character;
using Server.Character.Clothing;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Groups.Fire;
using Server.Groups.Police;
using Server.Inventory;
using Server.Models;

namespace Server.Groups
{
    public class DutyHandler
    {
        private static List<DutyPosition> _dutyPositions = new List<DutyPosition>
        {
            // Vespucci PD Car Park
            new DutyPosition(new Position(454.37802f, -989.5121f, 30.678345f), DutyPositionType.Law),

            // El Burro FD
            new DutyPosition(new Position(1191.854f, -1474.793f, 34.85953f), DutyPositionType.Medical )
        };

        [Command("duty")]
        public static void CommandDuty(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            Faction activeFaction = Faction.FetchFaction(playerCharacter.ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You've not set an active faction!");
                return;
            }

            DutyPosition nearestDutyPosition =
                _dutyPositions.FirstOrDefault(x => x.Position.Distance(player.Position) <= 3f);

            if (nearestDutyPosition == null)
            {
                player.SendErrorNotification("You're not near a duty location!");
                return;
            }

            switch (nearestDutyPosition.PositionType)
            {
                case DutyPositionType.Law when activeFaction.SubFactionType != SubFactionTypes.Law:
                    player.SendPermissionError();
                    return;

                case DutyPositionType.Medical when activeFaction.SubFactionType != SubFactionTypes.Medical:
                    player.SendPermissionError();
                    return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                playerCharacter.FactionDuty
                    ? new NativeMenuItem("Go Off Duty")
                    : new NativeMenuItem("Go On Duty"),
            };

            if (activeFaction.SubFactionType == SubFactionTypes.Law &&
                nearestDutyPosition.PositionType == DutyPositionType.Law && playerCharacter.FactionDuty)
            {
                // Law Options

                menuItems.Add(new NativeMenuItem("Duty Uniform"));
                menuItems.Add(new NativeMenuItem("Patrol Equipment"));
                menuItems.Add(new NativeMenuItem("Shotgun"));
                menuItems.Add(new NativeMenuItem("Assault Rifle"));
                //menuItems.Add(new NativeMenuItem("Detective Loadout"));
                menuItems.Add(new NativeMenuItem("SWAT Equipment"));
            }

            if (activeFaction.SubFactionType == SubFactionTypes.Medical &&
                nearestDutyPosition.PositionType == DutyPositionType.Medical && playerCharacter.FactionDuty)
            {
                // Medical Options

                menuItems.Add(new NativeMenuItem("Medical Uniform"));
                menuItems.Add(new NativeMenuItem("Fireman Uniform"));
                menuItems.Add(new NativeMenuItem("Hatchet"));
                menuItems.Add(new NativeMenuItem("Flashlight"));
                menuItems.Add(new NativeMenuItem("Crowbar"));
                menuItems.Add(new NativeMenuItem("Fire Extinguisher"));
            }

            NativeMenu dutyMenu = new NativeMenu("faction:duty:MainMenu", "Duty", "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, dutyMenu, true);
        }

        public static void OnMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            Faction activeFaction = Faction.FetchFaction(playerCharacter.ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You've not set an active faction!");
                return;
            }

            if (option == "Go Off Duty")
            {
                using Context context = new Context();

                Models.Character characterDb = context.Character.Find(playerCharacter.Id);

                if (characterDb == null)
                {
                    player.SendErrorNotification("An error occurred fetching your data.");
                    return;
                }

                characterDb.FactionDuty = false;
                characterDb.DutyStatus = 0;

                if (activeFaction.SubFactionType == SubFactionTypes.Law)
                {
                    player.DeleteData("Police:SwatUniform");
                    player.DeleteMetaData("PoliceDuty");
                    PoliceHandler.RemovePoliceItems(player);
                }

                if (activeFaction.SubFactionType == SubFactionTypes.Medical)
                {
                    FireHandler.RemoveFireItems(player);
                }

                bool hasUnitData = player.GetData("mdc:UnitName", out string unitName);

                if (hasUnitData && !string.IsNullOrEmpty(unitName))
                {
                    player.SetData("mdc:UnitName", "");
                }

                context.SaveChanges();

                player.SetData("FACTION:DUTYCLOTHING", "");

                CustomCharacter.LoadCustomCharacter(player);

                player.SendInfoNotification($"You've gone off duty.");
                Logging.AddToCharacterLog(player, "Has gone off duty.");
            }

            if (option == "Go On Duty")
            {
                bool hasCurrentWeaponData = player.GetData("CurrentWeaponHash", out uint weaponHash);

                if (hasCurrentWeaponData && weaponHash != 0)
                {
                    player.SendErrorNotification("You have a weapon equipped. Please un-equip it first.");
                    return;
                }

                using Context context = new Context();

                Models.Character characterDb = context.Character.Find(playerCharacter.Id);

                if (characterDb == null)
                {
                    player.SendErrorNotification("An error occurred fetching your data.");
                    return;
                }

                characterDb.FactionDuty = true;

                context.SaveChanges();

                if (activeFaction.SubFactionType == SubFactionTypes.Law)
                {
                }

                player.SendInfoNotification($"You've gone on duty.");

                Logging.AddToCharacterLog(player, "Has gone on duty.");
            }

            if (option == "Duty Uniform")
            {
                List<ClothesData> clothingData =
                    JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson);

                if (activeFaction.SubFactionType == SubFactionTypes.Law)
                {
                    using Context context = new Context();

                    Models.Character uniformCharacter = context.Character.Find(player.GetClass().CharacterId);

                    uniformCharacter.DutyStatus = 1;

                    context.SaveChanges();

                    player.SetSyncedMetaData("PoliceDuty", true);

                    #region Law Clothing

                    // Police
                    if (playerCharacter.Sex == 0)
                    {
                        // Male

                        foreach (ClothesData clothesData in clothingData)
                        {
                            if (clothesData.slot == 11)
                            {
                                clothesData.drawable = 55;
                                clothesData.texture = 0;
                                continue;
                            }
                            if (clothesData.slot == 6)
                            {
                                clothesData.drawable = 25;
                                clothesData.texture = 0;
                                continue;
                            }
                            if (clothesData.slot == 4)
                            {
                                clothesData.drawable = 35;
                                clothesData.texture = 0;
                                continue;
                            }
                            if (clothesData.slot == 8)
                            {
                                clothesData.drawable = 58;
                                clothesData.texture = 0;
                                continue;
                            }
                        }

                        player.SetData("FACTION:DUTYCLOTHING", JsonConvert.SerializeObject(clothingData));
                        Clothes.LoadClothes(player, clothingData, JsonConvert.DeserializeObject<List<AccessoryData>>(playerCharacter.AccessoryJson));
                    }
                    else
                    {
                        // Female

                        foreach (ClothesData clothesData in clothingData)
                        {
                            if (clothesData.slot == 11)
                            {
                                clothesData.drawable = 48;
                                clothesData.texture = 0;
                                continue;
                            }
                            if (clothesData.slot == 6)
                            {
                                clothesData.drawable = 25;
                                clothesData.texture = 0;
                                continue;
                            }
                            if (clothesData.slot == 4)
                            {
                                clothesData.drawable = 34;
                                clothesData.texture = 0;
                                continue;
                            }
                            if (clothesData.slot == 8)
                            {
                                clothesData.drawable = 152;
                                clothesData.texture = 0;
                                continue;
                            }
                        }
                        player.SetData("FACTION:DUTYCLOTHING", JsonConvert.SerializeObject(clothingData));
                        Clothes.LoadClothes(player, clothingData, JsonConvert.DeserializeObject<List<AccessoryData>>(playerCharacter.AccessoryJson));
                    }

                    #endregion Law Clothing
                }
            }

            #region Police Options

            if (option == "Patrol Equipment")
            {
                PoliceHandler.AddPoliceItems(player, 0);

                player.Armor = 100;

                player.SendInfoNotification($"Patrol Equipment Given");

                Logging.AddToCharacterLog(player, "Has taken Duty Equipment.");
            }

            if (option == "Shotgun")
            {
                PoliceHandler.AddPoliceItems(player, 1);
                player.SendInfoNotification($"Shotgun Given");

                Logging.AddToCharacterLog(player, "Has taken a Shotgun from LSPD");
            }
            if (option == "Assault Rifle")
            {
                PoliceHandler.AddPoliceItems(player, 2);
                player.SendInfoNotification($"Assault Rifle Given");

                Logging.AddToCharacterLog(player, "Has taken a Shotgun from LSPD");
            }

            if (option == "SWAT Equipment")
            {
                // SWAT Patrol Equipment

                player.RemoveAllWeapons();

                player.Model = (uint)PedModel.Swat01SMY;

                player.SetData("Police:SwatUniform", true);

                player.Armor = 100;

                player.SendInfoNotification($"SWAT Equipment Received!");

                Logging.AddToCharacterLog(player, "Has taken SWAT Equipment.");

                player.Position = player.Position;
            }

            #endregion Police Options

            #region Fire Department Weapons

            if (option == "Medical Uniform")
            {
                CustomCharacter.LoadCustomCharacter(player);

                using Context context = new Context();

                Models.Character uniformCharacter = context.Character.Find(player.GetClass().CharacterId);

                uniformCharacter.DutyStatus = 1;

                context.SaveChanges();

                List<ClothesData> clothingData =
                    JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson);

                #region Medical Clothing

                if (playerCharacter.Sex == 0)
                {
                    // Male

                    foreach (ClothesData clothesData in clothingData)
                    {
                        if (clothesData.slot == 11)
                        {
                            clothesData.drawable = 250;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 6)
                        {
                            clothesData.drawable = 25;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 4)
                        {
                            clothesData.drawable = 96;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 8)
                        {
                            clothesData.drawable = 129;
                            clothesData.texture = 0;
                            continue;
                        }

                        if (clothesData.slot == 3)
                        {
                            clothesData.drawable = 0;
                            clothesData.texture = 0;
                            continue;
                        }

                        if (clothesData.slot == 10)
                        {
                            clothesData.drawable = 58;
                            clothesData.texture = 0;
                        }
                    }

                    player.SetData("FACTION:DUTYCLOTHING", JsonConvert.SerializeObject(clothingData));
                    Clothes.LoadClothes(player, clothingData, JsonConvert.DeserializeObject<List<AccessoryData>>(playerCharacter.AccessoryJson));
                }
                else
                {
                    // Female

                    foreach (ClothesData clothesData in clothingData)
                    {
                        if (clothesData.slot == 11)
                        {
                            clothesData.drawable = 258;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 6)
                        {
                            clothesData.drawable = 25;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 4)
                        {
                            clothesData.drawable = 99;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 8)
                        {
                            clothesData.drawable = 159;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 3)
                        {
                            clothesData.drawable = 0;
                            clothesData.texture = 0;
                            continue;
                        }

                        if (clothesData.slot == 10)
                        {
                            clothesData.drawable = 66;
                            clothesData.texture = 0;
                        }
                    }
                    player.SetData("FACTION:DUTYCLOTHING", JsonConvert.SerializeObject(clothingData));
                    Clothes.LoadClothes(player, clothingData, JsonConvert.DeserializeObject<List<AccessoryData>>(playerCharacter.AccessoryJson));
                }

                #endregion Medical Clothing
            }

            if (option == "Fireman Uniform")
            {
                using Context context = new Context();

                Models.Character uniformCharacter = context.Character.Find(player.GetClass().CharacterId);

                uniformCharacter.DutyStatus = 2;

                context.SaveChanges();

                List<ClothesData> clothingData =
                    JsonConvert.DeserializeObject<List<ClothesData>>(playerCharacter.ClothesJson);
                List<AccessoryData> accessoryData =
                    JsonConvert.DeserializeObject<List<AccessoryData>>(playerCharacter.AccessoryJson);

                #region Fire Clothing

                if (playerCharacter.Sex == 0)
                {
                    // Male

                    foreach (ClothesData clothesData in clothingData)
                    {
                        if (clothesData.slot == 11)
                        {
                            clothesData.drawable = 314;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 6)
                        {
                            clothesData.drawable = 82;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 4)
                        {
                            clothesData.drawable = 120;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 8)
                        {
                            clothesData.drawable = 0;
                            clothesData.texture = 0;
                            continue;
                        }

                        if (clothesData.slot == 3)
                        {
                            clothesData.drawable = 0;
                            clothesData.texture = 0;
                            continue;
                        }

                        if (clothesData.slot == 10)
                        {
                            clothesData.drawable = 64;
                            clothesData.texture = 0;
                        }
                    }

                    foreach (AccessoryData data in accessoryData)
                    {
                        if (data.slot == 0)
                        {
                            // Hats
                            data.drawable = 137;
                            data.texture = 0;
                        }
                    }

                    player.SetData("FACTION:DUTYCLOTHING", JsonConvert.SerializeObject(clothingData));
                    player.SetData("FACTION:DUTYACCESSORY", JsonConvert.SerializeObject(accessoryData));
                    Clothes.LoadClothes(player, clothingData, accessoryData);
                }
                else
                {
                    // Female

                    foreach (ClothesData clothesData in clothingData)
                    {
                        if (clothesData.slot == 11)
                        {
                            clothesData.drawable = 325;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 6)
                        {
                            clothesData.drawable = 86;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 4)
                        {
                            clothesData.drawable = 126;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 8)
                        {
                            clothesData.drawable = 0;
                            clothesData.texture = 0;
                            continue;
                        }
                        if (clothesData.slot == 3)
                        {
                            clothesData.drawable = 0;
                            clothesData.texture = 0;
                            continue;
                        }

                        if (clothesData.slot == 10)
                        {
                            clothesData.drawable = 64;
                            clothesData.texture = 0;
                        }
                    }

                    foreach (AccessoryData data in accessoryData)
                    {
                        if (data.slot == 0)
                        {
                            // Hats
                            data.drawable = 137;
                            data.texture = 0;
                        }
                    }

                    player.SetData("FACTION:DUTYCLOTHING", JsonConvert.SerializeObject(clothingData));
                    player.SetData("FACTION:DUTYACCESSORY", JsonConvert.SerializeObject(accessoryData));
                    Clothes.LoadClothes(player, clothingData, accessoryData);
                }

                #endregion Fire Clothing
            }

            WeaponInfo generalInfo = new WeaponInfo(1, true, "LSFD");

            if (option == "Flashlight")
            {
                Inventory.Inventory playerInventory = player.FetchInventory();

                bool success = playerInventory.AddItem(new InventoryItem("ITEM_FIRE_WEAPON_FLASHLIGHT", "Flashlight", generalInfo.ToString()));

                if (!success)
                {
                    player.SendErrorNotification("Unable to add the item to your inventory.");
                    return;
                }
                player.SendInfoNotification($"You've taken a Flashlight from the locker.");
            }

            if (option == "Hatchet")
            {
                Inventory.Inventory playerInventory = player.FetchInventory();

                bool success = playerInventory.AddItem(new InventoryItem("ITEM_FIRE_WEAPON_HATCHET", "Hatchet", generalInfo.ToString()));

                if (!success)
                {
                    player.SendErrorNotification("Unable to add the item to your inventory.");
                    return;
                }
                player.SendInfoNotification($"You've taken a Hatchet from the locker.");
            }
            if (option == "Crowbar")
            {
                Inventory.Inventory playerInventory = player.FetchInventory();

                bool success = playerInventory.AddItem(new InventoryItem("ITEM_FIRE_WEAPON_CROWBAR", "Crowbar", generalInfo.ToString()));

                if (!success)
                {
                    player.SendErrorNotification("Unable to add the item to your inventory.");
                    return;
                }

                player.SendInfoNotification($"You've taken a Crowbar from the locker.");
            }
            if (option == "Fire Extinguisher")
            {
                Inventory.Inventory playerInventory = player.FetchInventory();

                generalInfo.AmmoCount = 1000;

                bool success = playerInventory.AddItem(new InventoryItem("ITEM_FIRE_WEAPON_FIRE_EXTINGUISHER", "Hatchet", generalInfo.ToString()));

                if (!success)
                {
                    player.SendErrorNotification("Unable to add the item to your inventory.");
                    return;
                }

                player.SendInfoNotification($"You've taken a Fire Extinguisher from the locker.");
            }

            #endregion Fire Department Weapons
        }
    }
}