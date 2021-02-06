using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Data;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Server.Objects;

namespace Server.Language
{
    public class LanguageHandler
    {
        public static readonly Position LearnPosition = new Position(-1636.378f, 180.75165f, 61.74951f);

        public static List<Language> Languages = new List<Language>();

        public static void InitLanguages()
        {
            Languages.Add(new Language("English", "en"));
            Languages.Add(new Language("Dutch", "nl"));
            Languages.Add(new Language("Danish", "da"));
            Languages.Add(new Language("Italian", "it"));
            Languages.Add(new Language("Spanish", "es"));
            Languages.Add(new Language("Chinese", "zh-Hans"));
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
            Languages.Add(new Language("Croatian", "hr"));
            Languages.Add(new Language("Turkish", "tr"));
            Languages.Add(new Language("Bulgarian", "bg"));
            Languages.Add(new Language("Greek", "el"));
            Languages.Add(new Language("Serbian (Cyrillic)", "sr-Cyrl"));
            Languages.Add(new Language("Serbian (Latin)", "sr-Latn"));
            Languages.Add(new Language("Romanian", "ro"));

            Blip positionBlip = new Blip("ULSA", LearnPosition, 133, 58, 0.75f);
            positionBlip.Add();

            TextLabel positionLabel = new TextLabel("ULSA Language School\n/learn", LearnPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.MediumPurple));
            positionLabel.Add();
        }

        public static async Task<Translations> FetchTranslation(Language toLanguage, string textToTranslate)
        {
            try
            {
                string COGNITIVE_SERVICES_KEY = Release.Default.TranslationKeyOne;

                string COGNITIVE_SERVICES_REGION = "northeurope";

                string TEXT_TRANSLATION_API_ENDPOINT = "https://api.cognitive.microsofttranslator.com/";

                string route = $"/translate?api-version=3.0&from=en&to={toLanguage.Code}";

                object[] body = new object[] { new { Text = textToTranslate } };

                var requestBody = JsonConvert.SerializeObject(body);

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    // Build the request.
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(TEXT_TRANSLATION_API_ENDPOINT + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", COGNITIVE_SERVICES_KEY);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", COGNITIVE_SERVICES_REGION);

                    // Send the request and get response.
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    string trim = result.Trim('[', ']');
                    Translations translation =
                        JsonConvert.DeserializeObject<Translations>(trim);

                    return translation;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}