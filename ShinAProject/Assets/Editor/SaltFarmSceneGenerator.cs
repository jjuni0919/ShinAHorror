using System.Linq;
using ShinA.Maps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShinA.Editor
{
    public static class SaltFarmSceneGenerator
    {
        private const string ScenePath = "Assets/Scenes/SaltFarmScene.unity";

        [InitializeOnLoadMethod]
        private static void ScheduleGeneration()
        {
            EditorApplication.delayCall += GenerateIfMissing;
        }

        [MenuItem("ShinA/Generate Salt Farm Scene")]
        public static void Generate()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            Scene saltFarmScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(saltFarmScene);

            GameObject initializer = new("SaltFarmMapInitializer");
            initializer.AddComponent<SaltFarmMapInitializer>();

            EditorSceneManager.SaveScene(saltFarmScene, ScenePath);
            EditorSceneManager.CloseScene(saltFarmScene, true);
            if (previousScene.IsValid())
            {
                SceneManager.SetActiveScene(previousScene);
            }

            RegisterBuildScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Salt farm map scene generated and registered.");
        }

        private static void GenerateIfMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                Generate();
            }
            else
            {
                RegisterBuildScene();
            }
        }

        private static void RegisterBuildScene()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(scene => scene.path != ScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
