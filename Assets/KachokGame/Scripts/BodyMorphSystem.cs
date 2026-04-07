using System;
using UnityEngine;

namespace Tutorial
{
    [Serializable]
    public struct BodyStage
    {
        public string name;
        public int    muscleThreshold;
        [Range(0f, 100f)] public float blendWeight;
    }

    /// <summary>
    /// 5 стадий телосложения через BlendShapes.
    /// Muscle-очки → плавное изменение тела в реальном времени.
    /// </summary>
    public class BodyMorphSystem : MonoBehaviour
    {
        public static BodyMorphSystem Instance { get; private set; }

        [Header("Стадии")]
        [SerializeField] private BodyStage[] stages = new BodyStage[]
        {
            new BodyStage { name = "Тощий",     muscleThreshold = 0,    blendWeight = 0f   },
            new BodyStage { name = "Худой",      muscleThreshold = 100,  blendWeight = 25f  },
            new BodyStage { name = "Средний",    muscleThreshold = 300,  blendWeight = 50f  },
            new BodyStage { name = "Спортивный", muscleThreshold = 600,  blendWeight = 75f  },
            new BodyStage { name = "Огромный",   muscleThreshold = 1000, blendWeight = 100f },
        };

        [Header("BlendShape")]
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        [SerializeField] private int   blendShapeIndex = 0;
        [SerializeField] private float morphSpeed      = 2f;

        private int   _musclePoints;
        private float _blendTarget;
        private float _blendValue;
        private int   _stageIndex;

        // ── События ──────────────────────────────────────────────
        public event Action<int>             OnMusclePointsChanged; // (total)
        public event Action<int, BodyStage>  OnStageUnlocked;       // (index, stage)

        public int       MusclePoints     => _musclePoints;
        public int       CurrentStageIndex => _stageIndex;
        public BodyStage CurrentStage     => stages[_stageIndex];
        public int StageCount             => stages.Length;

        public int NextStageMuscle =>
            _stageIndex < stages.Length - 1
                ? stages[_stageIndex + 1].muscleThreshold
                : stages[stages.Length - 1].muscleThreshold;

        // ─────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (bodyRenderer == null) return;
            if (Mathf.Abs(_blendValue - _blendTarget) < 0.05f) return;

            _blendValue = Mathf.MoveTowards(_blendValue, _blendTarget, morphSpeed * Time.deltaTime);
            bodyRenderer.SetBlendShapeWeight(blendShapeIndex, _blendValue);
        }

        public void AddMusclePoints(int amount)
        {
            _musclePoints += amount;
            OnMusclePointsChanged?.Invoke(_musclePoints);
            RecalcStage();
        }

        private void RecalcStage()
        {
            int newIdx = 0;
            for (int i = stages.Length - 1; i >= 0; i--)
            {
                if (_musclePoints >= stages[i].muscleThreshold) { newIdx = i; break; }
            }

            _blendTarget = CalcBlend(newIdx);

            if (newIdx != _stageIndex)
            {
                _stageIndex = newIdx;
                OnStageUnlocked?.Invoke(_stageIndex, stages[_stageIndex]);
                Debug.Log($"[BodyMorph] Новая стадия: {stages[_stageIndex].name}");
            }
        }

        private float CalcBlend(int idx)
        {
            if (idx >= stages.Length - 1) return stages[stages.Length - 1].blendWeight;
            float t = Mathf.InverseLerp(stages[idx].muscleThreshold,
                                        stages[idx + 1].muscleThreshold, _musclePoints);
            return Mathf.Lerp(stages[idx].blendWeight, stages[idx + 1].blendWeight, t);
        }
    }
}
