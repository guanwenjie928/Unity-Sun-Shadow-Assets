using UnityEngine;

namespace SunShadow
{
    /// <summary>
    /// 太阳控制器 — 根据高度角和方位角旋转方向光
    /// 同时管理阴影质量、太阳视觉指示器、地面方向箭头
    /// </summary>
    public class SunController : MonoBehaviour
    {
        [Header("光照")]
        [SerializeField] private Light sunLight;
        [SerializeField] private float lightIntensity = 1.3f;

        [Header("视觉太阳（可选）")]
        [SerializeField] private Transform sunVisualOrb;       // 天空中小球代表太阳位置
        [SerializeField] private float sunVisualDistance = 20f;

        [Header("地面方向箭头")]
        [SerializeField] private Transform groundArrow;         // 平放在地上指向太阳方位

        [Header("阴影")]
        [SerializeField] private float lowElevationThreshold = 10f;  // 低于此角度阴影变柔和
        [SerializeField] private float shadowStrength = 0.85f;

        void Start()
        {
            if (sunLight == null)
                sunLight = GetComponent<Light>();

            // 确保方向光开启阴影
            if (sunLight != null)
            {
                sunLight.shadows = LightShadows.Soft;
                sunLight.shadowStrength = shadowStrength;
                sunLight.intensity = lightIntensity;
            }
        }

        /// <summary>
        /// 更新太阳位置
        /// </summary>
        /// <param name="elevation">高度角: 0=地平线, 90=头顶</param>
        /// <param name="azimuth">方位角: 0=北, 90=东, 180=南, 270=西</param>
        public void UpdateSunPosition(float elevation, float azimuth)
        {
            Vector3 sunDirection = AnglesToDirection(elevation, azimuth);

            // 旋转方向光（光照方向 = -sunDirection, 即太阳指向地面）
            if (sunLight != null)
            {
                // 方向光的 forward 为照射方向，所以太阳在反方向
                sunLight.transform.forward = -sunDirection;
                // 低角度柔化阴影
                sunLight.shadowStrength = elevation < lowElevationThreshold
                    ? Mathf.Lerp(0.5f, shadowStrength, elevation / lowElevationThreshold)
                    : shadowStrength;
            }

            // 移动视觉太阳球
            if (sunVisualOrb != null)
            {
                sunVisualOrb.position = -sunDirection * sunVisualDistance;
            }

            // 旋转地面箭头（箭头Y轴朝上，Z轴指向太阳方位）
            if (groundArrow != null)
            {
                Vector3 flatDir = new Vector3(sunDirection.x, 0f, sunDirection.z).normalized;
                if (flatDir.sqrMagnitude > 0.001f)
                    groundArrow.rotation = Quaternion.LookRotation(flatDir, Vector3.up);
            }
        }

        /// <summary>
        /// 球坐标 → Unity 世界方向
        /// 高度角 0=地平线, 90=头顶; 方位角 0=北(+Z), 90=东(+X), 180=南(-Z), 270=西(-X)
        /// 返回方向: 从太阳指向地面的向量
        /// </summary>
        private Vector3 AnglesToDirection(float elevationDeg, float azimuthDeg)
        {
            float elevRad = elevationDeg * Mathf.Deg2Rad;
            float azimRad = azimuthDeg * Mathf.Deg2Rad;

            // 太阳从天空照向地面 (-Y 方向)，
            // 水平分量由 azimuth 决定
            float cosElev = Mathf.Cos(elevRad);
            float sinElev = Mathf.Sin(elevRad);

            return new Vector3(
                cosElev * Mathf.Sin(azimRad),   // X: 东(+X) ← 方位角90
                -sinElev,                        // Y: 上(+Y) → 太阳头顶时为 sin(90)=1, dir=(-Y)
                cosElev * Mathf.Cos(azimRad)    // Z: 北(+Z) ← 方位角0
            );
        }
    }
}
