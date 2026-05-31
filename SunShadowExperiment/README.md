# 太阳与影子夹角实验 — Unity WebGL

面向中国小学科学课的互动实验。学生通过拖动滑块调整太阳高度角和方位角，实时观察竖杆影子的长度和方向变化。

## 项目结构

```
SunShadowExperiment/
├── Assets/
│   ├── _Project/
│   │   ├── Scenes/               → MainScene.unity（主场景）
│   │   ├── Scripts/Runtime/      → 6 个运行时脚本
│   │   │   ├── GameManager.cs        中心协调器（单例）
│   │   │   ├── SunController.cs      太阳光旋转 + 阴影
│   │   │   ├── ShadowCalculator.cs   影长计算 + 地面绘制
│   │   │   ├── UIManager.cs          全屏 UI（小学中文）
│   │   │   ├── AnswerBoardManager.cs 12 小组答题板（预留）
│   │   │   └── CameraController.cs   轨道相机
│   │   ├── Scripts/Data/         → 配置数据
│   │   ├── Scripts/Editor/       → 构建脚本
│   │   ├── Materials/
│   │   └── Prefabs/
│   ├── Models/                   → 放入选好的 5-8 个 GLB
│   ├── Textures/                 → 放入 Ground103 四张贴图
│   └── WebGLTemplates/KidFriendly/ → 中文友好加载页
├── Packages/
└── ProjectSettings/
```

## 快速开始

### 1. 准备素材
从这个仓库的 `UnityAssets/` 目录复制文件到项目中：

**模型**（复制到 `Assets/Models/`）：
只选这些小的 .glb 文件（排除超过 500KB 的大文件）：
```
0asfS0AAh3w.glb (12K)    → 棍子
1EhSSu2nFJA.glb (16K)    → 棍子
1TUCDmHZKQY.glb (4K)     → 箭头（太阳方向指示）
1GYd3tw5LqW.glb (40K)    → 方块
3Oqw0rLIFtf.glb (4K)     → 球体（太阳视觉球）
0fE2T9eRA9V.glb (8K)     → 标记点（影子尖端）
QpDGgpHcH2.glb (52K)     → 地面平台
0okBVouy3-T.glb (83K)    → 杆子（主竖杆）
```

**纹理**（复制到 `Assets/Textures/Ground103/`）：
只复制这 4 个 JPG：
```
Ground103_2K-JPG_Color.jpg
Ground103_2K-JPG_NormalGL.jpg
Ground103_2K-JPG_Roughness.jpg
Ground103_2K-JPG_AmbientOcclusion.jpg
```

### 2. 关键优化设置

在 Unity Editor 中打开项目后，逐一配置：

#### 纹理导入设置 (选中每个 JPG)
- **Max Size**: 1024（不是 2048）
- **Compression**: High Quality
- **Crunch Compression**: 勾选，Quality 50
- **sRGB**: Color 贴图勾选，NormalGL/Roughness/AO 不勾选

#### 模型导入设置 (选中每个 GLB)
- **Mesh Compression**: 选 Low 或 Medium
- **Read/Write**: 不勾选
- **Import Materials**: 勾选
- **Material Creation Mode**: Standard

### 3. 搭建场景

按以下层级创建（或直接看 `MainScene.unity`）：

```
新建场景 → 保存为 Assets/_Project/Scenes/MainScene.unity

1. 创建空对象 "GameManager"，挂 GameManager.cs
2. 创建空对象 "SunController"，挂 SunController.cs
3. Directional Light → 挂 SunController
   - Shadow Type: Soft Shadows
   - Shadow Resolution: Medium
   - Strength: 1.3
4. Plane → Scale (10,1,10) → 做地面
   - 新建 Material，Albedo 贴 Ground103_Color
   - Normal Map 贴 Ground103_NormalGL
   - Smoothness: 0.1
5. 拖入杆子模型 → Position (0, 0.5, 0)
   - 新建空子对象 "ShadowIndicator" → 挂 ShadowCalculator
6. Main Camera → 挂 CameraController
   - Target: 杆子底部
   - Clear Flags: Solid Color (浅蓝天空)
7. Canvas → 参考下方 UI 布局搭建
```

### 4. 构建 WebGL

```
菜单: Tools → 太阳影子实验 → 构建 WebGL (小体积)
```

构建脚本会自动：
- 切换为 WebGL 平台
- 设置 Gamma 色彩空间
- 开启最大代码剥离
- 关闭异常支持（减小体积）
- 禁用阴影级联
- 使用 KidFriendly 中文加载模板

构建输出在 `Build/WebGL/`，预期总体积 17-22MB。

## UI 布局参考 (1920x1080)

```
+------------------------------------------------------------------+
|  [标题] 太阳与影子夹角实验                   [各小组答题情况]    |
+--------+---------------------------------+------------------------+
| 太阳   |                                 | [一组]⚪ [二组]⚪     |
|高度角  |       3D 实验观察区              | [三组]⚪ [四组]⚪    |
| 45°    |                                 | [五组]⚪ [六组]⚪    |
|        |    (杆子 + 地面 + 影子)         | [七组]⚪ [八组]⚪    |
| [滑块] |                                 | [九组]⚪ [十组]⚪    |
|        |                                 | [十一]⚪ [十二]⚪    |
| 太阳   |                                 |                       |
|方位角  |                                 |                       |
| 180°南 |                                 |                       |
| [滑块] |  影子长度: 2.0 米               |                       |
|        |                                 | [重置视角]            |
|[日出] [上午] [正午] [下午] [日落]        |                       |
+--------+---------------------------------+------------------------+
```

## 答题系统预留

`AnswerBoardManager.cs` 已提供完整接口：

```csharp
// 答题阶段接入，只需调用：
AnswerBoardManager board = FindObjectOfType<AnswerBoardManager>();
board.SetSlotState(1, GroupAnswerData.AnswerState.Correct);  // 第一组正确
board.SetSlotState(2, GroupAnswerData.AnswerState.Wrong);    // 第二组错误
board.ResetAllSlots();                                        // 全部重置
```

`GroupAnswerData.cs` 中包含：
- 12 组数据结构
- `SubmitAnswer()` 预留方法
- `MarkCorrect()` / `MarkWrong()`
- `GetCorrectCount()` / `GetWrongCount()` 统计

未来只需添加 `QuestionManager.cs`（出题 → 接收答案 → 判断 → 更新答题板），无需修改现有架构。

## WebGL 体积估算

| 组件 | 大小 |
|------|------|
| IL2CPP 引擎代码 (高剥离) | 8-10 MB |
| C# → wasm | 1-2 MB |
| 模型 (8个小GLB, 压缩) | < 1 MB |
| 纹理 (1K Crunch * 4张) | 3-5 MB |
| Shader (最小集) | 1-2 MB |
| 加载页面 + 其他 | < 1 MB |
| **总计** | **15-21 MB** |

## 素材来源 & 许可

| 来源 | 协议 |
|------|------|
| 3D 模型: [Poly Pizza](https://poly.pizza) | CC0 / CC-BY |
| 纹理: [ambientCG](https://ambientcg.com) | CC0 |
| 代码: 原创 | MIT |

---

## Unity 版本要求
- Unity 2021.3 LTS 或更高
- WebGL 构建模块

### 本地打开
```bash
git clone https://github.com/guanwenjie928/Unity-Sun-Shadow-Assets.git
# 把 SunShadowExperiment/ 文件夹拖入 Unity Hub
# 等待导入完成，按照上方步骤配置贴图和模型
# Tools → 太阳影子实验 → 构建 WebGL (小体积)
```
