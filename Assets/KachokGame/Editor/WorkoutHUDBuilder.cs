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
        static readonly Color Ink = new(0.07f, 0.07f, 0.09f, 0.98f);
        static readonly Color SoftBlack = new(0.11f, 0.11f, 0.14f, 0.94f);
        static readonly Color White = new(1f, 0.98f, 0.95f, 1f);
        static readonly Color Gold = new(1f, 0.79f, 0.16f, 1f);
        static readonly Color GoldDark = new(0.92f, 0.57f, 0.10f, 1f);
        static readonly Color Lime = new(0.50f, 1f, 0.33f, 1f);
        static readonly Color Red = new(1f, 0.28f, 0.25f, 1f);
        static readonly Color Blue = new(0.22f, 0.69f, 1f, 1f);
        static readonly Color Gray = new(0.43f, 0.43f, 0.48f, 1f);
        static readonly Color Shadow = new(0f, 0f, 0f, 0.35f);

        [MenuItem("KachokGame/Build Muscle Transform HUD")]
        [MenuItem("KachokGame/Build WorkoutHUD Canvas (Cartoon)")]
        [MenuItem("KachokGame/Build Roblox HUD (v2)")]
        public static void Build()
        {
            var old = GameObject.Find("WorkoutHUD_Canvas");
            if (old != null)
                Undo.DestroyObjectImmediate(old);

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
            var root = Child(canvasGO, "RootPanel");
            Stretch(root);

            var screenOverlay = Child(root, "ScreenOverlay");
            Stretch(screenOverlay);
            var screenOverlayImage = screenOverlay.AddComponent<Image>();
            screenOverlayImage.color = Color.clear;
            screenOverlayImage.raycastTarget = false;

            var hint = Card(root, "HintPanel", Blue, new Vector2(560f, 82f));
            SetAnchorBottom(hint, 30f, new Vector2(560f, 82f));
            var hintText = Label(hint, "HintText", "PRESS [E] TO START WORKOUT", 26, Ink);
            Stretch(hintText.gameObject);

            var workoutPanel = Child(root, "WorkoutPanel");
            Stretch(workoutPanel);

            var topLeft = Child(workoutPanel, "TopLeftCluster");
            SetTopLeft(topLeft, 24f, 24f);
            topLeft.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 240f);
            topLeft.AddComponent<VerticalLayoutGroup>().spacing = 16f;

            var muscleCard = Card(topLeft, "MuscleCard", Gold, new Vector2(360f, 110f));
            AddLayout(muscleCard, 360f, 110f);
            var muscleTitle = Label(muscleCard, "MuscleTitle", "MUSCLE", 22, Ink);
            Anchor(muscleTitle.gameObject, 0.07f, 0.62f, 0.45f, 0.92f);
            muscleTitle.alignment = TextAlignmentOptions.MidlineLeft;
            var muscleValue = Label(muscleCard, "MuscleValueText", "0/100", 40, Ink);
            Anchor(muscleValue.gameObject, 0.07f, 0.22f, 0.68f, 0.72f);
            muscleValue.alignment = TextAlignmentOptions.MidlineLeft;
            var muscleSub = Label(muscleCard, "MuscleSubText", "stage 1 of 5", 18, Ink);
            Anchor(muscleSub.gameObject, 0.07f, 0.05f, 0.90f, 0.28f);
            muscleSub.alignment = TextAlignmentOptions.MidlineLeft;
            var phaseBadge = Card(muscleCard, "PhaseBadgeBG", SoftBlack, new Vector2(118f, 40f));
            SetTopRight(phaseBadge, 18f, 18f);
            var phaseText = Label(phaseBadge, "PhaseBadgeText", "WARMUP", 16, White);
            Stretch(phaseText.gameObject);

            var staminaCard = Card(topLeft, "StaminaCard", Blue, new Vector2(360f, 110f));
            AddLayout(staminaCard, 360f, 110f);
            var staminaTitle = Label(staminaCard, "StaminaTitle", "ENERGY", 22, White);
            Anchor(staminaTitle.gameObject, 0.07f, 0.62f, 0.45f, 0.92f);
            staminaTitle.alignment = TextAlignmentOptions.MidlineLeft;
            var staminaPercent = Label(staminaCard, "StaminaPercent", "100%", 40, White);
            Anchor(staminaPercent.gameObject, 0.60f, 0.22f, 0.92f, 0.78f);
            staminaPercent.alignment = TextAlignmentOptions.MidlineRight;
            var (staminaBar, staminaFill) = Bar(staminaCard, "StaminaBar", Lime, 0.07f, 0.12f, 0.92f, 0.28f);

            var centerTop = Child(workoutPanel, "CenterTopCluster");
            SetAnchorTop(centerTop, -28f, new Vector2(420f, 130f));

            var multiplierCard = Card(centerTop, "MultiplierCard", Gold, new Vector2(320f, 110f));
            SetAnchorCenter(multiplierCard, new Vector2(0f, 0f), new Vector2(320f, 110f));
            var multiplierText = Label(multiplierCard, "MultiplierText", "x1.0", 76, Ink);
            Anchor(multiplierText.gameObject, 0.08f, 0.18f, 0.92f, 0.90f);
            multiplierText.fontStyle = FontStyles.Bold;
            var speedText = Label(multiplierCard, "TrainingSpeedText", "0.00 CLICKS / SEC", 18, Ink);
            Anchor(speedText.gameObject, 0.08f, 0.03f, 0.92f, 0.28f);

            var rightTop = Child(workoutPanel, "RightTopCluster");
            SetTopRight(rightTop, 24f, 24f);
            rightTop.GetComponent<RectTransform>().sizeDelta = new Vector2(380f, 244f);
            rightTop.AddComponent<VerticalLayoutGroup>().spacing = 16f;

            var progressCard = Card(rightTop, "ProgressCard", SoftBlack, new Vector2(380f, 118f));
            AddLayout(progressCard, 380f, 118f);
            var setInfo = Label(progressCard, "SetInfoText", "SET 1 OF 5", 28, White);
            Anchor(setInfo.gameObject, 0.07f, 0.54f, 0.93f, 0.88f);
            setInfo.alignment = TextAlignmentOptions.MidlineLeft;
            var setProgressText = Label(progressCard, "SetProgressText", "0/15", 24, Gold);
            Anchor(setProgressText.gameObject, 0.70f, 0.56f, 0.93f, 0.85f);
            setProgressText.alignment = TextAlignmentOptions.MidlineRight;
            var (setProgressBar, setProgressFill) = Bar(progressCard, "SetProgressBar", Gold, 0.07f, 0.24f, 0.93f, 0.40f);
            var dotsRow = Child(progressCard, "DotsRow");
            Anchor(dotsRow, 0.07f, 0.03f, 0.93f, 0.18f);
            var dotsLayout = dotsRow.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 12f;
            dotsLayout.childAlignment = TextAnchor.MiddleLeft;
            dotsLayout.childForceExpandHeight = false;
            dotsLayout.childForceExpandWidth = false;

            var fatigueCard = Card(rightTop, "FatigueCard", SoftBlack, new Vector2(380f, 110f));
            AddLayout(fatigueCard, 380f, 110f);
            var fatigueTitle = Label(fatigueCard, "FatigueTitle", "PUMP ZONE", 22, White);
            Anchor(fatigueTitle.gameObject, 0.07f, 0.62f, 0.48f, 0.92f);
            fatigueTitle.alignment = TextAlignmentOptions.MidlineLeft;
            var fatiguePercent = Label(fatigueCard, "FatiguePercent", "0%", 38, Red);
            Anchor(fatiguePercent.gameObject, 0.64f, 0.20f, 0.92f, 0.82f);
            fatiguePercent.alignment = TextAlignmentOptions.MidlineRight;
            var (fatigueBar, fatigueFill) = Bar(fatigueCard, "FatigueBar", Red, 0.07f, 0.12f, 0.93f, 0.28f);

            var clickButton = ButtonCard(workoutPanel, "ClickButton", "TRAIN", Gold, Ink, new Vector2(500f, 132f));
            SetAnchorBottom(clickButton.gameObject, 38f, new Vector2(500f, 132f));
            var clickButtonFill = clickButton.transform.Find("Fill").GetComponent<Image>();

            var clickCaption = Label(clickButton.gameObject, "ClickCaption", "TAP FAST TO BUILD MUSCLE", 20, Ink);
            Anchor(clickCaption.gameObject, 0.08f, 0.12f, 0.92f, 0.30f);

            var restPanel = Child(root, "RestPanel");
            Stretch(restPanel);
            restPanel.SetActive(false);
            var restShade = screenOverlay.AddComponent<LayoutElement>();
            restShade.ignoreLayout = true;

            var restCard = Card(restPanel, "RestCard", SoftBlack, new Vector2(620f, 280f));
            SetAnchorCenter(restCard, Vector2.zero, new Vector2(620f, 280f));
            var restHeader = Label(restCard, "RestHeaderText", "REST", 44, White);
            Anchor(restHeader.gameObject, 0.08f, 0.70f, 0.92f, 0.92f);
            var restMuscle = Label(restCard, "RestMuscleText", "+10 MUSCLE", 36, Gold);
            Anchor(restMuscle.gameObject, 0.08f, 0.46f, 0.92f, 0.68f);
            var (restTimerBar, _) = Bar(restCard, "RestTimerBar", Blue, 0.08f, 0.24f, 0.92f, 0.34f);
            var restTimerText = Label(restCard, "RestTimerText", "8s", 28, White);
            Anchor(restTimerText.gameObject, 0.08f, 0.08f, 0.92f, 0.22f);

            var resultsPanel = Child(root, "ResultsPanel");
            Stretch(resultsPanel);
            resultsPanel.SetActive(false);
            var resultsCard = Card(resultsPanel, "ResultsCard", Gold, new Vector2(700f, 420f));
            SetAnchorCenter(resultsCard, Vector2.zero, new Vector2(700f, 420f));
            var resultsTitle = Label(resultsCard, "ResultsTitleText", "WORKOUT COMPLETE", 42, Ink);
            Anchor(resultsTitle.gameObject, 0.06f, 0.80f, 0.94f, 0.94f);
            var resultsDay = Label(resultsCard, "ResultsDayText", "CHEST DAY", 28, Ink);
            Anchor(resultsDay.gameObject, 0.06f, 0.62f, 0.94f, 0.76f);
            var resultsSets = Label(resultsCard, "ResultsSetsText", "SETS: 5/5", 28, Ink);
            Anchor(resultsSets.gameObject, 0.06f, 0.48f, 0.94f, 0.60f);
            var resultsMuscle = Label(resultsCard, "ResultsMuscleText", "MUSCLE GAINED: +50", 30, Ink);
            Anchor(resultsMuscle.gameObject, 0.06f, 0.32f, 0.94f, 0.46f);
            var resultsRecord = Label(resultsCard, "ResultsRecordText", "NEW RECORD!", 34, Red);
            Anchor(resultsRecord.gameObject, 0.06f, 0.18f, 0.94f, 0.30f);
            var doneButton = ButtonCard(resultsCard, "DoneButton", "CLAIM", Ink, White, new Vector2(240f, 78f));
            SetAnchorBottom(doneButton.gameObject, 26f, new Vector2(240f, 78f));

            var tabWorkout = HiddenButton(root, "TabWorkout");
            var tabRest = HiddenButton(root, "TabRest");
            var tabResults = HiddenButton(root, "TabResults");

            var so = new SerializedObject(hud);
            so.Update();
            SetRef(so, "rootPanel", root);
            SetRef(so, "workoutPanel", workoutPanel);
            SetRef(so, "restPanel", restPanel);
            SetRef(so, "resultsPanel", resultsPanel);
            SetRef(so, "hintPanel", hint);
            SetRef(so, "tabWorkout", tabWorkout);
            SetRef(so, "tabRest", tabRest);
            SetRef(so, "tabResults", tabResults);
            SetRef(so, "muscleValueText", muscleValue);
            SetRef(so, "muscleSubText", muscleSub);
            SetRef(so, "phaseBadgeText", phaseText);
            SetRef(so, "phaseBadgeBG", phaseBadge.transform.Find("Fill").GetComponent<Image>());
            SetRef(so, "staminaBar", staminaBar);
            SetRef(so, "staminaPercent", staminaPercent);
            SetRef(so, "staminaFill", staminaFill);
            SetRef(so, "setProgressBar", setProgressBar);
            SetRef(so, "setProgressText", setProgressText);
            SetRef(so, "setProgressFill", setProgressFill);
            SetRef(so, "fatigueBar", fatigueBar);
            SetRef(so, "fatiguePercent", fatiguePercent);
            SetRef(so, "fatigueFill", fatigueFill);
            SetRef(so, "clickButton", clickButton);
            SetRef(so, "clickButtonBG", clickButtonFill);
            SetRef(so, "setDotsParent", dotsRow.transform);
            SetRef(so, "setDotPrefab", DotPrefab());
            SetRef(so, "setInfoText", setInfo);
            SetRef(so, "hintText", hintText);
            SetRef(so, "multiplierText", multiplierText);
            SetRef(so, "trainingSpeedText", speedText);
            SetRef(so, "restHeaderText", restHeader);
            SetRef(so, "restMuscleText", restMuscle);
            SetRef(so, "restTimerBar", restTimerBar);
            SetRef(so, "restTimerText", restTimerText);
            SetRef(so, "resultsTitleText", resultsTitle);
            SetRef(so, "resultsDayText", resultsDay);
            SetRef(so, "resultsSetsText", resultsSets);
            SetRef(so, "resultsMuscleText", resultsMuscle);
            SetRef(so, "resultsRecordText", resultsRecord);
            SetRef(so, "doneButton", doneButton);
            SetRef(so, "screenOverlay", screenOverlayImage);
            so.ApplyModifiedProperties();

            SetPrivateColor(hud, "tabActiveColor", GoldDark);
            SetPrivateColor(hud, "tabInactiveColor", SoftBlack);
            SetPrivateColor(hud, "warmupColor", Lime);
            SetPrivateColor(hud, "workZoneColor", Gold);
            SetPrivateColor(hud, "failColor", Red);

            var workout = Object.FindFirstObjectByType<Tutorial.DumbbellWorkout>();
            if (workout != null)
            {
                var pSo = new SerializedObject(workout);
                pSo.Update();
                SetRef(pSo, "workoutHUD", hud);
                pSo.ApplyModifiedProperties();
            }

            Selection.activeGameObject = canvasGO;
            Debug.Log("Muscle Transform style HUD rebuilt.");
        }

        static GameObject Card(GameObject parent, string name, Color fillColor, Vector2 size)
        {
            var root = Child(parent, name);
            root.GetComponent<RectTransform>().sizeDelta = size;

            var shadow = Child(root, "Shadow");
            Stretch(shadow);
            shadow.GetComponent<RectTransform>().anchoredPosition = new Vector2(6f, -8f);
            shadow.AddComponent<Image>().color = Shadow;

            var fill = Child(root, "Fill");
            Stretch(fill);
            fill.AddComponent<Image>().color = fillColor;
            return root;
        }

        static (Slider, Image) Bar(GameObject parent, string name, Color fillColor, float x0, float y0, float x1, float y1)
        {
            var track = Child(parent, name);
            Anchor(track, x0, y0, x1, y1);
            track.AddComponent<Image>().color = Gray;

            var slider = track.AddComponent<Slider>();
            slider.interactable = false;
            slider.direction = Slider.Direction.LeftToRight;

            var fillArea = Child(track, "FillArea");
            Stretch(fillArea);
            fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(6f, 6f);
            fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-6f, -6f);

            var fill = Child(fillArea, "Fill");
            Stretch(fill);
            var image = fill.AddComponent<Image>();
            image.color = fillColor;

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = track.GetComponent<Image>();
            slider.value = 0f;

            return (slider, image);
        }

        static Button ButtonCard(GameObject parent, string name, string text, Color fillColor, Color textColor, Vector2 size)
        {
            var card = Card(parent, name, fillColor, size);
            var hitbox = card.AddComponent<Image>();
            hitbox.color = new Color(1f, 1f, 1f, 0.001f);
            hitbox.raycastTarget = true;

            var button = card.AddComponent<Button>();
            button.targetGraphic = hitbox;
            var label = Label(card, "Label", text, 42, textColor);
            Anchor(label.gameObject, 0.08f, 0.30f, 0.92f, 0.82f);
            return button;
        }

        static Button HiddenButton(GameObject parent, string name)
        {
            var go = Child(parent, name);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1f, 1f);
            var image = go.AddComponent<Image>();
            image.color = Color.clear;
            return go.AddComponent<Button>();
        }

        static TextMeshProUGUI Label(GameObject parent, string name, string text, int size, Color color)
        {
            var go = Child(parent, name);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }

        static void AddLayout(GameObject go, float width, float height)
        {
            var layout = go.AddComponent<LayoutElement>();
            if (width >= 0f)
                layout.preferredWidth = width;
            if (height >= 0f)
                layout.preferredHeight = height;
        }

        static GameObject Child(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        static void Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void Anchor(GameObject go, float x0, float y0, float x1, float y1)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void SetAnchorBottom(GameObject go, float y, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = size;
        }

        static void SetAnchorTop(GameObject go, float y, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = size;
        }

        static void SetAnchorCenter(GameObject go, Vector2 offset, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = size;
        }

        static void SetTopLeft(GameObject go, float x, float y)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
        }

        static void SetTopRight(GameObject go, float x, float y)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-x, -y);
        }

        static void SetRef(SerializedObject so, string field, Object value)
        {
            var property = so.FindProperty(field);
            if (property != null)
                property.objectReferenceValue = value;
        }

        static void SetPrivateColor(WorkoutHUD hud, string name, Color color)
        {
            hud.GetType()
                .GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(hud, color);
        }

        static GameObject DotPrefab()
        {
            const string path = "Assets/KachokGame/SetDotPrefab.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                return prefab;

            var go = new GameObject("SetDot");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(24f, 24f);
            go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
            AddLayout(go, 24f, 24f);
            return PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
        }
    }
}
