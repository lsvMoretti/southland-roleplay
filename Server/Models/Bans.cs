using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using AltV.Net.Elements.Entities;

namespace Server.Models
{
    public class Bans
    {
        [Key]
        public int Id { get; set; }

        public string IpAddress { get; set; }

        public string SocialClubId { get; set; }

        public string HardwareIdHash { get; set; }

        public string HardwareIdExHash { get; set; }



        public Bans(string ipAddress, string socialClubId, string hardwareIdHash, string hardwareIdExHash)
        {
            IpAddress = ipAddress;
            SocialClubId = socialClubId;
            HardwareIdHash = hardwareIdHash;
            HardwareIdExHash = hardwareIdExHash;
        }

        public static bool IsAccountBanned(IPlayer player)
        {
            using Context context = new Context();

            Bans ban = context.Bans.FirstOrDefault(x => x.IpAddress == player.Ip);

            if (ban != null)return true;

            ban = context.Bans.FirstOrDefault(x => x.SocialClubId == player.SocialClubId.ToString());

            if (ban != null)return true;

            ban = context.Bans.FirstOrDefault(x => x.HardwareIdHash == player.HardwareIdHash.ToString());

            if (ban != null) return true;

            ban = context.Bans.FirstOrDefault(x => x.HardwareIdExHash == player.HardwareIdExHash.ToString());

            if (ban != null)  return true;

            return false;
        }
    }
}