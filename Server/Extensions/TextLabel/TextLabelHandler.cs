using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Elements.Entities;
using EntityStreamer;
using Newtonsoft.Json;

namespace Server.Extensions.TextLabel
{
    public class TextLabelHandler
    {
        public static List<TextLabel> TextLabels = new List<TextLabel>();

        /// <summary>
        /// Loads all existing MarijuanaLabels on Spawn
        /// </summary>
        /// <param name="player"></param>
        public static void LoadTextLabelsOnSpawn(IPlayer player)
        {
            RemoveAllTextLabelsForPlayer(player);
            foreach (TextLabel textLabel in TextLabels)
            {
                LoadTextLabelForPlayer(player, textLabel);
            }
        }

        /// <summary>
        /// Loads TextLabel for Player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="textLabel"></param>
        public static void LoadTextLabelForPlayer(IPlayer player, TextLabel textLabel)
        {
            player.Emit("createTextLabel", JsonConvert.SerializeObject(textLabel));
        }

        /// <summary>
        /// Removes a single text label for a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="textLabel"></param>
        public static void RemoveTextLabelForPlayer(IPlayer player, TextLabel textLabel)
        {
            player.Emit("deleteTextLabel", JsonConvert.SerializeObject(textLabel));
        }

        /// <summary>
        /// Removes all text labels for a player
        /// </summary>
        /// <param name="player"></param>
        public static void RemoveAllTextLabelsForPlayer(IPlayer player)
        {
            player.Emit("deleteAllTextLabels");
        }

        /// <summary>
        /// When a TextLabel is added to the Text Label List
        /// </summary>
        /// <param name="textLabel"></param>
        public static void OnTextLabelAdded(TextLabel textLabel)
        {
            foreach (IPlayer player in Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null))
            {
                LoadTextLabelForPlayer(player, textLabel);
            }
        }

        /// <summary>
        /// When a TextLabel is removed from the Label List
        /// </summary>
        /// <param name="textLabel"></param>
        public static void OnTextLabelRemoved(TextLabel textLabel)
        {
            foreach (IPlayer player in Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null))
            {
                RemoveAllTextLabelsForPlayer(player);

                foreach (TextLabel label in TextLabels)
                {
                    RemoveTextLabelForPlayer(player, label);
                }
            }
        }
    }
}