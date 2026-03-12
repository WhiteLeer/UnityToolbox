# Unity Toolbox

Master的Unity开发工具集，收录有价值的编辑器工具和实用脚本。

## 📁 项目结构

```
UnityToolbox/
├── Editor/
│   ├── ShaderTools/          # Shader相关工具
│   │   └── ShaderVariantChecker.cs
│   └── PostProcessDebugSystem/  # 后处理调试系统
│       ├── DebugDataCapture.cs
│       ├── ConsoleLogger.cs
│       └── Editor/
│           ├── PostProcessDebugWindow.cs
│           ├── EditorDebugCapture.cs
│           ├── EditorConsoleLogger.cs
│           └── DebugDataAnalyzer.cs
├── Documentation/            # 文档和使用说明
└── README.md
```

## 🛠️ 工具列表

### 1. Shader变体检测器 (ShaderVariantChecker)

**用途**：检测Shader编译状态、变体数量和移动设备兼容性问题

**功能**：
- ✅ 检查Shader编译错误和警告
- ✅ 预估Shader变体数量
- ✅ 检查Material启用的Keywords
- ✅ 检测华为等移动设备兼容性问题
- ✅ 检查关键属性值（Stencil、BlendMode等）
- ✅ 显示贴图绑定状态

**使用方法**：
1. 将 `Editor/ShaderTools/ShaderVariantChecker.cs` 复制到Unity项目的 `Assets/Editor/` 目录
2. Unity菜单 → `Tools → Shader变体检测器`
3. 拖入Shader或Material进行检测

**适用场景**：
- Shader在特定设备（如华为）不显示
- 怀疑Shader变体被剥离
- 检查multi_compile vs shader_feature差异
- 移动平台兼容性排查

**技术要点**：
- 兼容多版本Unity API
- 避免使用不同版本差异较大的ShaderUtil方法
- 针对华为Mali GPU常见问题提供检测建议

---

### 2. 后处理调试系统 (PostProcessDebugSystem)

**用途**：通用URP后处理效果调试工具，快速诊断ScriptableRenderPass问题

**功能**：
- ✅ 一键截图调试（GameView + Console日志 + 系统信息）
- ✅ 自动捕获Feature.Settings配置（反射读取）
- ✅ 中间步骤可视化（4个Pass的2x2网格显示）
- ✅ 场景对象Transform追踪
- ✅ 结构化Debug Report生成

**使用方法**：
1. 将 `Editor/PostProcessDebugSystem/` 文件夹复制到Unity项目的 `Assets/` 目录
2. Unity菜单 → `Window → 后处理调试工具`
3. 选择Feature → 点击"捕获调试信息"

**适用场景**：
- 后处理效果不显示或异常
- Shader采样纹理返回错误值
- 需要对比中间计算步骤
- 记录Bug复现条件和环境

**技术要点**：
- ReadPixels时机控制（WaitForEndOfFrame）
- Camera.pixelWidth匹配GameView分辨率
- SerializedObject反射读取配置
- Graphics.CopyTexture合并多Pass到2x2网格

**实战案例**：
- SSPR法线纹理返回(0,0,0) → 发现ConfigureInput()缺少Normal请求
- SSAO接触暗化验证 → 4步骤可视化确认算法正确性

---

## 📖 使用指南

### 安装方式

#### 方法1：直接复制
将对应工具的 `.cs` 文件复制到Unity项目的 `Assets/Editor/` 目录

#### 方法2：Git Submodule
```bash
cd YourUnityProject/Assets/Editor/
git submodule add https://github.com/wepie/UnityToolbox.git
```

---

## 🚀 更新日志

### 2026-03-12
- ✅ 添加 PostProcessDebugSystem 后处理调试系统
- ✅ 支持自动配置捕获和中间步骤可视化
- ✅ 完整验证SSAO和SSPR效果实现

### 2026-03-04
- ✅ 初始化项目
- ✅ 添加 ShaderVariantChecker 工具
- ✅ 解决华为设备Shader不显示问题的检测方案

---

## 📝 贡献指南

本项目用于收录Master开发过程中产生的有价值工具。

**收录标准**：
- ✅ 实际项目中验证有效
- ✅ 解决特定痛点问题
- ✅ 代码简洁可维护
- ✅ 有明确使用文档

---

## 📄 许可证

MIT License - 自由使用，保留署名

---

## 📮 联系方式

- **GitHub**: https://github.com/wepie/UnityToolbox
- **问题反馈**: [Issues](https://github.com/wepie/UnityToolbox/issues)

---

*Created by Claude Code Assistant*
