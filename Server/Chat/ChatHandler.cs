using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Language;

namespace Server.Chat
{
    public class ChatHandler
    {
        public const string ColorChatClose = "{E6E6E6}";
        public const string ColorChatNear = "{C8C8C8}";
        public const string ColorChatMedium = "{AAAAAA}";
        public const string ColorChatFar = "{8C8C8C}";
        public const string ColorChatLimit = "{6E6E6E}";
        public const string ColorChatMe = "{C2A2DA}";
        public const string ColorChatDo = "{0F9622}";
        public const string ColorOocClose = "{A0A0A0}";
        public const string ColorOocNear = "{A0A0A0}";
        public const string ColorOocMedium = "{A0A0A0}";
        public const string ColorOocFar = "{A0A0A0}";
        public const string ColorOocLimit = "{A0A0A0}";
        public const string ColorAdminInfo = "{00FCFF}";
        public const string ColorAdminNews = "{F93131}";
        public const string ColorAdminChat = "{F93131}";
        public const string ColorSuccess = "{33B517}";
        public const string ColorError = "{A80707}";
        public const string ColorInfo = "{FDFE8B}";
        public const string ColorHelp = "{FFFFFF}";
        public const string ColorWhite = "{ffffff}";
        public const string ColorPM = "{FFCC00}";
        public const string ColorTaxi = "{FFFF00}";
        public const string ColorWeapons = "{3399ff}";
        public const string ColorFactionMessage = "{e8a20d}";
        public const string ColorStatMessage = "{2a7ff7}";
        public const string ColorRadioMessage = "{FF7E00}";

        public const int ChatLength = 85;
        public const int ChatRanges = 5;

        public static readonly float ChatRange = 12f;
        public static float EmoteRange = 12f;
        public static readonly float WhisperRange = 2f;
        public static readonly float ShoutRange = 24f;

        private static string GetChatMessageColor(float distance, float distanceGap)
        {
            string color = null;
            if (distance < distanceGap)
            {
                color = ColorChatClose;
            }
            else if (distance < distanceGap * 2)
            {
                color = ColorChatNear;
            }
            else if (distance < distanceGap * 3)
            {
                color = ColorChatMedium;
            }
            else if (distance < distanceGap * 4)
            {
                color = ColorChatFar;
            }
            else
            {
                color = ColorChatLimit;
            }

            return color;
        }

        private static string GetOocMessageColor(float distance, float distanceGap)
        {
            string color = null;
            if (distance < distanceGap)
            {
                color = ColorOocClose;
            }
            else if (distance < distanceGap * 2)
            {
                color = ColorOocNear;
            }
            else if (distance < distanceGap * 3)
            {
                color = ColorOocMedium;
            }
            else if (distance < distanceGap * 4)
            {
                color = ColorOocFar;
            }
            else
            {
                color = ColorOocLimit;
            }

            return color;
        }

        public static async void SendMessageToNearbyPlayers(IPlayer player, string message, MessageType type, float range = 10,
            bool excludePlayer = false)
        {
            string secondMessage = string.Empty;

            range = type switch
            {
                MessageType.Talk => 7.5f,
                MessageType.Shout => 28f,
                MessageType.Whisper => 0.8f,
                MessageType.Me => 8f,
                MessageType.Do => 8f,
                MessageType.Ooc => 5.5f,
                MessageType.My => 8f,
                MessageType.Low => 3f,
                MessageType.DoLow => 3f,
                _ => range
            };

            float distanceGap = range / ChatRanges;

            /*
            if (message.Length > ChatLength)
            {
                // We need two lines to show the message
                secondMessage = message.Substring(ChatLength, message.Length - ChatLength);
                message = message.Remove(ChatLength, secondMessage.Length);
            }
            */

            Language.Language playerLanguage = player.GetClass().SpokenLanguage;

            string translatedText = null;

            if (playerLanguage.Code != "en")
            {
                translatedText = await LanguageHandler.FetchTranslation(playerLanguage, message);
                if (translatedText == null)
                {
                    player.SendErrorNotification("An error occurred translating.");
                    return;
                }
            }
            if (type == MessageType.VehicleWindowShut)
            {
                IVehicle playerVehicle = player.Vehicle;

                if (playerVehicle == null) return;

                foreach (IPlayer occupant in Alt.Server.GetPlayers().Where(x => x.Vehicle == playerVehicle))
                {
                    if (occupant == player && excludePlayer) continue;

                    List<Language.Language> targetLanguages = occupant.GetClass().SpokenLanguages;

                    if (playerLanguage.Code == "en")
                    {
                        occupant.SendChatMessage($"{ColorChatClose}{player.GetClass().Name} says: {message}");
                    }
                    else
                    {
                        bool hasLanguage = targetLanguages.Any(x => x.Code == playerLanguage.Code);

                        occupant.SendChatMessage(hasLanguage
                            ? $"{ColorChatClose}{player.GetClass().Name} says in {playerLanguage.LanguageName}: {message}"
                            : $"{ColorChatClose}{player.GetClass().Name} says in {playerLanguage.LanguageName}: {translatedText}");
                    }
                }

                return;
            }

            if (player.Dimension != 0 && type == MessageType.Talk)
            {
                range = 5.5f;
            }

            foreach (IPlayer target in Alt.Server.GetPlayers())
            {
                if (!target.IsSpawned()) continue;

                List<Language.Language> targetLanguages = target.GetClass().SpokenLanguages;

                if (target.FetchCharacter() != null && player.Dimension == target.Dimension)
                {
                    if (player != target || (player == target && !excludePlayer))
                    {
                        float distance = player.Position.Distance(target.Position);

                        if (distance <= range)
                        {
                            // Getting message color
                            string chatMessageColor = GetChatMessageColor(distance, distanceGap);
                            string oocMessageColor = GetOocMessageColor(distance, distanceGap);

                            switch (type)
                            {
                                case MessageType.Talk:
                                    // We send the message

                                    if (playerLanguage.Code == "en")
                                    {
                                        target.SendChatMessage($"{chatMessageColor}{player.GetClass().Name} says: {message}");
                                    }
                                    else
                                    {
                                        bool hasLanguage = targetLanguages.Any(x => x.Code == playerLanguage.Code);

                                        target.SendChatMessage(hasLanguage
                                            ? $"{chatMessageColor}{player.GetClass().Name} says in {playerLanguage.LanguageName}: {message}"
                                            : $"{chatMessageColor}{player.GetClass().Name} says in {playerLanguage.LanguageName}: {translatedText}");
                                    }

                                    break;

                                case MessageType.Shout:
                                    // We send the message

                                    if (playerLanguage.Code == "en")
                                    {
                                        target.SendChatMessage($"{chatMessageColor}{player.GetClass().Name} shouts: {message}");
                                    }
                                    else
                                    {
                                        bool hasLanguage = targetLanguages.Any(x => x.Code == playerLanguage.Code);

                                        target.SendChatMessage(hasLanguage
                                            ? $"{chatMessageColor}{player.GetClass().Name} shouts in {playerLanguage.LanguageName}: {message}"
                                            : $"{chatMessageColor}{player.GetClass().Name} shouts in {playerLanguage.LanguageName}: {translatedText}");
                                    }

                                    break;

                                case MessageType.Whisper:
                                    // We send the message

                                    if (playerLanguage.Code == "en")
                                    {
                                        target.SendChatMessage($"{ColorChatClose}{player.GetClass().Name} whispers: {message}");
                                    }
                                    else
                                    {
                                        bool hasLanguage = targetLanguages.Any(x => x.Code == playerLanguage.Code);

                                        target.SendChatMessage(hasLanguage
                                            ? $"{ColorChatClose}{player.GetClass().Name} whispers in {playerLanguage.LanguageName}: {message}"
                                            : $"{ColorChatClose}{player.GetClass().Name} whispers in {playerLanguage.LanguageName}: {translatedText}");
                                    }

                                    break;

                                case MessageType.Low:
                                    // We send the message

                                    if (playerLanguage.Code == "en")
                                    {
                                        target.SendChatMessage($"{ColorChatClose}{player.GetClass().Name} says in a low voice: {message}");
                                    }
                                    else
                                    {
                                        bool hasLanguage = targetLanguages.Any(x => x.Code == playerLanguage.Code);

                                        target.SendChatMessage(hasLanguage
                                            ? $"{ColorChatClose}{player.GetClass().Name} says in a low voice in {playerLanguage.LanguageName}: {message}"
                                            : $"{ColorChatClose}{player.GetClass().Name} says in a low voice in {playerLanguage.LanguageName}: {translatedText}");
                                    }

                                    break;

                                case MessageType.Me:
                                    // We send the message
                                    target.SendChatMessage($"* {ColorChatMe}{player.GetClass().Name} {message}");

                                    break;

                                case MessageType.My:
                                    // We send the message
                                    target.SendChatMessage($"* {ColorChatMe}{player.GetClass().Name}'s {message}");

                                    break;

                                case MessageType.Do:
                                    // We send the message
                                    target.SendChatMessage($"* {ColorChatMe}{message} (( {player.GetClass().Name} ))");

                                    break;

                                case MessageType.DoLow:
                                    // We send the message
                                    target.SendChatMessage($"* {ColorChatMe}{message} (( {player.GetClass().Name} ))");

                                    break;

                                case MessageType.Ooc:
                                    // We send the message
                                    target.SendChatMessage($"{oocMessageColor}(( {player.GetClass().Name} (ID:{player.GetPlayerId()}): {message} ))");

                                    break;

                                case MessageType.LocalPhone:
                                    // We send the message

                                    if (playerLanguage.Code == "en")
                                    {
                                        target.SendChatMessage(target == player
                                            ? $"{ChatHandler.ColorChatNear}[Phone] {ChatHandler.ColorWhite}{player.GetClass().Name} says: {message}"
                                            : $"{chatMessageColor}{player.GetClass().Name} says: {message}");
                                    }
                                    else
                                    {
                                        bool hasLanguage = targetLanguages.Any(x => x.Code == playerLanguage.Code);

                                        if (hasLanguage)
                                        {
                                            target.SendChatMessage(target == player
                                                ? $"{ChatHandler.ColorChatNear}[Phone] {ChatHandler.ColorWhite}{player.GetClass().Name} says in {playerLanguage.LanguageName}: {message}"
                                                : $"{chatMessageColor}{player.GetClass().Name} says in {playerLanguage.LanguageName}: {message}");
                                        }
                                        else
                                        {
                                            target.SendChatMessage(target == player
                                                ? $"{ChatHandler.ColorChatNear}[Phone] {ChatHandler.ColorWhite}{player.GetClass().Name} says in {playerLanguage.LanguageName}: {message}"
                                                : $"{chatMessageColor}{player.GetClass().Name} says in {playerLanguage.LanguageName}: {translatedText}");
                                        }
                                    }

                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}