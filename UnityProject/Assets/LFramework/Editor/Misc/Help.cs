using UnityEditor;
using UnityEngine;

namespace LFramework.Editor
{
    /// <summary>
    /// 帮助相关的实用函数。
    /// </summary>
    public static class Help
    {
        [MenuItem("LFramework/Documentation", false, 90)]
        public static void ShowDocumentation()
        {
            ShowHelp("https://LFramework.cn/document/");
        }

        [MenuItem("LFramework/API Reference", false, 91)]
        public static void ShowApiReference()
        {
            ShowHelp("https://LFramework.cn/api/");
        }

        private static void ShowHelp(string uri)
        {
            Application.OpenURL(uri);
        }
    }
}