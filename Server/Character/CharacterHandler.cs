using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Character.Clothing;
using Server.Character.Tattoo;
using Server.Extensions;
using Server.Inventory;
using Server.Models;
using Server.Weapons;

namespace Server.Character
{
    public class CharacterHandler
    {
        public static int NextMaskId = 1000;

        /// <summary>
        /// AFK time in minutes
        /// </summary>
        private static readonly int _afkTime = 30;

        public static void LoadCustomCharacter(IPlayer player, bool loadWeapons = false, bool reloadPosition = false)
        {
            Models.Character selectedCharacter = player.FetchCharacter();

            if (player.GetClass().AdminDuty)
            {
                if (reloadPosition)
                {
                    player.Position = player.Position;
                }
                else return;
            }

            if (JsonConvert.DeserializeObject<CustomCharacter>(selectedCharacter.CustomCharacter).Gender == 0)
            {
                if (player.Model != (uint)PedModel.FreemodeMale01)
                {
                    player.Model = (uint)PedModel.FreemodeMale01;
                }
            }
            else
            {
                if (player.Model != (uint)PedModel.FreemodeFemale01)
                {
                    player.Model = (uint)PedModel.FreemodeFemale01;
                }
            }

            player.Emit("loadCustomPlayer", selectedCharacter.CustomCharacter, selectedCharacter.ClothesJson, selectedCharacter.AccessoryJson);

            List<ClothesData> clothingData =
                JsonConvert.DeserializeObject<List<ClothesData>>(selectedCharacter.ClothesJson);

            List<AccessoryData> accessoryData =
                JsonConvert.DeserializeObject<List<AccessoryData>>(selectedCharacter.AccessoryJson);

            Clothes.LoadClothes(player, clothingData, accessoryData);

            TattooHandler.LoadTattoos(player);

            if (selectedCharacter.FactionDuty)
            {
                Faction activeFaction = Faction.FetchFaction(selectedCharacter.ActiveFaction);

                if (activeFaction == null) return;

                if (activeFaction.SubFactionType == SubFactionTypes.Law)
                {
                    if (player.HasData("Police:SwatUniform"))
                    {
                        player.Model = (uint)PedModel.Swat01SMY;
                    }

                    if (selectedCharacter.DutyStatus == 1)
                    {
                        #region Law Clothing

                        // Police
                        if (selectedCharacter.Sex == 0)
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
                            Clothes.LoadClothes(player, clothingData, JsonConvert.DeserializeObject<List<AccessoryData>>(selectedCharacter.AccessoryJson));
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
                            Clothes.LoadClothes(player, clothingData, JsonConvert.DeserializeObject<List<AccessoryData>>(selectedCharacter.AccessoryJson));
                        }

                        #endregion Law Clothing
                    }
                }

                if (activeFaction.SubFactionType == SubFactionTypes.Medical)
                {
                    if (selectedCharacter.DutyStatus == 1)
                    {
                        #region Medical Clothing

                        if (selectedCharacter.Sex == 0)
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
                            Clothes.LoadClothes(player, clothingData, JsonConvert.DeserializeObject<List<AccessoryData>>(selectedCharacter.AccessoryJson));
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
                            Clothes.LoadClothes(player, clothingData, JsonConvert.DeserializeObject<List<AccessoryData>>(selectedCharacter.AccessoryJson));
                        }

                        #endregion Medical Clothing
                    }

                    if (selectedCharacter.DutyStatus == 2)
                    {
                        // Fire outfit

                        #region Firemans Outfit

                        if (selectedCharacter.Sex == 0)
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
                                    clothesData.drawable = 151;
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
                                    clothesData.drawable = 187;
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

                        #endregion Firemans Outfit
                    }
                }
            }

            if (loadWeapons)
            {
                if (!string.IsNullOrEmpty(selectedCharacter.CurrentWeapon))
                {
                    InventoryItem currentWeaponItem = JsonConvert.DeserializeObject<InventoryItem>(selectedCharacter.CurrentWeapon);

                    if (currentWeaponItem != null)
                    {
                        WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(currentWeaponItem.ItemValue);

                        EquipItem.EquipWeapon(player, player.FetchInventory(), currentWeaponItem, weaponInfo.AmmoCount);
                    }
                }
            }

            if (reloadPosition)
            {
                player.Position = player.Position;
            }

            player.SetClothes(5, 0, 0);

            if (selectedCharacter.BackpackId > 0)
            {
                Models.Backpack backpack = Models.Backpack.FetchBackpack(selectedCharacter.BackpackId);

                if (backpack == null)
                {
                    player.SendNotification("~r~There was an error fetching the backpack.");
                    return;
                }

                player.SetClothes(5, backpack.Drawable, 0);
            }

            player.Emit("SetWalkStyle", selectedCharacter.WalkStyle);
        }

        public static void SaveCharacterPosition(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            using Context context = new Context();

            Models.Character character = context.Character.Find(playerCharacter.Id);

            character.PosX = player.Position.X;
            character.PosY = player.Position.Y;
            character.PosZ = player.Position.Z;

            context.SaveChanges();

            OfficerType officerType = player.GetClass().CurrentDuty;

            if (officerType == OfficerType.Response)
            {
                //Do this
            }

            int afoCount = 0;
            foreach (var target in Alt.GetAllPlayers())
            {
                lock (target)
                {
                    if (target.GetClass().CurrentDuty != OfficerType.AFO) continue;
                    afoCount++;
                }
            }
            Console.Write($"Total AFO Count: {afoCount}");
        }

        public static void CheckPlayerAfk()
        {
            string lastTimeData = "AFK:LastMove";
            string lastPositionData = "AFK:LastPosition";

            List<IPlayer> onlinePlayers = Alt.GetAllPlayers().ToList();

            foreach (IPlayer player in onlinePlayers)
            {
                Models.Account pAccount = player?.FetchAccount();

                if (pAccount == null) continue;

                if (player.GetClass().EditingCharacter) continue;

                if (pAccount.AdminLevel == AdminLevel.Director) continue;

                Position playerPosition = Position.Zero;

                lock (player)
                {
                    playerPosition = player.Position;
                }

                bool hasLastTime = player.GetData(lastTimeData, out DateTime lastMove);
                bool hasLastPos = player.GetData(lastPositionData, out Position lastPosition);

                if (!hasLastTime || !hasLastPos)
                {
                    player.SetData(lastTimeData, DateTime.Now);
                    player.SetData(lastPositionData, playerPosition);
                    continue;
                }

                if (lastPosition.Distance(playerPosition) >= 1.5)
                {
                    // Has moved more than 3 meters since last check
                    player.SetData(lastTimeData, DateTime.Now);
                    player.SetData(lastPositionData, playerPosition);
                    continue;
                }

                if (DateTime.Compare(DateTime.Now, lastMove.AddMinutes(_afkTime)) <= 0) continue;

                // Not moved for 10 minutes or had an update
                Logging.AddToCharacterLog(player, "has been kicked for AFK.");
                using Context context = new Context();

                Models.Account playerAccount = context.Account.Find(player.GetClass().AccountId);

                playerAccount.AfkKicks++;

                context.SaveChanges();

                player.Kick("AFK Limit Reached");

                Console.WriteLine($"{playerAccount.Username} has been kicked for AFK.");
            }

            Console.WriteLine($"Checked {onlinePlayers.Count} Online Players for AFK.");
        }

        public static void FetchPlayerDimension(IPlayer player)
        {
            player.Emit("SendPlayerDimension", player.Dimension);
        }
    }
}