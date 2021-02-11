namespace Server.Radio
{
    public class RadioChannelItem
    {
        public int Channel { get; set; }
        public int Slot { get; set; }

        public RadioChannelItem(int channel, int slot)
        {
            Channel = channel;
            Slot = slot;
        }

        public RadioChannelItem()
        {
            
        }
    }
}