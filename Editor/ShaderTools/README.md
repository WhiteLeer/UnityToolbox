# ShaderVariantChecker 使用说明

## 🎯 功能

检测Shader编译状态、变体数量和移动设备兼容性问题

## 📦 安装

将 `ShaderVariantChecker.cs` 复制到Unity项目的 `Assets/Editor/` 目录

## 🚀 快速开始

### 步骤1：打开工具
Unity菜单 → `Tools` → `Shader变体检测器`

### 步骤2：检查Shader
1. 拖入目标Shader到「目标Shader」栏
2. 点击「检查Shader编译状态」

**输出示例**：
```
✅ Shader支持当前平台
✅ Shader编译无错误
📊 预估变体数量: 96
📋 Render Queue: 3000
```

### 步骤3：检查Material
1. 拖入目标Material到「目标Material」栏
2. 点击「检查Material启用的Keywords」

**输出示例**：
```
✅ 启用的Keywords (2):
  • _USERIM_ON
  • _BASEUV_DEFAULT

📋 关键属性值:
  _EffectStencilBuffer = 0  ✅
  _BlendDst = 1
```

## 🔍 适用场景

### 场景1：华为设备Shader不显示
**症状**：Shader在华为手机不显示，其他设备正常

**排查步骤**：
1. 检查Material Keywords数量
   - 如果为0 → 变体被剥离
   - 解决：将 `shader_feature_local` 改为 `multi_compile_local`

2. 检查 `_EffectStencilBuffer`
   - 如果非0 → 动态Stencil问题
   - 解决：设为0

### 场景2：变体数量异常
**症状**：怀疑Shader变体被剥离

**检查方法**：
- 使用工具预估变体数量
- 对比 `shader_feature` vs `multi_compile` 差异

### 场景3：打包前检查
**用途**：确保Shader在移动平台正常

**检查项**：
- ✅ 无编译错误
- ✅ Keywords正常启用
- ✅ 关键属性值合理
- ✅ 贴图正确绑定

## ⚙️ 常见问题

### Q1: 工具显示"预估变体数量96"是什么意思？

A: 基于Shader代码计算的变体组合数。`multi_compile_local` 会编译所有96个组合，`shader_feature_local` 只编译Material使用的1-5个。

### Q2: Keywords数量为0怎么办？

A: 说明变体被剥离。解决方案：
```hlsl
// ❌ 会被剥离
#pragma shader_feature_local _USERIM_ON

// ✅ 强制编译
#pragma multi_compile_local __ _USERIM_ON
```

### Q3: _EffectStencilBuffer应该设为多少？

A: 建议设为 **0**，避免在华为等设备上出现动态Stencil兼容性问题。

## 📚 相关文档

详细技术指南：[ShaderVariantChecker-Guide.md](../../Documentation/ShaderVariantChecker-Guide.md)

## 📝 版本

- **v1.0** (2026-03-04): 初始版本，解决华为设备Shader不显示问题
