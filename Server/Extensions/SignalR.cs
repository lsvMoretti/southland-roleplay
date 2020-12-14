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
            string url = @"http://176.9.66.40:2000/signalr";

            hubConnection = new HubConnectionBuilder().WithUrl(url).Build();

            await Connect(hubConnection);

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
                foreach (IPlayer player in Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null))
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

            reconnected = await Connect(hubConnection);

            while (!reconnected)
            {
                reconnected = await Connect(hubConnection);
            }

            if (reconnected)
            {
                await hubConnection.InvokeAsync("AddToUserGroup", "AltVServer", null);

                Console.WriteLine("[SignalR] Hub Reconnected.");
            }
        }

        private static async Task<bool> Connect(HubConnection connection)
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
            catch (Exception e)
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
#if RELEASE
            hubConnection.InvokeAsync("SendMessageToACP", message);
#endif
        }

        public static void SendDiscordMessage(ulong channelId, string message)
        {
#if RELEASE
            hubConnection.InvokeAsync("SendDiscordMessage", channelId.ToString(), message);
#endif
        }

        public static void SendDiscordEmbed(ulong channelId, DiscordEmbed embed)
        {
#if RELEASE
            hubConnection.InvokeAsync("SendDiscordEmbed", channelId.ToString(), JsonConvert.SerializeObject(embed));
#endif
        }

        public static void SendPlayerCount()
        {
#if RELEASE
            hubConnection.InvokeAsync("ReceivePlayerCount", Alt.Server.GetPlayers().Count());
#endif
        }

        public static void SendUserLogin(Models.Account playerAccount)
        {
#if RELEASE
            hubConnection.InvokeAsync("OnUserLogin", playerAccount.Id, playerAccount.Username);
#endif
        }

        public static void SendUserLogout(Models.Account playerAccount)
        {
#if RELEASE

            hubConnection.InvokeAsync("OnUserLogout", playerAccount.Id, playerAccount.Username);
#endif
        }

        public static void SendDiscordUserMessage(string userId, string message)
        {
#if RELEASE
            hubConnection.InvokeAsync("SendDiscordUserMessage", userId, message);
#endif
        }

        public static void AddReport(AdminReportObject report)
        {
#if RELEASE
            hubConnection.InvokeAsync("SendGameReport", JsonConvert.SerializeObject(report));
#endif
        }

        public static void RemoveReport(AdminReportObject report)
        {
#if RELEASE
            hubConnection.InvokeAsync("RemoveGameReport", JsonConvert.SerializeObject(report));
#endif
        }

        public static void SendReportMessage(int reportId, string message)
        {
#if RELEASE
            hubConnection.InvokeAsync("SendDiscordReportMessage", reportId, message);
#endif
        }
    }
}