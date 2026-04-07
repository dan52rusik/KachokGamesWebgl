using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tutorial
{
    /// <summary>
    /// Кликер-тренировка с гантелями.
    ///
    /// Гантели лежат на сцене. Подходишь → подсказка.
    /// [E] → берёшь в руки, начинается сессия (WorkoutSession).
    /// ЛКМ / Пробел → клик (руки чередуются, кёрл-анимация).
    /// [X] / Esc → кладёшь обратно, сессия прерывается.
    ///
    /// Зависит от: WorkoutSession, StaminaSystem, WorkoutHUD (все — синглтоны / Inspector).
    /// </summary>
    public class DumbbellWorkout : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────
        [Header("UI")]
        [SerializeField] private WorkoutHUD workoutHUD;

        [Header("Кости верхней части руки (для кёрла)")]
        [SerializeField] private string rightArmBoneName = "Arm_R.001";
        [SerializeField] private string leftArmBoneName  = "Arm_L.001";

        [Header("Кости предплечья (точка крепления гантели)")]
        [SerializeField] private string rightForearmName = "Arm_R.002_end";
        [SerializeField] private string leftForearmName  = "Arm_L.002_end";

        [Header("Гантели")]
        [SerializeField] private Dumbbell rightDumbbell;
        [SerializeField] private Dumbbell leftDumbbell;
        [SerializeField] private float    detectRadius = 3f;

        [Header("Бицепс-кёрл")]
        [SerializeField] private float curlAngle = -80f;
        [SerializeField] private float curlSpeed = 6f;
        [SerializeField] private float holdTime  = 0.3f;

        [Header("Темп кликов")]
        [SerializeField] private float clickSpeedWindow = 1.2f;
        [SerializeField] private float speedToMultiplier = 1.25f;
        [SerializeField] private float maxTempoMultiplier = 8f;

        // ── Внутреннее состояние ──────────────────────────────────
        private bool _isWorking;
        private int  _clickCount;   // для чередования рук
        private bool _nearDumbbells;

        private Player         _player;
        private WorkoutSession _session;
        private StaminaSystem  _stamina;

        // Кости
        private Transform  _rightBone,    _leftBone;
        private Quaternion _rightRestRot, _leftRestRot;
        private Transform  _rightForearm, _leftForearm;
        private bool       _bonesReady;

        // Гантели (Transform кэш)
        private Transform  _rightDb,        _leftDb;
        private Transform  _rightOrigParent, _leftOrigParent;
        private Vector3    _rightOrigPos,    _leftOrigPos;
        private Quaternion _rightOrigRot,    _leftOrigRot;
        private Vector3    _rightOrigScale,  _leftOrigScale;

        // Blend 0=покой 1=поднято
        private float _rightT;
        private float _leftT;
        private readonly List<float> _recentClickTimes = new List<float>();
        private float _currentTempoMultiplier = 1f;
        private float _currentClickSpeed;
        private int _rightRepQueue;
        private int _leftRepQueue;
        private Coroutine _rightRepRoutine;
        private Coroutine _leftRepRoutine;

        // ─────────────────────────────────────────────────────────
        private void Start()
        {
            _player  = GetComponent<Player>();
            _session = EnsureSystem<WorkoutSession>("WorkoutSession");
            _stamina = EnsureSystem<StaminaSystem>("StaminaSystem");
            EnsureSystem<BodyMorphSystem>("BodyMorphSystem");

            workoutHUD?.SetClickCallback(OnClickPump);

            // Авто-завершение сессии → положить гантели
            if (_session != null)
                _session.OnSessionCompleted += _ => PutDownAndStop();

            StartCoroutine(Setup());
        }

        private static T EnsureSystem<T>(string objectName) where T : Component
        {
            T existing = FindFirstObjectByType<T>();
            if (existing != null)
                return existing;

            var go = new GameObject(objectName);
            T created = go.AddComponent<T>();
            Debug.Log($"[DW] Автосоздана система: {typeof(T).Name}");
            return created;
        }

        private IEnumerator Setup()
        {
            yield return null; // ждём инициализацию аниматора

            _rightBone = FindByName(transform, rightArmBoneName);
            _leftBone  = FindByName(transform, leftArmBoneName);

            if (_rightBone) { _rightRestRot = _rightBone.localRotation; Debug.Log($"[DW] ✓ Правая кость: {_rightBone.name}"); }
            else Debug.LogWarning($"[DW] ✗ '{rightArmBoneName}' не найдена");

            if (_leftBone)  { _leftRestRot  = _leftBone.localRotation;  Debug.Log($"[DW] ✓ Левая кость: {_leftBone.name}"); }
            else Debug.LogWarning($"[DW] ✗ '{leftArmBoneName}' не найдена");

            _rightForearm = FindByName(transform, rightForearmName);
            _leftForearm  = FindByName(transform, leftForearmName);

            if (_rightForearm != null && _rightForearm.name.EndsWith("_end") && _rightForearm.parent != null)
                _rightForearm = _rightForearm.parent;
            if (_leftForearm != null && _leftForearm.name.EndsWith("_end") && _leftForearm.parent != null)
                _leftForearm = _leftForearm.parent;

            // Фолбэк — первый дочерний к кости
            if (!_rightForearm && _rightBone && _rightBone.childCount > 0) _rightForearm = _rightBone.GetChild(0);
            if (!_leftForearm  && _leftBone  && _leftBone.childCount  > 0) _leftForearm  = _leftBone.GetChild(0);

            AssignDumbbells();
            CacheDumbbellState();
            _bonesReady = true;
        }

        // ── Update ────────────────────────────────────────────────
        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || !_bonesReady) return;

            if (!_isWorking)
            {
                CheckNearDumbbells();
                if (_nearDumbbells && kb.eKey.wasPressedThisFrame) PickUpAndStart();
                return;
            }

            // Прекратить тренировку
            if (kb.xKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)
            {
                _session?.EndSession();
                return; // PutDownAndStop вызовется через OnSessionCompleted
            }

            // Ввод клика
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) OnClickPump();
            if (kb.spaceKey.wasPressedThisFrame)                       OnClickPump();

            // Плавный blend
            UpdateTempo();
        }

        // ── LateUpdate ────────────────────────────────────────────
        private void LateUpdate()
        {
            if (!_bonesReady || !_isWorking) return;
            if (_rightT < 0.001f && _leftT < 0.001f) return;

            if (_rightBone)
            {
                Quaternion lifted = _rightRestRot * Quaternion.Euler(0f, 0f, curlAngle);
                _rightBone.localRotation = Quaternion.Slerp(_rightRestRot, lifted, _rightT);
            }
            if (_leftBone)
            {
                Quaternion lifted = _leftRestRot * Quaternion.Euler(0f, 0f, -curlAngle);
                _leftBone.localRotation = Quaternion.Slerp(_leftRestRot, lifted, _leftT);
            }
        }

        // ── Ближайшие гантели ────────────────────────────────────
        private void CheckNearDumbbells()
        {
            bool wasNear = _nearDumbbells;
            _nearDumbbells = (_rightDb != null && Vector3.Distance(transform.position, _rightDb.position) < detectRadius)
                          || (_leftDb  != null && Vector3.Distance(transform.position, _leftDb.position)  < detectRadius);

            if (_nearDumbbells && !wasNear) workoutHUD?.ShowHint("Нажми [E] чтобы взять гантели!");
            else if (!_nearDumbbells && wasNear) workoutHUD?.HideHint();
        }

        // ── Подобрать + начать ────────────────────────────────────
        private void PickUpAndStart()
        {
            if (_session == null)
            {
                Debug.LogError("[DW] WorkoutSession не найден на сцене. Тренировка не будет запущена.");
                return;
            }

            AttachDumbbell(_rightDb, _rightForearm,
                new Vector3(-0.00196f, 0.00917f, 0.00298f),
                Quaternion.Euler(16.395f, 81.053f, 91.449f),
                new Vector3(0.9953f, 0.6857f, 0.9953f));

            AttachDumbbell(_leftDb, _leftForearm,
                new Vector3(0.00195f, 0.00853f, 0.00322f),
                Quaternion.Euler(33.769f, -80.619f, -78.745f),
                new Vector3(0.9953f, 0.6857f, 0.9953f));

            _isWorking   = true;
            _clickCount  = 0;
            _rightT      = _leftT = 0f;
            _rightRepQueue = _leftRepQueue = 0;
            _rightRepRoutine = _leftRepRoutine = null;
            _recentClickTimes.Clear();
            _currentTempoMultiplier = 1f;
            _currentClickSpeed = 0f;

            _player?.SetMovementLocked(true);
            _session?.StartSession();
            workoutHUD?.HideHint();
            workoutHUD?.ShowWorkout();
            workoutHUD?.SetTempo(_currentClickSpeed, _currentTempoMultiplier);
            Debug.Log("[DW] 🏋 Гантели в руках! ЛКМ/Пробел = качай, X = положить");
        }

        private void AttachDumbbell(Transform db, Transform bone,
            Vector3 localPos, Quaternion localRot, Vector3 localScale)
        {
            if (db == null || bone == null) return;
            db.SetParent(bone);
            db.localPosition = localPos;
            db.localRotation = localRot;
            db.localScale    = localScale;
        }

        // ── Положить + остановить ─────────────────────────────────
        private void PutDownAndStop()
        {
            _rightT      = _leftT = 0f;
            _rightRepQueue = _leftRepQueue = 0;
            if (_rightRepRoutine != null) StopCoroutine(_rightRepRoutine);
            if (_leftRepRoutine != null) StopCoroutine(_leftRepRoutine);
            _rightRepRoutine = null;
            _leftRepRoutine = null;

            RestoreDumbbell(_rightDb, _rightOrigParent, _rightOrigPos, _rightOrigRot, _rightOrigScale);
            RestoreDumbbell(_leftDb,  _leftOrigParent,  _leftOrigPos,  _leftOrigRot,  _leftOrigScale);

            _isWorking  = false;
            _clickCount = 0;
            _recentClickTimes.Clear();
            _currentTempoMultiplier = 1f;
            _currentClickSpeed = 0f;

            _player?.SetMovementLocked(false);
            workoutHUD?.HideAll();
            Debug.Log("[DW] Гантели положены на место");
        }

        private void RestoreDumbbell(Transform db, Transform origParent,
            Vector3 pos, Quaternion rot, Vector3 scale)
        {
            if (db == null) return;
            db.SetParent(origParent);
            db.position   = pos;
            db.rotation   = rot;
            db.localScale = scale;
        }

        // ── Клик ──────────────────────────────────────────────────
        private void OnClickPump()
        {
            if (!_isWorking || _session == null) return;

            _clickCount++;
            bool useRight = (_clickCount % 2 == 1);

            if (useRight)
            {
                _rightRepQueue++;
                if (_rightRepRoutine == null)
                    _rightRepRoutine = StartCoroutine(PlayRep(true));
            }
            else
            {
                _leftRepQueue++;
                if (_leftRepRoutine == null)
                    _leftRepRoutine = StartCoroutine(PlayRep(false));
            }

            // Стамина → эффективность
            RegisterTempoClick();
            float eff = (_stamina != null ? _stamina.EfficiencyMult : 1f) * _currentTempoMultiplier;

            // Регистрируем клик в сессии
            _session.RegisterClick(eff);

            // Расход стамины (если упали — сессия завершится через событие)
            if (_stamina != null && !_stamina.ConsumeForClick())
                _session.EndSession();
        }

        private IEnumerator PlayRep(bool right)
        {
            while (_isWorking && (right ? _rightRepQueue : _leftRepQueue) > 0)
            {
                if (right) _rightRepQueue--;
                else _leftRepQueue--;

                float repSpeed = Mathf.Max(1f, curlSpeed * (0.75f + _currentTempoMultiplier * 0.25f));
                float upDuration = 1f / repSpeed;
                float downDuration = 1f / (repSpeed * 0.92f);
                float topPause = Mathf.Lerp(holdTime, 0.04f, Mathf.InverseLerp(1f, maxTempoMultiplier, _currentTempoMultiplier));

                yield return AnimateArm(right, right ? _rightT : _leftT, 1f, upDuration);

                if (topPause > 0f)
                    yield return new WaitForSeconds(topPause);

                yield return AnimateArm(right, 1f, 0f, downDuration);
            }

            if (right) _rightRepRoutine = null;
            else _leftRepRoutine = null;
        }

        private IEnumerator AnimateArm(bool right, float from, float to, float duration)
        {
            float elapsed = 0f;
            duration = Mathf.Max(0.01f, duration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float value = Mathf.SmoothStep(from, to, t);
                if (right) _rightT = value;
                else _leftT = value;
                yield return null;
            }

            if (right) _rightT = to;
            else _leftT = to;
        }

        // ── Утилиты ───────────────────────────────────────────────
        private void RegisterTempoClick()
        {
            _recentClickTimes.Add(Time.time);
            RecalculateTempo();
        }

        private void UpdateTempo()
        {
            RecalculateTempo();
        }

        private void RecalculateTempo()
        {
            float threshold = Time.time - clickSpeedWindow;
            for (int i = _recentClickTimes.Count - 1; i >= 0; i--)
            {
                if (_recentClickTimes[i] < threshold)
                    _recentClickTimes.RemoveAt(i);
            }

            if (_recentClickTimes.Count == 0)
            {
                _currentClickSpeed = 0f;
                _currentTempoMultiplier = 1f;
            }
            else
            {
                _currentClickSpeed = _recentClickTimes.Count / clickSpeedWindow;
                _currentTempoMultiplier = Mathf.Clamp(1f + _currentClickSpeed * speedToMultiplier, 1f, maxTempoMultiplier);
            }

            workoutHUD?.SetTempo(_currentClickSpeed, _currentTempoMultiplier);
        }

        private void AssignDumbbells()
        {
            if (rightDumbbell != null && leftDumbbell != null)
            {
                Debug.Log("[DW] Гантели назначены через Inspector");
                return;
            }
            var found = FindObjectsByType<Dumbbell>(FindObjectsSortMode.None);
            if (rightDumbbell == null && found.Length > 0) rightDumbbell = found[0];
            if (leftDumbbell  == null && found.Length > 1) leftDumbbell  = found[1];
        }

        private void CacheDumbbellState()
        {
            _rightDb = rightDumbbell != null ? rightDumbbell.transform : null;
            _leftDb  = leftDumbbell  != null ? leftDumbbell.transform  : null;

            Cache(ref _rightDb, ref _rightOrigParent, ref _rightOrigPos, ref _rightOrigRot, ref _rightOrigScale);
            Cache(ref _leftDb,  ref _leftOrigParent,  ref _leftOrigPos,  ref _leftOrigRot,  ref _leftOrigScale);
        }

        private static void Cache(ref Transform db, ref Transform parent,
            ref Vector3 pos, ref Quaternion rot, ref Vector3 scale)
        {
            if (db == null) return;
            parent = db.parent;
            pos    = db.position;
            rot    = db.rotation;
            scale  = db.localScale;
            Debug.Log($"[DW] ✓ Гантель закэширована: {db.name}");
        }

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
    }
}
