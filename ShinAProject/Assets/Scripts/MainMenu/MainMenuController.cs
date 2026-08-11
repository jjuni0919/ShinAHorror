using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShinA.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Button firstSelectedButton;
        [SerializeField] private string gameSceneName = "SampleScene";

        private Coroutine statusRoutine;

        public void Initialize(Text status, Button firstButton, string sceneName)
        {
            statusText = status;
            firstSelectedButton = firstButton;
            gameSceneName = sceneName;
        }

        private void Awake()
        {
            ApplyKoreanFont();

            if (statusText != null)
            {
                statusText.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (EventSystem.current != null && firstSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
            }
        }

        public void StartGame()
        {
            if (Application.CanStreamedLevelBeLoaded(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
                return;
            }

            ShowStatus("게임 씬을 찾을 수 없습니다.");
        }

        public void OpenOnlineMode()
        {
            ShowStatus("온라인 모드는 준비 중입니다.");
        }

        public void OpenSettings()
        {
            ShowStatus("설정 메뉴는 준비 중입니다.");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowStatus(string message)
        {
            if (statusText == null)
            {
                return;
            }

            if (statusRoutine != null)
            {
                StopCoroutine(statusRoutine);
            }

            statusRoutine = StartCoroutine(ShowStatusRoutine(message));
        }

        private IEnumerator ShowStatusRoutine(string message)
        {
            statusText.text = message;
            statusText.gameObject.SetActive(true);
            statusText.canvasRenderer.SetAlpha(1f);

            yield return new WaitForSecondsRealtime(2f);

            statusText.CrossFadeAlpha(0f, 0.35f, true);
            yield return new WaitForSecondsRealtime(0.4f);
            statusText.gameObject.SetActive(false);
            statusText.canvasRenderer.SetAlpha(1f);
            statusRoutine = null;
        }

        private void ApplyKoreanFont()
        {
            string[] preferredFonts =
            {
                "Malgun Gothic",
                "맑은 고딕",
                "Apple SD Gothic Neo",
                "Noto Sans CJK KR",
                "Noto Sans KR",
                "Arial Unicode MS"
            };

            Font font = Font.CreateDynamicFontFromOSFont(preferredFonts, 48);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            foreach (Text label in GetComponentsInChildren<Text>(true))
            {
                label.font = font;
            }
        }
    }
}
