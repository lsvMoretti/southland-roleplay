using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Server.Models
{
    public class AdminRecord
    {
        /// <summary>
        /// Unique ID of field
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Account ID of player
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Character ID of player
        /// </summary>
        public int CharacterId { get; set; }

        /// <summary>
        /// What type of record is it
        /// </summary>
        public AdminRecordType RecordType { get; set; }

        /// <summary>
        /// Date time it happened
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Reason
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// If Jail or Ban, how long for
        /// Jail - Minutes, Ban - Hours
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// The admin username that completed the action
        /// </summary>
        public string Admin { get; set; }

        public static List<AdminRecord> FetchAdminRecords(int AccountId)
        {
            using (Context context = new Context())
            {
                return context.AdminRecords.Where(x => x.AccountId == AccountId).ToList();
            }
        }
    }

    public enum AdminRecordType
    {
        Kick,
        Ban,
        Jail,
        Warning,
    }
}