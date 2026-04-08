using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    public class WorkoutHUD : MonoBehaviour
    {
        [Header("Root Panels")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private GameObject workoutPanel;
        [SerializeField] private GameObject restPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject hintPanel;

        [Header("Tabs")]
        [SerializeField] private Button tabWorkout;
        [SerializeField] private Button tabRest;
        [SerializeField] private Button tabResults;
        [SerializeField] private Color tabActiveColor = new(0.20f, 0.20f, 0.25f, 1f);
        [SerializeField] private Color tabInactiveColor = new(0.12f, 0.12f, 0.15f, 1f);

        [Header("Muscle Header")]
        [SerializeField] private TextMeshProUGUI muscleValueText;
        [SerializeField] private TextMeshProUGUI muscleSubText;
        [SerializeField] private TextMeshProUGUI phaseBadgeText;
        [SerializeField] private Image phaseBadgeBG;

        [Header("Stamina")]
        [SerializeField] private Slider staminaBar;
        [SerializeField] private TextMeshProUGUI staminaPercent;
        [SerializeField] private Image staminaFill;

        [Header("Set Progress")]
        [SerializeField] private Slider setProgressBar;
        [SerializeField] private TextMeshProUGUI setProgressText;
        [SerializeField] private Image setProgressFill;

        [Header("Fatigue")]
        [SerializeField] private Slider fatigueBar;
        [SerializeField] private TextMeshProUGUI fatiguePercent;
        [SerializeField] private Image fatigueFill;

        [Header("Click Button")]
        [SerializeField] private Button clickButton;
        [SerializeField] private Image clickButtonBG;

        [Header("Sets")]
        [SerializeField] private Transform setDotsParent;
        [SerializeField] private GameObject setDotPrefab;
        [SerializeField] private TextMeshProUGUI setInfoText;

        [Header("Hint")]
        [SerializeField] private TextMeshProUGUI hintText;

        [Header("Tempo")]
        [SerializeField] private TextMeshProUGUI multiplierText;
        [SerializeField] private TextMeshProUGUI trainingSpeedText;

        [Header("Rest")]
        [SerializeField] private TextMeshProUGUI restHeaderText;
        [SerializeField] private TextMeshProUGUI restMuscleText;
        [SerializeField] private Slider restTimerBar;
        [SerializeField] private TextMeshProUGUI restTimerText;

        [Header("Results")]
        [SerializeField] private TextMeshProUGUI resultsTitleText;
        [SerializeField] private TextMeshProUGUI resultsDayText;
        [SerializeField] private TextMeshProUGUI resultsSetsText;
        [SerializeField] private TextMeshProUGUI resultsMuscleText;
        [SerializeField] private TextMeshProUGUI resultsRecordText;
        [SerializeField] private Button doneButton;

        [Header("Phase Colors")]
        [SerializeField] private Color warmupColor = new(0.27f, 0.75f, 0.44f, 1f);
        [SerializeField] private Color workZoneColor = new(0.47f, 0.33f, 0.87f, 1f);
        [SerializeField] private Color failColor = new(0.87f, 0.22f, 0.22f, 1f);

        [Header("Effects")]
        [SerializeField] private Image screenOverlay;
        [SerializeField] private float shakeAmount = 6f;
        [SerializeField] private float shakeDuration = 0.12f;

        private Action _onClickCallback;
        private Coroutine _restCo;
        private Coroutine _shakeCo;
        private Coroutine _pulseCo;
        private RectTransform _clickButtonRect;
        private Vector2 _btnOrigin;
        private float _fatigueSmooth;
        private bool _peakTempoFxActive;

        private WorkoutSession _session;
        private StaminaSystem _stamina;
        private BodyMorphSystem _body;
        private bool _sessionBound;
        private bool _staminaBound;
        private bool _bodyBound;

        public bool BlocksWorkoutInput =>
            (restPanel != null && restPanel.activeSelf) ||
            (resultsPanel != null && resultsPanel.activeSelf);

        private void Awake()
        {
            if (rootPanel != null)
                rootPanel.SetActive(false);
            if (hintPanel != null)
                hintPanel.SetActive(false);

            if (clickButton != null)
            {
                _clickButtonRect = clickButton.GetComponent<RectTransform>();
                if (_clickButtonRect != null)
                    _btnOrigin = _clickButtonRect.anchoredPosition;

                clickButton.onClick.AddListener(() => _onClickCallback?.Invoke());
            }

            if (doneButton != null)
                doneButton.onClick.AddListener(HideAll);

            if (tabWorkout != null)
                tabWorkout.onClick.AddListener(() => SwitchTab(0));
            if (tabRest != null)
                tabRest.onClick.AddListener(() => SwitchTab(1));
            if (tabResults != null)
                tabResults.onClick.AddListener(() => SwitchTab(2));
        }

        private void Start()
        {
            EnsureBindings();
            RefreshAll();
        }

        private void Update()
        {
            if (!_sessionBound || !_staminaBound || !_bodyBound)
                EnsureBindings();
        }

        private void OnDestroy()
        {
            if (_session != null && _sessionBound)
            {
                _session.OnSessionStarted -= OnSessionStarted;
                _session.OnClickRegistered -= OnClickRegistered;
                _session.OnPhaseChanged -= OnPhaseChanged;
                _session.OnSetCompleted -= OnSetCompleted;
                _session.OnSessionCompleted -= OnSessionCompleted;
            }

            if (_stamina != null && _staminaBound)
            {
                _stamina.OnStaminaChanged -= OnStaminaChanged;
                _stamina.OnStaminaDepleted -= OnStaminaDepleted;
            }

            if (_body != null && _bodyBound)
            {
                _body.OnMusclePointsChanged -= OnMusclePointsChanged;
                _body.OnStageUnlocked -= OnStageUnlocked;
            }
        }

        public void SetClickCallback(Action cb) => _onClickCallback = cb;

        public void ShowHint(string text)
        {
            Debug.Log($"[WorkoutHUD] Показ подсказки: {text}");
            if (hintPanel != null)
                hintPanel.SetActive(true);
            if (hintText != null)
                hintText.text = text;
        }

        public void SetTempo(float clicksPerSecond, float multiplier)
        {
            if (trainingSpeedText != null)
                trainingSpeedText.text = $"Training Speed: {clicksPerSecond:0.00}";

            if (multiplierText != null)
                multiplierText.text = $"x{multiplier:0.0}";

            if (clickButtonBG != null)
            {
                float pulse = Mathf.InverseLerp(1f, 8f, multiplier);
                clickButtonBG.color = Color.Lerp(
                    new Color(1f, 0.78f, 0.08f, 1f),
                    new Color(1f, 0.35f, 0.05f, 1f),
                    pulse);
            }
        }

        public void ResetTempo()
        {
            SetTempo(0f, 1f);
            SetPeakTempoState(false, 0f);
        }

        public void SetPeakTempoState(bool active, float intensity)
        {
            if (active)
            {
                if (!_peakTempoFxActive)
                {
                    _peakTempoFxActive = true;
                    if (_pulseCo != null)
                        StopCoroutine(_pulseCo);
                    _pulseCo = StartCoroutine(PulseOverlay(Mathf.Clamp01(intensity)));
                }
            }
            else
            {
                _peakTempoFxActive = false;
                StopFailFX();
            }
        }

        public void HideHint()
        {
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }

        public void ShowWorkout()
        {
            Debug.Log("[WorkoutHUD] Включение интерфейса тренировки (ShowWorkout)");
            EnsureBindings();

            if (rootPanel == null)
            {
                Debug.LogError("[WorkoutHUD] RootPanel не назначен.");
                return;
            }

            ResetClickButtonPosition();
            ResetTempo();
            rootPanel.SetActive(true);
            SwitchTab(0);
            BuildSetDots();
            RefreshAll();
        }

        public void HideAll()
        {
            Debug.Log("[WorkoutHUD] Скрытие всего интерфейса");
            ResetClickButtonPosition();
            ResetTempo();
            if (rootPanel != null)
                rootPanel.SetActive(false);
        }

        private void EnsureBindings()
        {
            if (!_sessionBound)
            {
                _session = WorkoutSession.Instance;
                if (_session != null)
                {
                    _session.OnSessionStarted += OnSessionStarted;
                    _session.OnClickRegistered += OnClickRegistered;
                    _session.OnPhaseChanged += OnPhaseChanged;
                    _session.OnSetCompleted += OnSetCompleted;
                    _session.OnSessionCompleted += OnSessionCompleted;
                    _sessionBound = true;
                }
            }

            if (!_staminaBound)
            {
                _stamina = StaminaSystem.Instance;
                if (_stamina != null)
                {
                    _stamina.OnStaminaChanged += OnStaminaChanged;
                    _stamina.OnStaminaDepleted += OnStaminaDepleted;
                    _staminaBound = true;
                    SetStaminaBar(_stamina.StaminaRatio);
                }
            }

            if (!_bodyBound)
            {
                _body = BodyMorphSystem.Instance;
                if (_body != null)
                {
                    _body.OnMusclePointsChanged += OnMusclePointsChanged;
                    _body.OnStageUnlocked += OnStageUnlocked;
                    _bodyBound = true;
                    RefreshMuscleHeader();
                }
            }
        }

        private void OnStaminaChanged(float current, float max)
        {
            if (max > 0f)
                SetStaminaBar(current / max);
        }

        private void OnStaminaDepleted()
        {
            StartCoroutine(CollapseEffect());
        }

        private void OnMusclePointsChanged(int _)
        {
            RefreshMuscleHeader();
        }

        private void OnStageUnlocked(int _, BodyStage stage)
        {
            Debug.Log($"[HUD] Разблокирована стадия: {stage.name}");
        }

        private void OnSessionStarted()
        {
            BuildSetDots();
            RefreshAll();
        }

        private void OnClickRegistered(int cur, int max)
        {
            SetProgressBar(cur, max);
            SmoothFatigue(cur, max);

            if (_shakeCo != null)
                StopCoroutine(_shakeCo);
            _shakeCo = StartCoroutine(ShakeBtn());
        }

        private void OnPhaseChanged(WorkoutPhase phase)
        {
            ApplyPhaseBadge(phase);
        }

        private void OnSetCompleted(int setNum, int total, int muscle)
        {
            UpdateSetDots(setNum - 1);
            RefreshSetInfo();
            SetProgressBar(0, _session != null ? _session.ClicksPerSet : 0);
            SmoothFatigue(0, 1);

            if (setNum < total)
            {
                SwitchTab(1);
                if (restHeaderText != null)
                    restHeaderText.text = $"Подход {setNum} из {total} завершен!";
                if (restMuscleText != null)
                    restMuscleText.text = $"+{muscle} мышц";
                if (_restCo != null)
                    StopCoroutine(_restCo);
                _restCo = StartCoroutine(RestCountdown(8f));
            }
        }

        private void OnSessionCompleted(WorkoutResults results)
        {
            if (_restCo != null)
                StopCoroutine(_restCo);

            SwitchTab(2);

            string day = results.dayPlan switch
            {
                DayPlan.Chest => "День груди",
                DayPlan.Back => "День спины",
                DayPlan.Legs => "День ног",
                _ => ""
            };

            if (resultsTitleText != null)
                resultsTitleText.text = "ТРЕНИРОВКА ЗАВЕРШЕНА!";
            if (resultsDayText != null)
                resultsDayText.text = day;
            if (resultsSetsText != null)
                resultsSetsText.text = $"Подходы: {results.setsCompleted}/{results.totalSets}";
            if (resultsMuscleText != null)
                resultsMuscleText.text = $"Получено мышц: +{results.muscleGained}";
            if (resultsRecordText != null)
            {
                resultsRecordText.gameObject.SetActive(results.isNewRecord);
                resultsRecordText.text = "НОВЫЙ РЕКОРД!";
            }
        }

        private void RefreshAll()
        {
            EnsureBindings();

            if (_stamina != null)
                SetStaminaBar(_stamina.StaminaRatio);

            RefreshMuscleHeader();

            if (_session != null)
            {
                SetProgressBar(_session.CurrentClicks, _session.ClicksPerSet);
                ApplyPhaseBadge(_session.CurrentPhase);
                RefreshSetInfo();
                UpdateSetDots(_session.CurrentSet - 1);
            }
        }

        private void SetStaminaBar(float ratio)
        {
            if (staminaBar != null)
                staminaBar.value = ratio;
            if (staminaPercent != null)
                staminaPercent.text = $"{Mathf.RoundToInt(ratio * 100)}%";
            if (staminaFill != null)
                staminaFill.color = Color.Lerp(failColor, warmupColor, ratio);
        }

        private void SetProgressBar(int cur, int max)
        {
            float ratio = max > 0 ? (float)cur / max : 0f;
            if (setProgressBar != null)
                setProgressBar.value = ratio;
            if (setProgressText != null)
                setProgressText.text = $"{cur}/{max}";
        }

        private void SmoothFatigue(int cur, int max)
        {
            float target = max > 0 ? (float)cur / max : 0f;
            _fatigueSmooth = Mathf.Lerp(_fatigueSmooth, target, 0.35f);

            if (fatigueBar != null)
                fatigueBar.value = _fatigueSmooth;
            if (fatiguePercent != null)
                fatiguePercent.text = $"{Mathf.RoundToInt(_fatigueSmooth * 100)}%";
            if (fatigueFill != null)
                fatigueFill.color = Color.Lerp(warmupColor, failColor, _fatigueSmooth);
        }

        private void RefreshMuscleHeader()
        {
            if (_body == null)
                return;

            if (muscleValueText != null)
                muscleValueText.text = $"{_body.MusclePoints}<size=70%>/{_body.NextStageMuscle}</size>";
            if (muscleSubText != null)
                muscleSubText.text = $"stage {_body.CurrentStageIndex + 1} of {_body.StageCount}";
        }

        private void ApplyPhaseBadge(WorkoutPhase phase)
        {
            (string label, Color color) = phase switch
            {
                WorkoutPhase.Warmup => ("WARMUP", warmupColor),
                WorkoutPhase.WorkZone => ("WORK ZONE", workZoneColor),
                _ => ("FAIL", failColor)
            };

            if (phaseBadgeText != null)
                phaseBadgeText.text = label;
            if (phaseBadgeBG != null)
                phaseBadgeBG.color = color;
            if (setProgressFill != null)
                setProgressFill.color = color;
        }

        private void RefreshSetInfo()
        {
            if (_session == null || setInfoText == null)
                return;

            int displaySet = Mathf.Clamp(_session.CurrentSet + 1, 1, _session.TotalSets);
            setInfoText.text = $"SET {displaySet} OF {_session.TotalSets}";
        }

        private void BuildSetDots()
        {
            if (setDotsParent == null || setDotPrefab == null)
                return;

            foreach (Transform child in setDotsParent)
                Destroy(child.gameObject);

            int count = _session != null ? _session.TotalSets : 5;
            for (int i = 0; i < count; i++)
            {
                var dot = Instantiate(setDotPrefab, setDotsParent);
                var img = dot.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(1f, 1f, 1f, 0.25f);
            }
        }

        private void UpdateSetDots(int completedIdx)
        {
            if (setDotsParent == null)
                return;

            for (int i = 0; i < setDotsParent.childCount; i++)
            {
                var img = setDotsParent.GetChild(i).GetComponent<Image>();
                if (img != null)
                    img.color = i <= completedIdx ? workZoneColor : new Color(1f, 1f, 1f, 0.25f);
            }
        }

        private void SwitchTab(int idx)
        {
            if (workoutPanel != null)
                workoutPanel.SetActive(idx == 0);
            if (restPanel != null)
                restPanel.SetActive(idx == 1);
            if (resultsPanel != null)
                resultsPanel.SetActive(idx == 2);

            SetTabStyle(tabWorkout, idx == 0);
            SetTabStyle(tabRest, idx == 1);
            SetTabStyle(tabResults, idx == 2);
        }

        private void SetTabStyle(Button button, bool active)
        {
            if (button == null)
                return;

            var img = button.GetComponent<Image>();
            if (img == null)
            {
                Transform fill = button.transform.Find("Fill");
                if (fill != null)
                    img = fill.GetComponent<Image>();
            }

            if (img != null)
                img.color = active ? tabActiveColor : tabInactiveColor;
        }

        private IEnumerator RestCountdown(float duration)
        {
            float remaining = duration;
            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                if (restTimerBar != null)
                    restTimerBar.value = remaining / duration;
                if (restTimerText != null)
                    restTimerText.text = $"{Mathf.CeilToInt(remaining)}s";
                yield return null;
            }

            if (_session != null && _session.IsActive)
                _session.EndSession();
        }

        private IEnumerator ShakeBtn()
        {
            if (_clickButtonRect == null)
                yield break;

            float t = 0f;
            while (t < shakeDuration)
            {
                float x = UnityEngine.Random.Range(-shakeAmount, shakeAmount);
                float y = UnityEngine.Random.Range(-shakeAmount, shakeAmount);
                _clickButtonRect.anchoredPosition = _btnOrigin + new Vector2(x, y);
                t += Time.deltaTime;
                yield return null;
            }

            ResetClickButtonPosition();
        }

        private void ResetClickButtonPosition()
        {
            if (_clickButtonRect != null)
                _clickButtonRect.anchoredPosition = _btnOrigin;
        }

        private void StopFailFX()
        {
            if (_pulseCo != null)
            {
                StopCoroutine(_pulseCo);
                _pulseCo = null;
            }

            if (screenOverlay != null)
                screenOverlay.color = Color.clear;
        }

        private IEnumerator PulseOverlay(float intensity)
        {
            float alpha = Mathf.Lerp(0.08f, 0.18f, intensity);
            float speed = Mathf.Lerp(3.5f, 6f, intensity);
            Color red = new(1f, 0.14f, 0.14f, alpha);
            while (true)
            {
                float t = Mathf.PingPong(Time.time * speed, 1f);
                if (screenOverlay != null)
                    screenOverlay.color = Color.Lerp(Color.clear, red, t);
                yield return null;
            }
        }

        private IEnumerator CollapseEffect()
        {
            if (screenOverlay != null)
                screenOverlay.color = new Color(0f, 0f, 0f, 0.75f);
            yield return new WaitForSeconds(1.5f);
            if (screenOverlay != null)
                screenOverlay.color = Color.clear;
        }
    }
}
