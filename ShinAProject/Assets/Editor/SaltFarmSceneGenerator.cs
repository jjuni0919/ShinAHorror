using System.Collections.Generic;
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

        [MenuItem("ShinA/Generate Salt Farm Scene")]
        public static void Generate()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            Scene saltFarmScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(saltFarmScene);

            GameObject initializer = new("SaltFarmMapInitializer");
            initializer.AddComponent<SaltFarmMapInitializer>();

            bool saved = EditorSceneManager.SaveScene(saltFarmScene, ScenePath);
            EditorSceneManager.CloseScene(saltFarmScene, true);
            if (previousScene.IsValid())
            {
                SceneManager.SetActiveScene(previousScene);
            }

            if (!saved)
            {
                Debug.LogError($"Failed to save salt farm scene: {ScenePath}");
                return;
            }

            RegisterBuildScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Salt farm map scene generated and registered.");
        }

        private static void RegisterBuildScene()
        {
            List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
            int index = scenes.FindIndex(scene => scene.path == ScenePath);
            if (index < 0)
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            }
            else if (!scenes[index].enabled)
            {
                scenes[index] = new EditorBuildSettingsScene(ScenePath, true);
            }
            else
            {
                return;
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
