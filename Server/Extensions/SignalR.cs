using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Entities;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Server.Admin;
using Server.Chat;
using Server.Discord;

namespace Server.Extensions
{
    public class SignalR
    {
        private static HubConnection hubConnection;

        public static async void StartConnection()
        {
            string url = @"http://149.202.88.222:2000/signalr";

            hubConnection = new HubConnectionBuilder().WithUrl(url).Build();

            Connect(hubConnection);

            hubConnection.Closed += HubConnectionOnClosed;

            await hubConnection.InvokeAsync("AddToUserGroup", "AltVServer", null);

            #region Usergroup Callback

            hubConnection.On<string>("AddedToUserGroup",
                (usergroup) => { Console.WriteLine($"[SignalR] Added to Usergroup: {usergroup}."); });

            #endregion Usergroup Callback

            #region Fetch Online Players

            hubConnection.On<string>("FetchOnlinePlayers", (connectionId) =>
            {
                List<string> onlinePlayerList = new List<string>();

#if RELEASE
                foreach (IPlayer player in Alt.GetAllPlayers().Where(x => x.FetchCharacter() != null))
                {
                    onlinePlayerList.Add(player.GetClass().Name);
                }
#endif

                hubConnection.InvokeAsync("SendOnlinePlayerList", connectionId,
                    JsonConvert.SerializeObject(onlinePlayerList));
            });

            #endregion Fetch Online Players

            #region Fetch Game Time

            /*
            hubConnection.On<string>("AltVGameTime", (connectionId) =>
            {
                hubConnection.InvokeAsync("SendGameTime", connectionId, Settings.ServerSettings.Hour, Settings.ServerSettings.Minute);
            });
            */

            #endregion Fetch Game Time

            #region SendMessageToPlayer

            hubConnection.On<string, string>("AltVSendMessageToPlayer", (playerName, message) =>
            {
                var client = Utility.FindPlayerByNameOrId(playerName);

                client?.SendChatMessage(message);
            });

            #endregion SendMessageToPlayer

            hubConnection.On<string, string>("AltVSendIgAdminMessage", DiscordHandler.OnReceiveAdminMessage);

            hubConnection.On<int, string>("SendMessageToReport", AdminHandler.SendMessageToReport);

            hubConnection.On<int>("CloseReport", AdminHandler.CloseReport);
        }

        public static void SendGameTime()
        {
            hubConnection.InvokeAsync("SendGameTime", Settings.ServerSettings.Hour, Settings.ServerSettings.Minute);
        }

        private static async Task HubConnectionOnClosed(Exception arg)
        {
            Console.WriteLine("[SignalR] Hub Connection Closed. Reconnecting.");

            bool reconnected = false;

            reconnected = Connect(hubConnection);

            while (!reconnected)
            {
                reconnected = Connect(hubConnection);
            }

            if (reconnected)
            {
                await hubConnection.InvokeAsync("AddToUserGroup", "AltVServer", null);

                Console.WriteLine("[SignalR] Hub Reconnected.");
            }
        }

        private static bool Connect(HubConnection connection)
        {
            try
            {
                bool hubConnected = false;

                hubConnection.StartAsync().Wait();

                while (!hubConnected)
                {
                    if (hubConnection.State == HubConnectionState.Connected)
                    {
                        hubConnected = true;
                    }
                }

                return hubConnected;
            }
            catch
            {
                Console.WriteLine($"[SignalR] Unable to connect to hub. Retrying..");
                return false;
            }
        }

        /// <summary>
        /// Sends a message to the Admin Control Panel. Includes the DateTime format
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToACP(string message)
        {
            hubConnection.InvokeAsync("SendMessageToACP", message);
        }

        public static void SendDiscordMessage(ulong channelId, string message)
        {
            hubConnection.InvokeAsync("SendDiscordMessage", channelId.ToString(), message);
        }

        public static void SendDiscordEmbed(ulong channelId, DiscordEmbed embed)
        {
            hubConnection.InvokeAsync("SendDiscordEmbed", channelId.ToString(), JsonConvert.SerializeObject(embed));
        }

        public static void SendPlayerCount()
        {
            hubConnection.InvokeAsync("ReceivePlayerCount", Alt.GetAllPlayers().Count());
        }

        public static void SendUserLogin(Models.Account playerAccount)
        {
            hubConnection.InvokeAsync("OnUserLogin", playerAccount.Id, playerAccount.Username);
        }

        public static void SendUserLogout(Models.Account playerAccount)
        {
            hubConnection.InvokeAsync("OnUserLogout", playerAccount.Id, playerAccount.Username);
        }

        public static void SendDiscordUserMessage(string userId, string message)
        {
            hubConnection.InvokeAsync("SendDiscordUserMessage", userId, message);
        }

        public static void AddReport(AdminReportObject report)
        {
            hubConnection.InvokeAsync("SendGameReport", JsonConvert.SerializeObject(report));
        }

        public static void RemoveReport(AdminReportObject report)
        {
            hubConnection.InvokeAsync("RemoveGameReport", JsonConvert.SerializeObject(report));
        }

        public static void SendReportMessage(int reportId, string message)
        {
            hubConnection.InvokeAsync("SendDiscordReportMessage", reportId, message);
        }

        public static async void SendScreenShotToDiscord(string discordId, string image)
        {
            await hubConnection.InvokeAsync("SendScreenshotToDiscordUser", discordId, image);
        }
    }
}