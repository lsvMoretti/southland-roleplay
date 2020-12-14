namespace Server.Objects
{
    public class RadioStation
    {
        public string StationName { get; set; }
        public string StationUrl { get; set; }

        public RadioStation(string stationName, string stationUrl)
        {
            StationName = stationName;
            StationUrl = stationUrl;
        }
    }
}