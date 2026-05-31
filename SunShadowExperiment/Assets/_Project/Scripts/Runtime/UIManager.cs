using UnityEngine;
using UnityEngine.UI;

namespace SunShadow
{
    /// <summary>
    /// UI 管理器 — 面向小学生的大屏界面
    /// 64pt 大字、中文显示、彩色预设按钮、滑块控制
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("数据显示")]
        [SerializeField] private Text elevationValueText;     // "45°"
        [SerializeField] private Text elevationLabelText;     // "太阳高度角"
        [SerializeField] private Text azimuthValueText;       // "180°（南）"
        [SerializeField] private Text azimuthLabelText;       // "太阳方位角"
        [SerializeField] private Text shadowLengthText;       // "影子长度：2.0 米"
        [SerializeField] private Text hintText;               // 提示文字

        [Header("滑块")]
        [SerializeField] private Slider elevationSlider;      // 0-90
        [SerializeField] private Slider azimuthSlider;        // 0-360
        [SerializeField] private Text elevationSliderValue;   // 滑块当前值
        [SerializeField] private Text azimuthSliderValue;     // 滑块当前值

        [Header("预设按钮")]
        [SerializeField] private Button[] presetButtons;
        [SerializeField] private Text[] presetButtonTexts;

        [Header("功能按钮")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Text resetButtonText;

        [Header("答题板区域（预留）")]
        [SerializeField] private AnswerBoardManager answerBoard;

        [Header("引用")]
        [SerializeField] private ExperimentConfig config;

        private GameManager gm;

        void Start()
        {
            gm = GameManager.Instance;

            // 初始化滑块
            if (elevationSlider != null)
            {
                elevationSlider.minValue = config != null ? config.minElevationClamp : 0.5f;
                elevationSlider.maxValue = 90f;
                elevationSlider.value = gm.ElevationAngle;
                elevationSlider.onValueChanged.AddListener(OnElevationSliderChanged);
            }

            if (azimuthSlider != null)
            {
                azimuthSlider.minValue = 0f;
                azimuthSlider.maxValue = 360f;
                azimuthSlider.value = gm.AzimuthAngle;
                azimuthSlider.onValueChanged.AddListener(OnAzimuthSliderChanged);
            }

            // 预设按钮
            if (presetButtons != null && config != null)
            {
                for (int i = 0; i < presetButtons.Length && i < config.presets.Length; i++)
                {
                    int idx = i;
                    presetButtons[i].onClick.AddListener(() => OnPresetClicked(idx));
                    if (presetButtonTexts != null && i < presetButtonTexts.Length)
                        presetButtonTexts[i].text = config.presets[i].chineseName;
                }
            }

            // 重置按钮
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetClicked);
                if (resetButtonText != null)
                    resetButtonText.text = "重置";
            }

            // 订阅状态更新
            gm.OnExperimentUpdated += UpdateDisplays;

            // 首次刷新
            UpdateDisplays();

            // 显示提示
            if (hintText != null)
                hintText.text = "拖动下面的滑块，观察影子怎么变化吧！";
        }

        void OnDestroy()
        {
            if (gm != null)
                gm.OnExperimentUpdated -= UpdateDisplays;
        }

        // === 滑块回调 ===

        private void OnElevationSliderChanged(float value)
        {
            gm.SetElevationAngle(value);
            if (elevationSliderValue != null)
                elevationSliderValue.text = $"{value:F0}°";
        }

        private void OnAzimuthSliderChanged(float value)
        {
            gm.SetAzimuthAngle(value);
            if (azimuthSliderValue != null)
                azimuthSliderValue.text = $"{value:F0}°";
        }

        // === 预设回调 ===

        private void OnPresetClicked(int index)
        {
            if (config == null || index >= config.presets.Length) return;

            PresetData preset = config.presets[index];
            gm.ApplyPreset(preset);

            // 同步滑块位置
            if (elevationSlider != null) elevationSlider.value = preset.elevation;
            if (azimuthSlider != null) azimuthSlider.value = preset.azimuth;

            // 更新提示
            if (hintText != null)
            {
                string tip = preset.chineseName switch
                {
                    "日出" => "太阳刚升起，影子又长又暗",
                    "上午" => "太阳渐渐升高，影子变短了",
                    "正午" => "太阳在头顶，影子最短！",
                    "下午" => "太阳又降低了，影子慢慢变长",
                    "日落" => "太阳快落山了，影子变回很长",
                    _ => ""
                };
                hintText.text = tip;
            }
        }

        // === 重置 ===

        private void OnResetClicked()
        {
            gm.ResetToDefault();

            if (elevationSlider != null) elevationSlider.value = gm.ElevationAngle;
            if (azimuthSlider != null) azimuthSlider.value = gm.AzimuthAngle;

            if (hintText != null)
                hintText.text = "已重置，试试再调整看看吧！";
        }

        // === UI 刷新 ===

        public void UpdateDisplays()
        {
            if (gm == null) return;

            // 高度角显示
            if (elevationValueText != null)
                elevationValueText.text = $"{gm.ElevationAngle:F1}°";

            // 方位角显示（含中文方向）
            if (azimuthValueText != null)
            {
                string dir = AzimuthToChinese(gm.AzimuthAngle);
                azimuthValueText.text = $"{gm.AzimuthAngle:F1}°\n{dir}";
            }

            // 影子长度
            if (shadowLengthText != null)
            {
                if (gm.ShadowLength >= (config != null ? config.maxShadowDisplayLength * 0.95f : 28f))
                    shadowLengthText.text = $"影子很长很长！";
                else
                    shadowLengthText.text = $"{gm.ShadowLength:F1} 米";
            }
        }

        /// <summary>
        /// 方位角 → 中文方向
        /// </summary>
        private string AzimuthToChinese(float azimuth)
        {
            if (azimuth < 22.5f || azimuth >= 337.5f) return "北";
            if (azimuth < 67.5f)  return "东北";
            if (azimuth < 112.5f) return "东";
            if (azimuth < 157.5f) return "东南";
            if (azimuth < 202.5f) return "南";
            if (azimuth < 247.5f) return "西南";
            if (azimuth < 292.5f) return "西";
            return "西北";
        }
    }
}
