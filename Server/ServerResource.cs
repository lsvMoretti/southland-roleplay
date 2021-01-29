using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Timers;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using AltV.Net.EntitySync.ServerEvent;
using AltV.Net.EntitySync.SpatialPartitions;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Serilog;
using Server.Account;
using Server.Admin;
using Server.Apartments;
using Server.Audio;
using Server.Backpack;
using Server.Bank;
using Server.Character;
using Server.Character.Clothing;
using Server.Character.Scenes;
using Server.Character.Tattoo;
using Server.Chat;
using Server.Connection;
using Server.Dealerships;
using Server.Developer;
using Server.Discord;
using Server.DMV;
using Server.Doors;
using Server.Drug;
using Server.Extensions;
using Server.Focuses;
using Server.Graffiti;
using Server.Groups;
using Server.Groups.EUP;
using Server.Groups.Police;
using Server.Groups.Police.MDT;
using Server.Inventory.OpenInventory;
using Server.Jobs.Bus;
using Server.Jobs.Clerk;
using Server.Jobs.Delivery;
using Server.Jobs.Fishing;
using Server.Jobs.FoodStand;
using Server.Jobs.Taxi;
using Server.Language;
using Server.Map;
using Server.Motel;
using Server.Objects;
using Server.Phone;
using Server.Property;
using Server.Vehicle;
using Server.Vehicle.ModShop;
using Server.Weapons;

namespace Server
{
    public class ServerResource : AsyncResource
    {
        public override async void OnStart()
        {
            try
            {
                Console.SetOut(new Writer());

                AltEntitySync.Init(5, 200, (threadId) => true,
                    (threadCount, repository) => new ServerEventNetworkLayer(threadCount, repository),
                    (entity, threadCount) => entity.Type,
                    (entityId, entityType, threadCount) => entityType,
                    (threadId) =>
                    {
                        return threadId switch
                        {
                            //MARKER
                            0 => new LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 64),
                            //TEXT
                            1 => new LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 32),
                            //PROP
                            2 => new LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 1500),
                            //HELP TEXT
                            3 => new LimitedGrid3(50_000, 50_000, 100, 10_000, 10_000, 1),
                            //BLIP
                            4 => new EntityStreamer.GlobalEntity(),
                            //BLIP DYNAMIQUE
                            5 => new LimitedGrid3(50_000, 50_000, 175, 10_000, 10_000, 200),
                            _ => new LimitedGrid3(50_000, 50_000, 175, 10_000, 10_000, 115),
                        };
                    },
                    new IdProvider());

                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

                Console.WriteLine("Starting Southland Roleplay");

                Alt.OnPlayerConnect += ConnectionHandler.AltOnOnPlayerConnect;
                Alt.OnPlayerDisconnect += ConnectionHandler.OnPlayerDisconnect;
                Alt.OnPlayerEnterVehicle += VehicleHandler.AltOnOnPlayerEnterVehicle;
                Alt.OnPlayerLeaveVehicle += VehicleHandler.AltOnOnPlayerLeaveVehicle;
                Alt.OnPlayerDead += DeathHandler.OnPlayerDeath;
                Alt.OnWeaponDamage += DamageHandler.AltOnOnWeaponDamage;
                Alt.OnPlayerDamage += DamageHandler.AltOnOnPlayerDamage;
                Alt.OnPlayerWeaponChange += WeaponSwitch.AltOnOnPlayerWeaponChange;

                CommandExtension.Init();
                GameWorld.InitGameWorld();

                SignalR.StartConnection();

                await ServerLoaded();
                Console.WriteLine($"Server Loaded");

                #region Dev Callbacks

                Alt.OnClient<IPlayer, float>("dev:returnGameplayCamRot", (player, rot) =>
                {
                    player.SendInfoNotification($"Cam Rot: {rot}");
                });

                #endregion Dev Callbacks

                Alt.OnClient<IPlayer>("LoginScreenLoaded", ConnectionHandler.OnLoginViewLoaded);

                Alt.OnClient<IPlayer, string, string>("recieveLoginRequest", ConnectionHandler.ReceiveLoginRequest);

                ChatExtension chatExtension = new ChatExtension();

                Alt.OnClient<IPlayer, string>("sendChatMessage", chatExtension.OnChatEvent);

                Alt.OnClient<IPlayer, string, string, int>("NativeMenuCallback", MenuExtension.NativeMenuItemSelected);

                Alt.OnClient<IPlayer, string, string, string>("NativeMenuListChange", MenuExtension.NativeMenuListChange);

                Alt.OnClient<IPlayer, string, int, string>("NativeMenuIndexChange", MenuExtension.NativeMenuIndexChange);

                Alt.OnClient<IPlayer, string>("KeyUpEvent", KeyExtension.OnKeyUpEvent);

                Alt.OnClient<IPlayer, string, string>("contextMenuPressed", ContextMenuExtension.OnContextMenuClickEvent);

                #region MDT Call Backs

                Alt.OnClient<IPlayer>("respondLast911", MdtHandler.PlayerRespond911);

                Alt.OnClient<IPlayer>("requestBackup", MdtHandler.PlayerRequestBackup);

                Alt.OnClient<IPlayer>("fetch911Calls", MdtHandler.Fetch911Calls);

                Alt.OnClient<IPlayer>("mdcWindowClose", MdtHandler.CloseMdt);

                Alt.OnClient<IPlayer, string>("mdcPersonSearch", MdtHandler.FetchPersonSearch);

                Alt.OnClient<IPlayer, string>("MdcSearchResultProfileSelected", MdtHandler.MdcSearchResultProfileSelected);

                Alt.OnClient<IPlayer, string>("mdc:personSearchPropertySelected", MdtHandler.OnProfileSelectedProperty);

                Alt.OnClient<IPlayer, string>("mdc:setPatrolUnit", MdtHandler.OnSetPatrolUnit);

                Alt.OnClient<IPlayer, string>("mdc:vehicleSearch", MdtHandler.OnMdcPlateSearch);

                Alt.OnClient<IPlayer, int>("Responding911", MdtHandler.OnRespond911);

                #endregion MDT Call Backs

                Alt.OnClient<IPlayer>("SendRequestOnlinePlayers", (player) =>
                {
                    player.Emit("OnlinePlayerList", JsonConvert.SerializeObject(OnlinePlayer.FetchOnlinePlayers()));
                });

                Alt.OnClient<IPlayer, string>("PlayerSelectedMusicStream", AudioHandler.StreamPageStreamSelected);

                Alt.OnClient<IPlayer>("StopSelectedMusicStream", AudioHandler.StreamPageStopStream);

                #region Character Creator

                Alt.OnClient<IPlayer, int>("creator:onGenderChange", CharacterCreator.OnGenderChange);

                Alt.OnClient<IPlayer, string>("creator:CharacterCreatorFinished", CharacterCreator.OnCreationFinished);

                #endregion Character Creator

                #region Bank System

                Alt.OnClient<IPlayer, string, string, string>("BankTransaction", BankHandler.HandleBankTransaction);

                Alt.OnClient<IPlayer>("bankViewClosed", BankHandler.OnBankViewClose);

                Alt.OnClient<IPlayer, string, string, string>("BankTransfer", BankHandler.HandleBankTransfer);

                Alt.OnClient<IPlayer, string>("RequestNewBankCardPin", BankHandler.RequestNewBankCard);

                Alt.OnClient<IPlayer, string>("RequestBankAccountClosure", BankHandler.RequestAccountClosure);

                Alt.OnClient<IPlayer, string>("RequestNewBankAccount", BankHandler.RequestNewBankAccount);

                #endregion Bank System

                #region Atm System

                Alt.OnClient<IPlayer>("AtmPageLoaded", AtmHandler.AtmPageLoaded);

                Alt.OnClient<IPlayer>("atm:pageClosed", AtmHandler.AtmPageClosed);

                Alt.OnClient<IPlayer, string, string>("AtmWithdrawFunds", AtmHandler.AtmWithdrawFunds);

                Alt.OnClient<IPlayer, string>("SetBankAccountActive", BankHandler.SetBankAccountAsActive);

                Alt.OnClient<IPlayer, string>("ATMSystem:IncorrectPin", AtmHandler.OnAtmPinIncorrect);

                #endregion Atm System

                #region Hud System

                Alt.OnClient<IPlayer>("hud:FetchMoney", (player) =>
                {
                    if (!player.IsSpawned()) return;

                    bool hasMoneyValue = player.GetData("MoneyValue", out float money);
                    HudInfo newHudInfo = null;
                    if (hasMoneyValue)
                    {
                        newHudInfo = new HudInfo(player.Health, money, Settings.ServerSettings.Hour, Settings.ServerSettings.Minute);
                    }
                    else
                    {
                        newHudInfo = new HudInfo(player.Health, 0, Settings.ServerSettings.Hour, Settings.ServerSettings.Minute);
                    }

                    player.Emit("hud:RecieveMoneyUpdate", JsonConvert.SerializeObject(newHudInfo));
                });

                #endregion Hud System

                #region Dealership System

                Alt.OnClient<IPlayer>("dealership:pageclosed", DealershipHandler.OnWebViewClose);

                Alt.OnClient<IPlayer, int>("dealership:selectedVehiclePreview", DealershipHandler.OnVehiclePreviewSelect);

                Alt.OnClient<IPlayer, int, float>("dealership:returnCamRot", AdminCommands.FetchDealershipCamRotation);

                Alt.OnClient<IPlayer, string>("dealership:rotationChange", DealershipHandler.OnPreviewVehicleRotationChange);

                Alt.OnClient<IPlayer, string>("dealership:previewVehicleColorSelect", DealershipHandler.OnPreviewVehicleColorSelect);

                Alt.OnClient<IPlayer>("dealership:purchaseVehicle", DealershipHandler.OnPreviewSelectPurchase);

                Alt.OnClient<IPlayer>("dealership:voucherVehiclePurchase", (player) =>
                {
                    DealershipHandler.OnPaymentSelection(player, "voucher");
                });

                Alt.OnClient<IPlayer, string>("dealership:purchaseVehiclePaymentSelection", DealershipHandler.OnPaymentSelection);

                Alt.OnClient<IPlayer, string, string, string>("admin:dealership:createNewDealershipVehicle", AdminCommands.CreateNewDealershipVehicle);

                Alt.OnClient<IPlayer>("admin:dealership:callClosePage", AdminCommands.EventCloseDealershipEditPage);

                Alt.OnClient<IPlayer, string, string, string, string>("admin:dealership:editDealershipVehicle", AdminCommands.EditDealershipVehicle);

                Alt.OnClient<IPlayer, string>("admin:dealership:removeDealershipVehicle", AdminCommands.RemoveDealershipVehicle);

                #endregion Dealership System

                #region Phone System

                Alt.OnClient<IPlayer, string>("phone:handleCallNumber", PhoneCommands.HandleCallNumber);

                Alt.OnClient<IPlayer, string, string>("phone:smsNewNumber", (player, arg0, arg1) =>
                {
                    PhoneCommands.SmsNumber(player, string.Join(" ", arg0, arg1));
                });

                Alt.OnClient<IPlayer, string>("phone:smsExistingContact", PhoneCommands.HandleSmsContact);

                Alt.OnClient<IPlayer, string, string>("phone:handleAddContact", PhoneCommands.AddContact);

                Alt.OnClient<IPlayer, string, string>("911:startCall", Handler911.On911LocationReturn);

                Alt.OnClient<IPlayer, string, string>("311:startCall", Handler911.On311LocationReturn);

                #endregion Phone System

                #region DMV System

                Alt.OnClient<IPlayer, int>("dmv:finishedDriving", DmvHandler.OnDrivingTestFinished);

                Alt.OnClient<IPlayer, int>("dmv:speeding", DmvHandler.OnSpeeding);

                Alt.OnClient<IPlayer>("dmv:wrongVehicle", DmvHandler.OnIncorrectVehicle);

                #endregion DMV System

                #region Taxi System

                Alt.OnClient<IPlayer, string, string>("taxi:startCall", CallHandler.OnLocationReturn);

                #endregion Taxi System

                #region Faction Management

                Alt.OnClient<IPlayer>("factionViewClosed", AdminCommands.OnFactionViewClose);

                Alt.OnClient<IPlayer, string, string, string>("admin:faction:adjustRankPerm", AdminCommands.AdjustFactionRankPerm);

                Alt.OnClient<IPlayer, string>("admin:faction:fetchMembers", AdminCommands.FetchFactionMembers);

                Alt.OnClient<IPlayer, string>("admin:faction:removeMemberFromFaction", AdminCommands.RemoveFactionMember);

                Alt.OnClient<IPlayer, string, string, string>("admin:faction:adjustMemberRank", AdminCommands.AdjustFactionMemberRank);

                Alt.OnClient<IPlayer, int>("admin:faction:removeFaction", AdminCommands.OnFactionRemove);

                Alt.OnClient<IPlayer, string, string, string>("admin:faction:createFaction", AdminCommands.OnFactionCreateSubmit);

                Alt.OnClient<IPlayer, string>("faction:leader:showFactionRanks", LeaderCommands.OnRanksAddRank);

                #endregion Faction Management

                #region Bus Routes

                Alt.OnClient<IPlayer>("bus:finishedRoute", BusCommands.OnBusRouteFinish);
                Alt.OnClient<IPlayer, int, int>("bus:EnteredMarker", BusHandler.OnEnterMarker);

                #endregion Bus Routes

                #region Vehicle Mod System

                Alt.OnClient<IPlayer, string>("vehicleMod:ReturnVehicleMods", ModHandler.OnVehicleNumModsReturn);

                Alt.OnClient<IPlayer, string, int>("vehicleMod:returnModNames", ModHandler.OnReturnModNames);

                Alt.OnClient<IPlayer, string>("vehicleMod:returnVehicleWheelNames", ModHandler.OnWheelNamesFetched);

                #endregion Vehicle Mod System

                #region Property System

                Alt.OnClient<IPlayer, string, string>("admin:property:PropertyAddress", AdminCommands.OnPropertyFetchLocation);

                Alt.OnClient<IPlayer, string>("property:PurchasePropertyHandler", PropertyCommands.OnPurchasePropertySelectPayment);

                #endregion Property System

                #region Fuel System

                Alt.OnClient<IPlayer, bool>("returnIsByFuelPump", Vehicle.Commands.IsPlayerNearFuelPump);

                #endregion Fuel System

                Alt.OnClient<IPlayer, int>("weapon:unequip:ReturnAmmo", WeaponCommands.UnEquipWeapon);

                #region Help System

                Alt.OnClient<IPlayer, string>("HelpMenu:FetchCommands", HelpHandler.FetchHelpCommands);

                Alt.OnClient<IPlayer>("helpMenu:CloseHelpMenu", HelpHandler.OnHelpMenuClose);

                #endregion Help System

                #region Apartment System

                Alt.OnClient<IPlayer, string>("apartment:OnPaymentMethodReturn", ApartmentCommand.OnPaymentSelection);

                #endregion Apartment System

                Alt.OnClient<IPlayer, int>("weapon:death:fetchAmmo", DeathHandler.OnDeathReturnAmmo);

                #region Vehicle System

                Alt.OnClient<IPlayer, int>("ReturnVehicleClass", VehicleHandler.OnVehicleClassReturn);
                Alt.OnClient<IPlayer, float>("trunkPos:VInventory", VehicleInventory.OnReturnTrunkPosition);
                Alt.OnClient<IPlayer, int, bool>("IndicatorStateChange", VehicleHandler.OnIndicatorStateChange);

                #endregion Vehicle System

                #region Hotwire System

                Alt.OnClient<IPlayer>("VehicleScramble:MaxAttemptsReached", HotWire.OnMaxAttemptsReached);

                Alt.OnClient<IPlayer>("VehicleScramble:TimeExpired", HotWire.OnTimeExpired);

                Alt.OnClient<IPlayer>("VehicleScramble:CorrectWord", HotWire.OnCorrectWord);

                Alt.OnClient<IPlayer>("VehicleScramble:PageClosed", HotWire.OnPageClosed);

                #endregion Hotwire System

                Alt.OnClient<IPlayer, bool>("ChatStatusChange", (player, status) =>
                {
                    player.SetSyncedMetaData("TypeStatus", status);
                });

                #region Spectate System

                Alt.OnClient<IPlayer>("admin:OnSpectateFinish", AdminCommands.OnSpectateFinish);
                Alt.OnClient<IPlayer>("admin:spectate:setPlayerPos", AdminCommands.OnSpectateSetPosition);
                Alt.OnClient<IPlayer>("admin:spectate:UpdatePosition", AdminCommands.OnSpectateSetPosition);

                #endregion Spectate System

                #region Graffiit

                Alt.OnClient<IPlayer, float, float, float>("graffiti:FetchForwardPosition", GraffitiCommands.ReturnGraffitiPosition);

                #endregion Graffiit

                #region Crouching

                Alt.OnClient<IPlayer>("Player:CancelCrouch", (player) =>
                {
                    if (!player.IsSpawned()) return;

                    player.Emit("SetWalkStyle", player.GetClass().WalkStyle);
                });

                #endregion Crouching

                #region Cycle Storage

                Alt.OnClient<IPlayer, float>("vehicle:cycle:storeReturnPosition", Vehicle.Commands.OnCycleStoreReturnTrunkPosition);

                Alt.OnClient<IPlayer, float>("vehicle:cycle:getReturnPosition", Vehicle.Commands.OnGetReturnTrunkPosition);

                #endregion Cycle Storage

                #region Weapon System

                Alt.OnClient<IPlayer, int>("weapon:combineAmmo:ReturnCurrentAmmo", WeaponCommands.ReturnCurrentAmmo);
                Alt.OnClient<IPlayer, int>("CurrentWeaponAmmo", (player, ammo) =>
                {
                    player.SetData("weaponEvent:CurrentAmmo", ammo);
                });
                Alt.OnClient<IPlayer>("WeaponChange:TabReleased", WeaponSwitch.OnWeaponKeyBindReleased);

                #endregion Weapon System

                #region Clerk Job

                Alt.OnClient<IPlayer>("Clerk:FiveMinute", ClerkHandler.OnFiveMinuteInterval);
                Alt.OnClient<IPlayer>("Clerk:ReachedPosition", ClerkHandler.AtJobPosition);

                #endregion Clerk Job

                #region Sitting

                Alt.OnClient<IPlayer, float, float, float, string>("sitting:IsPlayerPositionFree", SittingHandler.IsPlayerPositionFree);

                Alt.OnClient<IPlayer, float, float, float, string>("sitting:ClearSeat", SittingHandler.ClearSeat);

                Alt.OnClient<IPlayer, Vector3, float>("sitting:SetPlayerPosition", SittingHandler.SetPlayerPosition);

                #endregion Sitting

                #region ELS Sync

                Alt.OnClient<IPlayer, IVehicle, int>("SetSirenStateForVehicle", (player, vehicle, state) =>
                {
                    Alt.EmitAllClients("OnSetSirenSoundState", vehicle, state);
                });

                Alt.OnClient<IPlayer, IVehicle, int>("SetSirenLightState", (player, vehicle, state) =>
                {
                    Alt.EmitAllClients("OnSetSirenLightState", vehicle, state);
                });

                Alt.OnClient<IPlayer, IVehicle, bool>("VehicleHornState", (player, vehicle, state) =>
                {
                    Alt.EmitAllClients("OnVehicleHornState", vehicle, state);
                });

                Alt.OnClient<IPlayer, IVehicle>("BlipVehicleSiren", (player, vehicle) =>
                {
                    Alt.EmitAllClients("OnBlipSiren", vehicle);
                });

                Alt.OnClient<IPlayer, IVehicle, int, bool>("VehicleHandler:ToggleExtra", VehicleHandler.ToggleVehicleExtra);

                Alt.OnClient<IPlayer, IVehicle, bool>("VehicleHandler:SetBombBayDoorState", VehicleHandler.SetBombBayDoorState);

                #endregion ELS Sync

                #region Door System

                Alt.OnClient<IPlayer, string, float, float, float>("sendClosestDoor", DoorHandler.OnReturnClosestDoor);

                #endregion Door System

                Alt.OnClient<IPlayer>("SendPlayerDimension", CharacterHandler.FetchPlayerDimension);

                Alt.OnClient<IPlayer, int>("admin:duty:fetchAmmo", AdminCommands.OnDutyReturnAmmo);

                #region Discord Intergration

                Alt.OnClient<IPlayer, string>("LinKDiscord:SendUserId", DiscordCommands.OnReturnDiscordLinkUserId);

                #endregion Discord Intergration

                #region Two Factor

                Alt.OnClient<IPlayer>("TFA:Complete", TwoFactorHandler.OnTwoFactorSetupComplete);
                Alt.OnClient<IPlayer>("TFA:ClosePage", TwoFactorHandler.OnTwoFactorSetupClose);
                Alt.OnClient<IPlayer, string>("TFA:SendCodeInput", TwoFactorHandler.OnRequestTwoFactor);

                #endregion Two Factor

                #region Food Stand Job

                Alt.OnClient<IPlayer, string, string>("FoodStand:FetchLocationData", FoodStandCommands.OnLocationDataReturn);

                #endregion Food Stand Job

                Alt.OnClient<IPlayer, Vector3, Vector3>("InteriorMapping:AddObject", PurchaseObjectHandler.OnInteriorMappingSaveObject);
                Alt.OnClient<IPlayer, Vector3, Vector3>("InteriorMapping:MoveObject", PurchaseObjectHandler.OnFinishMovingObject);

                #region Siren Handler

                Alt.OnClient<IPlayer, IVehicle>("newSirenHandler:HornPress", SirenHandler.OnHornPress);
                Alt.OnClient<IPlayer, IVehicle>("newSirenHandler:HornRelease", SirenHandler.OnHornRelease);

                #endregion Siren Handler
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

            Alt.Log(e.ToString());
            return;
        }

        public override void OnStop()
        {
            SaveEverything();
            Console.WriteLine("Stopped Southland Roleplay");
        }

        private static void SaveEverything()
        {
            using Context context = new Context();

            int playerSaveCount = 0;

            foreach (IPlayer player in Alt.Server.GetPlayers())
            {
                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                if (playerCharacter == null) continue;

                playerCharacter.PosX = player.Position.X;
                playerCharacter.PosY = player.Position.Y;
                playerCharacter.PosZ = player.Position.Z;

                playerCharacter.Dimension = player.Dimension;

                context.SaveChanges();

                player.Kick("Server Shutting Down");

                playerSaveCount++;
            }

            int vehicleCount = 0;

            foreach (IVehicle vehicle in Alt.Server.GetVehicles())
            {
                Models.Vehicle vehicleData = context.Vehicle.Find(vehicle.GetClass().Id);

                if (vehicleData == null) continue;

                if (vehicleData.FactionId > 0) continue;

                DegreeRotation rotation = vehicle.Rotation;

                vehicleData.PosX = vehicle.Position.X;
                vehicleData.PosY = vehicle.Position.Y;
                vehicleData.PosZ = vehicle.Position.Z;
                vehicleData.RotZ = rotation.Yaw;

                //vehicleData.Odometer = vehicle.GetClass().Distance;
                vehicleData.FuelLevel = vehicle.GetClass().FuelLevel;

                context.SaveChanges();

                vehicleCount++;
            }

            Console.WriteLine($"Saved {playerSaveCount} players & {vehicleCount} vehicles.");
        }

        public async Task ServerLoaded()
        {
            Developer.OnStart.OnStartEvent();

            AccountHandler.InitAccountSystem();

            LoadVehicle.ResetAllVehiclesSpawnStatus();

            //await MapHandler.LoadMaps();

            LoadProperties.LoadAllProperties();

            Clothes.LoadClothingItems();

            Settings.InitServerSettings();

            TimeWeather.InitTimeWeather();

            AudioHandler.LoadRadioStations();

            BankHandler.InitializeBanks();

            MinuteUpdate.StartMinuteTimer();

            VehicleHandler.StartFiveSecondTimer();

            DealershipHandler.LoadDealerships();

            RealEstateOffice.LoadRealEstateOffice();

            DmvHandler.InitDmv();

            Logging.InitLogging();

            JobHandler.InitTaxiJobLocation();

            BusHandler.InitBusJob();

            FocusHandler.InitFocuses();

            TutorialHandler.InitTutorial();

            TattooHandler.InitTattoos();

            WeaponSwitch.InitTick();

            UseDrug.InitTick();

            DeliveryHandler.LoadDeliveryPoints();

            WarehouseHandler.LoadWarehouses();

            GraffitiHandler.LoadGraffitis();

            ApartmentHandler.LoadApartments();

            Advertisements.LoadAdvertisementBuilding();

            FishingHandler.InitFishing();

            VehicleRental.InitVehicleRentalSystem();

            MotelHandler.InitMotels();

            VehicleRental.RefundVehicleRental();

            ClerkHandler.InitClerkJob();

            LanguageHandler.InitLanguages();

            BackpackHandler.LoadDroppedBackpacks();

            ActiveBusiness.InitActiveBusinessSystem();

            SceneHandler.LoadAllScenes();

            OpenInventoryHandler.CheckOpenInventoryLocations();

            FoodStandHandler.FetchFoodStandPositions();
            FoodStandCommands.StartFoodStandTimer();

            GrowingHandler.InitDrugGrowing();

            PurchaseObjectHandler.LoadBuyableObjects();

            WelcomePlayer.InitPed();

            WordListHandler.LoadWords();

            DoorHandler.SetPoliceDoorsLocked();

            await LoadVehicle.LoadFactionVehicles();

            await LoadVehicle.LoadCharacterVehicles();
        }
    }
}