using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Timers;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Admin;
using Server.Character;
using Server.Chat;
using Server.Inventory;
using Server.Language;
using Server.Models;
using Server.Weapons;

namespace Server.Extensions
{
    public static class PlayerExtension
    {
        public static Position PositionInFront(this IPlayer player, int distance)
        {
            try
            {
                Position position = player.Position;
                Rotation playerRot = player.Rotation;

                double z = playerRot.Yaw * (Math.PI / 180);
                double x = playerRot.Pitch * (Math.PI / 180);
                double num = Math.Abs(Math.Cos(x));

                Position forwardVector = new Position(Convert.ToSingle(-Math.Sin(z) * num), Convert.ToSingle(Math.Cos(z) * num), Convert.ToSingle(Math.Sin(x)));
                Position scaledVector = new Position(forwardVector.X * distance, forwardVector.Y * distance, forwardVector.Z * distance);

                return new Position(position.X + scaledVector.X, position.Y + scaledVector.Y, position.Z + scaledVector.Z);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Position.Zero;
            }
        }

        public static void SetPlayerNameTag(this IPlayer player, string name)
        {
            player.SetSyncedMetaData("playerNameTag", $"{name}");
        }

        /// <summary>
        /// Sets the Players ID
        /// </summary>
        /// <param name="player"></param>
        /// <param name="id"></param>
        public static void SetPlayerId(this IPlayer player, int id)
        {
            player.SetData("PLAYERID", id);
            player.SetSyncedMetaData("playerId", id);
        }

        /// <summary>
        /// Gets the Players ID
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetPlayerId(this IPlayer player)
        {
            bool hasData = player.GetData("PLAYERID", out int result);
            return !hasData ? 0 : result;
        }

        /// <summary>
        /// Freezes a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="frozen"></param>
        public static void FreezePlayer(this IPlayer player, bool frozen)
        {
            player.SetData("frozen", frozen);
            player.Emit("freezePlayer", frozen);
        }

        /// <summary>
        /// Returns state if frozen
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsFrozen(this IPlayer player)
        {
            bool hasData = player.GetData("frozen", out bool frozen);

            return hasData && frozen;
        }

        /// <summary>
        /// Toggles the state of the HUD & Minimap
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state">True = show hud</param>
        public static void ShowHud(this IPlayer player, bool state)
        {
            player.Emit("toggleHud", state);
            player.Emit("hud:SetState", state);
        }

        /// <summary>
        /// Toggles the state of Chat Input
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state">False = Disabled</param>
        public static void ChatInput(this IPlayer player, bool state)
        {
            player.Emit("toggleChat", state);
        }

        /// <summary>
        /// Toggles the visibility of Chat
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state">True = hidden</param>
        public static void HideChat(this IPlayer player, bool state)
        {
            player.Emit("showChat", state);
        }

        /// <summary>
        /// Sets the camera to frozen
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state">True = Frozen</param>
        public static void FreezeCam(this IPlayer player, bool state)
        {
            player.Emit("freezeCam", state);
        }

        /// <summary>
        /// Freezes the players control input
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state">True = freeze input</param>
        public static void FreezeInput(this IPlayer player, bool state)
        {
            player.Emit("freezeInput", state);
        }

        /// <summary>
        /// Sets the users account ID
        /// </summary>
        /// <param name="player"></param>
        /// <param name="accountId"></param>
        [Obsolete("Please use player.GetClass.AccountId()")]
        public static void SetAccountId(this IPlayer player, int accountId)
        {
            player.SetData("USERACCOUNTID", accountId);
        }

        /// <summary>
        /// Returns the users account ID
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        [Obsolete("Please use player.GetClass.AccountId()")]
        public static int FetchAccountId(this IPlayer player)
        {
            player.GetData("USERACCOUNTID", out int accountId);

            return accountId;
        }

        /// <summary>
        /// Fetches the users Account
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Models.Account FetchAccount(this IPlayer player)
        {
            player.GetData("USERACCOUNTID", out int accountId);

            return Models.Account.FindAccountById(accountId);
        }

        /// <summary>
        /// Sets the users Character ID
        /// </summary>
        /// <param name="player"></param>
        /// <param name="characterId"></param>
        public static void SetCharacterId(this IPlayer player, int characterId)
        {
            player.SetData("USERCHARACTERID", characterId);
        }

        /// <summary>
        /// Fetches the users current character ID
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        [Obsolete("Please use player.GetClass.CharacterId()")]
        public static int FetchCharacterId(this IPlayer player)
        {
            player.GetData("USERCHARACTERID", out int characterId);

            return characterId;
        }

        /// <summary>
        /// Fetches the players current Character
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Models.Character FetchCharacter(this IPlayer player)
        {
            bool hasData = player.GetData("USERCHARACTERID", out int characterId);
            if (!hasData) return null;

            return Models.Character.GetCharacter(characterId);
        }

        /// <summary>
        /// Shows the players Cursor
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state"></param>
        public static void ShowCursor(this IPlayer player, bool state)
        {
            player.Emit("toggleCursor", state);
        }

        /// <summary>
        /// Sets players clothes
        /// </summary>
        /// <param name="player"></param>
        /// <param name="slot"></param>
        /// <param name="drawable"></param>
        /// <param name="texture"></param>
        public static void SetClothes(this IPlayer player, int slot, int drawable, int texture)
        {
            player.Emit("setPlayerClothes", slot, drawable, texture);
        }

        /// <summary>
        /// Sets player accessory
        /// </summary>
        /// <param name="player"></param>
        /// <param name="slot"></param>
        /// <param name="drawable"></param>
        /// <param name="texture"></param>
        [Obsolete("Please use .SetAccessory", true)]
        public static void SetAccessories(this IPlayer player, int slot, int drawable, int texture)
        {
            SetAccessory(player, slot, drawable, texture);
        }

        /// <summary>
        /// Sets player accessory
        /// </summary>
        /// <param name="player"></param>
        /// <param name="slot"></param>
        /// <param name="drawable"></param>
        /// <param name="texture"></param>
        public static void SetAccessory(this IPlayer player, int slot, int drawable, int texture)
        {
            player.Emit("setPlayerAccessory", slot, drawable, texture);
        }

        /// <summary>
        /// Toggles the ability to use Alt to load the mouse and click
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state"></param>
        public static void AllowMouseContextMenu(this IPlayer player, bool state)
        {
            player.Emit("toggleContextMouse", state);
        }

        /// <summary>
        /// Request an IPL be loaded for a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ipl"></param>
        public static void RequestIpl(this IPlayer player, string ipl)
        {
            player.Emit("loadIpl", ipl);
        }

        /// <summary>
        /// Request an IPL to be unloaded for a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ipl"></param>
        public static void UnloadIpl(this IPlayer player, string ipl)
        {
            player.Emit("unloadIpl", ipl);
        }

        /// <summary>
        /// Request an Interior Prop to be loaded for a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="propName"></param>
        public static void LoadInteriorProp(this IPlayer player, string propName)
        {
            player.Emit("loadProp", propName);
        }

        /// <summary>
        /// Request an Interior Prop to be unloaded for a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="propName"></param>
        public static void UnloadInteriorProp(this IPlayer player, string propName)
        {
            player.Emit("unloadProp", propName);
        }

        /// <summary>
        /// Fetches the player Inventory
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Inventory.Inventory FetchInventory(this IPlayer player)
        {
            return player.FetchCharacter() == null ? null : new Inventory.Inventory(InventoryData.GetInventoryData(player.FetchCharacter().InventoryID));
        }

        public static Inventory.Inventory FetchBackpackInventory(this IPlayer player)
        {
            Models.Backpack currentBackpack = Models.Backpack.FetchBackpack(player.FetchCharacter().BackpackId);

            if (currentBackpack == null)
            {
                return null;
            }

            return Models.Backpack.FetchBackpackInventory(currentBackpack);
        }

        /// <summary>
        /// Returns characters name
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        [Obsolete("Please use player.GetClass.Name()")]
        public static string Name(this IPlayer player)
        {
            Models.Character playerCharacter = FetchCharacter(player);

            return playerCharacter?.Name;
        }

        /// <summary>
        /// Returns if player is male
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        [Obsolete("Please use player.GetClass.IsMale()")]
        public static bool IsMale(this IPlayer player)
        {
            return player.FetchCharacter().Sex == 0;
        }

        /// <summary>
        /// Loads the custom player custom
        /// </summary>
        /// <param name="player"></param>
        public static void LoadCharacterCustomization(this IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            CharacterHandler.LoadCustomCharacter(player, reloadPosition: true);

            //player.Emit("loadCustomPlayer", playerCharacter.CustomCharacter, playerCharacter.ClothesJson, playerCharacter.AccessoryJson);
        }

        /// <summary>
        /// Sets a player spawned. True - spawned
        /// </summary>
        /// <param name="player"></param>
        /// <param name="spawned">Sets a player spawned. True = spawned</param>
        [Obsolete("Please use player.GetClass.Spawned()")]
        public static void SetSpawned(this IPlayer player, bool spawned)
        {
            player.SetData("PLAYERISSPAWNED", spawned);
            player.Emit("setPlayerSpawned", spawned);
        }

        /// <summary>
        /// Returns if a player is spawned
        /// </summary>
        /// <param name="player"></param>
        /// <returns>True = spawned</returns>
        public static bool IsSpawned(this IPlayer player)
        {
            player.GetData("PLAYERISSPAWNED", out bool spawned);

            return spawned;
        }

        public static void PlayMusicFromUrl(this IPlayer player, string url)
        {
            player.Emit("PlayMusicUrl", url);
        }

        public static void StopMusic(this IPlayer player)
        {
            player.Emit("StopMusic");
        }

        /// <summary>
        /// Returns if a player has that job.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="job"></param>
        /// <returns>True if has job. False if doesn't</returns>
        public static bool HasJob(this IPlayer player, Models.Jobs job)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return false;

            return JsonConvert.DeserializeObject<List<Models.Jobs>>(playerCharacter.JobList).Contains(job);
        }

        /// <summary>
        /// Sets a Player Position with many params
        /// </summary>
        /// <param name="player"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="delayMs"></param>
        /// <param name="spawn"></param>
        /// <param name="destroyCameras"></param>
        /// <param name="switchOut"></param>
        /// <param name="unfreezeTime">Time after delayMs to unfreeze player</param>
        public static void SetPosition(this IPlayer player, Position position, Rotation rotation, double delayMs = 5000, bool spawn = false, bool destroyCameras = false, bool switchOut = false, double unfreezeTime = 0, bool loadWeapon = false)
        {
            try
            {
                Timer delayTimer = new Timer(delayMs);

                if (unfreezeTime != 0)
                {
                    player.FreezePlayer(true);
                    Timer unfreezeTimer = new Timer(delayMs + unfreezeTime) { AutoReset = false };
                    unfreezeTimer.Start();

                    unfreezeTimer.Elapsed += (sender, args) =>
                    {
                        unfreezeTimer.Stop();

                        player.FreezePlayer(false);
                    };
                }

                if (switchOut)
                {
                    player.Emit("switchOutPlayer", delayMs + 200);
                }

                delayTimer.Elapsed += (sender, args) =>
                {
                    try
                    {
                        delayTimer.Stop();
                        if (spawn)
                        {
                            player.Spawn(position);
                            player.Rotation = rotation;
                            if (destroyCameras)
                            {
                                CameraExtension.DeleteCamera(player);
                            }
                            player.Emit("StopScreenEvent");
                            return;
                        }
                        player.Position = position;
                        player.Rotation = rotation;
                        if (destroyCameras)
                        {
                            CameraExtension.DeleteCamera(player);
                        }

                        if (loadWeapon)
                        {
                            Timer weaponTimer = new Timer(4000) { AutoReset = false };

                            weaponTimer.Start();

                            weaponTimer.Elapsed += (o, eventArgs) =>
                            {
                                weaponTimer.Stop();
                                if (!String.IsNullOrEmpty(player.FetchCharacter().CurrentWeapon))
                                {
                                    InventoryItem currentWeaponItem =
                                        JsonConvert.DeserializeObject<InventoryItem>(player.FetchCharacter()
                                            .CurrentWeapon);

                                    if (currentWeaponItem != null)
                                    {
                                        WeaponInfo weaponInfo =
                                            JsonConvert.DeserializeObject<WeaponInfo>(currentWeaponItem.ItemValue);

                                        EquipItem.EquipWeapon(player, player.FetchInventory(), currentWeaponItem,
                                            weaponInfo.AmmoCount);
                                    }
                                }
                            };
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return;
                    }
                };

                delayTimer.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static void SetWaypoint(this IPlayer player, Position position)
        {
            player.Emit("setWaypoint", position);
        }

        public static void RemoveWaypoint(this IPlayer player)
        {
            player.Emit("clearWaypoint");
        }

        /// <summary>
        /// Fetches the Street Name and Area Name of the player, returning back returnEvent, streetName, areaName
        /// </summary>
        /// <param name="player"></param>
        /// <param name="returnEvent">The client callback</param>
        public static void FetchLocation(this IPlayer player, string returnEvent)
        {
            player.Emit("fetchPlayerStreetArea", returnEvent);
        }

        public static void SendNotification(this IPlayer player, string message, bool blink = false)
        {
            player.Emit("createNotification", message, blink);
        }

        /// <summary>
        /// Adds the selected amount of cash to a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="amount"></param>
        public static void AddCash(this IPlayer player, float amount)
        {
            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null) return;

            double rounded = Math.Round(amount, 2);

            playerCharacter.Money += (float)rounded;

            context.SaveChanges();
        }

        /// <summary>
        /// Adds the selected amount of cash to a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="amount"></param>
        public static void AddCash(this IPlayer player, double amount)
        {
            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null) return;

            double rounded = Math.Round(amount, 2);

            playerCharacter.Money += (float)rounded;

            context.SaveChanges();
        }

        /// <summary>
        /// Removes the selected amount of cash from a players hand.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="amount"></param>
        public static void RemoveCash(this IPlayer player, float amount)
        {
            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null) return;

            double rounded = Math.Round(amount, 2);

            playerCharacter.Money -= (float)rounded;

            context.SaveChanges();
        }

        /// <summary>
        /// Removes the selected amount of cash from a players hand.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="amount"></param>
        public static void RemoveCash(this IPlayer player, double amount)
        {
            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null) return;

            double rounded = Math.Round(amount, 2);

            playerCharacter.Money -= (float)rounded;

            context.SaveChanges();
        }

        public static PlayerEntity GetClass(this IPlayer player)
        {
            if (player.GetData("player-class", out PlayerEntity data))
            {
                return data;
            }

            PlayerEntity playerEntity = new PlayerEntity(player);
            player.SetData("player-class", playerEntity);

            return playerEntity;
        }

        /// <summary>
        /// Fetches if in Law Faction and optional is on duty
        /// </summary>
        /// <param name="player"></param>
        /// <param name="isOnDuty">Checks if player on duty</param>
        /// <returns></returns>
        public static bool IsLeo(this IPlayer player, bool isOnDuty = false)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return false;

            Faction activeFaction = Faction.FetchFaction(playerCharacter.ActiveFaction);

            if (activeFaction == null) return false;

            if (!isOnDuty)
            {
                if (activeFaction.SubFactionType == SubFactionTypes.Law) return true;
            }
            else
            {
                if (activeFaction.SubFactionType == SubFactionTypes.Law && playerCharacter.FactionDuty) return true;
            }

            return false;
        }

        public static bool IsInAnimation(this IPlayer player)
        {
            bool hasData = player.GetData("InAnimation", out bool inAnimation);

            if (hasData && inAnimation)
            {
                return true;
            }

            return false;
        }

        public static void SetInAnimation(this IPlayer player, bool status)
        {
            player.SetData("InAnimation", status);
        }

        public static void GetTrunkPosition(this IPlayer player, IVehicle vehicle, string returnEvent)
        {
            player.Emit("Vehicle:FetchTrunkPosition", vehicle, returnEvent);
        }

        public static void FetchForwardPosition(this IPlayer player, string returnEvent, double distance)
        {
            player.Emit("player:FetchForwardPosition", returnEvent, distance);
        }
    }

    public class PlayerEntity
    {
        private readonly IPlayer _player;

        public PlayerEntity(IPlayer newPlayer)
        {
            _player = newPlayer;
        }

        /// <summary>
        /// The players seat belt state
        /// </summary>
        public bool Seatbelt
        {
            get
            {
                _player.GetSyncedMetaData("PLAYERSEATBELT", out bool seatBeltState);
                return seatBeltState;
            }

            set => _player.SetSyncedMetaData("PLAYERSEATBELT", value);
        }

        /// <summary>
        /// The players spawned state
        /// </summary>
        public bool Spawned
        {
            get
            {
                _player.GetData("PLAYERISSPAWNED", out bool spawned);

                return spawned;
            }
            set
            {
                _player.SetData("PLAYERISSPAWNED", value);
                _player.Emit("setPlayerSpawned", value);
            }
        }

        /// <summary>
        /// Determines when a player is in the creator room
        /// </summary>
        public bool CreatorRoom
        {
            get
            {
                _player.GetSyncedMetaData("INCHARCREATORROOM", out bool inCreator);

                return inCreator;
            }
            set => _player.SetSyncedMetaData("INCHARCREATORROOM", value);
        }

        /// <summary>
        /// Returns if the player is Male (True = Male)
        /// </summary>
        public bool IsMale => _player.FetchCharacter().Sex == 0;

        /// <summary>
        /// Gets the players character name
        /// </summary>
        public string Name
        {
            get
            {
                if (_player.GetClass().AdminDuty)
                {
                    return _player.GetClass().UcpName;
                }

                if (_player.HasSyncedMetaData(HelperCommands.HelperDutyData))
                {
                    return _player.GetClass().UcpName;
                }

                bool hasMaskData = _player.GetData("IsWearingMask", out bool isWearingMask);

                if (hasMaskData && isWearingMask)
                {
                    bool hasNameData = _player.GetData("MaskName", out string maskName);

                    if (hasNameData)
                    {
                        return maskName;
                    }
                }

                Models.Character playerCharacter = _player.FetchCharacter();

                return playerCharacter?.Name;
            }
            set
            {
                _player.SetSyncedMetaData("playerNameTag", value);

                if (_player.GetClass().AdminDuty) return;

                if (value.ToLower().Contains("mask"))
                {
                    _player.SetData("IsWearingMask", true);
                    _player.SetData("MaskName", value);
                    return;
                }

                bool hasMaskData = _player.GetData("IsWearingMask", out bool isWearingMask);

                if (hasMaskData && isWearingMask)
                {
                    _player.SetData("IsWearingMask", false);
                }

                using Context context = new Context();
                Models.Character playerCharacter = context.Character.Find(_player.GetClass().CharacterId);

                if (playerCharacter == null) return;

                playerCharacter.Name = value;

                context.SaveChanges();
            }
        }

        /// <summary>
        /// The Player ID
        /// </summary>
        public int PlayerId
        {
            get
            {
                _player.GetData("PLAYERID", out int id);
                return id;
            }
            set => _player.SetData("PLAYERID", value);
        }

        /// <summary>
        /// The Character Id
        /// </summary>
        public int CharacterId
        {
            get
            {
                _player.GetData("USERCHARACTERID", out int id);
                return id;
            }
            set => _player.SetData("USERCHARACTERID", value);
        }

        /// <summary>
        /// Gets or sets if they have completed tutorial
        /// </summary>
        public bool CompletedTutorial
        {
            get
            {
                _player.GetData("COMPLETEDTUTORIAL", out bool tutorialResult);
                return tutorialResult;
            }
            set => _player.SetData("COMPLETEDTUTORIAL", value);
        }

        /// <summary>
        /// The players Account Id
        /// </summary>
        public int AccountId
        {
            get
            {
                bool hasData = _player.GetData("USERACCOUNTID", out int accountId);
                if (!hasData)
                {
                    return 0;
                }
                return accountId;
            }
            set => _player.SetData("USERACCOUNTID", value);
        }

        public bool Downed
        {
            get
            {
                bool hasData = _player.GetData("ISDOWNED", out bool isDowned);
                if (!hasData) return false;

                return isDowned;
            }
            set => _player.SetData("ISDOWNED", value);
        }

        public float Cash
        {
            get => _player.FetchCharacter().Money;

            set
            {
                Context context = new Context();

                Models.Character playerCharacter = context.Character.Find(CharacterId);

                if (playerCharacter == null) return;

                playerCharacter.Money = value;

                context.SaveChanges();
            }
        }

        public bool Fishing
        {
            get
            {
                _player.GetData("IsPlayerFishing", out bool isFishing);
                return isFishing;
            }
            set => _player.SetData("IsPlayerFishing", value);
        }

        public bool EditingCharacter
        {
            get
            {
                _player.GetData("IsEditingCharacter", out bool isEditing);
                return isEditing;
            }
            set
            {
                string lastTimeData = "AFK:LastMove";
                string lastPositionData = "AFK:LastPosition";
                _player.SetData(lastTimeData, DateTime.Now);
                _player.SetData(lastPositionData, _player.Position);
                _player.SetData("IsEditingCharacter", value);
            }
        }

        public int ClerkJob
        {
            get
            {
                _player.GetData("ClerkJobLocation", out int jobId);
                return jobId;
            }
            set => _player.SetData("ClerkJobLocation", value);
        }

        public int ClerkCount
        {
            get
            {
                bool hasCount = _player.GetData("ClerkJobCount", out int count);
                return !hasCount ? 0 : count;
            }
            set => _player.SetData("ClerkJobCount", value);
        }

        public bool HasClerkTask
        {
            get
            {
                _player.GetData("ClerkTask", out bool hasTask);
                return hasTask;
            }
            set => _player.SetData("ClerkTask", value);
        }

        /// <summary>
        /// Returns if player is using Ctrl + E sitting method
        /// </summary>
        public bool IsSitting
        {
            get
            {
                _player.GetData("IsSitting", out bool isSitting);
                return isSitting;
            }
            set => _player.SetData("IsSitting", value);
        }

        public Language.Language SpokenLanguage
        {
            get
            {
                bool hasData = _player.GetData("SpokenLanguage", out Language.Language spokenLanguage);
                if (hasData)
                {
                    return spokenLanguage;
                }

                string spokenString = _player.FetchCharacter().CurrentLanguage;

                spokenLanguage = !string.IsNullOrEmpty(spokenString) ? JsonConvert.DeserializeObject<Language.Language>(_player.FetchCharacter().CurrentLanguage) : LanguageHandler.Languages.FirstOrDefault(x => x.Code == "en");

                _player.SetData("SpokenLanguage", spokenLanguage);
                return spokenLanguage;
            }
            set => _player.SetData("SpokenLanguage", value);
        }

        public List<Language.Language> SpokenLanguages
        {
            get
            {
                try
                {
                    if (_player == null)
                    {
                        return null;
                    }
                    bool hasData = _player.GetData("SpokenLanguages", out List<Language.Language> spokenLanguages);
                    if (hasData && spokenLanguages != null)
                    {
                        return spokenLanguages;
                    }

                    if (string.IsNullOrEmpty(_player.FetchCharacter()?.Languages))
                    {
                        List<Language.Language> newLanguages = new List<Language.Language>
                        {
                            LanguageHandler.Languages.FirstOrDefault(x => x.Code == "en")
                        };

                        using Context context = new Context();

                        Models.Character character = context.Character.Find(_player.GetClass().CharacterId);

                        character.Languages = JsonConvert.SerializeObject(newLanguages);

                        context.SaveChanges();

                        _player.SetData("SpokenLanguages", newLanguages);

                        return newLanguages;
                    }

                    List<Language.Language> languages =
                        JsonConvert.DeserializeObject<List<Language.Language>>(_player.FetchCharacter().Languages);
                    _player.SetData("SpokenLanguages", languages);

                    return languages;
                }
                catch (Exception e)
                {
                    List<Language.Language> spokenLanguages = new List<Language.Language>
                    {
                        LanguageHandler.Languages.FirstOrDefault(x => x.Code == "en")
                    };

                    _player.SetData("SpokenLanguages", spokenLanguages);
                    Console.WriteLine(e);
                    return spokenLanguages;
                }
            }
            set => _player.SetData("SpokenLanguages", value);
        }

        public bool Cuffed
        {
            get
            {
                bool hasCuffedData = _player.GetData("IsCuffed", out bool cuffed);

                return hasCuffedData && cuffed;
            }
            set
            {
                _player.SetData("IsCuffed", value);

                _player.Emit("SetCuffState", value);
            }
        }

        public int WalkStyle
        {
            get
            {
                bool hasWalkStyle = _player.GetData("CurrentWalkStyle", out int walkStyle);
                if (hasWalkStyle) return walkStyle;

                walkStyle = _player.FetchCharacter().WalkStyle;

                _player.SetData("CurrentWalkStyle", walkStyle);

                return walkStyle;
            }
            set => _player.SetData("CurrentWalkStyle", value);
        }

        public bool AdminDuty
        {
            get
            {
                bool hasAdminDuty = _player.GetSyncedMetaData("AdminDuty", out bool adminDuty);

                if (!hasAdminDuty) return false;
                return adminDuty;
            }

            set => _player.SetSyncedMetaData("AdminDuty", value);
        }

        public string UcpName
        {
            get
            {
                bool hasUcpName = _player.GetSyncedMetaData("UcpName", out string ucpName);

                if (!hasUcpName) return null!;

                return ucpName;
            }
            set => _player.SetSyncedMetaData("UcpName", value);
        }
    }
}