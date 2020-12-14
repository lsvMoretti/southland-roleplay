namespace Server.Extensions.Blip
{
    public static class BlipStaticExtension
    {
        public static void Add(this Objects.Blip blip)
        {
            BlipHandler.BlipList.Add(blip);
            BlipHandler.OnBlipAdded(blip);
        }

        public static void Remove(this Objects.Blip blip)
        {
            BlipHandler.BlipList.Remove(blip);
            BlipHandler.OnBlipRemoved(blip);
        }
    }
}