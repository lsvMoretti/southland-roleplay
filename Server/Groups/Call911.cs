namespace Server.Groups
{
    public class Call911
    {
        public string Caller { get; set; }
        public string Number { get; set; }
        public string CallInformation { get; set; }
        public string Location { get; set; }

        /// <summary>
        /// A new 911 Call Object
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="number"></param>
        /// <param name="callInformation"></param>
        /// <param name="location"></param>
        public Call911(string caller, string number, string callInformation, string location)
        {
            Caller = caller;
            Number = number;
            CallInformation = callInformation;
            Location = location;
        }
    }
}