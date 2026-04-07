using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tutorial
{
    /// <summary>
    /// Кликер-тренировка.
    ///
    /// Гантели лежат на сцене. Подходишь → подсказка.
    /// E → берёшь в руки, начинается кликер.
    /// ЛКМ / Пробел → клик (руки чередуются).
    /// X → кладёшь обратно.
    /// </summary>
    public class DumbbellWorkout : MonoBehaviour
    {
        [Header("UI (опционально)")]
        [SerializeField] private WorkoutUI workoutUI;

        [Header("Кости рук (верхняя часть — для кёрла)")]
        [SerializeField] private string rightArmBoneName = "Arm_R.001";
        [SerializeField] private string leftArmBoneName  = "Arm_L.001";

        [Header("Кости предплечья (к ним крепятся гантели)")]
        [SerializeField] private string rightForearmName = "Arm_R.002_end";
        [SerializeField] private string leftForearmName  = "Arm_L.002_end";

        [Header("Поиск гантелей")]
        [Tooltip("Часть имени гантели для поиска")]
        [SerializeField] private string dumbbellSearchName = "гантел";
        [Tooltip("Дистанция обнаружения гантелей")]
        [SerializeField] private float detectRadius = 3f;

        [Header("Бицепс-кёрл")]
        [SerializeField] private float curlAngle = -80f;
        [SerializeField] private float curlSpeed = 6f;
        [SerializeField] private float holdTime  = 0.3f;

        [Header("Кликер")]
        [SerializeField] private int   clicksPerSet = 15;
        [SerializeField] private int   musclePerSet = 10;
        [SerializeField] private float decayDelay   = 1.5f;
        [SerializeField] private float decayRate    = 3f;

        // Состояние
        private bool  _isWorking;
        private int   _currentClicks, _clickCount;
        private float _lastClickTime;
        private int   _totalMuscle;
        private bool  _nearDumbbells;

        private Player _player;

        // Кости
        private Transform  _rightBone, _leftBone;
        private Quaternion _rightRestRot, _leftRestRot;
        private Transform  _rightForearm, _leftForearm;
        private bool       _bonesReady;

        // Гантели
        private Transform _rightDumbbell, _leftDumbbell;
        // Исходные данные (где лежали на земле)
        private Transform  _rightOrigParent, _leftOrigParent;
        private Vector3    _rightOrigPos,    _leftOrigPos;
        private Quaternion _rightOrigRot,    _leftOrigRot;
        private Vector3    _rightOrigScale,  _leftOrigScale;

        // Blend: 0 = покой, 1 = поднято
        private float _rightT, _rightTarget;
        private float _leftT,  _leftTarget;

        // ── Start ─────────────────────────────────────────────
        private void Start()
        {
            _player = GetComponent<Player>();
            workoutUI?.SetClickCallback(OnClickPump);
            StartCoroutine(Setup());
        }

        private IEnumerator Setup()
        {
            yield return null;

            // Кости рук
            _rightBone = FindByName(transform, rightArmBoneName);
            _leftBone  = FindByName(transform, leftArmBoneName);

            if (_rightBone != null) { _rightRestRot = _rightBone.localRotation; Debug.Log($"[Workout] ✓ Правая кость: {_rightBone.name}"); }
            else Debug.LogWarning($"[Workout] ✗ '{rightArmBoneName}' не найдена!");

            if (_leftBone != null) { _leftRestRot = _leftBone.localRotation; Debug.Log($"[Workout] ✓ Левая кость:  {_leftBone.name}"); }
            else Debug.LogWarning($"[Workout] ✗ '{leftArmBoneName}' не найдена!");

            // Предплечья (куда крепим гантели)
            _rightForearm = FindByName(transform, rightForearmName);
            _leftForearm  = FindByName(transform, leftForearmName);

            if (_rightForearm == null && _rightBone != null && _rightBone.childCount > 0)
                _rightForearm = _rightBone.GetChild(0);
            if (_leftForearm == null && _leftBone != null && _leftBone.childCount > 0)
                _leftForearm = _leftBone.GetChild(0);

            Debug.Log($"[Workout] Правое предплечье: {_rightForearm?.name ?? "✗"}");
            Debug.Log($"[Workout] Левое предплечье:  {_leftForearm?.name ?? "✗"}");

            // Найти гантели
            Transform[] allDumbbells = FindAllInSceneContaining(dumbbellSearchName);
            Debug.Log($"[Workout] Найдено гантелей: {allDumbbells.Length}");

            if (allDumbbells.Length > 0)
            {
                _rightDumbbell = allDumbbells[0];
                // Сохраняем исходное положение
                _rightOrigParent = _rightDumbbell.parent;
                _rightOrigPos    = _rightDumbbell.position;
                _rightOrigRot    = _rightDumbbell.rotation;
                _rightOrigScale  = _rightDumbbell.localScale;
                Debug.Log($"[Workout] ✓ Правая гантель: {_rightDumbbell.name}");
            }
            if (allDumbbells.Length > 1)
            {
                _leftDumbbell = allDumbbells[1];
                _leftOrigParent = _leftDumbbell.parent;
                _leftOrigPos    = _leftDumbbell.position;
                _leftOrigRot    = _leftDumbbell.rotation;
                _leftOrigScale  = _leftDumbbell.localScale;
                Debug.Log($"[Workout] ✓ Левая гантель:  {_leftDumbbell.name}");
            }

            _bonesReady = true;
        }

        // ── Update ────────────────────────────────────────────
        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || !_bonesReady) return;

            // --- Не тренируемся: проверяем близость ---
            if (!_isWorking)
            {
                CheckNearDumbbells();

                // E рядом с гантелями → подобрать и начать
                if (_nearDumbbells && kb.eKey.wasPressedThisFrame)
                    PickUpAndStart();

                return;
            }

            // --- Тренируемся ---
            // X → положить гантели
            if (kb.xKey.wasPressedThisFrame)
            {
                PutDownAndStop();
                return;
            }

            // Esc → тоже положить
            if (kb.escapeKey.wasPressedThisFrame)
            {
                PutDownAndStop();
                return;
            }

            // Клики
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) OnClickPump();
            if (kb.spaceKey.wasPressedThisFrame)                       OnClickPump();

            // Decay
            if (_currentClicks > 0 && Time.time - _lastClickTime > decayDelay)
            {
                _currentClicks = Mathf.Max(0, _currentClicks - Mathf.CeilToInt(decayRate * Time.deltaTime));
                workoutUI?.UpdateProgress(_currentClicks, clicksPerSet);
            }

            // Плавный blend
            _rightT = Mathf.MoveTowards(_rightT, _rightTarget, Time.deltaTime * curlSpeed);
            _leftT  = Mathf.MoveTowards(_leftT,  _leftTarget,  Time.deltaTime * curlSpeed);
        }

        // ── LateUpdate ───────────────────────────────────────
        private void LateUpdate()
        {
            if (!_bonesReady || !_isWorking) return;
            if (_rightT < 0.001f && _leftT < 0.001f) return;

            if (_rightBone != null)
            {
                Quaternion lifted = _rightRestRot * Quaternion.Euler(0f, 0f, curlAngle);
                _rightBone.localRotation = Quaternion.Slerp(_rightRestRot, lifted, _rightT);
            }
            if (_leftBone != null)
            {
                Quaternion lifted = _leftRestRot * Quaternion.Euler(0f, 0f, -curlAngle);
                _leftBone.localRotation = Quaternion.Slerp(_leftRestRot, lifted, _leftT);
            }
        }

        // ── Проверка близости к гантелям ──────────────────────
        private void CheckNearDumbbells()
        {
            bool wasNear = _nearDumbbells;
            _nearDumbbells = false;

            if (_rightDumbbell != null)
            {
                float dist = Vector3.Distance(transform.position, _rightDumbbell.position);
                if (dist < detectRadius) _nearDumbbells = true;
            }
            if (!_nearDumbbells && _leftDumbbell != null)
            {
                float dist = Vector3.Distance(transform.position, _leftDumbbell.position);
                if (dist < detectRadius) _nearDumbbells = true;
            }

            // Подсказки
            if (_nearDumbbells && !wasNear)
                workoutUI?.ShowHint("Нажми [E] чтобы взять гантели!");
            else if (!_nearDumbbells && wasNear)
                workoutUI?.HideHint();
        }

        // ── Подобрать + начать ────────────────────────────────
        private void PickUpAndStart()
        {
            // Прикрепить гантели к предплечьям
            if (_rightDumbbell != null && _rightForearm != null)
            {
                _rightDumbbell.SetParent(_rightForearm);
                _rightDumbbell.localPosition = new Vector3(-0.00196f, 0.00917f, 0.00298f);
                _rightDumbbell.localRotation = Quaternion.Euler(16.395f, 81.053f, 91.449f);
                _rightDumbbell.localScale    = new Vector3(0.9953f, 0.6857f, 0.9953f);
            }
            if (_leftDumbbell != null && _leftForearm != null)
            {
                _leftDumbbell.SetParent(_leftForearm);
                _leftDumbbell.localPosition = new Vector3(0.00195f, 0.00853f, 0.00322f);
                _leftDumbbell.localRotation = Quaternion.Euler(33.769f, -80.619f, -78.745f);
                _leftDumbbell.localScale    = new Vector3(0.9953f, 0.6857f, 0.9953f);
            }

            _isWorking     = true;
            _currentClicks = 0;
            _clickCount    = 0;
            _lastClickTime = Time.time;
            _rightTarget   = _leftTarget = 0f;
            _rightT        = _leftT = 0f;

            _player?.SetMovementLocked(true);
            workoutUI?.HideHint();
            workoutUI?.ShowWorkout();
            workoutUI?.UpdateProgress(0, clicksPerSet);
            Debug.Log("[Workout] 🏋 Гантели в руках! ЛКМ/Пробел = качай, X = положить");
        }

        // ── Положить + остановить ─────────────────────────────
        private void PutDownAndStop()
        {
            // Вернуть руки в покой
            _rightTarget = _leftTarget = 0f;
            _rightT = _leftT = 0f;

            // Вернуть гантели на место
            if (_rightDumbbell != null)
            {
                _rightDumbbell.SetParent(_rightOrigParent);
                _rightDumbbell.position   = _rightOrigPos;
                _rightDumbbell.rotation   = _rightOrigRot;
                _rightDumbbell.localScale = _rightOrigScale;
            }
            if (_leftDumbbell != null)
            {
                _leftDumbbell.SetParent(_leftOrigParent);
                _leftDumbbell.position   = _leftOrigPos;
                _leftDumbbell.rotation   = _leftOrigRot;
                _leftDumbbell.localScale = _leftOrigScale;
            }

            _isWorking     = false;
            _currentClicks = 0;
            _clickCount    = 0;

            _player?.SetMovementLocked(false);
            workoutUI?.HideWorkout();
            Debug.Log("[Workout] Гантели положены на место");
        }

        // ── Клик ──────────────────────────────────────────────
        private void OnClickPump()
        {
            if (!_isWorking) return;
            _currentClicks++; _clickCount++; _lastClickTime = Time.time;

            bool useRight = (_clickCount % 2 == 1);
            if (useRight) { _rightTarget = 1f; StartCoroutine(LowerAfter(true));  }
            else          { _leftTarget  = 1f; StartCoroutine(LowerAfter(false)); }

            Debug.Log($"[Workout] {(useRight ? "▶ П" : "◀ Л")} {_currentClicks}/{clicksPerSet}");

            if (_currentClicks >= clicksPerSet)
            {
                _totalMuscle += musclePerSet;
                workoutUI?.AddMuscle(musclePerSet);
                workoutUI?.ShowSetComplete(musclePerSet);
                workoutUI?.UpdateProgress(clicksPerSet, clicksPerSet);
                _currentClicks = 0; _clickCount = 0;
                Invoke(nameof(ResetUI), 1.2f);
                Debug.Log($"[Workout] 💪 +{musclePerSet} Всего: {_totalMuscle}");
            }
            else workoutUI?.UpdateProgress(_currentClicks, clicksPerSet);
        }

        private IEnumerator LowerAfter(bool r)
        {
            yield return new WaitForSeconds(holdTime);
            if (r) _rightTarget = 0f; else _leftTarget = 0f;
        }

        private void ResetUI() => workoutUI?.UpdateProgress(_currentClicks, clicksPerSet);

        // ── Утилиты ───────────────────────────────────────────
        private static Transform FindByName(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var f = FindByName(root.GetChild(i), name);
                if (f != null) return f;
            }
            return null;
        }

        private static Transform[] FindAllInSceneContaining(string substring)
        {
            var result = new List<Transform>();
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                FindContaining(root.transform, substring, result);
            return result.ToArray();
        }

        private static void FindContaining(Transform t, string sub, List<Transform> list)
        {
            if (t.name.Contains(sub))
                list.Add(t);
            for (int i = 0; i < t.childCount; i++)
                FindContaining(t.GetChild(i), sub, list);
        }
    }
}
