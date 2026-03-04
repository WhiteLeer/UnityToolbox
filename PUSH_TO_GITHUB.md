# 推送到GitHub指南

## 步骤1：创建GitHub仓库

1. 打开 https://github.com/new
2. 仓库名称：`UnityToolbox`
3. 描述：`Unity开发工具集 - Shader检测、编辑器扩展等实用工具`
4. 设为 **Public**（或Private根据需要）
5. **不要** 勾选 "Add a README file"（我们已经有了）
6. 点击 **Create repository**

## 步骤2：推送到GitHub

在终端执行（已在项目目录）：

```bash
# 添加远程仓库（替换YOUR_USERNAME为你的GitHub用户名）
git remote add origin https://github.com/YOUR_USERNAME/UnityToolbox.git

# 推送到main分支
git branch -M main
git push -u origin main
```

## 快捷命令（复制执行）

```bash
cd ~/Desktop/UnityToolbox
git remote add origin https://github.com/YOUR_USERNAME/UnityToolbox.git
git branch -M main
git push -u origin main
```

## 验证

访问：`https://github.com/YOUR_USERNAME/UnityToolbox`

应该看到：
- ✅ README.md 显示项目介绍
- ✅ Editor/ShaderTools/ShaderVariantChecker.cs
- ✅ Documentation/ShaderVariantChecker-Guide.md

---

## 后续更新流程

每次添加新工具后：

```bash
cd ~/Desktop/UnityToolbox
git add .
git commit -m "添加新工具：XXX"
git push
```

---

Master，项目已准备就绪！
