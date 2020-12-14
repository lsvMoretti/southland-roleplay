using System;
using Server.Models;

namespace Server.Phone
{
    public class PhoneMessage
    {
        public string PhoneNumber { get; set; }
        public string MessageText { get; set; }
        public DateTime Time { get; set; }
        public PhoneMessageState SentRecieved { get; set; }
    }
}