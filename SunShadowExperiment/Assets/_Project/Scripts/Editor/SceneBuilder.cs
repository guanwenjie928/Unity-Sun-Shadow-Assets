using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace SunShadow.Editor
{
    /// <summary>
    /// 一键生成完整实验场景 — 自动搭建地面、杆子、太阳、UI
    /// 菜单: Tools → 太阳影子实验 → 自动搭建场景
    /// </summary>
    public static class SceneBuilder
    {
        [MenuItem("Tools/太阳影子实验/自动搭建场景")]
        public static void BuildScene()
        {
            // 新场景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === 配置 ===
            var config = AssetDatabase.LoadAssetAtPath<ExperimentConfig>(
                "Assets/_Project/Data/ExperimentConfig.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ExperimentConfig>();
                AssetDatabase.CreateAsset(config, "Assets/_Project/Data/ExperimentConfig.asset");
            }

            // === GameManager ===
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();

            // === 地面 ===
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(1, 1, 1);

            var groundMat = new Material(Shader.Find("Standard"));
            var colorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Textures/Ground103/Ground103_2K-JPG_Color.jpg");
            if (colorTex != null)
            {
                groundMat.mainTexture = colorTex;
                // 尝试加载其他 PBR 贴图
                var normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/Textures/Ground103/Ground103_2K-JPG_NormalGL.jpg");
                if (normalTex != null)
                {
                    groundMat.SetTexture("_BumpMap", normalTex);
                    groundMat.EnableKeyword("_NORMALMAP");
                }
                var roughTex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/Textures/Ground103/Ground103_2K-JPG_Roughness.jpg");
                if (roughTex != null)
                    groundMat.SetTexture("_MetallicGlossMap", roughTex);
            }
            groundMat.SetFloat("_Smoothness", 0.1f);
            ground.GetComponent<MeshRenderer>().material = groundMat;
            AssetDatabase.CreateAsset(groundMat, "Assets/_Project/Materials/Ground.mat");

            // === 杆子 (先用内置 Cylinder，模型导入后替换) ===
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.position = new Vector3(0, 1f, 0);
            pole.transform.localScale = new Vector3(0.15f, 1f, 0.15f);
            var poleMat = new Material(Shader.Find("Standard"));
            poleMat.color = new Color(0.3f, 0.3f, 0.35f);
            pole.GetComponent<MeshRenderer>().material = poleMat;
            AssetDatabase.CreateAsset(poleMat, "Assets/_Project/Materials/Pole.mat");

            // ShadowCalculator 挂杆子下
            var shadowGo = new GameObject("ShadowIndicator");
            shadowGo.transform.SetParent(pole.transform);
            shadowGo.transform.localPosition = Vector3.zero;
            var shadowCalc = shadowGo.AddComponent<ShadowCalculator>();
            // 通过反射设置私有字段 (Editor only)
            var so = new SerializedObject(shadowCalc);
            so.FindProperty("poleBase").objectReferenceValue = pole.transform;
            so.ApplyModifiedProperties();

            // 创建影子尖端标记
            var tipMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tipMarker.name = "ShadowTipMarker";
            tipMarker.transform.localScale = Vector3.one * 0.08f;
            var tipMat = new Material(Shader.Find("Standard"));
            tipMat.color = Color.black;
            tipMarker.GetComponent<MeshRenderer>().material = tipMat;
            tipMarker.transform.SetParent(shadowGo.transform);
            so.FindProperty("shadowTipMarker").objectReferenceValue = tipMarker.transform;
            so.ApplyModifiedProperties();

            // === 太阳光 ===
            var sunGo = new GameObject("SunLight");
            var sunLight = sunGo.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.shadows = LightShadows.Soft;
            sunLight.shadowStrength = 0.85f;
            sunLight.intensity = 1.3f;
            sunLight.shadowResolution = ShadowResolution.Medium;
            sunLight.shadowNearPlane = 0.1f;
            var sunCtrl = sunGo.AddComponent<SunController>();

            // 视觉太阳球
            var sunVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sunVisual.name = "SunVisualOrb";
            sunVisual.transform.localScale = Vector3.one * 0.5f;
            var sunVisMat = new Material(Shader.Find("Standard"));
            sunVisMat.color = new Color(1f, 0.85f, 0.3f);
            sunVisMat.EnableKeyword("_EMISSION");
            sunVisMat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.2f) * 2f);
            sunVisual.GetComponent<MeshRenderer>().material = sunVisMat;
            var sunSo = new SerializedObject(sunCtrl);
            sunSo.FindProperty("sunVisualOrb").objectReferenceValue = sunVisual.transform;
            sunSo.FindProperty("sunLight").objectReferenceValue = sunLight;
            sunSo.ApplyModifiedProperties();

            // 地面箭头
            var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.name = "SunGroundArrow";
            arrow.transform.position = Vector3.zero;
            arrow.transform.localScale = new Vector3(0.1f, 0.02f, 1.5f);
            var arrowMat = new Material(Shader.Find("Standard"));
            arrowMat.color = new Color(1f, 0.55f, 0.2f); // 橙色
            arrowMat.EnableKeyword("_EMISSION");
            arrowMat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * 1.5f);
            arrow.GetComponent<MeshRenderer>().material = arrowMat;
            sunSo.FindProperty("groundArrow").objectReferenceValue = arrow.transform;
            sunSo.ApplyModifiedProperties();

            // === 指南针标记 ===
            var directions = new[] { ("北", new Vector3(0, 0.001f, 3f)),
                                     ("东", new Vector3(3f, 0.001f, 0f)),
                                     ("南", new Vector3(0, 0.001f, -3f)),
                                     ("西", new Vector3(-3f, 0.001f, 0f)) };
            foreach (var (name, pos) in directions)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = $"{name}_Marker";
                marker.transform.position = pos;
                marker.transform.localScale = new Vector3(0.3f, 0.005f, 0.3f);
            }

            // === 相机 ===
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }
            cam.transform.position = new Vector3(0, 6f, -7f);
            cam.transform.LookAt(new Vector3(0, 1f, 0));
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.4f, 0.65f, 0.9f); // 浅蓝天空
            var camCtrl = cam.gameObject.AddComponent<CameraController>();
            var camSo = new SerializedObject(camCtrl);
            camSo.FindProperty("target").objectReferenceValue = pole.transform;
            camSo.ApplyModifiedProperties();

            // === UI Canvas ===
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var uiManager = canvasGo.AddComponent<UIManager>();

            // 左上：标题
            var titleGo = CreateUIText(canvasGo.transform, "Title", "太阳与影子夹角实验",
                48, TextAnchor.MiddleLeft, new Vector2(30, -30), new Vector2(0, 1));
            titleGo.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 70);

            // 左上：高度角显示
            var elevLabel = CreateUIText(canvasGo.transform, "ElevLabel", "太阳高度角",
                36, TextAnchor.MiddleLeft, new Vector2(30, -120), new Vector2(0, 1));
            var elevValue = CreateUIText(canvasGo.transform, "ElevValue", "45°",
                64, TextAnchor.MiddleLeft, new Vector2(220, -120), new Vector2(0, 1));
            elevValue.GetComponent<Text>().color = new Color(1f, 0.42f, 0.21f); // #FF6B35

            // 左上：方位角显示
            var azimLabel = CreateUIText(canvasGo.transform, "AzimLabel", "太阳方位角",
                36, TextAnchor.MiddleLeft, new Vector2(30, -210), new Vector2(0, 1));
            var azimValue = CreateUIText(canvasGo.transform, "AzimValue", "180°\n南",
                48, TextAnchor.MiddleLeft, new Vector2(220, -210), new Vector2(0, 1));

            // 中央：影长
            var shadowText = CreateUIText(canvasGo.transform, "ShadowLength", "影子长度：2.0 米",
                56, TextAnchor.MiddleCenter, new Vector2(0, -600), new Vector2(0.5f, 1));
            shadowText.GetComponent<Text>().color = new Color(0.1f, 0.58f, 0.37f); // #1A936F

            // 左下：高度角滑块
            var elevSliderGo = CreateSlider(canvasGo.transform, "ElevationSlider",
                "上下拖动", new Vector2(40, -350), new Vector2(0, 1), new Vector2(60, 500));
            var elevSlider = elevSliderGo.GetComponent<Slider>();
            elevSlider.minValue = 0.5f; elevSlider.maxValue = 90f; elevSlider.value = 45f;
            elevSlider.direction = Slider.Direction.BottomToTop;

            // 左下：方位角滑块
            var azimSliderGo = CreateSlider(canvasGo.transform, "AzimuthSlider",
                "左右拖动", new Vector2(40, -850), new Vector2(0, 1), new Vector2(500, 60));

            // 预设按钮
            var presetNames = new[] { "日出", "上午", "正午", "下午", "日落" };
            var presetBtns = new Button[5];
            for (int i = 0; i < 5; i++)
            {
                var btnGo = CreateUIButton(canvasGo.transform, $"Preset_{presetNames[i]}",
                    presetNames[i], 32, new Vector2(140 + i * 140, -960), new Vector2(0, 1),
                    new Vector2(120, 50));
                presetBtns[i] = btnGo.GetComponent<Button>();
            }

            // 右上：重置按钮
            var resetGo = CreateUIButton(canvasGo.transform, "Reset", "重置视角",
                28, new Vector2(-30, -30), new Vector2(1, 1), new Vector2(120, 50));

            // 提示文字
            var hintGo = CreateUIText(canvasGo.transform, "Hint",
                "拖动滑块，观察影子的变化吧！",
                28, TextAnchor.MiddleCenter, new Vector2(0, -700), new Vector2(0.5f, 1));
            hintGo.GetComponent<Text>().color = new Color(0.4f, 0.4f, 0.4f);

            // 连线 UIManager
            var uiSo = new SerializedObject(uiManager);
            uiSo.FindProperty("elevationValueText").objectReferenceValue = elevValue.GetComponent<Text>();
            uiSo.FindProperty("azimuthValueText").objectReferenceValue = azimValue.GetComponent<Text>();
            uiSo.FindProperty("shadowLengthText").objectReferenceValue = shadowText.GetComponent<Text>();
            uiSo.FindProperty("elevationSlider").objectReferenceValue = elevSlider;
            uiSo.FindProperty("azimuthSlider").objectReferenceValue = azimSliderGo.GetComponent<Slider>();
            uiSo.FindProperty("hintText").objectReferenceValue = hintGo.GetComponent<Text>();
            uiSo.FindProperty("resetButton").objectReferenceValue = resetGo.GetComponent<Button>();
            uiSo.FindProperty("presetButtons").arraySize = 5;
            for (int i = 0; i < 5; i++)
                uiSo.FindProperty("presetButtons").GetArrayElementAtIndex(i)
                    .objectReferenceValue = presetBtns[i];
            uiSo.FindProperty("config").objectReferenceValue = config;
            uiSo.ApplyModifiedProperties();

            // 连线 GameManager
            var gmSo = new SerializedObject(gm);
            gmSo.FindProperty("sunController").objectReferenceValue = sunCtrl;
            gmSo.FindProperty("shadowCalculator").objectReferenceValue = shadowCalc;
            gmSo.FindProperty("uiManager").objectReferenceValue = uiManager;
            gmSo.FindProperty("config").objectReferenceValue = config;
            gmSo.ApplyModifiedProperties();

            // === 保存场景 ===
            EditorSceneManager.SaveScene(scene, "Assets/_Project/Scenes/MainScene.unity");
            Debug.Log("<color=green>场景搭建完成!</color> 保存为 MainScene.unity");
            Debug.Log("请检查模型和纹理是否正确导入，拖入杆子模型替换内置 Cylinder。");
        }

        private static GameObject CreateUIText(Transform parent, string name, string text,
            int fontSize, TextAnchor anchor, Vector2 anchoredPos, Vector2 pivot)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = pivot; rt.anchorMax = pivot;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(400, fontSize + 20);
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = anchor;
            txt.color = Color.black;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return go;
        }

        private static GameObject CreateSlider(Transform parent, string name,
            string label, Vector2 anchoredPos, Vector2 pivot, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = pivot; rt.anchorMax = pivot; rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var slider = go.AddComponent<Slider>();
            // 默认背景和填充用 Unity 内置
            return go;
        }

        private static GameObject CreateUIButton(Transform parent, string name,
            string text, int fontSize, Vector2 anchoredPos, Vector2 pivot, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = pivot; rt.anchorMax = pivot; rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 0.42f, 0.21f); // 橙色
            var btn = go.AddComponent<Button>();
            // 文本子对象
            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(go.transform);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;
            var txt = txtGo.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return go;
        }
    }
}
