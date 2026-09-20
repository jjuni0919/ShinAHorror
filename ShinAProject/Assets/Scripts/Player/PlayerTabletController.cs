using ShinA.Missions;
using ShinA.Settings;
using ShinA.Maps;
using ShinA.Inventory;
using ShinA.SaveSystem;
using ShinA.Economy;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ShinA.Player
{
    public sealed class PlayerTabletController : MonoBehaviour
    {
        private FirstPersonController controller;
        private Transform tabletRoot;
        private GameObject screenObject;
        private Material bodyMaterial;
        private Text missionText;
        private Text timerText;
        private Text progressText;
        private Text instructionText;
        private GameObject overviewRoot;
        private GameObject pageRoot;
        private Font tabletFont;
        private string page = "현황";
        private int pageIndex;
        private string feedback;
        private bool settlementConfirmationPending;
        private Vector3 loweredPosition = new(0.48f, -0.65f, 0.75f);
        private Vector3 raisedPosition = new(0f, -0.12f, 0.62f);

        public bool IsOpen { get; private set; }

        public void Initialize(Camera playerCamera, FirstPersonController playerController)
        {
            controller = playerController;
            BuildTablet(playerCamera.transform);
            SetOpen(false, true);
        }

        private void Update()
        {
            if (Time.timeScale == 0f)
            {
                return;
            }

            if (PlayerInputBindings.WasPressedThisFrame(PlayerAction.Tablet) &&
                (IsOpen || controller == null || controller.CanAct))
            {
                SetOpen(!IsOpen, false);
            }

            if (tabletRoot != null)
            {
                Vector3 target = IsOpen ? raisedPosition : loweredPosition;
                tabletRoot.localPosition = Vector3.Lerp(tabletRoot.localPosition, target,
                    1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
            }

            if (screenObject != null && screenObject.activeSelf)
            {
                RefreshScreen();
            }
        }

        public void SetOpen(bool open, bool immediate = false)
        {
            IsOpen = open;
            settlementConfirmationPending = false;
            controller?.SetGameplayInputBlocked(this, open);

            if (screenObject != null)
            {
                screenObject.SetActive(open);
                if (open) RenderPage();
            }

            if (immediate && tabletRoot != null)
            {
                tabletRoot.localPosition = open ? raisedPosition : loweredPosition;
            }
        }

        private void OnDisable()
        {
            controller?.SetGameplayInputBlocked(this, false);
        }

        private void OnEnable()
        {
            if (IsOpen)
            {
                controller?.SetGameplayInputBlocked(this, true);
            }
        }

        private void BuildTablet(Transform cameraTransform)
        {
            GameObject root = new("Player Tablet");
            tabletRoot = root.transform;
            tabletRoot.SetParent(cameraTransform, false);
            tabletRoot.localRotation = Quaternion.Euler(8f, 0f, 0f);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Tablet Body";
            body.transform.SetParent(tabletRoot, false);
            body.transform.localScale = new Vector3(0.72f, 0.44f, 0.035f);
            Destroy(body.GetComponent<Collider>());
            bodyMaterial = CreateMaterial(new Color(0.025f, 0.028f, 0.032f));
            body.GetComponent<Renderer>().sharedMaterial = bodyMaterial;

            GameObject canvasObject = new("Tablet Screen", typeof(RectTransform));
            canvasObject.transform.SetParent(tabletRoot, false);
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.localPosition = new Vector3(0f, 0f, -0.021f);
            canvasRect.localRotation = Quaternion.identity;
            canvasRect.sizeDelta = new Vector2(640f, 380f);
            canvasRect.localScale = Vector3.one * 0.001f;

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cameraTransform.GetComponent<Camera>();
            canvasObject.AddComponent<GraphicRaycaster>();

            Image screen = canvasObject.AddComponent<Image>();
            screen.color = new Color(0.025f, 0.12f, 0.14f, 1f);
            screen.raycastTarget = true;

            Font font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "맑은 고딕", "Arial" }, 32);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            tabletFont = font;

            overviewRoot = new GameObject("탐험 현황", typeof(RectTransform));
            overviewRoot.transform.SetParent(canvasObject.transform, false);
            RectTransform overviewRect = overviewRoot.GetComponent<RectTransform>();
            overviewRect.anchorMin = Vector2.zero;
            overviewRect.anchorMax = new Vector2(1f, 0.78f);
            overviewRect.offsetMin = overviewRect.offsetMax = Vector2.zero;
            pageRoot = new GameObject("태블릿 내용", typeof(RectTransform));
            pageRoot.transform.SetParent(canvasObject.transform, false);
            RectTransform pageRect = pageRoot.GetComponent<RectTransform>();
            pageRect.anchorMin = Vector2.zero;
            pageRect.anchorMax = new Vector2(1f, 0.78f);
            pageRect.offsetMin = pageRect.offsetMax = Vector2.zero;
            string[] tabs = { "현황", "방문지", "상점", "창고", "정산" };
            for (int i = 0; i < tabs.Length; i++)
            {
                string selected = tabs[i];
                CreateButton(canvasObject.transform, selected, new Vector2(0.02f + i * 0.195f, 0.8f),
                    new Vector2(0.2f + i * 0.195f, 0.98f), () =>
                    {
                        page = selected;
                        pageIndex = 0;
                        feedback = null;
                        settlementConfirmationPending = false;
                        RenderPage();
                    });
            }

            Text title = CreateText(overviewRoot.transform, font, "SHIN A // 현장 태블릿", 25,
                new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.96f), TextAnchor.MiddleLeft);
            title.color = new Color(0.55f, 0.92f, 0.88f);
            missionText = CreateText(overviewRoot.transform, font, string.Empty, 29,
                new Vector2(0.07f, 0.63f), new Vector2(0.93f, 0.82f), TextAnchor.MiddleLeft);
            timerText = CreateText(overviewRoot.transform, font, string.Empty, 54,
                new Vector2(0.07f, 0.35f), new Vector2(0.93f, 0.64f), TextAnchor.MiddleCenter);
            progressText = CreateText(overviewRoot.transform, font, string.Empty, 23,
                new Vector2(0.07f, 0.16f), new Vector2(0.93f, 0.36f), TextAnchor.MiddleLeft);
            instructionText = CreateText(overviewRoot.transform, font, string.Empty, 17,
                new Vector2(0.07f, 0.03f), new Vector2(0.93f, 0.16f), TextAnchor.MiddleLeft);

            screenObject = canvasObject;
            RenderPage();
            RefreshScreen();
            EnsureEventSystem();
        }

        private void RefreshScreen()
        {
            MissionSession session = MissionSession.Instance;
            PlayerProgress progress = session.Progress;
            if (session.IsActive)
            {
                int seconds = Mathf.CeilToInt(session.RemainingTime);
                missionText.text = $"{progress.Day}일차  ·  {session.ActiveMapName}";
                timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
                timerText.color = seconds <= 60
                    ? new Color(1f, 0.28f, 0.2f)
                    : new Color(0.65f, 1f, 0.88f);
                progressText.text = $"현장 창고  {session.FieldStorageCount}개  ·  임무 수익 +{session.PendingCurrency:N0}\n" +
                                    $"보유 재화  {progress.Currency:N0}";
                instructionText.text = "제한 시간 안에 본부 단말기에서 임무 완료 처리를 진행하십시오.";
                return;
            }

            missionText.text = $"{progress.Day}일차  ·  {(progress.IsCompanyDay ? "수익 정산일" : "탐험 준비")}";
            timerText.text = "--:--";
            timerText.color = new Color(0.55f, 0.92f, 0.88f);
            progressText.text = $"보유 재화  {progress.Currency:N0}\n주문 대기 {SaveManager.Instance.CurrentData.pendingOrders.Count}개";
            instructionText.text = session.LastOutcome switch
            {
                MissionOutcome.Completed => "이전 임무 완료 · 보관 물품 이송 완료",
                MissionOutcome.PlayerDied => "이전 임무 실패 · 플레이어 사망",
                MissionOutcome.TimeExpired => "이전 임무 실패 · 제한 시간 초과",
                _ => "임무를 선택해 탐사를 시작하십시오."
            };
        }

        public void ShowStorage()
        {
            page = "창고";
            pageIndex = 0;
            SetOpen(true);
        }

        private void RenderPage()
        {
            if (pageRoot == null) return;
            overviewRoot.SetActive(page == "현황");
            pageRoot.SetActive(page != "현황");
            foreach (Transform child in pageRoot.transform)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
            if (page == "현황") return;

            PlayerProgress progress = MissionSession.Instance.Progress;
            string heading = $"{progress.Day}일차 · 보유 {progress.Currency:N0}";
            List<(string label, System.Action action)> rows = new();
            if (page == "방문지")
            {
                bool waiting = SceneManager.GetActiveScene().name == "WaitingScene";
                foreach (MapRecord map in MapDatabase.Instance.Maps)
                {
                    if (map.mapId == "waiting_room" || map.mapId == "company" || progress.IsCompanyDay) continue;
                    MapRecord selected = map;
                    rows.Add(($"{map.displayName}{(SaveManager.Instance.CurrentData.selectedMapId == map.mapId ? " [선택됨]" : "")}", () =>
                    {
                        if (!waiting || MissionSession.Instance.IsActive)
                            feedback = "방문 장소는 대기실에서 선택할 수 있습니다.";
                        else
                        {
                            SaveManager.Instance.CurrentData.selectedMapId = selected.mapId;
                            SaveManager.Instance.SaveCurrent();
                            feedback = "태블릿을 닫고 중앙 출발 단말기에서 F 키를 누르세요.";
                        }
                        RenderPage();
                    }));
                }
                heading += progress.IsCompanyDay ? " · 수익 정산 필요" : " · 다음 탐험지 선택";
                if (progress.IsCompanyDay) feedback = "회수 박스에 물품을 맡기고 정산 탭을 이용해 주세요.";
            }
            else if (page == "상점")
            {
                ShopCatalog shop = Resources.Load<ShopCatalog>("ShopCatalog");
                if (shop != null)
                for (int i = 0; i < shop.Offers.Count; i++)
                {
                    if (!shop.IsAvailable(i, progress.Day)) continue;
                    int index = i;
                    ShopOffer offer = shop.Offers[i];
                    rows.Add(($"{offer.item.ItemName} · {offer.price:N0} · {(offer.availability >= 1f ? "상시" : "오늘의 상품")}", () =>
                    {
                        shop.TryBuy(index, out feedback);
                        RenderPage();
                    }));
                }
                heading += $" · 주문 {SaveManager.Instance.CurrentData.pendingOrders.Count}개";
            }
            else if (page == "정산")
            {
                CollectionBox box = FindFirstObjectByType<CollectionBox>();
                if (!progress.IsCompanyDay || box == null)
                {
                    heading = "수익 정산 안내";
                    feedback ??= "5일차마다 대기실 회수 박스에 물품을 맡긴 후 정산할 수 있습니다.";
                }
                else
                {
                    bool valid = box.TryGetRevenue(out int revenue);
                    heading = $"정산 예정 {revenue:N0} / 할당량 {box.RevenueQuota:N0}";
                    rows.Add(("맡긴 물품 판매 · 정산하기", () =>
                    {
                        if (revenue < box.RevenueQuota && !settlementConfirmationPending)
                        {
                            settlementConfirmationPending = true;
                            feedback = "할당량 미달입니다. 다시 누르면 게임이 종료됩니다.";
                            RenderPage();
                            return;
                        }
                        settlementConfirmationPending = false;
                        box.TrySettle(out feedback);
                        RenderPage();
                    }));
                    rows.Add(($"달성 시 퀘스트 보상 +{box.QuestReward:N0} (할당량 제외)", null));
                    foreach (int number in SaveManager.Instance.CurrentData.collectionItemNumbers)
                    {
                        ItemDefinition item = Resources.Load<ItemDefinition>($"Items/Item_{number:000}");
                        rows.Add((item != null ? $"{item.ItemName} · {item.SalePrice:N0}" : "알 수 없는 아이템", null));
                    }
                    if (!valid) feedback = "일부 물품 정보가 없어 정산할 수 없습니다.";
                }
            }
            else
            {
                IReadOnlyList<int> stored = MissionSession.Instance.FieldStorageItemNumbers;
                heading = $"{progress.Day}일차 현장 창고 · {stored.Count}개";
                for (int i = 0; i < stored.Count; i++)
                {
                    ItemDefinition item = Resources.Load<ItemDefinition>($"Items/Item_{stored[i]:000}");
                    rows.Add(($"{i + 1}. {(item != null ? item.ItemName : "알 수 없는 아이템")}", null));
                }
                if (!MissionSession.Instance.IsActive) feedback = "귀환한 물품은 대기실 바닥에서 직접 집을 수 있습니다.";
            }
            CreateText(pageRoot.transform, tabletFont, heading, 21, new Vector2(0.04f, 0.85f), new Vector2(0.96f, 1f), TextAnchor.MiddleLeft);
            int pages = Mathf.Max(1, (rows.Count + 3) / 4);
            pageIndex = Mathf.Clamp(pageIndex, 0, pages - 1);
            for (int i = 0; i < 4 && pageIndex * 4 + i < rows.Count; i++)
            {
                var row = rows[pageIndex * 4 + i];
                Vector2 min = new(0.04f, 0.66f - i * 0.16f);
                Vector2 max = new(0.96f, 0.8f - i * 0.16f);
                if (row.action == null) CreateText(pageRoot.transform, tabletFont, row.label, 21, min, max, TextAnchor.MiddleLeft);
                else CreateButton(pageRoot.transform, row.label, min, max, row.action);
            }
            if (pages > 1)
            {
                CreateButton(pageRoot.transform, "이전", new Vector2(0.04f, 0.02f), new Vector2(0.2f, 0.15f), () => { pageIndex--; RenderPage(); });
                CreateButton(pageRoot.transform, "다음", new Vector2(0.8f, 0.02f), new Vector2(0.96f, 0.15f), () => { pageIndex++; RenderPage(); });
            }
            CreateText(pageRoot.transform, tabletFont, feedback ?? (rows.Count == 0 ? "보관된 물품이 없습니다." : ""), 15,
                new Vector2(0.21f, 0f), new Vector2(0.79f, 0.17f), TextAnchor.MiddleCenter);
        }

        private void CreateButton(Transform parent, string label, Vector2 min, Vector2 max, System.Action action)
        {
            GameObject root = new(label, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image image = root.AddComponent<Image>();
            image.color = new Color(0.12f, 0.28f, 0.3f);
            Button button = root.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => { if (Time.timeScale > 0f) action(); });
            CreateText(root.transform, tabletFont, label, 22, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
        }

        private static Text CreateText(Transform parent, Font font, string value, int size,
            Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment)
        {
            GameObject textObject = new("Tablet Text", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = new Color(0.78f, 0.94f, 0.9f);
            text.raycastTarget = false;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { color = color };
        }

        private void OnDestroy()
        {
            if (bodyMaterial != null)
            {
                Destroy(bodyMaterial);
            }
        }
    }
}
