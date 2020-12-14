using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public enum LifestyleChoice
    {
        Homeless,

        [Display(Name = "Very Low")]
        VeryLow,

        Low,
        Medium,
        High,
        Luxury
    }
}