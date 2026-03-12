using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

/// <summary>
/// 通用Debug数据捕获器
/// 捕获：截图 + Console日志 + 系统诊断 + Feature配置 + 中间步骤可视化
/// </summary>
public class DebugDataCapture : MonoBehaviour
{
    private string effectName;
    private System.Action onComplete;
    private string featureSettings;
    private Material debugMaterial;
    private int[] debugSteps;

    public static void Capture(string effectName, string featureSettings = null, Material debugMaterial = null, int[] debugSteps = null, System.Action onComplete = null)
    {
        GameObject obj = new GameObject("[DebugCapture]");
        obj.hideFlags = HideFlags.HideAndDontSave;
        DebugDataCapture capture = obj.AddComponent<DebugDataCapture>();
        capture.effectName = effectName;
        capture.featureSettings = featureSettings;
        capture.debugMaterial = debugMaterial;
        capture.debugSteps = debugSteps ?? new int[] { 1, 2, 3, 4 }; // 默认Pass 1-4
        capture.onComplete = onComplete;
        capture.StartCoroutine(capture.CaptureCoroutine());
    }

    private IEnumerator CaptureCoroutine()
    {
        Debug.Log($"[Debug] 开始捕获 {effectName}...");

        // 等待渲染完成（关键：必须等到EndOfFrame才能ReadPixels）
        yield return new WaitForEndOfFrame();

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string dir = GetOutputDirectory();

        // 捕获截图
        Texture2D screenshot = CaptureScreen();
        string imagePath = Path.Combine(dir, $"{effectName}_{timestamp}.png");
        SaveTexture(screenshot, imagePath);

        // 捕获中间步骤可视化（如果提供了debugMaterial）
        if (debugMaterial != null)
        {
            Texture2D stepDebug = CaptureStepDebug();
            if (stepDebug != null)
            {
                string stepImagePath = Path.Combine(dir, $"{effectName}_{timestamp}_Steps.png");
                SaveTexture(stepDebug, stepImagePath);
                Destroy(stepDebug);
            }
        }

        // 导出日志
        string logPath = Path.Combine(dir, $"{effectName}_{timestamp}_Console.txt");
        ConsoleLogger.ExportLogs(logPath, 100);

        // 生成报告
        string reportPath = Path.Combine(dir, $"{effectName}_{timestamp}_Report.txt");
        GenerateReport(reportPath, timestamp);

        Debug.Log($"[Debug] ✅ 捕获完成！\n" +
                  $"截图: {Path.GetFileName(imagePath)}\n" +
                  $"日志: {Path.GetFileName(logPath)}\n" +
                  $"报告: {Path.GetFileName(reportPath)}\n" +
                  $"位置: {dir}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        onComplete?.Invoke();
        Destroy(screenshot);
        Destroy(gameObject);
    }

    private string GetOutputDirectory()
    {
        string dir = Path.Combine(Application.dataPath, "DebugCaptures");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return dir;
    }

    private Texture2D CaptureScreen()
    {
        // 使用Camera的实际渲染尺寸，而不是Screen（避免Game View尺寸不匹配）
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[DebugCapture] 未找到Main Camera！");
            return new Texture2D(1, 1);
        }

        int width = cam.pixelWidth;
        int height = cam.pixelHeight;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        return tex;
    }

    private Texture2D CaptureStepDebug()
    {
        Camera cam = Camera.main;
        if (cam == null || debugMaterial == null)
            return null;

        try
        {
            // 运行时验证材质参数（debug）
            Vector4 reflectionPlane = debugMaterial.GetVector("_ReflectionPlane");
            Debug.Log($"[DebugCapture] 运行时材质参数 _ReflectionPlane = {reflectionPlane}");

            // ⚠️ 关键：手动绑定深度纹理（Graphics.Blit不会自动传递）
            Shader.SetGlobalTexture("_CameraDepthTexture", Shader.GetGlobalTexture("_CameraDepthTexture"));

            int stepSize = 1024;
            int gridSize = 2;
            int totalSize = stepSize * gridSize;

            // 创建屏幕副本作为_BaseMap
            int width = cam.pixelWidth;
            int height = cam.pixelHeight;
            Texture2D screenCopy = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenCopy.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenCopy.Apply();

            // 设置到材质
            debugMaterial.SetTexture("_BaseMap", screenCopy);

            // 创建临时RT用于各个步骤
            RenderTexture[] stepRTs = new RenderTexture[4];
            for (int i = 0; i < 4; i++)
            {
                stepRTs[i] = RenderTexture.GetTemporary(stepSize, stepSize, 0, RenderTextureFormat.ARGB32);
            }

            // 创建全屏源RT
            RenderTexture fullscreenRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(screenCopy, fullscreenRT);

            // 渲染各个步骤（根据debugSteps指定的Pass索引）
            for (int i = 0; i < 4; i++)
            {
                int passIndex = debugSteps[i];
                Graphics.Blit(fullscreenRT, stepRTs[i], debugMaterial, passIndex);
            }

            // 创建最终合并RT
            RenderTexture finalRT = RenderTexture.GetTemporary(totalSize, totalSize, 0, RenderTextureFormat.ARGB32);

            // 设置步骤纹理到材质
            debugMaterial.SetTexture("_Step1", stepRTs[0]);
            debugMaterial.SetTexture("_Step2", stepRTs[1]);
            debugMaterial.SetTexture("_Step3", stepRTs[2]);
            debugMaterial.SetTexture("_Step4", stepRTs[3]);

            // 使用Pass 8合并（SSPR_StepDebug.shader的最后一个Pass）
            Graphics.Blit(fullscreenRT, finalRT, debugMaterial, 8);

            // 读取到Texture2D
            RenderTexture.active = finalRT;
            Texture2D result = new Texture2D(totalSize, totalSize, TextureFormat.RGB24, false);
            result.ReadPixels(new Rect(0, 0, totalSize, totalSize), 0, 0);
            result.Apply();
            RenderTexture.active = null;

            // 清理
            Destroy(screenCopy);
            RenderTexture.ReleaseTemporary(fullscreenRT);
            RenderTexture.ReleaseTemporary(finalRT);
            for (int i = 0; i < 4; i++)
            {
                RenderTexture.ReleaseTemporary(stepRTs[i]);
            }

            Debug.Log($"[DebugCapture] 成功生成Step Debug可视化 ({totalSize}x{totalSize})");
            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DebugCapture] Step Debug生成失败: {e.Message}");
            return null;
        }
    }

    private void SaveTexture(Texture2D tex, string path)
    {
        File.WriteAllBytes(path, tex.EncodeToPNG());
    }

    private void GenerateReport(string path, string timestamp)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{effectName} - Debug Report");
        sb.AppendLine("=" + new string('=', 78));
        sb.AppendLine($"Timestamp: {timestamp}");
        sb.AppendLine($"Time: {System.DateTime.Now}");
        sb.AppendLine();

        sb.AppendLine("=== System ===");
        sb.AppendLine($"Unity: {Application.unityVersion}");
        sb.AppendLine($"Platform: {Application.platform}");
        sb.AppendLine($"GPU: {SystemInfo.graphicsDeviceName}");
        sb.AppendLine($"API: {SystemInfo.graphicsDeviceType}");
        sb.AppendLine($"VRAM: {SystemInfo.graphicsMemorySize} MB");
        sb.AppendLine();

        sb.AppendLine("=== Screen ===");
        sb.AppendLine($"Resolution: {Screen.width}x{Screen.height}");
        sb.AppendLine($"DPI: {Screen.dpi}");
        sb.AppendLine();

        if (Camera.main != null)
        {
            Camera cam = Camera.main;
            sb.AppendLine("=== Camera ===");
            sb.AppendLine($"Position: {cam.transform.position}");
            sb.AppendLine($"Rotation: {cam.transform.eulerAngles}");
            sb.AppendLine($"FOV: {cam.fieldOfView}");
            sb.AppendLine($"Near: {cam.nearClipPlane} | Far: {cam.farClipPlane}");
            sb.AppendLine();
        }

        var stats = ConsoleLogger.GetStats();
        sb.AppendLine("=== Console Stats ===");
        sb.AppendLine($"Errors: {stats.errors}");
        sb.AppendLine($"Warnings: {stats.warnings}");
        sb.AppendLine($"Logs: {stats.logs}");
        sb.AppendLine();

        sb.AppendLine("=== Performance ===");
        sb.AppendLine($"FPS: {(int)(1.0f / Time.deltaTime)}");
        sb.AppendLine($"Frame Time: {Time.deltaTime * 1000:F2} ms");
        sb.AppendLine();

        // 自动收集场景对象信息
        GameObject testObject = GameObject.Find("测试物体");
        if (testObject != null)
        {
            sb.AppendLine("=== Scene Objects ===");
            sb.AppendLine($"Parent: {testObject.name}");
            sb.AppendLine($"  Position: {testObject.transform.position}");
            sb.AppendLine($"  Rotation: {testObject.transform.eulerAngles}");
            sb.AppendLine($"  Scale: {testObject.transform.localScale}");
            sb.AppendLine();

            // 遍历所有子对象
            foreach (Transform child in testObject.transform)
            {
                sb.AppendLine($"Child: {child.name}");
                sb.AppendLine($"  Position: {child.position}");
                sb.AppendLine($"  Local Position: {child.localPosition}");
                sb.AppendLine($"  Rotation: {child.eulerAngles}");
                sb.AppendLine($"  Scale: {child.localScale}");

                // 获取Renderer信息（如果有）
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    sb.AppendLine($"  Bounds Center: {renderer.bounds.center}");
                    sb.AppendLine($"  Bounds Size: {renderer.bounds.size}");
                }
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("=== Scene Objects ===");
            sb.AppendLine("⚠️ 未找到名为'测试物体'的GameObject");
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(featureSettings))
        {
            sb.AppendLine("=== Feature Settings ===");
            sb.AppendLine(featureSettings);
            sb.AppendLine();
        }

        File.WriteAllText(path, sb.ToString());
    }
}
