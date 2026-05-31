using System;
using UnityEngine;

namespace SunShadow
{
    /// <summary>
    /// 游戏中心管理器 — 单例，协调所有模块
    /// 所有角度的唯一真实来源，其他模块通过它同步状态
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("配置")]
        [SerializeField] private ExperimentConfig config;

        [Header("模块引用")]
        [SerializeField] private SunController sunController;
        [SerializeField] private ShadowCalculator shadowCalculator;
        [SerializeField] private UIManager uiManager;

        // === 实验状态 ===
        public float ElevationAngle { get; private set; }  // 太阳高度角 0-90°
        public float AzimuthAngle  { get; private set; }   // 太阳方位角 0-360°
        public float ShadowLength  { get; private set; }   // 影子长度（米）
        public float PoleHeight    { get; private set; }   // 杆子高度（米）

        /// <summary>实验状态更新事件 — UIManager 订阅</summary>
        public event Action OnExperimentUpdated;

        void Awake()
        {
            // 单例
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 应用默认配置
            PoleHeight    = config.defaultPoleHeight;
            ElevationAngle = config.defaultElevation;
            AzimuthAngle  = config.defaultAzimuth;
        }

        void Start()
        {
            // 初始化所有模块
            shadowCalculator?.SetPoleHeight(PoleHeight);
            UpdateExperimentState();
        }

        // === 外部调用 ===

        public void SetElevationAngle(float degrees)
        {
            ElevationAngle = Mathf.Clamp(degrees, config.minElevationClamp, 90f);
            UpdateExperimentState();
        }

        public void SetAzimuthAngle(float degrees)
        {
            AzimuthAngle = degrees % 360f;
            if (AzimuthAngle < 0f) AzimuthAngle += 360f;
            UpdateExperimentState();
        }

        public void SetPoleHeight(float height)
        {
            PoleHeight = Mathf.Clamp(height, config.minPoleHeight, config.maxPoleHeight);
            shadowCalculator?.SetPoleHeight(PoleHeight);
            UpdateExperimentState();
        }

        public void ApplyPreset(PresetData preset)
        {
            ElevationAngle = preset.elevation;
            AzimuthAngle = preset.azimuth;
            UpdateExperimentState();
        }

        public void ResetToDefault()
        {
            ElevationAngle = config.defaultElevation;
            AzimuthAngle = config.defaultAzimuth;
            PoleHeight = config.defaultPoleHeight;
            shadowCalculator?.SetPoleHeight(PoleHeight);
            UpdateExperimentState();
        }

        // === 内部计算 ===

        private void UpdateExperimentState()
        {
            // 1. 更新太阳位置
            sunController?.UpdateSunPosition(ElevationAngle, AzimuthAngle);

            // 2. 计算影子
            ShadowLength = shadowCalculator != null
                ? shadowCalculator.UpdateShadow(ElevationAngle, AzimuthAngle)
                : 0f;

            // 3. 通知 UI
            OnExperimentUpdated?.Invoke();
        }
    }
}
