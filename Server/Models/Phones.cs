using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Phone;

namespace Server.Models
{
    public class Phones
    {
        /// <summary>
        /// Unique ID for the Phone
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Phone Number relating to phone (Formatted like 213-1234567)
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Status of the Phones power
        /// </summary>
        public bool TurnedOn { get; set; }

        /// <summary>
        /// JSON of PhoneCall history (List)
        /// </summary>
        public string? CallHistory { get; set; }

        /// <summary>
        /// JSON of PhoneMessage history (list)
        /// </summary>
        public string? MessageHistory { get; set; }

        /// <summary>
        /// JSON of PhoneContact history (list)
        /// </summary>
        public string? ContactList { get; set; }

        /// <summary>
        /// Current Character ID that the phone is on
        /// </summary>
        public int CharacterId { get; set; }

        /// <summary>
        /// Fetches the Phone Data by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Phones FetchPhone(int id)
        {
            using (Context context = new Context())
            {
                return context.Phones.Find(id);
            }
        }

        /// <summary>
        /// Fetches the Phone Data by Number
        /// </summary>
        /// <param name="PhoneNumber"></param>
        /// <returns></returns>
        public static Phones FetchPhone(string PhoneNumber)
        {
            using (Context context = new Context())
            {
                return context.Phones.FirstOrDefault(pData => pData.PhoneNumber == PhoneNumber);
            }
        }

        /// <summary>
        /// Fetch a Phones Message history
        /// </summary>
        /// <param name="phoneData"></param>
        /// <returns></returns>
        public static List<PhoneMessage> FetchMessageHistory(Phones phoneData)
        {
            return JsonConvert.DeserializeObject<List<PhoneMessage>>(phoneData.MessageHistory);
        }

        /// <summary>
        /// Fetch a Phones Call history
        /// </summary>
        /// <param name="phoneData"></param>
        /// <returns></returns>
        public static List<PhoneCall> FetchCallHistory(Phones phoneData)
        {
            return JsonConvert.DeserializeObject<List<PhoneCall>>(phoneData.CallHistory);
        }

        /// <summary>
        /// Fetch a Phone's Contact list
        /// </summary>
        /// <param name="phoneData"></param>
        /// <returns></returns>
        public static List<PhoneContact> FetchContacts(Phones phoneData)
        {
            return JsonConvert.DeserializeObject<List<PhoneContact>>(phoneData.ContactList);
        }

        public static Phones CreatePhone(int characterId)
        {
            string newNumber = Utility.GenerateRandomNumber(7);
            string number = $"213-{newNumber}";

            if (FetchPhone(number) != null)
            {
                newNumber = Utility.GenerateRandomNumber(7);
                number = $"213-{newNumber}";
            }

            if (FetchPhone(number) != null)
            {
                return null;
            }

            Phones newPhone = new Phones
            {
                PhoneNumber = number,
                TurnedOn = true,
                CallHistory = JsonConvert.SerializeObject(new List<PhoneCall>()),
                MessageHistory = JsonConvert.SerializeObject(new List<PhoneMessage>()),
                ContactList = JsonConvert.SerializeObject(new List<PhoneContact>()),
                CharacterId = characterId
            };

            using (Context context = new Context())
            {
                context.Phones.Add(newPhone);
                context.SaveChanges();

                return newPhone;
            }
        }
    }
}