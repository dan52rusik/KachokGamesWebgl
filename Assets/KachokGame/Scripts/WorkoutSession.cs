using System;
using UnityEngine;

namespace Tutorial
{
    // ── Типы данных ───────────────────────────────────────────────
    public enum WorkoutPhase { Warmup, WorkZone, MuscleFail }

    public enum DayPlan { Chest, Back, Legs }

    [Serializable]
    public struct WorkoutResults
    {
        public int      setsCompleted;
        public int      totalSets;
        public int      muscleGained;
        public int[]    musclePerSet;
        public bool     isNewRecord;
        public DayPlan  dayPlan;
        public string   exerciseName;
    }

    /// <summary>
    /// Управляет сессией тренировки: подходы, упражнения, 3 фазы.
    /// Каждый день — уникальный план (грудь / спина / ноги).
    /// </summary>
    public class WorkoutSession : MonoBehaviour
    {
        public static WorkoutSession Instance { get; private set; }

        [Header("Конфигурация")]
        [SerializeField] private int totalSets     = 5;
        [SerializeField] private int clicksPerSet  = 15;
        [SerializeField] private int baseMusclePerSet = 10;

        [Header("Пороги фаз (0–1)")]
        [SerializeField, Range(0f, 1f)] private float warmupEnd   = 0.3f;
        [SerializeField, Range(0f, 1f)] private float workZoneEnd = 0.8f;

        [Header("Упражнения по дням")]
        [SerializeField] private string[] chestExercises = { "Жим гантелей", "Разводка", "Отжимания" };
        [SerializeField] private string[] backExercises  = { "Тяга гантели", "Шраги", "Гиперэкстензия" };
        [SerializeField] private string[] legsExercises  = { "Выпады", "Жим ногами", "Подъёмы на носки" };

        // ── Состояние ────────────────────────────────────────────
        private bool          _isActive;
        private int           _currentSet;
        private int           _currentClicks;
        private int           _totalMuscle;
        private int[]         _musclePerSet;
        private WorkoutPhase  _phase;
        private DayPlan       _todayPlan;
        private string[]      _todayExercises;
        private int           _exerciseIdx;
        private int           _recordMuscle;

        // ── События ──────────────────────────────────────────────
        public event Action                      OnSessionStarted;
        public event Action<WorkoutPhase>        OnPhaseChanged;
        public event Action<int, int>            OnClickRegistered;   // (current, max)
        public event Action<int, int, int>       OnSetCompleted;      // (setNum, totalSets, muscle)
        public event Action<WorkoutResults>      OnSessionCompleted;

        // ── Свойства ─────────────────────────────────────────────
        public bool         IsActive         => _isActive;
        public int          CurrentSet       => _currentSet;
        public int          TotalSets        => totalSets;
        public int          CurrentClicks    => _currentClicks;
        public int          ClicksPerSet     => clicksPerSet;
        public WorkoutPhase CurrentPhase     => _phase;
        public DayPlan      TodayPlan        => _todayPlan;
        public int          TotalMuscle      => _totalMuscle;
        public string CurrentExercise        =>
            (_todayExercises != null && _todayExercises.Length > 0)
                ? _todayExercises[_exerciseIdx % _todayExercises.Length]
                : "";

        // ─────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            int dayIdx  = (int)DateTime.Now.DayOfWeek % 3;
            _todayPlan  = (DayPlan)dayIdx;
            _todayExercises = GetExercises(_todayPlan);
        }

        // ── Публичный API ─────────────────────────────────────────
        public void StartSession()
        {
            if (_isActive) return;
            _isActive      = true;
            _currentSet    = 0;
            _currentClicks = 0;
            _totalMuscle   = 0;
            _exerciseIdx   = 0;
            _phase         = WorkoutPhase.Warmup;
            _musclePerSet  = new int[totalSets];

            StaminaSystem.Instance?.SetWorkoutActive(true);
            OnSessionStarted?.Invoke();
            Debug.Log($"[WorkoutSession] Начата: {_todayPlan} — {CurrentExercise}");
        }

        /// <summary>Клик. effMult — множитель эффективности (1 или 0.5 при дебаффе).</summary>
        public void RegisterClick(float effMult = 1f)
        {
            if (!_isActive) return;

            _currentClicks++;
            OnClickRegistered?.Invoke(_currentClicks, clicksPerSet);
            CheckPhase();

            if (_currentClicks >= clicksPerSet)
                CompleteSet(effMult);
        }

        public void EndSession()
        {
            if (!_isActive) return;
            Finish();
        }

        // ── Приватная логика ──────────────────────────────────────
        private void CheckPhase()
        {
            float r = (float)_currentClicks / clicksPerSet;
            WorkoutPhase p;
            if      (r < warmupEnd)   p = WorkoutPhase.Warmup;
            else if (r < workZoneEnd) p = WorkoutPhase.WorkZone;
            else                      p = WorkoutPhase.MuscleFail;

            if (p != _phase)
            {
                _phase = p;
                OnPhaseChanged?.Invoke(_phase);
            }
        }

        private void CompleteSet(float effMult)
        {
            int muscle = Mathf.Max(1, Mathf.RoundToInt(baseMusclePerSet * effMult));
            _musclePerSet[_currentSet] = muscle;
            _totalMuscle  += muscle;
            _currentSet   += 1;
            _currentClicks = 0;
            _phase         = WorkoutPhase.Warmup;
            _exerciseIdx  += 1;

            BodyMorphSystem.Instance?.AddMusclePoints(muscle);
            OnSetCompleted?.Invoke(_currentSet, totalSets, muscle);

            if (_currentSet >= totalSets)
                Finish();
        }

        private void Finish()
        {
            _isActive = false;
            StaminaSystem.Instance?.SetWorkoutActive(false);

            bool record = _totalMuscle > _recordMuscle;
            if (record) _recordMuscle = _totalMuscle;

            OnSessionCompleted?.Invoke(new WorkoutResults
            {
                setsCompleted = _currentSet,
                totalSets     = totalSets,
                muscleGained  = _totalMuscle,
                musclePerSet  = _musclePerSet,
                isNewRecord   = record,
                dayPlan       = _todayPlan,
                exerciseName  = CurrentExercise
            });
        }

        private string[] GetExercises(DayPlan p) => p switch
        {
            DayPlan.Chest => chestExercises,
            DayPlan.Back  => backExercises,
            DayPlan.Legs  => legsExercises,
            _             => chestExercises
        };
    }
}
