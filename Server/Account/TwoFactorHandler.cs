using System;
using System.IO;
using AltV.Net.Elements.Entities;
using Microsoft.AspNetCore.Connections;
using Server.Extensions;
using TwoFactorAuthNet;
using ConnectionHandler = Server.Connection.ConnectionHandler;

namespace Server.Account
{
    public class TwoFactorHandler
    {
        public static void SetupTwoFactorForPlayer(IPlayer player)
        {
            Models.Account playerAccount = player.FetchAccount();

            TwoFactorAuth tfa = new TwoFactorAuth("Southland Roleplay Game Server");

            var secretCode = tfa.CreateSecret(160);

            string qrCodeImageUrl = tfa.GetQrCodeImageAsDataUri("Southland Roleplay Game Server", secretCode);

            player.SetData("TFA:secretCode", secretCode);

            player.Emit("TFA:ShowWindow", secretCode, qrCodeImageUrl);
            player.FreezeCam(true);
            player.FreezeInput(true);
        }

        public static void OnTwoFactorSetupClose(IPlayer player)
        {
            player.FreezeCam(false);
            player.FreezeInput(false);
        }

        public static void OnTwoFactorSetupComplete(IPlayer player)
        {
            player.FreezeCam(false);
            player.FreezeInput(false);
            using Context context = new Context();

            Models.Account playerAccount = context.Account.Find(player.GetClass().AccountId);

            if (playerAccount == null)
            {
                player.SendErrorNotification("There was an error fetching your account information.");
                return;
            }

            player.GetData("TFA:secretCode", out string secretCode);

            playerAccount.Enable2FA = true;
            playerAccount.TwoFactorUserCode = secretCode;

            context.SaveChanges();

            player.SendInfoNotification($"Your two factor has been setup!");
        }

        public static void OnRequestTwoFactor(IPlayer player, string input)
        {
            Models.Account playerAccount = player.FetchAccount();

            TwoFactorAuth tfa = new TwoFactorAuth("Southland Roleplay Game Server");

            bool valid = tfa.VerifyCode(playerAccount.TwoFactorUserCode, input);
            if (!valid)
            {
                player.SendErrorNotification("The code was not accepted!");
                player.Emit("2FA:InvalidCode");
                return;
            }

            player.Emit("2FA:CloseInput");
            player.SendInfoNotification("Your 2FA code has been accepted!");
            player.FreezeCam(false);
            player.FreezeInput(false);
            ConnectionHandler.CompleteLogin(player);
        }
    }
}