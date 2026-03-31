using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public partial class ToolbarExtender
    {
        private static GUIContent _openCsProjectBtContent;

        static void InitOpenCsProject()
        {
            _openCsProjectBtContent = EditorGUIUtility.TrTextContentWithIcon("Open C# Project", "打开C#工程", "dll Script Icon");
        }

        static void ToolbarGUIOpenCsProject()
        {
            if (GUILayout.Button(_openCsProjectBtContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(120)))
            {
                OpenCSharpProject();
            }
        }
        
        static void OpenCSharpProject()
        {
            // Ensure that the mono islands are up-to-date
            AssetDatabase.Refresh();
            CodeEditor.Editor.CurrentCodeEditor.SyncAll();
            CodeEditor.Editor.CurrentCodeEditor.OpenProject();
        }
    }
}