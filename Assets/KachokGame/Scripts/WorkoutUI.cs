using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tutorial
{
    /// <summary>
    /// UI для мини-игры кликера с гантелями.
    /// Создай Canvas (Screen Space — Overlay) с дочерними элементами:
    /// - Panel "WorkoutPanel" (фон полупрозрачный)
    ///   - Image "ProgressBarBG" (фон бара)
    ///     - Image "ProgressBarFill" (заливка, Image Type = Filled)
    ///   - TextMeshProUGUI "ClickText" ("ЖМЁШЬ! 0/15")
    ///   - TextMeshProUGUI "MuscleText" ("💪 Мышца: 0")
    ///   - Button "WorkoutButton" ("КАЧАЙ!")
    ///   - TextMeshProUGUI "HintText" ("Подойди к гантелям [E]")
    /// По умолчанию WorkoutPanel скрыт.
    /// </summary>
    public class WorkoutUI : MonoBehaviour
    {
        [Header("Панель кликера")]
        [SerializeField] private GameObject workoutPanel;
        [SerializeField] private Image progressBarFill;
        [SerializeField] private TextMeshProUGUI clickText;
        [SerializeField] private TextMeshProUGUI muscleText;
        [SerializeField] private Button workoutButton;

        [Header("Подсказка")]
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private TextMeshProUGUI hintText;

        private int _totalMuscle;

        public int TotalMuscle => _totalMuscle;

        private System.Action _onClickCallback;

        private void Awake()
        {
            if (workoutPanel != null)
                workoutPanel.SetActive(false);

            if (hintPanel != null)
                hintPanel.SetActive(false);

            if (workoutButton != null)
                workoutButton.onClick.AddListener(OnWorkoutButtonClick);

            UpdateMuscleDisplay();
        }

        /// <summary>Установить callback для кнопки кликера</summary>
        public void SetClickCallback(System.Action callback)
        {
            _onClickCallback = callback;
        }

        private void OnWorkoutButtonClick()
        {
            _onClickCallback?.Invoke();
        }

        /// <summary>Показать UI кликера</summary>
        public void ShowWorkout()
        {
            if (workoutPanel != null)
                workoutPanel.SetActive(true);
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }

        /// <summary>Скрыть UI кликера</summary>
        public void HideWorkout()
        {
            if (workoutPanel != null)
                workoutPanel.SetActive(false);
        }

        /// <summary>Показать подсказку "Нажми [E] чтобы качаться"</summary>
        public void ShowHint(string text = "Нажми [E] чтобы качаться!")
        {
            if (hintPanel != null)
            {
                hintPanel.SetActive(true);
                if (hintText != null)
                    hintText.text = text;
            }
        }

        /// <summary>Скрыть подсказку</summary>
        public void HideHint()
        {
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }

        /// <summary>Обновить прогресс-бар и текст кликов</summary>
        public void UpdateProgress(int currentClicks, int maxClicks)
        {
            float ratio = Mathf.Clamp01((float)currentClicks / maxClicks);

            if (progressBarFill != null)
                progressBarFill.fillAmount = ratio;

            if (clickText != null)
                clickText.text = $"КАЧАЙ! {currentClicks}/{maxClicks}";
        }

        /// <summary>Добавить мышцу и обновить дисплей</summary>
        public void AddMuscle(int amount)
        {
            _totalMuscle += amount;
            UpdateMuscleDisplay();
        }

        private void UpdateMuscleDisplay()
        {
            if (muscleText != null)
                muscleText.text = $"\U0001F4AA Мышца: {_totalMuscle}";
        }

        /// <summary>Показать сообщение о завершённом подходе</summary>
        public void ShowSetComplete(int muscleGained)
        {
            if (clickText != null)
                clickText.text = $"ПОДХОД ЗАВЕРШЁН! +{muscleGained} 💪";
        }
    }
}
