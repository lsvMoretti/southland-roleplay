using System.Collections.Generic;
using EntityStreamer;
using Newtonsoft.Json;

namespace Server.Map
{
    public class Map
    {
        public string MapName { get; set; }
        public bool IsInterior { get; set; }
        public string Interior { get; set; }
        public List<MapObject> MapObjects { get; set; }

        [JsonIgnore]
        public List<Prop> LoadedObjects { get; set; }

        public Map()
        {
            LoadedObjects = new List<Prop>();
        }
    }
}