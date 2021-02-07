using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Animation
{
    public class Commands
    {
        [Command("stopanim", alternatives: "sa", commandType: CommandType.Anim, description: "Stops an animation")]
        public static void AnimCommandStopAnimation(IPlayer player)
        {
            if (!player.IsInAnimation())
            {
                player.SendErrorNotification("You're not in an Animation!");
                return;
            }

            Handler.StopPlayerAnimation(player);
        }

        [Command("medic", commandType: CommandType.Anim, description: "/medic [1-3]")]
        public static void AnimCommandMedic(IPlayer player, string numberString = "")
        {
            if (!Handler.CanAnim(player, true)) return;
            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/medic [1-3]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayScenario(player, "CODE_HUMAN_MEDIC_KNEEL");
                    break;

                case 2:
                    Handler.PlayScenario(player, "CODE_HUMAN_MEDIC_TEND_TO_DEAD");
                    break;

                case 3:
                    Handler.PlayScenario(player, "CODE_HUMAN_MEDIC_TIME_OF_DEATH");
                    break;

                default:
                    player.SendSyntaxMessage("/medic [1-3]");
                    break;
            }
        }

        [Command("cop", commandType: CommandType.Anim, description: "/cop [1-8]")]
        public static void AnimCommandCop(IPlayer player, string numberString = "")
        {
            if (!Handler.CanAnim(player, true)) return;
            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/cop [1-8]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayScenario(player, "WORLD_HUMAN_COP_IDLES");
                    break;

                case 2:
                    Handler.PlayScenario(player, "WORLD_HUMAN_GUARD_PATROL");
                    break;

                case 3:
                    Handler.PlayScenario(player, "WORLD_HUMAN_GUARD_STAND");
                    break;

                case 4:
                    Handler.PlayScenario(player, "WORLD_HUMAN_GUARD_STAND_CASINO");
                    break;

                case 5:
                    Handler.PlayScenario(player, "WORLD_HUMAN_GUARD_STAND_CLUBHOUSE");
                    break;

                case 6:
                    Handler.PlayScenario(player, "WORLD_HUMAN_GUARD_STAND_FACILITY");
                    break;

                case 7:
                    Handler.PlayScenario(player, "CODE_HUMAN_POLICE_CROWD_CONTROL");
                    break;

                case 8:
                    Handler.PlayScenario(player, "CODE_HUMAN_POLICE_INVESTIGATE");
                    break;

                default:
                    player.SendSyntaxMessage("/cop [1-8]");
                    break;
            }
        }

        [Command("surrender", commandType: CommandType.Anim, description: "/surrender")]
        public static void AnimCommandSurrender(IPlayer player)
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            player.Emit("Animation:Surrender");
        }

        [Command("hide", onlyOne: true, commandType: CommandType.Anim, description: "/hide [1-13]")]
        public static void AnimCommandHide(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/hide [1-13]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower@female@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower@female@idle_a", "idle_c");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower@female@react_cowering", "base_back_left");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@code_human_cower@male@base",
                        "base");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower@male@idle_a", "idle_b");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower@male@idle_b", "idle_d");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower@male@react_cowering", "base_back_left");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower_stand@female@base", "base");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower@female@base", "base");
                    break;

                case 10:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower_stand@female@idle_a", "idle_c");
                    break;

                case 11:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower_stand@male@base", "base");
                    break;

                case 12:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower_stand@male@idle_a", "idle_b");
                    break;

                case 13:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cower_stand@male@react_cowering", "base_right");
                    break;

                default:
                    player.SendSyntaxMessage("/hide [1-13]");
                    break;
            }
        }

        [Command("lookout", onlyOne: true, commandType: CommandType.Anim, description: "/lookout [1-4]")]
        public static void AnimCommandLookout(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/lookout [1-4]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cross_road@female@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cross_road@female@idle_a", "idle_c");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cross_road@male@base", "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_cross_road@male@idle_a", "idle_e");
                    break;

                default:
                    player.SendSyntaxMessage("/lookout [1-4]");
                    break;
            }
        }

        [Command("investigate", onlyOne: true, commandType: CommandType.Anim, description: "/investigate [1-9]")]
        public static void AnimCommandInvestigate(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/investigate [1-9]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_police_investigate@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_police_investigate@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_police_investigate@idle_b", "idle_f");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@medic@standing@kneel@base",
                        "base");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@medic@standing@kneel@idle_a", "idle_a");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@medic@standing@tendtodead@base", "base");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@medic@standing@tendtodead@idle_a", "idle_a");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@medic@standing@timeofdeath@base", "base");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@medic@standing@timeofdeath@idle_a", "idle_a");
                    break;

                default:
                    player.SendSyntaxMessage("/investigate [1-9]");
                    break;
            }
        }

        [Command("drink", onlyOne: true, commandType: CommandType.Anim, description: "/drink [1-12]")]
        public static void AnimCommandDrink(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/drink [1-12]");
                return;
            }

            switch (number)
            {
                case 1: // can in right hand
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_drinking@beer@female@base", "static");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_drinking@beer@male@base", "static");
                    break;

                case 3: //Coffee in right hand
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_aa_coffee@base",
                        "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_aa_coffee@idle_a", "idle_a");
                    break;

                case 5: // beer in right hand 683570518
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_drinking@beer@female@base", "base");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_drinking@beer@female@idle_a", "idle_f");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_drinking@beer@male@base", "base");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_drinking@beer@male@idle_a", "idle_a");
                    break;

                case 9: //coffee
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_drinking@coffee@female@base", "base");
                    break;

                case 10:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_drinking@coffee@female@idle_a", "idle_a");
                    break;

                case 11:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_drinking@coffee@male@base", "base");
                    break;

                case 12:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_drinking@coffee@male@idle_a", "idle_a");
                    break;

                default:
                    player.SendSyntaxMessage("/drink [1-12]");
                    break;
            }
        }

        [Command("crossarms", onlyOne: true, commandType: CommandType.Anim, description: "/crossarms [1-13]")]
        public static void AnimCommandCrossArms(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/crossarms [1-13]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_idles_cop@female@static", "static");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_idles_cop@male@static", "static");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_cop_idles@female@idle_a", "idle_b");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_cop_idles@female@idle_b", "idle_e");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_cop_idles@male@idle_a", "idle_b");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_cop_idles@male@idle_b", "idle_e");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@female_arms_crossed@base", "base");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_a");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_cop_idles@male@idle_b", "idle_e");
                    break;

                case 10:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@male_c@base", "base");
                    break;

                case 11:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@male_c@idle_a", "idle_a");
                    break;

                case 12:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@male_c@idle_b", "idle_d");
                    break;

                case 13:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "anim@heists@heist_corona@single_team", "single_team_loop_boss");
                    break;

                default:
                    player.SendSyntaxMessage("/crossarms [1-13]");
                    break;
            }
        }

        [Command("idle", onlyOne: true, commandType: CommandType.Anim, description: "/idle [1-30]")]
        public static void AnimCommandIdle(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/idle [1-30]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_music_listen@female@base", "static");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_music_listen@male@base", "static");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_rain@female@base", "static");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_rain@male_a@base", "static");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_texting@female@base", "static");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@code_human_wander_texting@male@base", "static");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_movie_studio_light@base", "base");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_movie_studio_light@idle_a", "idle_b");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_parking_meter@female@base", "base_female");
                    break;

                case 10:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_parking_meter@female@idle_a", "idle_b_female");
                    break;

                case 11:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_parking_meter@male@base", "base");
                    break;

                case 12:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_parking_meter@male@idle_a", "idle_a");
                    break;

                case 13:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_aa_coffee@base",
                        "base");
                    break;

                case 14:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_golf_player@male@base", "base");
                    break;

                case 15:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_golf_player@male@idle_a", "idle_a");
                    break;

                case 16:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_golf_player@male@idle_b", "idle_d");
                    break;

                case 17:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@female_arm_side@base", "base");
                    break;

                case 18:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_a");
                    break;

                case 19:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@female_hold_arm@base", "base");
                    break;

                case 20:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@female_hold_arm@idle_a", "idle_a");
                    break;

                case 21:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@male_a@base", "base");
                    break;

                case 22:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@male_a@idle_a", "idle_b");
                    break;

                case 23:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@male_a@idle_a", "idle_a");
                    break;

                case 24:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@male_b@base", "base");
                    break;

                case 25:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hang_out_street@male_b@idle_a", "idle_b");
                    break;

                case 26:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_hiker_standing@female@base", "base");
                    break;

                case 27:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_prostitute@hooker@base", "base");
                    break;

                case 28:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_prostitute@hooker@idle_a", "idle_a");
                    break;

                case 29:
                    Handler.PlayScenario(player, "WORLD_HUMAN_HANG_OUT_STREET");
                    break;

                case 30:
                    Handler.PlayScenario(player, "WORLD_HUMAN_HANG_OUT_STREET_CLUBHOUSE");
                    break;

                default:
                    player.SendSyntaxMessage("/idle [1-30]");
                    break;
            }
        }

        [Command("lean", onlyOne: true, commandType: CommandType.Anim, description: "/lean [1-40]")]
        public static void AnimCommandLean(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/lean [1-40]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@prop_human_bum_bin@base",
                        "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@prop_human_bum_bin@idle_a",
                        "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@prop_human_bum_bin@idle_b",
                        "idle_b");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_bum_shopping_cart@male@base", "base");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_bum_shopping_cart@male@idle_a", "idle_a");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@coffee@base", "base");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@coffee@idle_a", "idle_a");
                    break;

                case 8: //Right hand
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@smoke@base", "base");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@smoke@idle_a", "idle_a");
                    break;

                case 10:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@hand_up@base", "base");
                    break;

                case 11:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@hand_up@idle_a", "idle_a");
                    break;

                case 12:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@hand_up@idle_b", "idle_e");
                    break;

                case 13:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@holding_elbow@base", "base");
                    break;

                case 14:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@holding_elbow@idle_a", "idle_a");
                    break;

                case 15:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@holding_elbow@idle_b", "idle_e");
                    break;

                case 16:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@mobile@base", "base");
                    break;

                case 17:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@mobile@idle_a", "idle_a");
                    break;

                case 18:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@texting@base", "base");
                    break;

                case 19:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@female@wall@back@texting@idle_a", "idle_a");
                    break;

                case 20:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@coffee@base", "base");
                    break;

                case 21:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@coffee@idle_a", "idle_a");
                    break;

                case 22:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@beer@base", "base");
                    break;

                case 23:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@beer@idle_a", "idle_b");
                    break;

                case 24:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@foot_up@base", "base");
                    break;

                case 25:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@foot_up@idle_a", "idle_a");
                    break;

                case 26:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@foot_up@idle_b", "idle_d");
                    break;

                case 27:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@hands_together@base", "base");
                    break;

                case 28:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@hands_together@idle_a", "idle_c");
                    break;

                case 29:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@hands_together@idle_b", "idle_e");
                    break;

                case 30:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@legs_crossed@base", "base");
                    break;

                case 31:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@legs_crossed@idle_a", "idle_c");
                    break;

                case 32:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@legs_crossed@idle_b", "idle_d");
                    break;

                case 33:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@mobile@base", "base");
                    break;

                case 34:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@mobile@idle_a", "idle_a");
                    break;

                case 35:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@smoking@base", "base");
                    break;

                case 36:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@smoking@idle_a", "idle_a");
                    break;

                case 37:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@texting@base", "base");
                    break;

                case 38:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_leaning@male@wall@back@texting@idle_a", "idle_b");
                    break;

                case 39:
                    Handler.PlayScenario(player, "WORLD_HUMAN_LEANING");
                    break;

                case 40:
                    Handler.PlayScenario(player, "WORLD_HUMAN_LEANING_CASINO_TERRACE");
                    break;

                default:
                    player.SendSyntaxMessage("/lean [1-30]");
                    break;
            }
        }

        [Command("reach", onlyOne: true, commandType: CommandType.Anim, description: "/reach [1-2")]
        public static void AnimCommandReach(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/reach [1-2]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@prop_human_movie_bulb@base",
                        "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_movie_bulb@bidle_a", "idle_a");
                    break;

                default:
                    player.SendSyntaxMessage("/reach [1-2]");
                    break;
            }
        }

        [Command("workout", onlyOne: true, commandType: CommandType.Anim, description: "/workout [1-2]")]
        public static void AnimCommandWorkout(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/workout [1-12]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_muscle_chin_ups@male@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@prop_human_muscle_chin_ups@male@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_muscle_flex@arms_at_side@base", "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_muscle_flex@arms_at_side@idle_a", "idle_a");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_muscle_flex@arms_in_front@base", "base");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_muscle_flex@arms_in_front@idle_a", "idle_b");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_muscle_free_weights@male@barbell@base", "base");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_muscle_free_weights@male@barbell@idle_a", "idle_a");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_push_ups@male@base", "base");
                    break;

                case 10:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_push_ups@male@idle_a", "idle_a");
                    break;

                case 11:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sit_ups@male@base", "base");
                    break;

                case 12:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sit_ups@male@idle_a", "idle_a");
                    break;

                default:
                    player.SendSyntaxMessage("/workout [1-12]");
                    break;
            }
        }

        [Command("smoke", onlyOne: true, commandType: CommandType.Anim, description: "/smoke [1-18]")]
        public static void AnimCommandSmoke(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/smoke [1-18]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_aa_smoke@male@idle_a", "idle_c");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_aa_smoke@male@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_prostitute@french@base", "idle_a");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking@female@base", "base");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking@female@idle_a", "idle_a");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking@male@male_a@base", "base");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking@male@male_a@idle_a", "idle_c");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking@male@male_b@base", "base");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking@male@male_b@idle_a", "idle_b");
                    break;

                case 10: //Cigar object for pot
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking_pot@female@base", "base");
                    break;

                case 11:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking_pot@female@idle_a", "idle_b");
                    break;

                case 12:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking_pot@male@base", "base");
                    break;

                case 13:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_smoking_pot@male@idle_a", "idle_c");
                    break;

                case 14:
                    Handler.PlayScenario(player, "WORLD_HUMAN_AA_SMOKE");
                    break;

                case 15:
                    Handler.PlayScenario(player, "WORLD_HUMAN_SMOKING");
                    break;

                case 16:
                    Handler.PlayScenario(player, "WORLD_HUMAN_SMOKING_CLUBHOUSE");
                    break;

                case 17:
                    Handler.PlayScenario(player, "WORLD_HUMAN_SMOKING_POT");
                    break;

                case 18:
                    Handler.PlayScenario(player, "WORLD_HUMAN_SMOKING_POT_CLUBHOUSE");
                    break;

                default:
                    player.SendSyntaxMessage("/smoke [1-18]");
                    break;
            }
        }

        [Command("binoculars", onlyOne: true, commandType: CommandType.Anim, description: "/binoculars [1-7]")]
        public static void AnimCommandBinoculars(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/binoculars [1-7]");
                return;
            }

            switch (number)
            {
                case 1: //Added to right hand, same for the rest
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_binoculars@female@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_binoculars@female@idle_a", "idle_b");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_binoculars@female@idle_b", "idle_f");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_binoculars@female@idle_b", "idle_f");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_binoculars@female@idle_b", "idle_f");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_binoculars@female@idle_b", "idle_f");
                    break;

                case 7:
                    Handler.PlayScenario(player, "WORLD_HUMAN_BINOCULARS");
                    break;

                default:
                    player.SendSyntaxMessage("/binoculars [1-7]");
                    break;
            }
        }

        [Command("hobo", onlyOne: true, commandType: CommandType.Anim, description: "/hobo [1-13]")]
        public static void AnimCommandHobo(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/hobo [1-13]");
                return;
            }

            switch (number)
            {
                case 1: //On right hand
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_freeway@male@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_freeway@male@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_freeway@male@idle_b", "idle_d");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_standing@depressed@base", "base");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_standing@depressed@idle_a", "idle_a");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_wash@male@high@base", "base");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_wash@male@high@idle_a", "idle_a");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_wash@male@low@base", "base");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_wash@male@low@idle_a", "idle_a");
                    break;

                case 10:
                    Handler.PlayScenario(player, "WORLD_HUMAN_BUM_FREEWAY");
                    break;

                case 11:
                    Handler.PlayScenario(player, "WORLD_HUMAN_BUM_SLUMPED");
                    break;

                case 12:
                    Handler.PlayScenario(player, "WORLD_HUMAN_BUM_STANDING");
                    break;

                case 13:
                    Handler.PlayScenario(player, "WORLD_HUMAN_BUM_WASH");
                    break;

                default:
                    player.SendSyntaxMessage("/hobo [1-13]");
                    break;
            }
        }

        [Command("fallover", onlyOne: true, commandType: CommandType.Anim, description: "/fallover [1-7]")]
        public static void AnimCommandFallover(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/fallover [1-7]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_freeway@male@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_freeway@male@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_freeway@male@idle_b", "idle_d");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "combat@damage@writhe",
                        "writhe_loop");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "combat@damage@writheidle_a",
                        "writhe_idle_a");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "combat@damage@writheidle_b",
                        "writhe_idle_e");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "combat@damage@writheidle_c",
                        "writhe_idle_g");
                    break;

                default:
                    player.SendSyntaxMessage("/fallover [1-7]");
                    break;
            }
        }

        [Command("laydown", onlyOne: true, commandType: CommandType.Anim, description: "/laydown [1-12]")]
        public static void AnimCommandLaydown(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/laydown [1-12]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_slumped@male@laying_on_left_side@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_slumped@male@laying_on_left_side@idle_a", "idle_b");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_slumped@male@laying_on_right_side@idle_a", "idle_a");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_slumped@male@laying_on_right_side@idle_b", "idle_d");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sunbathe@female@back@base", "base");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sunbathe@female@back@idle_a", "idle_a");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sunbathe@female@front@base", "base");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sunbathe@female@front@idle_a", "idle_c");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sunbathe@male@back@base", "base");
                    break;

                case 10:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sunbathe@male@back@idle_a", "idle_a");
                    break;

                case 11:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sunbathe@male@front@base", "base");
                    break;

                case 12:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_sunbathe@male@front@idle_a", "idle_a");
                    break;

                default:
                    player.SendSyntaxMessage("/laydown [1-12]");
                    break;
            }
        }

        [Command("drunk", onlyOne: true, commandType: CommandType.Anim, description: "/drunk [1-2]")]
        public static void AnimCommandDrunk(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/drunk [1-2]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_standing@drunk@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_standing@drunk@idle_a", "idle_a");
                    break;

                default:
                    player.SendSyntaxMessage("/drunk [1-2]");
                    break;
            }
        }

        [Command("twitchy", onlyOne: true, commandType: CommandType.Anim, description: "/twitchy [1-4]")]
        public static void AnimCommandTwitchy(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/twitchy [1-4]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_bum_standing@twitchy@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop | (int)AnimationFlags.AllowPlayerControl,
                        "amb@world_human_bum_standing@twitchy@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_power_walker@female@base", "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop,
                        "amb@world_human_prostitute@crackhooker@base", "base");
                    break;

                default:
                    player.SendSyntaxMessage("/twitchy [1-4]");
                    break;
            }
        }

        [Command("signal", onlyOne: true, commandType: CommandType.Anim, description: "/signal [1-3]")]
        public static void AnimCommandSignal(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/signal [1-3]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_car_park_attendant@male@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_car_park_attendant@male@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_janitor@male@base", "base");
                    break;

                default:
                    player.SendSyntaxMessage("/signal [1-3]");
                    break;
            }
        }

        [Command("cheer", onlyOne: true, commandType: CommandType.Anim, description: "/cheer [1-9]")]
        public static void AnimCommandCheer(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/cheer [1-9]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_cheering@female_a",
                        "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_cheering@female_b",
                        "base");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_cheering@female_c",
                        "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_cheering@female_d",
                        "base");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_cheering@male_a",
                        "base");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_cheering@male_b",
                        "base");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_cheering@male_d",
                        "base");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_cheering@male_e",
                        "base");
                    break;

                case 9:
                    Handler.PlayScenario(player, "WORLD_HUMAN_CHEERING");
                    break;

                default:
                    player.SendSyntaxMessage("/cheer [1-9]");
                    break;
            }
        }

        [Command("clipboard", onlyOne: true, commandType: CommandType.Anim, description: "/clipboard [1-5]")]
        public static void AnimCommandClipboard(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/clipboard [1-5]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_clipboard@male@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_clipboard@male@idle_a", "idle_c");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_clipboard@male@idle_b", "idle_d");
                    break;

                case 4:
                    Handler.PlayScenario(player, "WORLD_HUMAN_CHEERING");
                    break;

                case 5:
                    Handler.PlayScenario(player, "WORLD_HUMAN_CLIPBOARD_FACILITY");
                    break;

                default:
                    player.SendSyntaxMessage("/clipboard [1-5]");
                    break;
            }
        }

        [Command("drugdeal", onlyOne: true, commandType: CommandType.Anim, description: "/drugdeal [1-5]")]
        public static void AnimCommandDrugDeal(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/drugdeal [1-5]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_drug_dealer_hard@male@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_drug_dealer_hard@male@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_drug_dealer_hard@male@idle_b", "idle_d");
                    break;

                case 4:
                    Handler.PlayScenario(player, "WORLD_HUMAN_DRUG_DEALER");
                    break;

                case 5:
                    Handler.PlayScenario(player, "WORLD_HUMAN_DRUG_DEALER_HARD");
                    break;

                default:
                    player.SendSyntaxMessage("/drugdeal [1-5]");
                    break;
            }
        }

        [Command("gardening", onlyOne: true, commandType: CommandType.Anim, description: "/gardening [1-4]")]
        public static void AnimCommandGardening(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/gardening [1-4]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_gardener_plant@female@base", "base_female");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_gardener_plant@female@idle_a", "idle_a_female");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_gardener_plant@male@base", "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_gardener_plant@male@idle_a", "idle_b");
                    break;

                default:

                    player.SendSyntaxMessage("/gardening [1-4]");
                    break;
            }
        }

        [Command("guard", onlyOne: true, commandType: CommandType.Anim, description: "/guard [1-9]")]
        public static void AnimCommandGuard(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/guard [1-9]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_guard_patrol@male@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_guard_patrol@male@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_guard_patrol@male@idle_b", "idle_e");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_stand_fire@male@base", "base");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_stand_guard@male@base", "base");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_stand_guard@male@idle_a", "idle_a");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_stand_guard@male@idle_b", "idle_d");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@amb@code_human_patrol@male@2h@base", "base");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@amb@code_human_patrol@male@2h@idle_a", "idle_c");
                    break;

                default:
                    player.SendSyntaxMessage("/guard [1-9]");
                    break;
            }
        }

        [Command("hammer", onlyOne: true, commandType: CommandType.Anim, description: "/hammer [1-2]")]
        public static void AnimCommandHammer(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/hammer [1-2]");
                return;
            }
            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_hammering@male@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_hammering@male@idle_a", "idle_a");
                    break;

                default:
                    player.SendSyntaxMessage("/hammer [1-2]");
                    break;
            }
        }

        [Command("jog", onlyOne: true, commandType: CommandType.Anim, description: "/jog [1-6]")]
        public static void AnimCommandJog(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/jog [1-6]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_jog_standing@female@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_jog_standing@female@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_jog_standing@male@fitbase", "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_jog_standing@male@fitidle_a", "idle_a");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_jog_standing@male@idle_a", "idle_a");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_jog_standing@male@idle_b", "idle_d");
                    break;

                default:
                    player.SendSyntaxMessage("/jog [1-6]");
                    break;
            }
        }

        [Command("guitar", onlyOne: true, commandType: CommandType.Anim, description: "/guitar [1-3]")]
        public static void AnimCommandGuitar(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/guitar [1-3]");
                return;
            }
            switch (number)
            {
                case 1://Acoustic guitar
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_musician@guitar@male@base", "base");
                    break;

                case 2://Eletric guitar
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_musician@guitar@male@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@mp_player_intcelebrationfemale@air_guitar", "air_guitar");
                    break;

                default:
                    player.SendSyntaxMessage("/jog [1-6]");
                    break;
            }
        }

        [Command("getjiggy", onlyOne: true, commandType: CommandType.Anim, description: "/getjiggy [1-4]")]
        public static void AnimCommand(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/getjiggy [1-4]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_partying@female@partying_beer@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_partying@female@partying_cellphone@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_partying@male@partying_beer@base", "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_partying@male@partying_beer@idle_a", "idle_b");
                    break;

                default:
                    player.SendSyntaxMessage("/getjiggy [1-4]");
                    break;
            }
        }

        [Command("sit", onlyOne: true, commandType: CommandType.Anim, description: "/sit [1-34]")]
        public static void AnimCommandSit(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/sit [1-34]");
                return;
            }
            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_picnic@female@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_picnic@female@idle_a", "idle_a");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_picnic@male@base", "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_picnic@male@idle_a", "idle_a");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_stupor@male@base", "base");
                    break;

                case 6:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_stupor@male@idle_a", "idle_c");
                    break;

                case 7:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@heists@heist_safehouse_intro@phone_couch@male", "phone_couch_male_idle");
                    break;

                case 8:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@heists@heist_safehouse_intro@phone_couch@female", "phone_couch_female_idle");
                    break;

                case 9:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@mp_rollarcoaster", "idle_b_player_two");
                    break;

                case 10:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_ARMCHAIR");
                    break;

                case 11:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_BAR");
                    break;

                case 12:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_BENCH");
                    break;

                case 13:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_BENCH_FACILITY");
                    break;

                case 14:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_BENCH_DRINK");
                    break;

                case 15:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_BENCH_DRINK_FACILITY");
                    break;

                case 16:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_BENCH_DRINK_BEER");
                    break;

                case 17:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_BENCH_FOOD");
                    break;

                case 18:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_BENCH_FOOD_FACILITY");
                    break;

                case 19:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_BUS_STOP_WAIT");
                    break;

                case 20:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_CHAIR");
                    break;

                case 21:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_CHAIR_DRINK");
                    break;

                case 22:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_CHAIR_DRINK_BEER");
                    break;

                case 23:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_CHAIR_DRINK_BEER");
                    break;

                case 24:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_CHAIR_UPRIGHT");
                    break;

                case 25:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_CHAIR_MP_PLAYER");
                    break;

                case 26:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_COMPUTER");
                    break;

                case 27:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_COMPUTER_LOW");
                    break;

                case 28:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_DECKCHAIR");
                    break;

                case 29:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_DECKCHAIR_DRINK");
                    break;

                case 30:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS");
                    break;

                case 31:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS_PRISON");
                    break;

                case 32:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_SEWING");
                    break;

                case 33:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_STRIP_WATCH");
                    break;

                case 34:
                    Handler.PlayScenario(player, "PROP_HUMAN_SEAT_SUNLOUNGER");
                    break;

                default:
                    player.SendSyntaxMessage("/sit [1-34]");
                    break;
            }
        }

        [Command("mech", onlyOne: true, commandType: CommandType.Anim, description: "/mech [1-5]")]
        public static void AnimCommandMech(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/mech [1-5]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_vehicle_mechanic@male@base", "base");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_vehicle_mechanic@male@idle_a", "idle_b");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_welding@male@base", "base");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_welding@male@idle_a", "idle_a");
                    break;

                case 5:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01");
                    break;

                default:
                    player.SendSyntaxMessage("/mech [1-5]");
                    break;
            }
        }

        [Command("yoga", onlyOne: true, commandType: CommandType.Anim, description: "/yoga [1-2]")]
        public static void AnimCommandYoga(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/yoga [1-2]");
                return;
            }
            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_yoga@female@base", "base_b");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "amb@world_human_yoga@male@base", "base_a");
                    break;

                default:
                    player.SendSyntaxMessage("/yoga [1-2]");
                    break;
            }
        }

        [Command("bonghit", onlyOne: true, commandType: CommandType.Anim, description: "/bonghit [1-4]")]
        public static void AnimCommandBongHit(IPlayer player, string numberString = "")
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            bool tryParse = int.TryParse(numberString, out int number);

            if (!tryParse || numberString == "")
            {
                player.SendSyntaxMessage("/bonghit [1-4]");
                return;
            }

            switch (number)
            {
                case 1:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@safehouse@bong", "bong_stage1");
                    break;

                case 2:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@safehouse@bong", "bong_stage2");
                    break;

                case 3:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@safehouse@bong", "bong_stage3");
                    break;

                case 4:
                    Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@safehouse@bong", "bong_stage4");
                    break;

                default:
                    player.SendSyntaxMessage("/bonghit [1-4]");
                    break;
            }
        }

        [Command("middlefinger", commandType: CommandType.Anim, description: "/middlefinger")]
        public static void AnimCommandMiddleFinger(IPlayer player)
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;

            Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@mp_player_intcelebrationmale@finger", "finger");
        }

        [Command("salute", commandType: CommandType.Anim, description: "/salute")]
        public static void AnimCommandSalute(IPlayer player)
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;
            Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@mp_player_intcelebrationmale@salute", "salute");
        }

        [Command("slowclap", commandType: CommandType.Anim, description: "/slowclap")]
        public static void AnimCommandSlowClap(IPlayer player)
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;
            Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@mp_player_intcelebrationmale@slow_clap", "slow_clap");
        }

        [Command("facepalm", commandType: CommandType.Anim, description: "/facepalm")]
        public static void AnimCommandFacepalm(IPlayer player)
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;
            Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@mp_player_intcelebrationmale@face_palm", "face_palm");
        }

        [Command("handsup", commandType: CommandType.Anim, description: "/handsup")]
        public static void AnimCommandHandsup(IPlayer player)
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;
            Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "mp_am_hold_up", "handsup_base");
        }

        [Command("restrained", commandType: CommandType.Anim, description: "/restrained")]
        public static void AnimCommandRestrained(IPlayer player)
        {
            bool canAnim = Handler.CanAnim(player, true);

            if (!canAnim) return;
            Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, "anim@move_m@prisoner_cuffed_rc", "aim_low_loop");
        }
    }
}