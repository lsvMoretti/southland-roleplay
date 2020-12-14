namespace ClothingConverter
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Collections.Generic;
    using System.Globalization;

    public partial class GtaClothing
    {
        [JsonProperty("GXT")]
        public string Gxt { get; set; }

        [JsonProperty("Localized")]
        public string Localized { get; set; }
    }

    public partial class GtaClothing
    {
        public static Dictionary<string, Dictionary<string, GtaClothing>> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, GtaClothing>>>(json, ClothingConverter.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Dictionary<string, Dictionary<string, GtaClothing>> self) => JsonConvert.SerializeObject(self, ClothingConverter.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    public partial class BestTorso
    {
        [JsonProperty("BestTorsoDrawable")]
        public long BestTorsoDrawable { get; set; }

        [JsonProperty("BestTorsoTexture")]
        public long BestTorsoTexture { get; set; }
    }

    public partial class BestTorso
    {
        public static Dictionary<string, Dictionary<string, BestTorso>> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, BestTorso>>>(json, Converter.Settings);
    }
}