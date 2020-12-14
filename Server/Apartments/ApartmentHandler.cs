using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.Marker;
using Server.Extensions.TextLabel;
using Server.Inventory;
using Server.Models;
using Server.Property;
using Blip = Server.Objects.Blip;

namespace Server.Apartments
{
    public class ApartmentHandler
    {
        public static Dictionary<int, Blip> Blips = new Dictionary<int, Blip>();

        public static Dictionary<int, TextLabel> TextLabels = new Dictionary<int, TextLabel>();

        public static Dictionary<int, Marker> Markers = new Dictionary<int, Marker>();

        public static void LoadApartments()
        {
            Console.WriteLine($"Loading Apartment Complexes");

            List<ApartmentComplexes> apartments = ApartmentComplexes.FetchApartmentComplexes();

            foreach (ApartmentComplexes complex in apartments)
            {
                LoadApartmentComplex(complex);
            }

            Console.WriteLine($"Loaded {apartments.Count} Apartment Complexes");
        }

        public static void LoadApartmentComplex(ApartmentComplexes complex)
        {
            Position complexPosition = new Position(complex.PosX, complex.PosY, complex.PosZ);

            Blip complexBlip = new Blip(complex.ComplexName, complexPosition, 475, 6, 0.8f);

            complexBlip.Add();

            Blips.Add(complex.Id, complexBlip);

            TextLabel complexLabel = new TextLabel(complex.ComplexName, complexPosition, TextFont.FontChaletComprimeCologne, new LsvColor(204, 51, 51));

            complexLabel.Add();

            TextLabels.Add(complex.Id, complexLabel);

            Marker complexMarker = new Marker(MarkerType.MarkerTypeUpsideDownCone, complexPosition, Vector3.Zero, Rotation.Zero, 1f, Color.FromArgb(242, 242, 118));

            complexMarker.Add();

            Markers.Add(complex.Id, complexMarker);
        }

        public static void UnloadApartmentComplex(ApartmentComplexes complex)
        {
            var blipInfo = Blips.FirstOrDefault(x => x.Key == complex.Id);
            
            blipInfo.Value?.Remove();

            var textLabelInfo = TextLabels.FirstOrDefault(x => x.Key == complex.Id);
            
            textLabelInfo.Value?.Remove();

            var markerInfo = Markers.FirstOrDefault(x => x.Key == complex.Id);
            
            markerInfo.Value?.Remove();

            Blips.Remove(complex.Id);
            TextLabels.Remove(complex.Id);
            Markers.Remove(complex.Id);
        }

        public static void LoadApartmentList(IPlayer player, ApartmentComplexes complex)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            List<Apartment> apartments = JsonConvert.DeserializeObject<List<Apartment>>(complex.ApartmentList);

            if (!apartments.Any())
            {
                player.SendErrorNotification("The complex is still being built.");
                return;
            }

            var apartmentsOrdered = apartments.OrderBy(x => x.Floor);

            foreach (Apartment apartment in apartmentsOrdered)
            {
                if (apartment.Owner == 0 && apartment.Price > 0)
                {
                    menuItems.Add(new NativeMenuItem(apartment.Name, $"Floor: {apartment.Floor} - ~g~FOR SALE: {apartment.Price:C}"));
                }
                else
                {
                    menuItems.Add(new NativeMenuItem(apartment.Name, $"Floor: {apartment.Floor}"));
                }
            }

            NativeMenu menu = new NativeMenu("apartment:ShowApartmentList", complex.ComplexName, "Select an Apartment", menuItems);

            player.SetData("ApartmentComplexList", JsonConvert.SerializeObject(complex));

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectApartment(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("ApartmentComplexList", out string complexJson);

            ApartmentComplexes complex = JsonConvert.DeserializeObject<ApartmentComplexes>(complexJson);

            List<Apartment> apartments = JsonConvert.DeserializeObject<List<Apartment>>(complex.ApartmentList);

            Apartment selectedApartment = apartments.FirstOrDefault(x => x.Name == option);

            if (selectedApartment == null)
            {
                player.SendErrorNotification("An error occurred fetching the apartment.");
                return;
            }

            if (selectedApartment.Locked)
            {
                player.SendErrorNotification("This apartment is locked.");
                return;
            }

            Interiors interior =
                Interiors.InteriorList.FirstOrDefault(s => s.InteriorName == selectedApartment.InteriorName);

            if (interior == null)
            {
                player.SendErrorNotification("Unable to fetch the interior for the apartment.");
                return;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            int newDimension = complex.Id;

            newDimension *= 10000;
            newDimension += apartments.IndexOf(selectedApartment);

            playerCharacter.Dimension = newDimension;
            playerCharacter.InsideApartmentComplex = complex.Id;
            playerCharacter.InsideApartment = selectedApartment.Name;

            context.SaveChanges();
            

            player.Position = interior.Position;

            player.RequestIpl(interior.Ipl);
            player.SetSyncedMetaData("PlayerDimension", player.Dimension);

            List<string> propList = JsonConvert.DeserializeObject<List<string>>(selectedApartment.PropList);

            if (propList.Any())
            {
                foreach (string prop in propList)
                {
                    player.LoadInteriorProp(prop);
                }
            }
        }

        public static void LeaveApartment(IPlayer player)
        {
            using Context context = new Context();

            var playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            var apartmentComplex = context.ApartmentComplexes.Find(playerCharacter.InsideApartmentComplex);

            var apartment = JsonConvert.DeserializeObject<List<Apartment>>(apartmentComplex.ApartmentList)
                .FirstOrDefault(x => x.Name == playerCharacter.InsideApartment);

            playerCharacter.Dimension = 0;
            playerCharacter.InsideApartmentComplex = 0;
            playerCharacter.InsideApartment = null;
            context.SaveChanges();
            

            player.Dimension = 0;
            player.Position = new Vector3(apartmentComplex.PosX, apartmentComplex.PosY, apartmentComplex.PosZ);
            player.SetSyncedMetaData("PlayerDimension", player.Dimension);

            List<string> propList = JsonConvert.DeserializeObject<List<string>>(apartment.PropList);

            if (propList.Any())
            {
                foreach (var prop in propList)
                {
                    player.UnloadInteriorProp(prop);
                }
            }
        }

        public static void PurchaseApartment(IPlayer player, ApartmentComplexes complex, Apartment apartment)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            string newKey = Utility.GenerateRandomString(8);

            Inventory.Inventory inventory = player.FetchInventory();

            inventory.AddItem(new InventoryItem("ITEM_APARTMENT_KEY", apartment.Name, newKey));

            using Context context = new Context();

            ApartmentComplexes apartmentComplex = context.ApartmentComplexes.Find(complex.Id);

            List<Apartment> apartments =
                JsonConvert.DeserializeObject<List<Apartment>>(apartmentComplex.ApartmentList);

            Apartment apartmentDb = apartments.FirstOrDefault(x => x.Name == apartment.Name);

            apartmentDb.Owner = playerCharacter.Id;
            apartmentDb.KeyCode = newKey;

            apartmentComplex.ApartmentList = JsonConvert.SerializeObject(apartments);

            context.SaveChanges();

            player.SendInfoNotification($"You've bought {apartment.Name} for {apartment.Price:C}.");
            return;
        }

        public static void ToggleApartmentLock(IPlayer player)
        {
            using Context context = new Context();
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            ApartmentComplexes apartmentComplex =
                context.ApartmentComplexes.Find(playerCharacter.InsideApartmentComplex);
            List<Apartment> apartments =
                JsonConvert.DeserializeObject<List<Apartment>>(apartmentComplex.ApartmentList);

            Apartment apartment = apartments.FirstOrDefault(x => x.Name == playerCharacter.InsideApartment);

            List<InventoryItem> keyList = player.FetchInventory().GetInventoryItems("ITEM_APARTMENT_KEY");

            if (keyList.FirstOrDefault(i => i.ItemValue == apartment.KeyCode) == null)
            {
                player.SendErrorNotification("You don't have keys");
                return;
            }

            apartment.Locked = !apartment.Locked;

            apartmentComplex.ApartmentList = JsonConvert.SerializeObject(apartments);

            context.SaveChanges();

            string lockState;

            lockState = apartment.Locked ? "locked" : "unlocked";

            player.SendInfoNotification($"You've {lockState} {apartment.Name}");
        }

        public static void LoadApartmentLockMenu(IPlayer player, ApartmentComplexes apartmentComplex)
        {
            List<Apartment> apartments =
                JsonConvert.DeserializeObject<List<Apartment>>(apartmentComplex.ApartmentList);

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> keyList = playerInventory.GetInventoryItems("ITEM_APARTMENT_KEY");

            List<Apartment> accessApartments = new List<Apartment>();

            foreach (var apartment in apartments)
            {
                foreach (var inventoryItem in keyList)
                {
                    if (apartment.KeyCode == inventoryItem.ItemValue)
                    {
                        accessApartments.Add(apartment);
                        continue;
                    }
                }
            }

            if (!accessApartments.Any())
            {
                player.SendErrorNotification("You don't have any keys for this complex.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (var accessApartment in accessApartments)
            {
                menuItems.Add(new NativeMenuItem(accessApartment.Name));
            }

            player.SetData("ATAPARTMENTCOMPLEX", JsonConvert.SerializeObject(apartmentComplex));

            NativeMenu menu = new NativeMenu("apartments:ToggleLock", apartmentComplex.ComplexName, "Select an Apartment to unlock", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void EventApartmentToggleLock(IPlayer player, string option)
        {
            if (option == "Close") return;

            using Context context = new Context();

            player.GetData("ATAPARTMENTCOMPLEX", out string complexJson);

            ApartmentComplexes apartmentComplex =
                JsonConvert.DeserializeObject<ApartmentComplexes>(complexJson);

            if (apartmentComplex == null)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            ApartmentComplexes dbComplex = context.ApartmentComplexes.Find(apartmentComplex.Id);

            List<Apartment> dbApartments = JsonConvert.DeserializeObject<List<Apartment>>(dbComplex.ApartmentList);

            Apartment apartment = dbApartments.FirstOrDefault(x => x.Name == option);

            if (apartment == null)
            {
                player.SendErrorNotification("An error occurred fetching your apartment.");
                return;
            }

            apartment.Locked = !apartment.Locked;

            dbComplex.ApartmentList = JsonConvert.SerializeObject(dbApartments);

            context.SaveChanges();

            

            string lockState = apartment.Locked ? "locked" : "unlocked";

            player.SendInfoNotification($"You've {lockState} {apartment.Name}");
        }

        /// <summary>
        /// Reloads Apartment props
        /// </summary>
        /// <param name="apartment"></param>
        public static void ReloadApartment(Apartment apartment)
        {
            List<IPlayer> players = Alt.Server.GetPlayers()
                .Where(x => x.FetchCharacter()?.InsideApartment == apartment.Name).ToList();

            if (!players.Any()) return;

            foreach (IPlayer player in players)
            {
                foreach (string prop in JsonConvert.DeserializeObject<List<string>>(apartment.PropList))
                {
                    player.UnloadInteriorProp(prop);
                }
            }

            foreach (IPlayer player in players)
            {
                foreach (string prop in JsonConvert.DeserializeObject<List<string>>(apartment.PropList))
                {
                    player.LoadInteriorProp(prop);
                }
            }
        }
    }
}