using System;
using System.Collections.Generic;
using System.Net.Http;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Admin;
using Server.Apartments;
using Server.Backpack;
using Server.Character;
using Server.Character.Clothing;
using Server.Character.Tattoo;
using Server.Discord;
using Server.DMV;
using Server.Focuses;
using Server.Graffiti;
using Server.Groups;
using Server.Groups.EUP;
using Server.Inventory;
using Server.Inventory.OpenInventory;
using Server.Jobs.Bus;
using Server.Jobs.Delivery;
using Server.Jobs.Taxi;
using Server.Language;
using Server.Models;
using Server.Objects;
using Server.Phone;
using Server.Property;
using Server.Property.Stores;
using Server.Vehicle;
using Server.Vehicle.ModShop;
using Server.Weapons;

namespace Server.Extensions
{
    public class MenuExtension
    {
        /// <summary>
        /// Fired when a player selects a menu item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="serverTrigger"></param>
        /// <param name="selectedItem"></param>
        /// <param name="index"></param>
        public static void NativeMenuItemSelected(IPlayer player, string serverTrigger, string selectedItem, int index)
        {
            try
            {
                player.ChatInput(true);
                player.HideChat(false);

                if (serverTrigger == "closeNativeMenu")
                {
                    if (player.IsInVehicle && player.Seat == 1)
                    {
                        LoadVehicle.LoadVehicleMods(player.Vehicle);
                    }

                    if (!player.IsInVehicle)
                    {
                        CharacterHandler.LoadCustomCharacter(player, false, true);
                        player.Position = player.Position;
                    }

                    player.Emit("CloseNativeMenu");
                    player.ChatInput(true);
                    player.HideChat(false);
                    return;
                }

                #region Outfit System

                if (serverTrigger == "OutfitSystem:OutfitMainMenu") OutfitCommands.OnOutfitMainMenuSelect(player, selectedItem);
                if (serverTrigger == "OutfitSystem:SubItemSelect") OutfitCommands.OnOutfitSubMenuSelect(player, selectedItem);

                #endregion Outfit System

                #region Admin Events

                if (serverTrigger == "AdminTeleportSelect")
                {
                    AdminCommands.TeleportSelected(player, selectedItem);
                    return;
                }

                if (serverTrigger == "AdminShowPlayerVehicles")
                {
                    AdminCommands.AdminPlayerVehicleSelected(player, selectedItem, index);
                    return;
                }

                if (serverTrigger == "admin:property:CreatePropertyTypeSelect") AdminCommands.OnCreatePropertyTypeSelect(player, selectedItem);

                if (serverTrigger == "admin:property:SelectPropertyInterior") AdminCommands.OnSelectPropertyInterior(player, selectedItem);

                if (serverTrigger == "admin:property:removeItem") AdminCommands.OnPropertyRemoveItemSelect(player, selectedItem, index);

                if (serverTrigger == "AdminSendPlayerTeleportSelect") AdminCommands.OnSendToTeleportSelect(player, selectedItem);

                if (serverTrigger == "AdminCommand:OnManagePlayer")
                    AdminCommands.OnManagePlayerSelect(player, selectedItem);

                if (serverTrigger == "AdminCommand:OnManagePlayer:Vehicles")
                    AdminCommands.OnManagingPlayerSelectVehicle(player, selectedItem);

                if (serverTrigger == "AdminCommand:OnManagePlayer:Vehicle:Selected")
                    AdminCommands.OnManagingPlayerSelectVehicleSelected(player, selectedItem);

                if (serverTrigger == "AdminCommand:OnManagePlayer:Properties") AdminCommands.OnManagingPlayerSelectProperty(player, selectedItem);

                if (serverTrigger == "AdminCommand:OnManagePlayer:Properties:Selected") AdminCommands.OnManagePlayerPropertiesSelected(player, selectedItem);

                if (serverTrigger == "AdminCommand:CreateApartment:InteriorSelect") AdminCommands.OnCreateApartmentInteriorSelect(player, selectedItem, index);

                if (serverTrigger == "AdminCommand:GotoInterior") AdminCommands.OnGotoInteriorSelect(player, selectedItem, index);

                if (serverTrigger == "Admin:SetPlayerFocuses") AdminCommands.OnSelectFocus(player, selectedItem);

                #endregion Admin Events

                #region Inventory Events

                if (serverTrigger == "InventoryMenuSelect")
                {
                    InventoryCommands.OnInventoryMenuSelect(player, selectedItem);
                    return;
                }

                if (serverTrigger == "InventoryMenuSubSelect")
                {
                    InventoryCommands.OnInventoryMenuSubSelect(player, selectedItem);
                    return;
                }

                if (serverTrigger == "InventoryGiveItemToPlayer")
                {
                    InventoryCommands.OnInventoryItemGiveItemToPlayer(player, selectedItem);
                    return;
                }

                if (serverTrigger == "InventoryMenuGiveItemToPlayerQuantity")
                {
                    InventoryCommands.OnGiveItemToPlayerQuantitySelect(player, selectedItem);
                    return;
                }

                if (serverTrigger == "inventory:DropItem:SelectQuantity")
                    InventoryCommands.OnInventoryDropItemQuantitySelect(player, selectedItem);

                if (serverTrigger == "inventory:DeleteItem:SelectQuantity") InventoryCommands.OnInventoryDeleteItemQuantitySelect(player, selectedItem);

                if (serverTrigger == "Inventory:Backpack:QuantitySelect") InventoryCommands.OnBackPackQuantitySelect(player, selectedItem);

                #endregion Inventory Events

                #region Clothing Store Events

                if (serverTrigger == "ClothingStoreMainMenu")
                {
                    ClothingStore.EventClothingStoreMainMenu(player, selectedItem);
                    return;
                }

                if (serverTrigger == "ClothingStoreShowClothingTypesMenu")
                {
                    ClothingStore.EventClothingStoreShowClothingTypesMenu(player, selectedItem);
                    return;
                }

                if (serverTrigger == "ClothingListSelect")
                {
                    ClothingStore.EventClothingListSelect(player, selectedItem, index);
                    return;
                }

                if (serverTrigger == "ClothingListSubSelect")
                {
                    ClothingStore.EventClothingListSubSelect(player, selectedItem, index);
                    return;
                }

                if (serverTrigger == "PurchaseClothingItemMenu")
                {
                    ClothingStore.EventPurchaseClothingItemMenu(player, selectedItem);
                    return;
                }

                if (serverTrigger == "ClothingStoreShowAccessoryTypesMenu")
                {
                    ClothingStore.EventClothingStoreShowAccessoryTypesMenu(player, selectedItem);
                    return;
                }

                if (serverTrigger == "AccessoryListSelect")
                {
                    ClothingStore.EventAccessoryListSelect(player, selectedItem, index);
                    return;
                }

                if (serverTrigger == "AccessoryListSubSelect")
                {
                    ClothingStore.EventAccessoryListSubSelect(player, selectedItem, index);
                    return;
                }

                if (serverTrigger == "PurchaseAccessoryMenu")
                {
                    ClothingStore.EventPurchaseAccessoryMenu(player, selectedItem);
                    return;
                }

                #endregion Clothing Store Events

                #region Store Events

                if (serverTrigger == "PropertyBuyItem")
                {
                    PropertyCommands.Remote_PropertyBuyItem(player, selectedItem);
                    return;
                }

                #endregion Store Events

                #region Clothing Events

                if (serverTrigger == "ClothesMainMenuSelect")
                {
                    ClothingCommand.OnClothingMainMenuSelect(player, selectedItem);
                    return;
                }

                if (serverTrigger == "ClothesInteraction")
                {
                    ClothingCommand.EventOnClothesInteractionSelected(player, selectedItem);
                    return;
                }

                if (serverTrigger == "SwitchClothingItemSelected")
                {
                    ClothingCommand.SelectedSwitchClothingItem(player, selectedItem);
                    return;
                }

                if (serverTrigger == "AccessoriesMainMenuSelect")
                {
                    ClothingCommand.EventAccessoriesMainMenuSelect(player, selectedItem);
                    return;
                }

                if (serverTrigger == "AccessoryInteraction")
                {
                    ClothingCommand.EventAccessoryInteraction(player, selectedItem);
                    return;
                }

                if (serverTrigger == "SwitchAccessoryItemSelected")
                {
                    ClothingCommand.EventSwitchAccessoryItemSelected(player, selectedItem);
                    return;
                }

                #endregion Clothing Events

                #region Phone System

                if (serverTrigger == "PhoneSystemSelectPhone")
                {
                    PhoneCommands.EventPhoneSelected(player, selectedItem);
                    return;
                }

                if (serverTrigger == "PhoneMenuSelectMainMenuItem")
                {
                    PhoneCommands.EventPhoneMainMenuSelected(player, selectedItem);
                }

                if (serverTrigger == "PhoneSystemShowContactsMainMenu")
                {
                    PhoneCommands.EventPhoneSystemContactSelected(player, selectedItem);
                }

                if (serverTrigger == "PhoneSystemContactsSubMenu")
                {
                    PhoneCommands.EventPhoneSystemContactSubMenu(player, selectedItem);
                }

                #endregion Phone System

                if (serverTrigger == "RealEstateSelectProperty") RealEstateOffice.EventRealEstateSelectProperty(player, selectedItem);

                #region DMV

                if (serverTrigger == "DMVMainMenuSelect")
                {
                    DmvCommands.OnDmvMainMenuSelect(player, selectedItem);
                }

                #endregion DMV

                #region Vehicle Commands

                if (serverTrigger == "vehicle:windowControl")
                {
                    Vehicle.Commands.OnWindowControlSelect(player, selectedItem);
                }

                if (serverTrigger == "vehicle:doorControl")
                {
                    Vehicle.Commands.OnVehicleDoorControl(player, selectedItem);
                }

                if (serverTrigger == "taxi:showTaxiCallList")
                {
                    TaxiCommands.OnTaxiCallListSelect(player, selectedItem);
                }

                if (serverTrigger == "vehicle:vget:mainmenu") Vehicle.Commands.OnVGetSelect(player, selectedItem, index);

                if (serverTrigger == "vehicle:vget:garageMenu")
                    Vehicle.Commands.OnGarageVGetSelect(player, selectedItem, index);

                if (serverTrigger == "vehicle:ConfirmScrap") Vehicle.Commands.OnConfirmVehicleScrap(player, selectedItem);

                if (serverTrigger == "VehicleCommand:PlaceStolePlate") Vehicle.Commands.OnPlaceStolenPlateSelect(player, selectedItem);

                #endregion Vehicle Commands

                #region Admin Faction Invite

                if (serverTrigger == "admin:faction:ainvite:showFactions")
                {
                    AdminCommands.AdminInviteFactionSelected(player, selectedItem);
                }

                if (serverTrigger == "admin:faction:ainvite:showRanks")
                {
                    AdminCommands.AdminInviteRankSelected(player, selectedItem);
                }

                #endregion Admin Faction Invite

                #region Faction Commands

                if (serverTrigger == "faction:leadership:giverank")
                {
                    LeaderCommands.OnGiveRankMenuCalled(player, selectedItem);
                    return;
                }

                if (serverTrigger == "faction:player:faction")
                {
                    FactionCommands.FactionCommandOnFactionSelect(player, selectedItem);
                    return;
                }

                if (serverTrigger == "faction:player:subFactionMenu")
                {
                    FactionCommands.FactionCommandOnFactionSubSelect(player, selectedItem);
                    return;
                }

                if (serverTrigger == "faction:leader:showFactionRanks")
                {
                    LeaderCommands.OnRanksRankSelected(player, selectedItem);
                    return;
                }

                if (serverTrigger == "faction:leader:editFactionRank")
                {
                    LeaderCommands.OnRanksEditRank(player, selectedItem);
                    return;
                }

                if (serverTrigger == "faction:leader:memberList")
                {
                    LeaderCommands.OnLeaderSelectMember(player, selectedItem);
                    return;
                }
                if (serverTrigger == "faction:leader:memberList:selected")
                {
                    LeaderCommands.OnLeaderSelectMemberOption(player, selectedItem);
                    return;
                }

                #endregion Faction Commands

                #region Vehicle Inventory

                if (serverTrigger == "vehicle:inventory:mainMenu")
                {
                    VehicleInventory.InventoryMenuMainMenuSelect(player, selectedItem);
                    return;
                }

                if (serverTrigger == "vehicle:inventory:takeItem:list")
                {
                    VehicleInventory.InventoryMenuOnItemTake(player, selectedItem, index);
                    return;
                }

                if (serverTrigger == "vehicle:inventory:placeItem:list")
                {
                    VehicleInventory.InventoryMenuOnItemPlace(player, selectedItem, index);
                    return;
                }
                if (serverTrigger == "vehicle:inventory:placeQuantity") VehicleInventory.InventoryMenuOnPlaceQuantitySelect(player, selectedItem);

                if (serverTrigger == "vehicle:inventory:takeQuantity") VehicleInventory.InventoryMenuOnTakeQuantitySelect(player, selectedItem);

                #endregion Vehicle Inventory

                #region Bus Route

                if (serverTrigger == "bus:RouteSelected")
                {
                    BusCommands.OnBusRouteSelect(player, selectedItem);
                }

                #endregion Bus Route

                #region Duty System

                if (serverTrigger == "faction:duty:MainMenu")
                {
                    DutyHandler.OnMainMenuSelect(player, selectedItem);
                }

                #endregion Duty System

                #region Vehicle Mod System

                if (serverTrigger == "vehicleMod:ShowMainMenu") ModHandler.OnMainMenuSelect(player, selectedItem);

                if (serverTrigger == "vehicleMod:showModNameMenu") ModHandler.OnModNameMenuSelect(player, selectedItem);

                if (serverTrigger == "vehicleMod:PurchaseMenuSelect") ModHandler.OnPurchaseMenuSelect(player, selectedItem);

                if (serverTrigger == "focus:mechanic:addModSelected") FocusCommands.OnAddModSelected(player, selectedItem);

                if (serverTrigger == "focus:mechanic:onRemoveModSelect") FocusCommands.OnRemoveModSelect(player, selectedItem);

                if (serverTrigger == "vehicleMod:purchaseDigitalRadio") ModHandler.OnDigitalRadioPurchaseSelect(player, selectedItem);

                if (serverTrigger == "modShop:WheelTypeSelectionMainMenu") ModHandler.OnWheelTypeSelected(player, selectedItem);

                if (serverTrigger == "modShop:selectWheelName") ModHandler.OnWheelNameSelected(player, selectedItem, index);

                if (serverTrigger == "modShop:purchaseSelectedWheels") ModHandler.OnWheelPurchaseSelect(player, selectedItem);

                if (serverTrigger == "focus:mechanic:colorSelection") FocusCommands.OnResprayMenuSelect(player, selectedItem);

                if (serverTrigger == "focus:mechanic:selectWheelColor") FocusCommands.OnWheelRespraySelect(player, selectedItem, index);

                #endregion Vehicle Mod System

                #region Weapon System

                if (serverTrigger == "WeaponManagementMainMenu") WeaponCommands.EventWeaponManagementMainMenu(player, selectedItem, index);

                if (serverTrigger == "WeaponMenuWeaponManagement")
                    WeaponCommands.EventWeaponMenuWeaponManagement(player, selectedItem);

                if (serverTrigger == "WeaponManagementCombineAmmo") WeaponCommands.EventWeaponManagementCombineAmmo(player, selectedItem);

                if (serverTrigger == "admin:weapon:selectWeapon") AdminCommands.OnAdminSelectWeapon(player, selectedItem);
                if (serverTrigger == "admin:weapon:selectQuantity") AdminCommands.OnAdminWeaponQuantitySelect(player, selectedItem);

                #endregion Weapon System

                if (serverTrigger == "admin:drugs:SelectedDrug") AdminCommands.OnSelectedDrug(player, selectedItem);

                #region TattooMenu

                if (serverTrigger == "store:tattoo:MainMenuSelect") TattooStore.OnMainMenuSelect(player, selectedItem);
                if (serverTrigger == "store:tattoo:TattooItemSelect") TattooStore.OnTattooSelect(player, selectedItem);
                if (serverTrigger == "store:tattoo:ConfirmPurchase") TattooStore.OnConfirmPurchase(player, selectedItem);

                if (serverTrigger == "tattoo:store:RemoveTattooMainMenu") TattooStore.OnRemoveMainMenuSelect(player, selectedItem);
                if (serverTrigger == "store:tattoo:OnRemoveTattooSelect") TattooStore.OnRemoveTattooSelect(player, selectedItem);

                #endregion TattooMenu

                #region Hair Store

                if (serverTrigger == "store:HairStore:MainMenuSelect") HairStore.OnMainMenuSelect(player, selectedItem);

                if (serverTrigger == "store:Hair:OnHairSelect") HairStore.OnHairSelect(player, selectedItem, index);

                if (serverTrigger == "store:hair:OnFacialHairSelect") HairStore.OnFacialHairSelect(player, selectedItem, index);

                if (serverTrigger == "HairStore:ColorSelection") HairStore.OnHairColorSelect(player, selectedItem);

                #endregion Hair Store

                #region Property System

                if (serverTrigger == "property:SellPropertySelectProperty")
                    PropertyCommands.OnSellPropertySelectProperty(player, selectedItem);

                if (serverTrigger == "property:SelectPropertySellType") PropertyCommands.OnSelectPropertySellType(player, selectedItem);

                if (serverTrigger == "property:SellVoucherOptions") PropertyCommands.SellVoucherOption(player, selectedItem);

                if (serverTrigger == "property:sell:selectPlayer") PropertyCommands.OnPropertySellSelectPlayer(player, selectedItem);

                if (serverTrigger == "property:sell:offerToPlayer") PropertyCommands.PropertySellOfferToPlayer(player, selectedItem);

                if (serverTrigger == "admin:property:SelectInterior") AdminCommands.OnChangeInteriorSelect(player, selectedItem, index);

                if (serverTrigger == "property:storage:SelectMainOption")
                    PropertyCommands.OnPropertyInventorySelectMainMenu(player, selectedItem);

                if (serverTrigger == "property:storage:OnSelectStoreItem")
                    PropertyCommands.OnSelectStoreItem(player, selectedItem, index);

                if (serverTrigger == "property:storage:OnSelectTakeItem")
                    PropertyCommands.OnSelectTakeItem(player, selectedItem, index);

                if (serverTrigger == "admin:Properties:PlayerProperties") AdminCommands.OnPlayerPropertySelect(player, selectedItem);

                if (serverTrigger == "Property:ConfirmSellMortgageProperty") PropertyCommands.OnSelectMortgageSale(player, selectedItem);

                #endregion Property System

                #region Graffiti System

                if (serverTrigger == "graffiti:SelectColor") GraffitiCommands.OnGraffitiColorSelect(player, selectedItem);

                if (serverTrigger == "admin:graffiti:OnSelectGraffiti") AdminCommands.OnSelectGraffiti(player, selectedItem);

                #endregion Graffiti System

                #region Apartment System

                if (serverTrigger == "apartment:ShowApartmentList") ApartmentHandler.OnSelectApartment(player, selectedItem);

                if (serverTrigger == "apartments:ToggleLock") ApartmentHandler.EventApartmentToggleLock(player, selectedItem);

                #endregion Apartment System

                #region Delivery System

                if (serverTrigger == "job:delivery:ShowShipmentType") DeliveryCommands.OnSelectShipmentType(player, selectedItem);

                if (serverTrigger == "job:delivery:OnSelectShipmentPoint") DeliveryCommands.OnSelectShipmentLocation(player, selectedItem);
                if (serverTrigger == "job:delivery:OnSelectWarehouse") DeliveryCommands.OnSelectWarehouse(player, selectedItem);

                #endregion Delivery System

                #region Police System

                if (serverTrigger == "character:tickets:MainMenu") CharacterCommands.OnTicketMainMenuSelect(player, selectedItem, index);

                #endregion Police System

                #region Cycle Storage System

                if (serverTrigger == "vehicle:cycle:GetCycleSelect") Vehicle.Commands.OnGetCycleSelect(player, selectedItem, index);

                #endregion Cycle Storage System

                #region Key Smith Store

                if (serverTrigger == "store:keysmith:MainMenuSelect") KeySmith.OnKeySmithMainMenuSelect(player, selectedItem);

                if (serverTrigger == "store:keysmith:OnSelectKeyItem") KeySmith.OnKeySmithSelectKeyItem(player, selectedItem, index);

                #endregion Key Smith Store

                #region Gun Store

                if (serverTrigger == "gunstore:MainMenuSelect") GunStore.OnGunStoreMainMenuSelect(player, selectedItem);

                #endregion Gun Store

                #region Language System

                if (serverTrigger == "Languages:LearnLanguage") LanguageCommands.OnLearnLanguageSelect(player, selectedItem);

                if (serverTrigger == "Languages:SelectLanguage") LanguageCommands.OnSelectSpokenLanguage(player, selectedItem);

                #endregion Language System

                #region Backpack System

                if (serverTrigger == "backpack:MainMenuSelect") BackpackCommands.OnBackpackMainMenuSelect(player, selectedItem, index);

                if (serverTrigger == "backpack:ShowItemOptions") BackpackCommands.OnShowItemOptionSelected(player, selectedItem);

                if (serverTrigger == "Backpack:ConfirmGiveToPlayer") BackpackCommands.OnConfirmGiveToPlayer(player, selectedItem);

                if (serverTrigger == "backpack:OnItemQuantitySelect")
                    BackpackCommands.OnItemQuantitySelect(player, selectedItem);

                #endregion Backpack System

                if (serverTrigger == "CharacterCommands:ShowIdSelect") CharacterCommands.OnShowIdSelect(player, selectedItem, index);

                if (serverTrigger == "character:showInfoMenu") CharacterCommands.OnShowInfoMenu(player, selectedItem);

                if (serverTrigger == "Makeup:MainMenuSelect") MakeupHandler.OnMainMenuSelect(player, selectedItem);

                if (serverTrigger == "Makeup:OnSubMenuSelect") MakeupHandler.OnSubMenuSelect(player, selectedItem, index);

                #region EUP Menu

                if (serverTrigger == "EupMenu:Police:Male") EupMenu.OnMalePoliceSelect(player, selectedItem);

                if (serverTrigger == "EupMenu:Police:PropMale") EupMenu.OnMalePolicePropSelect(player, selectedItem);

                if (serverTrigger == "EupMenu:Police:MaleRanks") EupMenu.OnPoliceMaleRankSelect(player, selectedItem);

                if (serverTrigger == "EupMenu:Police:Female") EupMenu.OnFemalePoliceSelect(player, selectedItem);

                if (serverTrigger == "EupMenu:Police:PropFemale") EupMenu.OnFemalePolicePropSelect(player, selectedItem);

                #endregion EUP Menu

                #region Open World Storage

                if (serverTrigger == "OpenInventory:StorageMainMenu") OpenInventoryCommands.OnOWStorageMainMenu(player, selectedItem);
                if (serverTrigger == "OpenInventory:StorageTakeItem") OpenInventoryCommands.OnOWTakeItemSelect(player, selectedItem, index);
                if (serverTrigger == "OpenInventory:StoreItem") OpenInventoryCommands.OnOWStoreItemSelect(player, selectedItem, index);

                #endregion Open World Storage

                #region GPS System

                if (serverTrigger == "CharacterCommands:AddGpsWaypoint") CharacterCommands.OnSelectGpsAddWayPoint(player, selectedItem, index);
                if (serverTrigger == "CharacterCommands:AddGpsName") CharacterCommands.OnSelectGpsRename(player, selectedItem, index);
                if (serverTrigger == "CharacterCommands:gps:gpsSelected") CharacterCommands.OnSelectGps(player, selectedItem, index);
                if (serverTrigger == "CharacterCommands:gps:wayPointSelected") CharacterCommands.GpsWayPointSelect(player, selectedItem, index);
                if (serverTrigger == "CharacterCommands:gps:gpsSelectedRemoveWaypoint")
                    CharacterCommands.OnSelectGpsSelectedRemoveWaypoint(player, selectedItem, index);
                if (serverTrigger == "CharacterCommands:gps:removeWayPointSelected")
                    CharacterCommands.GpsRemoveWayPointSelected(player, selectedItem, index);

                #endregion GPS System

                #region Discord Intergration

                if (serverTrigger == "DiscordMenu:MainMenu") DiscordCommands.OnDiscordMenuSelect(player, selectedItem);

                #endregion Discord Intergration

                if (serverTrigger == "Inventory:OnGiveItemToPlayerSelect")
                    InventoryCommands.OnSelectItemGiveToPlayer(player, selectedItem, index);

                if (serverTrigger == "Inventory:SelectedItemGiveToPlayerQuantity")
                    InventoryCommands.SelectedItemGiveToPlayerQuantitySelect(player, selectedItem);

                #region Interior Mapping

                if (serverTrigger == "BuyObject:SelectItem")
                    PurchaseObjectHandler.OnBuyObjectSelectItem(player, selectedItem, index);

                if (serverTrigger == "InteriorMapping:SelectObject")
                    PurchaseObjectHandler.OnInteriorMappingSelectObject(player, selectedItem, index);

                if (serverTrigger == "InteriorMapping:ShowMoveObjectList")
                    PurchaseObjectHandler.OnShowObjectMoveListSelected(player, selectedItem, index);

                if (serverTrigger == "InteriorMapping:ShowPickupObjectList")
                    PurchaseObjectHandler.OnItemPickupSelect(player, selectedItem, index);

                #endregion Interior Mapping

                if (serverTrigger == "WelcomePlayer:JobMenu") WelcomePlayer.OnJobMenuSelectItem(player, selectedItem);

                #region Drug System Revamp

                if (serverTrigger == "DrugSystem:DrugsMainMenuSelect") Drug.Commands.OnDrugMainMenuSelect(player, selectedItem, index);

                if (serverTrigger == "DrugSystem:SubMenuDrugSelected") Drug.Commands.SubMenuDrugSelected(player, selectedItem);

                if (serverTrigger == "DrugSystem:CombineDrugWithBag") Drug.Commands.OnCombineDrugWithDrugBag(player, selectedItem, index);

                if (serverTrigger == "DrugSystem:SelectedCombineDrugToBagQuantity") Drug.Commands.OnSelectedCombineDrugToBag(player, selectedItem);

                #endregion Drug System Revamp
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        /// <summary>
        /// Fired when a player changes to a new list item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="listTrigger"></param>
        /// <param name="menuItemText">The menu item text value (left value)</param>
        /// <param name="listText">The value of the list item (right value)</param>
        public static void NativeMenuListChange(IPlayer player, string listTrigger, string menuItemText,
            string listText)
        {
            try
            {
                #region Inventory

                if (listTrigger == "InventoryMenuGiveItemToPlayerQuantityTrigger") InventoryCommands.OnGiveItemToPlayerQuantityChange(player, listText);

                if (listTrigger == "InventoryMenuGiveItemToPlayerQuantityTrigger") InventoryCommands.OnGiveItemToPlayerQuantityChange(player, listText);

                if (listTrigger == "InventoryMenuGiveItemToPlayerQuantityTrigger") InventoryCommands.OnGiveItemToPlayerQuantityChange(player, listText);

                if (listTrigger == "inventory:DropItemQuantityChange")
                    InventoryCommands.OnInventoryDropItemListChange(player, listText);

                if (listTrigger == "inventory:DeleteItemQuantityChange") InventoryCommands.OnInventoryDeleteItemListChange(player, listText);

                if (listTrigger == "Inventory:SelectedItemGiveToPlayerQuantityList")
                    InventoryCommands.SelectedItemGiveToPlayerQuantityChange(player, menuItemText, listText);

                if (listTrigger == "backpack:ItemQuantityChange") BackpackCommands.ItemQuantityChange(player, menuItemText, listText);

                if (listTrigger == "Inventory:Backpack:QuantityChange") InventoryCommands.OnBackpackQuantityChange(player, listText);

                #endregion Inventory

                #region Vehicle

                if (listTrigger == "vehicle:inventory:takeQuantityChange") VehicleInventory.InventoryMenuOnTakeQuantityChange(player, listText);

                if (listTrigger == "vehicle:inventory:placeQuantityChange") VehicleInventory.InventoryMenuOnPlaceQuantityChange(player, listText);

                #endregion Vehicle

                #region Mechanic Focus

                if (listTrigger == "focus:mechanic:resprayListChange") FocusCommands.OnResprayListChange(player, menuItemText, listText);

                #endregion Mechanic Focus

                #region Admin

                if (listTrigger == "admin:weapon:onWeaponQuantityChange") AdminCommands.OnAdminWeaponQuantityChange(player, listText);

                #endregion Admin

                #region Weapon System

                if (listTrigger == "WeaponManagementCombineAmmoListTrigger") WeaponCommands.EventWeaponManagementCombineAmmoListTrigger(player, listText);

                #endregion Weapon System

                #region Hair & Makeup

                if (listTrigger == "HairStore:OnHairColorListChange") HairStore.OnHairColorListChange(player, listText);

                if (listTrigger == "Makeup:OnSubMenuListChange") MakeupHandler.OnSubMenuListChange(player, menuItemText, listText);

                #endregion Hair & Makeup

                #region Drug System

                if (listTrigger == "DrugSystem:OnCombineDrugToBagListChange") Drug.Commands.OnCombineDrugToBagQuantityChange(player, menuItemText, listText);

                #endregion Drug System
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        /// <summary>
        /// Fired when a player moves to a new item index
        /// </summary>
        /// <param name="player"></param>
        /// <param name="changeTrigger"></param>
        /// <param name="newIndex"></param>
        /// <param name="itemText"></param>
        public static void NativeMenuIndexChange(IPlayer player, string changeTrigger, int newIndex, string itemText)
        {
            try
            {
                if (string.IsNullOrEmpty(changeTrigger)) return;

                if (changeTrigger == "ClothingListItemChange")
                {
                    ClothingStore.EventClothingListItemChange(player, newIndex, itemText);
                    return;
                }

                if (changeTrigger == "ClothingListSubItemChange")
                {
                    ClothingStore.EventClothingListSubItemChange(player, newIndex, itemText);
                    return;
                }

                if (changeTrigger == "AccessoryListItemChange")
                {
                    ClothingStore.EventAccessoryListItemChange(player, newIndex, itemText);
                    return;
                }

                if (changeTrigger == "AccessoryListSubItemChange")
                {
                    ClothingStore.EventAccessoryListSubItemChange(player, newIndex, itemText);
                }

                if (changeTrigger == "store:Hair:OnHairChange") HairStore.OnHairChange(player, newIndex, itemText);
                if (changeTrigger == "store:hair:OnFacialHairChange") HairStore.OnFacialHairChange(player, newIndex, itemText);

                if (changeTrigger == "vehicleMod:OnModNameIndexChange")
                    ModHandler.OnModNameChangeIndex(player, newIndex, itemText);

                if (changeTrigger == "modshop:OnWheelNameChangeIndex") ModHandler.OnWheelNameChangeIndex(player, newIndex, itemText);

                if (changeTrigger == "focus:mechanic:OnWheelResprayChange") FocusCommands.OnWheelResprayChange(player, newIndex, itemText);

                if (changeTrigger == "Makeup:OnSubMenuItemChange") MakeupHandler.OnSubMenuItemChange(player, itemText);

                if (changeTrigger == "store:tattoo:TattooItemChange") TattooStore.OnTattooItemChange(player, newIndex, itemText);

                if (changeTrigger == "BuyObject:ChangeItem") PurchaseObjectHandler.OnBuyObjectChangeItem(player, newIndex, itemText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}