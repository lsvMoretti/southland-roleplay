using System.ComponentModel;

namespace Server.Drug
{
    public class DrugBag
    {
        public static readonly double SmallBagLimit = 7.0;
        public static readonly double LargeBagLimit = 28.0;

        public DrugType DrugType { get; set; }
        public double DrugQuantity { get; set; }

        public DrugBag()
        {
        }

        public DrugBag(DrugType drugType, double quantity)
        {
            DrugType = drugType;
            DrugQuantity = quantity;
        }
    }

    public enum DrugType
    {
        [Description("Empty")]
        Empty,

        [Description("Psilocybin Mushroom")]
        Mushroom,

        [Description("Marijuana")]
        Weed,

        [Description("Heroin")]
        Heroin,

        [Description("Crystal Meth")]
        Meth,

        [Description("Cocaine")]
        Cocaine,

        [Description("Ecstasy")]
        Ecstasy
    }

    public enum DrugBagType
    {
        ZipLockSmall,
        ZipLockLarge
    }
}