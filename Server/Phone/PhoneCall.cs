using System;

namespace Server.Phone
{
    public class PhoneCall
    {
        public string PhoneNumber { get; set; }
        public PhoneCallState CallType { get; set; }
        public DateTime Time { get; set; }
    }
}