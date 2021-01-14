using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;

namespace Server.Models
{
    public class Character
    {
        /// <summary>
        /// Unique Character Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Account Id of Owner
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Character Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// PosX (From Vector3)
        /// Default 0
        /// </summary>
        public float PosX { get; set; }

        /// <summary>
        /// PosY (From Vector3)
        /// Default 0
        /// </summary>
        public float PosY { get; set; }

        /// <summary>
        /// PosZ (From Vector3)
        /// Default 0
        /// </summary>
        public float PosZ { get; set; }

        /// <summary>
        /// Rotation (From Vector3)
        /// Default 0
        /// </summary>
        public float RotZ { get; set; }

        /// <summary>
        /// Player Dimension
        /// Default 0
        /// </summary>
        public int Dimension { get; set; }

        /// <summary>
        /// Player Cash on Hand
        /// Default 100
        /// </summary>
        public float Money { get; set; }

        /// <summary>
        /// Player Sex (0 Male, 1 Female)
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// Player Age
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Inventory Id
        /// Default 0
        /// </summary>
        public int InventoryID { get; set; }

        /// <summary>
        /// Total Hours Online
        /// Default 0
        /// </summary>
        public int TotalHours { get; set; }

        /// <summary>
        /// Total Minutes Online
        /// Default 0
        /// </summary>
        public int TotalMinutes { get; set; }

        /// <summary>
        /// Last Payday Time Check
        /// </summary>
        public DateTime LastTimeCheck { get; set; }

        /// <summary>
        /// Payday Account
        /// Default 0
        /// </summary>
        public long PaydayAccount { get; set; }

        /// <summary>
        /// The Amount of money to receive next payday.
        /// Default 0
        /// </summary>
        public int PaydayAmount { get; set; }

        /// <summary>
        /// string?'d list of PlayerFaction
        /// Default []
        /// </summary>
        public string? FactionList { get; set; }

        /// <summary>
        /// The current active faction Id
        /// Default 0
        /// </summary>
        public int ActiveFaction { get; set; }

        /// <summary>
        /// Json of Clothes ( List<clothesData>() )
        /// Default []
        /// </summary>
        public string? ClothesJson { get; set; }

        /// <summary>
        /// Json of Accessories (List<accessoryData>() )
        /// Default []
        /// </summary>
        [DefaultValue("[]")]
        public string? AccessoryJson { get; set; }

        /// <summary>
        /// The choice of the player's lifestyle
        /// Default 0 (To be selected in the new character page)
        /// </summary>
        public LifestyleChoice LifestyleChoice { get; set; }

        /// <summary>
        /// The house voucher for the player
        /// Default 1 / true
        /// </summary>
        public bool HouseVoucher { get; set; }

        /// <summary>
        /// Vehicle Voucher
        /// Default 1 / true
        /// </summary>
        public bool VehicleVoucher { get; set; }

        /// <summary>
        /// The amount of times a voucher is used to purchase a house
        /// Default 0
        /// </summary>
        public int HouseVoucherPurchases { get; set; }

        /// <summary>
        /// Is a player Anon (Masked)
        /// Default 0 / false
        /// </summary>
        public bool Anon { get; set; }

        /// <summary>
        /// JSON of Tattoo's
        /// Default []
        /// </summary>
        public string? TattooJson { get; set; }

        /// <summary>
        /// List of LicenseTypes enum
        /// Default []
        /// </summary>
        public string? LicensesHeld { get; set; }

        public int InsideProperty { get; set; }

        /// <summary>
        /// Which Apartment Complex they're inside of
        /// Default 0
        /// </summary>
        public int InsideApartmentComplex { get; set; }

        /// <summary>
        /// What Apartment Name they're inside of
        /// Default null or empty string?
        /// </summary>
        public string? InsideApartment { get; set; }

        /// <summary>
        /// The Active Phone Number for a Character
        /// Default null or empty
        /// </summary>
        public string? ActivePhoneNumber { get; set; }

        /// <summary>
        /// List of Jobs (Jobs enum)
        /// Default []
        /// </summary>
        public string? JobList { get; set; }

        /// <summary>
        /// The Bio of the Character from Creation
        /// Bio entered from Character Creation
        /// </summary>
        public string? Bio { get; set; }

        /// <summary>
        /// Has the Bio been past the Discord Approval
        /// 0 Denied, 1 Pending, 2 Accepted
        /// </summary>
        public int BioStatus { get; set; }

        /// <summary>
        /// Set when a player goes on / off duty
        /// Default 0 / False
        /// </summary>
        public bool FactionDuty { get; set; }

        /// <summary>
        /// Json of List<FocusTypes>
        /// Default []
        /// </summary>
        public string? FocusJson { get; set; }

        /// <summary>
        /// Sets if player is in garage
        /// Default 0
        /// </summary>
        public int InsideGarage { get; set; }

        /// <summary>
        /// If a player is in Jail
        /// Default 0 / False
        /// </summary>
        public bool InJail { get; set; }

        /// <summary>
        /// Minutes in Jail
        /// Default 0
        /// </summary>
        public int JailMinutes { get; set; }

        /// <summary>
        /// Character Description
        /// Default empty
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Custom Character
        /// Default []
        /// </summary>
        public string? CustomCharacter { get; set; }

        /// <summary>
        /// Start stage of the character.
        /// 0 - Character not made, 1 - Character customized, 2 - tutorial finished
        /// 3 - Started Welcome Player Interaction, heading to the DMV stage
        /// 4 - During Welcome Player Interaction, finished DMV stage
        /// </summary>
        public int StartStage { get; set; }

        /// <summary>
        /// Current Weapon Hash
        /// </summary>
        public string? CurrentWeapon { get; set; }

        /// <summary>
        /// Graffiti Clean Count in Hour
        /// </summary>
        public int GraffitiCleanCount { get; set; }

        /// <summary>
        /// Next time able to Graffiti
        /// </summary>
        public DateTime NextGraffitiTime { get; set; }

        /// <summary>
        /// Duty Status (PD/FD clothing setting)
        /// </summary>

        public int DutyStatus { get; set; }

        /// <summary>
        ///  Is renting motel room
        /// </summary>
        public bool RentingMotelRoom { get; set; }

        /// <summary>
        /// Inside Motel Id
        /// </summary>
        public int InMotel { get; set; }

        /// <summary>
        /// Inside Motel Room Id
        /// </summary>
        public int InMotelRoom { get; set; }

        /// <summary>
        /// Storage of Walk Styles
        /// </summary>
        public int WalkStyle { get; set; }

        /// <summary>
        /// Current rent vehicle key
        /// </summary>
        public string? RentVehicleKey { get; set; }

        /// <summary>
        /// List of Languages
        /// </summary>
        public string? Languages { get; set; }

        /// <summary>
        /// The max amount of Languages a player can learn
        /// </summary>
        public int MaxLanguages { get; set; }

        /// <summary>
        /// Current Language
        /// </summary>
        public string? CurrentLanguage { get; set; }

        /// <summary>
        /// Current Backpack Id
        /// </summary>
        public int BackpackId { get; set; }

        /// <summary>
        /// List of player outfits
        /// </summary>
        public string? Outfits { get; set; }

        /// <summary>
        /// Creates a Character
        /// </summary>
        /// <param name="character"></param>
        /// <returns>character.Id (Unique Id)</returns>
        public static int CreateCharacter(Character character)
        {
            using Context context = new Context();
            context.Character.Add(character);
            context.SaveChanges();

            return character.Id;
        }

        /// <summary>
        /// Fetches Character by Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Character DB</returns>
        public static Character GetCharacter(string? name)
        {
            using Context context = new Context();
            return context.Character.FirstOrDefault(i => i.Name == name);
        }

        /// <summary>
        /// Fetches Character by Character Id
        /// </summary>
        /// <param name="id">Character Id</param>
        /// <returns>Character DB</returns>
        public static Character GetCharacter(int id)
        {
            using Context context = new Context();
            Character character = context.Character.Find(id);
            return character;
        }

        /// <summary>
        /// Fetches list of Character Names by Account
        /// </summary>
        /// <param name="account">Account DB</param>
        /// <returns>List of Character Names (string? List)</returns>
        public static List<string?> FetchCharacterNames(Account account)
        {
            using Context context = new Context();
            List<Character> characters = context.Character.Where(i => i.OwnerId == account.Id).ToList();

            List<string?> characterList = new List<string?>();
            foreach (Character character in characters)
            {
                characterList.Add(character.Name);
            }

            return characterList;
        }

        /// <summary>
        /// Fetches a list of Characters by Account
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static List<Character> FetchCharacters(Account account)
        {
            using Context context = new Context();
            return context.Character.Where(i => i.OwnerId == account.Id).ToList();
        }
    }
}