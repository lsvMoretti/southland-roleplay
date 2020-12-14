using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Language
{
    public class LanguageCommands
    {
        private static readonly double _languagePrice = 150.00;

        [Command("learn", commandType: CommandType.Character, description: "Language: Used to learn a Language.")]
        public static void LearnLanguage(IPlayer player)

        {
            if (!player.IsSpawned()) return;

            if (player.Position.Distance(LanguageHandler.LearnPosition) > 3)
            {
                player.SendNotification("~r~You must be at the ULSA Language Building!");
                return;
            }

            if (player.GetClass().Cash < _languagePrice)
            {
                player.SendNotification($"~r~You don't have the funds. You require ~g~{_languagePrice:C}.");
                return;
            }

            List<Language> playerLanguages =
                JsonConvert.DeserializeObject<List<Language>>(player.FetchCharacter().Languages);

            if (playerLanguages.Count >= player.FetchCharacter().MaxLanguages)
            {
                player.SendNotification("~r~You've reached your max amount of languages!");
                return;
            }

            List<Language> serverLanguages = LanguageHandler.Languages;

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Language serverLanguage in serverLanguages)
            {
                if (playerLanguages.Contains(serverLanguage)) continue;

                menuItems.Add(new NativeMenuItem(serverLanguage.LanguageName));
            }

            if (!menuItems.Any())
            {
                player.SendNotification($"~r~There are no languages for you to learn!");
                return;
            }

            NativeMenu menu = new NativeMenu("Languages:LearnLanguage", "Languages", "Learn a Language!", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnLearnLanguageSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            Language selectedLanguage = LanguageHandler.Languages.FirstOrDefault(x => x.LanguageName == option);

            if (selectedLanguage == null)
            {
                player.SendErrorNotification("An error occurred fetching the language data.");
                return;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            List<Language> playerLanguages = JsonConvert.DeserializeObject<List<Language>>(playerCharacter.Languages);

            if (playerLanguages.Contains(selectedLanguage))
            {
                player.SendNotification("~r~You've already learned this language!");
                return;
            }

            playerLanguages.Add(selectedLanguage);

            if (playerLanguages.Count > playerCharacter.MaxLanguages)
            {
                player.SendNotification("~r~You've reached your max amount of languages.");
                return;
            }

            playerCharacter.Languages = JsonConvert.SerializeObject(playerLanguages);

            player.GetClass().SpokenLanguages = playerLanguages;

            context.SaveChanges();

            

            player.RemoveCash(_languagePrice);

            player.SendNotification($"~g~You've learned the {selectedLanguage.LanguageName} language. This has cost {_languagePrice:C}.");
        }

        [Command("language", commandType: CommandType.Character, description: "Language: Used to select a language")]
        public static void LanguageCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            List<Language> playerLanguages =
                JsonConvert.DeserializeObject<List<Language>>(playerCharacter.Languages);

            Language currentLanguage = JsonConvert.DeserializeObject<Language>(playerCharacter.CurrentLanguage);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Language playerLanguage in playerLanguages)
            {
                if (playerLanguage == currentLanguage) continue;

                menuItems.Add(new NativeMenuItem(playerLanguage.LanguageName));
            }

            if (!menuItems.Any())
            {
                player.SendNotification("~r~There are no other languages you know!");
                return;
            }

            NativeMenu menu = new NativeMenu("Languages:SelectLanguage", "Language", "Select your spoken language.", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectSpokenLanguage(IPlayer player, string option)
        {
            if (option == "Close") return;

            Language selectedLanguage = LanguageHandler.Languages.FirstOrDefault(x => x.LanguageName == option);

            if (selectedLanguage == null)
            {
                player.SendErrorNotification("There was an error fetching the language data.");
                return;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            playerCharacter.CurrentLanguage = JsonConvert.SerializeObject(selectedLanguage);

            context.SaveChanges();
            

            player.SendNotification($"~y~You've set your language to {selectedLanguage.LanguageName}.");

            player.GetClass().SpokenLanguage = selectedLanguage;
        }
    }
}