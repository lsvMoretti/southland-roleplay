namespace Server.Extensions.Marker
{
    public static class MarkerStaticExtension
    {
        public static void Add(this Marker marker)
        {
            MarkerHandler.MarkerLabels.Add(marker);
            MarkerHandler.OnMarkerAdded(marker);
        }

        public static void Remove(this Marker marker)
        {
            MarkerHandler.MarkerLabels.Remove(marker);
            MarkerHandler.OnMarkerRemoved(marker);
        }
    }
}