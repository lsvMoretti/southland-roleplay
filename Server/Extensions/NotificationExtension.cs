using AltV.Net.Elements.Entities;

namespace Server.Extensions
{
    public static class NotificationExtension
    {
        public static void SendInfoNotification(this IPlayer player, string message, int timeout = 6000)
        {
            player.Emit("SendNotification", message, timeout, "warning", "topCenter");
        }

        public static void SendErrorNotification(this IPlayer player, string message, int timeout = 6000)
        {
            player.Emit("SendNotification", message, timeout, "error", "topCenter");
        }
    }
}