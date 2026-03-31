using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace LFramework.Editor
{
    /// <summary>
    /// 日志重定向相关的实用函数。
    /// </summary>
    internal static class LogRedirection
    {
        private static readonly Regex LogRegex = new Regex(@" \(at (.+)\:(\d+)\)\r?\n");
        private static readonly string[] RedirectionFiles = { "DefaultLogHelper.cs", "LFrameworkLog.cs", "Log.cs" };

        [OnOpenAsset(0)]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            string selectedStackTrace = GetSelectedStackTrace();
            if (string.IsNullOrEmpty(selectedStackTrace))
            {
                return false;
            }

            if (!selectedStackTrace.Contains("LFramework.DefaultLogHelper:Log"))
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(instanceId);
            if (!ShouldRedirect(assetPath))
            {
                return false;
            }

            if (!TryGetRedirectInfo(selectedStackTrace, out string filePath, out int targetLine))
            {
                return false;
            }

            InternalEditorUtility.OpenFileAtLineExternal(filePath, targetLine);
            return true;
        }

        private static bool ShouldRedirect(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return true;
            }

            string fileName = Path.GetFileName(assetPath);
            for (int i = 0; i < RedirectionFiles.Length; i++)
            {
                if (string.Equals(fileName, RedirectionFiles[i], System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetRedirectInfo(string selectedStackTrace, out string filePath, out int line)
        {
            filePath = null;
            line = 0;

            Match match = LogRegex.Match(selectedStackTrace);
            while (match.Success)
            {
                string relativePath = match.Groups[1].Value;
                string fileName = Path.GetFileName(relativePath);
                if (!IsRedirectionFile(fileName))
                {
                    if (!TryGetAbsolutePath(relativePath, out filePath))
                    {
                        return false;
                    }

                    return int.TryParse(match.Groups[2].Value, out line);
                }

                match = match.NextMatch();
            }

            return false;
        }

        private static bool IsRedirectionFile(string fileName)
        {
            for (int i = 0; i < RedirectionFiles.Length; i++)
            {
                if (string.Equals(fileName, RedirectionFiles[i], System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetAbsolutePath(string relativePath, out string filePath)
        {
            filePath = null;
            if (relativePath.StartsWith("Assets/"))
            {
                filePath = Path.Combine(Application.dataPath, relativePath.Substring(7));
                return true;
            }

            if (relativePath.StartsWith("Packages/"))
            {
                filePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), relativePath);
                return true;
            }

            return false;
        }

        private static string GetSelectedStackTrace()
        {
            Assembly editorWindowAssembly = typeof(EditorWindow).Assembly;
            if (editorWindowAssembly == null)
            {
                return null;
            }

            System.Type consoleWindowType = editorWindowAssembly.GetType("UnityEditor.ConsoleWindow");
            if (consoleWindowType == null)
            {
                return null;
            }

            FieldInfo consoleWindowFieldInfo =
 consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            if (consoleWindowFieldInfo == null)
            {
                return null;
            }

            EditorWindow consoleWindow = consoleWindowFieldInfo.GetValue(null) as EditorWindow;
            if (consoleWindow == null)
            {
                return null;
            }

            if (consoleWindow != EditorWindow.focusedWindow)
            {
                return null;
            }

            FieldInfo activeTextFieldInfo =
 consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
            if (activeTextFieldInfo == null)
            {
                return null;
            }

            return (string)activeTextFieldInfo.GetValue(consoleWindow);
        }
    }
}