using System;
using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Elements.Entities;

namespace Server.Voice
{
    public class VoiceHandler
    {
        private static Dictionary<int, IVoiceChannel> _voiceChannels = new Dictionary<int, IVoiceChannel>();

        /// <summary>
        /// Creates a new voice channel and adds it to the VoiceChannels dictionary.
        /// </summary>
        /// <param name="id">Id of the channel</param>
        /// <param name="spatial"></param>
        /// <param name="range"></param>
        public static IVoiceChannel CreateVoiceChannel(int id, bool spatial, float range)
        {
            try
            {
                if (_voiceChannels.ContainsKey(id))
                {
                    Console.WriteLine("_voiceChannels contains key");
                    return null;
                }

                IVoiceChannel newChannel = Alt.CreateVoiceChannel(spatial, range);

                _voiceChannels.Add(id, newChannel);

                return newChannel;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Fetches the voice channel by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IVoiceChannel FetchVoiceChannel(int id)
        {
            bool hasChannel = _voiceChannels.TryGetValue(id, out IVoiceChannel channel);

            return hasChannel ? channel : null;
        }
    }
}