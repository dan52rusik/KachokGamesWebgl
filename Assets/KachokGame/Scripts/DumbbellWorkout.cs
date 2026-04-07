using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tutorial
{
    /// <summary>
    /// Тренировка-кликер. Поочерёдный бицепс-кёрл.
    /// Вращение костей — в LateUpdate (перекрывает Animator).
    ///
    /// Если рука гнётся в неправильную сторону — смени знак Curl Angle в Inspector
    /// прямо в Play Mode и сразу увидишь результат.
    /// </summary>
    public class DumbbellWorkout : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private WorkoutUI workoutUI;

        [Header("Поиск гантелей")]
        [SerializeField] private float detectRadius = 3f;

        [Header("Кости рук")]
        [SerializeField] private string rightArmBoneName = "Arm_R.001";
        [SerializeField] private string leftArmBoneName  = "Arm_L.001";

        [Header("Бицепс-кёрл")]
        [Tooltip("Угол кёрла. Если рука идёт внутрь — смени знак (напр. 80 → -80).")]
        [SerializeField] private float curlAngle = -80f;
        [Tooltip("Скорость подъёма/опускания")]
        [SerializeField] private float curlSpeed = 6f;
        [Tooltip("Пауза в верхней точке (сек)")]
        [SerializeField] private float holdTime  = 0.3f;

        // ── Состояние ─────────────────────────────────────────
        private Dumbbell   _nearbyDumbbell;
        private Dumbbell   _currentDumbbellR;
        private Dumbbell   _currentDumbbellL;
        private bool       _isWorking;
        private int        _currentClicks;
        private float      _lastClickTime;
        private int        _clickCount;

        private Player     _player;
        private Animator   _anim;
        private bool       _hasWorkoutParam;
        private Dumbbell[] _allDumbbells;

        // Кости рук и их исходные локальные повороты
        private Transform  _rightBone;
        private Transform  _leftBone;
        private Quaternion _rightRestRot;
        private Quaternion _leftRestRot;
        private bool       _restCached;

        // Точки крепления гантелей (прямые дети костей рук = запястья)
        private Transform _rightWrist;
        private Transform _leftWrist;

        // Blend t: 0 = покой, 1 = поднято
        private float _rightT, _rightTarget;
        private float _leftT,  _leftTarget;

        // ── Init ──────────────────────────────────────────────
        private void Start()
        {
            _player = GetComponent<Player>();
            _anim   = GetComponent<Animator>();

            if (_anim != null)
                foreach (AnimatorControllerParameter p in _anim.parameters)
                    if (p.name == "isWorkout") _hasWorkoutParam = true;

            workoutUI?.SetClickCallback(OnClickPump);

            _allDumbbells = FindObjectsByType<Dumbbell>(FindObjectsSortMode.None);
            Debug.Log($"[DumbbellWorkout] Гантелей на сцене: {_allDumbbells.Length}");

            StartCoroutine(CacheBones());
        }

        private IEnumerator CacheBones()
        {
            yield return null; // ждём кадр — Animator встаёт в дефолт

            _rightBone = FindBoneByName(transform, rightArmBoneName);
            _leftBone  = FindBoneByName(transform, leftArmBoneName);

            if (_rightBone != null)
            {
                _rightRestRot = _rightBone.localRotation;
                _rightWrist   = _rightBone.childCount > 0 ? _rightBone.GetChild(0) : _rightBone;
                Debug.Log($"[DumbbellWorkout] ✓ Правая: {_rightBone.name}  → wrist: {_rightWrist.name}");
            }
            else Debug.LogWarning($"[DumbbellWorkout] ✗ Кость '{rightArmBoneName}' не найдена!");

            if (_leftBone != null)
            {
                _leftRestRot = _leftBone.localRotation;
                _leftWrist   = _leftBone.childCount > 0 ? _leftBone.GetChild(0) : _leftBone;
                Debug.Log($"[DumbbellWorkout] ✓ Левая:  {_leftBone.name}  → wrist: {_leftWrist.name}");
            }
            else Debug.LogWarning($"[DumbbellWorkout] ✗ Кость '{leftArmBoneName}' не найдена!");

            _restCached = true;
        }

        // ── Update ─────────────────────────────────────────────
        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (!_isWorking) FindNearestDumbbell();

            if (!_isWorking && _nearbyDumbbell != null && kb.eKey.wasPressedThisFrame)
            { StartWorkout(); return; }

            if (_isWorking)
            {
                if (kb.eKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)
                { StopWorkout(); return; }

                Mouse mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.wasPressedThisFrame) OnClickPump();
                if (kb.spaceKey.wasPressedThisFrame) OnClickPump();

                // Decay
                Dumbbell db = _currentDumbbellR ?? _currentDumbbellL;
                if (db != null && _currentClicks > 0 && Time.time - _lastClickTime > db.DecayDelay)
                {
                    _currentClicks = Mathf.Max(0, _currentClicks - Mathf.CeilToInt(db.DecayRate * Time.deltaTime));
                    workoutUI?.UpdateProgress(_currentClicks, db.ClicksPerSet);
                }
            }

            _rightT = Mathf.MoveTowards(_rightT, _rightTarget, Time.deltaTime * curlSpeed);
            _leftT  = Mathf.MoveTowards(_leftT,  _leftTarget,  Time.deltaTime * curlSpeed);
        }

        // ── LateUpdate: поверх Animator ────────────────────────
        private void LateUpdate()
        {
            if (!_restCached) return;
            if (!_isWorking && _rightT < 0.001f && _leftT < 0.001f) return;

            if (_rightBone != null)
            {
                // Правая рука: Z+ (наружу от тела вперёд)
                Quaternion lifted = _rightRestRot * Quaternion.Euler(0f, 0f, curlAngle);
                _rightBone.localRotation = Quaternion.Slerp(_rightRestRot, lifted, _rightT);
            }

            if (_leftBone != null)
            {
                // Левая рука: угол инвертирован (зеркально правой)
                Quaternion lifted = _leftRestRot * Quaternion.Euler(0f, 0f, -curlAngle);
                _leftBone.localRotation = Quaternion.Slerp(_leftRestRot, lifted, _leftT);
            }
        }

        // ── StartWorkout ───────────────────────────────────────
        private void StartWorkout()
        {
            _isWorking     = true;
            _currentClicks = 0;
            _clickCount    = 0;
            _lastClickTime = Time.time;
            _rightTarget   = _leftTarget = 0f;

            _player?.SetMovementLocked(true);
            if (_anim != null && _hasWorkoutParam) _anim.SetBool("isWorkout", true);

            // Гантели к рукам
            if (_allDumbbells.Length > 0 && _rightWrist != null)
            {
                _allDumbbells[0].AttachTo(_rightWrist);
                _currentDumbbellR = _allDumbbells[0];
                Debug.Log($"[DumbbellWorkout] 🏋 Правая ← {_allDumbbells[0].name}");
            }
            if (_allDumbbells.Length > 1 && _leftWrist != null)
            {
                _allDumbbells[1].AttachTo(_leftWrist);
                _currentDumbbellL = _allDumbbells[1];
                Debug.Log($"[DumbbellWorkout] 🏋 Левая  ← {_allDumbbells[1].name}");
            }

            Dumbbell db = _currentDumbbellR ?? _currentDumbbellL;
            workoutUI?.HideHint();
            workoutUI?.ShowWorkout();
            workoutUI?.UpdateProgress(0, db?.ClicksPerSet ?? 15);
        }

        // ── StopWorkout ────────────────────────────────────────
        private void StopWorkout()
        {
            _rightTarget = _leftTarget = 0f;

            _currentDumbbellR?.ReturnToOriginal();
            _currentDumbbellL?.ReturnToOriginal();
            _currentDumbbellR = _currentDumbbellL = null;

            _isWorking = false; _currentClicks = 0; _clickCount = 0;
            _player?.SetMovementLocked(false);
            if (_anim != null && _hasWorkoutParam) _anim.SetBool("isWorkout", false);
            workoutUI?.HideWorkout();
        }

        // ── Click ──────────────────────────────────────────────
        private void OnClickPump()
        {
            Dumbbell db = _currentDumbbellR ?? _currentDumbbellL;
            if (!_isWorking || db == null) return;

            _currentClicks++;
            _clickCount++;
            _lastClickTime = Time.time;

            bool useRight = (_clickCount % 2 == 1);
            if (useRight) { _rightTarget = 1f; StartCoroutine(Lower(true));  }
            else          { _leftTarget  = 1f; StartCoroutine(Lower(false)); }

            Debug.Log($"[DumbbellWorkout] {(useRight ? "▶ Правая" : "◀ Левая")} {_currentClicks}/{db.ClicksPerSet}");

            if (_currentClicks >= db.ClicksPerSet)
            {
                int gain = db.MusclePerSet;
                workoutUI?.AddMuscle(gain);
                workoutUI?.ShowSetComplete(gain);
                workoutUI?.UpdateProgress(db.ClicksPerSet, db.ClicksPerSet);
                _currentClicks = 0; _clickCount = 0;
                Invoke(nameof(ResetUI), 1.2f);
                Debug.Log($"[DumbbellWorkout] 💪 ПОДХОД! +{gain} мышцы");
            }
            else workoutUI?.UpdateProgress(_currentClicks, db.ClicksPerSet);
        }

        private IEnumerator Lower(bool isRight)
        {
            yield return new WaitForSeconds(holdTime);
            if (isRight) _rightTarget = 0f; else _leftTarget = 0f;
        }

        private void ResetUI()
        {
            Dumbbell db = _currentDumbbellR ?? _currentDumbbellL;
            if (db != null) workoutUI?.UpdateProgress(_currentClicks, db.ClicksPerSet);
        }

        // ── Поиск ─────────────────────────────────────────────
        private void FindNearestDumbbell()
        {
            if (_allDumbbells == null || _allDumbbells.Length == 0)
                _allDumbbells = FindObjectsByType<Dumbbell>(FindObjectsSortMode.None);

            Dumbbell closest = null; float best = detectRadius;
            foreach (Dumbbell db in _allDumbbells)
            {
                if (db == null) continue;
                float d = Vector3.Distance(transform.position, db.transform.position);
                if (d < best) { best = d; closest = db; }
            }

            if (closest != null && _nearbyDumbbell == null)
            { _nearbyDumbbell = closest; workoutUI?.ShowHint("Нажми [E] чтобы качаться!"); }
            else if (closest != null) _nearbyDumbbell = closest;
            else if (closest == null && _nearbyDumbbell != null)
            { _nearbyDumbbell = null; workoutUI?.HideHint(); }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isWorking) return;
            var db = other.GetComponent<Dumbbell>() ?? other.GetComponentInParent<Dumbbell>();
            if (db != null) { _nearbyDumbbell = db; workoutUI?.ShowHint("Нажми [E] чтобы качаться!"); }
        }
        private void OnTriggerExit(Collider other)
        {
            var db = other.GetComponent<Dumbbell>() ?? other.GetComponentInParent<Dumbbell>();
            if (db != null && db == _nearbyDumbbell) { _nearbyDumbbell = null; workoutUI?.HideHint(); }
        }

        // ── Утилиты ───────────────────────────────────────────
        private static Transform FindBoneByName(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var f = FindBoneByName(root.GetChild(i), name);
                if (f != null) return f;
            }
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectRadius);
        }
    }
}
