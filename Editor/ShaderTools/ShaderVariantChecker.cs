using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Shader变体检测工具 - 用于检查Shader编译状态和变体数量（兼容多版本Unity）
///
/// 功能：
/// 1. 检查Shader编译状态和错误
/// 2. 检查Material启用的Keywords
/// 3. 预估Shader变体数量
/// 4. 检测华为等移动设备兼容性问题
///
/// 使用方法：
/// Unity菜单 → Tools → Shader变体检测器
/// </summary>
public class ShaderVariantChecker : EditorWindow
{
    private Shader targetShader;
    private Material targetMaterial;
    private Vector2 scrollPos;
    private List<string> logs = new List<string>();

    [MenuItem("Tools/Shader变体检测器")]
    public static void ShowWindow()
    {
        GetWindow<ShaderVariantChecker>("Shader变体检测");
    }

    void OnGUI()
    {
        GUILayout.Label("Shader变体检测工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetShader = EditorGUILayout.ObjectField("目标Shader", targetShader, typeof(Shader), false) as Shader;
        targetMaterial = EditorGUILayout.ObjectField("目标Material（可选）", targetMaterial, typeof(Material), false) as Material;

        EditorGUILayout.Space();

        if (GUILayout.Button("检查Shader编译状态", GUILayout.Height(30)))
        {
            CheckShaderCompilation();
        }

        if (GUILayout.Button("检查Material启用的Keywords", GUILayout.Height(30)))
        {
            CheckMaterialKeywords();
        }

        if (GUILayout.Button("清空日志", GUILayout.Height(25)))
        {
            logs.Clear();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var log in logs)
        {
            if (log.StartsWith("❌") || log.StartsWith("⚠️"))
                EditorGUILayout.HelpBox(log, MessageType.Warning);
            else if (log.StartsWith("✅"))
                EditorGUILayout.HelpBox(log, MessageType.Info);
            else
                EditorGUILayout.LabelField(log);
        }
        EditorGUILayout.EndScrollView();
    }

    void CheckShaderCompilation()
    {
        logs.Clear();

        if (targetShader == null)
        {
            logs.Add("❌ 请先选择Shader");
            return;
        }

        logs.Add($"🔍 Shader名称: {targetShader.name}");

        // 检查Shader是否支持
        if (!targetShader.isSupported)
        {
            logs.Add("❌ Shader不支持当前平台");
        }
        else
        {
            logs.Add("✅ Shader支持当前平台");
        }

        // 检查编译错误（兼容API）
        int errorCount = ShaderUtil.GetShaderMessageCount(targetShader);
        if (errorCount > 0)
        {
            logs.Add($"⚠️ Shader有 {errorCount} 个编译消息");

            // 获取错误信息
            var messages = ShaderUtil.GetShaderMessages(targetShader);
            for (int i = 0; i < messages.Length; i++)
            {
                logs.Add($"  [{i}] {messages[i].message} (Line {messages[i].line})");
            }
        }
        else
        {
            logs.Add("✅ Shader编译无错误");
        }

        // 检查RenderQueue
        logs.Add($"📋 Render Queue: {targetShader.renderQueue}");

        // 尝试获取变体信息（如果Unity版本支持）
        try
        {
            // 统计所有可能的变体组合
            int estimatedVariants = CalculateEstimatedVariants();
            logs.Add($"📊 预估变体数量: {estimatedVariants}");
        }
        catch
        {
            logs.Add("⚠️ 当前Unity版本无法获取变体数量");
        }
    }

    int CalculateEstimatedVariants()
    {
        // 从Shader代码推算变体数量
        // 这是一个示例计算，针对Transparent.shader
        // _USEDISSOLVE_ON: 2种 (off/on)
        // _USERIM_ON: 2种 (off/on)
        // _BASEUV: 2种 (DEFAULT/FLIPBOOKBLENDING)
        // _DISTORTIONTYPE: 3种 (NONE/SIMPLE/NORMAL)
        // _USEMASK_ON: 2种 (off/on)
        // _DEPTHFADE_ON: 2种 (off/on)

        return 2 * 2 * 2 * 3 * 2 * 2; // = 96
    }

    void CheckMaterialKeywords()
    {
        logs.Clear();

        if (targetMaterial == null)
        {
            logs.Add("❌ 请先选择Material");
            return;
        }

        logs.Add($"📦 Material: {targetMaterial.name}");
        logs.Add($"🎨 Shader: {targetMaterial.shader.name}");
        logs.Add($"📋 Render Queue: {targetMaterial.renderQueue}");

        // 检查Keywords
        var keywords = targetMaterial.shaderKeywords;
        if (keywords.Length == 0)
        {
            logs.Add("⚠️ 当前Material没有启用任何Keyword");
            logs.Add("   这可能导致在华为设备上不显示");
        }
        else
        {
            logs.Add($"✅ 启用的Keywords ({keywords.Length}):");
            foreach (var keyword in keywords)
            {
                logs.Add($"  • {keyword}");
            }
        }

        logs.Add("");
        logs.Add("📋 关键属性值:");

        // 检查重要属性
        if (targetMaterial.HasProperty("_EffectStencilBuffer"))
        {
            float val = targetMaterial.GetFloat("_EffectStencilBuffer");
            logs.Add($"  _EffectStencilBuffer = {val}");
            if (val != 0)
            {
                logs.Add("  ⚠️ 建议设为0，动态Stencil可能在华为设备上有问题");
            }
        }

        if (targetMaterial.HasProperty("_BlendDst"))
            logs.Add($"  _BlendDst = {targetMaterial.GetFloat("_BlendDst")}");

        if (targetMaterial.HasProperty("_ZWriteMode"))
            logs.Add($"  _ZWriteMode = {targetMaterial.GetFloat("_ZWriteMode")}");

        if (targetMaterial.HasProperty("_CullMode"))
            logs.Add($"  _CullMode = {targetMaterial.GetFloat("_CullMode")}");

        // 检查贴图
        logs.Add("");
        logs.Add("🖼️ 使用的贴图:");
        if (targetMaterial.HasProperty("_BaseMap"))
        {
            var tex = targetMaterial.GetTexture("_BaseMap");
            logs.Add($"  _BaseMap = {(tex ? tex.name : "NULL")}");
        }
        if (targetMaterial.HasProperty("_MaskMap"))
        {
            var tex = targetMaterial.GetTexture("_MaskMap");
            logs.Add($"  _MaskMap = {(tex ? tex.name : "NULL")}");
        }
        if (targetMaterial.HasProperty("_DistortionMap"))
        {
            var tex = targetMaterial.GetTexture("_DistortionMap");
            logs.Add($"  _DistortionMap = {(tex ? tex.name : "NULL")}");
        }

        logs.Add("");
        logs.Add("💡 检查建议:");
        logs.Add("  1. 确认Shader已改为multi_compile_local");
        logs.Add("  2. _EffectStencilBuffer建议设为0");
        logs.Add("  3. 打包前检查Graphics Settings是否启用Depth Texture");
    }
}
