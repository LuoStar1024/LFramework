#region Class Documentation

/************************************************************************************************************
Class Name:     CommandLineReader.cs
Type:           Util, Static
Definition:
                CommandLineReader.cs give the ability to access [Custom Arguments] sent
                through the command line. Simply add your custom arguments under the
                keyword '-CustomArgs:' and seperate them by ';'.
Example:
                C:\Program Files (x86)\Unity\Editor\Unity.exe [ProjectLocation] -executeMethod [Your entrypoint] -quit -CustomArgs:Language=en_US;Version=1.02

Example1:
                set WORKSPACE=.
                set UNITYEDITOR_PATH=G:/UnityEditor/2021.3.20f1c1/Editor
                set LOGFILE=./build.log
                set BUILDROOT=G:/github/TEngine/UnityProject/Bundles

                %UNITYEDITOR_PATH%/Unity.exe -projectPath %WORKSPACE%/UnityProject -logFile %LOGFILE% -executeMethod GameEditor.ReleaseTools.BuildPackage -quit -batchmode -CustomArgs:platform=Windows;packageVersion=1.02;outputRoot=%BUILDROOT%

                @REM for /f "delims=[" %%i in (%LOGFILE%) do echo %%i

                pause
************************************************************************************************************/

#endregion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Debug = UnityEngine.Debug;

#endregion

namespace GameEditor
{
    /// <summary>
    /// Unity命令行拓展帮助类。
    /// <remarks>可以用来制定自己项目的打包、编辑器工作流。</remarks>
    /// </summary>
    public class CommandLineReader
    {
        //Config
        private const string CUSTOM_ARGS_PREFIX = "-CustomArgs:";
        private const char CUSTOM_ARGS_SEPARATOR = ';';
        private const char CUSTOM_ARG_KEY_VALUE_SEPARATOR = '=';

        public static string[] GetCommandLineArgs()
        {
            return Environment.GetCommandLineArgs();
        }

        public static string GetCommandLine()
        {
            string[] args = GetCommandLineArgs();

            if (args.Length > 0)
            {
                return string.Join(" ", args);
            }
            else
            {
                Debug.LogError("CommandLineReader.cs - GetCommandLine() - Can't find any command line arguments!");
                return "";
            }
        }

        public static Dictionary<string, string> GetCustomArguments()
        {
            Dictionary<string, string> customArgsDict =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] commandLineArgs = GetCommandLineArgs();
            string[] customArgsRows = commandLineArgs
                .Where(row => row.StartsWith(CUSTOM_ARGS_PREFIX, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (customArgsRows.Length == 0)
            {
                Debug.LogError(
                    "CommandLineReader.cs - GetCustomArguments() - Can't retrieve any custom arguments in the command line [" +
                    GetCommandLine() + "].");
                return customArgsDict;
            }

            foreach (string customArgsRow in customArgsRows)
            {
                string customArgsStr = customArgsRow.Substring(CUSTOM_ARGS_PREFIX.Length);
                string[] customArgs = customArgsStr.Split(new[] { CUSTOM_ARGS_SEPARATOR },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (string customArg in customArgs)
                {
                    string[] customArgBuffer = customArg.Split(new[] { CUSTOM_ARG_KEY_VALUE_SEPARATOR }, 2);
                    if (customArgBuffer.Length == 2 && !string.IsNullOrWhiteSpace(customArgBuffer[0]))
                    {
                        string key = customArgBuffer[0].Trim();
                        string value = customArgBuffer[1].Trim();
                        if (customArgsDict.ContainsKey(key))
                        {
                            Debug.LogWarning(
                                "CommandLineReader.cs - GetCustomArguments() - Duplicate custom argument [" +
                                key + "] will be overwritten.");
                        }

                        customArgsDict[key] = value;
                    }
                    else
                    {
                        Debug.LogWarning("CommandLineReader.cs - GetCustomArguments() - The custom argument [" +
                                         customArg +
                                         "] seem to be malformed.");
                    }
                }
            }

            return customArgsDict;
        }

        public static bool TryGetCustomArgument(string argumentName, out string argument)
        {
            return GetCustomArguments().TryGetValue(argumentName, out argument);
        }

        public static string GetCustomArgument(string argumentName, string defaultValue)
        {
            return TryGetCustomArgument(argumentName, out var argument) ? argument : defaultValue;
        }

        /// <summary>
        /// 获取cmd输入的自定义参数数值。
        /// </summary>
        /// <param name="argumentName">自定义参数名称。</param>
        /// <returns>自定义参数数值。</returns>
        public static string GetCustomArgument(string argumentName)
        {
            Dictionary<string, string> customArgsDict = GetCustomArguments();

            if (customArgsDict.TryGetValue(argumentName, out var argument))
            {
                return argument;
            }
            else
            {
                Debug.LogError(
                    "CommandLineReader.cs - GetCustomArgument() - Can't retrieve any custom argument named [" +
                    argumentName + "] in the command line [" + GetCommandLine() + "].");
                return "";
            }
        }
    }
}