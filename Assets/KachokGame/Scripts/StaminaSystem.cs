using System;
using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// Система выносливости (Stamina 0–100).
    /// При < 20 → дебафф на эффективность (50%).
    /// При 0 → OnStaminaDepleted.
    /// Восстановление: сон (быстро), еда (средне), время (медленно).
    /// </summary>
    public class StaminaSystem : MonoBehaviour
    {
        public static StaminaSystem Instance { get; private set; }

        [Header("Настройки")]
        [SerializeField] private float maxStamina      = 100f;
        [SerializeField] private float startStamina    = 100f;

        [Header("Расход")]
        [SerializeField] private float staminaPerClick = 1f;

        [Header("Восстановление (единиц/сек)")]
        [SerializeField] private float timeRecoveryRate  = 0.5f;
        [SerializeField] private float sleepRecoveryRate = 8f;

        [Header("Восстановление едой")]
        [SerializeField] private float foodRecoveryAmount = 25f;

        [Header("Дебафф")]
        [SerializeField] private float debuffThreshold  = 20f;
        [SerializeField] private float debuffMultiplier = 0.5f;

        private float _stamina;
        private bool  _isSleeping;
        private bool  _isInWorkout;

        // ── События ──────────────────────────────────────────────
        public event Action<float, float> OnStaminaChanged;  // (current, max)
        public event Action               OnStaminaDepleted;
        public event Action<bool>         OnDebuffChanged;   // (isActive)

        public float Stamina          => _stamina;
        public float MaxStamina       => maxStamina;
        public float StaminaRatio     => _stamina / maxStamina;
        public bool  IsDebuffActive   => _stamina < debuffThreshold;
        public float EfficiencyMult   => IsDebuffActive ? debuffMultiplier : 1f;

        // ─────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _stamina = startStamina;
            OnStaminaChanged?.Invoke(_stamina, maxStamina);
        }

        private void Update()
        {
            if (_isInWorkout) return;

            float prev = _stamina;
            float rate = _isSleeping ? sleepRecoveryRate : timeRecoveryRate;
            _stamina = Mathf.Min(maxStamina, _stamina + rate * Time.deltaTime);

            if (Mathf.Abs(_stamina - prev) > 0.01f)
                OnStaminaChanged?.Invoke(_stamina, maxStamina);
        }

        public void SetWorkoutActive(bool active) => _isInWorkout = active;
        public void SetSleeping(bool sleeping)    => _isSleeping  = sleeping;

        /// <summary>Расходует стамину за клик. Возвращает false если персонаж упал.</summary>
        public bool ConsumeForClick()
        {
            bool wasDebuff = IsDebuffActive;
            _stamina = Mathf.Max(0f, _stamina - staminaPerClick);
            OnStaminaChanged?.Invoke(_stamina, maxStamina);

            if (IsDebuffActive != wasDebuff)
                OnDebuffChanged?.Invoke(IsDebuffActive);

            if (_stamina <= 0f) { OnStaminaDepleted?.Invoke(); return false; }
            return true;
        }

        public void RecoverByFood(float multiplier = 1f)
        {
            _stamina = Mathf.Min(maxStamina, _stamina + foodRecoveryAmount * multiplier);
            OnStaminaChanged?.Invoke(_stamina, maxStamina);
        }

        public void RecoverInstant(float amount)
        {
            _stamina = Mathf.Min(maxStamina, _stamina + amount);
            OnStaminaChanged?.Invoke(_stamina, maxStamina);
        }
    }
}
