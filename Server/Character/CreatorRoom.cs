using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Server.Apartments;
using Server.Character.Clothing;
using Server.Character.Tattoo;
using Server.Chat;
using Server.Dealerships;
using Server.Doors;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.Marker;
using Server.Extensions.TextLabel;
using Server.Focuses;
using Server.Groups.Police;
using Server.Inventory;
using Server.Language;
using Server.Models;
using Server.Motel;
using Server.Property;
using Server.Vehicle;
using Server.Weapons;

namespace Server.Character
{
    public class CreatorRoom
    {
        public static readonly Position CreatorPosition = new Position(-222.34286f, -1199.1692f, -148.92383f);

        public static readonly Rotation CreatorRotation = new DegreeRotation(0f, 0f, 267.874016f);

        /// <summary>
        /// Storage of Player IDs. Key = ID, Value = In use
        /// </summary>
        public static Dictionary<int, bool> PlayerIds = new Dictionary<int, bool>();

        public static int NextId = 1;

        private static readonly Position[] Positions =
        {
            new Position(-241.72748f, -1194.0396f, -148.92383f),
            new Position(-238.77362f, -1194.2373f, -148.92383f),
            new Position(-235.26593f, -1194.2902f, -148.92383f),
            new Position(-229.13406f, -1194.3693f, -148.92383f),
            new Position(-226.16704f, -1194.567f, -148.92383f),
            new Position(-223.02856f, -1194.8044f, -148.92383f)
        };

        public static Position DoorPosition = new Position(-222.23737f, -1191.1516f, -148.92383f);

        public static void SendToCreatorRoom(IPlayer player)
        {
            try
            {
                bool hasLabelData = player.GetData("CHARACTERLABELS", out string labelJson);

                if (hasLabelData)
                {
                    player.Emit("destroyNPCPreviews");

                    List<TextLabel> characterLabels = JsonConvert.DeserializeObject<List<TextLabel>>(labelJson);

                    foreach (TextLabel characterLabel in characterLabels)
                    {
                        TextLabelHandler.RemoveTextLabelForPlayer(player, characterLabel);
                    }
                }

                int currentId = player.GetPlayerId();

                if (currentId == 0)
                {
                    player.SetPlayerId(NextId);
                    NextId++;
                }

                player.GetClass().CreatorRoom = true;

                player.SendInfoNotification($"Your session ID is: {player.GetPlayerId()}");

                player.SetDateTime(1, 1, 1, 12, 0, 0);
                player.SetWeather(WeatherType.ExtraSunny);

                player.Dimension = (short)player.GetPlayerId();
                player.SetSyncedMetaData("PlayerDimension", player.Dimension);

                player.Model = (uint)PedModel.MovAlien01;

                player.SetPosition(new Position(-245.06374f, -1190.888f, -148.92383f), new DegreeRotation(0, 0, -85.03937f), 5000,
                    true, true, unfreezeTime: 1000);

                player.Rotation = new Rotation(0, 0, 270);

                List<Models.Character> playerCharacters = Models.Character.FetchCharacters(player.FetchAccount())
                    .Where(x => x.BioStatus == 2).ToList();

                List<TextLabel> textLabels = new List<TextLabel>();

                TextLabel doorLabel = new TextLabel($"Press 'F' to Leave", DoorPosition + new Position(0, 0, 0.75f),
                    TextFont.FontChaletComprimeCologne, new LsvColor(Color.DarkGray), 5f, player.Dimension);

                TextLabelHandler.LoadTextLabelForPlayer(player, doorLabel);
                textLabels.Add(doorLabel);

                if (playerCharacters.Any())
                {
                    foreach (Models.Character playerCharacter in playerCharacters)
                    {
                        using Context context = new Context();

                        Models.Character characterDb = context.Character.Find(playerCharacter.Id);

                        if (string.IsNullOrEmpty(characterDb.CustomCharacter))
                        {
                            CustomCharacter customCharacter = CustomCharacter.DefaultCharacter();

                            customCharacter.Gender = characterDb.Sex;

                            characterDb.CustomCharacter = JsonConvert.SerializeObject(customCharacter);
                        }

                        if (string.IsNullOrEmpty(characterDb.ClothesJson))
                        {
                            characterDb.ClothesJson = JsonConvert.SerializeObject(new List<ClothesData>());
                        }

                        if (string.IsNullOrEmpty(characterDb.AccessoryJson))
                        {
                            characterDb.AccessoryJson = JsonConvert.SerializeObject(new List<AccessoryData>());
                        }

                        if (string.IsNullOrEmpty(characterDb.FactionList))
                        {
                            characterDb.FactionList = JsonConvert.SerializeObject(new List<PlayerFaction>());
                        }

                        if (string.IsNullOrEmpty(characterDb.TattooJson))
                        {
                            characterDb.TattooJson = JsonConvert.SerializeObject(new List<TattooData>());
                        }

                        if (string.IsNullOrEmpty(characterDb.FocusJson))
                        {
                            characterDb.FocusJson = JsonConvert.SerializeObject(new List<FocusTypes>());
                        }

                        if (characterDb.InventoryID == 0)
                        {
                            InventoryData inv = InventoryData.CreateDefaultInventory(10, 5);

                            characterDb.InventoryID = inv.ID;
                        }

                        if (string.IsNullOrEmpty(characterDb.Languages))
                        {
                            // Adds English as a default

                            Language.Language language = LanguageHandler.Languages.First();

                            characterDb.Languages = JsonConvert.SerializeObject(new List<Language.Language>
                            {
                                language
                            });
                        }

                        if (string.IsNullOrEmpty(characterDb.CurrentLanguage))
                        {
                            Language.Language language = LanguageHandler.Languages.First();

                            characterDb.CurrentLanguage = JsonConvert.SerializeObject(language);
                        }

                        if (string.IsNullOrEmpty(characterDb.LicensesHeld))
                        {
                            characterDb.LicensesHeld = JsonConvert.SerializeObject(new List<LicenseTypes>());
                        }

                        if (string.IsNullOrEmpty(characterDb.Outfits))
                        {
                            characterDb.Outfits = JsonConvert.SerializeObject(new List<Outfit>());
                        }

                        context.SaveChanges();

                        int index = playerCharacters.IndexOf(playerCharacter);

                        switch (index)
                        {
                            case 0:
                                {
                                    Position position = Positions[0];
                                    TextLabel characterLabel = new TextLabel(
                                        $"{playerCharacter.Name}\nPlaytime: {playerCharacter.TotalHours}:{playerCharacter.TotalMinutes}\nPress 'F' to Play",
                                        position - new Position(0, 0, 1.5f), TextFont.FontChaletComprimeCologne,
                                        new LsvColor(Color.BurlyWood), dimension: player.Dimension);
                                    TextLabelHandler.LoadTextLabelForPlayer(player, characterLabel);
                                    textLabels.Add(characterLabel);
                                    break;
                                }
                            case 1:
                                {
                                    Position position = Positions[1];
                                    TextLabel characterLabel = new TextLabel(
                                        $"{playerCharacter.Name}\nPlaytime: {playerCharacter.TotalHours}:{playerCharacter.TotalMinutes}\nPress 'F' to Play",
                                        position - new Position(0, 0, 1.5f), TextFont.FontChaletComprimeCologne,
                                        new LsvColor(Color.BurlyWood), dimension: player.Dimension);
                                    TextLabelHandler.LoadTextLabelForPlayer(player, characterLabel);
                                    textLabels.Add(characterLabel);
                                    break;
                                }
                            case 2:
                                {
                                    Position position = Positions[2];
                                    TextLabel characterLabel = new TextLabel(
                                        $"{playerCharacter.Name}\nPlaytime: {playerCharacter.TotalHours}:{playerCharacter.TotalMinutes}\nPress 'F' to Play",
                                        position - new Position(0, 0, 1.5f), TextFont.FontChaletComprimeCologne,
                                        new LsvColor(Color.BurlyWood), dimension: player.Dimension);
                                    TextLabelHandler.LoadTextLabelForPlayer(player, characterLabel);
                                    textLabels.Add(characterLabel);
                                    break;
                                }
                            case 3:
                                {
                                    Position position = Positions[3];
                                    TextLabel characterLabel = new TextLabel(
                                        $"{playerCharacter.Name}\nPlaytime: {playerCharacter.TotalHours}:{playerCharacter.TotalMinutes}\nPress 'F' to Play",
                                        position - new Position(0, 0, 1.5f), TextFont.FontChaletComprimeCologne,
                                        new LsvColor(Color.BurlyWood), dimension: player.Dimension);
                                    TextLabelHandler.LoadTextLabelForPlayer(player, characterLabel);
                                    textLabels.Add(characterLabel);
                                    break;
                                }
                            case 4:
                                {
                                    Position position = Positions[4];
                                    TextLabel characterLabel = new TextLabel(
                                        $"{playerCharacter.Name}\nPlaytime: {playerCharacter.TotalHours}:{playerCharacter.TotalMinutes}\nPress 'F' to Play",
                                        position - new Position(0, 0, 1.5f), TextFont.FontChaletComprimeCologne,
                                        new LsvColor(Color.BurlyWood), dimension: player.Dimension);
                                    TextLabelHandler.LoadTextLabelForPlayer(player, characterLabel);
                                    textLabels.Add(characterLabel);
                                    break;
                                }
                            case 5:
                                {
                                    Position position = Positions[5];
                                    TextLabel characterLabel = new TextLabel(
                                        $"{playerCharacter.Name}\nPlaytime: {playerCharacter.TotalHours}:{playerCharacter.TotalMinutes}\nPress 'F' to Play",
                                        position - new Position(0, 0, 1.5f), TextFont.FontChaletComprimeCologne,
                                        new LsvColor(Color.BurlyWood), dimension: player.Dimension);
                                    TextLabelHandler.LoadTextLabelForPlayer(player, characterLabel);
                                    textLabels.Add(characterLabel);
                                    break;
                                }
                        }

                        LoadPreviewModel(player, characterDb, index);
                    }
                }

                player.SetData("INCREATORROOM", true);
                player.SetData("CHARACTERLABELS", JsonConvert.SerializeObject(textLabels));

                Alt.EmitAllClients("clearBlood", player);

                Models.Account playerAccount = player.FetchAccount();

                if (playerAccount.LastCharacter > 0)
                {
                    Models.Character lastCharacter = Models.Character.GetCharacter(playerAccount.LastCharacter);

                    if (lastCharacter != null)
                    {
                        player.SetCharacterId(lastCharacter.Id);
                        CharacterHandler.LoadCustomCharacter(player);
                        player.SendInfoNotification($"Loading in as your last character: {lastCharacter.Name}.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        private static void LoadPreviewModel(IPlayer player, Models.Character character, int pos)
        {
            List<ClothesData> clothingData =
                JsonConvert.DeserializeObject<List<ClothesData>>(character.ClothesJson);

            ClothesData topData = clothingData.FirstOrDefault(x => x.slot == (int)ClothesType.Top);

            int torso = 0;

            if (topData != null)
            {
                torso = Clothes.GetTorsoDataForTop(topData.drawable, character.Sex == 0);
            }

            player.Emit("LoadNPCPreview", pos, character.CustomCharacter, character.ClothesJson,
                character.AccessoryJson, torso);
        }

        public static void SelectPlayerCharacter(IPlayer player)
        {
            int nearestCharacter = -1;

            foreach (Position position in Positions)
            {
                if (player.Position.Distance(position) <= 1.5f)
                {
                    nearestCharacter = Array.IndexOf(Positions, position, 0);
                }
            }

            if (nearestCharacter == -1)
            {
                player.SendErrorNotification("You're not near a character.");
                return;
            }

            List<Models.Character> playerCharacters = Models.Character.FetchCharacters(player.FetchAccount());

            if (playerCharacters.ElementAtOrDefault(nearestCharacter) == null)
            {
                player.SendErrorNotification("You're not near a character.");
                return;
            }

            Models.Character selectedCharacter = playerCharacters[nearestCharacter];

            if (selectedCharacter.BioStatus != 2)
            {
                player.SendErrorNotification("This characters bio hasn't yet been accepted.");
                return;
            }

            player.SendInfoNotification($"You've selected {selectedCharacter.Name}.");

            player.SetCharacterId(selectedCharacter.Id);

            CharacterHandler.LoadCustomCharacter(player);

            using Context context = new Context();

            Models.Account playerAccount = context.Account.Find(player.GetClass().AccountId);

            playerAccount.LastCharacter = selectedCharacter.Id;

            context.SaveChanges();

            player.GetClass().WalkStyle = selectedCharacter.WalkStyle;

            if (selectedCharacter.StartStage != 2)
            {
                if (selectedCharacter.StartStage == 0)
                {
                    // Not been to creator
                    CharacterCreator.SendToCreator(player, 1);
                    return;
                }
            }
        }

        public static void LeaveCreatorRoom(IPlayer player)
        {
            try
            {
                Models.Character playerCharacter = player.FetchCharacter();

                if (playerCharacter.StartStage == 1)
                {
                    bool previousTutorialCompleted = Models.Character
                        .FetchCharacters(player.FetchAccount()).Any(x => x.StartStage == 2);

                    if (previousTutorialCompleted)
                    {
                        using Context context = new Context();

                        Models.Character pCharacter = context.Character.Find(playerCharacter.Id);

                        pCharacter.StartStage = 2;
                        pCharacter.PosX = TutorialHandler.StrawberrySpawnPosition.X;
                        pCharacter.PosY = TutorialHandler.StrawberrySpawnPosition.Y;
                        pCharacter.PosZ = TutorialHandler.StrawberrySpawnPosition.Z;

                        pCharacter.Dimension = 0;
                        context.SaveChanges();

                        LeaveCreatorRoom(player);

                        player.GetClass().CompletedTutorial = true;
                    }
                    // Send to Tutorial
                    TutorialHandler.StartTutorial(player);
                    return;
                }

                player.SetData("INCREATORROOM", false);

                player.ShowHud(false);

                Position characterPosition = new Position(playerCharacter.PosX, playerCharacter.PosY,
                    playerCharacter.PosZ + 0.25f);

                //player.Position = characterPosition;

                player.Emit("destroyNPCPreviews");

                player.AllowMouseContextMenu(true);

                player.GetData("CHARACTERLABELS", out string labelJson);

                List<TextLabel> characterLabels = JsonConvert.DeserializeObject<List<TextLabel>>(labelJson);

                foreach (TextLabel characterLabel in characterLabels)
                {
                    TextLabelHandler.RemoveTextLabelForPlayer(player, characterLabel);
                }

                TextLabelHandler.RemoveAllTextLabelsForPlayer(player);
                BlipHandler.RemoveAllBlipsForPlayer(player);

                player.SetWeather(TimeWeather.CurrentWeatherType);

                DateTime dateNow = DateTime.Now;

                player.SetDateTime(dateNow.Day, dateNow.Month, dateNow.Year, Settings.ServerSettings.Hour,
                    Settings.ServerSettings.Minute, 0);

                player.GetClass().Spawned = true;

                player.GetClass().CreatorRoom = false;

                LoadVehicle.CharacterLoaded(playerCharacter);
                DroppedItems.LoadDroppedItemsForPlayer(player);
                TextLabelHandler.LoadTextLabelsOnSpawn(player);
                MarkerHandler.LoadMarkersOnSpawn(player);
                BlipHandler.LoadBlipsOnSpawn(player);
                WelcomePlayer.InitPedForPlayer(player);

                player.Emit("CharacterLoaded", true);

                player.Emit("hud:CharacterLoaded");

                player.SetPlayerNameTag($"{player.GetClass().Name}");

                Logging.AddToCharacterLog(player, $"Left the Creator Room.");

                Models.Account playerAccount = player.FetchAccount();

                if (playerAccount.InJail && playerAccount.JailMinutes > 0)
                {
                    player.Dimension = player.GetPlayerId();
                    player.SetPosition(PoliceHandler.JailLocation + new Position(0, 0, 0.25f), Rotation.Zero, 5000,
                        switchOut: true);
                    player.SetSyncedMetaData("PlayerDimension", player.Dimension);
                    CharacterHandler.LoadCustomCharacter(player);
                }
                else if (playerAccount.InJail && playerAccount.JailMinutes <= 0)
                {
                    using Context accountContext = new Context();

                    playerAccount = accountContext.Account.Find(player.GetClass().AccountId);

                    // In jail and finished time;
                    player.Position = PoliceHandler.UnJailPosition;
                    player.Dimension = 0;
                    playerAccount.InJail = false;
                    playerAccount.JailMinutes = 0;
                    player.SendAdminMessage($"You have been released from jail.");
                    player.SetSyncedMetaData("PlayerDimension", player.Dimension);

                    accountContext.SaveChanges();
                }

                if (playerCharacter.InJail && playerCharacter.JailMinutes >= 1)
                {
                    KeyValuePair<Position, int> cell = PoliceHandler.JailCells.OrderBy(x => x.Value).FirstOrDefault();

                    PoliceHandler.JailCells.Remove(cell.Key);

                    int newCount = cell.Value + 1;

                    PoliceHandler.JailCells.Add(cell.Key, newCount);

                    player.Dimension = 0;

                    player.SetData("InJailCell", cell.Key);

                    player.Dimension = player.GetPlayerId();
                    player.SetPosition(cell.Key + new Position(0, 0, 0.25f), Rotation.Zero, 5000,
                        switchOut: true);
                    player.SetSyncedMetaData("PlayerDimension", player.Dimension);
                    CharacterHandler.LoadCustomCharacter(player);
                }
                else if (playerCharacter.InJail && playerCharacter.JailMinutes <= 0)
                {
                    using Context context = new Context();
                    Models.Character character = context.Character.Find(playerCharacter.Id);

                    if (character == null)
                    {
                        player.SendErrorNotification("An error has occurred.");
                        return;
                    }

                    playerCharacter.InJail = false;
                    playerCharacter.JailMinutes = 0;
                    characterPosition = PoliceHandler.UnJailPosition + new Position(0, 0, 0.25f);
                    player.SendInfoNotification($"You have been released from jail.");
                    context.SaveChanges();

                    player.SetPosition(characterPosition + new Position(0, 0, 0.25f), Rotation.Zero, 5000,
                        switchOut: true);
                    CharacterHandler.LoadCustomCharacter(player, true);

                    if (player.HasData("InJailCell"))
                    {
                        player.GetData("InJailCell", out Position cellPosition);

                        player.DeleteData("InJailCell");

                        KeyValuePair<Position, int> cell = PoliceHandler.JailCells.FirstOrDefault(x => x.Key == cellPosition);

                        PoliceHandler.JailCells.Remove(cell.Key);

                        int newCount = cell.Value - 1;

                        PoliceHandler.JailCells.Add(cell.Key, newCount);
                    }

                    player.SendInfoNotification($"You are leaving the room. Please wait.");
                }
                else
                {
                    if (playerCharacter.Dimension > 0)
                    {
                        if (playerCharacter.InMotel > 0)
                        {
                            using Context context = new Context();

                            Models.Motel inMotel = context.Motels.Find(playerCharacter.InMotel);

                            if (inMotel == null)
                            {
                                Console.WriteLine(
                                    $" {playerCharacter.Name} Has tried leaving a motel room that doesn't exist.");
                                return;
                            }

                            MotelRoom inMotelRoom = JsonConvert.DeserializeObject<List<MotelRoom>>(inMotel.RoomList)
                                .FirstOrDefault(x => x.Id == playerCharacter.InMotelRoom);

                            if (inMotelRoom == null)
                            {
                                Console.WriteLine(
                                    $" {playerCharacter.Name} Has tried leaving a motel room that doesn't exist.");
                                return;
                            }

                            MotelHandler.SetPlayerIntoMotelRoom(player, inMotelRoom);
                            return;
                        }

                        if (playerCharacter.InsideApartmentComplex > 0)
                        {
                            ApartmentComplexes complex =
                                ApartmentComplexes.FetchApartmentComplex(playerCharacter.InsideApartmentComplex);

                            if (complex == null)
                            {
                                player.SendErrorNotification("An error occurred setting you inside the apartment.");
                                return;
                            }

                            List<Apartment> apartments =
                                JsonConvert.DeserializeObject<List<Apartment>>(complex.ApartmentList);

                            Apartment apartment =
                                apartments.FirstOrDefault(x => x.Name == playerCharacter.InsideApartment);

                            if (apartment == null)
                            {
                                player.SendErrorNotification("An error occurred fetching the apartment data.");
                                return;
                            }

                            Interiors apartmentInterior =
                                Interiors.InteriorList.FirstOrDefault(x => x.InteriorName == apartment.InteriorName);

                            if (apartmentInterior == null)
                            {
                                player.SendErrorNotification("An error occurred fetching the apartment interior.");
                                return;
                            }

                            int newDimension = complex.Id;

                            newDimension *= 10000;
                            newDimension += apartments.IndexOf(apartment);

                            if (!string.IsNullOrEmpty(apartmentInterior.Ipl))
                            {
                                player.RequestIpl(apartmentInterior.Ipl);
                            }

                            List<string> propList = JsonConvert.DeserializeObject<List<string>>(apartment.PropList);

                            if (propList.Any())
                            {
                                foreach (string prop in propList)
                                {
                                    player.LoadInteriorProp(prop);
                                }
                            }

                            player.Dimension = newDimension;
                            player.SetSyncedMetaData("PlayerDimension", player.Dimension);

                            player.SetPosition(apartmentInterior.Position, Rotation.Zero);
                        }
                        else
                        {
                            Models.Property insideProperty =
                                Models.Property.FetchProperty((int)playerCharacter.Dimension);

                            if (insideProperty == null)
                            {
                                player.SendErrorNotification("An error occurred fetching this property.");
                                return;
                            }

                            Interiors propertyInterior =
                                Interiors.InteriorList.FirstOrDefault(
                                    x => x.InteriorName == insideProperty.InteriorName);

                            if (propertyInterior == null)
                            {
                                player.SendErrorNotification("An error occurred fetching the interior.");
                                return;
                            }

                            player.RequestIpl(propertyInterior.Ipl);

                            player.Dimension = insideProperty.Id;
                            player.SetPosition(propertyInterior.Position, Rotation.Zero);
                            player.SetSyncedMetaData("PlayerDimension", player.Dimension);

                            List<string> propertyPropList =
                                JsonConvert.DeserializeObject<List<string>>(insideProperty.PropList);

                            if (propertyPropList.Any())
                            {
                                foreach (string prop in propertyPropList)
                                {
                                    player.LoadInteriorProp(prop);
                                }
                            }
                        }
                    }
                    else
                    {
                        player.SetPosition(characterPosition + new Position(0, 0, 0.25f), Rotation.Zero, 5000,
                            switchOut: true);
                    }
                }

                player.Dimension = (int)playerCharacter.Dimension;
                player.SetSyncedMetaData("PlayerDimension", player.Dimension);

                if (!string.IsNullOrEmpty(playerCharacter.CurrentLanguage))
                {
                    player.GetClass().SpokenLanguage =
                        JsonConvert.DeserializeObject<Language.Language>(playerCharacter.CurrentLanguage);
                }

                player.GetClass().SpokenLanguages =
                    JsonConvert.DeserializeObject<List<Language.Language>>(playerCharacter.Languages);

                CharacterHandler.LoadCustomCharacter(player, true);

                player.SendInfoNotification($"You are leaving the room. Please wait.");

                DoorHandler.UpdateDoorsForPlayer(player);

                MotelHandler.LoadMotelsForPlayer(player);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}