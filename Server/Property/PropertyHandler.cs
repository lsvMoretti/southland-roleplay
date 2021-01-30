using System;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Elasticsearch.Net;
using Server.Audio;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Property
{
    public class PropertyHandler
    {
        #region Voucher Values

        public static readonly int HomelessVoucher = 0;
        public static readonly int VLowVoucher = 25000;
        public static readonly int LowVoucher = 75000;
        public static readonly int MedVoucher = 200000;
        public static readonly int HighVoucher = 400000;
        public static readonly int LuxuryVoucher = 1000000;

        #endregion Voucher Values

        public static int FetchVoucherValue(LifestyleChoice lifestyleChoice)
        {
            return lifestyleChoice switch
            {
                LifestyleChoice.Homeless => HomelessVoucher,
                LifestyleChoice.VeryLow => VLowVoucher,
                LifestyleChoice.Low => LowVoucher,
                LifestyleChoice.Medium => MedVoucher,
                LifestyleChoice.High => HighVoucher,
                LifestyleChoice.Luxury => LuxuryVoucher,
                _ => 0
            };
        }

        public static void ReloadPropertyRadio(Models.Property property)
        {
            foreach (IPlayer player in Alt.GetAllPlayers().Where(x => x.Dimension == property.Id).ToList())
            {
                player.StopMusic();
                SetPropertyRadioForPlayer(player, property);
            }
        }

        public static void SetPropertyRadioForPlayer(IPlayer player, Models.Property property)
        {
            if (string.IsNullOrEmpty(property.MusicStation)) return;

            player.PlayMusicFromUrl(property.MusicStation);
        }
    }
}