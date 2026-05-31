using UnityEditor;
using UnityEngine;

namespace SunShadow.Editor
{
    /// <summary>
    /// 一键 WebGL 构建脚本
    /// 菜单: Tools → 构建 WebGL
    /// </summary>
    public static class WebGLBuilder
    {
        [MenuItem("Tools/太阳影子实验/构建 WebGL (小体积)")]
        public static void BuildWebGLSmall()
        {
            BuildWebGL(false);
        }

        [MenuItem("Tools/太阳影子实验/构建 WebGL (开发调试)")]
        public static void BuildWebGLDebug()
        {
            BuildWebGL(true);
        }

        private static void BuildWebGL(bool debug)
        {
            // 确保平台已切换
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }

            // 场景
            string[] scenes = { "Assets/_Project/Scenes/MainScene.unity" };

            // 输出路径
            string buildPath = debug
                ? "Build/WebGL_Debug"
                : "Build/WebGL";

            // Player Settings 优化
            PlayerSettings.WebGL.memorySize = 256;
            PlayerSettings.WebGL.exceptionSupport = debug
                ? WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly
                : WebGLExceptionSupport.None;
            PlayerSettings.WebGL.dataCaching = !debug;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.decompressionFallback = true;

            // 色彩空间: Gamma (WebGL 更小更快)
            PlayerSettings.colorSpace = ColorSpace.Gamma;

            // Stripping
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(
                BuildTargetGroup.WebGL,
                debug ? ManagedStrippingLevel.Medium : ManagedStrippingLevel.High);

            // 模板
            PlayerSettings.WebGL.template = "PROJECT:KidFriendly";

            // Quality
            int qualityLevel = debug ? 1 : 0;
            QualitySettings.SetQualityLevel(qualityLevel, true);
            QualitySettings.shadowResolution = debug ? ShadowResolution.Medium : ShadowResolution.Low;
            QualitySettings.shadowCascades = 0; // 无级联
            QualitySettings.shadowDistance = 30;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.antiAliasing = 0;
            QualitySettings.softParticles = false;
            QualitySettings.vSyncCount = 0;

            // 构建
            BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WebGL,
                debug ? BuildOptions.Development : BuildOptions.None);

            // 输出大小
            if (!debug && System.IO.Directory.Exists(buildPath))
            {
                var dir = new System.IO.DirectoryInfo(buildPath);
                long totalSize = 0;
                foreach (var file in dir.GetFiles("*", System.IO.SearchOption.AllDirectories))
                    totalSize += file.Length;

                float sizeMB = totalSize / (1024f * 1024f);
                Debug.Log($"<color=green>WebGL 构建完成!</color> 总大小: <b>{sizeMB:F1} MB</b>");
                Debug.Log($"构建路径: {buildPath}");

                if (sizeMB > 25f)
                    Debug.LogWarning($"构建体积 {sizeMB:F1}MB 超过 25MB 目标！请检查纹理和模型设置。");
            }
        }
    }
}
