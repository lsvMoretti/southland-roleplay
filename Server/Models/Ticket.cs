using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    [Table("tickets")]
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        public int OwnerId { get; set; }

        public int OfficerId { get; set; }
        public string OfficerName { get; set; }
        public string Reason { get; set; }
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
        public bool Paid { get; set; }

        /// <summary>
        /// Creates new ticket object
        /// </summary>
        /// <param name="ownerId">Character Id</param>
        /// <param name="officerId">Officer Character Id</param>
        /// <param name="officerName">Officer Name</param>
        /// <param name="reason">Reason</param>
        /// <param name="amount">Amount</param>
        public Ticket(int ownerId, int officerId, string officerName, string reason, double amount)
        {
            OwnerId = ownerId;
            OfficerId = officerId;
            OfficerName = officerName;
            Reason = reason;
            Amount = amount;
            DateTime = DateTime.Now;
            Paid = false;
        }
    }
}