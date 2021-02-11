using System.Collections.Generic;

namespace Server.Radio
{
    public class RadioItem
    {
        public List<RadioChannelItem> RadioChannels { get; set; }

        public RadioItem()
        {
            
        }

        public RadioItem(List<RadioChannelItem> radioChannels)
        {
            RadioChannels = radioChannels;
        }
    }
}