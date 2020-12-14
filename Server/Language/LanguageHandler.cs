using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using AltV.Net.Data;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Server.Objects;
using Yandex.Translator;

namespace Server.Language
{
    public class LanguageHandler
    {
        public static readonly Position LearnPosition = new Position(-1636.378f, 180.75165f, 61.74951f);

        public static List<Language> Languages = new List<Language>();

        public static void InitLanguages()
        {
            Languages.Add(new Language("English", "en"));
            Languages.Add(new Language("Belarusian", "be"));
            Languages.Add(new Language("Dutch", "nl"));
            Languages.Add(new Language("Danish", "da"));
            Languages.Add(new Language("Italian", "it"));
            Languages.Add(new Language("Spanish", "es"));
            Languages.Add(new Language("Chinese", "zh"));
            Languages.Add(new Language("German", "de"));
            Languages.Add(new Language("Russian", "ru"));
            Languages.Add(new Language("French", "fr"));
            Languages.Add(new Language("Japanese", "ja"));
            Languages.Add(new Language("Korean", "ko"));
            Languages.Add(new Language("Polish", "pl"));
            Languages.Add(new Language("Arabic", "ar"));
            Languages.Add(new Language("Persian", "fa"));
            Languages.Add(new Language("Urdu", "ur"));
            Languages.Add(new Language("Afrikaans", "af"));

            Blip positionBlip = new Blip("ULSA", LearnPosition, 133, 58, 0.75f);
            positionBlip.Add();

            TextLabel positionLabel = new TextLabel("ULSA Language School\n/learn", LearnPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.MediumPurple));
            positionLabel.Add();
        }

        public static ITranslation FetchTranslation(Language toLanguage, string text)
        {
            try
            {
                string apiKey = Release.Default.YandexApi;

                IYandexTranslator translator = Yandex.Translator.Yandex.Translator(api =>
                {
                    api.ApiKey(apiKey).Format(ApiDataFormat.Json);
                });

                ITranslation translation = translator.Translate(toLanguage.Code, text);

                return translation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}