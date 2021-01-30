using System;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Server.Extensions;

namespace Server.Drug
{
    public class UseDrug
    {
        public static void UseWeedItem(IPlayer player)
        {
            player.Emit("StartScreenEvent", "ChopVision", 0, true);
            player.SetData("WEEDTIMEEND", DateTime.Now.AddMinutes(5));
            player.SetData("WEEDENDED", false);
            Logging.AddToCharacterLog(player, $"has used a drug. Marijuana");
        }

        public static void UseCocaineItem(IPlayer player)
        {
            player.Emit("StartScreenEvent", "DefaultFlash", 0, true);
            player.SetData("COCAINEENDTIME", DateTime.Now.AddMinutes(2));
            player.SetData("COCAINEENDED", false);
            Logging.AddToCharacterLog(player, $"has used a drug. Cocaine");
        }

        public static void UseMethItem(IPlayer player)
        {
            player.Emit("StartScreenEvent", "DMT_flight_intro", 0, true);
            player.SetData("METHENDTIME", DateTime.Now.AddMinutes(5));
            player.SetData("METHENDED", false);
            Logging.AddToCharacterLog(player, $"has used a drug. Meth");
        }

        public static void UseMushroomItem(IPlayer player)
        {
            player.Emit("StartScreenEvent", "DMT_flight_intro", 0, true);
            player.SetData("MUSHROOMENDTIME", DateTime.Now.AddMinutes(5));
            player.SetData("MUSHROOMENDED", false);
            Logging.AddToCharacterLog(player, $"has used a drug. Mushroom");
        }

        public static void UseHeroinItem(IPlayer player)
        {
            player.Emit("StartScreenEvent", "DrugsTrevorClownsFight", 0, true);
            player.SetData("HEROINENDTIME", DateTime.Now.AddMinutes(5));
            player.SetData("HEROINENDED", false);
            Logging.AddToCharacterLog(player, $"has used a drug. Heroin");
        }

        public static void UseEcstasyItem(IPlayer player)
        {
            player.Emit("StartScreenEvent", "BeastLaunch", 0, true);
            player.SetData("ECSTASYENDTIME", DateTime.Now.AddMinutes(2));
            player.SetData("ECTASYENDED", false);
            Logging.AddToCharacterLog(player, $"has used a drug. Ecstasy");
        }

        public static void InitTick()
        {
            Timer tickTimer = new Timer(1000) { AutoReset = true, Enabled = true };

            tickTimer.Elapsed += (sender, args) =>
            {
                tickTimer.Stop();
                foreach (IPlayer player in Alt.GetAllPlayers().Where(x => x.IsSpawned()))
                {
                    #region Marijuana

                    bool hasWeedTimeData = player.GetData("WEEDTIMEEND", out DateTime weedEndTime);

                    if (hasWeedTimeData)
                    {
                        player.GetData("WEEDENDED", out bool weedEnded);

                        if (!weedEnded)
                        {
                            if (DateTime.Compare(DateTime.Now, weedEndTime) > 0)
                            {
                                player.SetData("WEEDENDED", true);
                                player.Emit("StopScreenEvent");

                            }
                        }
                    }

                    #endregion Marijuana

                    #region Cocaine

                    bool hasCocaineTimeData = player.GetData("COCAINEENDTIME", out DateTime cocaineTime);

                    if (hasCocaineTimeData)
                    {
                        player.GetData("COCAINEENDED", out bool cocaineEnded);

                        if (!cocaineEnded)
                        {
                            if (DateTime.Compare(DateTime.Now, cocaineTime) > 0)
                            {
                                player.SetData("COCAINEENDED", true);
                                player.Emit("StopScreenEvent");
                            }
                        }
                    }

                    #endregion Cocaine

                    #region Meth

                    bool hasMethTimeData = player.GetData("METHENDTIME", out DateTime methTime);

                    if (hasMethTimeData)
                    {
                        player.GetData("METHENDED", out bool methEnded);

                        if (!methEnded)
                        {
                            if (DateTime.Compare(DateTime.Now, methTime) > 0)
                            {
                                player.SetData("METHENDED", true);
                                player.Emit("StopScreenEvent");
                            }
                        }
                    }

                    #endregion Meth

                    #region Mushrooms

                    bool hasMushroomEndTime = player.GetData("MUSHROOMENDTIME", out DateTime mushroomTime);

                    if (hasMushroomEndTime)
                    {
                        player.GetData("MUSHROOMENDED", out bool mushroomEnded);

                        if (!mushroomEnded)
                        {
                            if (DateTime.Compare(DateTime.Now, mushroomTime) > 0)
                            {
                                player.SetData("MUSHROOMENDED", true);
                                player.Emit("StopScreenEvent");
                            }
                        }
                    }

                    #endregion Mushrooms

                    #region Heroin

                    bool hasHeroinTime = player.GetData("HEROINENDTIME", out DateTime heroinTime);

                    if (hasHeroinTime)
                    {
                        player.GetData("HEROINENDED", out bool heroinEnded);

                        if (!heroinEnded)
                        {
                            if (DateTime.Compare(DateTime.Now, heroinTime) > 0)
                            {
                                player.SetData("HEROINENDED", true);
                                player.Emit("StopScreenEvent");
                            }
                        }
                    }

                    #endregion Heroin

                    #region Ecstasy

                    bool hasEcstasyTime = player.GetData("ECSTASYENDTIME", out DateTime ecstasyTime);

                    if (hasEcstasyTime)
                    {
                        player.GetData("ECTASYENDED", out bool ectasyEnded);

                        if (!ectasyEnded)
                        {
                            if (DateTime.Compare(DateTime.Now, ecstasyTime) > 0)
                            {
                                player.SetData("ECTASYENDED", true);
                                player.Emit("StopScreenEvent");
                            }
                        }
                    }

                    #endregion Ecstasy
                }

                tickTimer.Start();
            };
        }
    }
}