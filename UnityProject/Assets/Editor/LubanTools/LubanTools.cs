using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public static class LubanTools
    {
        /// <summary>
        /// 打开 Data Path 文件夹。
        /// </summary>
        [MenuItem("LFramework/Luban/ExcelToJson", false, 10)]
        public static void ExcelToJson()
        {
            string path = Application.dataPath + "/../../Configs/GameDataTable/gen_code_json_to_project_lazyload.bat";
            Debug.Log($"执行转表：{path}");
            ShellHelper.RunByPath(path);
        }

        /// <summary>
        /// 打开 Data Path 文件夹。
        /// </summary>
        [MenuItem("LFramework/Luban/ExcelToBin", false, 11)]
        public static void ExcelToBin()
        {
            string path = Application.dataPath + "/../../Configs/GameDataTable/gen_code_bin_to_project_lazyload.bat";
            Debug.Log($"执行转表：{path}");
            ShellHelper.RunByPath(path);
        }
    }
}