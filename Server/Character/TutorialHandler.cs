using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Character.Clothing;
using Server.Character.Tattoo;
using Server.Chat;
using Server.Extensions;
using Server.Jobs.Taxi;

namespace Server.Character
{
    public class TutorialHandler
    {
        private static Timer _timer = null;

        public static readonly Position LegionBankPosition = new Position(155.4989f, -1021.3714f, 34.380981f);

        private static Position _currentPosition;
        private static Position _newPosition;

        private static Rotation _currentRotation;
        private static Rotation _newRotation;

        private static readonly int _duration = 5000;

        public static readonly Position StrawberrySpawnPosition = new Position(259.9912f, -1204.3649f, 29.279907f);

        public static void InitTutorial()
        {
            _timer = new Timer(1)
            {
                AutoReset = true,
            };

            _timer.Start();

            _timer.Elapsed += TimerOnElapsed;
        }

        private static void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            List<IPlayer> players = Alt.Server.GetPlayers().ToList();

            if (!players.Any()) return;

            _timer.Stop();

            foreach (IPlayer player in players)
            {
                if (player.GetClass().CompletedTutorial) continue;

                if (player.GetClass().CharacterId == 0) continue;

                bool hasData = player.GetData("tutorial:Stage", out int tutorialStage);

                if (!hasData) continue;

                if (tutorialStage >= 19) continue;

                player.GetData("tutorial:time", out int time);

                if (time < 500)
                {
                    int newTime = time += 1;

                    player.SetData("tutorial:time", newTime);
                    continue;
                }

                player.GetData("tutorial:lastCamPos", out Position lastCamPosition);

                player.GetData("tutorial:lastCamRot", out Rotation lastCamRotation);

                if (tutorialStage == 1)
                {
                    player.SendNotification("This is Legion Square Bank. One of many throughout Los Santos.");
                    player.SetData("tutorial:Stage", 2);

                    player.SetData("tutorial:time", 1);
                    continue;
                }

                if (tutorialStage == 2)
                {
                    player.SendNotification("All ATM's throughout the City are useable as well!");
                    player.SetData("tutorial:Stage", 3);
                    player.SetData("tutorial:time", 1);
                    continue;
                }
                if (tutorialStage == 3)
                {
                    player.SendNotification("Now we're going to look at a few jobs.");
                    player.SetData("tutorial:Stage", 4);
                    player.SetData("tutorial:time", 1);
                    continue;
                }
                if (tutorialStage == 4)
                {
                    player.SendNotification("We also have many jobs, including Bus Driver, Taxi Driver, Fishing & Many More!");
                    player.SetData("tutorial:Stage", 5);
                    player.SetData("tutorial:time", 1);

                    Position newPosition = new Position(946.39124f, -153.25714f, 74.53845f);
                    Rotation newRotation = new Rotation(0, 0, 104.88189f);

                    player.SetData("tutorial:lastCamPos", newPosition);

                    player.SetData("tutorial:lastCamRot", newRotation);

                    CameraExtension.InterpolateCamera(player, lastCamPosition, lastCamRotation, 100, newPosition, newRotation, 100, _duration);

                    continue;
                }
                if (tutorialStage == 5)
                {
                    player.SendNotification("Here is the Taxi Depot. You may get the job here.");
                    player.SetData("tutorial:Stage", 6);
                    player.SetData("tutorial:time", 1);
                    continue;
                }
                if (tutorialStage == 6)
                {
                    player.SendNotification("Next we will head to the Bus Depot.");
                    player.SetData("tutorial:Stage", 7);
                    player.SetData("tutorial:time", 1);

                    Position newPosition = new Position(405.04614f, -650.7165f, 33.487915f);
                    Rotation newRotation = new Rotation(0, 0, -76.53543f);

                    player.SetData("tutorial:lastCamPos", newPosition);

                    player.SetData("tutorial:lastCamRot", newRotation);

                    CameraExtension.InterpolateCamera(player, lastCamPosition, lastCamRotation, 100, newPosition, newRotation, 100, _duration);

                    continue;
                }
                if (tutorialStage == 7)
                {
                    player.SendNotification("Here is the Bus Depot. Here you can drive or catch a bus.");
                    player.SetData("tutorial:Stage", 8);
                    player.SetData("tutorial:time", 1);
                    continue;
                }
                if (tutorialStage == 8)
                {
                    player.SendNotification("Next we will head to the Mechanic Job.");
                    player.SetData("tutorial:Stage", 9);
                    player.SetData("tutorial:time", 1);

                    Position newPosition = new Position(-44.887913f, -1099.1736f, 27.415405f);
                    Rotation newRotation = new Rotation(0, 0, -110.551186f);

                    player.SetData("tutorial:lastCamPos", newPosition);

                    player.SetData("tutorial:lastCamRot", newRotation);

                    CameraExtension.InterpolateCamera(player, lastCamPosition, lastCamRotation, 100, newPosition, newRotation, 100, _duration);

                    continue;
                }
                if (tutorialStage == 9)
                {
                    player.SendNotification("Here is the Mechanic Focus. As a mechanic you can work on vehicles.");
                    player.SetData("tutorial:Stage", 10);
                    player.SetData("tutorial:time", 1);
                    continue;
                }
                if (tutorialStage == 10)
                {
                    player.SendNotification("Now off to the Department of Motor Vehicles");
                    player.SetData("tutorial:Stage", 11);
                    player.SetData("tutorial:time", 1);

                    Position newPosition = new Position(-921.6f, -269.14285f, 45.451294f);
                    Rotation newRotation = new Rotation(0, 0, 110.551186f);

                    player.SetData("tutorial:lastCamPos", newPosition);

                    player.SetData("tutorial:lastCamRot", newRotation);

                    CameraExtension.InterpolateCamera(player, lastCamPosition, lastCamRotation, 100, newPosition, newRotation, 100, _duration);

                    continue;
                }
                if (tutorialStage == 11)
                {
                    player.SendNotification("At the DMV you can get any license you need.");
                    player.SetData("tutorial:Stage", 12);
                    player.SetData("tutorial:time", 1);
                    continue;
                }
                if (tutorialStage == 12)
                {
                    player.SendNotification("Next is the Police Headquarters");
                    player.SetData("tutorial:Stage", 13);
                    player.SetData("tutorial:time", 1);

                    Position newPosition = new Position(421.35825f, -971.8681f, 33.38025f);
                    Rotation newRotation = new Rotation(0, 0, -130.3937f);

                    player.SetData("tutorial:lastCamPos", newPosition);

                    player.SetData("tutorial:lastCamRot", newRotation);

                    CameraExtension.InterpolateCamera(player, lastCamPosition, lastCamRotation, 100, newPosition, newRotation, 100, _duration);

                    continue;
                }
                if (tutorialStage == 13)
                {
                    player.SendNotification("This is the Mission Row PD. Home of the LS-PD!.");
                    player.SetData("tutorial:Stage", 14);
                    player.SetData("tutorial:time", 1);
                    continue;
                }
                if (tutorialStage == 14)
                {
                    player.SendNotification("Next is the Medical & Fire Headquarters");
                    player.SetData("tutorial:Stage", 15);
                    player.SetData("tutorial:time", 1);

                    Position newPosition = new Position(227.31429f, -1622.8748f, 29.279907f);
                    Rotation newRotation = new Rotation(0, 0, 138.89763f);

                    player.SetData("tutorial:lastCamPos", newPosition);

                    player.SetData("tutorial:lastCamRot", newRotation);

                    CameraExtension.InterpolateCamera(player, lastCamPosition, lastCamRotation, 100, newPosition, newRotation, 100, _duration);

                    continue;
                }
                if (tutorialStage == 15)
                {
                    player.SendNotification("This is the Davis FD. Home of the LS-FD!.");
                    player.SetData("tutorial:Stage", 16);
                    player.SetData("tutorial:time", 1);
                    continue;
                }
                if (tutorialStage == 16)
                {
                    player.SendNotification("Now onto the shops!");
                    player.SetData("tutorial:Stage", 17);
                    player.SetData("tutorial:time", 1);

                    Position newPosition = new Position(27.837364f, -1356.4879f, 29.17871f);
                    Rotation newRotation = new Rotation(0, 0, -5.669291f);

                    player.SetData("tutorial:lastCamPos", newPosition);

                    player.SetData("tutorial:lastCamRot", newRotation);

                    CameraExtension.InterpolateCamera(player, lastCamPosition, lastCamRotation, 100, newPosition, newRotation, 100, _duration);

                    continue;
                }
                if (tutorialStage == 17)
                {
                    player.SendNotification("Many shops can be found throughout Los Santos.");
                    player.SendNotification("Including, Clothing, General, Tattoo, Dealerships");
                    player.SetData("tutorial:Stage", 18);
                    player.SetData("tutorial:time", 1);
                    continue;
                }
                if (tutorialStage == 18)
                {
                    player.SetData("tutorial:Stage", 19);
                    player.SendNotification($"This concludes the tutorial.");
                    player.HideChat(false);
                    player.ShowHud(true);
                    player.ChatInput(true);

                    player.SendNotification($"Your now going to spawn at the Strawberry Bus Depot.");

                    using Context context = new Context();

                    Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                    playerCharacter.StartStage = 2;

                    playerCharacter.PosX = StrawberrySpawnPosition.X;
                    playerCharacter.PosY = StrawberrySpawnPosition.Y;
                    playerCharacter.PosZ = StrawberrySpawnPosition.Z;

                    playerCharacter.Dimension = 0;

                    context.SaveChanges();

                    CameraExtension.DeleteCamera(player);

                    CreatorRoom.LeaveCreatorRoom(player);

                    player.GetClass().CompletedTutorial = true;

                    continue;
                }
            }

            _timer.Start();
        }

        public static void StartTutorial(IPlayer player)
        {
            if (player?.FetchCharacter() == null) return;

            player.HideChat(true);
            player.ShowHud(false);
            player.ChatInput(false);
            player.GetClass().CompletedTutorial = false;

            player.Dimension = 0;

            player.SendNotification(
                $"We are going to show you a few locations throughout Los Santos that will be helpful.");

            _currentPosition = player.Position;
            _newPosition = LegionBankPosition;

            _currentRotation = player.Rotation;
            _newRotation = new Rotation(0, 0, 155.9055f);

            CameraExtension.InterpolateCamera(player, _currentPosition, _currentRotation, 100, _newPosition, _newRotation, 100, _duration);

            player.SendNotification(
                $"The first place we are going to head across to is the Bank located at Legion Square.");

            player.SetData("tutorial:time", 1);
            player.SetData("tutorial:Stage", 1);

            player.SetData("tutorial:lastCamPos", _newPosition);

            player.SetData("tutorial:lastCamRot", _newRotation);

            Models.Character selectedCharacter = player.FetchCharacter();

            if (JsonConvert.DeserializeObject<CustomCharacter>(selectedCharacter.CustomCharacter).Gender == 0)
            {
                player.Model = (uint)PedModel.FreemodeMale01;
            }
            else
            {
                player.Model = (uint)PedModel.FreemodeFemale01;
            }

            player.SetCharacterId(selectedCharacter.Id);

            CharacterHandler.LoadCustomCharacter(player);
        }
    }
}