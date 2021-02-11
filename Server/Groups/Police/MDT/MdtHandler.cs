using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Character;
using Server.Chat;
using Server.Extensions;
using Server.Models;

namespace Server.Groups.Police.MDT
{
    public class MdtHandler
    {
        public static List<Call911> CallList = new List<Call911>();

        private static readonly string _unitData = "mdc:UnitName";

        /// <summary>
        /// Show the MDT to a player
        /// </summary>
        /// <param name="player"></param>
        public static void ShowMdt(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            Faction activeFaction = Faction.FetchFaction(playerCharacter.ActiveFaction);

            if (activeFaction == null) return;

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson);

            PlayerFaction playerFaction = JsonConvert
                .DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList)
                .FirstOrDefault(x => x.Id == activeFaction.Id);

            if (playerFaction == null) return;

            Rank playerRank = factionRanks.FirstOrDefault(x => x.Id == playerFaction.RankId);

            if (playerRank == null) return;

            bool hasUnitData = player.GetData("mdc:UnitName", out string unit);

            player.ChatInput(false);
            player.HideChat(true);
            player.ShowCursor(true);
            player.FreezeCam(true);
            player.FreezePlayer(true);

            if (hasUnitData)
            {
                player.Emit("showMDC", playerCharacter.Name, playerRank.Name, unit);
            }
            else
            {
                player.Emit("showMDC", playerCharacter.Name, playerRank.Name);
            }
        }

        /// <summary>
        /// Close the MDC
        /// </summary>
        /// <param name="player"></param>
        public static void CloseMdt(IPlayer player)
        {
            player.Emit("closeMDC");
            player.ChatInput(true);
            player.HideChat(false);
            player.ShowCursor(false);
            player.FreezeCam(false);
            player.FreezePlayer(false);
        }

        /// <summary>
        /// When a player requests backup through the MDT
        /// </summary>
        /// <param name="player"></param>
        public static void PlayerRequestBackup(IPlayer player)
        {
            player.GetData(_unitData, out string unit);
            //FactionCommands.FactionCommandRadio(player, $"{unit} requesting back up!");
            player.SendInfoNotification("Backup requested!");
        }

        /// <summary>
        /// When a player reports responding to last 911 call
        /// </summary>
        /// <param name="player"></param>
        public static void PlayerRespond911(IPlayer player)
        {
            player.GetData(_unitData, out string unit);
            //FactionCommands.FactionCommandRadio(player, $"{unit} responding to last 911 call.");
            player.SendInfoNotification("Responding last 911!");
        }

        /// <summary>
        /// Fetches list of unanswered 911 calls
        /// </summary>
        /// <param name="player"></param>
        public static void Fetch911Calls(IPlayer player)
        {
            player.Emit("911CallList", JsonConvert.SerializeObject(CallList));
        }

        /// <summary>
        /// Fetch a list of results from input
        /// </summary>
        /// <param name="player"></param>
        /// <param name="nameInput"></param>
        public static void FetchPersonSearch(IPlayer player, string nameInput)
        {
            using Context context = new Context();
            List<Models.Character> characterList = context.Character.Where(x => x.Name.ToLower().Contains(nameInput.ToLower())).ToList();

            if (!characterList.Any())
            {
                player.Emit("MdcPersonSearchNoResult");
                return;
            }

            List<PersonResult> results = new List<PersonResult>();

            foreach (Models.Character character in characterList)
            {
                if (character == null) continue;

                Console.WriteLine($"Searching for: {character.Name}");

                string gender = "Male";

                if (!string.IsNullOrEmpty(character.CustomCharacter))
                {
                    CustomCharacter customCharacter =
                        JsonConvert.DeserializeObject<CustomCharacter>(character.CustomCharacter);

                    if (customCharacter == null)
                    {
                        gender = character.Sex == 0 ? "Male" : "Female";
                    }
                    else
                    {
                        gender = customCharacter.Gender == 0 ? "Male" : "Female";
                    }
                }
                else
                {
                    gender = character.Sex == 0 ? "Male" : "Female";
                }

                results.Add(new PersonResult(character.Name, character.Age, gender, character.Id));
            }

            string json = JsonConvert.SerializeObject(results);

            player.SetData("MdcPersonSearchResult", json);
            player.Emit("MdcPersonSearchResult", json);
        }

        /// <summary>
        /// Result when player hits the profile button for a MDC person search
        /// </summary>
        /// <param name="player"></param>
        /// <param name="index"></param>
        public static void MdcSearchResultProfileSelected(IPlayer player, string index)
        {
            bool tryParse = int.TryParse(index, out int i);

            if (!tryParse)
            {
                player.SendErrorNotification("An error occurred fetching data.");
                return;
            }

            player.GetData("MdcPersonSearchResult", out string jsonData);

            List<PersonResult> personResults = JsonConvert.DeserializeObject<List<PersonResult>>(jsonData);

            PersonResult selectedPersonResult = personResults[i];

            Models.Character targetCharacter = Models.Character.GetCharacter(selectedPersonResult.CharacterId);

            bool driversLicense = JsonConvert.DeserializeObject<List<LicenseTypes>>(targetCharacter.LicensesHeld)
                .Contains(LicenseTypes.Driving);

            bool pistolLicense = JsonConvert.DeserializeObject<List<LicenseTypes>>(targetCharacter.LicensesHeld)
                .Contains(LicenseTypes.Pistol);

            List<Models.Property> targetProperties = Models.Property.FetchCharacterProperties(targetCharacter).Where(x => !x.Hidden).ToList();

            List<string> propertyAddress = new List<string>();

            if (targetProperties.Any())
            {
                foreach (var targetProperty in targetProperties)
                {
                    propertyAddress.Add(targetProperty.Address);
                }
            }

            MdtProfile mdtProfile = new MdtProfile(selectedPersonResult.Name, selectedPersonResult.Age.ToString(), selectedPersonResult.Gender, driversLicense, propertyAddress, Models.Vehicle.FetchCharacterVehicles(targetCharacter.Id), pistolLicense);

            player.Emit("MdcPersonProfileInformation", JsonConvert.SerializeObject(mdtProfile));
        }

        public static void OnProfileSelectedProperty(IPlayer player, string propertyAddress)
        {
            using Context context = new Context();

            Models.Property selectedProperty = context.Property.FirstOrDefault(x => x.Address == propertyAddress);

            

            if (selectedProperty == null)
            {
                player.SendErrorNotification("Unable to find this property.");
                return;
            }

            Position propertyPosition = new Position(selectedProperty.PosX, selectedProperty.PosY, selectedProperty.PosZ);

            player.SetWaypoint(propertyPosition);

            player.SendInfoNotification($"Navigating you to {selectedProperty.Address}.");
        }

        public static async void OnSetPatrolUnit(IPlayer player, string unit)
        {
            if (string.IsNullOrEmpty(unit)) return;

            player.SetData(_unitData, unit);

            player.SendNotification($"~g~Radio Channel Set: {unit}.");

        }

        public static void OnMdcPlateSearch(IPlayer player, string plate)
        {
            if (string.IsNullOrEmpty(plate)) return;

            using Context context = new Context();

            List<Models.Vehicle> vehicleList = context.Vehicle.Where(x => x.Plate.ToLower().Contains(plate.ToLower())).ToList();

            if (!vehicleList.Any())
            {
                player.Emit("MdcVehicleSearchNoResult");
                return;
            }

            List<VehicleResult> resultList = new List<VehicleResult>();

            foreach (Models.Vehicle vehicle in vehicleList)
            {
                resultList.Add(new VehicleResult(vehicle.Plate, Models.Character.GetCharacter(vehicle.OwnerId).Name,
                    vehicle.Name, vehicle.Id));
            }

            string resultJson = JsonConvert.SerializeObject(resultList);

            player.SetData("mdc:vehicleSearchResult", resultJson);

            player.Emit("mdc:vehicleSearchResult", resultJson);
        }

        public static void OnRespond911(IPlayer player, int index)
        {
            Call911 respondingCall = CallList[index];

            if (respondingCall == null) return;
            
            player.GetData(_unitData, out string unit);
            //FactionCommands.FactionCommandRadio(player, $"{unit} responding to the 911 at {respondingCall.Location}.");
        }
    }
}