using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Account
    {
        /// <summary>
        /// Unique User Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Accounts Username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Accounts Email Address
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Accounts Hashed Password
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Last Serial logged in with
        /// </summary>
        public string? LastSerial { get; set; }

        /// <summary>
        /// Last Social Club logged in with
        /// </summary>
        public string? LastSocial { get; set; }

        /// <summary>
        /// Last address of player
        /// </summary>
        public string? LastIp { get; set; }

        /// <summary>
        /// The Date & Time of Registration
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Administrator level
        /// </summary>
        public AdminLevel AdminLevel { get; set; }

        /// <summary>
        /// Is Player Banned
        /// </summary>
        public bool Banned { get; set; }

        /// <summary>
        /// UnBan Time
        /// </summary>
        public DateTime UnBanTime { get; set; }

        /// <summary>
        /// Last Character Id
        /// </summary>
        public int LastCharacter { get; set; }

        /// <summary>
        /// Is player in jail
        /// </summary>
        public bool InJail { get; set; }

        /// <summary>
        /// Release time for player
        /// </summary>
        public int JailMinutes { get; set; }

        public int TeamspeakDatabaseId { get; set; }

        /// <summary>
        /// Is the player a tester
        /// </summary>
        public bool Tester { get; set; }

        /// <summary>
        /// Amount of AFK kicks
        /// </summary>
        public int AfkKicks { get; set; }

        /// <summary>
        /// Returns if the player is logged in
        /// </summary>
        public bool IsOnline { get; set; }

        public bool Developer { get; set; }

        public string? DiscordId { get; set; }

        public bool Enable2FA { get; set; }

        public string? TwoFactorUserCode { get; set; }

        public string? HardwareIdHash { get; set; }

        public string? HardwareIdExHash { get; set; }

        public bool AutoLogin { get; set; }

        public bool Helper { get; set; }

        public DonationLevel DonatorLevel { get; set; }

        public int AcceptedReports { get; set; }
        public int AcceptedHelps { get; set; }

        public static Account FindAccountById(int id)
        {
            using Context context = new Context();
            return context.Account.Find(id);
        }

        /// <summary>
        /// Adds an Account to the Database and returns the Key (Unique Id)
        /// </summary>
        /// <param name="account">Account</param>
        /// <returns>int (Unique Id)</returns>
        public static int AddAccount(Account account)
        {
            using Context context = new Context();
            Account acc = context.Account.Add(account).Entity;
            context.SaveChanges();

            return acc.Id;
        }
    }

    public enum DonationLevel
    {
        [Description("None")]
        None,

        [Description("Bronze")]
        Bronze,

        [Description("Silver")]
        Silver,

        [Description("Gold")]
        Gold
    }

    public enum AdminLevel
    {
        [Description("None")]
        None,

        [Description("Tester")]
        Tester,

        [Description("Administrator")]
        Administrator,

        [Description("Head Administrator")]
        HeadAdmin,

        [Description("Management")]
        Management,

        [Description("Director")]
        Director
    }
}