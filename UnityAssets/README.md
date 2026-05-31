# Unity 太阳与影子夹角实验 — 素材包

## 素材清单

### 地面纹理 (Textures/) — 3 套 PBR 完整材质
| 纹理包 | 说明 | 包含贴图 |
|--------|------|----------|
| **Ground103** | 泥土/沙地 — 阴影对比度清晰，最适合投影实验 | Color, Normal(DX/GL), Displacement, AO, Roughness |
| **Ground074** | 干燥地面 — 浅色表面，影子边缘锐利 | 同上 |
| **Gravel043** | 碎石地面 — 适合做自然场景变体 | 同上 |

> 全部来自 [ambientCG](https://ambientcg.com)，CC0 协议，商用无忧。

### 3D 模型 (Models/) — 44 个低多边形 GLB 模型
全部来自 [Poly Pizza](https://poly.pizza)（CC0/CC-BY），搜索关键词覆盖：
- **cube** — 方块/立方体
- **cylinder** — 圆柱体/管道
- **cone** — 圆锥体
- **sphere** — 球体
- **pole** — 杆/柱子（关键！适合做影子投掷物）
- **stick** — 细棍
- **arrow-pointer** — 箭头/指针（适合指示太阳方向）
- **sundial** — 日晷相关
- **ground-plane** — 地面/平台
- **marker-pin** — 标记点

---

## Unity 快速上手

### 1. 导入素材
```
Unity 菜单: Assets → Import New Asset →
  选择 Textures/Ground103_2K-JPG_Color.jpg  （地面颜色贴图）
  选择 Models/*.glb                            （批量导入 3D 模型）
```

GLB 文件拖入 Unity 后会自动转为 Prefab，材质会一并导入。

### 2. 搭建光影实验场景（推荐配置）

#### 地面
```
GameObject → 3D Object → Plane
  Scale: (5, 1, 5)              // 放大地面
  Material: 新建 Material，Albedo 贴 Ground103_Color
            Normal Map 贴 Ground103_NormalGL
            调低 Smoothness 到 0.1（粗糙表面阴影更清晰）
```

#### 影子投掷物（选 1-3 个不同高度）
```
从 Models/ 拖入一个 pole 或 cylinder 模型的 Prefab
  Position: (0, 0.5, 0)         // 半埋在地面上
  Scale: 调整高度（不同高度 = 不同影子长度）
```

#### 太阳（方向光）
```
GameObject → Light → Directional Light
  Rotation: (30, 45, 0)         // 初始角度，模拟上午
  Shadow Type: Soft Shadows     // 软阴影更真实
  Strength: 1.2                 // 稍微亮一点
```

#### 角度参考（可选但推荐）
```
GameObject → 3D Object → Quad
  Position: 放在地面旁边
  Rotation: 平放在地上
  用做角度刻度参考面
```

### 3. 快速验证脚本

创建 C# 脚本 `SunController.cs`，挂载到 Directional Light 上：

```csharp
using UnityEngine;

/// <summary>
/// 用键盘控制太阳角度，实时观察影子变化
/// 适合课堂演示太阳高度角与影子长度的关系
/// </summary>
public class SunController : MonoBehaviour
{
    [Header("旋转速度")]
    public float speed = 30f;

    void Update()
    {
        // 左右箭头 = 改变方位角（东→西）
        float horizontal = Input.GetAxis("Horizontal");
        // 上下箭头 = 改变高度角（日出→正午→日落）
        float vertical = Input.GetAxis("Vertical");

        transform.Rotate(Vector3.up, horizontal * speed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right, vertical * speed * Time.deltaTime, Space.Self);
    }
}
```

操作方式：
- **← →** 太阳绕水平方向转（模拟一天从早到晚）
- **↑ ↓** 太阳高度角变化（模拟季节变化）
- 直接观察地面上**影子长度和方向的变化**

### 4. 进阶玩法建议

- **放置多根不同高度的 pole**：对比同一时刻不同高度物体的影子长度
- **用 arrow-pointer 模型标记太阳方向**
- **用 marker-pin 标记地面关键点**（如正午影子最短点）
- **开启 Unity Recorder**，录制影子变化的 time-lapse 视频

---

## 素材来源与协议

| 来源 | 协议 | 用途 |
|------|------|------|
| [ambientCG](https://ambientcg.com) | CC0 | 纹理贴图，完全自由使用 |
| [Poly Pizza](https://poly.pizza) | CC0 / CC-BY | 3D 模型，CC-BY 仅需署名 |

> CC0 = 公共领域，无需署名，可商用
> CC-BY = 需署名原作者，可商用

---

下载日期: 2026-05-31
