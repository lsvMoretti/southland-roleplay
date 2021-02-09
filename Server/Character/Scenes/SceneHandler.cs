using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AltV.Net.Data;
using Server.Extensions;
using Server.Extensions.TextLabel;

namespace Server.Character.Scenes
{
    public class SceneHandler
    {
        public static List<Scene> Scenes = new List<Scene>();

        public static void CreateScene(string text, Position position, LsvColor color, int characterId, int dimension, bool addToDb = true, int databaseId = 0)
        {
            try
            {
                TextLabel newLabel = new TextLabel(text, position, TextFont.FontChaletComprimeCologne, color, dimension: dimension);

                newLabel.Add();

                Scene newScene = new Scene(text, position, newLabel, characterId);

                if (addToDb)
                {
                    using Context context = new Context();

                    Models.Scene dbScene = new Models.Scene(characterId, position, text, dimension);

                    context.Scenes.Add(dbScene);

                    context.SaveChanges();

                    newScene.DatabaseId = dbScene.Id;
                }
                else
                {
                    newScene.DatabaseId = databaseId;
                }

                Scenes.Add(newScene);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static Scene? FetchNearestScene(Position position, float range = 3f, int characterId = 0)
        {
            float lastDistance = range;
            Scene? lastScene = null;

            foreach (Scene scene in Scenes)
            {
                if (characterId > 0)
                {
                    if (scene.CharacterId != characterId) continue;
                }

                Position scenePosition = scene.Position;

                var distance = scenePosition.Distance(position);

                if (distance < lastDistance)
                {
                    lastDistance = distance;
                    lastScene = scene;
                }
            }

            return lastScene;
        }

        public static bool RemoveScene(Scene scene)
        {
            try
            {
                if (scene == null) return false;

                scene.TextLabel.Remove();

                if (scene.DatabaseId <= 0) return false;

                using Context context = new Context();

                var dbScene = context.Scenes.Find(scene.DatabaseId);

                if (dbScene == null) return false;

                context.Scenes.Remove(dbScene);

                context.SaveChanges();
                Scenes.Remove(scene);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static void RemoveNearestScene(Position position)
        {
            try
            {
                Scene scene = Scenes.OrderBy(x => x.Position.Distance(position)).FirstOrDefault();

                if (scene == null) return;

                scene.TextLabel.Remove();

                Scenes.Remove(scene);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static void LoadAllScenes()
        {
            using Context context = new Context();

            List<Models.Scene> scenes = context.Scenes.ToList();

            if (!scenes.Any()) return;

            Console.WriteLine($"Loading Scenes..");

            LsvColor color = new LsvColor(194, 162, 218);

            foreach (Models.Scene scene in scenes)
            {
                CreateScene(scene.Text, new Position(scene.PosX, scene.PosY, scene.PosZ), color, scene.CharacterId, scene.Dimension, false, scene.Id);
            }

            Console.WriteLine($"Loaded {scenes.Count} Scenes.");
        }
    }
}