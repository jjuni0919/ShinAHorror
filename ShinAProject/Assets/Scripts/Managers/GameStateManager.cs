using ShinA.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShinA.Managers
{
    public enum GameState
    {
        Playing,
        Paused,
        Loading,
        GameOver
    }

    [DefaultExecutionOrder(-200)]
    public sealed class GameStateManager : MonoBehaviour
    {
        private static GameStateManager instance;
        private FirstPersonController playerController;
        private GameObject pauseScreen;
        private Text stateTitle;
        private Text stateInstruction;
        private GameObject restartButton;
        private EventSystem pausedEventSystem;
        private bool previousNavigationEvents;

        public static GameStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameStateManager>();
                    if (instance == null)
                    {
                        instance = new GameObject("GameStateManager").AddComponent<GameStateManager>();
                    }
                }

                return instance;
            }
        }

        public GameState CurrentState { get; private set; } = GameState.Playing;
        public bool IsPaused => CurrentState == GameState.Paused;

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
            BuildPauseScreen();
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplyState();
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame ||
                CurrentState == GameState.Loading || CurrentState == GameState.GameOver)
            {
                return;
            }

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<FirstPersonController>();
            }
            if (playerController != null)
            {
                SetState(IsPaused ? GameState.Playing : GameState.Paused);
            }
        }

        public void SetState(GameState state)
        {
            if (CurrentState == state)
            {
                return;
            }

            CurrentState = state;
            ApplyState();
        }

        private void ApplyState()
        {
            bool gameOver = CurrentState == GameState.GameOver;
            bool paused = CurrentState == GameState.Paused || gameOver;
            if (stateTitle != null) stateTitle.text = gameOver ? "게임 종료 · 수익 할당량 미달" : "일시정지";
            if (stateInstruction != null) stateInstruction.text = gameOver
                ? $"정산 수익 {ShinA.Missions.MissionSession.Instance.Progress.LastSettlementRevenue:N0}\n새 게임을 시작하면 기존 진행 상황이 초기화됩니다."
                : "ESC 키를 눌러 계속하기";
            restartButton?.SetActive(gameOver);
            Time.timeScale = paused ? 0f : 1f;
            pauseScreen?.SetActive(paused);

            if (paused && pausedEventSystem == null && EventSystem.current != null)
            {
                pausedEventSystem = EventSystem.current;
                previousNavigationEvents = pausedEventSystem.sendNavigationEvents;
                pausedEventSystem.sendNavigationEvents = false;
            }
            else if (!paused && pausedEventSystem != null)
            {
                pausedEventSystem.sendNavigationEvents = previousNavigationEvents;
                pausedEventSystem = null;
            }

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<FirstPersonController>();
            }
            if (playerController != null)
            {
                playerController.SetGameplayInputBlocked(this, CurrentState != GameState.Playing);
            }

            if (paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive)
            {
                return;
            }

            playerController = FindFirstObjectByType<FirstPersonController>();
            CurrentState = GameState.Playing;
            ApplyState();
        }

        private void BuildPauseScreen()
        {
            GameObject canvasObject = new("일시정지 화면", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            Image background = canvasObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.72f);

            Font font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 48);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            Text title = CreateText(canvasObject.transform, font, "일시정지", 52,
                new Vector2(0.25f, 0.5f), new Vector2(0.75f, 0.65f));
            title.fontStyle = FontStyle.Bold;
            stateTitle = title;
            stateInstruction = CreateText(canvasObject.transform, font, "ESC 키를 눌러 계속하기", 24,
                new Vector2(0.25f, 0.38f), new Vector2(0.75f, 0.5f));
            Text restartText = CreateText(canvasObject.transform, font, "새 게임", 28,
                new Vector2(0.4f, 0.25f), new Vector2(0.6f, 0.34f));
            restartText.raycastTarget = true;
            Button button = restartText.gameObject.AddComponent<Button>();
            button.targetGraphic = restartText;
            button.onClick.AddListener(() =>
            {
                if (!ShinA.SaveSystem.SaveManager.Instance.StartNewGame())
                    stateInstruction.text = "새 게임을 저장하거나 불러오지 못했습니다. 다시 시도해 주세요.";
            });
            restartButton = restartText.gameObject;
            restartButton.SetActive(false);

            pauseScreen = canvasObject;
            pauseScreen.SetActive(false);
        }

        private static Text CreateText(Transform parent, Font font, string value, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject textObject = new(value, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (pausedEventSystem != null)
            {
                pausedEventSystem.sendNavigationEvents = previousNavigationEvents;
            }
            if (playerController != null)
            {
                playerController.SetGameplayInputBlocked(this, false);
            }
            Time.timeScale = 1f;
            instance = null;
        }
    }
}
