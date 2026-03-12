# PostProcessDebugSystem - 后处理调试系统

## 概述

通用后处理效果调试工具，用于快速诊断URP ScriptableRenderPass问题。

## 功能特性

### 核心功能

1. **一键截图调试**
   - 自动捕获GameView当前画面
   - 记录Console完整日志（Info/Warning/Error）
   - 生成结构化Debug Report

2. **自动配置捕获**
   - 使用反射自动读取`ScriptableRendererFeature.Settings`字段
   - 支持基础类型、Vector3/4、Enum、RenderPassEvent
   - 无需手动编写配置序列化代码

3. **中间步骤可视化**
   - 4个调试Pass的2x2网格显示（2048x2048）
   - 在Feature.Settings中配置显示内容（如Depth、Normal、WorldPos等）
   - 自动创建Debug Material并渲染指定Pass

4. **场景对象追踪**
   - 自动捕获指定根物体的所有子对象Transform
   - 记录Position、Rotation、Scale、Bounds信息
   - 便于定位场景配置问题

## 文件结构

```
PostProcessDebugSystem/
├── DebugDataCapture.cs           # 运行时截图和数据捕获
├── ConsoleLogger.cs              # Console日志记录
├── Editor/
│   ├── EditorDebugCapture.cs     # Editor模式数据捕获
│   ├── EditorConsoleLogger.cs    # Editor日志记录
│   ├── DebugDataAnalyzer.cs      # 数据分析器
│   └── PostProcessDebugWindow.cs # Debug窗口UI
└── README.txt                    # 使用说明
```

## 使用方法

### 1. 安装

将整个 `PostProcessDebugSystem/` 文件夹复制到Unity项目的 `Assets/` 目录下。

### 2. 打开调试窗口

Unity菜单 → `Window → 后处理调试工具`

### 3. 配置调试步骤（可选）

在你的`ScriptableRendererFeature.Settings`中添加调试步骤枚举：

```csharp
[System.Serializable]
public class Settings
{
    [Header("Debug Visualization")]
    public MyFeatureDebugStep debugStep1 = MyFeatureDebugStep.Depth;
    public MyFeatureDebugStep debugStep2 = MyFeatureDebugStep.Normal;
    public MyFeatureDebugStep debugStep3 = MyFeatureDebugStep.WorldPos;
    public MyFeatureDebugStep debugStep4 = MyFeatureDebugStep.FinalResult;

    // ... 其他设置
}

public enum MyFeatureDebugStep
{
    None = 0,
    Depth = 1,
    Normal = 2,
    WorldPos = 3,
    FinalResult = 4
}
```

### 4. 创建StepDebug Shader（可选）

如需中间步骤可视化，需创建对应的Debug Shader，每个Pass对应一个调试步骤：

```hlsl
Shader "Hidden/MyFeature_StepDebug"
{
    SubShader
    {
        // Pass 0: DebugParams（参数可视化）
        Pass
        {
            Name "DebugParams"
            // ...
        }

        // Pass 1-4: 对应debugStep1-4
        Pass
        {
            Name "Depth"
            // 深度可视化
        }

        Pass
        {
            Name "Normal"
            // 法线可视化
        }

        // Pass 8: Combine（系统会自动将4个Pass合并到2x2网格）
        Pass
        {
            Name "Combine"
            // 留空，系统自动处理
        }
    }
}
```

### 5. 捕获调试信息

1. 在Debug窗口中找到你的Feature
2. 点击"捕获调试信息"按钮
3. 等待捕获完成

### 6. 查看结果

捕获结果保存在 `Assets/DebugCaptures/` 目录：

```
DebugCaptures/
├── MyFeature_20260312_210152.png          # 主画面截图
├── MyFeature_20260312_210152_Steps.png    # 4步骤2x2网格（如已配置）
├── MyFeature_20260312_210152_Report.txt   # 系统信息和配置
└── MyFeature_20260312_210152_Console.txt  # Console完整日志
```

## 技术要点

### 1. ReadPixels时机

```csharp
// ❌ 错误：可能在渲染未完成时读取
yield return null;
yield return null;

// ✅ 正确：等待帧渲染完成
yield return new WaitForEndOfFrame();
```

### 2. 分辨率匹配

```csharp
// ❌ 错误：Screen尺寸可能不匹配GameView
int width = Screen.width;
int height = Screen.height;

// ✅ 正确：使用Camera实际渲染尺寸
int width = camera.pixelWidth;
int height = camera.pixelHeight;
```

### 3. 自动配置读取

系统自动读取以下类型的Settings字段：
- 基础类型：`int`, `float`, `bool`, `string`
- Unity类型：`Vector3`, `Vector4`
- 枚举类型：`enum`
- 渲染事件：`RenderPassEvent`

复杂类型（如`Texture2D`、`Material`）会被跳过。

### 4. 场景对象追踪

默认搜索名为"测试物体"的GameObject，可修改`DebugDataCapture.cs`中的硬编码名称：

```csharp
GameObject testObject = GameObject.Find("测试物体");
```

## 常见问题

### Q: 截图为纯黑/纯白？

**A**: 检查以下几点：
1. 确认是在Play Mode下截图（Editor Mode使用SceneView截图）
2. 检查Camera是否正确渲染
3. 查看Console是否有错误

### Q: Steps图像显示不正确？

**A**:
1. 确认Feature.Settings中定义了`debugStep1-4`字段
2. 确认创建了对应的`_StepDebug.shader`
3. 检查Shader Pass索引是否正确（0=DebugParams, 1-4=步骤, 8=Combine）

### Q: 配置信息未捕获？

**A**:
1. 确认Settings类是`[System.Serializable]`
2. 检查字段是public的
3. 复杂类型会被自动跳过

### Q: ReadPixels报错"outside of RenderTexture bounds"？

**A**: 分辨率不匹配，使用`Camera.pixelWidth/pixelHeight`而非`Screen.width/height`

## 适用场景

- ✅ URP后处理效果不显示
- ✅ Shader采样纹理返回异常值
- ✅ 不清楚中间计算步骤是否正确
- ✅ 需要对比多个调试阶段
- ✅ 记录Bug复现条件和环境信息

## 实战案例

### SSPR反射效果调试

**问题**：SSPR完全不显示反射效果

**调试过程**：
1. 使用Debug系统可视化法线纹理 → 发现返回(0,0,0)
2. 检查Shader代码 → `SampleSceneNormals()`调用正确
3. 检查RenderPass → 发现`ConfigureInput()`未请求Normal输入
4. 修复：添加`ScriptableRenderPassInput.Normal`

**关键发现**：通过法线可视化Pass（Normal * 0.5 + 0.5转RGB），直接看到纹理数据异常

### SSAO接触暗化验证

**目标**：确认SSAO对地面-立方体接触处有效果

**步骤**：
1. 捕获4个调试Pass：Depth、AO强度、EdgeMask、FinalResult
2. 对比AO强度图和最终结果
3. 确认接触处确实有暗化

## 扩展开发

### 添加自定义分析器

在`DebugDataAnalyzer.cs`中添加自定义分析逻辑：

```csharp
public static string AnalyzeDebugData(string featureName, Texture2D screenshot)
{
    StringBuilder sb = new StringBuilder();

    // 自定义分析逻辑
    // 例如：采样特定像素、计算平均亮度等

    return sb.ToString();
}
```

### 支持更多配置类型

修改`PostProcessDebugWindow.cs`中的`SerializeFeatureSettings()`：

```csharp
// 添加对新类型的支持
if (field.FieldType == typeof(YourCustomType))
{
    var value = (YourCustomType)field.GetValue(settings);
    sb.AppendLine($"{field.Name}: {value.ToString()}");
}
```

## 技术限制

- 仅支持URP（Built-in RP和HDRP不支持）
- 中间步骤可视化最多4个Pass
- 场景对象追踪需硬编码根物体名称
- 不支持运行时修改调试配置（需在Settings中预先定义）

## 版本历史

### v1.0 (2026-03-12)
- ✅ 初始版本
- ✅ 一键截图 + Console日志 + 系统信息
- ✅ 自动配置捕获（反射读取Settings）
- ✅ 4步骤2x2网格可视化
- ✅ 场景对象Transform追踪
- ✅ 完整的SSAO和SSPR调试验证

---

*Created for Unity 2021.3+ / URP 12+*
