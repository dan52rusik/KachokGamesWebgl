using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// Повесь этот скрипт на каждый объект гантелей.
    /// Добавь SphereCollider (isTrigger=true, radius≈1.5) — зона подхода.
    /// holdPoint — точка, куда "прилипнут" гантели при подборе (пустой дочерний Transform на руке персонажа).
    /// </summary>
    public class Dumbbell : MonoBehaviour
    {
        [Header("Настройки")]
        [Tooltip("Радиус зоны взаимодействия (SphereCollider trigger)")]
        [SerializeField] private float interactionRadius = 2f;

        [Tooltip("Сколько мышцы даёт одна полная прокачка")]
        [SerializeField] private int musclePerSet = 10;

        [Tooltip("Количество кликов для одного подхода")]
        [SerializeField] private int clicksPerSet = 15;

        [Tooltip("Время (сек) до авто-сброса прогресса, если не кликать")]
        [SerializeField] private float decayDelay = 1.5f;

        [Tooltip("Скорость убывания прогресса (кликов/сек)")]
        [SerializeField] private float decayRate = 3f;

        /// <summary>Позиция, куда вернётся гантель при отпускании</summary>
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Transform _originalParent;

        private SphereCollider _triggerCollider;

        public int MusclePerSet => musclePerSet;
        public int ClicksPerSet => clicksPerSet;
        public float DecayDelay => decayDelay;
        public float DecayRate => decayRate;

        private void Awake()
        {
            _originalPosition = transform.localPosition;
            _originalRotation = transform.localRotation;
            _originalParent = transform.parent;

            // Авто-создание trigger-коллайдера, если нет
            _triggerCollider = GetComponent<SphereCollider>();
            if (_triggerCollider == null)
            {
                _triggerCollider = gameObject.AddComponent<SphereCollider>();
                _triggerCollider.isTrigger = true;
                _triggerCollider.radius = interactionRadius;
            }
            else
            {
                _triggerCollider.isTrigger = true;
                _triggerCollider.radius = interactionRadius;
            }
        }

        /// <summary>Прикрепить гантели к точке на персонаже</summary>
        public void AttachTo(Transform holdPoint)
        {
            if (holdPoint == null) return;
            transform.SetParent(holdPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Отключить trigger, пока гантели в руках
            if (_triggerCollider != null)
                _triggerCollider.enabled = false;
        }

        /// <summary>Вернуть гантели на место</summary>
        public void ReturnToOriginal()
        {
            transform.SetParent(_originalParent);
            transform.localPosition = _originalPosition;
            transform.localRotation = _originalRotation;

            if (_triggerCollider != null)
                _triggerCollider.enabled = true;
        }
    }
}
