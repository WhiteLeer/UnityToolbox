using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Debug数据分析器 - 为AI提供快速访问
/// </summary>
public class DebugDataAnalyzer
{
    [MenuItem("Tools/Post Process Debug/📊 Analyze Latest Data")]
    public static void AnalyzeLatestData()
    {
        var package = GetLatestDebugPackage();

        if (package == null)
        {
            EditorUtility.DisplayDialog("没有Debug数据",
                "请先运行游戏并捕获Debug数据。\n\n" +
                "打开：Window → Post Process Debug Center",
                "确定");
            return;
        }

        string summary = GenerateSummary(package);
        Debug.Log(summary);

        EditorUtility.DisplayDialog("Debug数据摘要",
            summary + "\n\n━━━━━━━━━━━━━━━━━━━━\n\n" +
            "💬 现在对Claude说：\n\"请分析Debug数据\"\n\n" +
            "Claude会自动读取文件并分析。",
            "好的");
    }

    [MenuItem("Tools/Post Process Debug/📂 Open Debug Folder")]
    public static void OpenDebugFolder()
    {
        string dir = Path.Combine(Application.dataPath, "DebugCaptures");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        EditorUtility.RevealInFinder(dir);
    }

    [MenuItem("Tools/Post Process Debug/🗑️ Clear All Debug Files")]
    public static void ClearAllFiles()
    {
        string dir = Path.Combine(Application.dataPath, "DebugCaptures");
        if (!Directory.Exists(dir))
            return;

        var files = Directory.GetFiles(dir).Where(f => !f.EndsWith(".meta")).ToArray();

        if (files.Length == 0)
        {
            EditorUtility.DisplayDialog("没有文件", "Debug文件夹是空的。", "确定");
            return;
        }

        if (EditorUtility.DisplayDialog("清理确认",
            $"确定删除所有 {files.Length} 个Debug文件？",
            "删除", "取消"))
        {
            foreach (var file in files)
                File.Delete(file);

            AssetDatabase.Refresh();
            Debug.Log($"[Debug] 已清理 {files.Length} 个文件");
        }
    }

    public static DebugPackage GetLatestDebugPackage()
    {
        string dir = Path.Combine(Application.dataPath, "DebugCaptures");
        if (!Directory.Exists(dir))
            return null;

        var images = Directory.GetFiles(dir, "*.png")
            .OrderByDescending(f => File.GetLastWriteTime(f))
            .ToArray();

        if (images.Length == 0)
            return null;

        string imagePath = images[0];
        string baseName = Path.GetFileNameWithoutExtension(imagePath);

        return new DebugPackage
        {
            imagePath = imagePath,
            consolePath = Path.Combine(dir, baseName + "_Console.txt"),
            reportPath = Path.Combine(dir, baseName + "_Report.txt"),
            timestamp = File.GetLastWriteTime(imagePath)
        };
    }

    private static string GenerateSummary(DebugPackage package)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("=== 最新Debug数据包 ===");
        sb.AppendLine($"时间: {package.timestamp:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        sb.AppendLine("📁 文件:");
        sb.AppendLine($"  • 截图: {(File.Exists(package.imagePath) ? "✓" : "✗")} {Path.GetFileName(package.imagePath)}");
        sb.AppendLine($"  • 日志: {(File.Exists(package.consolePath) ? "✓" : "✗")} {Path.GetFileName(package.consolePath)}");
        sb.AppendLine($"  • 报告: {(File.Exists(package.reportPath) ? "✓" : "✗")} {Path.GetFileName(package.reportPath)}");
        sb.AppendLine();

        // Console统计
        if (File.Exists(package.consolePath))
        {
            string log = File.ReadAllText(package.consolePath);
            int errors = CountOccurrences(log, "[Error]");
            int warnings = CountOccurrences(log, "[Warning]");

            sb.AppendLine("🔍 Console统计:");
            sb.AppendLine($"  错误: {errors}");
            sb.AppendLine($"  警告: {warnings}");

            if (errors > 0)
                sb.AppendLine("  ⚠️ 发现错误！");
            sb.AppendLine();
        }

        // 报告摘要
        if (File.Exists(package.reportPath))
        {
            string[] lines = File.ReadAllLines(package.reportPath);
            sb.AppendLine("📊 系统信息:");
            foreach (var line in lines)
            {
                if (line.Contains("GPU:") || line.Contains("Resolution:") || line.Contains("FPS:"))
                    sb.AppendLine($"  {line.Trim()}");
            }
        }

        return sb.ToString();
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int i = 0;
        while ((i = text.IndexOf(pattern, i, System.StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            i += pattern.Length;
        }
        return count;
    }

    public class DebugPackage
    {
        public string imagePath;
        public string consolePath;
        public string reportPath;
        public System.DateTime timestamp;
    }
}
