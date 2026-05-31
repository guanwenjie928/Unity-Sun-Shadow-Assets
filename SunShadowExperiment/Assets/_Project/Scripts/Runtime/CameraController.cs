using UnityEngine;

namespace SunShadow
{
    /// <summary>
    /// 轨道相机控制器 — 右键拖拽旋转，滚轮缩放
    /// 支持桌面鼠标和移动端触摸
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("目标")]
        [SerializeField] private Transform target;              // 围绕哪个点旋转（杆子底部）
        [SerializeField] private Vector3 targetOffset = Vector3.zero;

        [Header("距离")]
        [SerializeField] private float distance = 8f;
        [SerializeField] private float minDistance = 3f;
        [SerializeField] private float maxDistance = 20f;

        [Header("角度")]
        [SerializeField] private float pitch = 45f;             // 俯仰角
        [SerializeField] private float yaw = 0f;                // 水平旋转角
        [SerializeField] private Vector2 pitchLimits = new Vector2(5f, 80f);  // 最小/最大俯仰

        [Header("速度")]
        [SerializeField] private float rotateSpeed = 3f;
        [SerializeField] private float zoomSpeed = 3f;
        [SerializeField] private float smoothTime = 0.15f;

        // 默认值（用于重置）
        private float defaultDistance = 8f;
        private float defaultPitch = 45f;
        private float defaultYaw = 0f;

        // 平滑
        private float targetDistance;
        private float targetPitch;
        private float targetYaw;
        private float velDistance;
        private float velPitch;
        private float velYaw;

        // 触摸
        private Vector2 lastTouchPos;
        private float lastTouchDistance;

        void Start()
        {
            defaultDistance = distance;
            defaultPitch = pitch;
            defaultYaw = yaw;

            targetDistance = distance;
            targetPitch = pitch;
            targetYaw = yaw;
        }

        void Update()
        {
            HandleInput();
        }

        void LateUpdate()
        {
            // 平滑过渡
            distance = Mathf.SmoothDamp(distance, targetDistance, ref velDistance, smoothTime);
            pitch    = Mathf.SmoothDamp(pitch, targetPitch, ref velPitch, smoothTime);
            yaw      = Mathf.SmoothDampAngle(yaw, targetYaw, ref velYaw, smoothTime);

            pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

            // 计算位置
            Vector3 targetPos = target != null ? target.position + targetOffset : targetOffset;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 negDistance = new Vector3(0f, 0f, -distance);
            transform.position = targetPos + rot * negDistance;

            // 始终看向目标
            transform.LookAt(targetPos);
        }

        private void HandleInput()
        {
            // 桌面：右键拖拽旋转
            if (Input.GetMouseButton(1))
            {
                targetYaw   += Input.GetAxis("Mouse X") * rotateSpeed * 5f;
                targetPitch -= Input.GetAxis("Mouse Y") * rotateSpeed * 5f;
                targetPitch  = Mathf.Clamp(targetPitch, pitchLimits.x, pitchLimits.y);
            }

            // 桌面：滚轮缩放
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                targetDistance -= scroll * zoomSpeed * 3f;
                targetDistance  = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            }

            // 移动端触摸
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved)
                {
                    targetYaw   += t.deltaPosition.x * rotateSpeed * 0.3f;
                    targetPitch -= t.deltaPosition.y * rotateSpeed * 0.3f;
                    targetPitch  = Mathf.Clamp(targetPitch, pitchLimits.x, pitchLimits.y);
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);
                float currentDist = (t0.position - t1.position).magnitude;

                if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                    lastTouchDistance = currentDist;
                else
                {
                    float delta = lastTouchDistance - currentDist;
                    targetDistance += delta * zoomSpeed * 0.02f;
                    targetDistance  = Mathf.Clamp(targetDistance, minDistance, maxDistance);
                    lastTouchDistance = currentDist;
                }
            }
        }

        /// <summary>重置到默认视角</summary>
        public void ResetView()
        {
            targetDistance = defaultDistance;
            targetPitch    = defaultPitch;
            targetYaw      = defaultYaw;
        }
    }
}
