using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Tutorial.Editor
{
    public static class WorkoutHUDBuilder
    {
        static readonly Color Black = new Color(0.05f, 0.05f, 0.07f, 0.96f);
        static readonly Color White = Color.white;
        static readonly Color Yellow = new Color(1f, 0.82f, 0.08f, 1f);
        static readonly Color Blue = new Color(0.10f, 0.58f, 0.92f, 1f);
        static readonly Color Red = new Color(1f, 0.12f, 0.12f, 1f);
        static readonly Color Gray = new Color(0.40f, 0.40f, 0.45f, 1f);
        static readonly Color DarkGray = new Color(0.15f, 0.15f, 0.18f, 0.96f);
        static readonly Color Shadow = new Color(0f, 0f, 0f, 0.4f);

        [MenuItem("KachokGame/Build Roblox HUD (v2)")]
        [MenuItem("KachokGame/Build WorkoutHUD Canvas (Cartoon)")]
        public static void Build()
        {
            var old = GameObject.Find("WorkoutHUD_Canvas");
            if (old != null) Undo.DestroyObjectImmediate(old);

            foreach (var legacy in Object.FindObjectsByType<StandaloneInputModule>(FindObjectsSortMode.None))
                Undo.DestroyObjectImmediate(legacy);

            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }
            else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject);
            }

            var canvasGO = new GameObject("WorkoutHUD_Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Workout HUD");

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            var hud = canvasGO.AddComponent<WorkoutHUD>();
            var root = Child(canvasGO, "RootPanel"); Stretch(root);

            // Hint
            var hint = Panel(canvasGO, "HintPanel", Blue, new Vector2(460f, 64f));
            SetAnchorBottom(hint, 280f, new Vector2(460f, 64f));
            var hintText = Label(hint, "HintText", "Нажми E, чтобы взять", 24, White); Stretch(hintText.gameObject);

            // Muscle Card
            var muscleCard = Panel(root, "MuscleCard", Yellow, new Vector2(240f, 76f));
            SetTopLeft(muscleCard, 28f, 28f);
            var muscleValue = Label(muscleCard, "MuscleValueText", "0/100", 26, Black);
            Anchor(muscleValue.gameObject, 0.08f, 0.4f, 0.92f, 0.95f); muscleValue.alignment = TextAlignmentOptions.MidlineLeft;
            var muscleSub = Label(muscleCard, "MuscleSubText", "мышца · стадия 1", 14, Black);
            Anchor(muscleSub.gameObject, 0.08f, 0.05f, 0.92f, 0.45f); muscleSub.alignment = TextAlignmentOptions.MidlineLeft;

            // Stamina Card
            var staminaCard = Panel(root, "StaminaCard", Blue, new Vector2(240f, 76f));
            SetTopRight(staminaCard, 28f, 28f);
            var staminaPercent = Label(staminaCard, "StaminaPercent", "100%", 26, White);
            Anchor(staminaPercent.gameObject, 0.55f, 0.35f, 0.92f, 0.90f); staminaPercent.alignment = TextAlignmentOptions.MidlineRight;
            var staminaLabel = Label(staminaCard, "StaminaLabel", "СИЛЫ", 18, White);
            Anchor(staminaLabel.gameObject, 0.08f, 0.45f, 0.55f, 0.90f); staminaLabel.alignment = TextAlignmentOptions.MidlineLeft;
            var (staminaBar, staminaFill) = Bar(staminaCard, "StaminaBar", Blue, 0.08f, 0.15f, 0.92f, 0.30f);

            // Tabs
            var tabs = Child(root, "TabBar");
            SetAnchorTop(tabs, -40f, new Vector2(400f, 54f));
            var tabsLayout = tabs.AddComponent<HorizontalLayoutGroup>();
            tabsLayout.spacing = 14f; tabsLayout.childAlignment = TextAnchor.MiddleCenter;
            tabsLayout.childControlHeight = true; tabsLayout.childControlWidth = true;
            var tabWorkout = FlatButton(tabs, "TabWorkout", "ТРЕН", Yellow, Black, new Vector2(130f, 54f));
            var tabRest = FlatButton(tabs, "TabRest", "ОТДЫХ", Black, White, new Vector2(130f, 54f));
            var tabResults = FlatButton(tabs, "TabResults", "ИТОГ", Black, White, new Vector2(130f, 54f));

            // Workout Panel
            var workoutPanel = Child(root, "WorkoutPanel"); Stretch(workoutPanel);

            var multiplierText = Label(workoutPanel, "MultiplierText", "x1.0", 82, Yellow);
            SetAnchorCenter(multiplierText.gameObject, new Vector2(300f, 80f), new Vector2(260f, 100f));
            multiplierText.fontStyle = FontStyles.Bold;

            var phaseBadge = Panel(workoutPanel, "PhaseBadgeBG", DarkGray, new Vector2(260f, 56f));
            SetAnchorTop(phaseBadge, -120f, new Vector2(260f, 56f));
            var phaseText = Label(phaseBadge, "PhaseBadgeText", "СТАРТ", 22, White); Stretch(phaseText.gameObject);

            var speedBadge = Panel(workoutPanel, "SpeedBadge", Blue, new Vector2(340f, 60f));
            SetAnchorBottom(speedBadge, 370f, new Vector2(340f, 60f));
            var speedText = Label(speedBadge, "TrainingSpeedText", "Training Speed: 0.00", 24, White); Stretch(speedText.gameObject);

            // Main Card (BOTTOM)
            var mainCard = Panel(workoutPanel, "MainCard", Black, new Vector2(1040f, 280f));
            SetAnchorBottom(mainCard, 40f, new Vector2(1040f, 280f));

            var content = Child(mainCard, "Content"); Stretch(content);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.offsetMin = new Vector2(30f, 25f);
            contentRt.offsetMax = new Vector2(-30f, -25f);

            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 16f;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;

            var topRow = Row(content, "TopRow", 30f, 16f);
            var dots = Child(topRow, "DotsRow"); 
            AddLayout(dots, 200f, 20f);
            var dotsLayout = dots.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 14f; dotsLayout.childAlignment = TextAnchor.MiddleLeft;
            dotsLayout.childControlHeight = true; dotsLayout.childControlWidth = true;
            dotsLayout.childForceExpandHeight = true; dotsLayout.childForceExpandWidth = false;

            var fatigueLabel = Label(topRow, "FatigueLabel", "ТЕМП", 18, White); AddLayout(fatigueLabel.gameObject, 80f, 30f);
            var fatigueTrack = Track(topRow, "FatigueBar", new Vector2(400f, 24f)); AddLayout(fatigueTrack, 400f, 24f);
            var fatigueBar = fatigueTrack.GetComponent<Slider>();
            var fatigueFill = fatigueTrack.transform.Find("FillArea/Fill").GetComponent<Image>(); fatigueFill.color = Gray;
            var fatigueText = Label(topRow, "FatiguePercent", "0%", 18, White); AddLayout(fatigueText.gameObject, 70f, 30f);

            var progressRow = Row(content, "ProgressRow", 46f, 16f);
            var progressTrack = Track(progressRow, "SetProgressBar", new Vector2(800f, 42f)); AddLayout(progressTrack, 800f, 46f);
            var setProgress = progressTrack.GetComponent<Slider>();
            var setProgressFill = progressTrack.transform.Find("FillArea/Fill").GetComponent<Image>();
            var setProgressText = Label(progressRow, "SetProgressText", "0/15", 22, White); AddLayout(setProgressText.gameObject, 100f, 46f);

            var setInfo = Label(content, "SetInfoText", "Подход 1 из 5 · Выпады", 26, White);
            AddLayout(setInfo.gameObject, -1f, 32f); setInfo.alpha = 0.85f;

            var buttonRow = Row(content, "ButtonRow", 84f, 24f);
            var exitButton = FlatButton(buttonRow, "ExitButton", "EXIT", Red, White, new Vector2(240f, 84f)); AddLayout(exitButton.gameObject, 240f, 84f);
            var autoButton = FlatButton(buttonRow, "ClickButton", "TRAIN", Yellow, Black, new Vector2(680f, 84f)); AddLayout(autoButton.gameObject, 680f, 84f);
            var clickButtonBG = autoButton.transform.Find("Fill").GetComponent<Image>();

            // Rest
            var restPanel = Child(root, "RestPanel"); Stretch(restPanel); restPanel.SetActive(false);
            var restCard = Panel(restPanel, "RestCard", Black, new Vector2(480f, 220f)); SetAnchorCenter(restCard, Vector2.zero, new Vector2(480f, 220f));
            var restHeader = Label(restCard, "RestHeaderText", "ОТДЫХ", 32, White); Anchor(restHeader.gameObject, 0.05f, 0.65f, 0.95f, 0.95f);
            var restMuscle = Label(restCard, "RestMuscleText", "+10 МЫШЦ", 34, Yellow); Anchor(restMuscle.gameObject, 0.05f, 0.35f, 0.95f, 0.65f);
            var (restTimerBar, _) = Bar(restCard, "RestTimerBar", Blue, 0.08f, 0.16f, 0.92f, 0.28f);
            var restTimerText = Label(restCard, "RestTimerText", "8с", 24, White); Anchor(restTimerText.gameObject, 0.05f, 0.00f, 0.95f, 0.16f);

            // Results
            var resultsPanel = Child(root, "ResultsPanel"); Stretch(resultsPanel); resultsPanel.SetActive(false);
            var resultsCard = Panel(resultsPanel, "ResultsCard", Black, new Vector2(560f, 320f)); SetAnchorCenter(resultsCard, Vector2.zero, new Vector2(560f, 320f));
            var resultsTitle = Label(resultsCard, "ResultsTitleText", "ГОТОВО!", 38, White); Anchor(resultsTitle.gameObject, 0.05f, 0.80f, 0.95f, 0.95f);
            var resultsDay = Label(resultsCard, "ResultsDayText", "День X", 24, White); Anchor(resultsDay.gameObject, 0.05f, 0.60f, 0.95f, 0.78f);
            var resultsSets = Label(resultsCard, "ResultsSetsText", "Подходы: X/X", 24, White); Anchor(resultsSets.gameObject, 0.05f, 0.44f, 0.95f, 0.60f);
            var resultsMuscle = Label(resultsCard, "ResultsMuscleText", "+X Мышц", 26, Yellow); Anchor(resultsMuscle.gameObject, 0.05f, 0.24f, 0.95f, 0.44f);
            var resultsRecord = Label(resultsCard, "ResultsRecordText", "НОВЫЙ РЕКОРД", 26, Red); Anchor(resultsRecord.gameObject, 0.05f, 0.10f, 0.95f, 0.24f);
            var doneButton = FlatButton(resultsCard, "DoneButton", "OK", Blue, White, new Vector2(200f, 60f)); SetAnchorBottom(doneButton.gameObject, 25f, new Vector2(200f, 60f));

            // Wiring
            var so = new SerializedObject(hud); so.Update();
            SetRef(so, "rootPanel", root); SetRef(so, "workoutPanel", workoutPanel); SetRef(so, "restPanel", restPanel); SetRef(so, "resultsPanel", resultsPanel); SetRef(so, "hintPanel", hint);
            SetRef(so, "tabWorkout", tabWorkout); SetRef(so, "tabRest", tabRest); SetRef(so, "tabResults", tabResults);
            SetRef(so, "muscleValueText", muscleValue); SetRef(so, "muscleSubText", muscleSub);
            SetRef(so, "phaseBadgeText", phaseText); SetRef(so, "phaseBadgeBG", phaseBadge.transform.Find("Fill").GetComponent<Image>());
            SetRef(so, "staminaBar", staminaBar); SetRef(so, "staminaFill", staminaFill); SetRef(so, "staminaPercent", staminaPercent);
            SetRef(so, "setProgressBar", setProgress); SetRef(so, "setProgressFill", setProgressFill); SetRef(so, "setProgressText", setProgressText);
            SetRef(so, "fatigueBar", fatigueBar); SetRef(so, "fatigueFill", fatigueFill); SetRef(so, "fatiguePercent", fatigueText);
            SetRef(so, "clickButton", autoButton); SetRef(so, "clickButtonBG", clickButtonBG);
            SetRef(so, "setDotsParent", dots.transform); SetRef(so, "setDotPrefab", DotPrefab());
            SetRef(so, "setInfoText", setInfo); SetRef(so, "hintText", hintText);
            SetRef(so, "multiplierText", multiplierText); SetRef(so, "trainingSpeedText", speedText);
            SetRef(so, "restHeaderText", restHeader); SetRef(so, "restMuscleText", restMuscle); SetRef(so, "restTimerBar", restTimerBar); SetRef(so, "restTimerText", restTimerText);
            SetRef(so, "resultsTitleText", resultsTitle); SetRef(so, "resultsDayText", resultsDay); SetRef(so, "resultsSetsText", resultsSets); SetRef(so, "resultsMuscleText", resultsMuscle); SetRef(so, "resultsRecordText", resultsRecord);
            SetRef(so, "doneButton", doneButton);
            so.ApplyModifiedProperties();

            SetPrivateColor(hud, "tabActiveColor", Yellow); SetPrivateColor(hud, "tabInactiveColor", Black);
            SetPrivateColor(hud, "warmupColor", Blue); SetPrivateColor(hud, "workZoneColor", Yellow); SetPrivateColor(hud, "failColor", Red);

            var player = Object.FindFirstObjectByType<Tutorial.DumbbellWorkout>();
            if (player != null) { var pSo = new SerializedObject(player); pSo.Update(); SetRef(pSo, "workoutHUD", hud); pSo.ApplyModifiedProperties(); }

            Selection.activeGameObject = canvasGO; Debug.Log("Workout HUD rebuilt completely with correct layout.");
        }

        static GameObject Panel(GameObject parent, string name, Color fillColor, Vector2 size) {
            var root = Child(parent, name); root.GetComponent<RectTransform>().sizeDelta = size;
            var shadow = Child(root, "Shadow"); Stretch(shadow); shadow.GetComponent<RectTransform>().offsetMin = new Vector2(4f, -6f); shadow.GetComponent<RectTransform>().offsetMax = new Vector2(4f, -6f); shadow.AddComponent<Image>().color = Shadow;
            var fill = Child(root, "Fill"); Stretch(fill); fill.AddComponent<Image>().color = fillColor;
            return root; }

        static (Slider, Image) Bar(GameObject parent, string name, Color fillColor, float x0, float y0, float x1, float y1) {
            var s = Track(parent, name, Vector2.zero); Anchor(s, x0, y0, x1, y1);
            var fill = s.transform.Find("FillArea/Fill").GetComponent<Image>(); fill.color = fillColor;
            return (s.GetComponent<Slider>(), fill); }

        static GameObject Track(GameObject parent, string name, Vector2 size) {
            var root = Child(parent, name); if (size != Vector2.zero) root.GetComponent<RectTransform>().sizeDelta = size;
            root.AddComponent<Image>().color = Gray; var sl = root.AddComponent<Slider>(); sl.interactable = false; sl.direction = Slider.Direction.LeftToRight;
            var fa = Child(root, "FillArea"); Stretch(fa); var f = Child(fa, "Fill"); Stretch(f); f.AddComponent<Image>().color = Yellow;
            sl.fillRect = f.GetComponent<RectTransform>(); sl.targetGraphic = root.GetComponent<Image>(); sl.value = 0f; return root; }

        static Button FlatButton(GameObject parent, string name, string text, Color fillColor, Color textColor, Vector2 size) {
            var r = Panel(parent, name, fillColor, size); AddLayout(r, size.x, size.y); var b = r.AddComponent<Button>();
            var l = Label(r, "Label", text, 28, textColor); Stretch(l.gameObject); return b; }

        static TextMeshProUGUI Label(GameObject p, string n, string t, int s, Color c) {
            var go = Child(p, n); var tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text = t; tmp.fontSize = s; tmp.color = c;
            tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = FontStyles.Bold; tmp.raycastTarget = false; tmp.textWrappingMode = TextWrappingModes.NoWrap; return tmp; }

        static GameObject Row(GameObject parent, string name, float height, float spacing) {
            var row = Child(parent, name); AddLayout(row, -1f, height);
            var layout = row.AddComponent<HorizontalLayoutGroup>(); layout.spacing = spacing; layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = true; layout.childForceExpandWidth = false; return row; }

        static void AddLayout(GameObject go, float w, float h) { var l = go.AddComponent<LayoutElement>(); if (w >= 0) l.preferredWidth = w; if (h >= 0) l.preferredHeight = h; }
        static GameObject Child(GameObject p, string n) { var go = new GameObject(n); go.transform.SetParent(p.transform, false); go.AddComponent<RectTransform>(); return go; }
        static void Stretch(GameObject go) { var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; }
        static void Anchor(GameObject go, float x0, float y0, float x1, float y1) { var rt = go.GetComponent<RectTransform>(); rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1); rt.offsetMin = rt.offsetMax = Vector2.zero; }
        
        static void SetAnchorBottom(GameObject go, float yCenter, Vector2 size) { var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f); rt.pivot = new Vector2(0.5f, 0f); rt.anchoredPosition = new Vector2(0f, yCenter); rt.sizeDelta = size; }
        static void SetAnchorTop(GameObject go, float yCenter, Vector2 size) { var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f); rt.anchoredPosition = new Vector2(0f, yCenter); rt.sizeDelta = size; }
        static void SetAnchorCenter(GameObject go, Vector2 offset, Vector2 size) { var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = offset; rt.sizeDelta = size; }
        static void SetTopLeft(GameObject go, float x, float y) { var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f); rt.anchoredPosition = new Vector2(x, -y); }
        static void SetTopRight(GameObject go, float x, float y) { var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(1f, 1f); rt.anchoredPosition = new Vector2(-x, -y); }
        
        static void SetRef(SerializedObject so, string f, Object v) { var p = so.FindProperty(f); if (p != null) p.objectReferenceValue = v; }
        static void SetPrivateColor(WorkoutHUD h, string n, Color c) { h.GetType().GetField(n, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(h, c); }

        static GameObject DotPrefab() {
            const string path = "Assets/KachokGame/SetDotPrefab.prefab"; var pf = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (pf != null) return pf;
            var go = new GameObject("SetDot"); var rt = go.AddComponent<RectTransform>(); rt.sizeDelta = new Vector2(20f, 20f); go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.35f); AddLayout(go, 20f, 20f); return PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
        }
    }
}
