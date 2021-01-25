using System;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Donations
    {
        [Key] public int Id { get; set; }
        public int? AccountId { get; set; }
        public DonationType? Type { get; set; }
        public bool? Activated { get; set; }
        public DateTime? DateTime { get; set; }
    }

    public enum DonationType
    {
        Bronze,
        Silver,
        Gold
    }
}