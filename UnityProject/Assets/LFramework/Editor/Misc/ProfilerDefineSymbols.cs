using UnityEditor;

namespace LFramework.Editor
{
    /// <summary>
    /// Profiler分析器宏定义操作类。
    /// </summary>
    public class ProfilerDefineSymbols
    {
        private const string EnableFirstProfiler = "FIRST_PROFILER";
        private const string EnableDinProfiler = "DIN_PROFILER";

        private static readonly string[] AllProfilerDefineSymbols = new string[]
        {
            EnableFirstProfiler,
            EnableDinProfiler,
        };

        /// <summary>
        /// 禁用所有日志脚本宏定义。
        /// </summary>
        [MenuItem("LFramework/Profiler Define Symbols/Disable All Profiler", false, 40)]
        public static void DisableAllLogs()
        {
            foreach (string aboveLogScriptingDefineSymbol in AllProfilerDefineSymbols)
            {
                ScriptingDefineSymbols.RemoveScriptingDefineSymbol(aboveLogScriptingDefineSymbol);
            }
        }

        /// <summary>
        /// 开启所有日志脚本宏定义。
        /// </summary>
        [MenuItem("LFramework/Profiler Define Symbols/Enable All Profiler", false, 41)]
        public static void EnableAllLogs()
        {
            DisableAllLogs();
            foreach (string aboveLogScriptingDefineSymbol in AllProfilerDefineSymbols)
            {
                ScriptingDefineSymbols.AddScriptingDefineSymbol(aboveLogScriptingDefineSymbol);
            }
        }
    }
}