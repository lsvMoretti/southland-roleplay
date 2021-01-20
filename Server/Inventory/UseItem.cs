using System;
using System.Globalization;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Drug;
using Server.Extensions;

namespace Server.Inventory
{
    public class UseItem
    {
        public static void UseItemAttribute(IPlayer player, InventoryItem invitem)
        {
            Inventory playerInventory = player.FetchInventory();

            player.GetData("SELECTEDINVITEM", out int itemValue);

            InventoryItem item = playerInventory.GetInventory()[itemValue];

            if (item.ItemInfo.ID == "ITEM_CLOTHES")
            {
                player.SendErrorNotification("Use /clothes instead.");
                return;
            }

            if (item.ItemInfo.ID == "ITEM_WEAPON")
            {
                player.SendErrorNotification("Weapons can only be equipped!");
                return;
            }

            if (item.ItemInfo.ID == "ITEM_SANDWICH")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"eats {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_DRUG_XANAX")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendInfoNotification($"You have taken Xanax.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMALLCONDOM")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"Has opened a Small Condom packet.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_MEDIUMCONDOM")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"Has opened a Medium Condom packet.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_LARGECONDOM")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"Has opened a Large Condom packet.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_XLCONDOM")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"Has opened a XL Condom packet.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_WATERBOTTLE")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_RAINEWATER")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_RIVIERAWATER")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CLOUDWATER")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_TIKIWATER")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CLEVERWATER")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CUPCOFFEE")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CUPLATTE")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CUPMOCKA")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CUPAMERICANO")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CUPESPRESSO")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CUPHOTCHOCO")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CUPMACCHIATO")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_DOUGHNUT")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"eats {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SPRUNK" || item.ItemInfo.ID == "ITEM_ECOLA" || item.ItemInfo.ID == "ITEM_LIGHTSPRUNK" || item.ItemInfo.ID == "ITEM_ECOLALIGHT" || item.ItemInfo.ID == "ITEM_ORANGOTANG" || item.ItemInfo.ID == "ITEM_DIETORANGOTANG")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_PHATCHIPS_PAPRIKA" || item.ItemInfo.ID == "ITEM_PHATCHIPS_CHEESE" || item.ItemInfo.ID == "ITEM_PHATCHIPS_HABANERO" || item.ItemInfo.ID == "ITEM_PHATCHIPS_STICKYRIB" || item.ItemInfo.ID == "ITEM_PHATCHIPS_SALTSAUCE" || item.ItemInfo.ID == "ITEM_PHATCHIPS_SUPERSALT" || item.ItemInfo.ID == "ITEM_PHATCHIPS_SOURCREAM" || item.ItemInfo.ID == "ITEM_PHATCHIPS_BLACKLIQUORICE" || item.ItemInfo.ID == "ITEM_PHATCHIPS_HONEYMUSTARD" || item.ItemInfo.ID == "ITEM_PHATCHIPS_KETCHUP" || item.ItemInfo.ID == "ITEM_PHATCHIPS_CHEESE" || item.ItemInfo.ID == "ITEM_PHATCHIPS_CHIPOTLE" || item.ItemInfo.ID == "ITEM_PHATCHIPS_BAVARIAN" || item.ItemInfo.ID == "ITEM_PHATCHIPS_CHEESE" || item.ItemInfo.ID == "ITEM_PHATCHIPS_GREEKGREENS" || item.ItemInfo.ID == "ITEM_PHATCHIPS_SALMONRANCHER")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"eats a pack of {item.ItemInfo.Description.ToString(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_RELEASEPINK" || item.ItemInfo.ID == "ITEM_RELEASEBLUE" || item.ItemInfo.ID == "ITEM_RELEASEGREEN")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts chewing a piece of {item.ItemInfo.Name.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_MILK")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_EGOCHASER")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating an {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SWEETNOTHINGS")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a pack of {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_CARAMEL")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_WHITE")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_DARK")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_MINT")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_ZEBRABAR")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_CAPTAINSLOG")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_METEORITE")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_EARTHQUAKES")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating an {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_CANDYBOX_PSQS")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts eating a pack of {item.ItemInfo.Description.ToString(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_PISSWASSER_BEER")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"drinks from {item.ItemInfo.Description.ToLower(CultureInfo.CurrentCulture)}");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMOKING_69BRAND")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts smoking a 69 Brand Cigar.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMOKING_CARDIAQUE")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts smoking a Cardiaque Cigarette.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMOKING_DEBONAIRE")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts smoking a Debonaire Cigarette.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMOKING_ATHENA200S")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts smoking a Athena 200's Cigarette.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMOKING_CANCER")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts smoking a Cancer Cigarette.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMOKING_HOMIESSHARP")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts smoking a Homies Sharp Cigarette.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMOKING_ESTANCIA")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts smoking a Estancia Cigar.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMOKING_REDWOOD")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts smoking a Redwood Cigarette.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_SMOKING_YUKON")
            {
                bool success = playerInventory.RemoveItem(item);
                if (success)
                {
                    player.SendEmoteMessage($"starts chewing on a piece of Yukon Tabacco.");
                    return;
                }
            }

            if (item.ItemInfo.ID == "ITEM_DRUG_WEED")
            {
                player.SendErrorNotification("Please use /drugs.");
                return;
                /*
                playerInventory.RemoveItem(item, 1);
                UseDrug.UseWeedItem(player);
                return;*/
            }

            if (item.ItemInfo.ID == "ITEM_DRUG_COCAINE")
            {
                player.SendErrorNotification("Please use /drugs.");
                return;/*
                playerInventory.RemoveItem(item, 1);
                UseDrug.UseCocaineItem(player);
                return;*/
            }

            if (item.ItemInfo.ID == "ITEM_DRUG_METH")
            {
                player.SendErrorNotification("Please use /drugs.");
                return;
                /*
                playerInventory.RemoveItem(item, 1);
                UseDrug.UseMethItem(player);
                return;*/
            }

            if (item.ItemInfo.ID == "ITEM_DRUG_HEROIN")
            {
                player.SendErrorNotification("Please use /drugs.");
                return;
                /*
                playerInventory.RemoveItem(item, 1);
                UseDrug.UseHeroinItem(player);
                return;*/
            }

            if (item.ItemInfo.ID == "ITEM_DRUG_MUSHROOM")
            {
                player.SendErrorNotification("Please use /drugs.");
                return;
                /*
                playerInventory.RemoveItem(item, 1);
                UseDrug.UseMushroomItem(player);
                return;*/
            }

            if (item.ItemInfo.ID == "ITEM_DRUG_ECSTASY")
            {
                player.SendErrorNotification("Please use /drugs.");
                return;
                /*
                playerInventory.RemoveItem(item, 1);
                UseDrug.UseEcstasyItem(player);
                return;*/
            }

            if (item.ItemInfo.ID == "ITEM_DRUG_ZIPLOCK_BAG_SMALL")
            {
                player.SendErrorNotification("Please use /drugs.");
                return;
            }

            if (item.ItemInfo.ID == "ITEM_DRUG_ZIPLOCK_BAG_LARGE")
            {
                player.SendErrorNotification("Please use /drugs.");
                return;
            }

            if (item.ItemInfo.ID == "ITEM_BEEFY_BURGER")
            {
                playerInventory.RemoveItem(item);
                player.SendEmoteMessage($"eats a messy Beefy Bills Burger.");
                return;
            }

            if (item.ItemInfo.ID == "ITEM_HOTDOG")
            {
                playerInventory.RemoveItem(item);
                player.SendEmoteMessage($"eats a Hotdog that's been covered in sauce.");
                return;
            }

            player.SendErrorNotification("An error occurred. #ER29");
            return;
        }
    }
}