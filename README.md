# Unity Toolbox

Master的Unity开发工具集，收录有价值的编辑器工具和实用脚本。

## 📁 项目结构

```
UnityToolbox/
├── Editor/
│   └── ShaderTools/          # Shader相关工具
│       └── ShaderVariantChecker.cs
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
