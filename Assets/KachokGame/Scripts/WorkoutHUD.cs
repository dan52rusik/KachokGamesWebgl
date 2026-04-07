using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tutorial
{
    /// <summary>
    /// Полный HUD тренировки. 3 вкладки: Тренировка → Отдых → Результат.
    /// Подписывается на WorkoutSession, StaminaSystem, BodyMorphSystem.
    /// Принцип: минимум в покое, максимум во время действия.
    /// </summary>
    public class WorkoutHUD : MonoBehaviour
    {
        // ── Панели ───────────────────────────────────────────────
        [Header("Корневые панели")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private GameObject workoutPanel;
        [SerializeField] private GameObject restPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject hintPanel;

        // ── Вкладки ──────────────────────────────────────────────
        [Header("Вкладки")]
        [SerializeField] private Button tabWorkout;
        [SerializeField] private Button tabRest;
        [SerializeField] private Button tabResults;
        [SerializeField] private Color tabActiveColor   = new Color(0.20f, 0.20f, 0.25f, 1f);
        [SerializeField] private Color tabInactiveColor = new Color(0.12f, 0.12f, 0.15f, 1f);

        // ── Шапка ────────────────────────────────────────────────
        [Header("Шапка мышцы")]
        [SerializeField] private TextMeshProUGUI muscleValueText; // "240/500"
        [SerializeField] private TextMeshProUGUI muscleSubText;   // "мышца · стадия 2 из 5"
        [SerializeField] private TextMeshProUGUI phaseBadgeText;  // "Рабочая зона"
        [SerializeField] private Image           phaseBadgeBG;

        // ── Шкалы ────────────────────────────────────────────────
        [Header("Шкала выносливости")]
        [SerializeField] private Slider          staminaBar;
        [SerializeField] private TextMeshProUGUI staminaPercent;
        [SerializeField] private Image           staminaFill;

        [Header("Прогресс подхода")]
        [SerializeField] private Slider          setProgressBar;
        [SerializeField] private TextMeshProUGUI setProgressText;
        [SerializeField] private Image           setProgressFill;

        [Header("Усталость мышц")]
        [SerializeField] private Slider          fatigueBar;
        [SerializeField] private TextMeshProUGUI fatiguePercent;
        [SerializeField] private Image           fatigueFill;

        // ── Кнопка-мышца ─────────────────────────────────────────
        [Header("Кнопка клика")]
        [SerializeField] private Button          clickButton;
        [SerializeField] private Image           clickButtonBG;

        // ── Подходы ──────────────────────────────────────────────
        [Header("Подходы")]
        [SerializeField] private Transform       setDotsParent;
        [SerializeField] private GameObject      setDotPrefab;
        [SerializeField] private TextMeshProUGUI setInfoText;   // "Подход 3 из 5 · Бицепс-кёрл"

        // ── Подсказка ─────────────────────────────────────────────
        [Header("Подсказка")]
        [SerializeField] private TextMeshProUGUI hintText;

        [Header("Темп тренировки")]
        [SerializeField] private TextMeshProUGUI multiplierText;
        [SerializeField] private TextMeshProUGUI trainingSpeedText;

        // ── Панель отдыха ─────────────────────────────────────────
        [Header("Отдых")]
        [SerializeField] private TextMeshProUGUI restHeaderText;
        [SerializeField] private TextMeshProUGUI restMuscleText;
        [SerializeField] private Slider          restTimerBar;
        [SerializeField] private TextMeshProUGUI restTimerText;

        // ── Панель результата ─────────────────────────────────────
        [Header("Результат")]
        [SerializeField] private TextMeshProUGUI resultsTitleText;
        [SerializeField] private TextMeshProUGUI resultsDayText;
        [SerializeField] private TextMeshProUGUI resultsSetsText;
        [SerializeField] private TextMeshProUGUI resultsMuscleText;
        [SerializeField] private TextMeshProUGUI resultsRecordText;
        [SerializeField] private Button          doneButton;

        // ── Цвета фаз ─────────────────────────────────────────────
        [Header("Цвета фаз")]
        [SerializeField] private Color warmupColor   = new Color(0.27f, 0.75f, 0.44f, 1f);
        [SerializeField] private Color workZoneColor = new Color(0.47f, 0.33f, 0.87f, 1f);
        [SerializeField] private Color failColor     = new Color(0.87f, 0.22f, 0.22f, 1f);

        // ── Эффекты ───────────────────────────────────────────────
        [Header("Эффекты")]
        [SerializeField] private Image screenOverlay;
        [SerializeField] private float shakeAmount   = 6f;
        [SerializeField] private float shakeDuration = 0.12f;

        // ── Внутреннее состояние ──────────────────────────────────
        private Action   _onClickCallback;
        private Coroutine _restCo;
        private Coroutine _shakeCo;
        private Coroutine _pulseCo;
        private RectTransform _clickButtonRect;
        private Vector2  _btnOrigin;
        private float    _fatigueSmooth;

        private WorkoutSession  _session;
        private StaminaSystem   _stamina;
        private BodyMorphSystem _body;

        // ─────────────────────────────────────────────────────────
        private void Awake()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
            if (hintPanel != null) hintPanel.SetActive(false);

            if (clickButton != null)
            {
                _clickButtonRect = clickButton.GetComponent<RectTransform>();
                if (_clickButtonRect != null)
                    _btnOrigin = _clickButtonRect.anchoredPosition;
                clickButton.onClick.AddListener(() => _onClickCallback?.Invoke());
            }
            if (doneButton != null) doneButton.onClick.AddListener(HideAll);

            if (tabWorkout != null) tabWorkout.onClick.AddListener(() => SwitchTab(0));
            if (tabRest    != null) tabRest.onClick.AddListener(()    => SwitchTab(1));
            if (tabResults != null) tabResults.onClick.AddListener(() => SwitchTab(2));
        }

        private void Start()
        {
            _session = WorkoutSession.Instance;
            _stamina = StaminaSystem.Instance;
            _body    = BodyMorphSystem.Instance;

            if (_session != null)
            {
                _session.OnSessionStarted  += OnSessionStarted;
                _session.OnClickRegistered += OnClickRegistered;
                _session.OnPhaseChanged    += OnPhaseChanged;
                _session.OnSetCompleted    += OnSetCompleted;
                _session.OnSessionCompleted += OnSessionCompleted;
            }
            if (_stamina != null)
            {
                _stamina.OnStaminaChanged  += (cur, max) => SetStaminaBar(cur / max);
                _stamina.OnStaminaDepleted += () => StartCoroutine(CollapseEffect());
            }
            if (_body != null)
            {
                _body.OnMusclePointsChanged += _ => RefreshMuscleHeader();
                _body.OnStageUnlocked       += (i, s) => Debug.Log($"[HUD] Разблокирована стадия: {s.name}");
            }
        }

        // ── Публичный API (вызывается из DumbbellWorkout) ────────
        public void SetClickCallback(Action cb) => _onClickCallback = cb;

        public void ShowHint(string text)
        {
            Debug.Log($"[WorkoutHUD] Показ подсказки: {text}");
            if (hintPanel != null) hintPanel.SetActive(true);
            if (hintText  != null) hintText.text = text;
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
                clickButtonBG.color = Color.Lerp(new Color(1f, 0.78f, 0.08f, 1f), new Color(1f, 0.35f, 0.05f, 1f), pulse);
            }
        }

        public void ResetTempo()
        {
            SetTempo(0f, 1f);
        }

        public void HideHint() 
        { 
            if (hintPanel != null) hintPanel.SetActive(false); 
        }

        public void ShowWorkout()
        {
            Debug.Log("[WorkoutHUD] Включение интерфейса тренировки (ShowWorkout)");
            if (rootPanel != null) 
            {
                ResetClickButtonPosition();
                ResetTempo();
                rootPanel.SetActive(true);
                SwitchTab(0);
                BuildSetDots();
                RefreshAll();
            }
            else
            {
                Debug.LogError("[WorkoutHUD] ОШИБКА: Ссылка на RootPanel пуста! Пересобери HUD через меню.");
            }
        }
        public void HideAll() 
        { 
            Debug.Log("[WorkoutHUD] Скрытие всего интерфейса");
            ResetClickButtonPosition();
            ResetTempo();
            if (rootPanel != null) rootPanel.SetActive(false); 
        }

        // ── Обработчики событий ───────────────────────────────────
        private void OnSessionStarted()
        {
            BuildSetDots();
            RefreshAll();
        }

        private void OnClickRegistered(int cur, int max)
        {
            SetProgressBar(cur, max);
            SmoothFatigue(cur, max);
            if (_shakeCo != null) StopCoroutine(_shakeCo);
            _shakeCo = StartCoroutine(ShakeBtn());
        }

        private void OnPhaseChanged(WorkoutPhase p)
        {
            ApplyPhaseBadge(p);
            if (p == WorkoutPhase.MuscleFail) StartFailFX();
            else                              StopFailFX();
        }

        private void OnSetCompleted(int setNum, int total, int muscle)
        {
            UpdateSetDots(setNum - 1);
            if (setNum < total)
            {
                // Показать отдых только если сессия НЕ завершена
                SwitchTab(1);
                if (restHeaderText  != null) restHeaderText.text = $"Подход {setNum} из {total} завершён!";
                if (restMuscleText  != null) restMuscleText.text = $"+{muscle} МЫШЦ";
                if (_restCo != null) StopCoroutine(_restCo);
                _restCo = StartCoroutine(RestCountdown(8f));
            }
        }

        private void OnSessionCompleted(WorkoutResults r)
        {
            if (_restCo != null) StopCoroutine(_restCo);
            SwitchTab(2);

            string day = r.dayPlan switch
            {
                DayPlan.Chest => "День груди",
                DayPlan.Back  => "День спины",
                DayPlan.Legs  => "День ног",
                _             => ""
            };
            if (resultsTitleText  != null) resultsTitleText.text  = "ТРЕНИРОВКА ЗАВЕРШЕНА!";
            if (resultsDayText    != null) resultsDayText.text    = day;
            if (resultsSetsText   != null) resultsSetsText.text   = $"Подходы: {r.setsCompleted}/{r.totalSets}";
            if (resultsMuscleText != null) resultsMuscleText.text = $"ПОЛУЧЕНО МЫШЦ: +{r.muscleGained}";
            if (resultsRecordText != null)
            {
                resultsRecordText.gameObject.SetActive(r.isNewRecord);
                resultsRecordText.text = "НОВЫЙ РЕКОРД!";
            }
        }

        // ── Обновление дисплеев ───────────────────────────────────
        private void RefreshAll()
        {
            if (_stamina != null) SetStaminaBar(_stamina.StaminaRatio);
            RefreshMuscleHeader();
            if (_session != null)
            {
                SetProgressBar(_session.CurrentClicks, _session.ClicksPerSet);
                ApplyPhaseBadge(_session.CurrentPhase);
                RefreshSetInfo();
            }
        }

        private void SetStaminaBar(float ratio)
        {
            if (staminaBar     != null) staminaBar.value  = ratio;
            if (staminaPercent != null) staminaPercent.text = $"{Mathf.RoundToInt(ratio * 100)}%";
            if (staminaFill    != null) staminaFill.color = Color.Lerp(failColor, warmupColor, ratio);
        }

        private void SetProgressBar(int cur, int max)
        {
            float r = max > 0 ? (float)cur / max : 0f;
            if (setProgressBar  != null) setProgressBar.value = r;
            if (setProgressText != null) setProgressText.text = $"{cur}/{max}";
        }

        private void SmoothFatigue(int cur, int max)
        {
            float target = max > 0 ? (float)cur / max : 0f;
            _fatigueSmooth = Mathf.Lerp(_fatigueSmooth, target, 0.35f);
            if (fatigueBar     != null) fatigueBar.value    = _fatigueSmooth;
            if (fatiguePercent != null) fatiguePercent.text = $"{Mathf.RoundToInt(_fatigueSmooth * 100)}%";
            if (fatigueFill    != null) fatigueFill.color   = Color.Lerp(warmupColor, failColor, _fatigueSmooth);
        }

        private void RefreshMuscleHeader()
        {
            if (_body == null) return;
            if (muscleValueText != null)
                muscleValueText.text = $"{_body.MusclePoints}<size=70%>/{_body.NextStageMuscle}</size>";
            if (muscleSubText != null)
                muscleSubText.text = $"мышца · стадия {_body.CurrentStageIndex + 1} из {_body.StageCount}";
        }

        private void ApplyPhaseBadge(WorkoutPhase p)
        {
            (string label, Color color) = p switch
            {
                WorkoutPhase.Warmup    => ("Разогрев",   warmupColor),
                WorkoutPhase.WorkZone  => ("Рабочая зона", workZoneColor),
                _                      => ("Отказ мышц", failColor)
            };
            if (phaseBadgeText != null) phaseBadgeText.text = label;
            if (phaseBadgeBG   != null) phaseBadgeBG.color  = color;
            if (setProgressFill!= null) setProgressFill.color = color;
        }

        private void RefreshSetInfo()
        {
            if (_session == null || setInfoText == null) return;
            setInfoText.text = $"Подход {_session.CurrentSet + 1} из {_session.TotalSets} · {_session.CurrentExercise}";
        }

        // ── Точки подходов ───────────────────────────────────────
        private void BuildSetDots()
        {
            if (setDotsParent == null || setDotPrefab == null) return;
            foreach (Transform c in setDotsParent) Destroy(c.gameObject);
            int n = _session != null ? _session.TotalSets : 5;
            for (int i = 0; i < n; i++)
            {
                var dot = Instantiate(setDotPrefab, setDotsParent);
                var img = dot.GetComponent<Image>();
                if (img) img.color = new Color(1f, 1f, 1f, 0.25f);
            }
        }

        private void UpdateSetDots(int completedIdx)
        {
            if (setDotsParent == null) return;
            for (int i = 0; i < setDotsParent.childCount; i++)
            {
                var img = setDotsParent.GetChild(i).GetComponent<Image>();
                if (img) img.color = i <= completedIdx ? workZoneColor : new Color(1f, 1f, 1f, 0.25f);
            }
        }

        // ── Вкладки ──────────────────────────────────────────────
        private void SwitchTab(int idx)
        {
            if (workoutPanel != null) workoutPanel.SetActive(idx == 0);
            if (restPanel    != null) restPanel.SetActive(idx == 1);
            if (resultsPanel != null) resultsPanel.SetActive(idx == 2);
            SetTabStyle(tabWorkout, idx == 0);
            SetTabStyle(tabRest,    idx == 1);
            SetTabStyle(tabResults, idx == 2);
        }

        private void SetTabStyle(Button b, bool active)
        {
            if (b == null) return;
            var img = b.GetComponent<Image>();
            if (img == null)
            {
                Transform fill = b.transform.Find("Fill");
                if (fill != null) img = fill.GetComponent<Image>();
            }
            if (img) img.color = active ? tabActiveColor : tabInactiveColor;
        }

        // ── Корутины ─────────────────────────────────────────────
        private IEnumerator RestCountdown(float duration)
        {
            float remaining = duration;
            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                if (restTimerBar  != null) restTimerBar.value  = remaining / duration;
                if (restTimerText != null) restTimerText.text  = $"{Mathf.CeilToInt(remaining)}с";
                yield return null;
            }
            if (_session != null && _session.IsActive) SwitchTab(0);
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

        private void StartFailFX()
        {
            if (_pulseCo != null) StopCoroutine(_pulseCo);
            _pulseCo = StartCoroutine(PulseOverlay());
        }

        private void StopFailFX()
        {
            if (_pulseCo != null) { StopCoroutine(_pulseCo); _pulseCo = null; }
            if (screenOverlay != null) screenOverlay.color = Color.clear;
        }

        private IEnumerator PulseOverlay()
        {
            Color red = new Color(0.87f, 0.1f, 0.1f, 0.22f);
            while (true)
            {
                float t = Mathf.PingPong(Time.time * 2f, 1f);
                if (screenOverlay != null) screenOverlay.color = Color.Lerp(Color.clear, red, t);
                yield return null;
            }
        }

        private IEnumerator CollapseEffect()
        {
            if (screenOverlay != null) screenOverlay.color = new Color(0f, 0f, 0f, 0.75f);
            yield return new WaitForSeconds(1.5f);
            if (screenOverlay != null) screenOverlay.color = Color.clear;
        }

    }
}
