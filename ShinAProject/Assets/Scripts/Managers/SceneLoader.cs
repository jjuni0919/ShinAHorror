using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShinA.Managers
{
    public sealed class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;

        public static SceneLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<SceneLoader>();
                    if (instance == null)
                    {
                        GameObject managerObject = new("SceneLoader");
                        instance = managerObject.AddComponent<SceneLoader>();
                    }
                }

                return instance;
            }
        }

        public event Action<string> LoadStarted;
        public event Action<float> LoadProgressChanged;
        public event Action<string> LoadCompleted;

        public bool IsLoading { get; private set; }
        public float LoadProgress { get; private set; }
        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateOnStartup()
        {
            _ = Instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool LoadScene(string sceneName)
        {
            if (IsLoading || string.IsNullOrWhiteSpace(sceneName) ||
                !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                return false;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
            return true;
        }

        public bool ReloadCurrentScene()
        {
            return LoadScene(CurrentSceneName);
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            IsLoading = true;
            LoadProgress = 0f;
            LoadStarted?.Invoke(sceneName);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                IsLoading = false;
                yield break;
            }

            while (!operation.isDone)
            {
                LoadProgress = Mathf.Clamp01(operation.progress / 0.9f);
                LoadProgressChanged?.Invoke(LoadProgress);
                yield return null;
            }

            LoadProgress = 1f;
            IsLoading = false;
            LoadProgressChanged?.Invoke(LoadProgress);
            LoadCompleted?.Invoke(sceneName);
        }
    }
}
