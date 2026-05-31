using UnityEngine;

namespace SunShadow
{
    /// <summary>
    /// 实验配置 — ScriptableObject
    /// 集中管理所有可调参数，方便课堂不同教学需求
    /// </summary>
    [CreateAssetMenu(fileName = "ExperimentConfig", menuName = "太阳影子实验/实验配置")]
    public class ExperimentConfig : ScriptableObject
    {
        [Header("杆子设置")]
        [Tooltip("默认杆子高度（米）")]
        public float defaultPoleHeight = 2.0f;
        [Tooltip("最小杆高")]
        public float minPoleHeight = 0.5f;
        [Tooltip("最大杆高")]
        public float maxPoleHeight = 5.0f;

        [Header("太阳默认角度")]
        [Range(0f, 90f)]
        [Tooltip("默认太阳高度角")]
        public float defaultElevation = 45f;
        [Range(0f, 360f)]
        [Tooltip("默认太阳方位角 (0=北, 90=东, 180=南, 270=西)")]
        public float defaultAzimuth = 180f;

        [Header("影子显示")]
        public Color shadowColor = new Color(0f, 0f, 0f, 0.55f);
        [Range(0.01f, 0.2f)]
        public float shadowLineWidth = 0.06f;
        [Tooltip("影子最大显示长度，超过此值截断")]
        public float maxShadowDisplayLength = 30f;
        [Tooltip("最小高度角，避免除零")]
        [Range(0.1f, 5f)]
        public float minElevationClamp = 0.5f;

        [Header("预设角度")]
        public PresetData[] presets = new PresetData[]
        {
            new PresetData { chineseName = "日出", elevation = 3f,  azimuth = 90f },
            new PresetData { chineseName = "上午", elevation = 30f, azimuth = 120f },
            new PresetData { chineseName = "正午", elevation = 75f, azimuth = 180f },
            new PresetData { chineseName = "下午", elevation = 30f, azimuth = 240f },
            new PresetData { chineseName = "日落", elevation = 3f,  azimuth = 270f },
        };
    }

    [System.Serializable]
    public struct PresetData
    {
        public string chineseName;
        [Range(0f, 90f)]
        public float elevation;
        [Range(0f, 360f)]
        public float azimuth;
    }
}
