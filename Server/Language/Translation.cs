using System.Collections.Generic;
using Newtonsoft.Json;

namespace Server.Language
{
    public class Translations
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }
    }

    public class Translation
    {
        [JsonProperty("translations")]
        public List<Translations> Translations { get; set; }
    }
}