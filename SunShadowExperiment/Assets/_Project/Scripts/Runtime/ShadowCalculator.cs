using UnityEngine;

namespace SunShadow
{
    /// <summary>
    /// 影子计算器 — 三角公式计算 + 地面 LineRenderer 绘制
    /// 核心公式: 影子长度 = 杆子高度 / tan(太阳高度角)
    /// </summary>
    public class ShadowCalculator : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Transform poleBase;         // 杆子底部（地面位置）
        [SerializeField] private LineRenderer shadowLine;
        [SerializeField] private Transform shadowTipMarker; // 影子尖端小球

        [Header("参数")]
        [SerializeField] private float poleHeight = 2.0f;
        [SerializeField] private ExperimentConfig config;
        [Range(0.001f, 0.05f)]
        [SerializeField] private float yOffset = 0.002f;    // 略高于地面避免 Z-fighting

        [Header("可调样式")]
        [SerializeField] private float shadowLineWidth = 0.06f;
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.55f);

        // 公开属性
        public float CalculatedShadowLength { get; private set; }
        public float ShadowDirectionDegrees { get; private set; }

        void Start()
        {
            if (config != null)
            {
                shadowLineWidth = config.shadowLineWidth;
                shadowColor = config.shadowColor;
                if (poleHeight <= 0f) poleHeight = config.defaultPoleHeight;
            }

            SetupLineRenderer();
        }

        private void SetupLineRenderer()
        {
            if (shadowLine == null)
            {
                shadowLine = GetComponent<LineRenderer>();
                if (shadowLine == null)
                    shadowLine = gameObject.AddComponent<LineRenderer>();
            }

            shadowLine.positionCount = 2;
            shadowLine.startWidth = shadowLineWidth;
            shadowLine.endWidth = shadowLineWidth * 0.3f;    // 尖端变细
            shadowLine.startColor = shadowColor;
            shadowLine.endColor = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * 0.3f);
            shadowLine.material = new Material(Shader.Find("Sprites/Default"));
            shadowLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shadowLine.receiveShadows = false;
        }

        /// <summary>
        /// 更新影子 — 返回计算出的影子长度
        /// </summary>
        public float UpdateShadow(float elevationAngle, float azimuthAngle)
        {
            float elevationClamped = Mathf.Max(elevationAngle, config != null ? config.minElevationClamp : 0.5f);
            float elevRad = elevationClamped * Mathf.Deg2Rad;

            // 核心公式: L = H / tan(θ)
            CalculatedShadowLength = poleHeight / Mathf.Tan(elevRad);

            // 截断超长影子
            float maxLen = config != null ? config.maxShadowDisplayLength : 30f;
            CalculatedShadowLength = Mathf.Min(CalculatedShadowLength, maxLen);

            // 影子方向：与太阳方位角相反
            ShadowDirectionDegrees = (azimuthAngle + 180f) % 360f;
            float shadowDirRad = ShadowDirectionDegrees * Mathf.Deg2Rad;

            // 影子在地面的向量（XZ平面）
            Vector3 shadowDir = new Vector3(
                Mathf.Sin(shadowDirRad),  // X
                0f,
                Mathf.Cos(shadowDirRad)   // Z
            );

            // 更新 LineRenderer
            if (shadowLine != null)
            {
                Vector3 basePos = poleBase != null ? poleBase.position : transform.position;

                Vector3 start = basePos + Vector3.up * yOffset;
                Vector3 end = start + shadowDir * CalculatedShadowLength;
                end.y = start.y; // 保持在地面

                shadowLine.SetPosition(0, start);
                shadowLine.SetPosition(1, end);
            }

            // 更新影子尖端标记
            if (shadowTipMarker != null)
            {
                Vector3 basePos = poleBase != null ? poleBase.position : transform.position;
                Vector3 tipPos = basePos + shadowDir * CalculatedShadowLength;
                tipPos.y = yOffset;
                shadowTipMarker.position = tipPos;

                // 只在影子可见时显示
                float alpha = Mathf.Clamp01(CalculatedShadowLength / 0.2f);
                shadowTipMarker.gameObject.SetActive(alpha > 0.01f);
            }

            return CalculatedShadowLength;
        }

        public void SetPoleHeight(float height)
        {
            poleHeight = height;
        }
    }
}
